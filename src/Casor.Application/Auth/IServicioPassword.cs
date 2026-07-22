namespace Casor.Application.Auth;

/// <summary>
/// Contrato de hashing de contraseñas. La app NUNCA compara contraseñas:
/// compara huellas (hashes). La implementación vive en Infrastructure —
/// el dominio no sabe qué algoritmo se usa (regla de capas).
/// </summary>
public interface IServicioPassword
{
    string Hashear(string password);
    bool Verificar(string password, string hash);
}
