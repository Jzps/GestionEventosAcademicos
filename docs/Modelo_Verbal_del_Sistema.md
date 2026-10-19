# Modelo Verbal del Sistema

## Descripción del problema actual

Las instituciones educativas realizan diferentes eventos académicos, como conferencias, seminarios, talleres, charlas y actividades de formación. Actualmente, la información y gestión de estos eventos puede encontrarse distribuida entre diferentes medios y procesos.

Esta situación puede dificultar el acceso de los estudiantes a información actualizada sobre los eventos, así como la organización de las inscripciones, participantes y asistencia por parte de los responsables.

## Causas identificadas

- La información de los eventos puede encontrarse distribuida en diferentes medios.
- No existe un mecanismo centralizado para organizar la información de los eventos.
- El proceso de inscripción puede gestionarse mediante mecanismos independientes.
- El seguimiento de participantes y asistencia puede resultar difícil de organizar.

## Impacto en los usuarios

El problema afecta principalmente a estudiantes y responsables u organizadores de eventos.

### Estudiantes

- Dificultad para conocer los eventos disponibles.
- Dificultad para consultar información actualizada.
- Dificultad para gestionar y verificar sus inscripciones.
- Dificultad para consultar su participación en las actividades.

### Responsables u organizadores

- Dificultad para mantener organizada la información de los eventos.
- Dificultad para controlar las personas inscritas.
- Dificultad para consultar la información de los participantes.
- Dificultad para realizar seguimiento de la asistencia.

## Flujo actual

| Fase del Proceso              | Responsable del Evento                                                                        | Estudiante                                                |
| :---------------------------- | :-------------------------------------------------------------------------------------------- | :-------------------------------------------------------- |
| **1. Planificación**          | - Identifica la necesidad del evento<br>- Define la información (fechas, cupos, lugar)        |                                                           |
| **2. Difusión e Inscripción** | - Comunica la información a los estudiantes<br>- Recopila la información de los participantes | - Conoce el evento disponible<br>- Realiza su inscripción |
| **3. Ejecución y Control**    | - Realiza el evento<br>- Verifica la participación<br>- Registra la asistencia final          | - Asiste al evento                                        |

---

## Propuesta de solución

Se propone desarrollar un **Sistema de Gestión de Eventos Académicos** que centralice la información relacionada con las actividades académicas.

La solución busca facilitar la gestión de los eventos por parte de los responsables y permitir a los estudiantes consultar las actividades disponibles y gestionar su participación.

También se busca organizar la información de participantes y asistencia, proporcionando a los responsables un mejor seguimiento de los eventos académicos.

---

## Requisitos de Usuario

| ID. Requisito | Nombre del Requisito         | Descripción del Requisito                                                                              |
| :------------ | :--------------------------- | :----------------------------------------------------------------------------------------------------- |
| **RU-01**     | Consultar eventos            | El estudiante necesita consultar los eventos académicos disponibles y su información principal.        |
| **RU-02**     | Consultar detalle del evento | El estudiante necesita consultar la información detallada de un evento antes de decidir si participar. |
| **RU-03**     | Gestionar inscripción        | El estudiante necesita inscribirse y cancelar su inscripción en los eventos disponibles.               |
| **RU-04**     | Consultar inscripciones      | El estudiante necesita consultar los eventos en los que se encuentra inscrito.                         |
| **RU-05**     | Gestionar eventos            | El responsable necesita registrar, modificar y gestionar la información de los eventos académicos.     |
| **RU-06**     | Gestionar participantes      | El responsable necesita consultar y controlar las personas inscritas en cada evento.                   |
| **RU-07**     | Gestionar asistencia         | El responsable necesita registrar y consultar la asistencia de los participantes.                      |

---

## Requisitos de Sistema – Funcionales

