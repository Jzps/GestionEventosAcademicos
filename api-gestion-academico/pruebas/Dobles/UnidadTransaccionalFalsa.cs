using ApiGestionAcademico.Repositorios;

namespace Pruebas.ApiGestionAcademico.Dobles;

/// <summary>
/// Doble de la unidad transaccional: ejecuta la operación directamente y registra los
/// recursos bloqueados para poder verificar que la fachada serializa el acceso.
/// </summary>
public sealed class UnidadTransaccionalFalsa : IUnidadTransaccional
{
    /// <summary>Recursos que se bloquearon durante la última operación.</summary>
    public List<string> RecursosBloqueados { get; } = new();

    public async Task<T> EjecutarAsync<T>(string? recursoBloqueo, Func<CancellationToken, Task<T>> operacion, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(recursoBloqueo))
        {
            RecursosBloqueados.Add(recursoBloqueo);
        }

        return await operacion(cancellationToken);
    }
}
