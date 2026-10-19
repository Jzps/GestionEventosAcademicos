# Pruebas de extremo a extremo de la interfaz (Playwright)

Pruebas reales de navegador (Microsoft Edge) sobre la interfaz Blazor del prototipo.
Verifican los flujos de ambos roles contra la API y la base de datos reales.

## Requisitos

- Los contenedores levantados: `docker compose up -d --build`.
- Node.js 18 o superior.
- `npm install` (descarga el paquete `playwright`; **no** descarga navegadores porque la
  suite usa el canal `msedge`, es decir, el Microsoft Edge ya instalado en el equipo).

## Ejecución

```bash
cd front/pruebas-e2e
npm install
node --test prueba-frontend.test.mjs
```

## Qué cubre

| Prueba | Flujo verificado |
| --- | --- |
| 1 | Portada sin sesión: no se exponen datos |
| 2 | Ruta protegida sin sesión: redirige al acceso y no devuelve 5xx |
| 3 | Credenciales incorrectas: mensaje claro y sin sesión creada |
| 4 | Inicio de sesión del estudiante, sin el mensaje de sesión expirada |
| 5 | Listado de eventos con datos reales |
| 6 | Detalle del evento con nombre, fechas, lugar, responsable y cupos |
| 7 | Filtrado por nombre aplicado en la API |
| 8 | Filtro sin coincidencias: estado vacío |
| 9 | Inscripción del estudiante con persistencia comprobada en la API |
| 10 | Inscripción duplicada: se rechaza con mensaje |
| 11 | Mis inscripciones: estado de cada registro |
| 12 | El estudiante no accede a las secciones del responsable |
| 13 | Cierre de sesión: la cookie se elimina y el acceso se bloquea |
| 14 | Inicio de sesión del responsable y sus opciones de menú |
| 15 | El formulario de registro carga las categorías reales de SQL Server |
| 16 | El responsable registra un evento y queda persistido |
| 17 | Modificación de un evento: el cambio persiste |
| 18 | Participantes y registro/consulta de asistencia |
| 19 | Tras inscribirse, el botón y el listado reflejan el estado real |
| 20 | Cancelar saca la inscripción de la vista activa, queda en el historial y permite reinscribirse |
| 21 | El rol Responsable no ve las opciones de inscripción |
| 22 | Registro público de estudiante y acceso con la cuenta creada |
| 23 | El responsable registra una observación de asistencia y queda persistida |
| 24 | Sin errores de consola ni peticiones fallidas inesperadas |

## Datos de prueba

La suite crea eventos cuyo nombre empieza por `E2E Playwright` y cuentas de usuario cuyo
correo empieza por `e2e-playwright-`, y **los elimina al finalizar** (junto con sus
inscripciones y asistencias por el borrado en cascada). No toca los datos de demostración.
