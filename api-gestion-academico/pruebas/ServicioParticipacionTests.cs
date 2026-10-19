using ApiGestionAcademico.Excepciones;
using ApiGestionAcademico.Modelos;
using ApiGestionAcademico.Peticiones;
using ApiGestionAcademico.Servicios;
using Pruebas.ApiGestionAcademico.Dobles;
using Xunit;

namespace Pruebas.ApiGestionAcademico;

/// <summary>
/// Pruebas del servicio de participantes y asistencia. La regla RN-06 exige que solo se
/// registre asistencia para estudiantes que tengan una inscripción en el evento.
/// </summary>
public class ServicioParticipacionTests
{
    private sealed class Escenario
    {
        public AlmacenFalso Almacen { get; } = new();
        public RepositorioEventoFalso Eventos { get; }
        public RepositorioInscripcionFalso Inscripciones { get; }
        public RepositorioAsistenciaFalso Asistencias { get; }
        public UnidadTransaccionalFalsa Unidad { get; } = new();
        public Estudiante Estudiante { get; }
        public Responsable Responsable { get; }
        public Responsable OtroResponsable { get; }
        public Evento Evento { get; }
        public ServicioParticipacion Servicio { get; }

        public Escenario()
        {
            Eventos = new RepositorioEventoFalso(Almacen);
            Inscripciones = new RepositorioInscripcionFalso(Almacen);
            Asistencias = new RepositorioAsistenciaFalso(Almacen);

            Estudiante = new Estudiante("Ana Gomez", "estudiante@uni.edu.co", "hash", "EST001", "Ingenieria") { Id = 1 };
            Responsable = new Responsable("Carlos Rios", "responsable@uni.edu.co", "hash", "Facultad", "Coordinador") { Id = 2 };
            OtroResponsable = new Responsable("Maria Lopez", "coordinacion@uni.edu.co", "hash", "Direccion", "Directora") { Id = 3 };

            Almacen.Usuarios[Estudiante.Id] = Estudiante;
            Almacen.Usuarios[Responsable.Id] = Responsable;
            Almacen.Usuarios[OtroResponsable.Id] = OtroResponsable;

            Evento = new Evento
            {
                Id = 20,
                Nombre = "Seminario de prueba",
                Descripcion = "Evento utilizado en las pruebas unitarias del prototipo.",
                Lugar = "Auditorio",
                CuposTotales = 10,
                FechaRegistro = DateTime.Now.AddDays(-5),
                FechaHoraInicio = DateTime.Now.AddDays(5),
                FechaHoraFin = DateTime.Now.AddDays(5).AddHours(2),
                Estado = EstadoEvento.InscripcionesAbiertas,
                ResponsableId = Responsable.Id,
                CategoriaId = 1
            };

            Almacen.Eventos[Evento.Id] = Evento;

            Servicio = new ServicioParticipacion(Eventos, Inscripciones, Asistencias, Unidad, new RelojFalso(DateTimeOffset.Now));
        }

        public Inscripcion CrearInscripcion(EstadoInscripcion estado = EstadoInscripcion.Activa, long id = 500)
        {
            var inscripcion = new Inscripcion(Evento, Estudiante, DateTime.Now) { Id = id };
            inscripcion.Estado = estado;
            Almacen.Inscripciones[inscripcion.Id] = inscripcion;
            return inscripcion;
        }
    }

    [Fact]
    public async Task RegistraLaAsistenciaDeUnParticipanteInscrito()
    {
        var escenario = new Escenario();
        var inscripcion = escenario.CrearInscripcion();

        var registro = await escenario.Servicio.RegistrarAsistenciaAsync(
            new PeticionAsistenciaDto { InscripcionId = inscripcion.Id, Asistio = true },
            escenario.Responsable.Id,
            CancellationToken.None);

        Assert.True(registro.Asistio);
        Assert.Equal(inscripcion.Id, registro.InscripcionId);
        Assert.Single(escenario.Almacen.Asistencias);
    }

    [Fact]
    public async Task ActualizaLaAsistenciaCuandoYaExisteUnRegistro()
    {
        var escenario = new Escenario();
        var inscripcion = escenario.CrearInscripcion();

        await escenario.Servicio.RegistrarAsistenciaAsync(
            new PeticionAsistenciaDto { InscripcionId = inscripcion.Id, Asistio = true },
            escenario.Responsable.Id,
            CancellationToken.None);

        var registro = await escenario.Servicio.RegistrarAsistenciaAsync(
            new PeticionAsistenciaDto { InscripcionId = inscripcion.Id, Asistio = false, Observacion = "No se presento" },
            escenario.Responsable.Id,
            CancellationToken.None);

        Assert.False(registro.Asistio);
        Assert.Equal("No se presento", registro.Observacion);
        Assert.Single(escenario.Almacen.Asistencias);
    }

