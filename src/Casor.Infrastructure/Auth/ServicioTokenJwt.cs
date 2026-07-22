using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Casor.Application.Auth;
using Casor.Domain.Entidades;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Casor.Infrastructure.Auth;

/// <summary>
/// Fabrica JWTs. Un JWT tiene 3 partes separadas por puntos:
///   cabecera.datos.firma
/// Los "datos" (claims) dicen QUIÉN es y QUÉ ROL tiene; la "firma" se calcula
/// con la clave secreta del servidor — si alguien altera los datos, la firma
/// deja de calzar y la API rechaza el token. Por eso el servidor no necesita
/// recordar sesiones: el carnet se auto-valida.
/// </summary>
public class ServicioTokenJwt(IConfiguration config) : IServicioToken
{
    public (string token, DateTimeOffset expira) CrearToken(Usuario usuario)
    {
        var clave = config["Jwt:Key"]
            ?? throw new InvalidOperationException("Falta Jwt:Key en la configuración");
        var minutos = int.TryParse(config["Jwt:ExpiraMinutos"], out var m) ? m : 480;
        var expira = DateTimeOffset.UtcNow.AddMinutes(minutos);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, usuario.Username),
            new Claim(ClaimTypes.Role, usuario.Rol.ToString()),   // "Admin" o "Pos" → RNF-03
        };

        var creds = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(clave)),
            SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: expira.UtcDateTime,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(jwt), expira);
    }
}
