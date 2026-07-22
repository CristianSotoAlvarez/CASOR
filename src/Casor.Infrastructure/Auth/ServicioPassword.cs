using Casor.Application.Auth;
using Microsoft.AspNetCore.Identity;

namespace Casor.Infrastructure.Auth;

/// <summary>
/// Hashing con PasswordHasher de ASP.NET Identity (PBKDF2-HMAC-SHA256,
/// con salt aleatorio e iteraciones incluidos en el propio hash).
/// Nota de diseño: el diccionario menciona Argon2id como ideal; PBKDF2 del
/// stack oficial es sólido y sin dependencias extra — se reevalúa en el
/// hardening de Fase 4 si se exige Argon2.
/// </summary>
public class ServicioPassword : IServicioPassword
{
    private readonly PasswordHasher<object> _hasher = new();
    private static readonly object _dummy = new();

    public string Hashear(string password) => _hasher.HashPassword(_dummy, password);

    public bool Verificar(string password, string hash) =>
        _hasher.VerifyHashedPassword(_dummy, hash, password)
            != PasswordVerificationResult.Failed;
}
