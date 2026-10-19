using ApiGestionAcademico.Modelos;

namespace ApiGestionAcademico.Peticiones;

/// <summary>Participante de un evento, con su inscripción y asistencia (HU-10).</summary>
public sealed class ParticipanteDto
{
    public long InscripcionId { get; set; }
    public long EstudianteId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string Programa { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public EstadoInscripcion EstadoInscripcion { get; set; }
    public string EstadoInscripcionDescripcion { get; set; } = string.Empty;
    public DateTime FechaHoraRegistro { get; set; }
    public bool TieneAsistencia { get; set; }
    public bool Asistio { get; set; }
    public string? Observacion { get; set; }
}

/// <summary>Solicitud de registro de asistencia (HU-11).</summary>
public sealed class PeticionAsistenciaDto
{
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Debe indicar la inscripción.")]
    [System.ComponentModel.DataAnnotations.Range(1, long.MaxValue, ErrorMessage = "La inscripción indicada no es válida.")]
    public long InscripcionId { get; set; }

    public bool Asistio { get; set; }

    [System.ComponentModel.DataAnnotations.StringLength(500, ErrorMessage = "La observación no puede superar los 500 caracteres.")]
    public string? Observacion { get; set; }
}

/// <summary>Registro de asistencia de un evento (HU-12).</summary>
public sealed class AsistenciaDto
{
    public long Id { get; set; }
    public long InscripcionId { get; set; }
    public long EventoId { get; set; }
    public string NombreEvento { get; set; } = string.Empty;
    public long EstudianteId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public bool Asistio { get; set; }
    public DateTime FechaHoraRegistro { get; set; }
    public string? Observacion { get; set; }
}
