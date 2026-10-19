using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FrontGestionEventos.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace FrontGestionEventos.Services;

/// <summary>
/// Cliente base de la API. Centraliza el manejo de respuestas, errores y serialización
/// para que los servicios concretos solo se ocupen de su recurso.
///
/// El token de la API se obtiene de la claim <see cref="ClaimsSesion.TokenDeApi"/> de la
/// cookie de sesión y se adjunta en cada petición. Se hace de forma explícita (y no con
/// un DelegatingHandler) porque los handlers creados por IHttpClientFactory se resuelven
/// desde el proveedor raíz, donde los servicios con ámbito de circuito son instancias
/// distintas: el token nunca se habría adjuntado.
/// </summary>
public abstract class ClienteApiBase
{
    private readonly AuthenticationStateProvider _estadoAutenticacion;

    protected ClienteApiBase(IHttpClientFactory fabrica, AuthenticationStateProvider estadoAutenticacion)
    {
        Cliente = fabrica.CreateClient("api");
        _estadoAutenticacion = estadoAutenticacion;
    }

    protected HttpClient Cliente { get; }

    private static readonly JsonSerializerOptions OpcionesJson = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>Obtiene el token de la API de la sesión autenticada, si existe.</summary>
    private async Task<string?> ObtenerTokenAsync()
    {
        var estado = await _estadoAutenticacion.GetAuthenticationStateAsync();
        var principal = estado.User;

        if (principal.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        return principal.FindFirst(ClaimsSesion.TokenDeApi)?.Value;
    }

    /// <summary>Realiza una petición GET y devuelve el resultado de la API.</summary>
    protected async Task<ResultadoApi<T>> ObtenerAsync<T>(string ruta, CancellationToken cancellationToken = default)
        => await EnviarAsync<T>(HttpMethod.Get, ruta, null, cancellationToken);

    /// <summary>Realiza una petición POST y devuelve el resultado de la API.</summary>
    protected async Task<ResultadoApi<T>> EnviarAsync<T>(string ruta, object cuerpo, CancellationToken cancellationToken = default)
        => await EnviarAsync<T>(HttpMethod.Post, ruta, cuerpo, cancellationToken);

    /// <summary>Realiza una petición PUT y devuelve el resultado de la API.</summary>
    protected async Task<ResultadoApi<T>> ActualizarAsync<T>(string ruta, object cuerpo, CancellationToken cancellationToken = default)
        => await EnviarAsync<T>(HttpMethod.Put, ruta, cuerpo, cancellationToken);

    /// <summary>Realiza una petición DELETE y devuelve el resultado de la API.</summary>
    protected async Task<ResultadoApi<T>> EliminarAsync<T>(string ruta, CancellationToken cancellationToken = default)
        => await EnviarAsync<T>(HttpMethod.Delete, ruta, null, cancellationToken);

    private async Task<ResultadoApi<T>> EnviarAsync<T>(HttpMethod metodo, string ruta, object? cuerpo, CancellationToken cancellationToken)
    {
        using var peticion = new HttpRequestMessage(metodo, ruta);
        var token = await ObtenerTokenAsync();

        if (!string.IsNullOrWhiteSpace(token))
        {
            peticion.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        if (cuerpo is not null)
        {
            peticion.Content = JsonContent.Create(cuerpo, options: OpcionesJson);
        }

        HttpResponseMessage respuesta;
        try
        {
            respuesta = await Cliente.SendAsync(peticion, cancellationToken);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return ResultadoApi<T>.Error("La solicitud tardó demasiado. Inténtelo de nuevo.");
        }
        catch (HttpRequestException)
        {
            return ResultadoApi<T>.Error("No fue posible conectar con la API del sistema. Verifique que el servicio esté disponible.");
        }

        using (respuesta)
        {
            return await InterpretarAsync<T>(respuesta, cancellationToken);
        }
    }

    private static async Task<ResultadoApi<T>> InterpretarAsync<T>(HttpResponseMessage respuesta, CancellationToken cancellationToken)
    {
        if (respuesta.IsSuccessStatusCode)
        {
            if (respuesta.StatusCode == HttpStatusCode.NoContent)
            {
                return ResultadoApi<T>.Exitoso(default);
            }

            var contenido = await respuesta.Content.ReadAsStringAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(contenido))
            {
                return ResultadoApi<T>.Exitoso(default);
            }

            var datos = JsonSerializer.Deserialize<T>(contenido, OpcionesJson);
            return ResultadoApi<T>.Exitoso(datos);
        }

        // El mensaje propio de la API tiene prioridad; solo si no existe se usa el
        // mensaje genérico según el código de estado.
        var mensaje = string.Empty;

        try
        {
            var cuerpo = await respuesta.Content.ReadAsStringAsync(cancellationToken);
            if (!string.IsNullOrWhiteSpace(cuerpo))
            {
                var error = JsonSerializer.Deserialize<RespuestaErrorDto>(cuerpo, OpcionesJson);
                if (!string.IsNullOrWhiteSpace(error?.Mensaje))
                {
                    mensaje = error!.Mensaje;
                }
            }
        }
        catch (JsonException)
        {
            // Se conserva el mensaje genérico cuando la API no devuelve un cuerpo JSON.
        }

        if (string.IsNullOrWhiteSpace(mensaje))
        {
            mensaje = respuesta.StatusCode switch
            {
                HttpStatusCode.Unauthorized => "La sesión expiró o las credenciales no son válidas. Inicie sesión nuevamente.",
                HttpStatusCode.Forbidden => "No tiene permisos para realizar esta operación.",
                HttpStatusCode.NotFound => "El recurso solicitado no existe.",
                HttpStatusCode.Conflict => "La operación no se puede completar con los datos actuales.",
                HttpStatusCode.BadRequest => "Los datos enviados no son válidos.",
                _ => $"Se produjo un error ({(int)respuesta.StatusCode}) al communicate con la API."
            };
        }

        return ResultadoApi<T>.Error(mensaje, (int)respuesta.StatusCode);
    }
}
