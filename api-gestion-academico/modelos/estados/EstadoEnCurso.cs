using ApiGestionAcademico.Excepciones;

namespace ApiGestionAcademico.Modelos.Estados;

/// <summary>El evento está en ejecución. No admite inscripciones (RN-05).</summary>
public sealed class EstadoEnCurso : IEventoEstado
{
    public EstadoEvento Estado => EstadoEvento.EnCurso;

    public string Etiqueta => "En curso";

    public bool EstaDisponible => true;

    public bool AdmiteInscripciones => false;

    public bool PermiteCancelarInscripcion => false;

    public bool PermiteRegistrarAsistencia => true;

    public string MotivoBloqueoInscripcion => "El evento ya se encuentra en curso.";

    public void Inscribir(Evento evento, Estudiante estudiante)
        => throw new ExcepcionNegocio("El evento ya se encuentra en curso y no admite nuevas inscripciones.");

    public IEventoEstado Transicionar(TransicionEvento transicion) => transicion switch
    {
        TransicionEvento.Finalizar => new EstadoFinalizado(),
        TransicionEvento.Cancelar => new EstadoCancelado(),
        _ => throw new ExcepcionNegocio($"La transición '{transicion}' no es válida desde el estado '{Etiqueta}'.")
    };

    public TransicionEvento? TransicionAutomatica(Evento evento, DateTime ahora, int cuposDisponibles)
        => ahora >= evento.FechaHoraFin ? TransicionEvento.Finalizar : null;
}
