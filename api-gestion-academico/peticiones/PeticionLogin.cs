namespace ApiGestionAcademico.Peticiones;

/// <summary>Credenciales enviadas en el inicio de sesión.</summary>
public sealed class PeticionLogin
{
    /// <summary>Correo institucional del usuario.</summary>
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "El correo es obligatorio.")]
    [System.ComponentModel.DataAnnotations.EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    public string Email { get; set; } = string.Empty;

    /// <summary>Contraseña del usuario.</summary>
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "La contraseña es obligatoria.")]
    public string Contrasena { get; set; } = string.Empty;
}
