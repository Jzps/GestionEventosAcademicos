using System.ComponentModel.DataAnnotations.Schema;
using ApiGestionAcademico.Excepciones;
using ApiGestionAcademico.Modelos.Estados;

namespace ApiGestionAcademico.Modelos;

/// <summary>
/// Entidad central del dominio: un evento académico. Concentra sus propios datos y las
/// reglas que le corresponden (cupos, fechas y estado del ciclo de vida). La lógica de
/// inscripción se delega en el objeto de estado (patrón State) y la construcción válida
/// del evento se delega en <see cref="EventoBuilder"/> (patrón Builder).
/// </summary>
public sealed class Evento
{
    private IEventoEstado? _estadoActual;

    /// <summary>Identificador del evento.</summary>
    public long Id { get; internal set; }

    /// <summary>Nombre del evento.</summary>
    public string Nombre { get; internal set; } = string.Empty;

    /// <summary>Descripción del evento.</summary>
    public string Descripcion { get; internal set; } = string.Empty;

    /// <summary>Fecha y hora de inicio.</summary>
    public DateTime FechaHoraInicio { get; internal set; }

    /// <summary>Fecha y hora de finalización.</summary>
    public DateTime FechaHoraFin { get; internal set; }

    /// <summary>Lugar donde se realiza el evento.</summary>
    public string Lugar { get; internal set; } = string.Empty;

    /// <summary>Cupo total del evento. Siempre mayor que cero (RN-04).</summary>
    public int CuposTotales { get; internal set; }

    /// <summary>Identificador persistido del estado del evento.</summary>
    public EstadoEvento Estado { get; internal set; }

    /// <summary>Fecha y hora en la que se registró el evento.</summary>
    public DateTime FechaRegistro { get; internal set; }

    /// <summary>Identificador del responsable que administra el evento.</summary>
    public long ResponsableId { get; internal set; }

    /// <summary>Identificador de la categoría del evento.</summary>
    public long CategoriaId { get; internal set; }

    /// <summary>Versión de la fila, usada para control de concurrencia optimista.</summary>
    public byte[]? Version { get; internal set; }

    /// <summary>Responsable que administra el evento.</summary>
    public Responsable? Responsable { get; internal set; }

    /// <summary>Categoría a la que pertenece el evento.</summary>
    public Categoria? Categoria { get; internal set; }

    /// <summary>Inscripciones registradas en el evento.</summary>
    public ICollection<Inscripcion> Inscripciones { get; } = new List<Inscripcion>();

    /// <summary>
    /// Comportamiento vigente del evento. Se reconstruye a partir del estado persistido,
    /// de modo que el patrón State sobrevive a la recuperación desde SQL Server.
    /// </summary>
    [NotMapped]
    public IEventoEstado EstadoActual => _estadoActual ??= FabricaEstadosEvento.Crear(Estado);

    /// <summary>True si el evento puede mostrarse en el listado de eventos del estudiante.</summary>
    public bool EstaDisponible() => EstadoActual.EstaDisponible;

    /// <summary>True si el evento admite nuevas inscripciones.</summary>
    public bool AdmiteInscripciones() => EstadoActual.AdmiteInscripciones;

    /// <summary>Cupos que quedan disponibles según las inscripciones activas.</summary>
    public int GetCuposDisponibles()
    {
        var activos = Inscripciones.Count(i => i.EstaActiva());
        return Math.Max(0, CuposTotales - activos);
    }

    /// <summary>True cuando todavía hay al menos un cupo disponible.</summary>
    public bool TieneCuposDisponibles() => GetCuposDisponibles() > 0;

    /// <summary>True cuando el estudiante ya tiene una inscripción activa en el evento.</summary>
    public bool EstaInscrito(Estudiante estudiante)
        => Inscripciones.Any(i => i.EstudianteId == estudiante.Id && i.EstaActiva());

    /// <summary>True cuando la fecha de inicio es posterior a la fecha de registro (RN-03).</summary>
    public bool ValidarFechaInicio() => FechaHoraInicio > FechaRegistro;

    /// <summary>True cuando el cupo total es mayor que cero (RN-04).</summary>
    public bool ValidarCupos() => CuposTotales > 0;

    /// <summary>True cuando el estado vigente permite cancelar inscripciones.</summary>
    public bool AdmiteCancelacion() => EstadoActual.PermiteCancelarInscripcion;

