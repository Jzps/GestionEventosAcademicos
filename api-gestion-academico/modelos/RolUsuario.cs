namespace ApiGestionAcademico.Modelos;

/// <summary>
/// Roles de usuario soportados por el sistema. El valor se persiste como cadena
/// y actúa además como discriminador de la jerarquía <c>Usuario</c>.
/// </summary>
public enum RolUsuario
{
    Estudiante = 1,
    Responsable = 2
}
