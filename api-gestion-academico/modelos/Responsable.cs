namespace ApiGestionAcademico.Modelos;

/// <summary>
/// Usuario con rol Responsable. Registra y administra eventos y controla
/// participantes y asistencia.
/// </summary>
public class Responsable : Usuario
{
    /// <summary>Dependencia o unidad académica a la que pertenece.</summary>
    public string Dependencia { get; internal set; } = string.Empty;

    /// <summary>Cargo que ocupa dentro de la institución.</summary>
    public string Cargo { get; internal set; } = string.Empty;

    protected Responsable()
    {
        Rol = RolUsuario.Responsable;
    }

    public Responsable(string nombreCompleto, string email, string contrasenaHash, string dependencia, string cargo)
        : base(nombreCompleto, email, contrasenaHash)
    {
        Rol = RolUsuario.Responsable;
        Dependencia = dependencia;
        Cargo = cargo;
    }
}
