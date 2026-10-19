using FrontGestionEventos.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace FrontGestionEventos.Services;

/// <summary>Servicio frontend de consulta y gestión de eventos.</summary>
public sealed class ServicioEventos : ClienteApiBase
{
    public ServicioEventos(IHttpClientFactory fabrica, AuthenticationStateProvider estadoAutenticacion)
        : base(fabrica, estadoAutenticacion)
    {
    }

    /// <summary>Lista los eventos disponibles con los filtros indicados (RF-01, RF-03).</summary>
    public Task<ResultadoApi<List<EventoResumenDto>>> ListarAsync(string? nombre, DateTime? fecha, long? categoriaId, CancellationToken cancellationToken = default)
    {
        var parametros = new List<string>();

        if (!string.IsNullOrWhiteSpace(nombre))
        {
            parametros.Add($"nombre={Uri.EscapeDataString(nombre.Trim())}");
        }

        if (fecha is not null)
        {
            parametros.Add($"fecha={fecha.Value:yyyy-MM-dd}");
        }

        if (categoriaId is not null && categoriaId > 0)
        {
            parametros.Add($"categoriaId={categoriaId.Value}");
        }

        var ruta = "api/eventos";
        if (parametros.Count > 0)
        {
            ruta += "?" + string.Join("&", parametros);
        }

        return ObtenerAsync<List<EventoResumenDto>>(ruta, cancellationToken);
    }

    /// <summary>Consulta el detalle de un evento (RF-02).</summary>
    public Task<ResultadoApi<EventoDetalleDto>> ObtenerDetalleAsync(long id, CancellationToken cancellationToken = default)
        => ObtenerAsync<EventoDetalleDto>($"api/eventos/{id}", cancellationToken);

    /// <summary>Lista los eventos administrados por el responsable (RF-09).</summary>
    public Task<ResultadoApi<List<EventoResumenDto>>> ListarGestionadosAsync(CancellationToken cancellationToken = default)
        => ObtenerAsync<List<EventoResumenDto>>("api/eventos/gestionados", cancellationToken);

    /// <summary>Lista las categorías disponibles.</summary>
    public Task<ResultadoApi<List<CategoriaDto>>> ListarCategoriasAsync(CancellationToken cancellationToken = default)
        => ObtenerAsync<List<CategoriaDto>>("api/eventos/categorias", cancellationToken);

    /// <summary>Registra un evento académico (RF-07).</summary>
    public Task<ResultadoApi<EventoDetalleDto>> RegistrarAsync(ModeloEventoFormulario modelo, CancellationToken cancellationToken = default)
        => EnviarAsync<EventoDetalleDto>("api/eventos", modelo, cancellationToken);

    /// <summary>
    /// Retira un evento de la actividad operativa: deja de estar disponible y sus
    /// inscripciones activas quedan canceladas.
    /// </summary>
    public Task<ResultadoApi<RespuestaRetiroEventoDto>> RetirarEventoAsync(long id, CancellationToken cancellationToken = default)
        => EliminarAsync<RespuestaRetiroEventoDto>($"api/eventos/{id}", cancellationToken);

    /// <summary>Modifica un evento académico (RF-08).</summary>
    public Task<ResultadoApi<EventoDetalleDto>> ModificarAsync(long id, ModeloEventoFormulario modelo, CancellationToken cancellationToken = default)
        => ActualizarAsync<EventoDetalleDto>($"api/eventos/{id}", modelo, cancellationToken);
}
