namespace ApiGestionAcademico.Peticiones;

/// <summary>
/// Respuesta uniforme de error de la API. Nunca expone detalles internos de la
/// aplicación ni información sensible como contraseñas.
/// </summary>
public sealed class RespuestaErrorDto
{
    /// <summary>Código HTTP devuelto.</summary>
    public int Codigo { get; set; }

    /// <summary>Mensaje entendible para el usuario final.</summary>
    public string Mensaje { get; set; } = string.Empty;

    /// <summary>Detalle adicional opcional.</summary>
    public string? Detalle { get; set; }

    /// <summary>Identificador de trazabilidad de la petición.</summary>
    public string? CodigoSeguimiento { get; set; }
}
