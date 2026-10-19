# Plantilla de Análisis y Selección de Patrones GoF

> **Instrumento de apoyo para justificar patrones GoF no repetidos dentro del alcance de un producto mínimo viable**
> 
> **Uso sugerido:** Llenar primero el alcance del MVP, luego analizar necesidades de diseño reales y finalmente aprobar únicamente los patrones que aporten valor y no generen sobreingeniería.

---

## 1. Datos Generales del Proyecto

* **Nombre del proyecto:** Sistema de Gestión de Eventos Académicos
* **Equipo de trabajo:** Juan Pablo Gutierrez Vargas, Sergio Andrés Claro Rivero y Tomás Álvarez Castillo
* **Asignatura:** Diseño de Sistemas de Información
* **Docente:** Anibal Antonio Torres Cañas
* **Fecha:** 18 de octubre de 2026

---

## 2. Descripción General del Proyecto

* **Problema que resuelve el proyecto:** La información de los eventos académicos de la institución (conferencias, seminarios, talleres y charlas) está distribuida en diferentes medios y no existe un mecanismo centralizado para gestionarla. Esto dificulta que los estudiantes conozcan los eventos disponibles y gestionen sus inscripciones, y que los responsables controlen los inscritos y la asistencia.
* **Objetivo general del sistema:** Desarrollar un Sistema de Gestión de Eventos Académicos que centralice la información de los eventos, permita a los responsables registrarlos y administrarlos y a los estudiantes consultarlos e inscribirse, facilitando el seguimiento de participantes y asistencia.
* **Descripción del producto propuesto:** Aplicación web con dos roles de usuario. El estudiante consulta el listado de eventos y su detalle (fecha, hora, lugar, cupos disponibles y responsable) y gestiona sus inscripciones; el responsable registra y modifica eventos, consulta los participantes y registra la asistencia. El sistema aplica las reglas de negocio del proceso, como el control de cupos, una sola inscripción activa por evento y el cierre de inscripciones cuando el evento inicia.

---

## 3. Alcance del MVP

### 3.1 Funcionalidades incluidas en el MVP

| ID | Funcionalidad del MVP | Prioridad | Justificación |
| :---: | :--- | :---: | :--- |
| **F1** | Registrar evento (HU-07 / RF-07) | Alta | Es el inicio del flujo principal: sin eventos registrados por el responsable no hay nada que consultar ni a qué inscribirse. Aplica RN-03 y RN-04. |
| **F2** | Consultar eventos disponibles (HU-01 / RF-01) | Alta | Reúne en un solo lugar la oferta de eventos, atacando la causa principal del problema (información dispersa). |
| **F3** | Consultar información del evento (HU-02 / RF-02) | Alta | El estudiante necesita conocer fecha, hora, lugar, cupos y responsable para decidir si participa. |
| **F4** | Inscribirse a un evento (HU-04 / RF-04) | Alta | Cierra el flujo principal y es donde se aplican las reglas de cupos, duplicidad y estado del evento (RN-01, RN-02, RN-05, RN-08). |

### 3.2 Restricciones del MVP

* El proyecto debe implementar 3 patrones GoF.
* Los patrones no se pueden repetir.
* Cada patrón debe responder a una necesidad real del sistema.
* La selección debe evitar sobreingeniería.
* Los patrones deben ser viables dentro del tiempo y alcance del MVP.

### 3.3 Funcionalidades fuera del MVP

Siguiendo el Story Mapping, las funcionalidades de seguimiento se dejan para versiones posteriores, ya que en esta primera etapa pueden manejarse manualmente sin afectar la validación del flujo principal:

* Filtrar eventos por nombre, fecha o categoría (HU-03 / RF-03).
* Cancelar inscripción (HU-05 / RF-05).
* Consultar mis inscripciones y su estado (HU-06 / RF-06).
* Modificar evento (HU-08 / RF-08).
* Consultar eventos gestionados (HU-09 / RF-09).
* Consultar participantes de un evento (HU-10 / RF-10).
* Registrar y consultar asistencia (HU-11 y HU-12 / RF-11 y RF-12).
* Envío de notificaciones o recordatorios a los inscritos (no está contemplado en los requisitos actuales).

