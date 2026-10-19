using ApiGestionAcademico.Excepciones;
using ApiGestionAcademico.Modelos;
using ApiGestionAcademico.Peticiones;
using ApiGestionAcademico.Servicios;
using Microsoft.Extensions.Configuration;
using Pruebas.ApiGestionAcademico.Dobles;
using Xunit;

namespace Pruebas.ApiGestionAcademico;

/// <summary>Pruebas del servicio de autenticación local y del hash de contraseñas.</summary>
public class ServicioAutenticacionTests
{
    private static (ServicioAutenticacion Servicio, RepositorioUsuarioFalso Usuarios) CrearServicio(Estudiante? usuario = null)
    {
        var almacen = new AlmacenFalso();
        var repositorio = new RepositorioUsuarioFalso(almacen);
        var hash = new ServicioHashContrasena();

        if (usuario is not null)
        {
            usuario.Id = 1;
            almacen.Usuarios[usuario.Id] = usuario;
        }

        var configuracion = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Clave"] = "ClaveDePruebas-Autenticacion-GestionEventos",
                ["Jwt:Emisor"] = "Pruebas",
                ["Jwt:Audiencia"] = "Pruebas"
            })
            .Build();

        var token = new ServicioToken(configuracion, new RelojFalso(DateTimeOffset.Now));
        return (new ServicioAutenticacion(repositorio, hash, token), repositorio);
    }

    [Fact]
    public void ElHashNoGuardaLaContrasenaEnTextoPlano()
    {
        var hash = new ServicioHashContrasena();

        var resultado = hash.Generar("Estudiante2024!");

        Assert.DoesNotContain("Estudiante2024!", resultado, StringComparison.Ordinal);
        Assert.True(hash.Verificar("Estudiante2024!", resultado));
        Assert.False(hash.Verificar("otra-contrasena", resultado));
    }

    [Fact]
    public void ElHashGeneraSalesDistintosParaLaMismaContrasena()
    {
        var hash = new ServicioHashContrasena();

        var primero = hash.Generar("Estudiante2024!");
        var segundo = hash.Generar("Estudiante2024!");

        Assert.NotEqual(primero, segundo);
        Assert.True(hash.Verificar("Estudiante2024!", primero));
        Assert.True(hash.Verificar("Estudiante2024!", segundo));
    }

    [Fact]
    public async Task IniciaSesionConCredencialesValidas()
    {
        var hash = new ServicioHashContrasena();
        var estudiante = new Estudiante("Ana Gomez", "estudiante@uni.edu.co", hash.Generar("Estudiante2024!"), "EST001", "Ingenieria");
        var (servicio, _) = CrearServicio(estudiante);

        var respuesta = await servicio.IniciarSesionAsync(
            new PeticionLogin { Email = "estudiante@uni.edu.co", Contrasena = "Estudiante2024!" },
            CancellationToken.None);

        Assert.False(string.IsNullOrWhiteSpace(respuesta.Token));
        Assert.Equal(RolUsuario.Estudiante, respuesta.Usuario.Rol);
        Assert.Equal("EST001", respuesta.Usuario.Codigo);
    }

    [Fact]
    public async Task RechazaUnaContrasenaIncorrecta()
    {
        var hash = new ServicioHashContrasena();
        var estudiante = new Estudiante("Ana Gomez", "estudiante@uni.edu.co", hash.Generar("Estudiante2024!"), "EST001", "Ingenieria");
        var (servicio, _) = CrearServicio(estudiante);

        await Assert.ThrowsAsync<ExcepcionNoAutorizado>(() => servicio.IniciarSesionAsync(
            new PeticionLogin { Email = "estudiante@uni.edu.co", Contrasena = "incorrecta" },
            CancellationToken.None));
    }

    [Fact]
    public async Task RechazaUnCorreoDesconocido()
    {
        var (servicio, _) = CrearServicio();

        await Assert.ThrowsAsync<ExcepcionNoAutorizado>(() => servicio.IniciarSesionAsync(
            new PeticionLogin { Email = "nadie@uni.edu.co", Contrasena = "Estudiante2024!" },
            CancellationToken.None));
    }

    [Fact]
    public async Task RechazaUnaCuentaInactiva()
    {
        var hash = new ServicioHashContrasena();
        var estudiante = new Estudiante("Ana Gomez", "estudiante@uni.edu.co", hash.Generar("Estudiante2024!"), "EST001", "Ingenieria")
        {
            Activo = false
        };
        var (servicio, _) = CrearServicio(estudiante);

        await Assert.ThrowsAsync<ExcepcionNoAutorizado>(() => servicio.IniciarSesionAsync(
            new PeticionLogin { Email = "estudiante@uni.edu.co", Contrasena = "Estudiante2024!" },
            CancellationToken.None));
    }
}
