using ApiGestionAcademico.Excepciones;
using ApiGestionAcademico.Modelos;
using ApiGestionAcademico.Peticiones;
using ApiGestionAcademico.Repositorios;
using ApiGestionAcademico.Servicios;
using Pruebas.ApiGestionAcademico.Dobles;
using Xunit;

namespace Pruebas.ApiGestionAcademico;

/// <summary>
/// Pruebas de la retirada de eventos: el evento pasa al estado Cancelado (terminal en el
/// patrón State), sus inscripciones activas dejan de estar activas y el historial se
/// conserva. Solo el responsable que administra el evento puede ejecutarla.
/// </summary>
public class RetiroEventoTests
{
    private sealed class RepositorioCategoriaFalso : IRepositorioCategoria
    {
        private readonly List<Categoria> _categorias = new() { new Categoria("Seminarios", "Seminarios academicos") { Id = 1 } };

        public Task<List<Categoria>> ListarAsync(CancellationToken cancellationToken) => Task.FromResult(_categorias);

        public Task<Categoria?> ObtenerPorIdAsync(long id, CancellationToken cancellationToken)
            => Task.FromResult(_categorias.FirstOrDefault(c => c.Id == id));
    }

    private sealed class Escenario
    {
        public AlmacenFalso Almacen { get; } = new();
        public RepositorioEventoFalso Eventos { get; }
        public RepositorioInscripcionFalso Inscripciones { get; }
        public RepositorioUsuarioFalso Usuarios { get; }
        public RepositorioCategoriaFalso Categorias { get; } = new();
        public UnidadTransaccionalFalsa Unidad { get; } = new();
        public Estudiante Estudiante { get; }
        public Responsable Responsable { get; }
        public Responsable OtroResponsable { get; }
        public Evento Evento { get; }
        public ServicioEventos Servicio { get; }

        public Escenario()
        {
            Eventos = new RepositorioEventoFalso(Almacen);
            Inscripciones = new RepositorioInscripcionFalso(Almacen);
            Usuarios = new RepositorioUsuarioFalso(Almacen);

            Estudiante = new Estudiante("Ana Gomez", "estudiante@uni.edu.co", "hash", "EST001", "Ingenieria") { Id = 1 };
            Responsable = new Responsable("Carlos Rios", "responsable@uni.edu.co", "hash", "Facultad", "Coordinador") { Id = 2 };
            OtroResponsable = new Responsable("Maria Lopez", "coordinacion@uni.edu.co", "hash", "Direccion", "Directora") { Id = 3 };
            Almacen.Usuarios[Estudiante.Id] = Estudiante;
            Almacen.Usuarios[Responsable.Id] = Responsable;
            Almacen.Usuarios[OtroResponsable.Id] = OtroResponsable;

            Evento = new EventoBuilder()
                .ConNombre("Seminario de prueba")
                .ConDescripcion("Evento utilizado en las pruebas de retirada.")
                .ConFechas(DateTime.Now.AddDays(5), DateTime.Now.AddDays(5).AddHours(2))
                .ConLugar("Auditorio Principal")
                .ConCupos(5)
                .ConCategoria(new Categoria("Seminarios", "Seminarios academicos") { Id = 1 })
                .ConResponsable(Responsable)
                .ConFechaRegistro(DateTime.Now)
                .Build();

            Evento.Id = 10;
            Almacen.Eventos[Evento.Id] = Evento;

            Servicio = new ServicioEventos(Eventos, Categorias, Usuarios, Inscripciones, Unidad, new RelojFalso(DateTimeOffset.Now));
        }

        public Inscripcion Inscribir(Estudiante? estudiante = null)
        {
            var inscripcion = new Inscripcion(Evento, estudiante ?? Estudiante, DateTime.Now) { Id = 100 + Almacen.Inscripciones.Count };
            Evento.Inscripciones.Add(inscripcion);
            Almacen.Inscripciones[inscripcion.Id] = inscripcion;
            return inscripcion;
        }
    }

    [Fact]
    public async Task ElResponsableAutorizadoPuedeRetirarSuEvento()
    {
        var escenario = new Escenario();
        var inscripcion = escenario.Inscribir();

        var respuesta = await escenario.Servicio.RetirarAsync(escenario.Evento.Id, escenario.Responsable.Id, CancellationToken.None);

        Assert.Equal(EstadoEvento.Cancelado, respuesta.Estado);
        Assert.Equal(1, respuesta.InscripcionesCanceladas);
        Assert.Equal(EstadoEvento.Cancelado, escenario.Evento.Estado);

        // La inscripción activa deja de estar activa, pero se conserva el registro.
        Assert.False(inscripcion.EstaActiva());
        Assert.Equal(EstadoInscripcion.Cancelada, inscripcion.Estado);
        Assert.NotNull(inscripcion.FechaHoraCancelacion);
        Assert.True(escenario.Almacen.Inscripciones.ContainsKey(inscripcion.Id), "El historial debe conservarse");
    }

