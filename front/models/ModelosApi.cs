namespace FrontGestionEventos.Models;

/// <summary>Rol de usuario en el sistema.</summary>
public enum RolUsuario
{
    Estudiante = 1,
    Responsable = 2
}

/// <summary>Estado identificable de un evento.</summary>
public enum EstadoEvento
{
    Programado = 0,
    InscripcionesAbiertas = 1,
    InscripcionesCerradas = 2,
    EnCurso = 3,
    Finalizado = 4,
    Cancelado = 5
}

/// <summary>Estado de una inscripción.</summary>
public enum EstadoInscripcion
{
    Activa = 1,
    Cancelada = 2
}

/// <summary>Datos del usuario autenticado.</summary>
public sealed class UsuarioActualDto
{
    public long Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public RolUsuario Rol { get; set; }
    public string RolDescripcion { get; set; } = string.Empty;
    public string? Codigo { get; set; }
    public string? Programa { get; set; }
    public string? Dependencia { get; set; }
    public string? Cargo { get; set; }
}

/// <summary>Respuesta del proceso de autenticación.</summary>
public sealed class RespuestaAutenticacionDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiraEn { get; set; }
    public UsuarioActualDto Usuario { get; set; } = new();
}

/// <summary>
/// Resultado de un inicio de sesión exitoso: los datos del usuario y el ticket de un solo
/// uso que el navegador envía para que el servidor firme la cookie de sesión.
/// </summary>
public sealed class ResultadoAccesoDto
{
    public UsuarioActualDto Usuario { get; set; } = new();
    public string Ticket { get; set; } = string.Empty;
}

/// <summary>Resumen de un evento para el listado.</summary>
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

    /// <summary>True cuando el estudiante autenticado tiene una inscripción activa en el evento.</summary>
    public bool EstaInscrito { get; set; }
}

/// <summary>Detalle de un evento.</summary>
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

/// <summary>Categoría de eventos.</summary>
public sealed class CategoriaDto
{
    public long Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
}

/// <summary>Información de una inscripción del estudiante.</summary>
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

/// <summary>Participante de un evento.</summary>
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

/// <summary>Registro de asistencia de un evento.</summary>
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

/// <summary>Respuesta de error de la API.</summary>
public sealed class RespuestaErrorDto
{
    public int Codigo { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public string? Detalle { get; set; }
}

/// <summary>Resultado de retirar un evento de la actividad operativa.</summary>
public sealed class RespuestaRetiroEventoDto
{
    public long EventoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public EstadoEvento Estado { get; set; }
    public int InscripcionesCanceladas { get; set; }
    public string Mensaje { get; set; } = string.Empty;
}
