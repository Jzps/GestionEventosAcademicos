using ApiGestionAcademico.Excepciones;
using ApiGestionAcademico.Modelos;
using ApiGestionAcademico.Peticiones;
using ApiGestionAcademico.Repositorios;

namespace ApiGestionAcademico.Servicios;

/// <summary>
/// Servicio de aplicación para la gestión de eventos: consulta, filtrado, registro y
/// modificación (RF-01, RF-02, RF-03, RF-07, RF-08 y RF-09).
/// </summary>
public sealed class ServicioEventos
{
    private readonly IRepositorioEvento _eventos;
    private readonly IRepositorioCategoria _categorias;
    private readonly IRepositorioUsuario _usuarios;
    private readonly IRepositorioInscripcion _inscripciones;
    private readonly IUnidadTransaccional _unidadTransaccional;
    private readonly TimeProvider _reloj;

    public ServicioEventos(
        IRepositorioEvento eventos,
        IRepositorioCategoria categorias,
        IRepositorioUsuario usuarios,
        IRepositorioInscripcion inscripciones,
        IUnidadTransaccional unidadTransaccional,
        TimeProvider reloj)
    {
        _eventos = eventos;
        _categorias = categorias;
        _usuarios = usuarios;
        _inscripciones = inscripciones;
        _unidadTransaccional = unidadTransaccional;
        _reloj = reloj;
    }

    /// <summary>Lista los eventos disponibles aplicando los criterios de filtrado (RF-01, RF-03).</summary>
    public async Task<List<EventoResumenDto>> ListarAsync(string? nombre, DateTime? fecha, long? categoriaId, long? estudianteId, CancellationToken cancellationToken)
    {
        var eventos = await _eventos.ListarAsync(nombre, fecha, categoriaId, cancellationToken);
        var ahora = _reloj.GetUtcNow().LocalDateTime;

        var resultado = new List<EventoResumenDto>();

        foreach (var evento in eventos)
        {
            await SincronizarEstadoAsync(evento, ahora, cancellationToken);

            if (!evento.EstaDisponible())
            {
                continue;
            }

            resultado.Add(ToResumen(evento, EstaInscritoActiva(evento, estudianteId)));
        }

        return resultado;
    }

    /// <summary>
    /// True cuando el estudiante indicado tiene una inscripción **activa** en el evento.
    /// Las inscripciones canceladas se conservan como historial, pero no cuentan como
    /// inscripción vigente (RN-02 se refiere a las activas).
    /// </summary>
    private static bool EstaInscritoActiva(Evento evento, long? estudianteId)
        => estudianteId is not null
           && evento.Inscripciones.Any(i => i.EstudianteId == estudianteId.Value && i.EstaActiva());

    /// <summary>Consulta el detalle de un evento (RF-02).</summary>
    public async Task<EventoDetalleDto> ObtenerDetalleAsync(long eventoId, long? estudianteId, CancellationToken cancellationToken)
    {
        var evento = await _eventos.ObtenerPorIdAsync(eventoId, cancellationToken)
            ?? throw new ExcepcionNoEncontrado("El evento solicitado no existe.");

        await SincronizarEstadoAsync(evento, _reloj.GetUtcNow().LocalDateTime, cancellationToken);

        var estaInscrito = false;
        if (estudianteId is not null)
        {
            var estudiante = await _usuarios.ObtenerPorIdAsync(estudianteId.Value, cancellationToken);
            if (estudiante is Estudiante)
            {
                estaInscrito = evento.Inscripciones.Any(i => i.EstudianteId == estudianteId && i.EstaActiva());
            }
        }

        return ToDetalle(evento, estaInscrito);
    }

    /// <summary>Lista los eventos administrados por un responsable (RF-09).</summary>
    public async Task<List<EventoResumenDto>> ListarGestionadosAsync(long responsableId, CancellationToken cancellationToken)
    {
        var eventos = await _eventos.ListarGestionadosAsync(responsableId, cancellationToken);
        var ahora = _reloj.GetUtcNow().LocalDateTime;
        var resultado = new List<EventoResumenDto>();

        foreach (var evento in eventos)
        {
            await SincronizarEstadoAsync(evento, ahora, cancellationToken);
            resultado.Add(ToResumen(evento, false));
        }

        return resultado;
    }