    [Fact]
    public async Task UnResponsableNoPuedeRetirarEventosDeOtroResponsable()
    {
        var escenario = new Escenario();

        var excepcion = await Assert.ThrowsAsync<ExcepcionAccesoProhibido>(() =>
            escenario.Servicio.RetirarAsync(escenario.Evento.Id, escenario.OtroResponsable.Id, CancellationToken.None));

        Assert.Contains("administra", excepcion.Message, StringComparison.OrdinalIgnoreCase);
        Assert.NotEqual(EstadoEvento.Cancelado, escenario.Evento.Estado);
    }

    [Fact]
    public async Task UnEventoRetiradoNoSePuedeRetirarDeNuevo()
    {
        var escenario = new Escenario();
        await escenario.Servicio.RetirarAsync(escenario.Evento.Id, escenario.Responsable.Id, CancellationToken.None);

        var excepcion = await Assert.ThrowsAsync<ExcepcionNegocio>(() =>
            escenario.Servicio.RetirarAsync(escenario.Evento.Id, escenario.Responsable.Id, CancellationToken.None));

        Assert.Contains("ya fue retirado", excepcion.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UnEventoRetiradoNoAdmiteModificaciones()
    {
        var escenario = new Escenario();
        await escenario.Servicio.RetirarAsync(escenario.Evento.Id, escenario.Responsable.Id, CancellationToken.None);

        var excepcion = await Assert.ThrowsAsync<ExcepcionNegocio>(() =>
            escenario.Servicio.ActualizarAsync(escenario.Evento.Id, new PeticionEventoDto
            {
                Nombre = "Seminario de prueba",
                Descripcion = "Intento de modificación de un evento retirado.",
                FechaHoraInicio = DateTime.Now.AddDays(6),
                FechaHoraFin = DateTime.Now.AddDays(6).AddHours(2),
                Lugar = "Auditorio",
                CuposTotales = 5,
                CategoriaId = 1
            }, escenario.Responsable.Id, CancellationToken.None));

        Assert.Contains("retirado", excepcion.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(EstadoEvento.Cancelado, escenario.Evento.Estado);
    }

    [Fact]
    public async Task UnEventoRetiradoNoSeReabreAlCambiarLasFechas()
    {
        // El estado Cancelado es terminal: el patrón State no permite reactivarlo.
        var escenario = new Escenario();
        await escenario.Servicio.RetirarAsync(escenario.Evento.Id, escenario.Responsable.Id, CancellationToken.None);

        var validado = new EventoBuilder()
            .ConNombre("Seminario de prueba")
            .ConDescripcion("Intento de reapertura mediante un cambio de fechas.")
            .ConFechas(DateTime.Now.AddDays(30), DateTime.Now.AddDays(30).AddHours(2))
            .ConLugar("Auditorio")
            .ConCupos(5)
            .ConCategoria(new Categoria("Seminarios", "Seminarios academicos") { Id = 1 })
            .ConResponsable(escenario.Responsable)
            .ConFechaRegistro(escenario.Evento.FechaRegistro)
            .Build();

        escenario.Evento.AplicarDatosDe(validado);

        Assert.Equal(EstadoEvento.Cancelado, escenario.Evento.Estado);
        Assert.False(escenario.Evento.EstaDisponible());
        Assert.False(escenario.Evento.AdmiteInscripciones());
    }

    [Fact]
    public async Task UnEventoRetiradoNoAdmiteNuevasInscripciones()
    {
        var escenario = new Escenario();
        await escenario.Servicio.RetirarAsync(escenario.Evento.Id, escenario.Responsable.Id, CancellationToken.None);

        var excepcion = Assert.Throws<ExcepcionNegocio>(() => escenario.Evento.RegistrarInscripcion(escenario.Estudiante));
        Assert.Contains("cancelado", excepcion.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LaRetiradaConservaLasInscripcionesHistoricas()
    {
        var escenario = new Escenario();
        var activa = escenario.Inscribir();

        var historica = new Inscripcion(escenario.Evento, escenario.Estudiante, DateTime.Now) { Id = 999 };
        historica.Cancelar(DateTime.Now.AddMinutes(-5));
        escenario.Evento.Inscripciones.Add(historica);
        escenario.Almacen.Inscripciones[historica.Id] = historica;

        await escenario.Servicio.RetirarAsync(escenario.Evento.Id, escenario.Responsable.Id, CancellationToken.None);

        // Ambas siguen existiendo: la activa queda cancelada y la histórica no revive.
        Assert.Equal(2, escenario.Almacen.Inscripciones.Count);
        Assert.Equal(EstadoInscripcion.Cancelada, activa.Estado);
        Assert.Equal(EstadoInscripcion.Cancelada, historica.Estado);
        // Tras la retirada no queda ninguna inscripción activa: la histórica no revive.
        Assert.Equal(0, respuestaInscripcionesActivas(escenario));
    }

    [Fact]
    public void RetirarUnEventoYaRetiradoLanzaExcepcion()
    {
        var escenario = new Escenario();
        escenario.Evento.Estado = EstadoEvento.Cancelado;

        var excepcion = Assert.Throws<ExcepcionNegocio>(() => escenario.Evento.Retirar(DateTime.Now));
        Assert.Contains("ya fue retirado", excepcion.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static int respuestaInscripcionesActivas(Escenario escenario)
        => escenario.Almacen.Inscripciones.Values.Count(i => i.EstaActiva());
}
