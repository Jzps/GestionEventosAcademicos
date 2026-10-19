using ApiGestionAcademico.Excepciones;
using ApiGestionAcademico.Modelos;
using ApiGestionAcademico.Modelos.Estados;
using ApiGestionAcademico.Repositorios;

namespace ApiGestionAcademico.Servicios;

/// <summary>
/// Patrón Facade: punto de entrada único del caso de uso de inscripción (RF-04 / HU-04)
/// y de su cancelación (RF-05 / HU-05). La fachada coordina a <see cref="Estudiante"/>,
/// <see cref="Evento"/> y <see cref="Inscripcion"/>, aplica las reglas de negocio en un
/// orden fijo y garantiza la consistencia transaccional.
/// </summary>
public sealed class InscripcionFacade
{
    private readonly IRepositorioUsuario _usuarios;
    private readonly IRepositorioEvento _eventos;
    private readonly IRepositorioInscripcion _inscripciones;
    private readonly IUnidadTransaccional _unidad;
    private readonly TimeProvider _reloj;

    public InscripcionFacade(
        IRepositorioUsuario usuarios,
        IRepositorioEvento eventos,
        IRepositorioInscripcion inscripciones,
        IUnidadTransaccional unidad,
        TimeProvider reloj)
    {
        _usuarios = usuarios;
        _eventos = eventos;
        _inscripciones = inscripciones;
        _unidad = unidad;
        _reloj = reloj;
    }

    /// <summary>
    /// Registra la inscripción de un estudiante a un evento.
    /// </summary>
    /// <remarks>
    /// Orden de validaciones:
    /// 1. El usuario existe y tiene el rol Estudiante.
    /// 2. El evento existe.
    /// 3. Se sincroniza el estado del evento con la hora actual y los cupos (patrón State).
    /// 4. RN-02: el estudiante no debe tener una inscripción activa en el evento.
    /// 5. RN-01 y RN-08: el evento debe tener cupos disponibles.
    /// 6. RN-05: el evento debe admitir inscripciones.
    /// 7. Se crea la inscripción y se persiste dentro de la transacción.
    /// </remarks>
    public async Task<Inscripcion> InscribirseAsync(long estudianteId, long eventoId, CancellationToken cancellationToken)
    {
        var estudiante = await _usuarios.ObtenerPorIdAsync(estudianteId, cancellationToken) as Estudiante
            ?? throw new ExcepcionAccesoProhibido("Solo un estudiante puede inscribirse a un evento.");

        return await _unidad.EjecutarAsync(
            $"evento:{eventoId}",
            async ct => await EjecutarInscripcionAsync(estudiante, eventoId, ct),
            cancellationToken);
    }

    /// <summary>
    /// Lista las inscripciones de un estudiante con la información del evento
    /// y su estado (RF-06 / HU-06).
    /// </summary>
    public async Task<List<Inscripcion>> ListarPorEstudianteAsync(long estudianteId, CancellationToken cancellationToken)
    {
        var estudiante = await _usuarios.ObtenerPorIdAsync(estudianteId, cancellationToken) as Estudiante
            ?? throw new ExcepcionAccesoProhibido("Solo un estudiante puede consultar sus inscripciones.");

        var inscripciones = await _inscripciones.ListarPorEstudianteAsync(estudiante.Id, cancellationToken);
        var ahora = _reloj.GetUtcNow().LocalDateTime;

        foreach (var inscripcion in inscripciones)
        {
            if (inscripcion.Evento is null)
            {
                continue;
            }

            var activos = await _eventos.ContarInscripcionesActivasAsync(inscripcion.EventoId, cancellationToken);
            inscripcion.Evento.ActualizarEstado(ahora, Math.Max(0, inscripcion.Evento.CuposTotales - activos));
        }

        return inscripciones;
    }

