using ApiGestionAcademico.Modelos;
using Microsoft.EntityFrameworkCore;

namespace ApiGestionAcademico.Repositorios;

/// <summary>Implementación del acceso a datos de los registros de asistencia.</summary>
public sealed class RepositorioAsistencia : IRepositorioAsistencia
{
    private readonly ContextoDatos _contexto;

    public RepositorioAsistencia(ContextoDatos contexto)
    {
        _contexto = contexto;
    }

    public async Task GuardarAsync(Asistencia asistencia, CancellationToken cancellationToken)
    {
        var entrada = _contexto.Entry(asistencia);
        if (entrada.State == EntityState.Detached)
        {
            _contexto.Asistencias.Attach(asistencia);
            entrada = _contexto.Entry(asistencia);
        }

        entrada.State = EntityState.Added;
        await _contexto.SaveChangesAsync(cancellationToken);
    }

    public async Task ActualizarAsync(Asistencia asistencia, CancellationToken cancellationToken)
    {
        // Solo se modifica el registro de asistencia, no la inscripción relacionada.
        var entrada = _contexto.Entry(asistencia);
        if (entrada.State == EntityState.Detached)
        {
            _contexto.Asistencias.Attach(asistencia);
            entrada = _contexto.Entry(asistencia);
        }

        entrada.State = EntityState.Modified;
        await _contexto.SaveChangesAsync(cancellationToken);
    }

    public async Task<Asistencia?> ObtenerPorInscripcionAsync(long inscripcionId, CancellationToken cancellationToken)
        => await _contexto.Asistencias
            .Include(a => a.Inscripcion).ThenInclude(i => i!.Estudiante)
            .FirstOrDefaultAsync(a => a.InscripcionId == inscripcionId, cancellationToken);

    public async Task<List<Asistencia>> ListarPorEventoAsync(long eventoId, CancellationToken cancellationToken)
        => await _contexto.Asistencias
            .Include(a => a.Inscripcion).ThenInclude(i => i!.Estudiante)
            .Include(a => a.Inscripcion).ThenInclude(i => i!.Evento)
            .Where(a => a.Inscripcion!.EventoId == eventoId)
            .OrderBy(a => a.Inscripcion!.Estudiante!.NombreCompleto)
            .ToListAsync(cancellationToken);
}
