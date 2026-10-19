namespace ApiGestionAcademico.Repositorios;

/// <summary>
/// Unidad de trabajo transaccional. Permite ejecutar una operación de negocio dentro de
/// una transacción y, de forma opcional, tomando un bloqueo exclusivo sobre un recurso
/// lógico (por ejemplo, un evento) para evitar condiciones de carrera.
/// </summary>
public interface IUnidadTransaccional
{
    /// <summary>
    /// Ejecuta la operación dentro de una transacción. Si se indica <paramref name="recursoBloqueo"/>,
    /// se toma un bloqueo exclusivo sobre ese recurso durante la transacción.
    /// </summary>
    Task<T> EjecutarAsync<T>(string? recursoBloqueo, Func<CancellationToken, Task<T>> operacion, CancellationToken cancellationToken);
}
