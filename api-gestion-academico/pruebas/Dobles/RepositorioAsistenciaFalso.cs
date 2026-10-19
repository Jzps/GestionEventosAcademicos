using ApiGestionAcademico.Modelos;
using ApiGestionAcademico.Repositorios;

namespace Pruebas.ApiGestionAcademico.Dobles;

/// <summary>Doble en memoria del repositorio de asistencias.</summary>
public sealed class RepositorioAsistenciaFalso : IRepositorioAsistencia
{
    private readonly AlmacenFalso _almacen;

    public RepositorioAsistenciaFalso(AlmacenFalso almacen)
    {
        _almacen = almacen;
    }

    public Task GuardarAsync(Asistencia asistencia, CancellationToken cancellationToken)
    {
        if (asistencia.Id == 0)
        {
            asistencia.Id = _almacen.ObtenerSiguienteId();
        }

        _almacen.Asistencias[asistencia.Id] = asistencia;
        return Task.CompletedTask;
    }

    public Task ActualizarAsync(Asistencia asistencia, CancellationToken cancellationToken)
    {
        _almacen.Asistencias[asistencia.Id] = asistencia;
        return Task.CompletedTask;
    }

    public Task<Asistencia?> ObtenerPorInscripcionAsync(long inscripcionId, CancellationToken cancellationToken)
        => Task.FromResult(_almacen.Asistencias.Values
            .FirstOrDefault(a => a.InscripcionId == inscripcionId));

    public Task<List<Asistencia>> ListarPorEventoAsync(long eventoId, CancellationToken cancellationToken)
        => Task.FromResult(_almacen.Asistencias.Values
            .Where(a => a.Inscripcion?.EventoId == eventoId)
            .ToList());
}
