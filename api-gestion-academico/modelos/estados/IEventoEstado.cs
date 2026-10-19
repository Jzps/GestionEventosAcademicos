using ApiGestionAcademico.Excepciones;

namespace ApiGestionAcademico.Modelos.Estados;

/// <summary>Transiciones posibles dentro del ciclo de vida de un evento.</summary>
public enum TransicionEvento
{
    /// <summary>Apertura de inscripciones (Programado → InscripcionesAbiertas).</summary>
    AbrirInscripciones = 1,

    /// <summary>
    /// Reapertura de inscripciones cuando se libera un cupo
    /// (InscripcionesCerradas → InscripcionesAbiertas).
    /// </summary>
    ReabrirInscripciones = 6,

    /// <summary>Cierre de inscripciones por falta de cupos o por inicio del evento.</summary>
    CerrarInscripciones = 2,

    /// <summary>Inicio del evento (→ EnCurso).</summary>
    Iniciar = 3,

    /// <summary>Finalización del evento (→ Finalizado).</summary>
    Finalizar = 4,

    /// <summary>Cancelación del evento (→ Cancelado).</summary>
    Cancelar = 5
}

/// <summary>
/// Abstracción del patrón State. Cada estado concreto define el comportamiento del evento
/// (si aparece en el listado, si admite inscripciones, si permite cancelar y si admite
/// registrar asistencia) y las transiciones hacia las que puede moverse.
/// </summary>
public interface IEventoEstado
{
    /// <summary>Identificador persistido del estado.</summary>
    EstadoEvento Estado { get; }

    /// <summary>Etiqueta legible del estado, usada en la interfaz.</summary>
    string Etiqueta { get; }

    /// <summary>True cuando el evento debe mostrarse en el listado de eventos del estudiante.</summary>
    bool EstaDisponible { get; }

    /// <summary>True cuando el evento admite nuevas inscripciones (RN-05).</summary>
    bool AdmiteInscripciones { get; }

    /// <summary>True cuando el estudiante puede cancelar su inscripción (RN-07).</summary>
    bool PermiteCancelarInscripcion { get; }

    /// <summary>True cuando el responsable puede registrar asistencia (RN-06 se valida aparte).</summary>
    bool PermiteRegistrarAsistencia { get; }

    /// <summary>Motivo legible por el que no se admiten inscripciones en este estado.</summary>
    string MotivoBloqueoInscripcion { get; }

    /// <summary>
    /// Ejecuta la inscripción validando las reglas del estado. Lanza <see cref="ExcepcionNegocio"/>
    /// cuando la operación no está permitida.
    /// </summary>
    void Inscribir(Evento evento, Estudiante estudiante);

    /// <summary>Devuelve el estado destino de la transición solicitada.</summary>
    IEventoEstado Transicionar(TransicionEvento transicion);

    /// <summary>
    /// Indica si el estado exige una transición automática por tiempo o por disponibilidad de cupos.
    /// Devuelve null cuando el estado ya es estable.
    /// </summary>
    TransicionEvento? TransicionAutomatica(Evento evento, DateTime ahora, int cuposDisponibles);
}
