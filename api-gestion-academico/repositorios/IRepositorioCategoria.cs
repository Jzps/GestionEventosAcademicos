using ApiGestionAcademico.Modelos;

namespace ApiGestionAcademico.Repositorios;

/// <summary>Contrato de acceso a las categorías de eventos.</summary>
public interface IRepositorioCategoria
{
    /// <summary>Lista todas las categorías ordenadas por nombre.</summary>
    Task<List<Categoria>> ListarAsync(CancellationToken cancellationToken);

    /// <summary>Obtiene una categoría por su identificador.</summary>
    Task<Categoria?> ObtenerPorIdAsync(long id, CancellationToken cancellationToken);
}
