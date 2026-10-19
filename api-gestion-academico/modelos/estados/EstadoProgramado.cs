using ApiGestionAcademico.Excepciones;

namespace ApiGestionAcademico.Modelos.Estados;

/// <summary>
/// Estado inicial de todo evento recién construido. El MVP no contempla una acción
/// administrativa de apertura de inscripciones, por lo que el dominio promociona
/// automáticamente este estado a <see cref="EstadoInscripcionesAbiertas"/> en cuanto el evento
/// es consultado y su fecha de inicio todavía no ha llegado.
/// </summary>
public sealed class EstadoProgramado : IEventoEstado
{
    public EstadoEvento Estado => EstadoEvento.Programado;

    public string Etiqueta => "Programado";

    public bool EstaDisponible => true;

    public bool AdmiteInscripciones => false;

    public bool PermiteCancelarInscripcion => false;

    public bool PermiteRegistrarAsistencia => true;

    public string MotivoBloqueoInscripcion => "El evento se encuentra en estado programado.";

    public void Inscribir(Evento evento, Estudiante estudiante)
        => throw new ExcepcionNegocio("El evento aún no tiene abiertas las inscripciones.");

    public IEventoEstado Transicionar(TransicionEvento transicion) => transicion switch
    {
        TransicionEvento.AbrirInscripciones => new EstadoInscripcionesAbiertas(),
        TransicionEvento.Iniciar => new EstadoEnCurso(),
        TransicionEvento.Cancelar => new EstadoCancelado(),
        _ => throw new ExcepcionNegocio($"La transición '{transicion}' no es válida desde el estado '{Etiqueta}'.")
    };

    public TransicionEvento? TransicionAutomatica(Evento evento, DateTime ahora, int cuposDisponibles)
        => ahora < evento.FechaHoraInicio ? TransicionEvento.AbrirInscripciones : TransicionEvento.Iniciar;
}
