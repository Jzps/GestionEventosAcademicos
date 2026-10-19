// Pruebas de extremo a extremo de la interfaz Blazor del Sistema de Gestión de Eventos
// Académicos. Se ejecutan con el canal Microsoft Edge (no requiere descargar navegadores).
//
//   cd front/pruebas-e2e
//   npm install
//   node --test prueba-frontend.test.mjs
//
// Requisitos: los contenedores deben estar levantados (docker compose up -d --build).

import { test, before, after, describe } from 'node:test';
import assert from 'node:assert/strict';
import { execFileSync } from 'node:child_process';
import { fileURLToPath } from 'node:url';
import { dirname, resolve } from 'node:path';
import { chromium } from 'playwright';

const FRONT = process.env.FRONT_URL ?? 'http://localhost:8080';
const API = process.env.API_URL ?? 'http://localhost:8081';
const RAIZ = process.env.RAIZ_PROYECTO ?? resolve(dirname(fileURLToPath(import.meta.url)), '..', '..');

const CREDENCIALES = {
  estudiante: { correo: 'estudiante@uni.edu.co', contrasena: 'Estudiante2024!' },
  responsable: { correo: 'responsable@uni.edu.co', contrasena: 'Responsable2024!' }
};

const PREFIJO = 'E2E Playwright';
let navegador;
let contexto;
let pagina;
let webSockets = 0;
const erroresConsola = [];
const peticionesFallidas = [];

const marcaTiempo = () => new Date().toISOString().replace(/[:.]/g, '-');
const espera = (ms) => new Promise((r) => setTimeout(r, ms));

/** Elimina únicamente los eventos creados por esta prueba (y sus datos dependientes). */
function limpiarEventosDePrueba() {
  try {
    execFileSync('docker', [
      'compose', 'exec', '-T', 'sqlserver',
      '/opt/mssql-tools18/bin/sqlcmd', '-S', 'localhost', '-U', 'sa',
      '-P', 'GestionEventos2024!', '-C', '-d', 'GestionEventosAcademicos',
      '-b', '-Q',
      `SET QUOTED_IDENTIFIER ON; DELETE FROM dbo.Eventos WHERE Nombre LIKE '${PREFIJO}%';` +
      ` DELETE FROM dbo.Usuarios WHERE Email LIKE 'e2e-playwright-%';`
    ], { cwd: RAIZ, stdio: 'pipe' });
  } catch (error) {
    console.warn('Aviso: no se pudo ejecutar la limpieza de datos de prueba:', error.message);
  }
}

/** Espera a que se establezca el circuito interactivo de Blazor. */
async function esperarCircuito(minimo) {
  const limite = Date.now() + 30000;
  while (Date.now() < limite) {
    if (webSockets >= minimo) {
      await espera(300);
      return;
    }
    await espera(100);
  }
  throw new Error('No se estableció el circuito interactivo de Blazor');
}

/** Navegación completa, esperando a que el circuito de Blazor esté operativo. */
async function navegar(destino) {
  const antes = webSockets;
  await pagina.goto(destino, { waitUntil: 'domcontentloaded' });
  await esperarCircuito(antes + 1);
}

async function iniciarSesion(rol) {
  const { correo, contrasena } = CREDENCIALES[rol];
  await navegar(`${FRONT}/acceder`);
  await pagina.waitForSelector('#correo');
  await pagina.fill('#correo', correo);
  await pagina.fill('#contrasena', contrasena);
  const antes = webSockets;
  await pagina.click('button[type=submit]');
  await pagina.waitForURL(/\/(eventos|responsable\/eventos)$/, { timeout: 25000 });
  await esperarCircuito(antes + 1);
  await pagina.waitForSelector('.usuario-nombre', { timeout: 20000 });
}

async function cerrarSesion() {
  const antes = webSockets;
  await pagina.click('button:has-text("Cerrar sesión")');
  await pagina.waitForURL(/\/acceder/, { timeout: 25000 });
  await esperarCircuito(antes + 1);
}


/** Elige un evento disponible en el que el estudiante todavía no esté inscrito. */
async function eventoDisponibleParaInscripcion(token) {
  const eventos = await (await fetch(`${API}/api/eventos`, { headers: { Authorization: `Bearer ${token}` } })).json();
  const mias = await (await fetch(`${API}/api/inscripciones/mias`, { headers: { Authorization: `Bearer ${token}` } })).json();
  const inscritos = new Set(mias.filter((i) => i.estado === 1).map((i) => i.eventoId));
  return eventos.find((e) => e.admiteInscripciones && e.cuposDisponibles > 0 && !inscritos.has(e.id));
}


