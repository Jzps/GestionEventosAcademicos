using System.Collections.Concurrent;
using System.Security.Cryptography;
using FrontGestionEventos.Models;

namespace FrontGestionEventos.Services;

/// <summary>
/// Almacén de tickets de un solo uso. Permite canjear unas credenciales ya validadas por
/// la API por una cookie de sesión: el token viaja del servidor de la API al del frontend
/// sin pasar por el navegador, y este solo recibe un identificador aleatorio y de vida
/// corta que se consume en el primer uso.
/// </summary>
public sealed class ServicioTickets
{
    private const int LongitudTicket = 32;
    private static readonly TimeSpan Vigencia = TimeSpan.FromMinutes(2);

    private readonly ConcurrentDictionary<string, Ticket> _tickets = new(StringComparer.Ordinal);

    /// <summary>Registra un ticket de un solo uso y devuelve su identificador.</summary>
    public string Registrar(string token, UsuarioActualDto usuario)
    {
        Limpiar();

        var identificador = Convert.ToHexString(RandomNumberGenerator.GetBytes(LongitudTicket));
        _tickets[identificador] = new Ticket(token, usuario, DateTimeOffset.UtcNow.Add(Vigencia));
        return identificador;
    }

    /// <summary>Consume un ticket y devuelve los datos de sesión asociados.</summary>
    public Ticket? Consumir(string identificador)
    {
        if (string.IsNullOrWhiteSpace(identificador) || !_tickets.TryRemove(identificador, out var ticket))
        {
            return null;
        }

        return ticket.Expira > DateTimeOffset.UtcNow ? ticket : null;
    }

    private void Limpiar()
    {
        var ahora = DateTimeOffset.UtcNow;
        foreach (var par in _tickets)
        {
            if (par.Value.Expira < ahora)
            {
                _tickets.TryRemove(par.Key, out _);
            }
        }
    }

    /// <summary>Datos asociados a un ticket pendiente de canje.</summary>
    public sealed record Ticket(string Token, UsuarioActualDto Usuario, DateTimeOffset Expira);
}
