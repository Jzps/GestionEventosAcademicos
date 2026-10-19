namespace ApiGestionAcademico.Modelos;

/// <summary>
/// Registro de asistencia de un participante a un evento. Solo puede existir si el
/// estudiante cuenta con una inscripción vigente (RN-06).
/// </summary>
public sealed class Asistencia
{
    /// <summary>Identificador del registro.</summary>
    public long Id { get; internal set; }

    /// <summary>Inscripción a la que corresponde el registro.</summary>
    public long InscripcionId { get; internal set; }

    /// <summary>Indicador de asistencia.</summary>
    public bool Asistio { get; internal set; }

    /// <summary>Fecha y hora en que se registró la asistencia.</summary>
    public DateTime FechaHoraRegistro { get; internal set; }

    /// <summary>Observación opcional del responsable.</summary>
    public string? Observacion { get; internal set; }

    /// <summary>Inscripción relacionada.</summary>
    public Inscripcion? Inscripcion { get; internal set; }

    public Asistencia()
    {
    }

    public Asistencia(Inscripcion inscripcion, bool asistio, string? observacion, DateTime momento)
    {
        InscripcionId = inscripcion.Id;
        Inscripcion = inscripcion;
        Asistio = asistio;
        Observacion = observacion;
        FechaHoraRegistro = momento;
    }

    /// <summary>Registra por primera vez la asistencia.</summary>
    public void Registrar(bool asistio, string? observacion, DateTime momento)
    {
        Asistio = asistio;
        Observacion = observacion;
        FechaHoraRegistro = momento;
    }

    /// <summary>Actualiza un registro de asistencia existente.</summary>
    public void Actualizar(bool asistio, string? observacion, DateTime momento)
    {
        Asistio = asistio;
        Observacion = observacion;
        FechaHoraRegistro = momento;
    }
}