| ID. Requisito | Nombre del Requisito             | Descripción del Requisito                                                                                                                 | Usuario     |
| :------------ | :------------------------------- | :---------------------------------------------------------------------------------------------------------------------------------------- | :---------- |
| **RF-01**     | Listar eventos                   | El sistema debe permitir consultar un listado de los eventos académicos disponibles.                                                      | Estudiante  |
| **RF-02**     | Consultar información del evento | El sistema debe mostrar la información de un evento, incluyendo nombre, descripción, fecha, hora, lugar, cupos disponibles y responsable. | Estudiante  |
| **RF-03**     | Filtrar eventos                  | El sistema debe permitir filtrar los eventos disponibles según criterios como nombre, fecha o categoría.                                  | Estudiante  |
| **RF-04**     | Inscribirse a un evento          | El sistema debe permitir al estudiante realizar su inscripción a un evento que tenga disponibilidad.                                      | Estudiante  |
| **RF-05**     | Cancelar inscripción             | El sistema debe permitir al estudiante cancelar su inscripción a un evento en el que se encuentre registrado.                             | Estudiante  |
| **RF-06**     | Consultar inscripciones          | El sistema debe permitir al estudiante consultar los eventos en los que está inscrito y el estado de sus inscripciones.                   | Estudiante  |
| **RF-07**     | Registrar evento                 | El sistema debe permitir al responsable registrar un nuevo evento académico ingresando la información requerida.                          | Responsable |
| **RF-08**     | Modificar evento                 | El sistema debe permitir al responsable modificar la información de un evento registrado.                                                 | Responsable |
| **RF-09**     | Consultar eventos gestionados    | El sistema debe permitir al responsable consultar los eventos que administra.                                                             | Responsable |
| **RF-10**     | Consultar participantes          | El sistema debe permitir al responsable consultar a los estudiantes inscritos en un evento.                                               | Responsable |
| **RF-11**     | Registrar asistencia             | El sistema debe permitir al responsable registrar la asistencia de los participantes de un evento.                                        | Responsable |
| **RF-12**     | Consultar asistencia             | El sistema debe permitir al responsable consultar el registro de asistencia de un evento.                                                 | Responsable |

---

## Requisitos de Sistema – No Funcionales

| ID. Requisito | Tipo de RNF    | Descripción del Requisito                                                                                                                             |
| :------------ | :------------- | :---------------------------------------------------------------------------------------------------------------------------------------------------- |
| **RNF-01**    | Usabilidad     | El sistema debe presentar una interfaz clara y sencilla que permita a estudiantes y responsables realizar sus tareas sin procedimientos innecesarios. |
| **RNF-02**    | Accesibilidad  | La información y los elementos principales de la interfaz deben ser fácilmente identificables y legibles para los usuarios.                           |
| **RNF-03**    | Seguridad      | El sistema debe restringir las funcionalidades de acuerdo con el rol del usuario autenticado.                                                         |
| **RNF-04**    | Compatibilidad | La interfaz debe responder y adaptarse correctamente a diferentes tamaños de pantalla.                                                                |
| **RNF-05**    | Consistencia   | El sistema debe mantener una estructura y navegación consistente entre sus diferentes módulos y pantallas.                                            |

---

## Reglas de Negocio

| ID. RN    | Descripción                                                                                                     |
| :-------- | :-------------------------------------------------------------------------------------------------------------- |
| **RN-01** | Un estudiante no podrá inscribirse a un evento cuando no existan cupos disponibles.                             |
| **RN-02** | Un estudiante no podrá tener más de una inscripción activa en el mismo evento.                                  |
| **RN-03** | Un evento debe tener una fecha y hora de inicio posteriores a la fecha y hora de su registro.                   |
| **RN-04** | La cantidad de cupos de un evento debe ser mayor que cero.                                                      |
| **RN-05** | Un evento no podrá recibir nuevas inscripciones una vez iniciada su fecha y hora de realización.                |
| **RN-06** | La asistencia sólo podrá registrarse para estudiantes que tengan una inscripción en el evento.                  |
| **RN-07** | Un estudiante solo podrá cancelar su inscripción mientras el evento se encuentre habilitado para inscripciones. |
| **RN-08** | La cantidad de participantes inscritos no podrá superar la cantidad de cupos definida para el evento.           |

> **Alcance del MVP:**  
> El MVP contempla las funcionalidades esenciales para el flujo principal del sistema: el responsable registra el evento, y el estudiante lo consulta (listado y detalle) y se inscribe. Se excluyen funcionalidades de seguimiento como consultar inscripciones, eventos gestionados y participantes, ya que en esta primera etapa pueden manejarse manualmente sin afectar la validación del flujo core.

---

## Historias de Usuario / Casos de Uso (Matriz de Jira)

