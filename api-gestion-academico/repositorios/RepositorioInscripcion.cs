using ApiGestionAcademico.Modelos;
using Microsoft.EntityFrameworkCore;

namespace ApiGestionAcademico.Repositorios;

/// <summary>Implementación del acceso a datos de las inscripciones.</summary>
public sealed class RepositorioInscripcion : IRepositorioInscripcion
{
    private readonly ContextoDatos _contexto;

    public RepositorioInscripcion(ContextoDatos contexto)
    {
        _contexto = contexto;
    }

    public async Task GuardarAsync(Inscripcion inscripcion, CancellationToken cancellationToken)
    {
        // Se adjunta la inscripción sin propagar el estado Added al evento y al
        // estudiante relacionados, que ya están siendo rastreados por el contexto.
        var entrada = _contexto.Entry(inscripcion);
        if (entrada.State == EntityState.Detached)
        {
            _contexto.Inscripciones.Attach(inscripcion);
            entrada = _contexto.Entry(inscripcion);
        }

        entrada.State = EntityState.Added;
        await _contexto.SaveChangesAsync(cancellationToken);
    }

    public async Task ActualizarAsync(Inscripcion inscripcion, CancellationToken cancellationToken)
    {
        // Solo se modifica la inscripción: no se sobrescribe el evento ni el estudiante.
        var entrada = _contexto.Entry(inscripcion);
        if (entrada.State == EntityState.Detached)
        {
            _contexto.Inscripciones.Attach(inscripcion);
            entrada = _contexto.Entry(inscripcion);
        }

        entrada.State = EntityState.Modified;
        await _contexto.SaveChangesAsync(cancellationToken);
    }

    public async Task<Inscripcion?> ObtenerPorIdAsync(long id, CancellationToken cancellationToken)
        => await _contexto.Inscripciones
            .Include(i => i.Estudiante)
            .Include(i => i.Evento).ThenInclude(e => e!.Responsable)
            .Include(i => i.Asistencia)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

    public async Task<Inscripcion?> ObtenerActivaAsync(long estudianteId, long eventoId, CancellationToken cancellationToken)
        => await _contexto.Inscripciones
            .Include(i => i.Estudiante)
            .Include(i => i.Evento).ThenInclude(e => e!.Responsable)
            .Include(i => i.Asistencia)
            .FirstOrDefaultAsync(i => i.EstudianteId == estudianteId
                && i.EventoId == eventoId
                && i.Estado == EstadoInscripcion.Activa, cancellationToken);

    public async Task GuardarTodasAsync(IEnumerable<Inscripcion> inscripciones, CancellationToken cancellationToken)
    {
        foreach (var inscripcion in inscripciones)
        {
            var entrada = _contexto.Entry(inscripcion);
            if (entrada.State == EntityState.Detached)
            {
                _contexto.Inscripciones.Attach(inscripcion);
                entrada = _contexto.Entry(inscripcion);
            }

            entrada.State = EntityState.Modified;
        }

        await _contexto.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<Inscripcion>> ListarPorEstudianteAsync(long estudianteId, CancellationToken cancellationToken)
        => await _contexto.Inscripciones
            .Include(i => i.Evento).ThenInclude(e => e!.Responsable)
            .Include(i => i.Evento).ThenInclude(e => e!.Categoria)
            .Include(i => i.Asistencia)
            .Where(i => i.EstudianteId == estudianteId)
            .OrderByDescending(i => i.FechaHoraRegistro)
            .ToListAsync(cancellationToken);

    public async Task<List<Inscripcion>> ListarPorEventoAsync(long eventoId, CancellationToken cancellationToken)
        => await _contexto.Inscripciones
            .Include(i => i.Estudiante)
            .Include(i => i.Asistencia)
            .Where(i => i.EventoId == eventoId)
            .OrderBy(i => i.FechaHoraRegistro)
            .ToListAsync(cancellationToken);
}
