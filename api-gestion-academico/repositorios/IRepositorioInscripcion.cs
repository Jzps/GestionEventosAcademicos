using ApiGestionAcademico.Modelos;

namespace ApiGestionAcademico.Repositorios;

/// <summary>Contrato de acceso a las inscripciones.</summary>
public interface IRepositorioInscripcion
{
    /// <summary>Guarda una inscripción nueva.</summary>
    Task GuardarAsync(Inscripcion inscripcion, CancellationToken cancellationToken);

    /// <summary>Actualiza una inscripción existente.</summary>
    Task ActualizarAsync(Inscripcion inscripcion, CancellationToken cancellationToken);

    /// <summary>Obtiene una inscripción por su identificador, con estudiante y evento.</summary>
    Task<Inscripcion?> ObtenerPorIdAsync(long id, CancellationToken cancellationToken);

    /// <summary>Obtiene la inscripción activa de un estudiante en un evento.</summary>
    Task<Inscripcion?> ObtenerActivaAsync(long estudianteId, long eventoId, CancellationToken cancellationToken);

    /// <summary>Lista las inscripciones de un estudiante.</summary>
    Task<List<Inscripcion>> ListarPorEstudianteAsync(long estudianteId, CancellationToken cancellationToken);

    /// <summary>Guarda los cambios de varias inscripciones en una sola operación.</summary>
    Task GuardarTodasAsync(IEnumerable<Inscripcion> inscripciones, CancellationToken cancellationToken);

    /// <summary>Lista las inscripciones de un evento, con la información del estudiante.</summary>
    Task<List<Inscripcion>> ListarPorEventoAsync(long eventoId, CancellationToken cancellationToken);
}
