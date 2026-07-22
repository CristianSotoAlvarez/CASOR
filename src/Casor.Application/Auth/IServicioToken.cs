using Casor.Domain.Entidades;

namespace Casor.Application.Auth;

/// <summary>Fabrica el JWT firmado para un usuario autenticado.</summary>
public interface IServicioToken
{
    (string token, DateTimeOffset expira) CrearToken(Usuario usuario);
}
