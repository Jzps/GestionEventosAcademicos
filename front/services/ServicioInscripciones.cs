using FrontGestionEventos.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace FrontGestionEventos.Services;

/// <summary>Servicio frontend de gestión de inscripciones del estudiante.</summary>
public sealed class ServicioInscripciones : ClienteApiBase
{
    public ServicioInscripciones(IHttpClientFactory fabrica, AuthenticationStateProvider estadoAutenticacion)
        : base(fabrica, estadoAutenticacion)
    {
    }

    /// <summary>Registra la inscripción a un evento (RF-04).</summary>
    public Task<ResultadoApi<InscripcionDto>> InscribirseAsync(long eventoId, CancellationToken cancellationToken = default)
        => EnviarAsync<InscripcionDto>("api/inscripciones", new { eventoId }, cancellationToken);

    /// <summary>Cancela una inscripción propia (RF-05).</summary>
    public Task<ResultadoApi<InscripcionDto>> CancelarAsync(long inscripcionId, CancellationToken cancellationToken = default)
        => EliminarAsync<InscripcionDto>($"api/inscripciones/{inscripcionId}", cancellationToken);

    /// <summary>Lista las inscripciones del estudiante (RF-06).</summary>
    public Task<ResultadoApi<List<InscripcionDto>>> MisInscripcionesAsync(CancellationToken cancellationToken = default)
        => ObtenerAsync<List<InscripcionDto>>("api/inscripciones/mias", cancellationToken);
}
