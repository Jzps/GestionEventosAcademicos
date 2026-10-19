using ApiGestionAcademico.Excepciones;
using ApiGestionAcademico.Modelos;
using ApiGestionAcademico.Servicios;
using Pruebas.ApiGestionAcademico.Dobles;
using Xunit;

namespace Pruebas.ApiGestionAcademico;

/// <summary>
/// Pruebas del patrón Facade: la inscripción debe aplicarse siempre con el mismo orden
/// de validaciones y respecting las reglas RN-01, RN-02, RN-05 y RN-08.
/// </summary>
public class InscripcionFacadeTests
{
    private sealed class Escenario
    {
        public AlmacenFalso Almacen { get; } = new();
        public RepositorioEventoFalso Eventos { get; }
        public RepositorioInscripcionFalso Inscripciones { get; }
        public RepositorioUsuarioFalso Usuarios { get; }
        public UnidadTransaccionalFalsa Unidad { get; } = new();
        public Estudiante Estudiante { get; }
        public Estudiante OtroEstudiante { get; }
        public Responsable Responsable { get; }
        public Evento Evento { get; }
        public InscripcionFacade Fachada { get; }

        public Escenario(int cupos = 10, EstadoEvento? estado = null)
        {
            Eventos = new RepositorioEventoFalso(Almacen);
            Inscripciones = new RepositorioInscripcionFalso(Almacen);
            Usuarios = new RepositorioUsuarioFalso(Almacen);

            Estudiante = new Estudiante("Ana Gomez", "estudiante@uni.edu.co", "hash", "EST001", "Ingenieria");
            OtroEstudiante = new Estudiante("Luis Perez", "otro@uni.edu.co", "hash", "EST002", "Contaduria");
            Responsable = new Responsable("Carlos Rios", "responsable@uni.edu.co", "hash", "Facultad", "Coordinador");

            Estudiante.Id = 1;
            OtroEstudiante.Id = 2;
            Responsable.Id = 3;
            Almacen.Usuarios[Estudiante.Id] = Estudiante;
            Almacen.Usuarios[OtroEstudiante.Id] = OtroEstudiante;
            Almacen.Usuarios[Responsable.Id] = Responsable;

            var categoria = new Categoria("Seminarios", "Seminarios academicos") { Id = 1 };

            Evento = new EventoBuilder()
                .ConNombre("Seminario de prueba")
                .ConDescripcion("Evento utilizado en las pruebas unitarias del prototipo.")
                .ConFechas(DateTime.Now.AddDays(5), DateTime.Now.AddDays(5).AddHours(2))
                .ConLugar("Auditorio Principal")
                .ConCupos(cupos)
                .ConCategoria(categoria)
                .ConResponsable(Responsable)
                .ConFechaRegistro(DateTime.Now)
                .Build();

            Evento.Id = 10;
            if (estado is not null)
            {
                Evento.Estado = estado.Value;
            }

            Almacen.Eventos[Evento.Id] = Evento;

            Fachada = new InscripcionFacade(Usuarios, Eventos, Inscripciones, Unidad, new RelojFalso(DateTimeOffset.Now));
        }
    }

    [Fact]
    public async Task InscribeCuandoSeCumplenLasCondiciones()
    {
        var escenario = new Escenario();

        var inscripcion = await escenario.Fachada.InscribirseAsync(escenario.Estudiante.Id, escenario.Evento.Id, CancellationToken.None);

        Assert.Equal(escenario.Estudiante.Id, inscripcion.EstudianteId);
        Assert.Equal(escenario.Evento.Id, inscripcion.EventoId);
        Assert.Equal(EstadoInscripcion.Activa, inscripcion.Estado);
        Assert.True(inscripcion.EstaActiva());
        Assert.Single(escenario.Almacen.Inscripciones);
    }

    [Fact]
    public async Task BloqueaElEventoDuranteLaInscripcion()
    {
        var escenario = new Escenario();

        await escenario.Fachada.InscribirseAsync(escenario.Estudiante.Id, escenario.Evento.Id, CancellationToken.None);

        Assert.Contains("evento:10", escenario.Unidad.RecursosBloqueados);
    }

