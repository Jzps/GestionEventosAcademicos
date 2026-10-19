using System.Security.Cryptography;
using System.Text;

namespace ApiGestionAcademico.Servicios;

/// <summary>
/// Genera y verifica hashes de contraseña usando PBKDF2-HMAC-SHA256 con salt por usuario.
/// Las contraseñas nunca se almacenan en texto plano.
/// </summary>
public sealed class ServicioHashContrasena
{
    private const int Iteraciones = 120_000;
    private const int TamanoSalt = 16;
    private const int TamanoHash = 32;
    private const string Version = "v1";

    /// <summary>Genera el hash de una contraseña en texto plano.</summary>
    public string Generar(string contrasena)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contrasena);

        var salt = RandomNumberGenerator.GetBytes(TamanoSalt);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(contrasena),
            salt,
            Iteraciones,
            HashAlgorithmName.SHA256,
            TamanoHash);

        return $"{Version}.{Iteraciones}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    /// <summary>Verifica una contraseña frente a un hash almacenado.</summary>
    public bool Verificar(string contrasena, string hashAlmacenado)
    {
        if (string.IsNullOrWhiteSpace(contrasena) || string.IsNullOrWhiteSpace(hashAlmacenado))
        {
            return false;
        }

        var partes = hashAlmacenado.Split('.');
        if (partes.Length != 4 || partes[0] != Version)
        {
            return false;
        }

        if (!int.TryParse(partes[1], out var iteraciones) || iteraciones <= 0)
        {
            return false;
        }

        byte[] salt;
        byte[] hashEsperado;
        try
        {
            salt = Convert.FromBase64String(partes[2]);
            hashEsperado = Convert.FromBase64String(partes[3]);
        }
        catch (FormatException)
        {
            return false;
        }

        var hashCalculado = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(contrasena),
            salt,
            iteraciones,
            HashAlgorithmName.SHA256,
            hashEsperado.Length);

        return CryptographicOperations.FixedTimeEquals(hashCalculado, hashEsperado);
    }
}
