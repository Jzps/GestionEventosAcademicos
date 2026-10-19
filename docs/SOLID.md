# Análisis de Principios SOLID

## Sistema de Gestión de Eventos Académicos

> **Instrumento de apoyo:** Justificación de principios SOLID dentro del alcance de un Producto Mínimo Viable (MVP).  
> **Uso sugerido:** Complete la plantilla durante el análisis del código o diseño y consérvela como evidencia para revisión, retroalimentación o refactorización.

---

## 1. Información General

- **Lenguaje / Framework:** C# y Blazor (utilizados para desarrollar el prototipo).
- **Tipo de Análisis:** Análisis estático del diseño orientado a objetos, basado en el diagrama de clases, los requisitos del sistema y las decisiones de arquitectura previstas para el prototipo.
- **Contexto:** El análisis se realiza sobre el modelo de dominio propuesto para el MVP, que contempla:
  - Registro de eventos por parte del responsable.
  - Consulta de listado, detalle e inscripción a eventos por parte de los estudiantes.
  - Funcionalidades complementarias proyectadas (cancelación de inscripciones y control de asistencia), sin asumir que forman parte del MVP actualmente implementado.

---

## 2. Objetivo del Análisis

Evaluar el diseño de clases del **Sistema de Gestión de Eventos Académicos** respecto a los cinco principios SOLID, identificando decisiones de diseño favorables, posibles problemas de cohesión y acoplamiento, y oportunidades de mejora antes de consolidar la implementación del prototipo.

El análisis se centra en:

- Las responsabilidades de las clases.
- Las posibilidades de extensión.
- La relación de herencia entre `Usuario`, `Estudiante` y `Responsable`.
- La presencia o ausencia de interfaces.
- El nivel de dependencia entre los elementos del modelo.

> **Nota:** Al encontrarse en fase de requisitos y diseño, las conclusiones corresponden al diagrama de clases propuesto. No se consideran resultados de ejecución, pruebas automatizadas ni comportamientos comprobados en código ejecutable.

---

## 3. Descripción Breve del Componente

- **Función Principal:** Representar los elementos principales del dominio académico y las operaciones necesarias para gestionar eventos, usuarios, inscripciones y asistencia.
- **Responsabilidades Actuales:** El modelo contempla la información de usuarios, gestión de eventos y sus cupos, creación y cancelación de inscripciones, consulta de participantes y registro de asistencia. Algunas operaciones se concentran en `Estudiante` y `Responsable`, mientras que las reglas de disponibilidad de eventos recaen principalmente en `Evento`.
- **Clases e Interfaces Involucradas:**
  - _Clases:_ `Usuario`, `Estudiante`, `Responsable`, `Evento`, `Categoria`, `Inscripcion` y `Asistencia`.
  - _Enumeraciones:_ `RolUsuario`, `EstadoEvento` y `EstadoInscripcion`.
  - _Interfaces:_ No se identifican interfaces propias del dominio en el diseño actual.
- **Dependencias Principales:** Herencia entre `Usuario` y sus subclases (`Estudiante`, `Responsable`); asociaciones entre `Evento`, `Categoria`, `Inscripcion` y `Asistencia`. Se emplean colecciones para relaciones y enumeraciones para estados. No se especifican aún repositorios, servicios de aplicación, inyección de dependencias ni persistencia.
- **Relación con otros Módulos:** Define las reglas y entidades centrales. En la arquitectura final se relacionará con la UI (Blazor), autenticación/autorización, persistencia y servicios de aplicación (casos de uso).

---

## 4. Análisis de Principios SOLID

---

### 4.1 S – Single Responsibility Principle (SRP)

> **Definición:** Una clase o módulo debe tener una sola razón para cambiar.

- **Elemento evaluado:** Distribución de responsabilidades en `Usuario`, `Estudiante`, `Responsable`, `Evento`, `Inscripcion` y `Asistencia`.
- **Evaluación:** **Cumple parcialmente**

#### Evidencia observada

