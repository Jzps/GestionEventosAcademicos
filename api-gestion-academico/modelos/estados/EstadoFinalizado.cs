using ApiGestionAcademico.Excepciones;

namespace ApiGestionAcademico.Modelos.Estados;

/// <summary>El evento terminó. Se conserva para consultar participantes y asistencia.</summary>
public sealed class EstadoFinalizado : IEventoEstado
{
    public EstadoEvento Estado => EstadoEvento.Finalizado;

    public string Etiqueta => "Finalizado";

    public bool EstaDisponible => true;

    public bool AdmiteInscripciones => false;

    public bool PermiteCancelarInscripcion => false;

    public bool PermiteRegistrarAsistencia => true;

    public string MotivoBloqueoInscripcion => "El evento ya finalizado.";

    public void Inscribir(Evento evento, Estudiante estudiante)
        => throw new ExcepcionNegocio("El evento ya finalizó y no admite nuevas inscripciones.");

    public IEventoEstado Transicionar(TransicionEvento transicion) => transicion switch
    {
        TransicionEvento.Cancelar => new EstadoCancelado(),
        _ => throw new ExcepcionNegocio($"La transición '{transicion}' no es válida desde el estado '{Etiqueta}'.")
    };

    public TransicionEvento? TransicionAutomatica(Evento evento, DateTime ahora, int cuposDisponibles) => null;
}
