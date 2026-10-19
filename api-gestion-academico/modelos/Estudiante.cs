namespace ApiGestionAcademico.Modelos;

/// <summary>
/// Usuario con rol Estudiante. Puede consultar eventos e inscribirse.
/// </summary>
public class Estudiante : Usuario
{
    /// <summary>Código académico del estudiante.</summary>
    public string Codigo { get; internal set; } = string.Empty;

    /// <summary>Programa académico en el que está matriculado.</summary>
    public string Programa { get; internal set; } = string.Empty;

    protected Estudiante()
    {
        Rol = RolUsuario.Estudiante;
    }

    public Estudiante(string nombreCompleto, string email, string contrasenaHash, string codigo, string programa)
        : base(nombreCompleto, email, contrasenaHash)
    {
        Rol = RolUsuario.Estudiante;
        Codigo = codigo;
        Programa = programa;
    }
}
