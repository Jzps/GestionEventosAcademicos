using ApiGestionAcademico.Modelos;
using ApiGestionAcademico.Repositorios;

namespace Pruebas.ApiGestionAcademico.Dobles;

/// <summary>Doble en memoria del repositorio de usuarios.</summary>
public sealed class RepositorioUsuarioFalso : IRepositorioUsuario
{
    private readonly AlmacenFalso _almacen;

    public RepositorioUsuarioFalso(AlmacenFalso almacen)
    {
        _almacen = almacen;
    }

    public Task<Usuario?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken)
        => Task.FromResult(_almacen.Usuarios.Values
            .FirstOrDefault(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase)));

    public Task<Usuario?> ObtenerPorIdAsync(long id, CancellationToken cancellationToken)
        => Task.FromResult(_almacen.Usuarios.TryGetValue(id, out var usuario) ? usuario : null);

    public Task GuardarAsync(Usuario usuario, CancellationToken cancellationToken)
    {
        if (usuario.Id == 0)
        {
            usuario.Id = _almacen.ObtenerSiguienteId();
        }

        _almacen.Usuarios[usuario.Id] = usuario;
        return Task.CompletedTask;
    }
}
