using ApiGestionAcademico.Modelos;
using Microsoft.EntityFrameworkCore;

namespace ApiGestionAcademico.Repositorios;

/// <summary>Implementación del acceso a datos de los eventos sobre Entity Framework Core.</summary>
public sealed class RepositorioEvento : IRepositorioEvento
{
    private readonly ContextoDatos _contexto;

    public RepositorioEvento(ContextoDatos contexto)
    {
        _contexto = contexto;
    }

    public async Task<List<Evento>> ListarAsync(string? nombre, DateTime? fecha, long? categoriaId, CancellationToken cancellationToken)
    {
        var consulta = _contexto.Eventos
            .Include(e => e.Categoria)
            .Include(e => e.Responsable)
            .Include(e => e.Inscripciones)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(nombre))
        {
            var texto = nombre.Trim();
            consulta = consulta.Where(e => e.Nombre.Contains(texto));
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

        return await consulta
            .OrderBy(e => e.FechaHoraInicio)
            .ToListAsync(cancellationToken);
    }

    public async Task<Evento?> ObtenerPorIdAsync(long id, CancellationToken cancellationToken)
        => await _contexto.Eventos
            .Include(e => e.Categoria)
            .Include(e => e.Responsable)
            .Include(e => e.Inscripciones)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<List<Evento>> ListarGestionadosAsync(long responsableId, CancellationToken cancellationToken)
        => await _contexto.Eventos
            .Include(e => e.Categoria)
            .Include(e => e.Responsable)
            .Include(e => e.Inscripciones)
            .Where(e => e.ResponsableId == responsableId)
            .OrderBy(e => e.FechaHoraInicio)
            .ToListAsync(cancellationToken);

    public async Task<int> ContarInscripcionesActivasAsync(long eventoId, CancellationToken cancellationToken)
        => await _contexto.Inscripciones
            .CountAsync(i => i.EventoId == eventoId && i.Estado == EstadoInscripcion.Activa, cancellationToken);

    public async Task GuardarAsync(Evento evento, CancellationToken cancellationToken)
    {
        _contexto.Eventos.Add(evento);
        await _contexto.SaveChangesAsync(cancellationToken);
    }

    public async Task ActualizarAsync(Evento evento, CancellationToken cancellationToken)
    {
        // Se marca únicamente el evento como modificado. Usar DbSet.Update sobrescribiría
        // también el responsable, la categoría y las inscripciones relacionadas.
        var entrada = _contexto.Entry(evento);
        if (entrada.State == EntityState.Detached)
        {
            _contexto.Attach(evento);
            entrada = _contexto.Entry(evento);
        }

        entrada.State = EntityState.Modified;
        await _contexto.SaveChangesAsync(cancellationToken);
    }
}