| ID        | Épica | Historia de Usuario              | Prioridad | Puntos |
| :-------- | :---- | :------------------------------- | :-------- | :----- |
| **HU-01** | EP-01 | Consultar eventos disponibles    | Alta      | 2      |
| **HU-02** | EP-01 | Consultar información del evento | Alta      | 3      |
| **HU-03** | EP-01 | Filtrar eventos                  | Media     | 5      |
| **HU-04** | EP-02 | Inscribirse a un evento          | Alta      | 3      |
| **HU-05** | EP-02 | Cancelar inscripción             | Media     | 3      |
| **HU-06** | EP-02 | Consultar inscripciones          | Alta      | 3      |
| **HU-07** | EP-03 | Registrar evento                 | Alta      | 5      |
| **HU-08** | EP-03 | Modificar evento                 | Alta      | 3      |
| **HU-09** | EP-03 | Consultar eventos gestionados    | Alta      | 2      |
| **HU-10** | EP-04 | Consultar participantes          | Alta      | 3      |
| **HU-11** | EP-04 | Registrar asistencia             | Alta      | 5      |
| **HU-12** | EP-04 | Consultar asistencia             | Media     | 3      |

---

## Detalle de Épicas e Historias de Usuario

### ÉPICA 1 — Consulta de eventos académicos

#### Título para Jira

`EP-01 – Consulta de eventos académicos`

#### Descripción

- **Objetivo:** Facilitar a los estudiantes el acceso a la información de los eventos académicos disponibles. Esta épica agrupa las funcionalidades que permiten al estudiante conocer las actividades académicas ofrecidas por la institución, consultar la información detallada de cada evento y encontrar eventos mediante criterios de búsqueda.
- **Actor principal:** Estudiante.
- **Historias asociadas:**
  - HU-01 – Consultar eventos disponibles.
  - HU-02 – Consultar información del evento.
  - HU-03 – Filtrar eventos.

---

#### HU-01 — Consultar eventos disponibles

##### Título para Jira

`HU-01 – Consultar eventos disponibles`

##### Descripción

> **Como** estudiante,  
> **quiero** consultar el listado de eventos académicos disponibles,  
> **para** conocer las actividades en las que puedo participar.

##### Criterios de aceptación

**Escenario 1: Existen eventos disponibles**

```gherkin
Scenario: Consultar listado de eventos
  Given que el estudiante se encuentra autenticado
  And existen eventos académicos disponibles
  When el estudiante accede a la sección de eventos
  Then el sistema debe mostrar el listado de eventos disponibles
```

**Escenario 2: No existen eventos disponibles**

```gherkin
Scenario: No existen eventos disponibles
  Given que el estudiante se encuentra autenticado
  And no existen eventos académicos disponibles
  When el estudiante accede a la sección de eventos
  Then el sistema debe informar que no existen eventos disponibles
```

- **Story Points:** 2
- **Prioridad:** Alta

---

#### HU-02 — Consultar información del evento

##### Título para Jira

`HU-02 – Consultar información del evento`

##### Descripción

> **Como** estudiante,  
> **quiero** consultar la información detallada de un evento académico,  
> **para** conocer sus características y determinar si es de mi interés.

##### Criterios de aceptación

**Escenario 1: Consultar detalle**

```gherkin
Scenario: Consultar detalle de un evento
  Given que el estudiante visualiza el listado de eventos
  And existe un evento disponible
  When el estudiante selecciona el evento
  Then el sistema debe mostrar la información detallada del evento
```

**Escenario 2: Información mostrada**

```gherkin
Scenario: Mostrar información relevante del evento
  Given que el estudiante consulta el detalle de un evento
  When el sistema muestra la información
  Then debe mostrar la descripción
  And debe mostrar la fecha y hora
  And debe mostrar el lugar
  And debe mostrar los cupos disponibles
  And debe mostrar el responsable
```

- **Story Points:** 3
- **Prioridad:** Alta

---

#### HU-03 — Filtrar eventos

##### Título para Jira

`HU-03 – Filtrar eventos`

##### Descripción

> **Como** estudiante,  
> **quiero** filtrar los eventos académicos disponibles por diferentes criterios,  
> **para** encontrar rápidamente las actividades que sean de mi interés.

##### Criterios de aceptación

**Escenario 1: Filtrar por nombre**

```gherkin
Scenario: Filtrar eventos por nombre
  Given que existen eventos disponibles
  When el estudiante ingresa un nombre como criterio de búsqueda
  Then el sistema debe mostrar los eventos que coincidan con el criterio
```

**Escenario 2: Filtrar por fecha**

