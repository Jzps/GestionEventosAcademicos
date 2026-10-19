namespace ApiGestionAcademico.Modelos;

/// <summary>
/// Clase abstracta base de los usuarios del sistema. Concentra los datos comunes de
/// identidad y perfil. El rol concreto se define en las subclases, de modo que el tipo
/// del objeto y el rol persistido nunca pueden contradecirse (LSP).
/// </summary>
public abstract class Usuario
{
    /// <summary>Identificador del usuario.</summary>
    public long Id { get; internal set; }

    /// <summary>Nombre completo de la persona.</summary>
    public string NombreCompleto { get; internal set; } = string.Empty;

    /// <summary>Correo institucional, usado como nombre de usuario.</summary>
    public string Email { get; internal set; } = string.Empty;

    /// <summary>Hash de la contraseña (PBKDF2). Nunca se almacena la contraseña en texto plano.</summary>
    public string ContrasenaHash { get; internal set; } = string.Empty;

    /// <summary>Rol del usuario. Es el discriminador de la jerarquía en la tabla Usuarios.</summary>
    public RolUsuario Rol { get; internal set; }

    /// <summary>Indica si la cuenta está habilitada para iniciar sesión.</summary>
    public bool Activo { get; internal set; } = true;

    protected Usuario()
    {
    }

    protected Usuario(string nombreCompleto, string email, string contrasenaHash)
    {
        NombreCompleto = nombreCompleto;
        Email = email;
        ContrasenaHash = contrasenaHash;
        Activo = true;
    }

    /// <summary>Devuelve el rol efectivo del usuario.</summary>
    public RolUsuario GetRol() => Rol;
}
