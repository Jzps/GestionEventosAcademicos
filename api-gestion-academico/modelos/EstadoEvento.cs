namespace ApiGestionAcademico.Modelos;

/// <summary>
/// Identificador persistido del estado del evento. El comportamiento de cada estado
/// vive en las clases que implementan <see cref="ApiGestionAcademico.Modelos.Estados.IEventoEstado"/>
/// (patrón State).
/// </summary>
public enum EstadoEvento
{
    Programado = 0,
    InscripcionesAbiertas = 1,
    InscripcionesCerradas = 2,
    EnCurso = 3,
    Finalizado = 4,
    Cancelado = 5
}
