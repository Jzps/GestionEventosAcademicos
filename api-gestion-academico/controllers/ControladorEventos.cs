using ApiGestionAcademico.Modelos;
using ApiGestionAcademico.Peticiones;
using ApiGestionAcademico.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiGestionAcademico.Controllers;

/// <summary>
/// Endpoints de consulta y gestión de eventos académicos
/// (RF-01, RF-02, RF-03, RF-07, RF-08 y RF-09).
/// </summary>
[Authorize]
[Route("api/eventos")]
public sealed class ControladorEventos : ControladorApi
{
    private readonly ServicioEventos _eventos;

    public ControladorEventos(ServicioEventos eventos)
    {
        _eventos = eventos;
    }

    /// <summary>Lista los eventos disponibles con filtros opcionales por nombre, fecha y categoría.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EventoResumenDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EventoResumenDto>>> Listar(
        [FromQuery] string? nombre,
        [FromQuery] DateTime? fecha,
        [FromQuery] long? categoriaId,
        CancellationToken cancellationToken)
    {
        // Solo el estudiante puede tener inscripciones: el rol permite marcar en el
        // listado si el usuario ya está inscrito en cada evento.
        long? estudianteId = RolActual == RolUsuario.Estudiante ? UsuarioIdActual : null;

        return Ok(await _eventos.ListarAsync(nombre, fecha, categoriaId, estudianteId, cancellationToken));
    }

    /// <summary>Consulta el detalle de un evento.</summary>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(EventoDetalleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaErrorDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventoDetalleDto>> Detalle(long id, CancellationToken cancellationToken)
    {
        long? estudianteId = RolActual == RolUsuario.Estudiante ? UsuarioIdActual : null;
        return Ok(await _eventos.ObtenerDetalleAsync(id, estudianteId, cancellationToken));
    }

    /// <summary>Lista las categorías disponibles.</summary>
    [HttpGet("categorias")]
    [ProducesResponseType(typeof(IEnumerable<CategoriaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CategoriaDto>>> Categorias(CancellationToken cancellationToken)
        => Ok(await _eventos.ListarCategoriasAsync(cancellationToken));

    /// <summary>Lista los eventos administrados por el responsable autenticado.</summary>
    [HttpGet("gestionados")]
    [Authorize(Roles = "Responsable")]
    [ProducesResponseType(typeof(IEnumerable<EventoResumenDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EventoResumenDto>>> Gestionados(CancellationToken cancellationToken)
        => Ok(await _eventos.ListarGestionadosAsync(UsuarioIdActual, cancellationToken));

    /// <summary>Registra un nuevo evento académico.</summary>
    [HttpPost]
    [Authorize(Roles = "Responsable")]
    [ProducesResponseType(typeof(EventoDetalleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RespuestaErrorDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespuestaErrorDto), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EventoDetalleDto>> Registrar(
        [FromBody] PeticionEventoDto peticion,
        CancellationToken cancellationToken)
    {
        var evento = await _eventos.CrearAsync(peticion, UsuarioIdActual, cancellationToken);
        return CreatedAtAction(nameof(Detalle), new { id = evento.Id }, evento);
    }

    /// <summary>
    /// Retira un evento administrado por el responsable: deja de estar disponible y sus
    /// inscripciones activas quedan canceladas (la información histórica se conserva).
    /// </summary>
    [HttpDelete("{id:long}")]
    [Authorize(Roles = "Responsable")]
    [ProducesResponseType(typeof(RespuestaRetiroEventoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaErrorDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(RespuestaErrorDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespuestaErrorDto), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RespuestaRetiroEventoDto>> Retirar(long id, CancellationToken cancellationToken)
        => Ok(await _eventos.RetirarAsync(id, UsuarioIdActual, cancellationToken));

    /// <summary>Modifica un evento administrado por el responsable autenticado.</summary>
    [HttpPut("{id:long}")]
    [Authorize(Roles = "Responsable")]
    [ProducesResponseType(typeof(EventoDetalleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaErrorDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(RespuestaErrorDto), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EventoDetalleDto>> Modificar(
        long id,
        [FromBody] PeticionEventoDto peticion,
        CancellationToken cancellationToken)
        => Ok(await _eventos.ActualizarAsync(id, peticion, UsuarioIdActual, cancellationToken));
}
