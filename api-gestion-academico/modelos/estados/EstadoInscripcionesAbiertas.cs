using ApiGestionAcademico.Excepciones;

namespace ApiGestionAcademico.Modelos.Estados;

/// <summary>
/// Estado en el que el evento acepta inscripciones. Aplica RN-01, RN-02, RN-05 y RN-08:
/// solo se inscribe si el evento tiene cupos y el estudiante no tiene una inscripción activa.
/// </summary>
public sealed class EstadoInscripcionesAbiertas : IEventoEstado
{
    public EstadoEvento Estado => EstadoEvento.InscripcionesAbiertas;

    public string Etiqueta => "Inscripciones abiertas";

    public bool EstaDisponible => true;

    public bool AdmiteInscripciones => true;

    public bool PermiteCancelarInscripcion => true;

    public bool PermiteRegistrarAsistencia => true;

    public string MotivoBloqueoInscripcion => "No hay cupos disponibles.";

    public void Inscribir(Evento evento, Estudiante estudiante)
    {
        if (evento.EstaInscrito(estudiante))
        {
            throw new ExcepcionNegocio("Ya te encuentras inscrito en este evento.");
        }

        if (!evento.TieneCuposDisponibles())
        {
            throw new ExcepcionNegocio("No hay cupos disponibles para este evento.");
        }

        evento.AgregarInscripcion(new Inscripcion(evento, estudiante, DateTime.Now));
    }

    public IEventoEstado Transicionar(TransicionEvento transicion) => transicion switch
    {
        TransicionEvento.CerrarInscripciones => new EstadoInscripcionesCerradas(),
        TransicionEvento.Iniciar => new EstadoEnCurso(),
        TransicionEvento.Cancelar => new EstadoCancelado(),
        _ => throw new ExcepcionNegocio($"La transición '{transicion}' no es válida desde el estado '{Etiqueta}'.")
    };
    public TransicionEvento? TransicionAutomatica(Evento evento, DateTime ahora, int cuposDisponibles)
    {
        if (ahora >= evento.FechaHoraInicio)
        {
            return TransicionEvento.CerrarInscripciones;
        }

        if (cuposDisponibles <= 0)
        {
            return TransicionEvento.CerrarInscripciones;
        }

        return null;
    }
}
