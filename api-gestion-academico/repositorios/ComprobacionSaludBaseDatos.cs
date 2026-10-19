using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ApiGestionAcademico.Repositorios;

/// <summary>
/// Comprobación de salud de la base de datos. Verifica que la conexión funciona y que el
/// esquema está disponible, de modo que el sistema no se declare saludable cuando la
/// base de datos no puede utilizarse para las operaciones.
/// </summary>
public sealed class ComprobacionSaludBaseDatos : IHealthCheck
{
    private readonly ContextoDatos _contexto;

    public ComprobacionSaludBaseDatos(ContextoDatos contexto)
    {
        _contexto = contexto;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext contexto, CancellationToken cancellationToken = default)
    {
        try
        {
            var conectado = await _contexto.Database.CanConnectAsync(cancellationToken);
            if (!conectado)
            {
                return HealthCheckResult.Unhealthy("No se pudo establecer conexión con SQL Server.");
            }

            // Verifica que el esquema se haya aplicado: sin las tablas la API no puede operar.
            await _contexto.Eventos.AsNoTracking().AnyAsync(cancellationToken);
            await _contexto.Usuarios.AsNoTracking().AnyAsync(cancellationToken);

            return HealthCheckResult.Healthy("La base de datos está disponible y el esquema está aplicado.");
        }
        catch (Exception excepcion)
        {
            return HealthCheckResult.Unhealthy("La base de datos no puede utilizarse: " + excepcion.Message);
        }
    }
}
