using ApiGestionAcademico.Excepciones;
using ApiGestionAcademico.Modelos;
using ApiGestionAcademico.Peticiones;
using ApiGestionAcademico.Repositorios;
using ApiGestionAcademico.Servicios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------------------------
// Servicios de la API
// ---------------------------------------------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddExceptionHandler<ManejadorErrores>();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(opciones =>
{
    opciones.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API - Sistema de Gestión de Eventos Académicos",
        Version = "v1",
        Description = "API REST del prototipo académico. Permite a los estudiantes consultar eventos e inscripciones y a los responsables registrar eventos, consultar participantes y registrar asistencia."
    });

    var esquema = new OpenApiSecurityScheme
    {
        Name = "Autorización",
        Description = "Introduzca el token JWT obtenido en POST /api/auth/login.",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };

    opciones.AddSecurityDefinition("Bearer", esquema);
    opciones.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { esquema, Array.Empty<string>() }
    });
});

var claveJwt = builder.Configuration["Jwt:Clave"]
    ?? throw new InvalidOperationException("Falta configurar 'Jwt:Clave' en la configuración de la aplicación.");
var emisor = builder.Configuration["Jwt:Emisor"] ?? "GestionEventosAcademicos";
var audiencia = builder.Configuration["Jwt:Audiencia"] ?? "GestionEventosAcademicos";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opciones =>
    {
        opciones.RequireHttpsMetadata = false;
        opciones.SaveToken = true;
        opciones.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = emisor,
            ValidateAudience = true,
            ValidAudience = audiencia,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(claveJwt)),
            ClockSkew = TimeSpan.FromSeconds(30),
            RoleClaimType = System.Security.Claims.ClaimTypes.Role,
            NameClaimType = System.Security.Claims.ClaimTypes.Name
        };
    });

builder.Services.AddAuthorization();

var cadenaConexion = builder.Configuration.GetConnectionString("SqlServer")
    ?? throw new InvalidOperationException("Falta configurar la cadena de conexión 'SqlServer'.");

builder.Services.AddDbContext<ContextoDatos>(opciones =>
    opciones.UseSqlServer(cadenaConexion, sql => sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)));

// Repositorios: los servicios dependen de estas abstracciones (DIP).
builder.Services.AddScoped<IRepositorioEvento, RepositorioEvento>();
builder.Services.AddScoped<IRepositorioUsuario, RepositorioUsuario>();
builder.Services.AddScoped<IRepositorioInscripcion, RepositorioInscripcion>();
builder.Services.AddScoped<IRepositorioAsistencia, RepositorioAsistencia>();
builder.Services.AddScoped<IRepositorioCategoria, RepositorioCategoria>();
builder.Services.AddScoped<IUnidadTransaccional, UnidadTransaccionalSqlServer>();

// Servicios de aplicación y dominio.
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<ServicioHashContrasena>();
builder.Services.AddSingleton<ServicioToken>();
builder.Services.AddScoped<ServicioAutenticacion>();
builder.Services.AddScoped<ServicioEventos>();
builder.Services.AddScoped<InscripcionFacade>();
builder.Services.AddScoped<ServicioParticipacion>();
builder.Services.AddScoped<InicializadorDatos>();

// La comprobación de salud verifica que la base de datos sea realmente utilizable.
builder.Services.AddHealthChecks()
    .AddCheck<ComprobacionSaludBaseDatos>("base-de-datos", tags: new[] { "requerido" });

var app = builder.Build();

// ---------------------------------------------------------------------------------------------
// Canalización de la aplicación
// ---------------------------------------------------------------------------------------------
app.UseExceptionHandler(new ExceptionHandlerOptions
{
    // .NET 10 entrega el error en IExceptionHandlerFeature; el manejador centralizado
    // lo traduce a una respuesta JSON uniforme para el cliente.
    ExceptionHandler = async contexto =>
    {
        var error = contexto.Features.Get<IExceptionHandlerFeature>()?.Error
            ?? new InvalidOperationException("Error no controlado en la solicitud.");

        var manejador = contexto.RequestServices.GetRequiredService<ManejadorErrores>();
        await manejador.TryHandleAsync(contexto, error, contexto.RequestAborted);
    }
});
app.UseSwagger();
app.UseSwaggerUI(opciones =>
{
    opciones.SwaggerEndpoint("/swagger/v1/swagger.json", "API - Gestión de Eventos Académicos v1");
    opciones.DocumentTitle = "API - Gestión de Eventos Académicos";
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Punto de entrada de la API: identifica el servicio y sus recursos principales.
app.MapGet("/", () => Results.Ok(new
{
    nombre = "API - Sistema de Gestión de Eventos Académicos",
    version = "1.0.0",
    descripcion = "Prototipo académico: consulta e inscripción de eventos, gestión de eventos, participantes y asistencia.",
    documentacion = "/swagger",
    salud = "/health",
    autenticacion = "POST /api/auth/login",
    recursos = new[]
    {
        "GET    /api/eventos",
        "GET    /api/eventos/{id}",
        "GET    /api/eventos/categorias",
        "GET    /api/eventos/gestionados        (Responsable)",
        "POST   /api/eventos                    (Responsable)",
        "PUT    /api/eventos/{id}               (Responsable)",
        "GET    /api/inscripciones/mias         (Estudiante)",
        "POST   /api/inscripciones              (Estudiante)",
        "DELETE /api/inscripciones/{id}         (Estudiante)",
        "GET    /api/participantes/{eventoId}   (Responsable)",
        "GET    /api/asistencia/{eventoId}      (Responsable)",
        "POST   /api/asistencia                  (Responsable)"
    }
})).AllowAnonymous();

app.MapHealthChecks("/health");
app.MapHealthChecks("/api/salud");

// Inicialización de la base de datos: se aplica el esquema (migraciones) y se cargan los
// datos de demostración de forma idempotente, con reintentos mientras SQL Server arranca.
await app.InicializarBaseDeDatosAsync();

app.Run();

/// <summary>Tareas de inicialización ejecutadas antes de levantar la aplicación.</summary>
internal static class InicializacionAplicacion
{
    public static async Task InicializarBaseDeDatosAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var proveedor = scope.ServiceProvider;
        var logger = proveedor.GetRequiredService<ILogger<Program>>();
        var contexto = proveedor.GetRequiredService<ContextoDatos>();
        var inicializador = proveedor.GetRequiredService<InicializadorDatos>();

        const int intentosMaximos = 30;
        for (var intento = 1; intento <= intentosMaximos; intento++)
        {
            try
            {
                await contexto.Database.MigrateAsync();
                await inicializador.SembrarAsync(contexto, CancellationToken.None);
                logger.LogInformation("Base de datos inicializada correctamente (intento {Intento}).", intento);
                return;
            }
            catch (Exception excepcion)
            {
                logger.LogWarning(
                    "La base de datos aún no está disponible (intento {Intento} de {IntentosMaximos}): {Mensaje}",
                    intento,
                    intentosMaximos,
                    excepcion.Message);

                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }

        throw new InvalidOperationException("No fue posible inicializar la base de datos después de varios intentos.");
    }
}

/// <summary>Marcador usado por las pruebas de integración.</summary>
public partial class Program;
