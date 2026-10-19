using ApiGestionAcademico.Peticiones;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiGestionAcademico.Excepciones;

/// <summary>
/// Manejador centralizado de errores. Traduce las excepciones de la aplicación a
/// respuestas HTTP con un mensaje entendible, sin exponer detalles internos.
/// </summary>
public sealed class ManejadorErrores : IExceptionHandler
{
    private readonly ILogger<ManejadorErrores> _logger;

    public ManejadorErrores(ILogger<ManejadorErrores> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (codigo, mensaje, detalle) = Traducir(exception);

        if (codigo >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Error no controlado en {Ruta}", httpContext.Request.Path);
        }
        else
        {
            _logger.LogWarning(exception, "Error de negocio en {Ruta}", httpContext.Request.Path);
        }

        var respuesta = new RespuestaErrorDto
        {
            Codigo = codigo,
            Mensaje = mensaje,
            Detalle = detalle,
            CodigoSeguimiento = httpContext.TraceIdentifier
        };

        httpContext.Response.StatusCode = codigo;
        httpContext.Response.ContentType = "application/json; charset=utf-8";

        await httpContext.Response.WriteAsJsonAsync(respuesta, cancellationToken);
        return true;
    }

    private static (int Codigo, string Mensaje, string? Detalle) Traducir(Exception exception) => exception switch
    {
        ExcepcionNoEncontrado e => (StatusCodes.Status404NotFound, e.Message, null),
        ExcepcionNoAutorizado e => (StatusCodes.Status401Unauthorized, e.Message, null),
        ExcepcionAccesoProhibido e => (StatusCodes.Status403Forbidden, e.Message, null),
        ExcepcionNegocio e => (StatusCodes.Status409Conflict, e.Message, null),
        ArgumentException e => (StatusCodes.Status400BadRequest, e.Message, null),
        _ => (StatusCodes.Status500InternalServerError,
            "Ocurrió un error inesperado al procesar la solicitud.",
            null)
    };
}