```gherkin
Scenario: Filtrar eventos por fecha
  Given que existen eventos disponibles
  When el estudiante selecciona una fecha como criterio
  Then el sistema debe mostrar los eventos correspondientes a la fecha seleccionada
```

**Escenario 3: Filtrar por categoría**

```gherkin
Scenario: Filtrar eventos por categoría
  Given que existen eventos disponibles
  When el estudiante selecciona una categoría
  Then el sistema debe mostrar los eventos correspondientes a la categoría seleccionada
```

**Escenario 4: Sin coincidencia**

```gherkin
Scenario: No existen eventos que coincidan con el filtro
  Given que existen eventos registrados
  When el estudiante aplica un filtro sin coincidencias
  Then el sistema debe informar que no se encontraron eventos
```

- **Story Points:** 5
- **Prioridad:** Media

---

### ÉPICA 2 — Gestión de inscripciones

#### Título para Jira

`EP-02 – Gestión de inscripciones`

#### Descripción

- **Objetivo:** Permitir a los estudiantes gestionar su participación en los eventos académicos. Esta épica agrupa las funcionalidades relacionadas con la inscripción del estudiante a los eventos, incluyendo el registro de una inscripción, su cancelación y la consulta de las inscripciones realizadas.
- **Actor principal:** Estudiante.
- **Historias asociadas:**
  - HU-04 – Inscribirse a un evento.
  - HU-05 – Cancelar inscripción.
  - HU-06 – Consultar inscripciones.

---

#### HU-04 — Inscribirse a un evento

##### Título para Jira

`HU-04 – Inscribirse a un evento`

##### Descripción

> **Como** estudiante,  
> **quiero** inscribirme a un evento académico disponible,  
> **para** asegurar mi participación en la actividad.

##### Criterios de aceptación

**Escenario 1: Inscripción exitosa**

```gherkin
Scenario: Inscribirse a un evento disponible
  Given que el estudiante se encuentra autenticado
  And el evento tiene cupos disponibles
  And el estudiante no está inscrito en el evento
  When el estudiante selecciona la opción "Inscribirse"
  Then el sistema debe registrar la inscripción
  And debe mostrar una confirmación
```

**Escenario 2: Evento sin cupos**

```gherkin
Scenario: Intentar inscribirse sin cupos
  Given que el evento no tiene cupos disponibles
  When el estudiante intenta inscribirse
  Then el sistema no debe permitir la inscripción
  And debe informar que no hay cupos disponibles
```

**Escenario 3: Inscripción duplicada**

```gherkin
Scenario: Intentar realizar una inscripción duplicada
  Given que el estudiante ya está inscrito en el evento
  When intenta inscribirse nuevamente
  Then el sistema no debe registrar otra inscripción
  And debe informar que ya se encuentra inscrito
```

- **Story Points:** 3
- **Prioridad:** Alta

---

#### HU-05 — Cancelar inscripción

##### Título para Jira

`HU-05 – Cancelar inscripción`

##### Descripción

> **Como** estudiante,  
> **quiero** cancelar mi inscripción a un evento,  
> **para** liberar mi cupo cuando ya no pueda participar.

##### Criterios de aceptación

**Escenario 1: Cancelación permitida**

```gherkin
Scenario: Cancelar una inscripción activa
  Given que el estudiante tiene una inscripción activa
  And el evento permite cancelar la inscripción
  When el estudiante selecciona "Cancelar inscripción"
  Then el sistema debe cancelar la inscripción
  And debe actualizar su estado
```

**Escenario 2: Cancelación no permitida**

```gherkin
Scenario: Intentar cancelar fuera del período permitido
  Given que el estudiante tiene una inscripción activa
  And el evento ya no permite cancelar inscripciones
  When el estudiante intenta cancelar la inscripción
  Then el sistema no debe permitir la cancelación
  And debe informar que la cancelación no está disponible
```

- **Story Points:** 3
- **Prioridad:** Media

---

#### HU-06 — Consultar inscripciones

##### Título para Jira

`HU-06 – Consultar inscripciones`

##### Descripción

> **Como** estudiante,  
> **quiero** consultar mis inscripciones a eventos académicos,  
> **para** conocer las actividades en las que estoy registrado y el estado de cada inscripción.

##### Criterios de aceptación

**Escenario 1: Existen inscripciones**