    [Fact]
    public async Task GuardaYActualizaLaObservacionDeAsistencia()
    {
        // La observación debe persistir y poder modificarse sin crear registros duplicados.
        var escenario = new Escenario();
        var inscripcion = escenario.CrearInscripcion();

        var registro = await escenario.Servicio.RegistrarAsistenciaAsync(
            new PeticionAsistenciaDto { InscripcionId = inscripcion.Id, Asistio = true, Observacion = "Llegó tarde" },
            escenario.Responsable.Id,
            CancellationToken.None);

        Assert.Equal("Llegó tarde", registro.Observacion);
        var almacenado = Assert.Single(escenario.Almacen.Asistencias.Values);
        Assert.Equal("Llegó tarde", almacenado.Observacion);

        var actualizado = await escenario.Servicio.RegistrarAsistenciaAsync(
            new PeticionAsistenciaDto { InscripcionId = inscripcion.Id, Asistio = true, Observacion = "Ingresó con el grupo B" },
            escenario.Responsable.Id,
            CancellationToken.None);

        Assert.Equal("Ingresó con el grupo B", actualizado.Observacion);
        Assert.Single(escenario.Almacen.Asistencias);
        Assert.Equal("Ingresó con el grupo B", escenario.Almacen.Asistencias.Values.First().Observacion);
    }

    [Fact]
    public async Task LaObservacionSeRecuperaAlConsultarLaAsistencia()
    {
        var escenario = new Escenario();
        var inscripcion = escenario.CrearInscripcion();

        await escenario.Servicio.RegistrarAsistenciaAsync(
            new PeticionAsistenciaDto { InscripcionId = inscripcion.Id, Asistio = true, Observacion = "Observación persistida" },
            escenario.Responsable.Id,
            CancellationToken.None);

        var registros = await escenario.Servicio.ListarAsistenciaAsync(escenario.Evento.Id, escenario.Responsable.Id, CancellationToken.None);

        var registro = Assert.Single(registros);
        Assert.Equal("Observación persistida", registro.Observacion);
        Assert.True(registro.Asistio);
    }

    [Fact]
    public async Task ExigeUnaInscripcionExistenteParaRegistrarAsistencia()
    {
        // RN-06: solo se registra asistencia para estudiantes inscritos en el evento.
        var escenario = new Escenario();

        await Assert.ThrowsAsync<ExcepcionNoEncontrado>(() => escenario.Servicio.RegistrarAsistenciaAsync(
            new PeticionAsistenciaDto { InscripcionId = 999, Asistio = true },
            escenario.Responsable.Id,
            CancellationToken.None));
    }

    [Fact]
    public async Task ExigeUnaInscripcionActivaParaRegistrarAsistencia()
    {
        var escenario = new Escenario();
        var inscripcion = escenario.CrearInscripcion(EstadoInscripcion.Cancelada);

        await Assert.ThrowsAsync<ExcepcionNegocio>(() => escenario.Servicio.RegistrarAsistenciaAsync(
            new PeticionAsistenciaDto { InscripcionId = inscripcion.Id, Asistio = true },
            escenario.Responsable.Id,
            CancellationToken.None));
    }

    [Fact]
    public async Task SoloElResponsableDelEventoPuedeRegistrarAsistencia()
    {
        var escenario = new Escenario();
        var inscripcion = escenario.CrearInscripcion();

        await Assert.ThrowsAsync<ExcepcionAccesoProhibido>(() => escenario.Servicio.RegistrarAsistenciaAsync(
            new PeticionAsistenciaDto { InscripcionId = inscripcion.Id, Asistio = true },
            escenario.OtroResponsable.Id,
            CancellationToken.None));
    }

    [Fact]
    public async Task ListaLosParticipantesDelEvento()
    {
        var escenario = new Escenario();
        escenario.CrearInscripcion();

        var participantes = await escenario.Servicio.ListarParticipantesAsync(escenario.Evento.Id, escenario.Responsable.Id, CancellationToken.None);

        var participante = Assert.Single(participantes);
        Assert.Equal("Ana Gomez", participante.NombreCompleto);
        Assert.Equal("EST001", participante.Codigo);
        Assert.Equal(EstadoInscripcion.Activa, participante.EstadoInscripcion);
    }
}
