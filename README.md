# Sistema de Gestión de Eventos Académicos

Prototipo académico funcional del **Sistema de Gestión de Eventos Académicos**: una
aplicación web con dos roles (Estudiante y Responsable) que centraliza la información de
los eventos de la institución, permite consultarlos e inscribirse, y da soporte al control
de participantes y asistencia.

El proyecto es **ejecutable de extremo a extremo**: frontend Blazor, API ASP.NET Core,
persistencia real en SQL Server y arranque con un único comando de Docker Compose.

---

## 1. Qué resuelve el sistema

Los eventos académicos de la institución están dispersos en distintos medios y procesos.
Esto dificulta que el estudiante conozca la oferta, se inscriba y verifique su
participación, y que el responsable controle los inscritos y la asistencia.

El sistema centraliza esa información:

- El **responsable** registra y modifica eventos, consulta los que administra y controla
  participantes y asistencia.
- El **estudiante** consulta el listado y el detalle de los eventos, aplica filtros, se
  inscribe, cancela su inscripción y revisa su estado.

Los datos que muestra la interfaz provienen siempre de **SQL Server a través de la API**:
no hay datos estáticos ni simulados en la pantalla.

## 2. Roles disponibles

| Rol | Puede hacer |
| --- | --- |
| **Estudiante** | Listar y filtrar eventos, consultar el detalle, inscribirse, cancelar inscripciones y consultar sus inscripciones (activas e historial de cancelaciones). |
| **Responsable** | Registrar, modificar, consultar y **retirar** (eliminar) los eventos que administra, consultar participantes y registrar y consultar la asistencia con observaciones. |

Las operaciones de inscripción son **exclusivas del rol Estudiante**: el responsable no ve los
botones de inscripción en la interfaz y la API rechaza sus intentos con **HTTP 403**. Las
cuentas de responsable se habilitan por el inicializador de demostración; el registro
público de la pantalla «Crear cuenta» es solo para estudiantes.

## 3. Funcionalidades del MVP (recorrido principal)

1. El responsable inicia sesión y **registra un evento** futuro con cupos válidos.
2. El estudiante inicia sesión y **consulta el listado** real de eventos.
3. El estudiante abre el **detalle** del evento recién registrado.
4. El estudiante se **inscribe** y recibe la confirmación.

## 4. Funcionalidades complementarias implementadas

Filtrado por nombre, fecha y categoría · consulta de inscripciones (activas e historial de
cancelaciones) · cancelación de inscripción · volver a inscribirse tras cancelar · consulta
de eventos gestionados · modificación de eventos · **retirada (eliminación) de un evento
propio, con confirmación y cancelación de sus inscripciones activas** · consulta de
participantes · registro de asistencia con observación editable · consulta y modificación de
la asistencia · **registro público de cuentas de estudiante** (pantalla «Crear cuenta», en
el enlace de la pantalla de acceso).

### Retirada de un evento

La acción «Eliminar evento» (solo en *Mis eventos*) retira el evento de la actividad
operativa: pasa al estado **Cancelado**, deja de aparecer en el listado de los estudiantes
y **cancela sus inscripciones activas**. La información histórica de inscripciones y
asistencias se conserva; lo que deja de estar vigente es la inscripción. Un evento
retirado es terminal: no admite modificaciones ni puede reabrirse, ni siquiera cambiando
sus fechas. La operación es transaccional (o se retira todo, o no cambia nada) y solo la
puede ejecutar el responsable que administra el evento (`DELETE /api/eventos/{id}`).

Todas están disponibles en el menú según el rol, en apartados secundarios que no
interrumpen el recorrido principal.

## 5. Tecnologías utilizadas

| Capa | Tecnología |
| --- | --- |
| Frontend | Blazor Web App (interactividad de servidor), .NET 10, CSS propio sin recursos externos |
| API | ASP.NET Core Web API, .NET 10, controllers REST, JWT, Swagger / OpenAPI |
| Dominio | C# con principios SOLID y los patrones GoF **Builder**, **Facade** y **State** |
| Persistencia | SQL Server 2022 con Entity Framework Core 10 y migraciones |
| Orquestación | Docker Compose (3 contenedores en una red privada) |
| Pruebas | xUnit (55 unitarias), Playwright sobre Edge (26 de interfaz) y scripts de verificación de API (88 + concurrencia) |

## 6. Estructura de carpetas

