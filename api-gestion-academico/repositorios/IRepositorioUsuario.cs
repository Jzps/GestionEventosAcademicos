using ApiGestionAcademico.Modelos;

namespace ApiGestionAcademico.Repositorios;

/// <summary>Contrato de acceso a los usuarios del sistema.</summary>
public interface IRepositorioUsuario
{
    /// <summary>Busca un usuario por su correo.</summary>
    Task<Usuario?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken);

    /// <summary>Busca un usuario por su identificador.</summary>
    Task<Usuario?> ObtenerPorIdAsync(long id, CancellationToken cancellationToken);

    /// <summary>Persiste un usuario nuevo.</summary>
    Task GuardarAsync(Usuario usuario, CancellationToken cancellationToken);
}
