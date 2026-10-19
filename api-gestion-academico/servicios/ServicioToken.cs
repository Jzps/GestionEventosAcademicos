using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiGestionAcademico.Modelos;
using Microsoft.IdentityModel.Tokens;

namespace ApiGestionAcademico.Servicios;

/// <summary>Emite los tokens JWT usedados por la API para autenticar las peticiones.</summary>
public sealed class ServicioToken
{
    private readonly IConfiguration _configuracion;
    private readonly TimeProvider _reloj;

    public ServicioToken(IConfiguration configuracion, TimeProvider reloj)
    {
        _configuracion = configuracion;
        _reloj = reloj;
    }

    /// <summary>Genera un token para el usuario indicado.</summary>
    public (string Token, DateTime ExpiraEn) Generar(Usuario usuario)
    {
        var clave = _configuracion["Jwt:Clave"]
            ?? throw new InvalidOperationException("No se configuró la clave de firma de los tokens (Jwt:Clave).");
        var duracionMinutos = _configuracion.GetValue("Jwt:DuracionMinutos", 120);
        var ahora = _reloj.GetUtcNow().UtcDateTime;
        var expira = ahora.AddMinutes(duracionMinutos);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Email),
            new(JwtRegisteredClaimNames.UniqueName, usuario.Email),
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.NombreCompleto),
            new(ClaimTypes.Role, usuario.Rol.ToString())
        };

        if (usuario is Estudiante estudiante)
        {
            claims.Add(new Claim("codigo", estudiante.Codigo));
            claims.Add(new Claim("programa", estudiante.Programa));
        }
        else if (usuario is Responsable responsable)
        {
            claims.Add(new Claim("dependencia", responsable.Dependencia));
            claims.Add(new Claim("cargo", responsable.Cargo));
        }

        var credenciales = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(clave)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuracion["Jwt:Emisor"],
            audience: _configuracion["Jwt:Audiencia"],
            claims: claims,
            notBefore: ahora,
            expires: expira,
            signingCredentials: credenciales);

        return (new JwtSecurityTokenHandler().WriteToken(token), expira);
    }
}