```gherkin
Scenario: Consultar inscripciones registradas
  Given que el estudiante tiene inscripciones registradas
  When accede a la sección "Mis inscripciones"
  Then el sistema debe mostrar sus eventos inscritos
  And debe mostrar el estado de cada inscripción
```

**Escenario 2: No existen inscripciones**

```gherkin
Scenario: Consultar cuando no existen inscripciones
  Given que el estudiante no tiene inscripciones registradas
  When accede a la sección "Mis inscripciones"
  Then el sistema debe informar que no tiene eventos inscritos
```

- **Story Points:** 3
- **Prioridad:** Alta

---

### ÉPICA 3 — Gestión de eventos académicos

#### Título para Jira

`EP-03 – Gestión de eventos académicos`

#### Descripción

- **Objetivo:** Facilitar a los responsables la administración de la información de los eventos académicos. Esta épica contempla las funcionalidades necesarias para que los responsables registren nuevos eventos, modifiquen su información y consulten los eventos que tienen bajo su gestión.
- **Actor principal:** Responsable.
- **Historias asociadas:**
  - HU-07 – Registrar evento.
  - HU-08 – Modificar evento.
  - HU-09 – Consultar eventos gestionados.

---

#### HU-07 — Registrar evento

##### Título para Jira

`HU-07 – Registrar evento`

##### Descripción

> **Como** responsable,  
> **quiero** registrar un nuevo evento académico con su información correspondiente,  
> **para** ponerlo a disposición de los estudiantes.

##### Criterios de aceptación

**Escenario 1: Registro exitoso**

```gherkin
Scenario: Registrar un nuevo evento
  Given que el responsable se encuentra autenticado
  When ingresa la información requerida del evento
  And confirma el registro
  Then el sistema debe crear el evento
  And debe mostrar una confirmación del registro
```

**Escenario 2: Información incompleta**

```gherkin
Scenario: Intentar registrar un evento con información incompleta
  Given que el responsable se encuentra en el formulario de registro
  When intenta registrar el evento sin completar la información requerida
  Then el sistema no debe registrar el evento
  And debe indicar los campos que deben completarse
```

- **Story Points:** 5
- **Prioridad:** Alta

---

#### HU-08 — Modificar evento

##### Título para Jira

`HU-08 – Modificar evento`

##### Descripción

> **Como** responsable,  
> **quiero** modificar la información de un evento que administro,  
> **para** mantener actualizados sus datos.

##### Criterios de aceptación

**Escenario 1: Modificación exitosa**

```gherkin
Scenario: Modificar información de un evento
  Given que existe un evento administrado por el responsable
  When modifica la información del evento
  And confirma los cambios
  Then el sistema debe actualizar la información del evento
  And debe mostrar una confirmación
```

**Escenario 2: Cancelar modificación**

```gherkin
Scenario: Cancelar una modificación
  Given que el responsable está modificando un evento
  When decide cancelar los cambios
  Then el sistema no debe modificar la información registrada
```

- **Story Points:** 3
- **Prioridad:** Alta

---

#### HU-09 — Consultar eventos gestionados

##### Título para Jira

`HU-09 – Consultar eventos gestionados`

##### Descripción

> **Como** responsable,  
> **quiero** consultar los eventos que administro,  
> **para** realizar seguimiento de las actividades bajo mi responsabilidad.

##### Criterios de aceptación

**Escenario 1: Existen eventos gestionados**

```gherkin
Scenario: Consultar eventos administrados
  Given que el responsable se encuentra autenticado
  And tiene eventos registrados
  When accede a la sección de eventos gestionados
  Then el sistema debe mostrar los eventos que administra
```

**Escenario 2: No existen eventos gestionados**

```gherkin
Scenario: No existen eventos administrados
  Given que el responsable no tiene eventos registrados
  When accede a la sección de eventos gestionados
  Then el sistema debe informar que no tiene eventos registrados
```

- **Story Points:** 2
- **Prioridad:** Alta

---

### ÉPICA 4 — Gestión de participantes y asistencia

#### Título para Jira

`EP-04 – Gestión de participantes y asistencia`

#### Descripción

- **Objetivo:** Facilitar a los responsables el seguimiento de los estudiantes inscritos y su asistencia a los eventos académicos. Esta épica contempla las funcionalidades relacionadas con la consulta de participantes, el registro de asistencia y la consulta de los registros de asistencia de cada evento.
- **Actor principal:** Responsable.
- **Historias asociadas:**
  - HU-10 – Consultar participantes.
  - HU-11 – Registrar asistencia.
  - HU-12 – Consultar asistencia.

