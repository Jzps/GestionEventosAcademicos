using ApiGestionAcademico.Peticiones;
using ApiGestionAcademico.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiGestionAcademico.Controllers;

/// <summary>Endpoints de autenticación local del prototipo.</summary>
[AllowAnonymous]
[Route("api/auth")]
public sealed class ControladorAutenticacion : ControladorApi
{
    private readonly ServicioAutenticacion _autenticacion;

    public ControladorAutenticacion(ServicioAutenticacion autenticacion)
    {
        _autenticacion = autenticacion;
    }

    /// <summary>Inicia sesión con las credenciales del usuario.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(RespuestaAutenticacionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaErrorDto), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<RespuestaAutenticacionDto>> Login(
        [FromBody] PeticionLogin peticion,
        CancellationToken cancellationToken)
    {
        var respuesta = await _autenticacion.IniciarSesionAsync(peticion, cancellationToken);
        return Ok(respuesta);
    }

    /// <summary>
    /// Registra una cuenta de estudiante. El rol lo asigna el backend, por lo que un
    /// cliente no puede crear una cuenta de responsable desde este endpoint.
    /// </summary>
    [HttpPost("registro")]
    [ProducesResponseType(typeof(RespuestaAutenticacionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RespuestaErrorDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespuestaErrorDto), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RespuestaAutenticacionDto>> Registrar(
        [FromBody] PeticionRegistroDto peticion,
        CancellationToken cancellationToken)
    {
        var respuesta = await _autenticacion.RegistrarAsync(peticion, cancellationToken);
        return Created("/api/auth/perfil", respuesta);
    }
}

/// <summary>Endpoints de consulta del perfil autenticado.</summary>
[Authorize]
[Route("api/auth/perfil")]
public sealed class ControladorPerfil : ControladorApi
{
    private readonly ServicioAutenticacion _autenticacion;

    public ControladorPerfil(ServicioAutenticacion autenticacion)
    {
        _autenticacion = autenticacion;
    }

    /// <summary>Devuelve los datos del usuario autenticado.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(UsuarioActualDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<UsuarioActualDto>> Perfil(CancellationToken cancellationToken)
        => Ok(await _autenticacion.ObtenerPerfilAsync(UsuarioIdActual, cancellationToken));
}