- `Evento`: Concentra datos y reglas del evento (`GetCuposDisponibles()`, `TieneCuposDisponibles()`, `AdmiteInscripciones()`, `ValidarCupos()`).
- `Estudiante`: Contiene operaciones de inscripción y consulta (`Inscribirse()`, `CancelarInscripcion()`, `ConsultarInscripciones()`).
- `Responsable`: Reúne registro/modificación de eventos, consulta de participantes y asistencia (`RegistrarEvento()`, `ModificarEvento()`, `ConsultarParticipantes()`, `RegistrarAsistencia()`).
- `Usuario`: Reúne identidad/perfil e incluye comportamientos como `Autenticar()`, `GetRol()` y `ActualizarPerfil()`.

#### Preguntas guía & Hallazgos

1. **¿Tiene una única responsabilidad definida?** `Evento` e `Inscripcion` muestran una buena separación inicial. Sin embargo, `Responsable` agrupa múltiples casos de uso (eventos, participantes, asistencia) que podrían distribuirse en servicios.
2. **¿Mezcla responsabilidades de diferente índole?** `Usuario` combina datos de perfil con la operación `Autenticar()`, la cual debería gestionarse en un servicio de autenticación independiente en C# + Blazor.
3. **¿Existen múltiples razones de cambio?** Sí, en `Responsable` y `Usuario`.
4. **¿El nombre refleja una sola intención?** Sí en las entidades de dominio, aunque acumulan lógica de orquestación.

#### Impacto técnico potencial

- **Complejidad acumulada:** Cambios en flujos de asistencia o participantes afectan a la misma clase `Responsable`.
- **Dificultad de mantenimiento:** La mezcla de autenticación con datos de usuario complica la evolución independiente de las políticas de acceso.
- **Acoplamiento en UI:** Si la lógica de negocio se traslada directamente a componentes Blazor, se mezclará la presentación con las reglas del sistema.

#### Recomendaciones

- Conservar en `Evento` únicamente las reglas del dominio (cupos, fechas) y mover la coordinación a **servicios de aplicación** (`GestionEventosService`, `InscripcionService`, `AsistenciaService`).
- Extraer la autenticación a un servicio especializado (`AuthService`).
- Mantener los componentes Blazor enfocados exclusivamente en la presentación e interacción.

---

### 4.2 O – Open/Closed Principle (OCP)

> **Definición:** Las entidades de software deben estar abiertas para extensión, pero cerradas para modificación.

- **Elemento evaluado:** Capacidad del modelo para incorporar nuevas funcionalidades sin modificar las clases existentes.
- **Evaluación:** **Cumple parcialmente**

#### Evidencia observada

- `Usuario` es una clase abstracta que permite extender nuevos tipos de usuario vía herencia.
- `Evento` gestiona la disponibilidad y los estados mediante `AdmiteInscripciones()` y `ActualizarEstado()`.
- `EstadoEvento` (enum) define los estados: `Programado`, `InscripcionesAbiertas`, `InscripcionesCerradas`, `EnCurso`, `Finalizado`, `Cancelado`.

#### Preguntas guía & Hallazgos

1. **¿Agregar funcionalidad obliga a modificar código estable?** La lógica de `ActualizarEstado()` reside en `Evento`. Si cambian las reglas de transición o se agregan condiciones complejas, habrá que modificar el código interno de la clase.
2. **¿Se usan abstracciones o polimorfismo?** Se usa herencia en `Usuario`, pero no existen puntos de extensión (patrones _Strategy_, _State_, interfaces) para las reglas de negocio variables.
3. **¿Existen estructuras condicionales crecientes?** Es probable al evaluar los valores de `EstadoEvento`.
4. **¿Favorece agregar comportamiento sin romper lo construido?** La estructura de herencia sí, pero las reglas internas de eventos no.

#### Impacto técnico potencial

- Modificaciones directas en clases centrales (`Evento`) ante cambios en políticas de inscripción.
- Riesgo de regresiones al alterar métodos existentes para dar cabida a nuevos comportamientos.

#### Recomendaciones

