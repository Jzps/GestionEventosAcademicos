using FrontGestionEventos.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace FrontGestionEventos.Services;

/// <summary>Servicio frontend de consulta de participantes y registro de asistencia.</summary>
public sealed class ServicioParticipacion : ClienteApiBase
{
    public ServicioParticipacion(IHttpClientFactory fabrica, AuthenticationStateProvider estadoAutenticacion)
        : base(fabrica, estadoAutenticacion)
    {
    }

    /// <summary>Lista los participantes de un evento (RF-10).</summary>
    public Task<ResultadoApi<List<ParticipanteDto>>> ListarParticipantesAsync(long eventoId, CancellationToken cancellationToken = default)
        => ObtenerAsync<List<ParticipanteDto>>($"api/participantes/{eventoId}", cancellationToken);

    /// <summary>Registra o actualiza la asistencia de un participante (RF-11).</summary>
    public Task<ResultadoApi<AsistenciaDto>> RegistrarAsistenciaAsync(long inscripcionId, bool asistio, string? observacion, CancellationToken cancellationToken = default)
        => EnviarAsync<AsistenciaDto>("api/asistencia", new { inscripcionId, asistio, observacion }, cancellationToken);

    /// <summary>Consulta la asistencia de un evento (RF-12).</summary>
    public Task<ResultadoApi<List<AsistenciaDto>>> ListarAsistenciaAsync(long eventoId, CancellationToken cancellationToken = default)
        => ObtenerAsync<List<AsistenciaDto>>($"api/asistencia/{eventoId}", cancellationToken);
}