---

## 4. Matriz Global de Análisis de Patrones del Proyecto

*Esta matriz se diligencia antes de aprobar patrones, para identificar necesidades reales de diseño a nivel de proyecto.*

| Necesidad de diseño detectada | Módulo o funcionalidad afectada | Tipo de problema | Patrón candidato | ¿Aporta valor al MVP? | ¿Se solapa con otro patrón? |
| :--- | :--- | :---: | :---: | :---: | :---: |
| Crear eventos con muchos atributos (algunos opcionales) que deben cumplir RN-03 y RN-04 desde que existen | Registrar evento (F1) | Creacional | Builder | Sí | No |
| La interfaz debe coordinar varias clases (Estudiante, Evento, Inscripción) y validar varias reglas para inscribir | Inscribirse a un evento (F4) | Estructural | Facade | Sí | No |
| Lo que se puede hacer con un evento (mostrarse como disponible, aceptar inscripciones) depende de su estado | Ciclo de vida del evento (F2, F4) | Comportamiento | State | Sí | No |
| Varias formas de filtrar eventos (nombre, fecha, categoría) | Filtrar eventos (fuera del MVP) | Comportamiento | Strategy | No, el filtro es de la versión 2 | No |
| Avisar a los inscritos cuando un evento cambia o se cancela | Modificar evento (fuera del MVP) | Comportamiento | Observer | No, no es un requisito actual | No |
| Encadenar las validaciones de la inscripción (RN-01, RN-02, RN-05) | Inscribirse a un evento (F4) | Comportamiento | Chain of Responsibility | No, son pocas validaciones y fijas | Sí, con State y Facade |
| Crear el tipo de usuario correcto (Estudiante o Responsable) al autenticarse | Autenticación (transversal) | Creacional | Factory Method | No, solo hay dos tipos fijos | No |
| Restringir funciones según el rol del usuario (RNF-03) | Seguridad (transversal) | Estructural | Proxy | No, ya lo cubre el modelo de clases | Sí, con Facade |

---

## 5. Fichas de Análisis Individuales

### 5.1 Ficha de análisis del patrón: Builder

#### 5.1.1 Identificación general
* **Nombre del patrón:** Builder (Constructor)
* **Categoría:** `[X]` Creacional | `[ ]` Estructural | `[ ]` De comportamiento
* **Módulo, componente o funcionalidad donde se aplicará:** Registrar evento (F1 – HU-07 / RF-07): clase `EventoBuilder`, encargada de crear los objetos `Evento`.

#### 5.1.2 Problema de diseño que se quiere resolver
Un evento tiene muchos atributos: nombre, descripción, fecha y hora de inicio y de fin, lugar, cupos totales, categoría, responsable, estado y fecha de registro. Algunos son obligatorios y otros opcionales, y además el evento debe cumplir reglas de negocio desde el momento en que se crea (RN-03: la fecha y hora de inicio deben ser posteriores a las de registro; RN-04: los cupos deben ser mayores que cero). Crearlo con un constructor de muchos parámetros es propenso a errores, por ejemplo intercambiar la fecha de inicio y la de fin porque son del mismo tipo, y si las validaciones se hacen después de crear el objeto pueden quedar eventos incompletos o inválidos en el sistema.

#### 5.1.3 Evidencias del problema en el proyecto
* `[ ]` Alto acoplamiento
* `[ ]` Multiplicidad de algoritmos
* `[ ]` Cambio frecuente de comportamiento
* `[X]` Complejidad en la creación de objetos
* `[ ]` Necesidad de desacoplar emisor y receptor
* `[ ]` Necesidad de ampliar funcionalidad sin modificar la clase base
* `[ ]` Interfaz incompatible
* `[X]` Otro: Garantizar que el objeto nazca válido (RN-03, RN-04)

