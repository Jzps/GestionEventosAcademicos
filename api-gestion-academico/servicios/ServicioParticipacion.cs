using ApiGestionAcademico.Excepciones;
using ApiGestionAcademico.Modelos;
using ApiGestionAcademico.Peticiones;
using ApiGestionAcademico.Repositorios;

namespace ApiGestionAcademico.Servicios;

/// <summary>
/// Servicio de aplicación para la consulta de participantes (RF-10 / HU-10) y el registro
/// y consulta de asistencia (RF-11 / HU-11, RF-12 / HU-12).
/// </summary>
public sealed class ServicioParticipacion
{
    private readonly IRepositorioEvento _eventos;
    private readonly IRepositorioInscripcion _inscripciones;
    private readonly IRepositorioAsistencia _asistencias;
    private readonly IUnidadTransaccional _unidad;
    private readonly TimeProvider _reloj;

    public ServicioParticipacion(
        IRepositorioEvento eventos,
        IRepositorioInscripcion inscripciones,
        IRepositorioAsistencia asistencias,
        IUnidadTransaccional unidad,
        TimeProvider reloj)
    {
        _eventos = eventos;
        _inscripciones = inscripciones;
        _asistencias = asistencias;
        _unidad = unidad;
        _reloj = reloj;
    }

    /// <summary>Lista los participantes inscritos en un evento administrado por el responsable.</summary>
    public async Task<List<ParticipanteDto>> ListarParticipantesAsync(long eventoId, long responsableId, CancellationToken cancellationToken)
    {
        await VerificarGestionAsync(eventoId, responsableId, cancellationToken);

        var inscripciones = await _inscripciones.ListarPorEventoAsync(eventoId, cancellationToken);

        return inscripciones
            .Select(i => new ParticipanteDto
            {
                InscripcionId = i.Id,
                EstudianteId = i.EstudianteId,
                NombreCompleto = i.Estudiante?.NombreCompleto ?? string.Empty,
                Codigo = i.Estudiante?.Codigo ?? string.Empty,
                Programa = i.Estudiante?.Programa ?? string.Empty,
                Email = i.Estudiante?.Email ?? string.Empty,
                EstadoInscripcion = i.Estado,
                EstadoInscripcionDescripcion = i.Estado == EstadoInscripcion.Activa ? "Activa" : "Cancelada",
                FechaHoraRegistro = i.FechaHoraRegistro,
                TieneAsistencia = i.TieneAsistenciaRegistrada(),
                Asistio = i.Asistencia?.Asistio ?? false,
                Observacion = i.Asistencia?.Observacion
            })
            .ToList();
    }

    /// <summary>Registra o actualiza la asistencia de un participante (RF-11 / HU-11).</summary>
    public async Task<AsistenciaDto> RegistrarAsistenciaAsync(PeticionAsistenciaDto peticion, long responsableId, CancellationToken cancellationToken)
    {
        return await _unidad.EjecutarAsync(
            $"inscripcion:{peticion.InscripcionId}",
            async ct => await EjecutarRegistroAsistenciaAsync(peticion, responsableId, ct),
            cancellationToken);
    }

    /// <summary>Consulta los registros de asistencia de un evento (RF-12 / HU-12).</summary>
    public async Task<List<AsistenciaDto>> ListarAsistenciaAsync(long eventoId, long responsableId, CancellationToken cancellationToken)
    {
        await VerificarGestionAsync(eventoId, responsableId, cancellationToken);

        var registros = await _asistencias.ListarPorEventoAsync(eventoId, cancellationToken);

        return registros
            .Select(a => new AsistenciaDto
            {
                Id = a.Id,
                InscripcionId = a.InscripcionId,
                EventoId = a.Inscripcion?.EventoId ?? eventoId,
                NombreEvento = a.Inscripcion?.Evento?.Nombre ?? string.Empty,
                EstudianteId = a.Inscripcion?.EstudianteId ?? 0,
                NombreCompleto = a.Inscripcion?.Estudiante?.NombreCompleto ?? string.Empty,
                Codigo = a.Inscripcion?.Estudiante?.Codigo ?? string.Empty,
                Asistio = a.Asistio,
                FechaHoraRegistro = a.FechaHoraRegistro,
                Observacion = a.Observacion
            })
            .ToList();
    }

    private async Task<AsistenciaDto> EjecutarRegistroAsistenciaAsync(PeticionAsistenciaDto peticion, long responsableId, CancellationToken cancellationToken)
    {
        var inscripcion = await _inscripciones.ObtenerPorIdAsync(peticion.InscripcionId, cancellationToken)
            ?? throw new ExcepcionNoEncontrado("La inscripción indicada no existe.");

        // El responsable solo puede registrar asistencia en los eventos que administra.
        await VerificarGestionAsync(inscripcion.EventoId, responsableId, cancellationToken);

        // RN-06: solo se registra asistencia para estudiantes inscritos en el evento.
        if (!inscripcion.EstaActiva())
        {
            throw new ExcepcionNegocio("El estudiante no tiene una inscripción activa en este evento.");
        }

        var momento = _reloj.GetUtcNow().LocalDateTime;
        var existente = await _asistencias.ObtenerPorInscripcionAsync(inscripcion.Id, cancellationToken);

        if (existente is null)
        {
            var asistencia = new Asistencia(inscripcion, peticion.Asistio, peticion.Observacion, momento);
            await _asistencias.GuardarAsync(asistencia, cancellationToken);
            return Mapear(asistencia, inscripcion);
        }

        existente.Actualizar(peticion.Asistio, peticion.Observacion, momento);
        await _asistencias.ActualizarAsync(existente, cancellationToken);
        return Mapear(existente, inscripcion);
    }

    private async Task VerificarGestionAsync(long eventoId, long responsableId, CancellationToken cancellationToken)
    {
        var evento = await _eventos.ObtenerPorIdAsync(eventoId, cancellationToken)
            ?? throw new ExcepcionNoEncontrado("El evento solicitado no existe.");

        if (evento.ResponsableId != responsableId)
        {
            throw new ExcepcionAccesoProhibido("Solo el responsable del evento puede administrar sus participantes.");
        }
    }

    private static AsistenciaDto Mapear(Asistencia asistencia, Inscripcion inscripcion) => new()
    {
        Id = asistencia.Id,
        InscripcionId = inscripcion.Id,
        EventoId = inscripcion.EventoId,
        NombreEvento = inscripcion.Evento?.Nombre ?? string.Empty,
        EstudianteId = inscripcion.EstudianteId,
        NombreCompleto = inscripcion.Estudiante?.NombreCompleto ?? string.Empty,
        Codigo = inscripcion.Estudiante?.Codigo ?? string.Empty,
        Asistio = asistencia.Asistio,
        FechaHoraRegistro = asistencia.FechaHoraRegistro,
        Observacion = asistencia.Observacion
    };
}