    /// <summary>
    /// Cancela una inscripción propia mientras el evento lo permita (RN-07).
    /// </summary>
    public async Task<Inscripcion> CancelarAsync(long inscripcionId, long estudianteId, CancellationToken cancellationToken)
    {
        var estudiante = await _usuarios.ObtenerPorIdAsync(estudianteId, cancellationToken) as Estudiante
            ?? throw new ExcepcionAccesoProhibido("Solo un estudiante puede cancelar sus inscripciones.");

        var inscripcion = await _inscripciones.ObtenerPorIdAsync(inscripcionId, cancellationToken)
            ?? throw new ExcepcionNoEncontrado("La inscripción no existe.");

        if (inscripcion.EstudianteId != estudiante.Id)
        {
            throw new ExcepcionAccesoProhibido("Solo puede cancelar sus propias inscripciones.");
        }

        if (!inscripcion.EstaActiva())
        {
            throw new ExcepcionNegocio("La inscripción ya se encuentra cancelada.");
        }

        var evento = inscripcion.Evento
            ?? await _eventos.ObtenerPorIdAsync(inscripcion.EventoId, cancellationToken)
            ?? throw new ExcepcionNoEncontrado("El evento de la inscripción no existe.");

        var ahora = _reloj.GetUtcNow().LocalDateTime;
        var activos = await _eventos.ContarInscripcionesActivasAsync(evento.Id, cancellationToken);
        evento.ActualizarEstado(ahora, Math.Max(0, evento.CuposTotales - activos));

        // RN-07: se permite cancelar mientras el evento no haya comenzado. Un evento que
        // cerró sus inscripciones por falta de cupos sigue admitiendo cancelaciones: el
        // estudiante que ocupa el último cupo no debe quedar bloqueado.
        if (!evento.PuedeCancelarInscripcion(ahora))
        {
            throw new ExcepcionNegocio("El evento no permite cancelar inscripciones en este momento.");
        }

        inscripcion.Cancelar(ahora);
        await _inscripciones.ActualizarAsync(inscripcion, cancellationToken);

        return inscripcion;
    }

    private async Task<Inscripcion> EjecutarInscripcionAsync(Estudiante estudiante, long eventoId, CancellationToken cancellationToken)
    {
        var evento = await _eventos.ObtenerPorIdAsync(eventoId, cancellationToken)
            ?? throw new ExcepcionNoEncontrado("El evento solicitado no existe.");

        var ahora = _reloj.GetUtcNow().LocalDateTime;
        var activos = await _eventos.ContarInscripcionesActivasAsync(eventoId, cancellationToken);
        var cuposDisponibles = Math.Max(0, evento.CuposTotales - activos);

        // El estado del evento se recalcula con los datos más recientes (patrón State).
        evento.ActualizarEstado(ahora, cuposDisponibles);

        // RN-02: no puede existir más de una inscripción activa en el mismo evento.
        var existente = await _inscripciones.ObtenerActivaAsync(estudiante.Id, eventoId, cancellationToken);
        if (existente is not null)
        {
            throw new ExcepcionNegocio("Ya te encuentras inscrito en este evento.");
        }

        // RN-01 y RN-08: no se puede inscribir sin cupos disponibles.
        if (cuposDisponibles <= 0)
        {
            throw new ExcepcionNegocio("No hay cupos disponibles para este evento.");
        }

        // RN-05: el evento debe admitir inscripciones.
        if (!evento.EstadoActual.AdmiteInscripciones)
        {
            throw new ExcepcionNegocio(evento.EstadoActual.MotivoBloqueoInscripcion);
        }

        // La creación de la inscripción y la validación de cupos la realiza el propio
        // evento, que delega en su estado (patrón State).
        evento.RegistrarInscripcion(estudiante);
        var inscripcion = evento.Inscripciones.Last();
        await _inscripciones.GuardarAsync(inscripcion, cancellationToken);

        // Si con esta inscripción se agotaron los cupos, el evento cierra sus inscripciones.
        if (evento.GetCuposDisponibles() <= 0 && evento.EstadoActual.AdmiteInscripciones)
        {
            evento.Transicionar(TransicionEvento.CerrarInscripciones);
            await _eventos.ActualizarAsync(evento, cancellationToken);
        }

        return inscripcion;
    }
}
