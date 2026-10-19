using ApiGestionAcademico.Modelos;
using ApiGestionAcademico.Repositorios;

namespace Pruebas.ApiGestionAcademico.Dobles;

/// <summary>Doble en memoria del repositorio de inscripciones.</summary>
public sealed class RepositorioInscripcionFalso : IRepositorioInscripcion
{
    private readonly AlmacenFalso _almacen;

    public RepositorioInscripcionFalso(AlmacenFalso almacen)
    {
        _almacen = almacen;
    }

    public Task GuardarAsync(Inscripcion inscripcion, CancellationToken cancellationToken)
    {
        if (inscripcion.Id == 0)
        {
            inscripcion.Id = _almacen.ObtenerSiguienteId();
        }

        _almacen.Inscripciones[inscripcion.Id] = inscripcion;
        return Task.CompletedTask;
    }

    public Task ActualizarAsync(Inscripcion inscripcion, CancellationToken cancellationToken)
    {
        _almacen.Inscripciones[inscripcion.Id] = inscripcion;
        return Task.CompletedTask;
    }

    public Task<Inscripcion?> ObtenerPorIdAsync(long id, CancellationToken cancellationToken)
        => Task.FromResult(_almacen.Inscripciones.TryGetValue(id, out var inscripcion) ? inscripcion : null);

    public Task<Inscripcion?> ObtenerActivaAsync(long estudianteId, long eventoId, CancellationToken cancellationToken)
        => Task.FromResult(_almacen.Inscripciones.Values.FirstOrDefault(i =>
            i.EstudianteId == estudianteId && i.EventoId == eventoId && i.EstaActiva()));

    public Task GuardarTodasAsync(IEnumerable<Inscripcion> inscripciones, CancellationToken cancellationToken)
    {
        foreach (var inscripcion in inscripciones)
        {
            _almacen.Inscripciones[inscripcion.Id] = inscripcion;
        }

        return Task.CompletedTask;
    }

    public Task<List<Inscripcion>> ListarPorEstudianteAsync(long estudianteId, CancellationToken cancellationToken)
        => Task.FromResult(_almacen.Inscripciones.Values
            .Where(i => i.EstudianteId == estudianteId)
            .OrderByDescending(i => i.FechaHoraRegistro)
            .ToList());

    public Task<List<Inscripcion>> ListarPorEventoAsync(long eventoId, CancellationToken cancellationToken)
        => Task.FromResult(_almacen.Inscripciones.Values
            .Where(i => i.EventoId == eventoId)
            .OrderBy(i => i.FechaHoraRegistro)
            .ToList());
}
