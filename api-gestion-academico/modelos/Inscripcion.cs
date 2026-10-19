namespace ApiGestionAcademico.Modelos;

/// <summary>
/// Inscripción de un estudiante a un evento. Concentra su propio estado (activa o cancelada)
/// y la relación con la asistencia, si ya fue registrada.
/// </summary>
public sealed class Inscripcion
{
    /// <summary>Identificador de la inscripción.</summary>
    public long Id { get; internal set; }

    /// <summary>Identificador del estudiante inscrito.</summary>
    public long EstudianteId { get; internal set; }

    /// <summary>Identificador del evento.</summary>
    public long EventoId { get; internal set; }

    /// <summary>Fecha y hora en que se realizó la inscripción.</summary>
    public DateTime FechaHoraRegistro { get; internal set; }

    /// <summary>Estado de la inscripción.</summary>
    public EstadoInscripcion Estado { get; internal set; } = EstadoInscripcion.Activa;

    /// <summary>Fecha y hora de cancelación, cuando aplica.</summary>
    public DateTime? FechaHoraCancelacion { get; internal set; }

    /// <summary>Estudiante inscrito.</summary>
    public Estudiante? Estudiante { get; internal set; }

    /// <summary>Evento al que corresponde la inscripción.</summary>
    public Evento? Evento { get; internal set; }

    /// <summary>Registro de asistencia asociado (como máximo uno).</summary>
    public Asistencia? Asistencia { get; internal set; }

    public Inscripcion()
    {
    }

    public Inscripcion(Evento evento, Estudiante estudiante, DateTime fechaHoraRegistro)
    {
        EventoId = evento.Id;
        Evento = evento;
        EstudianteId = estudiante.Id;
        Estudiante = estudiante;
        FechaHoraRegistro = fechaHoraRegistro;
        Estado = EstadoInscripcion.Activa;
    }

    /// <summary>True cuando la inscripción está activa.</summary>
    public bool EstaActiva() => Estado == EstadoInscripcion.Activa;

    /// <summary>True cuando ya existe un registro de asistencia para la inscripción.</summary>
    public bool TieneAsistenciaRegistrada() => Asistencia is not null;

    /// <summary>Cancela la inscripción (RN-07).</summary>
    public void Cancelar(DateTime momento)
    {
        if (!EstaActiva())
        {
            throw new Excepciones.ExcepcionNegocio("La inscripción ya se encuentra cancelada.");
        }

        Estado = EstadoInscripcion.Cancelada;
        FechaHoraCancelacion = momento;
    }
}