---

#### HU-10 — Consultar participantes

##### Título para Jira

`HU-10 – Consultar participantes`

##### Descripción

> **Como** responsable,  
> **quiero** consultar los estudiantes inscritos en un evento,  
> **para** conocer las personas que participarán en la actividad.

##### Criterios de aceptación

**Escenario 1: Existen participantes**

```gherkin
Scenario: Consultar participantes inscritos
  Given que existe un evento administrado por el responsable
  And existen estudiantes inscritos
  When el responsable consulta los participantes del evento
  Then el sistema debe mostrar los estudiantes inscritos
```

**Escenario 2: No existen participantes**

```gherkin
Scenario: Consultar un evento sin participantes
  Given que existe un evento administrado por el responsable
  And no existen estudiantes inscritos
  When el responsable consulta los participantes
  Then el sistema debe informar que no existen participantes inscritos
```

- **Story Points:** 3
- **Prioridad:** Alta

---

#### HU-11 — Registrar asistencia

##### Título para Jira

`HU-11 – Registrar asistencia`

##### Descripción

> **Como** responsable,  
> **quiero** registrar la asistencia de los estudiantes inscritos,  
> **para** llevar un control de su participación en el evento.

##### Criterios de aceptación

**Escenario 1: Registrar asistencia**

```gherkin
Scenario: Registrar asistencia de un participante
  Given que el estudiante se encuentra inscrito en el evento
  When el responsable registra su asistencia
  Then el sistema debe guardar el registro de asistencia
```

**Escenario 2: Participante no inscrito**

```gherkin
Scenario: Intentar registrar asistencia de un estudiante no inscrito
  Given que el estudiante no está inscrito en el evento
  When el responsable intenta registrar su asistencia
  Then el sistema no debe permitir registrar la asistencia
  And debe informar que el estudiante no está inscrito
```

- **Story Points:** 5
- **Prioridad:** Alta

---

#### HU-12 — Consultar asistencia

##### Título para Jira

`HU-12 – Consultar asistencia`

##### Descripción

> **Como** responsable,  
> **quiero** consultar los registros de asistencia de un evento,  
> **para** realizar seguimiento de la participación de los estudiantes.

##### Criterios de aceptación

**Escenario 1: Existen registros de asistencia**

```gherkin
Scenario: Consultar registros de asistencia
  Given que existen registros de asistencia para un evento
  When el responsable consulta la asistencia
  Then el sistema debe mostrar el estado de asistencia de los participantes
```

**Escenario 2: No existen registros**

```gherkin
Scenario: Consultar asistencia sin registros
  Given que no existen registros de asistencia para el evento
  When el responsable consulta la asistencia
  Then el sistema debe informar que no existen registros de asistencia
```

- **Story Points:** 3
- **Prioridad:** Media

## 1. Descripción de Gráficos y Arquitectura Conceptual

Para el desarrollo y estructuración de este sistema de gestión de eventos académicos, se han empleado tres herramientas clave de modelado y planificación ágil:

### 1.1. Mapa de Impacto (Impact Map)

- **¿Qué es?** Es un mapa mental estratégico que conecta los objetivos del negocio con los actores involucrados, los impactos deseados en su comportamiento y los entregables o funcionalidades necesarios para lograrlo.
- **Propósito en este proyecto:** Alinear el objetivo principal (_"Facilitar la gestión de eventos académicos y mejorar el acceso de los estudiantes a la información, inscripción y seguimiento"_) con las necesidades de los dos actores principales (**Estudiante** y **Responsable**), definiendo qué cambios de comportamiento se esperan de ellos y qué características del sistema impulsarán dichos cambios.

### 1.2. Story Mapping (Mapa de Historias de Usuario)

- **¿Qué es?** Una técnica visual que organiza las historias de usuario de forma bidimensional: de izquierda a derecha según el flujo de la experiencia del usuario (actividades y tareas), y de arriba hacia abajo según su prioridad o versiones de lanzamiento (MVP - Versión 1, Versión 2, Versión 3).
- **Propósito en este proyecto:** Estructurar el backlog del producto de manera lógica y secuencial, permitiendo identificar claramente el Producto Mínimo Viable (**MVP**) para un lanzamiento temprano y planificar iteraciones futuras de manera organizada.