    /// <summary>
    /// Retira un evento de la actividad operativa del sistema: el evento pasa al estado
    /// <see cref="EstadoEvento.Cancelado"/> (terminal en el patrón State) y sus
    /// inscripciones activas quedan canceladas. La información histórica de inscripciones
    /// y asistencias se conserva; el evento deja de estar disponible para los estudiantes.
    /// </summary>
    public async Task<RespuestaRetiroEventoDto> RetirarAsync(long eventoId, long responsableId, CancellationToken cancellationToken)
    {
        var evento = await _eventos.ObtenerPorIdAsync(eventoId, cancellationToken)
            ?? throw new ExcepcionNoEncontrado("El evento solicitado no existe.");

        if (evento.ResponsableId != responsableId)
        {
            throw new ExcepcionAccesoProhibido("Solo puede retirar los eventos que administra.");
        }

        if (evento.Estado == EstadoEvento.Cancelado)
        {
            throw new ExcepcionNegocio("El evento ya fue retirado.");
        }

        var momento = _reloj.GetUtcNow().LocalDateTime;
        var canceladas = new List<Inscripcion>();

        // La retirada es transaccional: o se retira el evento y se cancelan sus
        // inscripciones, o no queda ningún cambio a medias.
        await _unidadTransaccional.EjecutarAsync<bool>($"evento:{eventoId}", async ct =>
        {
            canceladas = evento.Retirar(momento);
            await _eventos.ActualizarAsync(evento, ct);

            if (canceladas.Count > 0)
            {
                await _inscripciones.GuardarTodasAsync(canceladas, ct);
            }

            return true;
        }, cancellationToken);

        return new RespuestaRetiroEventoDto
        {
            EventoId = evento.Id,
            Nombre = evento.Nombre,
            Estado = evento.Estado,
            InscripcionesCanceladas = canceladas.Count,
            Mensaje = canceladas.Count > 0
                ? $"El evento «{evento.Nombre}» fue retirado y {canceladas.Count} inscripción(es) activa(s) quedaron canceladas."
                : $"El evento «{evento.Nombre}» fue retirado y ya no está disponible para los estudiantes."
        };
    }

    /// <summary>Lista las categorías disponibles para clasificar los eventos.</summary>
    public async Task<List<CategoriaDto>> ListarCategoriasAsync(CancellationToken cancellationToken)
    {
        var categorias = await _categorias.ListarAsync(cancellationToken);
        return categorias
            .Select(c => new CategoriaDto { Id = c.Id, Nombre = c.Nombre, Descripcion = c.Descripcion })
            .ToList();
    }

    /// <summary>Registra un nuevo evento académico (RF-07 / HU-07) usando el patrón Builder.</summary>
    public async Task<EventoDetalleDto> CrearAsync(PeticionEventoDto peticion, long responsableId, CancellationToken cancellationToken)
    {
        var responsable = await _usuarios.ObtenerPorIdAsync(responsableId, cancellationToken) as Responsable
            ?? throw new ExcepcionNoAutorizado("Solo un responsable puede registrar eventos.");

        var categoria = await _categorias.ObtenerPorIdAsync(peticion.CategoriaId, cancellationToken)
            ?? throw new ExcepcionNoEncontrado("La categoría seleccionada no existe.");

        var ahora = _reloj.GetUtcNow().LocalDateTime;

        // Patrón Builder: el evento se construye paso a paso y nace validado (RN-03, RN-04).
        var evento = new EventoBuilder()
            .ConNombre(peticion.Nombre)
            .ConDescripcion(peticion.Descripcion)
            .ConFechas(peticion.FechaHoraInicio, peticion.FechaHoraFin)
            .ConLugar(peticion.Lugar)
            .ConCupos(peticion.CuposTotales)
            .ConCategoria(categoria)
            .ConResponsable(responsable)
            .ConFechaRegistro(ahora)
            .Build();

        await _eventos.GuardarAsync(evento, cancellationToken);

        // El evento recién creado arranca en estado "Programado"; el dominio lo promociona
        // automáticamente a "Inscripciones abiertas" en su primera lectura.
        await SincronizarEstadoAsync(evento, ahora, cancellationToken);

        return ToDetalle(evento, false);
    }