**Explicación contextualizada en el proyecto:**  
El formulario del responsable recoge los datos del evento en varios campos. Con un constructor del tipo `Evento(nombre, descripcion, inicio, fin, lugar, cupos, categoria, responsable)` el código es difícil de leer y fácil de equivocar. Con `EventoBuilder` cada dato se asigna con un método con nombre (`conNombre()`, `conFechas()`, `conLugar()`, `conCupos()`, `conCategoria()`) y el método `build()` aplica las validaciones de RN-03 y RN-04 (`validarFechaInicio` y `validarCupos`), asigna la fecha de registro y el estado inicial. Si algún dato no cumple, el evento no se crea.

#### 5.1.4 Relación con el MVP
* **¿Por qué este problema sí debe resolverse en el MVP y no en una versión futura?** Registrar evento es la primera funcionalidad del flujo principal (F1): las demás (listar, ver detalle e inscribirse) trabajan sobre los eventos creados aquí. Si desde el inicio se permiten eventos inválidos, el error se propaga a la consulta y a la inscripción.
* **¿Qué riesgo habría si no se aplica este patrón en el MVP?** Que se guarden eventos con fecha pasada o con cupos en cero o negativos, que las validaciones se repitan en la pantalla y en el controlador, y que el constructor sea cada vez más difícil de mantener al agregar campos.
* **¿La implementación del patrón es razonable para el tiempo y complejidad del MVP?** `[X]` Sí | `[ ]` No
* **Justificación:** Es una sola clase (`EventoBuilder`) con métodos sencillos y un `build()`; no necesita la clase Director ni jerarquías adicionales. Se implementa en poco tiempo y se podrá reutilizar en Modificar evento (versión 2).

#### 5.1.5 Análisis de adecuación del patrón
* **¿Qué parte del sistema cambia o podría cambiar?** Los datos que se piden para crear un evento (podrían agregarse campos opcionales) y las validaciones que deben cumplirse al crearlo.
* **¿Qué se desea desacoplar?** La forma de construir y validar un evento, de la clase Evento y del formulario o controlador que recibe los datos.
* **¿Qué ventaja concreta ofrece este patrón en este proyecto?** Todo evento que existe en el sistema cumple RN-03 y RN-04, el código de creación es legible y los campos opcionales no obligan a pasar valores nulos.
* **¿Qué principio de diseño ayuda a cumplir?**  
  * `[ ]` Abierto/Cerrado  
  * `[X]` Responsabilidad Única  
  * `[ ]` Inversión de Dependencias  
  * `[ ]` Segregación de Interfaces  
  * `[ ]` Otro  
* **Justificación técnica:** Evento se encarga de su comportamiento (cupos e inscripciones) y EventoBuilder solo de ensamblarlo y validarlo antes de crearlo. El controlador no necesita conocer el orden de los parámetros ni las reglas de creación.

#### 5.1.6 Conclusión de decisión
* **Decisión final sobre este patrón:** `[X]` Se aprueba | `[ ]` Se descarta
* **Justificación final:** Resuelve un problema real de la funcionalidad F1, tiene bajo costo de implementación y asegura desde el diseño el cumplimiento de RN-03 y RN-04.

---

### 5.2 Ficha de análisis del patrón: Facade

#### 5.2.1 Identificación general
* **Nombre del patrón:** Facade (Fachada)
* **Categoría:** `[ ]` Creacional | `[X]` Estructural | `[ ]` De comportamiento
* **Módulo, componente o funcionalidad donde se aplicará:** Inscribirse a un evento (F4 – HU-04 / RF-04): clase `InscripcionFacade`, punto de entrada entre la interfaz y las clases `Estudiante`, `Evento` e `Inscripción`.

#### 5.2.2 Problema de diseño que se quiere resolver
Inscribir a un estudiante no es una sola operación. Hay que buscar al estudiante y al evento, verificar que el evento admita inscripciones (RN-05), que tenga cupos (RN-01, RN-08) y que el estudiante no tenga ya una inscripción activa en él (RN-02); luego crear la inscripción con su fecha y estado ACTIVA, guardarla y actualizar los cupos del evento. Si la interfaz hace todo esto directamente, queda acoplada a muchas clases del dominio y las reglas de negocio terminan mezcladas con la pantalla.