    /// <summary>
    /// True cuando el estudiante puede cancelar su inscripción (RN-07). La cancelación
    /// se permite mientras el evento no haya comenzado: un evento en curso, finalizado o
    /// cancelado ya no admite cambios de participación. Un evento que cerró sus
    /// inscripciones por falta de cupos sigue admitiendo cancelaciones, porque el
    /// estudiante que ocupa el último cupo no puede quedar bloqueado.
    /// </summary>
    public bool PuedeCancelarInscripcion(DateTime ahora)
        => EstadoActual.PermiteCancelarInscripcion
           && ahora < FechaHoraInicio
           && Estado != EstadoEvento.Cancelado;

    /// <summary>
    /// Retira el evento de la actividad operativa del sistema: pasa al estado
    /// <see cref="EstadoEvento.Cancelado"/> y cancela sus inscripciones activas.
    /// El historial de inscripciones y asistencias se conserva; lo que desaparece es la
    /// condición de inscripción vigente.
    /// </summary>
    /// <param name="momento">Instante en que se registra la cancelación.</param>
    /// <returns>Las inscripciones que quedaron canceladas.</returns>
    public List<Inscripcion> Retirar(DateTime momento)
    {
        if (Estado == EstadoEvento.Cancelado)
        {
            throw new ExcepcionNegocio("El evento ya fue retirado.");
        }

        // El estado Cancelado es terminal: la transición lanza si el evento ya estaba retirado.
        Transicionar(TransicionEvento.Cancelar);

        var canceladas = new List<Inscripcion>();

        foreach (var inscripcion in Inscripciones.Where(i => i.EstaActiva()))
        {
            inscripcion.Cancelar(momento);
            canceladas.Add(inscripcion);
        }

        return canceladas;
    }

    /// <summary>Agrega una inscripción ya validada al evento.</summary>
    internal void AgregarInscripcion(Inscripcion inscripcion) => Inscripciones.Add(inscripcion);

    /// <summary>
    /// Registra la inscripción de un estudiante. La validación se delega en el estado
    /// vigente del evento (patrón State), que comprueba cupos y duplicidad.
    /// </summary>
    public void RegistrarInscripcion(Estudiante estudiante) => EstadoActual.Inscribir(this, estudiante);

    /// <summary>
    /// Aplica una transición del ciclo de vida, delegando en el objeto de estado.
    /// </summary>
    public void Transicionar(TransicionEvento transicion)
    {
        _estadoActual = EstadoActual.Transicionar(transicion);
        Estado = _estadoActual.Estado;
    }

    /// <summary>
    /// Recalcula el estado del evento según la hora actual y los cupos disponibles
    /// (patrón State: cada estado decide si exige una transición automática).
    /// </summary>
    public void ActualizarEstado(DateTime ahora, int cuposDisponibles)
    {
        // El número de iteraciones acota el ciclo ante cualquier estado inconsistente.
        for (var i = 0; i < 8; i++)
        {
            var transicion = EstadoActual.TransicionAutomatica(this, ahora, cuposDisponibles);
            if (transicion is null)
            {
                return;
            }

            Transicionar(transicion.Value);
        }
    }

    /// <summary>
    /// Copia sobre el evento actual los datos ya validados de otro evento. Se usa en la
    /// modificación de eventos, donde el <see cref="EventoBuilder"/> produce la versión
    /// validada de los datos.
    /// </summary>
    internal void AplicarDatosDe(Evento datosValidados)
    {
        var cambiaronFechas = FechaHoraInicio != datosValidados.FechaHoraInicio
            || FechaHoraFin != datosValidados.FechaHoraFin;

        Nombre = datosValidados.Nombre;
        Descripcion = datosValidados.Descripcion;
        FechaHoraInicio = datosValidados.FechaHoraInicio;
        FechaHoraFin = datosValidados.FechaHoraFin;
        Lugar = datosValidados.Lugar;
        CuposTotales = datosValidados.CuposTotales;
        CategoriaId = datosValidados.CategoriaId;
        ResponsableId = datosValidados.ResponsableId;

        if (cambiaronFechas && Estado != EstadoEvento.Cancelado)
        {
            // Al cambiar las fechas se reinicia el ciclo de vida: el estado se recalcula
            // automáticamente a partir del nuevo rango de fechas. Un evento retirado es
            // terminal y no puede reabrirse por un cambio de fecha.
            _estadoActual = FabricaEstadosEvento.Crear(EstadoEvento.Programado);
            Estado = EstadoEvento.Programado;
        }
    }
}
