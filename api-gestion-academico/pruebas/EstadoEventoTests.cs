using ApiGestionAcademico.Excepciones;
using ApiGestionAcademico.Modelos;
using ApiGestionAcademico.Modelos.Estados;
using Xunit;

namespace Pruebas.ApiGestionAcademico;

/// <summary>
/// Pruebas del patrón State: cada estado define qué se puede hacer con el evento y
/// las transiciones hacia las que puede moverse.
/// </summary>
public class EstadoEventoTests
{
    private static readonly DateTime Ahora = new(2026, 5, 1, 8, 0, 0);

    private static Estudiante Estudiante(long id = 10) =>
        new("Estudiante", "estudiante@uni.edu.co", "hash", "EST001", "Ingenieria") { Id = id };

    private static Evento EventoConEstado(EstadoEvento estado, int cupos = 10)
    {
        var evento = new Evento
        {
            Id = 1,
            Nombre = "Evento de prueba",
            Descripcion = "Evento utilizado en las pruebas unitarias.",
            Lugar = "Auditorio",
            CuposTotales = cupos,
            FechaRegistro = Ahora.AddDays(-10),
            FechaHoraInicio = Ahora.AddDays(5),
            FechaHoraFin = Ahora.AddDays(5).AddHours(2),
            Estado = estado,
            ResponsableId = 1,
            CategoriaId = 1
        };

        return evento;
    }

    [Fact]
    public void EstadoProgramadoSeAbreSoloCuandoElEventoEsFuturo()
    {
        var evento = EventoConEstado(EstadoEvento.Programado);

        var transicion = evento.EstadoActual.TransicionAutomatica(evento, Ahora, evento.GetCuposDisponibles());

        Assert.Equal(TransicionEvento.AbrirInscripciones, transicion);
    }

    [Fact]
    public void ActualizarEstadoPromueveElEventoProgramadoAInscripcionesAbiertas()
    {
        var evento = EventoConEstado(EstadoEvento.Programado);

        evento.ActualizarEstado(Ahora, evento.GetCuposDisponibles());

        Assert.Equal(EstadoEvento.InscripcionesAbiertas, evento.Estado);
        Assert.True(evento.AdmiteInscripciones());
    }

    [Fact]
    public void LasInscripcionesSeCierranCuandoNoHayCupos()
    {
        var evento = EventoConEstado(EstadoEvento.InscripcionesAbiertas, cupos: 1);

        var transicion = evento.EstadoActual.TransicionAutomatica(evento, Ahora, 0);

        Assert.Equal(TransicionEvento.CerrarInscripciones, transicion);

        evento.ActualizarEstado(Ahora, 0);
        Assert.Equal(EstadoEvento.InscripcionesCerradas, evento.Estado);
        Assert.False(evento.AdmiteInscripciones());
    }

    [Fact]
    public void ElEventoEnCursoNoAdmiteInscripciones()
    {
        var evento = EventoConEstado(EstadoEvento.EnCurso);

        var excepcion = Assert.Throws<ExcepcionNegocio>(() => evento.RegistrarInscripcion(Estudiante()));

        Assert.Contains("curso", excepcion.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ElEventoCanceladoNoAdmiteInscripciones()
    {
        var evento = EventoConEstado(EstadoEvento.Cancelado);

        var excepcion = Assert.Throws<ExcepcionNegocio>(() => evento.RegistrarInscripcion(Estudiante()));

        Assert.Contains("cancelado", excepcion.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ElEventoFinalizadoNoAdmiteInscripciones()
    {
        var evento = EventoConEstado(EstadoEvento.Finalizado);

        Assert.Throws<ExcepcionNegocio>(() => evento.RegistrarInscripcion(Estudiante()));
        Assert.False(evento.EstaDisponible() && evento.AdmiteInscripciones());
    }

    [Fact]
    public void LasInscripcionesCerradasPermitenCancelarSiElEventoNoHaComenzado()
    {
        // RN-07: la cancelación se permite mientras el evento no haya comenzado. Un evento
        // que cerró sus inscripciones por falta de cupos no debe bloquear al estudiante que
        // ocupa el último cupo.
        var evento = EventoConEstado(EstadoEvento.InscripcionesCerradas);

        Assert.True(evento.EstadoActual.PermiteCancelarInscripcion);
        Assert.True(evento.PuedeCancelarInscripcion(Ahora));
        Assert.False(evento.PuedeCancelarInscripcion(Ahora.AddDays(10)),
            "Un evento que ya comenzó no admite cancelaciones.");
    }

    [Fact]
    public void LasInscripcionesSeReabrenCuandoSeLiberaUnCupo()
    {
        // Tras una cancelación el evento vuelve a admitir inscripciones.
        var evento = EventoConEstado(EstadoEvento.InscripcionesCerradas);

        var transicion = evento.EstadoActual.TransicionAutomatica(evento, Ahora, 1);

        Assert.Equal(TransicionEvento.ReabrirInscripciones, transicion);

        evento.ActualizarEstado(Ahora, 1);
        Assert.Equal(EstadoEvento.InscripcionesAbiertas, evento.Estado);
        Assert.True(evento.AdmiteInscripciones());
    }

    [Fact]
    public void LasTransicionesInvalidasSonRechazadas()
    {
        var estado = new EstadoFinalizado();

        var excepcion = Assert.Throws<ExcepcionNegocio>(() => estado.Transicionar(TransicionEvento.AbrirInscripciones));

        Assert.Contains("no es válida", excepcion.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void LaFabricaReconstruyeElComportamientoDesdeElEstadoPersistido()
    {
        var estado = FabricaEstadosEvento.Crear(EstadoEvento.InscripcionesAbiertas);

        Assert.IsType<EstadoInscripcionesAbiertas>(estado);
        Assert.True(estado.AdmiteInscripciones);
        Assert.Equal(EstadoEvento.InscripcionesAbiertas, estado.Estado);
    }
}
