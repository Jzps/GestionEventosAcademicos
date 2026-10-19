using ApiGestionAcademico.Modelos;
using Microsoft.EntityFrameworkCore;

namespace ApiGestionAcademico.Repositorios;

/// <summary>Implementación del acceso a datos de los usuarios.</summary>
public sealed class RepositorioUsuario : IRepositorioUsuario
{
    private readonly ContextoDatos _contexto;

    public RepositorioUsuario(ContextoDatos contexto)
    {
        _contexto = contexto;
    }

    public async Task<Usuario?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken)
        => await _contexto.Usuarios.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public async Task<Usuario?> ObtenerPorIdAsync(long id, CancellationToken cancellationToken)
        => await _contexto.Usuarios.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task GuardarAsync(Usuario usuario, CancellationToken cancellationToken)
    {
        _contexto.Usuarios.Add(usuario);
        await _contexto.SaveChangesAsync(cancellationToken);
    }
}