### 1.3. Diagrama de Clases

- **¿Qué es?** Un diagrama de estructura estática en UML que muestra las clases del sistema, sus atributos, operaciones (métodos) y las relaciones estáticas entre ellas (asociaciones, herencia, enumeraciones).
- **Propósito en este proyecto:** Servir como el modelo de dominio técnico y base para el diseño de la base de datos y la arquitectura de software orientada a objetos, especificando cómo interactúan entidades como `Usuario`, `Estudiante`, `Responsable`, `Evento`, `Categoria`, `Inscripcion` y `Asistencia`.

---

## 2. Mapa de Impacto

### Objetivo del Proyecto

> **"Facilitar la gestión de eventos académicos y mejorar el acceso de los estudiantes a la información, inscripción y seguimiento de los eventos."**

---

### Actor 1: Estudiante

- **Impacto 1:** Necesita conocer los eventos académicos disponibles.
  - **Entregables:**
    - Listar eventos
    - Filtrar eventos
- **Impacto 2:** Necesita realizar y gestionar sus inscripciones.
  - **Entregables:**
    - Inscribirse a un evento
    - Cancelar inscripción
    - Consultar inscripciones

---

### Actor 2: Responsable

- **Impacto 1:** Necesita administrar la información de los eventos académicos.
  - **Entregables:**
    - Registrar evento
    - Modificar evento
    - Consultar eventos gestionados
- **Impacto 2:** Necesita controlar las personas inscritas en cada evento.
  - **Entregables:**
    - Consultar participantes
- **Impacto 3:** Necesita registrar y consultar la asistencia de los participantes.
  - **Entregables:**
    - Registrar asistencia
    - Consultar asistencia

---

## 3. Story Mapping (Historias de Usuario)

### 3.1. Consultar Eventos

- **Tareas:**
  - Consultar eventos disponibles
  - Consultar información del evento
  - Filtrar eventos
- **Historias de Usuario:**
  - **Versión 1 (MVP):**
    - _Estudiante:_ "Como estudiante, quiero ver el listado de eventos académicos disponibles para conocer las actividades que puedo realizar."
    - _Estudiante:_ "Como estudiante, quiero consultar la descripción del evento para conocer de qué trata."
    - _Estudiante:_ "Como estudiante, quiero filtrar eventos por nombre para encontrar rápidamente uno específico."
  - **Versión 2:**
    - _Estudiante:_ "Como estudiante, quiero visualizar los eventos con su información básica para identificar cuáles son de mi interés."
    - _Estudiante:_ "Como estudiante, quiero consultar fecha, hora y lugar para saber cuándo y dónde se realizará."
    - _Estudiante:_ "Como estudiante, quiero filtrar eventos por fecha para encontrar actividades disponibles en un periodo determinado."
  - **Versión 3:**
    - _Estudiante:_ "Como estudiante, quiero consultar los cupos disponibles y el responsable para saber si puedo participar."
    - _Estudiante:_ "Como estudiante, quiero filtrar eventos por categoría para encontrar actividades relacionadas con mis intereses."

---

### 3.2. Gestionar Inscripción

- **Tareas:**
  - Inscribirse a un evento
  - Cancelar inscripción
  - Consultar inscripciones
- **Historias de Usuario:**
  - **Versión 1 (MVP):**
    - _Estudiante:_ "Como estudiante, quiero inscribirme a un evento disponible para asegurar mi participación."
  - **Versión 2:**
    - _Estudiante:_ "Como estudiante, quiero cancelar mi inscripción antes de que finalice el periodo permitido para poder retirarme del evento."
    - _Estudiante:_ "Como estudiante, quiero consultar el estado de mis inscripciones para conocer la situación de cada registro."

---

### 3.3. Gestionar Eventos

- **Tareas:**
  - Registrar evento
  - Modificar evento
  - Consultar eventos gestionados
- **Historias de Usuario:**
  - **Versión 1 (MVP):**
    - _Responsable:_ "Como responsable, quiero registrar un nuevo evento académico para ponerlo a disposición de los estudiantes."
  - **Versión 2:**
    - _Responsable:_ "Como responsable, quiero ingresar la información requerida del evento para registrar sus datos correctamente."
    - _Responsable:_ "Como responsable, quiero actualizar los datos de fecha, hora, lugar o cupos de un evento para mantener su información vigente."
  - **Versión 3:**
    - _Responsable:_ "Como responsable, quiero consultar la información de mis eventos para verificar datos registrados."

