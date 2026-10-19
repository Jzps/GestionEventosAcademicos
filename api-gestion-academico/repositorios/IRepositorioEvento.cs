using ApiGestionAcademico.Modelos;

namespace ApiGestionAcademico.Repositorios;

/// <summary>
/// Contrato de acceso a los eventos. Interfaz segregated (ISP): expone únicamente las
/// operaciones que el sistema necesita, sin incluir las de otros recursos.
/// </summary>
public interface IRepositorioEvento
{
    /// <summary>Lista los eventos que satisfacen los criterios de filtrado.</summary>
    Task<List<Evento>> ListarAsync(string? nombre, DateTime? fecha, long? categoriaId, CancellationToken cancellationToken);

    /// <summary>Obtiene un evento por su identificador, con responsable, categoría e inscripciones.</summary>
    Task<Evento?> ObtenerPorIdAsync(long id, CancellationToken cancellationToken);

    /// <summary>Obtiene los eventos administrados por un responsable.</summary>
    Task<List<Evento>> ListarGestionadosAsync(long responsableId, CancellationToken cancellationToken);

    /// <summary>Cuenta las inscripciones activas de un evento.</summary>
    Task<int> ContarInscripcionesActivasAsync(long eventoId, CancellationToken cancellationToken);

    /// <summary>Persiste un evento nuevo.</summary>
    Task GuardarAsync(Evento evento, CancellationToken cancellationToken);

    /// <summary>Persiste los cambios de un evento existente.</summary>
    Task ActualizarAsync(Evento evento, CancellationToken cancellationToken);
}
