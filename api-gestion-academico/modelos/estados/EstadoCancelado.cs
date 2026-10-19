using ApiGestionAcademico.Excepciones;

namespace ApiGestionAcademico.Modelos.Estados;

/// <summary>
/// El evento fue cancelado. No se muestra en el listado de eventos del estudiante y
/// no admite ninguna operación de participación.
/// </summary>
public sealed class EstadoCancelado : IEventoEstado
{
    public EstadoEvento Estado => EstadoEvento.Cancelado;

    public string Etiqueta => "Cancelado";

    public bool EstaDisponible => false;

    public bool AdmiteInscripciones => false;

    public bool PermiteCancelarInscripcion => false;

    public bool PermiteRegistrarAsistencia => false;

    public string MotivoBloqueoInscripcion => "El evento fue cancelado.";

    public void Inscribir(Evento evento, Estudiante estudiante)
        => throw new ExcepcionNegocio("El evento fue cancelado.");

    public IEventoEstado Transicionar(TransicionEvento transicion)
        => throw new ExcepcionNegocio($"La transición '{transicion}' no es válida desde el estado '{Etiqueta}'.");

    public TransicionEvento? TransicionAutomatica(Evento evento, DateTime ahora, int cuposDisponibles) => null;
}
