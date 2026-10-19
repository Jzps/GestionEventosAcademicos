namespace FrontGestionEventos.Services;

/// <summary>Claims propios de la sesión local del frontend.</summary>
public static class ClaimsSesion
{
    /// <summary>
    /// Claim que transporta el token JWT de la API. Viaja dentro de la cookie de sesión,
    /// que el middleware de autenticación cifra con Data Protection.
    /// </summary>
    public const string TokenDeApi = "token-api";
}
