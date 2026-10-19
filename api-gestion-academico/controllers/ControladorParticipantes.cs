using ApiGestionAcademico.Peticiones;
using ApiGestionAcademico.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiGestionAcademico.Controllers;

/// <summary>Endpoint de consulta de participantes de un evento (RF-10 / HU-10).</summary>
[Authorize(Roles = "Responsable")]
[Route("api/participantes")]
public sealed class ControladorParticipantes : ControladorApi
{
    private readonly ServicioParticipacion _participacion;

    public ControladorParticipantes(ServicioParticipacion participacion)
    {
        _participacion = participacion;
    }

    /// <summary>Lista los estudiantes inscritos en un evento administrado por el responsable.</summary>
    [HttpGet("{eventoId:long}")]
    [ProducesResponseType(typeof(IEnumerable<ParticipanteDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaErrorDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(RespuestaErrorDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<ParticipanteDto>>> Listar(long eventoId, CancellationToken cancellationToken)
        => Ok(await _participacion.ListarParticipantesAsync(eventoId, UsuarioIdActual, cancellationToken));
}