#### 5.2.3 Evidencias del problema en el proyecto
* `[X]` Alto acoplamiento
* `[ ]` Multiplicidad de algoritmos
* `[ ]` Cambio frecuente de comportamiento
* `[ ]` Complejidad en la creación de objetos
* `[ ]` Necesidad de desacoplar emisor y receptor
* `[ ]` Necesidad de ampliar funcionalidad sin modificar la clase base
* `[ ]` Interfaz incompatible
* `[X]` Otro: Proceso de varios pasos que involucra varias clases

**Explicación contextualizada en el proyecto:**  
En el diagrama de clases la inscripción involucra a Estudiante (`inscribirse()`, `estaInscritoEn()`), a Evento (`admiteInscripciones()`, `tieneCuposDisponibles()`, `agregarInscripcion()`) y a Inscripcion. Sin una fachada, la pantalla de detalle del evento tendría que llamar a todos estos métodos en el orden correcto. Con `InscripcionFacade` la interfaz solo invoca `inscribir(idEstudiante, idEvento)` y recibe la inscripción creada o el motivo por el que no fue posible. Así se mantiene la decisión del equipo de que las reglas son del evento y no de la pantalla.

#### 5.2.4 Relación con el MVP
* **¿Por qué este problema sí debe resolverse en el MVP y no en una versión futura?** La inscripción cierra el flujo principal del MVP y es la funcionalidad que aplica más reglas de negocio a la vez (RN-01, RN-02, RN-05, RN-08); es el punto donde más fácil se mezcla la lógica de negocio con la interfaz.
* **¿Qué riesgo habría si no se aplica este patrón en el MVP?** Lógica duplicada o inconsistente si más de una pantalla permite inscribirse, reglas que se saltan por llamar los métodos en un orden incorrecto (por ejemplo, crear la inscripción antes de verificar los cupos) y una interfaz difícil de cambiar.
* **¿La implementación del patrón es razonable para el tiempo y complejidad del MVP?** `[X]` Sí | `[ ]` No
* **Justificación:** Es una clase con pocos métodos que delega en clases que ya existen en el diagrama; no agrega jerarquías. En la versión 2 puede crecer de forma natural con `cancelarInscripcion()`.

#### 5.2.5 Análisis de adecuación del patrón
* **¿Qué parte del sistema cambia o podría cambiar?** La interfaz (el prototipo HTML puede cambiar de diseño o de tecnología) y la cantidad u orden de las validaciones del proceso de inscripción.
* **¿Qué se desea desacoplar?** La capa de presentación de las clases del dominio (Estudiante, Evento, Inscripción) y de la persistencia.
* **¿Qué ventaja concreta ofrece este patrón en este proyecto?** Un único punto de entrada para inscribirse, que garantiza que todas las reglas se verifiquen siempre y en el mismo orden, sin que la interfaz dependa de la estructura interna del dominio.
* **¿Qué principio de diseño ayuda a cumplir?**  
  * `[ ]` Abierto/Cerrado  
  * `[X]` Responsabilidad Única  
  * `[ ]` Inversión de Dependencias  
  * `[ ]` Segregación de Interfaces  
  * `[X]` Otro: Mínimo conocimiento (Ley de Deméter)  
* **Justificación técnica:** La interfaz solo conoce a la fachada y no a los objetos internos. La fachada tiene una sola responsabilidad, coordinar el caso de uso, y cada clase del dominio conserva su propia lógica (Evento valida los cupos, Inscripción maneja su estado).

#### 5.2.6 Conclusión de decisión
* **Decisión final sobre este patrón:** `[X]` Se aprueba | `[ ]` Se descarta
* **Justificación final:** Simplifica el caso de uso más importante del MVP, protege las reglas de negocio de la capa de presentación y su costo de implementación es bajo.

