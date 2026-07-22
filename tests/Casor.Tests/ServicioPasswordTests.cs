using Casor.Infrastructure.Auth;
using Xunit;

namespace Casor.Tests;

public class ServicioPasswordTests
{
    private readonly ServicioPassword _svc = new();

    [Fact]
    public void Hash_y_verificacion_correcta()
    {
        var hash = _svc.Hashear("MiClaveSegura123");
        Assert.NotEqual("MiClaveSegura123", hash);      // jamás texto plano
        Assert.True(_svc.Verificar("MiClaveSegura123", hash));
    }

    [Fact]
    public void Clave_incorrecta_falla()
    {
        var hash = _svc.Hashear("MiClaveSegura123");
        Assert.False(_svc.Verificar("otraClave", hash));
    }

    [Fact]
    public void Mismo_password_produce_hashes_distintos()
    {
        // Gracias al salt aleatorio: si roban la BD, no pueden agrupar
        // usuarios que comparten contraseña.
        Assert.NotEqual(_svc.Hashear("abc123"), _svc.Hashear("abc123"));
    }
}
