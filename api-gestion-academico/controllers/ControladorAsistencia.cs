using ApiGestionAcademico.Peticiones;
using ApiGestionAcademico.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiGestionAcademico.Controllers;

/// <summary>Endpoints de registro y consulta de asistencia (RF-11 y RF-12).</summary>
[Authorize(Roles = "Responsable")]
[Route("api/asistencia")]
public sealed class ControladorAsistencia : ControladorApi
{
    private readonly ServicioParticipacion _participacion;

    public ControladorAsistencia(ServicioParticipacion participacion)
    {
        _participacion = participacion;
    }

    /// <summary>Registra o actualiza la asistencia de un participante.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(AsistenciaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RespuestaErrorDto), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AsistenciaDto>> Registrar(
        [FromBody] PeticionAsistenciaDto peticion,
        CancellationToken cancellationToken)
    {
        var registro = await _participacion.RegistrarAsistenciaAsync(peticion, UsuarioIdActual, cancellationToken);
        return Created($"/api/asistencia/{registro.InscripcionId}", registro);
    }

    /// <summary>Consulta los registros de asistencia de un evento.</summary>
    [HttpGet("{eventoId:long}")]
    [ProducesResponseType(typeof(IEnumerable<AsistenciaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaErrorDto), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<AsistenciaDto>>> Listar(long eventoId, CancellationToken cancellationToken)
        => Ok(await _participacion.ListarAsistenciaAsync(eventoId, UsuarioIdActual, cancellationToken));
}
