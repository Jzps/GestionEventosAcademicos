using ApiGestionAcademico.Excepciones;
using ApiGestionAcademico.Modelos;
using Pruebas.ApiGestionAcademico.Dobles;
using Xunit;

namespace Pruebas.ApiGestionAcademico;

/// <summary>
/// Pruebas del patrón Builder: el evento debe construirse de forma válida y rechazar
/// los datos que violan RN-03 y RN-04.
/// </summary>
public class EventoBuilderTests
{
    private static readonly DateTime FechaRegistro = new(2026, 5, 1, 8, 0, 0);

    private static Categoria Categoria() => new("Seminarios", "Seminarios academicos") { Id = 1 };

    private static Responsable Responsable() => new("Responsable", "responsable@uni.edu.co", "hash", "Dependencia", "Cargo") { Id = 1 };

    [Fact]
    public void ConstruyeUnEventoValido()
    {
        var evento = new EventoBuilder()
            .ConNombre("Seminario de Arquitectura de Software")
            .ConDescripcion("Seminario sobre analisis y diseno de sistemas.")
            .ConFechas(FechaRegistro.AddDays(10), FechaRegistro.AddDays(10).AddHours(2))
            .ConLugar("Auditorio Principal")
            .ConCupos(40)
            .ConCategoria(Categoria())
            .ConResponsable(Responsable())
            .ConFechaRegistro(FechaRegistro)
            .Build();

        Assert.Equal("Seminario de Arquitectura de Software", evento.Nombre);
        Assert.Equal(40, evento.CuposTotales);
        Assert.Equal(FechaRegistro, evento.FechaRegistro);
        Assert.Equal(EstadoEvento.Programado, evento.Estado);
        Assert.True(evento.ValidarFechaInicio());
        Assert.True(evento.ValidarCupos());
    }

    [Fact]
    public void RechazaCuposNoPositivos()
    {
        // RN-04: los cupos totales deben ser mayores que cero.
        var excepcion = Assert.Throws<ExcepcionNegocio>(() => new EventoBuilder()
            .ConNombre("Evento sin cupos")
            .ConDescripcion("Evento para validar la regla de cupos.")
            .ConFechas(FechaRegistro.AddDays(5), FechaRegistro.AddDays(5).AddHours(1))
            .ConLugar("Sala 1")
            .ConCupos(0)
            .ConCategoria(Categoria())
            .ConResponsable(Responsable())
            .ConFechaRegistro(FechaRegistro)
            .Build());

        Assert.Contains("cupos", excepcion.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RechazaFechaInicioAnteriorAlRegistro()
    {
        // RN-03: la fecha de inicio debe ser posterior a la fecha de registro.
        var excepcion = Assert.Throws<ExcepcionNegocio>(() => new EventoBuilder()
            .ConNombre("Evento con fecha invalida")
            .ConDescripcion("Evento para validar la regla de fechas.")
            .ConFechas(FechaRegistro.AddDays(-1), FechaRegistro.AddHours(2))
            .ConLugar("Sala 2")
            .ConCupos(20)
            .ConCategoria(Categoria())
            .ConResponsable(Responsable())
            .ConFechaRegistro(FechaRegistro)
            .Build());

        Assert.Contains("posterior", excepcion.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RechazaFechaDeFinAnteriorAlInicio()
    {
        var excepcion = Assert.Throws<ExcepcionNegocio>(() => new EventoBuilder()
            .ConNombre("Evento con rango invertido")
            .ConDescripcion("Evento para validar el rango de fechas.")
            .ConFechas(FechaRegistro.AddDays(3), FechaRegistro.AddDays(1))
            .ConLugar("Sala 3")
            .ConCupos(20)
            .ConCategoria(Categoria())
            .ConResponsable(Responsable())
            .ConFechaRegistro(FechaRegistro)
            .Build());

        Assert.Contains("finalización", excepcion.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ExigeLosCamposObligatorios()
    {
        var excepcion = Assert.Throws<ExcepcionNegocio>(() => new EventoBuilder().Build());
        Assert.Contains("nombre", excepcion.Message, StringComparison.OrdinalIgnoreCase);
    }
}
