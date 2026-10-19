using ApiGestionAcademico.Excepciones;

namespace ApiGestionAcademico.Modelos.Estados;

/// <summary>
/// Estado en el que el evento ya no recibe inscripciones, por agotamiento de cupos (RN-08)
/// o porque el evento comenzó (RN-05). Según RN-07, en este estado el estudiante
/// tampoco puede cancelar su inscripción.
/// </summary>
public sealed class EstadoInscripcionesCerradas : IEventoEstado
{
    public EstadoEvento Estado => EstadoEvento.InscripcionesCerradas;

    public string Etiqueta => "Inscripciones cerradas";

    public bool EstaDisponible => true;

    public bool AdmiteInscripciones => false;

    /// <summary>
    /// True: el cierre de inscripciones por cupos agotados no impide cancelar. La
    /// condición definitiva la evalúa <see cref="Evento.PuedeCancelarInscripcion"/>, que
    /// exige además que el evento todavía no haya comenzado (RN-07).
    /// </summary>
    public bool PermiteCancelarInscripcion => true;

    public bool PermiteRegistrarAsistencia => true;

    public string MotivoBloqueoInscripcion => "Las inscripciones para este evento están cerradas.";

    public void Inscribir(Evento evento, Estudiante estudiante)
        => throw new ExcepcionNegocio("Las inscripciones para este evento están cerradas.");

    public IEventoEstado Transicionar(TransicionEvento transicion) => transicion switch
    {
        TransicionEvento.ReabrirInscripciones => new EstadoInscripcionesAbiertas(),
        TransicionEvento.Iniciar => new EstadoEnCurso(),
        TransicionEvento.Cancelar => new EstadoCancelado(),
        _ => throw new ExcepcionNegocio($"La transición '{transicion}' no es válida desde el estado '{Etiqueta}'.")
    };

    public TransicionEvento? TransicionAutomatica(Evento evento, DateTime ahora, int cuposDisponibles)
    {
        if (ahora >= evento.FechaHoraInicio)
        {
            return TransicionEvento.Iniciar;
        }

        // Si se liberó un cupo (por ejemplo, tras una cancelación) y el evento todavía no
        // ha comenzado, las inscripciones se reabren.
        if (cuposDisponibles > 0)
        {
            return TransicionEvento.ReabrirInscripciones;
        }

        return null;
    }
}
