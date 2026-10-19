using ApiGestionAcademico.Modelos;

namespace ApiGestionAcademico.Repositorios;

/// <summary>Contrato de acceso a los registros de asistencia.</summary>
public interface IRepositorioAsistencia
{
    /// <summary>Guarda un registro de asistencia nuevo.</summary>
    Task GuardarAsync(Asistencia asistencia, CancellationToken cancellationToken);

    /// <summary>Actualiza un registro de asistencia existente.</summary>
    Task ActualizarAsync(Asistencia asistencia, CancellationToken cancellationToken);

    /// <summary>Obtiene el registro de asistencia de una inscripción.</summary>
    Task<Asistencia?> ObtenerPorInscripcionAsync(long inscripcionId, CancellationToken cancellationToken);

    /// <summary>Lista los registros de asistencia de un evento.</summary>
    Task<List<Asistencia>> ListarPorEventoAsync(long eventoId, CancellationToken cancellationToken);
}
