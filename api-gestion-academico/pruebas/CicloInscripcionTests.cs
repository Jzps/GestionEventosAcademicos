using ApiGestionAcademico.Excepciones;
using ApiGestionAcademico.Modelos;
using ApiGestionAcademico.Servicios;
using Pruebas.ApiGestionAcademico.Dobles;
using Xunit;

namespace Pruebas.ApiGestionAcademico;

/// <summary>
/// Pruebas del ciclo de vida de la inscripción: activa, cancelada y nueva inscripción
/// tras una cancelación (RN-02 se refiere a las inscripciones activas).
/// </summary>
public class CicloInscripcionTests
{
    private sealed class Escenario
    {
        public AlmacenFalso Almacen { get; } = new();
        public RepositorioEventoFalso Eventos { get; }
        public RepositorioInscripcionFalso Inscripciones { get; }
        public RepositorioUsuarioFalso Usuarios { get; }
        public UnidadTransaccionalFalsa Unidad { get; } = new();
        public Estudiante Estudiante { get; }
        public Responsable Responsable { get; }
        public Evento Evento { get; }
        public InscripcionFacade Fachada { get; }

        public Escenario(int cupos = 5)
        {
            Eventos = new RepositorioEventoFalso(Almacen);
            Inscripciones = new RepositorioInscripcionFalso(Almacen);
            Usuarios = new RepositorioUsuarioFalso(Almacen);

            Estudiante = new Estudiante("Ana Gomez", "estudiante@uni.edu.co", "hash", "EST001", "Ingenieria") { Id = 1 };
            Responsable = new Responsable("Carlos Rios", "responsable@uni.edu.co", "hash", "Facultad", "Coordinador") { Id = 2 };
            Almacen.Usuarios[Estudiante.Id] = Estudiante;
            Almacen.Usuarios[Responsable.Id] = Responsable;

            Evento = new EventoBuilder()
                .ConNombre("Seminario de prueba")
                .ConDescripcion("Evento utilizado en las pruebas unitarias del prototipo.")
                .ConFechas(DateTime.Now.AddDays(5), DateTime.Now.AddDays(5).AddHours(2))
                .ConLugar("Auditorio Principal")
                .ConCupos(cupos)
                .ConCategoria(new Categoria("Seminarios", "Seminarios academicos") { Id = 1 })
                .ConResponsable(Responsable)
                .ConFechaRegistro(DateTime.Now)
                .Build();

            Evento.Id = 10;
            Almacen.Eventos[Evento.Id] = Evento;

            Fachada = new InscripcionFacade(Usuarios, Eventos, Inscripciones, Unidad, new RelojFalso(DateTimeOffset.Now));
        }
    }

    [Fact]
    public async Task LaInscripcionActivaBloqueaUnaSegundaInscripcion()
    {
        var escenario = new Escenario();
        await escenario.Fachada.InscribirseAsync(escenario.Estudiante.Id, escenario.Evento.Id, CancellationToken.None);

        var excepcion = await Assert.ThrowsAsync<ExcepcionNegocio>(() =>
            escenario.Fachada.InscribirseAsync(escenario.Estudiante.Id, escenario.Evento.Id, CancellationToken.None));

        Assert.Contains("inscrito", excepcion.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Single(escenario.Almacen.Inscripciones);
    }

    [Fact]
    public async Task UnaInscripcionCanceladaNoBloqueaUnaNuevaInscripcion()
    {
        // RN-02 se refiere a las inscripciones activas: el historial cancelado no impide
        // volver a inscribirse mientras el evento admita inscripciones y tenga cupos.
        var escenario = new Escenario();
        var primera = await escenario.Fachada.InscribirseAsync(escenario.Estudiante.Id, escenario.Evento.Id, CancellationToken.None);

        await escenario.Fachada.CancelarAsync(primera.Id, escenario.Estudiante.Id, CancellationToken.None);

        var segunda = await escenario.Fachada.InscribirseAsync(escenario.Estudiante.Id, escenario.Evento.Id, CancellationToken.None);

        Assert.NotEqual(primera.Id, segunda.Id);
        Assert.Equal(EstadoInscripcion.Activa, segunda.Estado);

        // El historial se conserva: la inscripción cancelada sigue existiendo.
        var cancelada = escenario.Almacen.Inscripciones[primera.Id];
        Assert.Equal(EstadoInscripcion.Cancelada, cancelada.Estado);
        Assert.NotNull(cancelada.FechaHoraCancelacion);
        Assert.Equal(2, escenario.Almacen.Inscripciones.Count);

        // Solo existe una inscripción activa para el evento.
        var activas = escenario.Almacen.Inscripciones.Values
            .Count(i => i.EventoId == escenario.Evento.Id && i.EstaActiva());
        Assert.Equal(1, activas);
    }

    [Fact]
    public async Task LosCuposSeLiberanAlCancelarYSeConsumenAlReinscribir()
    {
        var escenario = new Escenario(cupos: 1);
        var primera = await escenario.Fachada.InscribirseAsync(escenario.Estudiante.Id, escenario.Evento.Id, CancellationToken.None);

        Assert.Equal(0, escenario.Evento.GetCuposDisponibles());
        Assert.Equal(EstadoEvento.InscripcionesCerradas, escenario.Evento.Estado);

        await escenario.Fachada.CancelarAsync(primera.Id, escenario.Estudiante.Id, CancellationToken.None);

        // Tras cancelar, el cupo vuelve a estar disponible.
        var trasCancelar = await escenario.Eventos.ObtenerPorIdAsync(escenario.Evento.Id, CancellationToken.None);
        Assert.NotNull(trasCancelar);
        Assert.Equal(1, trasCancelar!.GetCuposDisponibles());

        await escenario.Fachada.InscribirseAsync(escenario.Estudiante.Id, escenario.Evento.Id, CancellationToken.None);

        var trasReinscribir = await escenario.Eventos.ObtenerPorIdAsync(escenario.Evento.Id, CancellationToken.None);
        Assert.NotNull(trasReinscribir);
        Assert.Equal(0, trasReinscribir!.GetCuposDisponibles());
    }

    [Fact]
    public async Task LaConsultaDeInscripcionesActivasExcluyeLasCanceladas()
    {
        var escenario = new Escenario();
        var primera = await escenario.Fachada.InscribirseAsync(escenario.Estudiante.Id, escenario.Evento.Id, CancellationToken.None);
        await escenario.Fachada.CancelarAsync(primera.Id, escenario.Estudiante.Id, CancellationToken.None);

        var activas = await escenario.Inscripciones.ObtenerActivaAsync(escenario.Estudiante.Id, escenario.Evento.Id, CancellationToken.None);
        Assert.Null(activas);

        var todas = await escenario.Inscripciones.ListarPorEstudianteAsync(escenario.Estudiante.Id, CancellationToken.None);
        Assert.Single(todas);
        Assert.Equal(EstadoInscripcion.Cancelada, todas[0].Estado);
    }
}