    [Fact]
    public async Task RechazaInscripcionSinCupos()
    {
        // RN-01: no se puede inscribirse cuando no hay cupos disponibles.
        var escenario = new Escenario(cupos: 1);

        var inscripcionExistente = new Inscripcion(escenario.Evento, escenario.OtroEstudiante, DateTime.Now);
        inscripcionExistente.Id = 100;
        escenario.Almacen.Inscripciones[inscripcionExistente.Id] = inscripcionExistente;

        var excepcion = await Assert.ThrowsAsync<ExcepcionNegocio>(() =>
            escenario.Fachada.InscribirseAsync(escenario.Estudiante.Id, escenario.Evento.Id, CancellationToken.None));

        Assert.Contains("cupos", excepcion.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Single(escenario.Almacen.Inscripciones);
    }

    [Fact]
    public async Task RechazaInscripcionDuplicada()
    {
        // RN-02: un estudiante no puede tener más de una inscripción activa en el mismo evento.
        var escenario = new Escenario();

        await escenario.Fachada.InscribirseAsync(escenario.Estudiante.Id, escenario.Evento.Id, CancellationToken.None);

        var excepcion = await Assert.ThrowsAsync<ExcepcionNegocio>(() =>
            escenario.Fachada.InscribirseAsync(escenario.Estudiante.Id, escenario.Evento.Id, CancellationToken.None));

        Assert.Contains("inscrito", excepcion.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Single(escenario.Almacen.Inscripciones);
    }

    [Fact]
    public async Task CierraLasInscripcionesCuandoSeAgotanLosCupos()
    {
        // RN-08: la cantidad de inscritos no puede superar los cupos definidos.
        var escenario = new Escenario(cupos: 1);

        await escenario.Fachada.InscribirseAsync(escenario.Estudiante.Id, escenario.Evento.Id, CancellationToken.None);

        Assert.Equal(EstadoEvento.InscripcionesCerradas, escenario.Evento.Estado);
        Assert.Equal(0, escenario.Evento.GetCuposDisponibles());
    }

    [Fact]
    public async Task RechazaInscripcionEnUnEventoQueNoAdmiteInscripciones()
    {
        // RN-05: no se admiten inscripciones una vez iniciado o cerrado el evento.
        var escenario = new Escenario(estado: EstadoEvento.Finalizado);

        var excepcion = await Assert.ThrowsAsync<ExcepcionNegocio>(() =>
            escenario.Fachada.InscribirseAsync(escenario.Estudiante.Id, escenario.Evento.Id, CancellationToken.None));

        Assert.Contains("finalizado", excepcion.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RechazaInscripcionesDeUnResponsable()
    {
        // Los roles no pueden ejecutar operaciones que no les corresponden.
        var escenario = new Escenario();

        var excepcion = await Assert.ThrowsAsync<ExcepcionAccesoProhibido>(() =>
            escenario.Fachada.InscribirseAsync(escenario.Responsable.Id, escenario.Evento.Id, CancellationToken.None));

        Assert.Contains("estudiante", excepcion.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RechazaLaCancelacionCuandoElEventoNoLoPermite()
    {
        // RN-07: no se puede cancelar cuando el evento ya comenzó (estado EnCurso).
        var escenario = new Escenario(estado: EstadoEvento.EnCurso);

        var inscripcion = new Inscripcion(escenario.Evento, escenario.Estudiante, DateTime.Now);
        inscripcion.Id = 200;
        escenario.Almacen.Inscripciones[inscripcion.Id] = inscripcion;

        var excepcion = await Assert.ThrowsAsync<ExcepcionNegocio>(() =>
            escenario.Fachada.CancelarAsync(inscripcion.Id, escenario.Estudiante.Id, CancellationToken.None));

        Assert.Contains("no permite cancelar", excepcion.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CancelaLaInscripcionCuandoElEventoLoPermite()
    {
        var escenario = new Escenario();

        var inscripcion = new Inscripcion(escenario.Evento, escenario.Estudiante, DateTime.Now);
        inscripcion.Id = 300;
        escenario.Almacen.Inscripciones[inscripcion.Id] = inscripcion;

        var resultado = await escenario.Fachada.CancelarAsync(inscripcion.Id, escenario.Estudiante.Id, CancellationToken.None);

        Assert.Equal(EstadoInscripcion.Cancelada, resultado.Estado);
        Assert.NotNull(resultado.FechaHoraCancelacion);
    }
}
