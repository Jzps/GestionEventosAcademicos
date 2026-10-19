using System.ComponentModel.DataAnnotations;
using ApiGestionAcademico.Modelos;

namespace ApiGestionAcademico.Peticiones;

/// <summary>Resumen de un evento para el listado y las tarjetas de la interfaz.</summary>
public sealed class EventoResumenDto
{
    public long Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaHoraInicio { get; set; }
    public DateTime FechaHoraFin { get; set; }
    public string Lugar { get; set; } = string.Empty;
    public int CuposTotales { get; set; }
    public int CuposDisponibles { get; set; }
    public EstadoEvento Estado { get; set; }
    public string EstadoDescripcion { get; set; } = string.Empty;
    public bool AdmiteInscripciones { get; set; }
    public string NombreCategoria { get; set; } = string.Empty;
    public string NombreResponsable { get; set; } = string.Empty;

    /// <summary>
    /// True cuando el estudiante autenticado tiene una inscripción **activa** en el evento.
    /// Las inscripciones canceladas no cuentan (RN-02 se refiere a las activas).
    /// </summary>
    public bool EstaInscrito { get; set; }
}

/// <summary>Detalle completo de un evento.</summary>
public sealed class EventoDetalleDto
{
    public long Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaHoraInicio { get; set; }
    public DateTime FechaHoraFin { get; set; }
    public string Lugar { get; set; } = string.Empty;
    public int CuposTotales { get; set; }
    public int CuposDisponibles { get; set; }
    public EstadoEvento Estado { get; set; }
    public string EstadoDescripcion { get; set; } = string.Empty;
    public bool AdmiteInscripciones { get; set; }
    public bool PermiteCancelarInscripcion { get; set; }
    public string MotivoBloqueoInscripcion { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }
    public long CategoriaId { get; set; }
    public string NombreCategoria { get; set; } = string.Empty;
    public long ResponsableId { get; set; }
    public string NombreResponsable { get; set; } = string.Empty;
    public string DependenciaResponsable { get; set; } = string.Empty;
    public bool EstaInscrito { get; set; }
}

/// <summary>Categoría disponible para clasificar los eventos.</summary>
public sealed class CategoriaDto
{
    public long Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
}

/// <summary>Datos de registro y modificación de un evento (HU-07 y HU-08).</summary>
public sealed class PeticionEventoDto
{
    [Required(ErrorMessage = "El nombre del evento es obligatorio.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 200 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción del evento es obligatoria.")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "La descripción debe tener al menos 10 caracteres.")]
    public string Descripcion { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha y hora de inicio es obligatoria.")]
    public DateTime FechaHoraInicio { get; set; }

    [Required(ErrorMessage = "La fecha y hora de finalización es obligatoria.")]
    public DateTime FechaHoraFin { get; set; }

    [Required(ErrorMessage = "El lugar del evento es obligatorio.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "El lugar debe tener al menos 3 caracteres.")]
    public string Lugar { get; set; } = string.Empty;

    [Range(1, 100000, ErrorMessage = "Los cupos totales deben ser mayores que cero.")]
    public int CuposTotales { get; set; }

    [Required(ErrorMessage = "Debe seleccionar una categoría.")]
    public long CategoriaId { get; set; }
}

/// <summary>
/// Resultado de retirar (cancelar) un evento de la actividad operativa.
/// </summary>
public sealed class RespuestaRetiroEventoDto
{
    /// <summary>Identificador del evento retirado.</summary>
    public long EventoId { get; set; }

    /// <summary>Nombre del evento retirado.</summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Estado en que queda el evento.</summary>
    public EstadoEvento Estado { get; set; }

    /// <summary>Cantidad de inscripciones activas que quedaron canceladas.</summary>
    public int InscripcionesCanceladas { get; set; }

    /// <summary>Mensaje explicativo para la interfaz.</summary>
    public string Mensaje { get; set; } = string.Empty;
}