    /// <summary>Modifica un evento administrado por el responsable (RF-08 / HU-08).</summary>
    public async Task<EventoDetalleDto> ActualizarAsync(long eventoId, PeticionEventoDto peticion, long responsableId, CancellationToken cancellationToken)
    {
        var evento = await _eventos.ObtenerPorIdAsync(eventoId, cancellationToken)
            ?? throw new ExcepcionNoEncontrado("El evento solicitado no existe.");

        if (evento.ResponsableId != responsableId)
        {
            throw new ExcepcionAccesoProhibido("Solo puede modificar los eventos que administra.");
        }

        // Un evento retirado es terminal: no admite modificaciones ni reactivate.
        if (evento.Estado == EstadoEvento.Cancelado)
        {
            throw new ExcepcionNegocio("El evento fue retirado y no admite modificaciones.");
        }

        var categoria = await _categorias.ObtenerPorIdAsync(peticion.CategoriaId, cancellationToken)
            ?? throw new ExcepcionNoEncontrado("La categoría seleccionada no existe.");

        var responsable = await _usuarios.ObtenerPorIdAsync(responsableId, cancellationToken) as Responsable
            ?? throw new ExcepcionNoAutorizado("Solo un responsable puede modificar eventos.");

        // Se vuelve a validar la información con el Builder antes de aplicarla.
        var validado = new EventoBuilder()
            .ConNombre(peticion.Nombre)
            .ConDescripcion(peticion.Descripcion)
            .ConFechas(peticion.FechaHoraInicio, peticion.FechaHoraFin)
            .ConLugar(peticion.Lugar)
            .ConCupos(peticion.CuposTotales)
            .ConCategoria(categoria)
            .ConResponsable(responsable)
            .ConFechaRegistro(evento.FechaRegistro)
            .Build();

        evento.AplicarDatosDe(validado);

        await SincronizarEstadoAsync(evento, _reloj.GetUtcNow().LocalDateTime, cancellationToken);
        await _eventos.ActualizarAsync(evento, cancellationToken);

        return ToDetalle(evento, false);
    }

    /// <summary>
    /// Recalcula el estado del evento según la hora actual y los cupos disponibles y lo
    /// persiste cuando cambia (patrón State).
    /// </summary>
    private async Task SincronizarEstadoAsync(Evento evento, DateTime ahora, CancellationToken cancellationToken)
    {
        var estadoInicial = evento.Estado;
        evento.ActualizarEstado(ahora, evento.GetCuposDisponibles());

        if (evento.Estado != estadoInicial)
        {
            await _eventos.ActualizarAsync(evento, cancellationToken);
        }
    }

    private static EventoResumenDto ToResumen(Evento evento, bool estaInscrito) => new()
    {
        Id = evento.Id,
        Nombre = evento.Nombre,
        Descripcion = evento.Descripcion,
        FechaHoraInicio = evento.FechaHoraInicio,
        FechaHoraFin = evento.FechaHoraFin,
        Lugar = evento.Lugar,
        CuposTotales = evento.CuposTotales,
        CuposDisponibles = evento.GetCuposDisponibles(),
        Estado = evento.Estado,
        EstadoDescripcion = evento.EstadoActual.Etiqueta,
        AdmiteInscripciones = evento.EstadoActual.AdmiteInscripciones && evento.TieneCuposDisponibles(),
        NombreCategoria = evento.Categoria?.Nombre ?? string.Empty,
        NombreResponsable = evento.Responsable?.NombreCompleto ?? string.Empty,
        EstaInscrito = estaInscrito
    };

    private static EventoDetalleDto ToDetalle(Evento evento, bool estaInscrito) => new()
    {
        Id = evento.Id,
        Nombre = evento.Nombre,
        Descripcion = evento.Descripcion,
        FechaHoraInicio = evento.FechaHoraInicio,
        FechaHoraFin = evento.FechaHoraFin,
        Lugar = evento.Lugar,
        CuposTotales = evento.CuposTotales,
        CuposDisponibles = evento.GetCuposDisponibles(),
        Estado = evento.Estado,
        EstadoDescripcion = evento.EstadoActual.Etiqueta,
        AdmiteInscripciones = evento.EstadoActual.AdmiteInscripciones && evento.TieneCuposDisponibles(),
        PermiteCancelarInscripcion = evento.EstadoActual.PermiteCancelarInscripcion,
        MotivoBloqueoInscripcion = evento.EstadoActual.AdmiteInscripciones
            ? evento.TieneCuposDisponibles() ? string.Empty : "No hay cupos disponibles."
            : evento.EstadoActual.MotivoBloqueoInscripcion,
        FechaRegistro = evento.FechaRegistro,
        CategoriaId = evento.CategoriaId,
        NombreCategoria = evento.Categoria?.Nombre ?? string.Empty,
        ResponsableId = evento.ResponsableId,
        NombreResponsable = evento.Responsable?.NombreCompleto ?? string.Empty,
        DependenciaResponsable = evento.Responsable?.Dependencia ?? string.Empty,
        EstaInscrito = estaInscrito
    };
}
