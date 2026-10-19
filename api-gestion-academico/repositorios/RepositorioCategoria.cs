using ApiGestionAcademico.Modelos;
using Microsoft.EntityFrameworkCore;

namespace ApiGestionAcademico.Repositorios;

/// <summary>Implementación del acceso a datos de las categorías.</summary>
public sealed class RepositorioCategoria : IRepositorioCategoria
{
    private readonly ContextoDatos _contexto;

    public RepositorioCategoria(ContextoDatos contexto)
    {
        _contexto = contexto;
    }

    public async Task<List<Categoria>> ListarAsync(CancellationToken cancellationToken)
        => await _contexto.Categorias.OrderBy(c => c.Nombre).ToListAsync(cancellationToken);

    public async Task<Categoria?> ObtenerPorIdAsync(long id, CancellationToken cancellationToken)
        => await _contexto.Categorias.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
}
