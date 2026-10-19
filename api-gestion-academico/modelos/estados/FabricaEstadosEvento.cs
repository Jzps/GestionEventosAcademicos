namespace ApiGestionAcademico.Modelos.Estados;

/// <summary>
/// Fábrica que reconstruye el objeto de estado a partir del identificador persistido
/// en SQL Server. Permite que el comportamiento del patrón State sobreviva a la
/// serialización del evento en la base de datos.
/// </summary>
public static class FabricaEstadosEvento
{
    /// <summary>Devuelve la implementación de <see cref="IEventoEstado"/> correspondiente al identificador.</summary>
    public static IEventoEstado Crear(EstadoEvento estado) => estado switch
    {
        EstadoEvento.Programado => new EstadoProgramado(),
        EstadoEvento.InscripcionesAbiertas => new EstadoInscripcionesAbiertas(),
        EstadoEvento.InscripcionesCerradas => new EstadoInscripcionesCerradas(),
        EstadoEvento.EnCurso => new EstadoEnCurso(),
        EstadoEvento.Finalizado => new EstadoFinalizado(),
        EstadoEvento.Cancelado => new EstadoCancelado(),
        _ => throw new ArgumentOutOfRangeException(nameof(estado), estado, "Estado de evento no reconocido.")
    };
}