/** Crea un evento auxiliar de prueba mediante la API (se limpia al final de la suite). */
async function crearEventoDePrueba(tokenResponsable, cupos = 2) {
  const categorias = await (await fetch(`${API}/api/eventos/categorias`, { headers: { Authorization: `Bearer ${tokenResponsable}` } })).json();
  const inicio = new Date(Date.now() + 15 * 24 * 3600 * 1000);
  const fin = new Date(inicio.getTime() + 2 * 3600 * 1000);
  const respuesta = await fetch(`${API}/api/eventos`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${tokenResponsable}` },
    body: JSON.stringify({
      nombre: `${PREFIJO} ${marcaTiempo()}`,
      descripcion: 'Evento auxiliar creado por la prueba automatizada para validar el flujo de inscripción.',
      fechaHoraInicio: inicio.toISOString().slice(0, 16),
      fechaHoraFin: fin.toISOString().slice(0, 16),
      lugar: 'Auditorio de Pruebas',
      cuposTotales: cupos,
      categoriaId: categorias[0].id
    })
  });
  if (respuesta.status !== 201) throw new Error(`No se pudo crear el evento de prueba: HTTP ${respuesta.status}`);
  return respuesta.json();
}

async function apiLogin(rol) {
  const { correo, contrasena } = CREDENCIALES[rol];
  const respuesta = await fetch(`${API}/api/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email: correo, contrasena })
  });
  const cuerpo = await respuesta.json();
  return cuerpo.token;
}

