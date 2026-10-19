using System.Security.Claims;
using ApiGestionAcademico.Excepciones;
using ApiGestionAcademico.Modelos;
using Microsoft.AspNetCore.Mvc;

namespace ApiGestionAcademico.Controllers;

/// <summary>
/// Base de los controladores de la API. Concentra la lectura de los datos de la
/// identidad autenticada para que los controladores no repitan ese trabajo.
/// Cada controlador concreto declara su propia ruta.
/// </summary>
[ApiController]
[Produces("application/json")]
public abstract class ControladorApi : ControllerBase
{
    /// <summary>Identificador del usuario autenticado.</summary>
    protected long UsuarioIdActual
    {
        get
        {
            var valor = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (long.TryParse(valor, out var id))
            {
                return id;
            }

            throw new ExcepcionNoAutorizado("La sesión no es válida.");
        }
    }

    /// <summary>Rol del usuario autenticado.</summary>
    protected RolUsuario RolActual
    {
        get
        {
            var valor = User.FindFirstValue(ClaimTypes.Role);
            return Enum.TryParse<RolUsuario>(valor, out var rol) ? rol : RolUsuario.Estudiante;
        }
    }
}