- Evaluar si la complejidad justifica aplicar el patrón _Strategy_ mediante una interfaz como `IPoliticaInscripcion`.
- Documentar claramente las reglas de transición de estados antes de agregar abstracciones innecesarias para el MVP.

---

### 4.3 L – Liskov Substitution Principle (LSP)

> **Definición:** Los objetos de una subclase deben poder sustituir a los de su clase base sin alterar el comportamiento esperado del sistema.

- **Elemento evaluado:** Relación de herencia entre `Usuario`, `Estudiante` y `Responsable`.
- **Evaluación:** **Cumple parcialmente**

#### Evidencia observada

- `Estudiante` y `Responsable` heredan de `Usuario` (atributos comunes: `Id`, `NombreCompleto`, `Email`, `Contrasena`, `Rol`, `Activo`).
- Métodos comunes heredados: `Autenticar()`, `GetRol()`, `ActualizarPerfil()`.
- Las subclases extienden la funcionalidad con métodos específicos de su rol.

#### Preguntas guía & Hallazgos

1. **¿Las subclases respetan el contrato de la superclase?** Sí, no se identifican redefiniciones que rompan el comportamiento base.
2. **¿Lanzan excepciones inesperadas o deshabilitan métodos?** No se observa en el diseño actual.
3. **¿Riesgos detectados?** El atributo `Rol` existe en la clase base y se asigna en los constructores de las subclases. Debe evitarse la inconsistencia entre el tipo concreto del objeto y el valor almacenado en `Rol`.

#### Impacto técnico potencial

- Si el valor de `Rol` diverge del tipo concreto, pueden fallar las comprobaciones de autorización.
- Depender explícitamente del subtipo concreto en capas superiores en lugar de la abstracción `Usuario`.

#### Recomendaciones

- Hacer que el tipo concreto sea la única fuente de verdad para el rol (o un método/propiedad de solo lectura en la clase base).
- Diseñar pruebas unitarias que garanticen que `Estudiante` y `Responsable` se comportan de manera idéntica al ser tratados como `Usuario` en operaciones comunes.

---

### 4.4 I – Interface Segregation Principle (ISP)

> **Definición:** Los clientes no deben depender de interfaces que no utilizan.

- **Elemento evaluado:** Separación de contratos y responsabilidades mediante interfaces específicas.
- **Evaluación:** **Cumple parcialmente**

#### Evidencia observada

- El modelo **no define interfaces** explícitas para las operaciones de negocio.
- Las operaciones están distribuidas directamente en clases concretas (`Estudiante.ConsultarInscripciones()`, `Responsable.ConsultarParticipantes()`).

#### Preguntas guía & Hallazgos

1. **¿Existen interfaces 'gordas' o monolíticas?** No existen interfaces actualmente, lo que impide comprobar una segregación formal.
2. **¿Se pueden dividir los contratos?** Las operaciones ya se perciben divididas lógicamente por rol (Estudiante vs. Responsable).

#### Impacto técnico potencial

- Al definir los futuros servicios de aplicación, existe el riesgo de crear interfaces únicas y masivas (`IEventosService`) que fuercen a los componentes a depender de métodos irrelevantes.
- Dificultad para realizar pruebas unitarias con mocks si no se usan interfaces segregadas.

#### Recomendaciones

- Crear contratos de servicios pequeños y orientados al cliente, como `IEventoQueryService`, `IInscripcionService`, `IAsistenciaService`.
- Evitar la tentación de crear una única interfaz para toda la gestión del sistema.

---

### 4.5 D – Dependency Inversion Principle (DIP)

> **Definición:** Los módulos de alto nivel no deben depender de módulos de bajo nivel; ambos deben depender de abstracciones.

- **Elemento evaluado:** Dependencias entre el modelo de dominio y los componentes de infraestructura/aplicación.
- **Evaluación:** **Cumple parcialmente**

#### Evidencia observada

- El diseño no especifica abstracciones de persistencia (repositorios) ni mecanismos de inyección de dependencias.
- Se observa una creación/asociación directa entre entidades (ej. `Responsable` gestionando/creando directamente instancias de `Asistencia`).