---

### 3.4. Gestionar Participantes

- **Tareas:**
  - Consultar participantes
- **Historias de Usuario:**
  - **Versión 1 (MVP):**
    - _Estudiante / Responsable:_ "Como usuario, quiero consultar los estudiantes inscritos en un evento para conocer sus participantes."

---

## 4. Diagrama de Clases (Modelo de Dominio)

### 4.1. Clases Principales y Jerarquía

- **`Usuario` (Abstracta)**
  - **Atributos:**
    - `-Long id`
    - `-String nombreCompleto`
    - `-String email`
    - `-String contrasena`
    - `-RolUsuario rol`
    - `-boolean activo`
  - **Métodos:**
    - `+autenticar(email, contrasena) : boolean`
    - `+getRol() : RolUsuario`
    - `+actualizarPerfil(datos) : void`

- **`Estudiante` (Hereda de `Usuario`)**
  - **Atributos:**
    - `-String codigo`
    - `-String programa`
  - **Métodos:**
    - `+consultarEventosDisponibles() : List<Evento>`
    - `+filtrarEventos(nombre, fecha, categoria) : List<Evento>`
    - `+consultarDetalleEvento(idEvento) : Evento`
    - `+inscribirse(evento) : Inscripcion`
    - `+cancelarInscripcion(inscripcion) : void`
    - `+consultarInscripciones() : List<Inscripcion>`
    - `+estaInscritoEn(evento) : boolean`

- **`Responsable` (Hereda de `Usuario`)**
  - **Atributos:**
    - `-String dependencia`
    - `-String cargo`
  - **Métodos:**
    - `+registrarEvento(evento) : Evento`
    - `+modificarEvento(evento) : void`
    - `+consultarEventosGestionados() : List<Evento>`
    - `+consultarParticipantes(evento) : List<Inscripcion>`
    - `+registrarAsistencia(inscripcion, asisitio) : Asistencia`
    - `+consultarAsistencia(evento) : List<Asistencia>`

---

### 4.2. Entidades de Negocio

- **`Categoria`**
  - **Atributos:**
    - `-Long id`
    - `-String nombre`
    - `-String descripcion`
  - **Métodos:**
    - `+getEventos() : List<Evento>`

- **`Evento`**
  - **Atributos:**
    - `-Long id`
    - `-String nombre`
    - `-String descripcion`
    - `-LocalDateTime fechaHoraInicio`
    - `-LocalDateTime fechaHoraFin`
    - `-String lugar`
    - `-int cuposTotales`
    - `-EstadoEvento estado`
    - `-LocalDateTime fechaRegistro`
  - **Métodos:**
    - `+getCuposDisponibles() : int`
    - `+tieneCuposDisponibles() : boolean`
    - `+admiteInscripciones() : boolean`
    - `+validarFechaInicio() : boolean`
    - `+validarCupos() : boolean`
    - `+agregarInscripcion(estudiante) : Inscripcion`
    - `+getParticipantes() : List<Inscripcion>`
    - `+actualizarEstado() : void`

- **`Inscripcion`**
  - **Atributos:**
    - `-Long id`
    - `-LocalDateTime fechaHoraRegistro`
    - `-EstadoInscripcion estado`
    - `-LocalDateTime fechaHoraCancelacion`
  - **Métodos:**
    - `+cancelar() : void`
    - `+estaActiva() : boolean`
    - `+tieneAsistenciaRegistrada() : boolean`

- **`Asistencia`**
  - **Atributos:**
    - `-Long id`
    - `-boolean asistio`
    - `-LocalDateTime fechaHoraRegistro`
    - `-String observacion`
  - **Métodos:**
    - `+registrar(asistio) : void`
    - `+actualizar(asistio) : void`

---

### 4.3. Enumeraciones

- **`RolUsuario`**
  - `ESTUDIANTE`
  - `RESPONSABLE`

- **`EstadoEvento`**
  - `PROGRAMADO`
  - `INSCRIPCIONES_ABIERTAS`
  - `INSCRIPCIONES_CERRADAS`
  - `EN_CURSO`
  - `FINALIZADO`
  - `CANCELADO`

- **`EstadoInscripcion`**
  - `ACTIVA`
