using ApiGestionAcademico.Modelos;
using ApiGestionAcademico.Repositorios;

namespace Pruebas.ApiGestionAcademico.Dobles;

/// <summary>Doble en memoria del repositorio de eventos.</summary>
public sealed class RepositorioEventoFalso : IRepositorioEvento
{
    private readonly AlmacenFalso _almacen;

    public RepositorioEventoFalso(AlmacenFalso almacen)
    {
        _almacen = almacen;
    }

    public Task<List<Evento>> ListarAsync(string? nombre, DateTime? fecha, long? categoriaId, CancellationToken cancellationToken)
    {
        var consulta = _almacen.Eventos.Values.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(nombre))
        {
            var texto = nombre.Trim();
            consulta = consulta.Where(e => e.Nombre.Contains(texto, StringComparison.OrdinalIgnoreCase));
        }

        if (fecha is not null)
        {
            var dia = fecha.Value.Date;
            consulta = consulta.Where(e => e.FechaHoraInicio.Date == dia);
        }

        if (categoriaId is not null && categoriaId > 0)
        {
            consulta = consulta.Where(e => e.CategoriaId == categoriaId.Value);
        }

        return Task.FromResult(consulta.OrderBy(e => e.FechaHoraInicio).ToList());
    }

    public Task<Evento?> ObtenerPorIdAsync(long id, CancellationToken cancellationToken)
        => Task.FromResult(_almacen.Eventos.TryGetValue(id, out var evento) ? evento : null);

    public Task<List<Evento>> ListarGestionadosAsync(long responsableId, CancellationToken cancellationToken)
        => Task.FromResult(_almacen.Eventos.Values
            .Where(e => e.ResponsableId == responsableId)
            .OrderBy(e => e.FechaHoraInicio)
            .ToList());

    public Task<int> ContarInscripcionesActivasAsync(long eventoId, CancellationToken cancellationToken)
        => Task.FromResult(_almacen.Inscripciones.Values
            .Count(i => i.EventoId == eventoId && i.EstaActiva()));

    public Task GuardarAsync(Evento evento, CancellationToken cancellationToken)
    {
        if (evento.Id == 0)
        {
            evento.Id = _almacen.ObtenerSiguienteId();
        }

        _almacen.Eventos[evento.Id] = evento;
        return Task.CompletedTask;
    }

    public Task ActualizarAsync(Evento evento, CancellationToken cancellationToken)
    {
        _almacen.Eventos[evento.Id] = evento;
        return Task.CompletedTask;
    }
}
