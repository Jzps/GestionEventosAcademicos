using System.ComponentModel.DataAnnotations;

namespace FrontGestionEventos.Models;

/// <summary>Modelo del formulario de registro y modificación de eventos (HU-07 y HU-08).</summary>
public sealed class ModeloEventoFormulario
{
    [Required(ErrorMessage = "El nombre del evento es obligatorio.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "El nombre debe tener al menos 3 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción del evento es obligatoria.")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "La descripción debe tener al menos 10 caracteres.")]
    public string Descripcion { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha y hora de inicio es obligatoria.")]
    public DateTime? FechaHoraInicio { get; set; }

    [Required(ErrorMessage = "La fecha y hora de finalización es obligatoria.")]
    public DateTime? FechaHoraFin { get; set; }

    [Required(ErrorMessage = "El lugar del evento es obligatorio.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "El lugar debe tener al menos 3 caracteres.")]
    public string Lugar { get; set; } = string.Empty;

    [Range(1, 100000, ErrorMessage = "Los cupos totales deben ser mayores que cero.")]
    public int CuposTotales { get; set; } = 30;

    [Required(ErrorMessage = "Debe seleccionar una categoría.")]
    public long CategoriaId { get; set; }
}

/// <summary>Modelo del formulario de acceso al sistema.</summary>
public sealed class ModeloAccesoFormulario
{
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    public string Contrasena { get; set; } = string.Empty;
}

/// <summary>
/// Modelo del formulario de registro público. El rol no forma parte del formulario:
/// el backend asigna siempre el rol Estudiante.
/// </summary>
public sealed class ModeloRegistroFormulario
{
    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 200 caracteres.")]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    public string Contrasena { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debe repetir la contraseña.")]
    [Compare(nameof(Contrasena), ErrorMessage = "Las contraseñas no coinciden.")]
    public string ConfirmarContrasena { get; set; } = string.Empty;

    [Required(ErrorMessage = "El código del estudiante es obligatorio.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El código debe tener al menos 3 caracteres.")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El programa es obligatorio.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "El programa debe tener al menos 3 caracteres.")]
    public string Programa { get; set; } = string.Empty;
}
