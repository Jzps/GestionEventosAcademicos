# Modelo relacional (SQL Server)

Este directorio contiene la representación relacional del modelo de dominio del
**Sistema de Gestión de Eventos Académicos**.

## Archivo `ModeloRelacional.sql`

El script es la expresión SQL del esquema que la API aplica sobre SQL Server. Se generó
con el siguiente comando (herramienta `dotnet-ef`, versión 10.0.0):

```bash
cd api-gestion-academico
dotnet ef migrations script --idempotent -o ../db/ModeloRelacional.sql
```

Por lo tanto, **el script y el esquema aplicado son el mismo**: la API ejecuta las
migraciones de Entity Framework Core (`api-gestion-academico/repositorios/Migraciones/`),
que son el origen de ambos. Si se modifica el modelo, se debe regenerar el script:

```bash
dotnet ef migrations add <NombreDeLaMigracion> --output-dir repositorios/Migraciones
dotnet ef migrations script --idempotent -o ../db/ModeloRelacional.sql
```

## Tablas

| Tabla | Entidad del dominio | Notas |
| --- | --- | --- |
| `Usuarios` | `Usuario` (abstracta), `Estudiante`, `Responsable` | Tabla única con jerarquía TPH. La columna `Rol` es el discriminador (`Estudiante` / `Responsable`). |
| `Categorias` | `Categoria` | Nombre único. |
| `Eventos` | `Evento` | Incluye `Version` (`rowversion`) para control de concurrencia optimista. |
| `Inscripciones` | `Inscripcion` | Relación N–N entre `Usuarios` (estudiantes) y `Eventos`. |
| `Asistencias` | `Asistencia` | Como máximo un registro por inscripción (índice único `UX_Asistencias_Inscripcion`). |

## Claves y restricciones

- **Claves primarias:** `Id` (`bigint IDENTITY`) en las cinco tablas.
- **Claves foráneas:**
  - `Eventos.ResponsableId` → `Usuarios.Id` (un responsable administra varios eventos).
  - `Eventos.CategoriaId` → `Categorias.Id` (una categoría clasifica varios eventos).
  - `Inscripciones.EstudianteId` → `Usuarios.Id` (un estudiante tiene varias inscripciones).
  - `Inscripciones.EventoId` → `Eventos.Id` (un evento recibe varias inscripciones).
  - `Asistencias.InscripcionId` → `Inscripciones.Id` (relación 1 a 0..1).
- **Restricciones de integridad:**
  - `CK_Eventos_Cupos`: `CuposTotales > 0` (RN-04).
  - `CK_Eventos_Fechas`: `FechaHoraFin > FechaHoraInicio`.
  - `CK_Usuarios_Rol`: el rol solo puede ser `Estudiante` o `Responsable`.
  - `CK_Usuarios_Perfil`: el perfil del usuario debe ser coherente con su rol
    (un estudiante tiene código y programa; un responsable, dependencia y cargo).
  - `UX_Inscripciones_Activas`: índice único filtrado `([EstudianteId], [EventoId]) WHERE [Estado] = 1`,
    que impide dos inscripciones activas del mismo estudiante en el mismo evento (RN-02).
  - `IX_Usuarios_Email`: correo único (una cuenta por persona).
  - `UX_Asistencias_Inscripcion`: una sola asistencia por inscripción.

## Cupos disponibles

Los cupos disponibles **no se almacenan**: se calculan en el dominio como
`CuposTotales - (inscripciones activas)`. Evita valores contradictorios y permite que
el control de concurrencia (bloqueo de aplicación `sp_getapplock` sobre el evento) sea
la única fuente de verdad al inscribirse.

## Restricción operativa al ejecutar DML manualmente

La tabla `Inscripciones` tiene un **índice único filtrado** (`UX_Inscripciones_Activas`).
SQL Server exige la opción `SET QUOTED_IDENTIFIER ON` para ejecutar `INSERT`, `UPDATE` o
`DELETE` sobre tablas con índices filtrados. Entity Framework Core la activa siempre por
su cuenta, de modo que **la aplicación no requiere ninguna medida especial**; pero
cualquier script manual (por ejemplo, `sqlcmd` para limpiar datos de prueba) debe
incluirla explícitamente:

```sql
SET QUOTED_IDENTIFIER ON;
DELETE FROM dbo.Eventos WHERE Nombre LIKE 'E2E%';
```

Sin esa sentencia, SQL Server responde: *«DELETE failed because the following SET options
have incorrect settings: 'QUOTED_IDENTIFIER'»*.

## Datos de demostración

El inicializador `api-gestion-academico/servicios/InicializadorDatos.cs` crea, de forma
idempotente, las cuentas de demostración, las categorías, los eventos (con fechas
relativas al momento de la inicialización), las inscripciones y algunos registros de
asistencia. Si se borra el volumen de Docker, al volver a levantar los contenedores se
vuelven a crear.