```text
GestionEventosAcademicos/
├── api-gestion-academico/          # API ASP.NET Core
│   ├── controllers/                 # Endpoints HTTP (eventos, inscripciones, participantes, asistencia)
│   ├── excepciones/                # Excepciones de dominio y manejador centralizado de errores
│   ├── modelos/                    # Entidades, enumeraciones, EventoBuilder y estados (patrón State)
│   │   └── estados/                # IEventoEstado y sus seis implementaciones
│   ├── peticiones/                 # DTO de entrada y salida de la API
│   ├── pruebas/                    # Pruebas unitarias xUnit + verificación de extremo a extremo
│   ├── repositorios/               # Contratos e implementaciones de acceso a datos (EF Core)
│   │   └── Migraciones/            # Migraciones que applied el esquema en SQL Server
│   ├── servicios/                  # Casos de uso: ServicioEventos, InscripcionFacade, autenticación…
│   ├── Program.cs
│   ├── appsettings.json
│   ├── Dockerfile
│   └── ApiGestionAcademico.csproj
├── db/
│   ├── ModeloRelacional.sql        # Script SQL del esquema (generado desde las migraciones)
│   └── README.md
├── front/                          # Frontend Blazor
│   ├── components/
│   │   ├── layout/                 # MainLayout y NavMenu
│   │   ├── pages/                  # Portada, acceso, eventos, detalle, inscripciones, panel
│   │   └── shared/                 # Tarjeta de evento, mensajes, estados vacíos, etiquetas
│   ├── models/                     # DTO del frontend y modelos de formulario
│   ├── services/                   # Clientes de la API y sesión del usuario
│   ├── wwwroot/                    # Hoja de estilos propia (app.css)
│   ├── Program.cs, App.razor, Routes.razor, _Imports.razor
│   ├── Dockerfile
│   └── FrontGestionEventos.csproj
├── docs/                           # Documentación previa (modelo verbal, C4, SOLID, patrones)
├── docker-compose.yml
├── .env.example
├── .gitignore
├── README.md
└── INFORME_ARQUITECTURA_SOLID_GOF.md
```

## 7. Requisitos previos

- **Docker Desktop** (o Docker Engine + Compose v2) en ejecución.
- Puertos libres en el equipo anfitrión: **8080** (frontend) y **8081** (API).
- ~4 GB de memoria libre para el contenedor de SQL Server.
- Para compilar y ejecutar las pruebas fuera de Docker: **SDK de .NET 10**.

No se requiere instalar SQL Server ni ninguna base de datos: todo corre en contenedores.

## 8. Comando de inicio

```bash
docker compose up -d --build
```

La primera vez descarga las imágenes y compila los proyectos (puede tardar varios
minutos). La API aplica el esquema sobre SQL Server y carga los datos de demostración de
forma **idempotente**, sin necesidad de scripts adicionales.

Verificación rápida del estado:

```bash
docker compose ps
docker compose logs --tail=200
docker compose logs -f
```

## 9. Puertos y direcciones

| Servicio | URL | Puerto del equipo | Puerto del contenedor |
| --- | --- | ---: | ---: |
| Frontend Blazor | <http://localhost:8080> | 8080 | 8080 |
| API ASP.NET Core | <http://localhost:8081> | 8081 | 8080 |
| Swagger / OpenAPI | <http://localhost:8081/swagger> | 8081 | 8080 |
| Comprobación de salud | <http://localhost:8081/health> | 8081 | 8080 |
| SQL Server | no se publica (acceso interno `sqlserver:1433`) | — | 1433 |

> Los puertos 8080 y 8081 pertenecen a **contenedores distintos**, por lo que no entran en
> conflicto entre sí.

## 10. Cuentas de demostración

Cuentas **locales** creadas por el inicializador de datos. No están conectadas a ninguna
plataforma institucional.

| Rol | Correo | Contraseña |
| --- | --- | --- |
| Estudiante (principal) | `estudiante@uni.edu.co` | `Estudiante2024!` |
| Responsable (principal) | `responsable@uni.edu.co` | `Responsable2024!` |
| Estudiante adicional | `ana.gomez@uni.edu.co` | `Estudiante2024!` |
| Estudiante adicional | `juan.perez@uni.edu.co` | `Estudiante2024!` |
| Estudiante adicional | `laura.torres@uni.edu.co` | `Estudiante2024!` |
| Responsable adicional | `coordinacion@uni.edu.co` | `Responsable2024!` |

Las contraseñas se almacenan como hash **PBKDF2-HMAC-SHA256** con salt por usuario; nunca
en texto plano.

### Cómo se conserva la sesión

1. El formulario de acceso valida las credenciales **contra la API** (servidor a servidor).
2. La API devuelve un JWT. El frontend lo asocia a un **ticket de un solo uso** y envía al
   navegador únicamente ese ticket: el token JWT nunca sale del servidor.
3. El navegador visita `GET /api-sesion/establecer?ticket=…`, el frontend canjea el ticket
   por una **cookie cifrada con Data Protection** (`HttpOnly`, `SameSite=Lax`) y redirige a
   la pantalla del rol correspondiente.
