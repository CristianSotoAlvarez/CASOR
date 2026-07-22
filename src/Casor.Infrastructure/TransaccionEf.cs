using Casor.Application.Comun;
using Microsoft.EntityFrameworkCore.Storage;

namespace Casor.Infrastructure;

/// <summary>
/// Adaptador: envuelve la transacción de EF tras la interfaz de Application.
/// Si se libera sin Confirmar (excepción en el camino), EF hace rollback solo.
/// </summary>
public sealed class TransaccionEf(IDbContextTransaction tx) : ITransaccion
{
    public Task ConfirmarAsync() => tx.CommitAsync();
    public ValueTask DisposeAsync() => tx.DisposeAsync();
}
