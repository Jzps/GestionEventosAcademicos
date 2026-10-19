using ApiGestionAcademico.Modelos;
using ApiGestionAcademico.Peticiones;
using ApiGestionAcademico.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiGestionAcademico.Controllers;

/// <summary>
/// Endpoints de gestión de inscripciones del estudiante
/// (RF-04, RF-05 y RF-06).
/// </summary>
[Authorize(Roles = "Estudiante")]
[Route("api/inscripciones")]
public sealed class ControladorInscripciones : ControladorApi
{
    private readonly InscripcionFacade _inscripciones;

    public ControladorInscripciones(InscripcionFacade inscripciones)
    {
        _inscripciones = inscripciones;
    }

    /// <summary>Registra la inscripción del estudiante a un evento.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(InscripcionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RespuestaErrorDto), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<InscripcionDto>> Inscribirse(
        [FromBody] PeticionInscripcionDto peticion,
        CancellationToken cancellationToken)
    {
        var inscripcion = await _inscripciones.InscribirseAsync(UsuarioIdActual, peticion.EventoId, cancellationToken);
        var dto = await MapearAsync(inscripcion, cancellationToken);
        return Created($"/api/inscripciones/{dto.Id}", dto);
    }

    /// <summary>Cancela una inscripción propia.</summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(typeof(InscripcionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaErrorDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(RespuestaErrorDto), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<InscripcionDto>> Cancelar(long id, CancellationToken cancellationToken)
    {
        var inscripcion = await _inscripciones.CancelarAsync(id, UsuarioIdActual, cancellationToken);
        return Ok(await MapearAsync(inscripcion, cancellationToken));
    }

    /// <summary>Lista las inscripciones del estudiante autenticado.</summary>
    [HttpGet("mias")]
    [ProducesResponseType(typeof(IEnumerable<InscripcionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<InscripcionDto>>> MisInscripciones(CancellationToken cancellationToken)
    {
        var lista = await _inscripciones.ListarPorEstudianteAsync(UsuarioIdActual, cancellationToken);
        var resultado = new List<InscripcionDto>();

        foreach (var inscripcion in lista)
        {
            resultado.Add(await MapearAsync(inscripcion, cancellationToken));
        }

        return Ok(resultado);
    }

    private async Task<InscripcionDto> MapearAsync(Inscripcion inscripcion, CancellationToken cancellationToken)
    {
        // La inscripción se carga con su evento (incluido en la fachada), de modo que el
        // estado de cancelación refleja las reglas reales del evento (RN-07).
        var evento = inscripcion.Evento;
        var permiteCancelar = evento is not null
            && inscripcion.EstaActiva()
            && evento.PuedeCancelarInscripcion(DateTime.Now);

        return new InscripcionDto
        {
            Id = inscripcion.Id,
            EventoId = inscripcion.EventoId,
            NombreEvento = evento?.Nombre ?? string.Empty,
            FechaHoraInicio = evento?.FechaHoraInicio ?? default,
            Lugar = evento?.Lugar ?? string.Empty,
            Estado = inscripcion.Estado,
            EstadoDescripcion = inscripcion.Estado == EstadoInscripcion.Activa ? "Activa" : "Cancelada",
            FechaHoraRegistro = inscripcion.FechaHoraRegistro,
            FechaHoraCancelacion = inscripcion.FechaHoraCancelacion,
            PermiteCancelar = permiteCancelar,
            MotivoBloqueoCancelacion = permiteCancelar
                ? string.Empty
                : "El evento ya comenzó, se canceló o no admite cancelaciones."
        };
    }
}