---

### 5.3 Ficha de análisis del patrón: State

#### 5.3.1 Identificación general
* **Nombre del patrón:** State (Estado)
* **Categoría:** `[ ]` Creacional | `[ ]` Estructural | `[X]` De comportamiento
* **Módulo, componente o funcionalidad donde se aplicará:** Ciclo de vida del Evento (atributo estado: `EstadoEvento`). Afecta a Consultar eventos disponibles (F2) e Inscribirse a un evento (F4).

#### 5.3.2 Problema de diseño que se quiere resolver
Un evento pasa por varios estados (Programado, Inscripciones abiertas, Inscripciones cerradas, En curso, Finalizado y Cancelado) y lo que se puede hacer con él depende del estado en que se encuentre: solo acepta inscripciones mientras están abiertas (RN-05), deja de recibirlas cuando se agotan los cupos (RN-08) o cuando inicia, y solo algunos estados deben mostrarse a los estudiantes como disponibles. Si esto se controla con condicionales sobre la enumeración, cada método de Evento termina con un switch o una cadena de if que revisa el estado, y los cambios de estado quedan dispersos.

#### 5.3.3 Evidencias del problema en el proyecto
* `[ ]` Alto acoplamiento
* `[ ]` Multiplicidad de algoritmos
* `[X]` Cambio frecuente de comportamiento
* `[ ]` Complejidad en la creación de objetos
* `[ ]` Necesidad de desacoplar emisor y receptor
* `[ ]` Necesidad de ampliar funcionalidad sin modificar la clase base
* `[ ]` Interfaz incompatible
* `[X]` Otro: Condicionales repetidos según el estado del evento

**Explicación contextualizada en el proyecto:**  
En el diagrama de clases Evento tiene el atributo estado con seis valores y métodos como `admiteInscripciones()` (RN-05, RN-07), `tieneCuposDisponibles()` y `actualizarEstado()`, y cada uno tendría que preguntar en qué estado está el evento. Con State cada estado es una clase (`InscripcionesAbiertas`, `InscripcionesCerradas`, `EnCurso`, etc.) que implementa la interfaz `EstadoEvento` con métodos como `inscribir()`, `estaDisponible()` y `siguienteEstado()`; Evento solo delega en su estado actual. Por ejemplo, cuando se llenan los cupos, `InscripcionesAbiertas` cambia el evento a `InscripcionesCerradas`.

#### 5.3.4 Relación con el MVP
* **¿Por qué este problema sí debe resolverse en el MVP y no en una versión futura?** Desde el MVP el sistema debe decidir si un evento aparece como disponible (F2) y si acepta una inscripción (F4); RN-05 y RN-08 hacen parte del flujo principal. Además, el estado ya existe en el diagrama de clases, por lo que es mejor definir bien su comportamiento desde el inicio que reescribirlo cuando lleguen cancelar inscripción y asistencia.
* **¿Qué riesgo habría si no se aplica este patrón en el MVP?** Aceptar inscripciones en eventos cerrados, cancelados o ya iniciados; condicionales duplicados en varios métodos; y que cada estado o regla nueva (por ejemplo RN-07, o registrar asistencia solo en eventos en curso o finalizados) obligue a modificar muchos métodos de Evento.
* **¿La implementación del patrón es razonable para el tiempo y complejidad del MVP?** `[X]` Sí | `[ ]` No
* **Justificación:** Los estados ya están definidos en el modelo, así que no se agrega complejidad nueva. En el MVP cada clase de estado solo implementa unos pocos métodos cortos (`inscribir`, `estaDisponible` y `siguienteEstado`).

