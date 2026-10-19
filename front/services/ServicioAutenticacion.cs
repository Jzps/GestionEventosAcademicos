using FrontGestionEventos.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace FrontGestionEventos.Services;

/// <summary>
/// Servicio de autenticación del frontend: valida las credenciales contra la API y
/// devuelve un ticket de un solo uso que permite firmar la cookie de sesión.
/// </summary>
public sealed class ServicioAutenticacion : ClienteApiBase
{
    private readonly ServicioTickets _tickets;

    public ServicioAutenticacion(
        IHttpClientFactory fabrica,
        AuthenticationStateProvider estadoAutenticacion,
        ServicioTickets tickets) : base(fabrica, estadoAutenticacion)
    {
        _tickets = tickets;
    }

    /// <summary>
    /// Valida las credenciales contra la API. Si son correctas devuelve los datos del
    /// usuario y el ticket que el navegador debe enviar para activar la cookie de sesión.
    /// </summary>
    public async Task<ResultadoApi<ResultadoAccesoDto>> IniciarSesionAsync(
        ModeloAccesoFormulario modelo,
        CancellationToken cancellationToken = default)
    {
        var cuerpo = new { email = modelo.Email.Trim(), contrasena = modelo.Contrasena };
        var resultado = await EnviarAsync<RespuestaAutenticacionDto>("api/auth/login", cuerpo, cancellationToken);

        if (!resultado.Exito || resultado.Datos is null)
        {
            return ResultadoApi<ResultadoAccesoDto>.Error(resultado.Mensaje, resultado.Codigo);
        }

        // El token nunca se envía al navegador: queda asociado al ticket, que el
        // servidor canjea por la cookie de sesión cifrada.
        var ticket = _tickets.Registrar(resultado.Datos.Token, resultado.Datos.Usuario);

        return ResultadoApi<ResultadoAccesoDto>.Exitoso(new ResultadoAccesoDto
        {
            Usuario = resultado.Datos.Usuario,
            Ticket = ticket
        });
    }

    /// <summary>
    /// Registra una cuenta de estudiante y devuelve el ticket de un solo uso que permite
    /// activar la cookie de sesión. El rol lo asigna el backend: el formulario no lo envía.
    /// </summary>
    public async Task<ResultadoApi<ResultadoAccesoDto>> RegistrarAsync(
        ModeloRegistroFormulario modelo,
        CancellationToken cancellationToken = default)
    {
        var cuerpo = new
        {
            nombreCompleto = modelo.NombreCompleto.Trim(),
            email = modelo.Email.Trim(),
            contrasena = modelo.Contrasena,
            codigo = modelo.Codigo.Trim(),
            programa = modelo.Programa.Trim()
        };

        var resultado = await EnviarAsync<RespuestaAutenticacionDto>("api/auth/registro", cuerpo, cancellationToken);

        if (!resultado.Exito || resultado.Datos is null)
        {
            return ResultadoApi<ResultadoAccesoDto>.Error(resultado.Mensaje, resultado.Codigo);
        }

        var ticket = _tickets.Registrar(resultado.Datos.Token, resultado.Datos.Usuario);

        return ResultadoApi<ResultadoAccesoDto>.Exitoso(new ResultadoAccesoDto
        {
            Usuario = resultado.Datos.Usuario,
            Ticket = ticket
        });
    }
}