describe('Interfaz Blazor · Sistema de Gestión de Eventos Académicos', () => {
  before(async () => {
    navegador = await chromium.launch({ channel: 'msedge' });
    contexto = await navegador.newContext({ viewport: { width: 1400, height: 900 } });
    pagina = await contexto.newPage();

    pagina.on('websocket', (ws) => {
      if (ws.url().includes('_blazor')) webSockets++;
    });
    pagina.on('console', (m) => {
      if (m.type() === 'error') erroresConsola.push(m.text());
    });
    pagina.on('pageerror', (e) => erroresConsola.push(`pageerror: ${e.message}`));
    pagina.on('requestfailed', (r) => peticionesFallidas.push(`${r.method()} ${r.url()} :: ${r.failure()?.errorText}`));
    pagina.on('response', (r) => {
      if (r.status() >= 400) peticionesFallidas.push(`HTTP ${r.status()} ${r.request().method()} ${r.url()}`);
    });

    limpiarEventosDePrueba();
  });

  after(async () => {
    await navegador?.close();
    limpiarEventosDePrueba();
  });

  // ---------------------------------------------------------------- 1
  test('1. Sin sesión la aplicación muestra la portada y no expone datos', async () => {
    await navegar(FRONT);

    assert.match(await pagina.locator('h1').first().innerText(), /Gestión de Eventos Académicos/i);
    assert.ok(await pagina.locator('text=Iniciar sesión').count() > 0, 'Debe ofrecerse el inicio de sesión');
    assert.equal(await pagina.locator('.tarjeta-evento').count(), 0, 'No debe listar eventos sin sesión');
  });

  // ---------------------------------------------------------------- 2
  test('2. Una ruta protegida sin sesión redirige al acceso sin error de servidor', async () => {
    const respuesta = await pagina.goto(`${FRONT}/responsable/eventos/nuevo`, { waitUntil: 'domcontentloaded' });

    assert.ok(respuesta.status() < 500, `No debe responder 5xx (fue ${respuesta.status()})`);
    assert.match(pagina.url(), /\/acceder/, 'Debe redirigir a la pantalla de acceso');
  });

  // ---------------------------------------------------------------- 3
  test('3. Credenciales incorrectas muestran un mensaje claro y no crean sesión', async () => {
    await navegar(`${FRONT}/acceder`);
    await pagina.waitForSelector('#correo');
    await pagina.fill('#correo', 'responsable@uni.edu.co');
    await pagina.fill('#contrasena', 'contrasena-inexistente');
    await pagina.click('button[type=submit]');

    await pagina.waitForSelector('.mensaje-error', { timeout: 20000 });
    const mensaje = await pagina.locator('.mensaje-error').innerText();
    assert.match(mensaje, /credenciales|incorrectas|no son correctas/i, `Mensaje inesperado: ${mensaje}`);
    assert.match(pagina.url(), /\/acceder/, 'Debe permanecer en la pantalla de acceso');
    assert.equal(await pagina.locator('.usuario-nombre').count(), 0, 'No debe existir una sesión iniciada');
  });

  // ---------------------------------------------------------------- 4
  test('4. El estudiante inicia sesión y no aparece el mensaje de sesión expirada', async () => {
    await iniciarSesion('estudiante');

    const cuerpo = await pagina.locator('body').innerText();
    assert.ok(!/sesión expiró|sesión expirada/i.test(cuerpo), 'No debe mostrarse el mensaje de sesión expirada');

    const nombre = await pagina.locator('.usuario-nombre').innerText();
    assert.match(nombre, /Ana Maria Gomez/i);
    assert.ok(await pagina.locator('.etiqueta-estudiante').count() > 0, 'Debe mostrarse el rol Estudiante');
  });

  // ---------------------------------------------------------------- 5
  test('5. El listado de eventos muestra tarjetas con los datos reales', async () => {
    await navegar(`${FRONT}/eventos`);
    await pagina.waitForSelector('.tarjeta-evento', { timeout: 25000 });

    const tarjetas = await pagina.locator('.tarjeta-evento').count();
    assert.ok(tarjetas > 0, 'Debe listar eventos disponibles');

    const primera = pagina.locator('.tarjeta-evento').first();
    assert.ok((await primera.locator('.tarjeta-datos').innerText()).length > 0, 'La tarjeta debe mostrar fecha, lugar y responsable');
    assert.ok((await primera.locator('.cupos-valor').innerText()).includes('de'), 'La tarjeta debe mostrar los cupos');
  });

  // ---------------------------------------------------------------- 6
  test('6. El detalle del evento muestra la información completa', async () => {
    await pagina.locator('.tarjeta-evento .tarjeta-titulo a').first().click();
    await pagina.waitForSelector('.detalle-evento', { timeout: 25000 });

    // El CSS aplica text-transform a las etiquetas, por eso se comparan sin distinguir mayúsculas.
    const datos = (await pagina.locator('.detalle-datos').innerText()).toLowerCase();
    for (const campo of ['fecha y hora de inicio', 'fecha y hora de finalización', 'lugar', 'responsable', 'cupos disponibles']) {
      assert.ok(datos.includes(campo), `El detalle debe mostrar «${campo}»`);
    }
    assert.ok((await pagina.locator('h1').innerText()).length > 0, 'Debe mostrar el nombre del evento');
  });

  // ---------------------------------------------------------------- 7
  test('7. El filtrado por nombre se aplica en la API', async () => {
    await navegar(`${FRONT}/eventos`);
    await pagina.waitForSelector('.tarjeta-evento');

    const total = await pagina.locator('.tarjeta-evento').count();
    await pagina.fill('#filtro-nombre', 'Seminario de Arquitectura');
    await pagina.click('button:has-text("Aplicar filtros")');
    await pagina.waitForTimeout(1500);

    const filtrados = await pagina.locator('.tarjeta-evento').count();
    assert.ok(filtrados < total, `El filtrado debe reducir los resultados (${total} → ${filtrados})`);
    assert.ok(filtrados >= 1, 'Debe quedar al menos un resultado');

    await pagina.click('button:has-text("Limpiar")');
    await pagina.waitForTimeout(1500);
    assert.equal(await pagina.locator('.tarjeta-evento').count(), total, 'Limpiar debe restaurar el listado');
  });

  // ---------------------------------------------------------------- 8
  test('8. Un filtro sin coincidencias informa que no se encontraron eventos', async () => {
    await navegar(`${FRONT}/eventos`);
    await pagina.waitForSelector('.tarjeta-evento');

    await pagina.fill('#filtro-nombre', 'evento-inexistente-xyz');
    await pagina.click('button:has-text("Aplicar filtros")');
    await pagina.waitForSelector('.estado-vacio', { timeout: 20000 });

    assert.match(await pagina.locator('.estado-vacio').innerText(), /No se encontraron eventos/i);
  });

  // ---------------------------------------------------------------- 9
  test('9. El estudiante se inscribe y la inscripción persiste', async () => {
    const token = await apiLogin('estudiante');
    const tokenResponsable = await apiLogin('responsable');

    // Evento propio de la prueba: garantiza cupos libres y ausencia de inscripción previa.
    const evento = await crearEventoDePrueba(tokenResponsable, 2);

    await navegar(`${FRONT}/eventos/${evento.id}`);
    await pagina.waitForSelector('.panel-acciones');

    await pagina.click('button:has-text("Inscribirme en el evento")');
    const exito = pagina.locator('.mensaje-exito', { hasText: 'inscripción' });
    await exito.first().waitFor({ timeout: 25000 });
    assert.match(await exito.first().innerText(), /inscripción|inscrito/i);

    // Persistencia comprobada directamente contra la API.
    const misInscripciones = await fetch(`${API}/api/inscripciones/mias`, { headers: { Authorization: `Bearer ${token}` } });
    const lista = await misInscripciones.json();
    const inscripcion = lista.find((i) => i.eventoId === evento.id);
    assert.ok(inscripcion, 'La inscripción debe persistir en SQL Server');
    assert.equal(inscripcion.estado, 1, 'La inscripción debe quedar activa');

    // Los cupos del evento deben reflejar la inscripción.
    const detalle = await fetch(`${API}/api/eventos/${evento.id}`, { headers: { Authorization: `Bearer ${token}` } });
    const detalleJson = await detalle.json();
    assert.equal(detalleJson.cuposDisponibles, 1, 'Los cupos deben descontarse tras la inscripción');
  });

  // ---------------------------------------------------------------- 10
  test('10. Una inscripción duplicada se rechaza con un mensaje explicativo', async () => {
    const token = await apiLogin('estudiante');
    const tokenResponsable = await apiLogin('responsable');
    const evento = await crearEventoDePrueba(tokenResponsable, 3);
    assert.ok(evento, 'Debe existir un evento disponible para la prueba');

    await navegar(`${FRONT}/eventos/${evento.id}`);
    await pagina.waitForSelector('.panel-acciones');

    if (await pagina.locator('text=Ya se encuentra inscrito').count() > 0) {
      // Ya está inscrito (caso del test anterior): se valida la lista de inscripciones.
      await navegar(`${FRONT}/mis-inscripciones`);
      await pagina.waitForSelector('.tabla tbody tr', { timeout: 25000 });
      assert.ok((await pagina.locator('.tabla').innerText()).includes('Activa'),
        'La inscripción debe mostrarse como activa con su estado');
      return;
    }

    await pagina.click('button:has-text("Inscribirme en el evento")');
    await pagina.locator('.mensaje-exito', { hasText: 'inscripción' }).first().waitFor({ timeout: 25000 });

    // Segundo intento: debe rechazarse.
    await pagina.reload({ waitUntil: 'domcontentloaded' });
    await pagina.waitForSelector('.panel-acciones', { timeout: 25000 });
    if (await pagina.locator('button:has-text("Inscribirme en el evento")').count() > 0) {
      await pagina.click('button:has-text("Inscribirme en el evento")');
      await pagina.waitForSelector('.mensaje-error', { timeout: 25000 });
      assert.match(await pagina.locator('.mensaje-error').innerText(), /inscrito/i);
    }
  });

  // ---------------------------------------------------------------- 11
  test('11. Mis inscripciones muestra el estado de cada inscripción', async () => {
    await navegar(`${FRONT}/mis-inscripciones`);
    await pagina.waitForSelector('.tabla tbody tr', { timeout: 25000 });

    const texto = await pagina.locator('.tabla').first().innerText();
    assert.match(texto, /Activa/, 'Debe mostrarse el estado de cada inscripción activa');
  });

  // ---------------------------------------------------------------- 12
  test('12. El estudiante no accede a las secciones del responsable', async () => {
    const respuesta = await pagina.goto(`${FRONT}/responsable/eventos`, { waitUntil: 'domcontentloaded' });
    assert.ok(respuesta.status() < 500, `No debe responder 5xx (fue ${respuesta.status()})`);
    assert.match(pagina.url(), /\/acceder/, 'Debe redirigir al acceso');

    // La navegación interna tampoco debe mostrar las opciones del responsable.
    await navegar(`${FRONT}/eventos`);
    await pagina.waitForSelector('.navegacion-lista');
    assert.equal(await pagina.locator('.navegacion-lista a:has-text("Registrar evento")').count(), 0,
      'El menú del estudiante no debe mostrar "Registrar evento"');
  });

  // ---------------------------------------------------------------- 13
  test('13. Cerrar sesión elimina la cookie y bloquea el acceso', async () => {
    await iniciarSesion('estudiante');
    await cerrarSesion();

    assert.match(pagina.url(), /\/acceder/);
    assert.equal(await pagina.locator('.usuario-nombre').count(), 0, 'Debe cerrarse la sesión');

    const respuesta = await pagina.goto(`${FRONT}/eventos`, { waitUntil: 'domcontentloaded' });
    assert.match(pagina.url(), /\/acceder/, 'Tras cerrar sesión no debe accederse a las rutas protegidas');
    assert.ok(respuesta.status() < 500);
  });

  // ---------------------------------------------------------------- 14
  test('14. El responsable inicia sesión y ve sus opciones', async () => {
    await iniciarSesion('responsable');

    assert.match(await pagina.locator('.usuario-nombre').innerText(), /Carlos Alberto Rios/i);
    assert.ok(await pagina.locator('.navegacion-lista a:has-text("Registrar evento")').count() > 0);
    assert.ok(await pagina.locator('.navegacion-lista a:has-text("Mis eventos")').count() > 0);
  });

  // ---------------------------------------------------------------- 15
  test('15. El formulario de registro carga las categorías reales de SQL Server', async () => {
    await navegar(`${FRONT}/responsable/eventos/nuevo`);
    await pagina.waitForSelector('#categoria', { state: 'attached', timeout: 25000 });
    await pagina.waitForFunction(() => document.querySelectorAll('#categoria option').length > 1, null, { timeout: 25000 });

    const opciones = await pagina.locator('#categoria option').allTextContents();
    assert.ok(opciones.length > 1, 'Debe listar las categorías disponibles');

    // Se comparan con las que devuelve la API (misma fuente de datos).
    const token = await apiLogin('responsable');
    const categorias = await (await fetch(`${API}/api/eventos/categorias`, { headers: { Authorization: `Bearer ${token}` } })).json();
    assert.equal(opciones.length - 1, categorias.length, 'El formulario debe mostrar todas las categorías de la API');
  });

  // ---------------------------------------------------------------- 16
  test('16. El responsable registra un evento y queda persistido', async () => {
    const nombre = `${PREFIJO} evento ${marcaTiempo()}`;
    const inicio = new Date(Date.now() + 12 * 24 * 3600 * 1000);
    const fin = new Date(inicio.getTime() + 2 * 3600 * 1000);
    const formato = (d) => `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}T${String(d.getHours()).padStart(2, '0')}:${String(d.getMinutes()).padStart(2, '0')}`;

    await navegar(`${FRONT}/responsable/eventos/nuevo`);
    await pagina.waitForSelector('#categoria', { state: 'attached' });
    await pagina.waitForFunction(() => document.querySelectorAll('#categoria option').length > 1, null, { timeout: 25000 });

    await pagina.fill('#nombre', nombre);
    await pagina.fill('#descripcion', 'Evento creado por la prueba automatizada de la interfaz para validar el flujo completo.');
    await pagina.fill('#inicio', formato(inicio));
    await pagina.fill('#fin', formato(fin));
    await pagina.fill('#lugar', 'Auditorio de Pruebas');
    await pagina.fill('#cupos', '3');

    const opciones = await pagina.locator('#categoria option').allTextContents();
    const indice = opciones.findIndex((o) => o.trim().length > 0 && o !== 'Seleccione una categoría');
    await pagina.selectOption('#categoria', { index: indice });

    await pagina.click('button:has-text("Registrar evento")');
    await pagina.waitForURL(/\/responsable\/eventos$/, { timeout: 30000 });
    await pagina.waitForSelector('.tabla tbody tr', { timeout: 25000 });

    // El evento debe verse en la sección de eventos gestionados.
    assert.ok((await pagina.locator('.tabla').innerText()).includes(nombre), 'El evento debe aparecer en "Mis eventos"');

    // Y debe existir en SQL Server (verificado a través de la API).
    const token = await apiLogin('responsable');
    const gestionados = await (await fetch(`${API}/api/eventos/gestionados`, { headers: { Authorization: `Bearer ${token}` } })).json();
    const evento = gestionados.find((e) => e.nombre === nombre);
    assert.ok(evento, 'El evento debe persistir en la base de datos');
    assert.equal(evento.cuposTotales, 3);
  });

  // ---------------------------------------------------------------- 17
  test('17. El responsable modifica un evento y el cambio persiste', async () => {
    const token = await apiLogin('responsable');
    const gestionados = await (await fetch(`${API}/api/eventos/gestionados`, { headers: { Authorization: `Bearer ${token}` } })).json();
    const evento = gestionados.find((e) => e.nombre.startsWith(PREFIJO));
    assert.ok(evento, 'Debe existir el evento de prueba');

    await navegar(`${FRONT}/responsable/eventos/${evento.id}/editar`);
    await pagina.waitForSelector('#nombre');
    await pagina.waitForFunction(() => document.querySelectorAll('#categoria option').length > 1, null, { timeout: 25000 });

    await pagina.fill('#lugar', 'Auditorio Modificado');
    await pagina.click('button:has-text("Guardar cambios")');
    await pagina.waitForURL(/\/responsable\/eventos$/, { timeout: 30000 });

    const detalle = await (await fetch(`${API}/api/eventos/${evento.id}`, { headers: { Authorization: `Bearer ${token}` } })).json();
    assert.equal(detalle.lugar, 'Auditorio Modificado', 'La modificación debe persistir');
  });

  // ---------------------------------------------------------------- 18
  test('18. El responsable consulta participantes y registra asistencia', async () => {
    const token = await apiLogin('responsable');
    const gestionados = await (await fetch(`${API}/api/eventos/gestionados`, { headers: { Authorization: `Bearer ${token}` } })).json();
    const evento = gestionados.find((e) => e.nombre.startsWith(PREFIJO));
    assert.ok(evento, 'Debe existir el evento de prueba');

    await navegar(`${FRONT}/responsable/eventos/${evento.id}/participantes`);
    await pagina.waitForSelector('.tabla, .estado-vacio', { timeout: 25000 });
    assert.ok(await pagina.locator('.estado-vacio').count() > 0, 'El evento recién creado no tiene participantes todavía');

    // Se inscribe un estudiante para poder registrar asistencia.
    const tokenEstudiante = await apiLogin('estudiante');
    const inscripcion = await fetch(`${API}/api/inscripciones`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${tokenEstudiante}` },
      body: JSON.stringify({ eventoId: evento.id })
    });
    assert.equal(inscripcion.status, 201, 'El estudiante debe poder inscribirse');

    await pagina.reload({ waitUntil: 'domcontentloaded' });
    await pagina.waitForSelector('.tabla tbody tr', { timeout: 25000 });
    assert.match(await pagina.locator('.tabla tbody').innerText(), /Ana Maria Gomez/i, 'Debe listar al participante');

    await pagina.click('button:has-text("Registrar")');
    await pagina.waitForSelector('.mensaje-exito', { timeout: 25000 });
    assert.match(await pagina.locator('.mensaje-exito').innerText(), /asistencia/i);

    await navegar(`${FRONT}/responsable/eventos/${evento.id}/asistencia`);
    await pagina.waitForSelector('.tabla tbody tr', { timeout: 25000 });
    assert.match(await pagina.locator('.tabla tbody').innerText(), /Asistió/i, 'Debe mostrar el registro de asistencia');
  });

  // ---------------------------------------------------------------- 19
  test('19. Tras inscribirse el boton y el listado reflejan el estado real', async () => {
    await iniciarSesion('estudiante');

    const token = await apiLogin('estudiante');
    const tokenResponsable = await apiLogin('responsable');
    const objetivo = await crearEventoDePrueba(tokenResponsable, 2);
    assert.ok(objetivo, 'Debe existir un evento disponible para la prueba');

    // Listado: antes de inscribirse, la tarjeta ofrece la inscripcion.
    await navegar(`${FRONT}/eventos`);
    await pagina.waitForSelector('.tarjeta-evento', { timeout: 25000 });
    const tarjeta = pagina.locator('.tarjeta-evento', { hasText: objetivo.nombre }).first();
    assert.ok((await tarjeta.locator('button:has-text("Inscribirme")').count()) > 0,
      'Antes de inscribirse la tarjeta debe ofrecer el boton de inscripcion');

    // Detalle: se inscribe y el boton cambia sin recargar la aplicacion.
    await pagina.goto(`${FRONT}/eventos/${objetivo.id}`, { waitUntil: 'domcontentloaded' });
    await pagina.waitForSelector('.panel-acciones');
    await pagina.click('.panel-acciones button:has-text("Inscribirme en el evento")');
    await pagina.locator('.mensaje-exito', { hasText: 'inscripción' }).first().waitFor({ timeout: 25000 });
    await pagina.waitForTimeout(700);

    const panel = await pagina.locator('.panel-acciones').innerText();
    assert.match(panel, /Ya se encuentra inscrito/i, 'El detalle debe indicar que ya esta inscrito');
    assert.equal(await pagina.locator('.panel-acciones button:has-text("Inscribirme")').count(), 0,
      'El boton de inscripcion debe desaparecer tras inscribirse');

    // Listado: la tarjeta ya no ofrece la inscripcion y muestra el estado.
    await navegar(`${FRONT}/eventos`);
    await pagina.waitForSelector('.tarjeta-evento', { timeout: 25000 });
    const tarjeta2 = pagina.locator('.tarjeta-evento', { hasText: objetivo.nombre }).first();
    assert.equal(await tarjeta2.locator('button:has-text("Inscribirme")').count(), 0,
      'El listado no debe ofrecer la inscripcion de un evento en el que ya esta inscrito');
    assert.match(await tarjeta2.innerText(), /Ya estás inscrito/i,
      'El listado debe indicar que el estudiante ya esta inscrito');
  });

  // ---------------------------------------------------------------- 20
  test('20. Cancelar quita la inscripcion de la vista activa y permite reinscribirse', async () => {
    await iniciarSesion('estudiante');
    await navegar(`${FRONT}/mis-inscripciones`);
    await pagina.waitForSelector('.tabla tbody tr', { timeout: 25000 });

    // Se toma la primera inscripcion activa y se cancela.
    const filaActiva = pagina.locator('.tabla tbody tr', { hasText: 'Activa' }).first();
    assert.ok(await filaActiva.count() > 0, 'Debe existir al menos una inscripcion activa');
    const nombreEvento = (await filaActiva.locator('td').first().innerText()).split('\n')[0].trim();

    await filaActiva.locator('button:has-text("Cancelar inscripción")').first().click();
    await pagina.locator('.mensaje-exito', { hasText: 'cancelada' }).first().waitFor({ timeout: 25000 });
    await pagina.waitForTimeout(800);

    // La vista principal ya no debe incluir la inscripcion cancelada.
    const textoTabla = await pagina.locator('.tabla').first().innerText();
    assert.ok(!textoTabla.includes(nombreEvento),
      'La inscripcion cancelada debe salir de la vista de activas');

    // Pero debe quedar en el historial de cancelaciones.
    const historial = pagina.locator('.historial');
    assert.ok(await historial.count() > 0, 'Debe existir el historial de cancelaciones');
    assert.match(await historial.innerText(), /Cancelada/i);

    // El detalle vuelve a ofrecer la inscripcion y el estudiante puede reinscribirse.
    const idEvento = await (async () => {
      const token = await apiLogin('estudiante');
      const mias = await (await fetch(`${API}/api/inscripciones/mias`, { headers: { Authorization: `Bearer ${token}` } })).json();
      const cancelada = mias.find((i) => i.estado === 2 && i.nombreEvento === nombreEvento);
      return cancelada ? cancelada.eventoId : null;
    })();
    assert.ok(idEvento, 'Debe encontrarse la inscripcion cancelada en la API');

    await navegar(`${FRONT}/eventos/${idEvento}`);
    await pagina.waitForSelector('.panel-acciones');
    const panel = await pagina.locator('.panel-acciones').innerText();
    assert.ok(!/Ya se encuentra inscrito/i.test(panel),
      'Una inscripcion cancelada no debe mostrarse como inscripcion activa');

    if (await pagina.locator('.panel-acciones button:has-text("Inscribirme en el evento")').count() > 0) {
      await pagina.click('.panel-acciones button:has-text("Inscribirme en el evento")');
      await pagina.locator('.mensaje-exito', { hasText: 'inscripción' }).first().waitFor({ timeout: 25000 });
      assert.match(await pagina.locator('.panel-acciones').innerText(), /Ya se encuentra inscrito/i,
        'Tras reinscribirse el detalle debe indicar la inscripcion activa');
    }
  });

  // ---------------------------------------------------------------- 21
  test('21. El rol Responsable no ve las opciones de inscripcion', async () => {
    await iniciarSesion('responsable');

    await navegar(`${FRONT}/eventos`);
    await pagina.waitForSelector('.tarjeta-evento', { timeout: 25000 });

    assert.equal(await pagina.locator('.tarjeta-evento button:has-text("Inscribirme")').count(), 0,
      'El responsable no debe ver botones de inscripcion en el listado');
    assert.equal(await pagina.locator('.tarjeta-evento .estado-inscripcion').count(), 0,
      'El responsable no debe ver el estado de inscripcion de estudiante');

    await pagina.locator('.tarjeta-evento .tarjeta-titulo a').first().click();
    await pagina.waitForSelector('.panel-acciones', { timeout: 25000 });
    assert.equal(await pagina.locator('.panel-acciones button:has-text("Inscribirme")').count(), 0,
      'El responsable no debe ver el boton de inscripcion en el detalle');
  });

  // ---------------------------------------------------------------- 22
  test('22. Registro publico de estudiante y acceso con la cuenta creada', async () => {
    const correo = `e2e-playwright-${Date.now()}@uni.edu.co`;

    await navegar(`${FRONT}/acceder`);
    await pagina.waitForSelector('a:has-text("Crear cuenta de estudiante")');

    await pagina.click('a:has-text("Crear cuenta de estudiante")');
    await pagina.waitForSelector('#nombre', { timeout: 20000 });

    // Validacion de campos obligatorios.
    await pagina.click('button:has-text("Crear cuenta")');
    await pagina.waitForSelector('.validation-message', { timeout: 15000 });
    assert.ok((await pagina.locator('.validation-message').allTextContents()).length > 0,
      'El formulario debe validar los campos obligatorios');

    // Registro correcto.
    await pagina.fill('#nombre', 'Estudiante Registrado por la Prueba');
    await pagina.fill('#correo', correo);
    await pagina.fill('#codigo', 'EST-E2E-1');
    await pagina.fill('#programa', 'Ingenieria de Sistemas');
    await pagina.fill('#contrasena', 'Estudiante2024!');
    await pagina.fill('#confirmar', 'Estudiante2024!');
    await pagina.click('button:has-text("Crear cuenta")');

    await pagina.waitForURL(/\/eventos$/, { timeout: 25000 });
    await pagina.waitForSelector('.usuario-nombre', { timeout: 20000 });
    assert.match(await pagina.locator('.usuario-nombre').innerText(), /Estudiante Registrado/i);

    // La cuenta nueva es de estudiante: no ve las funciones del responsable.
    assert.equal(await pagina.locator('.navegacion-lista a:has-text("Registrar evento")').count(), 0,
      'La cuenta registrada no debe acceder a las funciones del responsable');

    const respuesta = await pagina.goto(`${FRONT}/responsable/eventos`, { waitUntil: 'domcontentloaded' });
    assert.match(pagina.url(), /\/acceder/, 'La cuenta de estudiante no debe entrar al panel del responsable');
    assert.ok(respuesta.status() < 500);
  });

  // ---------------------------------------------------------------- 23
  test('23. El responsable registra una observacion de asistencia y queda persistida', async () => {
    // La prueba anterior deja la sesión del estudiante registrado: se cambia al responsable.
    await iniciarSesion('responsable');

    const tokenResponsable = await apiLogin('responsable');
    const gestionados = await (await fetch(`${API}/api/eventos/gestionados`, { headers: { Authorization: `Bearer ${tokenResponsable}` } })).json();
    const evento = gestionados.find((e) => e.nombre.startsWith(PREFIJO));
    assert.ok(evento, 'Debe existir el evento de prueba');

    // Se garantiza un participante inscrito en el evento.
    const tokenEstudiante = await apiLogin('estudiante');
    const participantes = await (await fetch(`${API}/api/participantes/${evento.id}`, { headers: { Authorization: `Bearer ${tokenResponsable}` } })).json();
    if (!participantes.length) {
      const inscripcion = await fetch(`${API}/api/inscripciones`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${tokenEstudiante}` },
        body: JSON.stringify({ eventoId: evento.id })
      });
      assert.equal(inscripcion.status, 201, 'El estudiante debe poder inscribirse');
      // La página ya estaba cargada: se recarga para ver al nuevo participante.
      await pagina.reload({ waitUntil: 'domcontentloaded' });
    }

    await navegar(`${FRONT}/responsable/eventos/${evento.id}/participantes`);
    await pagina.waitForSelector('.tabla tbody tr', { timeout: 25000 });

    const campo = pagina.locator('.tabla-participantes input[type=text]').first();
    assert.ok(await campo.count() > 0, 'Debe existir un campo editable de observacion');
    await campo.fill('Observacion de la prueba de interfaz');

    await pagina.locator('.tabla-participantes button', { hasText: /Registrar|Actualizar/ }).first().click();
    await pagina.locator('.mensaje-exito', { hasText: 'asistencia' }).first().waitFor({ timeout: 25000 });

    // La observacion debe quedar persistida y visible tras recargar.
    await pagina.reload({ waitUntil: 'domcontentloaded' });
    await pagina.waitForSelector('.tabla-participantes input[type=text]', { timeout: 25000 });
    const valor = await pagina.locator('.tabla-participantes input[type=text]').first().inputValue();
    assert.equal(valor, 'Observacion de la prueba de interfaz',
      'La observacion debe persistir y recuperarse al recargar');

    // Y debe verse en el registro de asistencia del evento.
    await navegar(`${FRONT}/responsable/eventos/${evento.id}/asistencia`);
    await pagina.waitForSelector('.tabla tbody tr', { timeout: 25000 });
    assert.match(await pagina.locator('.tabla tbody').innerText(), /Observacion de la prueba de interfaz/,
      'La observacion debe mostrarse en el registro de asistencia');
  });

  // ---------------------------------------------------------------- 24
  test('24. El responsable retira un evento con confirmacion y el listado se actualiza', async () => {
    const tokenResponsable = await apiLogin('responsable');
    const evento = await crearEventoDePrueba(tokenResponsable, 3);
    assert.ok(evento, 'Debe existir el evento de prueba');

    await iniciarSesion('responsable');
    await navegar(`${FRONT}/responsable/eventos`);
    await pagina.waitForSelector('.tabla tbody tr', { timeout: 25000 });

    // El evento aparece en la vista operativa del responsable.
    const fila = pagina.locator('.tabla tbody tr', { hasText: evento.nombre }).first();
    assert.ok(await fila.count() > 0, 'El evento debe aparecer en "Mis eventos"');

    // El botón de retirar solo existe en la vista del responsable.
    const boton = fila.locator('button:has-text("Eliminar evento")');
    assert.ok(await boton.count() > 0, 'Debe existir la acción "Eliminar evento"');

    // Al pulsar se muestra la confirmación con su advertencia.
    await boton.first().click();
    await pagina.waitForSelector('.confirmacion', { timeout: 15000 });
    const confirmacion = await pagina.locator('.confirmacion').innerText();
    assert.match(confirmacion, /inscripciones activas/i, 'La confirmación debe advertir de las inscripciones');
    assert.match(confirmacion, /no estará disponible|dejará de estar disponible/i,
      'La confirmación debe advertir de la retirada del evento');

    // Cancelar la confirmación no retira nada.
    await pagina.click('.confirmacion button:has-text("Cancelar")');
    await pagina.waitForTimeout(500);
    assert.ok((await pagina.locator('.tabla tbody tr', { hasText: evento.nombre }).count()) > 0,
      'Cancelar la confirmación debe dejar el evento en su sitio');

    // Confirmar la retirada.
    await pagina.locator('tr', { hasText: evento.nombre }).first().locator('button:has-text("Eliminar evento")').click();
    await pagina.waitForSelector('.confirmacion', { timeout: 15000 });
    await pagina.click('.confirmacion button:has-text("retirar el evento")');

    await pagina.locator('.mensaje-exito', { hasText: 'retirado' }).first().waitFor({ timeout: 25000 });
    await pagina.waitForTimeout(800);

    // El evento sale de la vista operativa y pasa al historial de retirados.
    const tablaOperativa = await pagina.locator('.tabla').first().innerText();
    assert.ok(!tablaOperativa.includes(evento.nombre),
      'El evento retirado debe salir de la vista operativa');

    const historial = pagina.locator('.historial');
    assert.ok(await historial.count() > 0, 'Debe existir el historial de eventos retirados');

    // El historial es un desplegable: se abre para comprobar su contenido.
    await historial.locator('summary').click();
    await pagina.waitForTimeout(400);
    const textoHistorial = await historial.innerText();
    assert.match(textoHistorial, /Cancelado/i, 'El historial debe mostrar el estado Cancelado');
    assert.ok(textoHistorial.includes(evento.nombre), 'El historial debe identificar el evento retirado');

    // La API confirma la persistencia: el evento queda en estado Cancelado.
    const detalle = await (await fetch(`${API}/api/eventos/${evento.id}`, { headers: { Authorization: `Bearer ${tokenResponsable}` } })).json();
    assert.equal(detalle.estado, 5, 'El evento debe quedar Cancelado en la base de datos');
    assert.equal(detalle.admiteInscripciones, false);

    // El estudiante no lo ve en el listado público.
    await iniciarSesion('estudiante');
    await navegar(`${FRONT}/eventos`);
    await pagina.waitForSelector('.tarjeta-evento, .estado-vacio', { timeout: 25000 });
    const listado = await pagina.locator('.rejilla-tarjetas').innerText().catch(() => '');
    assert.ok(!listado.includes(evento.nombre), 'El evento retirado no debe aparecer en el listado de estudiantes');

    // Y su detalle indica que ya no está disponible.
    await navegar(`${FRONT}/eventos/${evento.id}`);
    await pagina.waitForSelector('.detalle-evento, .estado-vacio', { timeout: 25000 });
    const detalleUI = await pagina.locator('.seccion').innerText();
    assert.match(detalleUI, /retirado|no está disponible/i,
      'El detalle debe indicar que el evento ya no está disponible');
    assert.equal(await pagina.locator('.panel-acciones button:has-text("Inscribirme")').count(), 0,
      'No debe poder inscribirse en un evento retirado');
  });

  // ---------------------------------------------------------------- 25
  test('25. El estudiante no ve la acción de eliminar eventos', async () => {
    await iniciarSesion('estudiante');
    await navegar(`${FRONT}/eventos`);
    await pagina.waitForSelector('.tarjeta-evento, .estado-vacio', { timeout: 25000 });

    assert.equal(await pagina.locator('button:has-text("Eliminar evento")').count(), 0,
      'El listado público no debe ofrecer la acción de eliminar eventos');

    await pagina.goto(`${FRONT}/responsable/eventos`, { waitUntil: 'domcontentloaded' });
    assert.match(pagina.url(), /\/acceder/, 'El estudiante no debe entrar a la gestión de eventos');
  });

  // ---------------------------------------------------------------- 26
  test('26. La interfaz no genera errores de consola ni peticiones fallidas inesperadas', async () => {
    // Se ignoran los 401/403 esperados de las pruebas de seguridad (rutas protegidas sin sesión).
    const relevantes = peticionesFallidas.filter((p) => !/HTTP 40[13]/.test(p));
    assert.deepEqual(erroresConsola, [], `Errores de consola: ${erroresConsola.join(' | ')}`);
    assert.deepEqual(relevantes, [], `Peticiones fallidas: ${relevantes.join(' | ')}`);
  });
});