#### 5.3.5 Análisis de adecuación del patrón
* **¿Qué parte del sistema cambia o podría cambiar?** Las reglas de lo que se puede hacer con un evento en cada etapa de su ciclo de vida. En próximas versiones se agregan acciones que dependen del estado: cancelar inscripción solo con inscripciones abiertas (RN-07) y registrar asistencia solo en eventos en curso o finalizados.
* **¿Qué se desea desacoplar?** El comportamiento propio de cada estado y las reglas de transición entre estados, de la clase Evento.
* **¿Qué ventaja concreta ofrece este patrón en este proyecto?** Elimina los condicionales sobre el estado, hace explícitas las transiciones y garantiza que un evento nunca acepte una acción que su estado no permite.
* **¿Qué principio de diseño ayuda a cumplir?**  
  * `[X]` Abierto/Cerrado  
  * `[X]` Responsabilidad Única  
  * `[ ]` Inversión de Dependencias  
  * `[ ]` Segregación de Interfaces  
  * `[ ]` Otro  
* **Justificación técnica:** Agregar un estado o cambiar el comportamiento de uno existente se hace creando o modificando solo su clase, sin tocar Evento ni los demás estados. Cada clase de estado tiene una sola responsabilidad: definir cómo se comporta el evento en ese estado.

#### 5.3.6 Conclusión de decisión
* **Decisión final sobre este patrón:** `[X]` Se aprueba | `[ ]` Se descarta
* **Justificación final:** El comportamiento del evento depende claramente de su estado y este es el núcleo de las reglas de inscripción del MVP. Es el patrón de comportamiento que mejor se ajusta; Strategy y Observer se descartaron por estar fuera del alcance del MVP.

---

## 6. Consolidado Final de los Patrones Seleccionados

| Nº | Patrón GoF | Categoría | Módulo/Funcionalidad | Problema que resuelve | Justificación en el MVP | ¿Se repite? |
| :---: | :---: | :---: | :--- | :--- | :--- | :---: |
| **1** | **Builder** | Creacional | Registrar evento (F1) | Creación de eventos con muchos atributos que deben cumplir RN-03 y RN-04 | Asegura que solo se registren eventos válidos desde la primera funcionalidad del flujo | No |
| **2** | **Facade** | Estructural | Inscribirse a un evento (F4) | Interfaz acoplada a varias clases y reglas para inscribir | Un solo punto de entrada para el caso de uso principal, que aplica RN-01, RN-02, RN-05 y RN-08 | No |
| **3** | **State** | Comportamiento | Ciclo de vida del evento (F2, F4) | Comportamiento del evento que depende de su estado | Controla si el evento aparece como disponible y si acepta inscripciones, sin condicionales dispersos | No |

---

## 7. Conclusión General del Análisis

Los 3 patrones seleccionados (**Builder**, **Facade** y **State**) responden a necesidades reales del flujo principal del MVP: el responsable registra el evento y el estudiante lo consulta y se inscribe. Además, no se repiten y cubren las tres categorías GoF: creacional, estructural y de comportamiento.

* **Builder** asegura que los eventos se creen válidos (RN-03 y RN-04).
* **State** controla qué se puede hacer con un evento según su estado y decide si aparece como disponible (RN-05 y RN-08).
* **Facade** le da a la interfaz un único punto de entrada para la inscripción, coordinando a Estudiante, Evento e Inscripción.

Los tres se complementan sin solaparse: **Builder** crea el evento en su estado inicial, **State** define su comportamiento durante su ciclo de vida y **Facade** orquesta el caso de uso sin conocer los detalles de cada estado.

Para evitar sobreingeniería se descartaron **Strategy** (los filtros están fuera del MVP), **Observer** (las notificaciones no son un requisito), **Chain of Responsibility** (se solapa con State y Facade), **Factory Method** (solo hay dos tipos fijos de usuario) y **Proxy** (el control por rol ya lo cubren el modelo de clases y la fachada). Los tres patrones aprobados son viables en el tiempo del MVP y dejan el diseño preparado para las siguientes versiones (cancelar inscripción, modificar evento y asistencia).

**Como consecuencia, el diagrama de clases debe actualizarse para incluir `EventoBuilder`, `InscripcionFacade` y la interfaz `EstadoEvento` con sus estados concretos, en lugar de la enumeración actual.**