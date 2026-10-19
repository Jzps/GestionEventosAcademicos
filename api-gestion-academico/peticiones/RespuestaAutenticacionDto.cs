using ApiGestionAcademico.Modelos;

namespace ApiGestionAcademico.Peticiones;

/// <summary>Información pública del usuario autenticado.</summary>
public sealed class UsuarioActualDto
{
    public long Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public RolUsuario Rol { get; set; }
    public string RolDescripcion { get; set; } = string.Empty;
    public string? Codigo { get; set; }
    public string? Programa { get; set; }
    public string? Dependencia { get; set; }
    public string? Cargo { get; set; }
}

/// <summary>Respuesta del proceso de autenticación: token y datos del usuario.</summary>
public sealed class RespuestaAutenticacionDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiraEn { get; set; }
    public UsuarioActualDto Usuario { get; set; } = new();
}