4. En cada petición a la API, el token se toma de la cookie y se envía en el encabezado
   `Authorization`. La cookie persiste entre recargas y queda en el volumen
   `claves-front`, de modo que las claves de Data Protection sobreviven a la recreación del
   contenedor.
5. `GET /api-sesion/cerrar` elimina la cookie y termina la sesión.

Las rutas protegidas (`/eventos`, `/responsable/...`) redirigen a `/acceder` cuando no hay
sesión válida, y el menú se adapta al rol autenticado.

Los datos de demostración incluyen 5 categorías, 8 eventos (cuatro con inscripciones
abiertas, uno con los cupos agotados y dos ya finalizados), inscripciones de varios
estudiantes y registros de asistencia de un evento ya realizado.

## 11. Detener los servicios (sin borrar los datos)

```bash
docker compose down
```

Este comando detiene y elimina los contenedores, pero **conserva el volumen de SQL
Server**: la información sigue ahí y se recupera al volver a levantar los servicios.

Para borrar también los datos (y regenerar los de demostración desde cero):

```bash
docker compose down -v
docker compose up -d
```

## 12. Cómo ejecutar las pruebas

**Pruebas unitarias** (55 casos: Builder, Facade, State, ciclo de inscripción, retirada de
eventos, registro de usuarios, autenticación, participantes y asistencia):

```bash
cd api-gestion-academico/pruebas
dotnet test
```

**Verificación de extremo a extremo de la API** (88 comprobaciones: inventario de rutas de
Swagger, RF-01 a RF-12, RN-01 a RN-08, autenticación, token inválido y expirado, registro
público de estudiantes, estado de inscripción en el listado, observaciones de asistencia,
**retirada de eventos**, autorización por rol y validaciones):

```bash
python api-gestion-academico/pruebas/verificacion-extremo-a-extremo.py
```

**Verificación de concurrencia** (dos estudiantes compitiendo por el último cupo):

```bash
python api-gestion-academico/pruebas/verificacion-concurrencia.py
```

**Pruebas de la interfaz con navegador real** (26 pruebas con Playwright sobre Microsoft
Edge: inicio de sesión de ambos roles, listado, detalle, filtros, inscripción y estado del
botón, cancelación y reinscripción, historial, separación de roles, registro público de
estudiantes, categorías del formulario, registro y modificación de evento, **retirada de un
evento con confirmación**, participantes, observaciones de asistencia, rutas protegidas y
cierre de sesión):

```bash
cd front/pruebas-e2e
npm install
node --test prueba-frontend.test.mjs
```

Las tres suites de integración **crean sus propios datos y los limpian al terminar**
(eventos con nombre `E2E …`); no modifican los datos de demostración.

## 13. Configuración opcional

Para cambiar la contraseña de SQL Server o la clave de firma de los tokens, copie el
archivo de ejemplo y edite los valores:

```bash
cp .env.example .env
```

| Variable | Descripción | Valor por defecto |
| --- | --- | --- |
| `MSSQL_SA_PASSWORD` | Contraseña de la cuenta `sa` de SQL Server | `GestionEventos2024!` |
| `JWT_CLAVE` | Clave de firma de los tokens (mínimo 32 caracteres) | clave de demostración local |

## 14. Documentación técnica

- **`INFORME_ARQUITECTURA_SOLID_GOF.md`**: arquitectura implementada, aplicación de cada
  principio SOLID con referencias al código real, los tres patrones GoF (Builder, Facade y
  State) con su flujo de ejecución, matriz de trazabilidad RF/RN, decisiones de
  accesibilidad, puertos efectivos y resultados de la validación.
- **`db/README.md`**: correspondencia entre el modelo de dominio y el esquema relacional.
- **`docs/`**: documentación previa del proyecto (modelo verbal, análisis C4, SOLID y
  selección de patrones GoF).

## 15. Notas para la demostración

1. `docker compose up -d --build` y abrir <http://localhost:8080>.
2. Iniciar sesión como **responsable** (`responsable@uni.edu.co`) → *Registrar evento*:
   complete nombre, descripción, fecha futura, lugar, cupos y categoría.
3. Cerrar sesión e iniciar sesión como **estudiante** (`estudiante@uni.edu.co`).
4. En *Eventos disponibles* aparece el evento recién creado: abrir su detalle e
   pulsar **Inscribirme en el evento**. La confirmación aparece en pantalla.
5. *Mis inscripciones* muestra el registro activo y permite cancelarlo mientras el evento
   admita cancelaciones.
6. Con el rol responsable: *Mis eventos* → *Participantes* (registrar asistencia) →
   *Asistencia* (consultar el registro).
