using System.Data;
using Microsoft.EntityFrameworkCore;

namespace ApiGestionAcademico.Repositorios;

/// <summary>
/// Unidad de trabajo transaccional sobre SQL Server. Usa el nivel de aislamiento
/// <see cref="IsolationLevel.Serializable"/> y, cuando se indica un recurso, toma un
/// bloqueo de aplicación exclusivo (<c>sp_getapplock</c>) para que dos inscripciones
/// simultáneas no puedan ocupar el último cupo disponible (RN-08).
/// </summary>
public sealed class UnidadTransaccionalSqlServer : IUnidadTransaccional
{
    private readonly ContextoDatos _contexto;

    public UnidadTransaccionalSqlServer(ContextoDatos contexto)
    {
        _contexto = contexto;
    }

    public async Task<T> EjecutarAsync<T>(string? recursoBloqueo, Func<CancellationToken, Task<T>> operacion, CancellationToken cancellationToken)
    {
        // La estrategia de reintentos del contexto (EnableRetryOnFailure) exige que la
        // transacción se abra dentro de ella, de lo contrario SQL Server rechaza la
        // combinación de transacción de usuario con estrategia de reintentos.
        var estrategia = _contexto.Database.CreateExecutionStrategy();

        return await estrategia.ExecuteAsync(async ct =>
        {
            var transaccion = await _contexto.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);

            try
            {
                if (!string.IsNullOrWhiteSpace(recursoBloqueo))
                {
                    // sp_getapplock serializa el acceso al recurso lógico (por ejemplo, un
                    // evento) para que dos inscripciones simultáneas no ocupen el último cupo.
                    await _contexto.Database.ExecuteSqlInterpolatedAsync(
                        $"EXEC sp_getapplock @Resource = {recursoBloqueo}, @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 15000;",
                        ct);
                }

                var resultado = await operacion(ct);
                await transaccion.CommitAsync(ct);
                return resultado;
            }
            catch
            {
                await transaccion.RollbackAsync(ct);
                throw;
            }
            finally
            {
                await transaccion.DisposeAsync();
            }
        }, cancellationToken);
    }
}
