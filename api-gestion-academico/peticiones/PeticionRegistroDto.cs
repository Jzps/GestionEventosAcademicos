using System.ComponentModel.DataAnnotations;

namespace ApiGestionAcademico.Peticiones;

/// <summary>
/// Datos del registro público de una cuenta de estudiante.
/// No contiene ningún campo de rol: el backend asigna siempre <c>RolUsuario.Estudiante</c>,
/// de modo que un cliente no pueda elevar privilegios desde el formulario.
/// </summary>
public sealed class PeticionRegistroDto
{
    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 200 caracteres.")]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [StringLength(200, ErrorMessage = "El correo no puede superar los 200 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    public string Contrasena { get; set; } = string.Empty;

    [Required(ErrorMessage = "El código del estudiante es obligatorio.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El código debe tener al menos 3 caracteres.")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El programa es obligatorio.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "El programa debe tener al menos 3 caracteres.")]
    public string Programa { get; set; } = string.Empty;
}
