using ApiGestionAcademico.Modelos;

namespace ApiGestionAcademico.Peticiones;

/// <summary>Solicitud de inscripción a un evento (HU-04).</summary>
public sealed class PeticionInscripcionDto
{
    /// <summary>Identificador del evento al que se desea inscribirse.</summary>
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Debe indicar el evento.")]
    [System.ComponentModel.DataAnnotations.Range(1, long.MaxValue, ErrorMessage = "El evento indicado no es válido.")]
    public long EventoId { get; set; }
}

/// <summary>Información de una inscripción consultada por el estudiante (HU-06).</summary>
public sealed class InscripcionDto
{
    public long Id { get; set; }
    public long EventoId { get; set; }
    public string NombreEvento { get; set; } = string.Empty;
    public DateTime FechaHoraInicio { get; set; }
    public string Lugar { get; set; } = string.Empty;
    public EstadoInscripcion Estado { get; set; }
    public string EstadoDescripcion { get; set; } = string.Empty;
    public DateTime FechaHoraRegistro { get; set; }
    public DateTime? FechaHoraCancelacion { get; set; }
    public bool PermiteCancelar { get; set; }
    public string MotivoBloqueoCancelacion { get; set; } = string.Empty;
}
