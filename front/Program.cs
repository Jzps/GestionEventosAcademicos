using System.Globalization;
using System.Security.Claims;
using FrontGestionEventos;
using FrontGestionEventos.Models;
using FrontGestionEventos.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// El prototipo se presenta en español.
var cultura = new CultureInfo("es-CO");
CultureInfo.DefaultThreadCurrentCulture = cultura;
CultureInfo.DefaultThreadCurrentUICulture = cultura;
builder.Services.Configure<RequestLocalizationOptions>(opciones =>
{
    opciones.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture(cultura);
    opciones.SupportedCultures = new List<CultureInfo> { cultura, CultureInfo.InvariantCulture };
    opciones.SupportedUICultures = new List<CultureInfo> { cultura, CultureInfo.InvariantCulture };
});

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

var urlBaseApi = builder.Configuration["Api:UrlBase"] ?? "http://localhost:8081/";
builder.Services.AddHttpClient("api", cliente =>
{
    cliente.BaseAddress = new Uri(urlBaseApi);
    cliente.Timeout = TimeSpan.FromSeconds(30);
});

// ---------------------------------------------------------------------------------------
// Autenticación local del prototipo.
//
// La sesión se conserva en una cookie cifrada por el middleware de autenticación de ASP.NET
// Core (Data Protection). La cookie es HttpOnly: el token JWT nunca es accesible desde
// JavaScript. Las credenciales de la API se canjean por un ticket de un solo uso que el
// navegador envía al endpoint /api-sesion/establecer para que el servidor firme la cookie.
//
// `AddAuthentication` también es indispensable: sin él, el middleware de autorización de
// las páginas protegidas falla al intentar emitir el challenge (HTTP 500).
// ---------------------------------------------------------------------------------------
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opciones =>
    {
        opciones.Cookie.Name = "ges-eventos-sesion";
        opciones.Cookie.HttpOnly = true;
        opciones.Cookie.SameSite = SameSiteMode.Lax;
        opciones.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        opciones.ExpireTimeSpan = TimeSpan.FromHours(8);
        opciones.SlidingExpiration = true;
        opciones.LoginPath = "/acceder";
        opciones.AccessDeniedPath = "/acceder";
    });

builder.Services.AddAuthorization();

// Proporciona el estado de autenticación en cascada a los componentes. El proveedor
// estándar (ServerAuthenticationStateProvider) toma la identidad del HttpContext de la
// petición inicial, donde la cookie ya fue validada.
builder.Services.AddCascadingAuthenticationState();

// Almacén de tickets de un solo uso para el canje de credenciales por cookie de sesión.
builder.Services.AddSingleton<ServicioTickets>();

builder.Services.AddScoped<ServicioAutenticacion>();
builder.Services.AddScoped<ServicioEventos>();
builder.Services.AddScoped<ServicioInscripciones>();
builder.Services.AddScoped<ServicioParticipacion>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseRequestLocalization();
app.UseStaticFiles();
app.UseAntiforgery();

// La cookie de sesión debe validarse antes de resolver las rutas protegidas.
app.UseAuthentication();
app.UseAuthorization();

// Publica los recursos estáticos del framework de Blazor (_framework/blazor.web.js)
// y los recursos de la aplicación a partir del manifiesto de la compilación.
app.MapStaticAssets();

// ---------------------------------------------------------------------------
// Endpoints de sesión del frontend.
//
// Se invocan mediante una redirección del navegador (no mediante fetch), por lo que la
// cookie se establece en la respuesta que el navegador recibe y almacena.
// ---------------------------------------------------------------------------
app.MapGet("/api-sesion/establecer", async (HttpContext contexto, ServicioTickets tickets) =>
{
    var ticket = contexto.Request.Query["ticket"].ToString();
    var destino = contexto.Request.Query["destino"].ToString();
    var usuario = tickets.Consumir(ticket);

    if (usuario is null)
    {
        return Results.Redirect("/acceder?error=ticket");
    }

    var identidad = new ClaimsIdentity(
        new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Usuario.NombreCompleto),
            new Claim(ClaimTypes.Email, usuario.Usuario.Email),
            new Claim(ClaimTypes.Role, usuario.Usuario.Rol.ToString()),
            // Claim propia con el token que la API exige en el encabezado Authorization.
            new Claim(ClaimsSesion.TokenDeApi, usuario.Token)
        },
        CookieAuthenticationDefaults.AuthenticationScheme,
        ClaimTypes.Name,
        ClaimTypes.Role);

    await contexto.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        new ClaimsPrincipal(identidad),
        new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });

    return Results.Redirect(string.IsNullOrWhiteSpace(destino) ? "/" : destino);
}).AllowAnonymous();

app.MapGet("/api-sesion/cerrar", async (HttpContext contexto) =>
{
    await contexto.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/acceder?salida=1");
}).AllowAnonymous();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
