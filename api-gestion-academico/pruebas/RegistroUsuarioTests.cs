using ApiGestionAcademico.Excepciones;
using ApiGestionAcademico.Modelos;
using ApiGestionAcademico.Peticiones;
using ApiGestionAcademico.Servicios;
using Microsoft.Extensions.Configuration;
using Pruebas.ApiGestionAcademico.Dobles;
using Xunit;

namespace Pruebas.ApiGestionAcademico;

/// <summary>Pruebas del registro público de cuentas de estudiante.</summary>
public class RegistroUsuarioTests
{
    private static (ServicioAutenticacion Servicio, AlmacenFalso Almacen) CrearServicio()
    {
        var almacen = new AlmacenFalso();
        var repositorio = new RepositorioUsuarioFalso(almacen);
        var hash = new ServicioHashContrasena();
        var configuracion = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Clave"] = "ClaveDePruebas-Registro-GestionEventos",
                ["Jwt:Emisor"] = "Pruebas",
                ["Jwt:Audiencia"] = "Pruebas"
            })
            .Build();

        var token = new ServicioToken(configuracion, new RelojFalso(DateTimeOffset.Now));
        return (new ServicioAutenticacion(repositorio, hash, token), almacen);
    }

    private static PeticionRegistroDto PeticionValida(string correo = "nuevo.estudiante@uni.edu.co") => new()
    {
        NombreCompleto = "Nuevo Estudiante Prueba",
        Email = correo,
        Contrasena = "Estudiante2024!",
        Codigo = "EST2025999",
        Programa = "Ingeniería de Sistemas"
    };

    [Fact]
    public async Task RegistraUnEstudianteNuevo()
    {
        var (servicio, almacen) = CrearServicio();

        var respuesta = await servicio.RegistrarAsync(PeticionValida(), CancellationToken.None);

        Assert.False(string.IsNullOrWhiteSpace(respuesta.Token));
        Assert.Equal(RolUsuario.Estudiante, respuesta.Usuario.Rol);
        Assert.Equal("EST2025999", respuesta.Usuario.Codigo);

        var usuario = Assert.Single(almacen.Usuarios.Values);
        var estudiante = Assert.IsType<Estudiante>(usuario);
        Assert.Equal(RolUsuario.Estudiante, estudiante.Rol);
        Assert.Equal("nuevo.estudiante@uni.edu.co", estudiante.Email);

        // La contraseña se guarda cifrada, nunca en texto plano.
        Assert.NotEqual("Estudiante2024!", estudiante.ContrasenaHash);
        Assert.True(new ServicioHashContrasena().Verificar("Estudiante2024!", estudiante.ContrasenaHash));
    }

    [Fact]
    public async Task RechazaCorreosDuplicados()
    {
        var (servicio, almacen) = CrearServicio();
        almacen.Usuarios[1] = new Estudiante("Existente", "nuevo.estudiante@uni.edu.co", "hash", "EST1", "Programa");

        var excepcion = await Assert.ThrowsAsync<ExcepcionNegocio>(() =>
            servicio.RegistrarAsync(PeticionValida(), CancellationToken.None));

        Assert.Contains("correo", excepcion.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Single(almacen.Usuarios);
    }

    [Fact]
    public async Task RechazaContrasenasQueNoCumplenLaPolitica()
    {
        var (servicio, almacen) = CrearServicio();

        var peticion = PeticionValida();
        peticion.Contrasena = "corta";

        var excepcion = await Assert.ThrowsAsync<ExcepcionNegocio>(() =>
            servicio.RegistrarAsync(peticion, CancellationToken.None));

        Assert.Contains("contraseña", excepcion.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(almacen.Usuarios);
    }

    [Fact]
    public async Task ElRolLoAsignaElBackendYNoElCliente()
    {
        // La petición no contiene ningún campo de rol: la cuenta creada es siempre de estudiante.
        var (servicio, almacen) = CrearServicio();

        await servicio.RegistrarAsync(PeticionValida("otro.estudiante@uni.edu.co"), CancellationToken.None);

        var usuario = Assert.Single(almacen.Usuarios.Values);
        Assert.IsType<Estudiante>(usuario);
        Assert.Equal(RolUsuario.Estudiante, usuario.GetRol());
        Assert.NotEqual(RolUsuario.Responsable, usuario.GetRol());
    }

    [Fact]
    public async Task ElEstudianteRegistradoPuedeIniciarSesion()
    {
        var (servicio, almacen) = CrearServicio();

        await servicio.RegistrarAsync(PeticionValida(), CancellationToken.None);

        var respuesta = await servicio.IniciarSesionAsync(
            new PeticionLogin { Email = "nuevo.estudiante@uni.edu.co", Contrasena = "Estudiante2024!" },
            CancellationToken.None);

        Assert.False(string.IsNullOrWhiteSpace(respuesta.Token));
        Assert.Equal(RolUsuario.Estudiante, respuesta.Usuario.Rol);
    }
}