#### Preguntas guía & Hallazgos

1. **¿La lógica depende de implementaciones concretas?** El modelo de dominio define relaciones de entidad estándar, pero no explicita cómo se comunicará con la infraestructura o persistencia.
2. **¿Existen abstracciones para desacoplar?** Aún no han sido definidas en el diagrama de clases.

#### Impacto técnico potencial

- Acoplamiento directo entre la interfaz Blazor / lógica de aplicación y los mecanismos de almacenamiento.
- Alta dificultad para implementar pruebas unitarias sin una base de datos o almacenamiento real.

#### Recomendaciones

- Definir interfaces de repositorio (`IEventoRepository`, `IInscripcionRepository`).
- Utilizar el contenedor de Inyección de Dependencias nativo de .NET / Blazor para proveer las implementaciones de los servicios a los componentes UI.

---

## 5. Conclusión y Síntesis

### Estado General del Diseño

El modelo de dominio presenta una **estructura inicial coherente y funcional** para las necesidades del MVP. Muestra una clara identificación de entidades y una división básica de conceptos. Sin embargo, la acumulación de responsabilidades de coordinación dentro de las entidades y la ausencia de una capa de servicios e interfaces abstractas representan oportunidades de mejora clave antes de iniciar la codificación.

### Matriz de Evaluación

| Principio | Cumplimiento | Evidencia                                                                                  | Problema Detectado                                                             | Recomendación Principal                                                                                     |
| :-------- | :----------: | :----------------------------------------------------------------------------------------- | :----------------------------------------------------------------------------- | :---------------------------------------------------------------------------------------------------------- |
| **SRP**   | **Parcial**  | `Responsable` reúne eventos, participantes y asistencia. `Usuario` incluye `Autenticar()`. | Acumulación de responsabilidades y múltiples razones de cambio.                | Trasladar la coordinación a servicios C# (`GestionEventosService`, etc.) y autenticación a `AuthService`.   |
| **OCP**   | **Parcial**  | Herencia en `Usuario`; lógica de estados concentrada en `Evento`.                          | Incorporar reglas complejas requerirá modificar métodos en `Evento`.           | Identificar reglas variables y aislar políticas mediante interfaces o patrones de extensión si se requiere. |
| **LSP**   | **Parcial**  | `Estudiante` y `Responsable` heredan correctamente de `Usuario`.                           | Posibles inconsistencias entre el atributo `Rol` y el tipo de objeto concreto. | Unificar la fuente de verdad del rol y validar la sustitución mediante pruebas unitarias.                   |
| **ISP**   | **Parcial**  | No se definen interfaces en el diagrama del dominio.                                       | Falta de contratos explícitos que garanticen bajo acoplamiento con la UI.      | Definir interfaces pequeñas y centradas en el caso de uso (`IInscripcionService`, etc.).                    |
| **DIP**   | **Parcial**  | Ausencia de abstracciones para repositorios o persistencia.                                | Riesgo de acoplamiento directo entre UI/Dominio y la infraestructura.          | Definir abstracciones (`IRepository`) e integrar Inyección de Dependencias en Blazor.                       |

### Diagnóstico de Deuda Técnica

- **Nivel de Deuda Técnica:** **Medio (Potencial)**
- **Justificación:** Se trata de aspectos de diseño arquitectónico no resueltos previos a la implementación. Corregirlos en esta etapa no requiere refactorizar código existente y evitará sobrecostos de mantenimiento durante la fase de desarrollo.

---

## 6. Recomendación Final

1. **Reestructurar la capa de aplicación:** Extraer las operaciones de orquestación (registrar evento, inscribirse, tomar asistencia) de las clases de entidad (`Responsable`, `Estudiante`) hacia **servicios de aplicación C#**.
2. **Introducir Inyección de Dependencias:** Diseñar el flujo de datos apoyándose en interfaces de repositorio y servicios que se registrarán en la configuración de Blazor.
3. **Mantener la simplicidad del MVP:** No sobre-diseñar incluyendo patrones complejos salvo en los puntos donde la variabilidad del negocio lo exija claramente.
