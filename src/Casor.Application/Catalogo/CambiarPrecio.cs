using Casor.Application.Comun;
using Casor.Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Casor.Application.Catalogo;

/// <summary>
/// Cambia el precio archivando el vigente en el histórico (RF-01).
/// Todo en una transacción: o pasan las 3 cosas, o ninguna.
/// </summary>
public class CambiarPrecio(ICasorDb db)
{
    public async Task EjecutarAsync(Guid productoId, decimal precioNuevo, Guid usuarioId)
    {
        if (precioNuevo < 0)
            throw new ArgumentException("El precio no puede ser negativo");

        await using var tx = await db.IniciarTransaccionAsync();

        var producto = await db.Productos.FirstOrDefaultAsync(p => p.Id == productoId)
            ?? throw new InvalidOperationException("Producto no existe");

        var vigente = await db.PreciosHistoricos
            .FirstOrDefaultAsync(p => p.ProductoId == productoId && p.VigenteHasta == null);
        if (vigente is not null)
            vigente.VigenteHasta = DateTimeOffset.UtcNow;

        db.PreciosHistoricos.Add(new PrecioHistorico
        {
            ProductoId = productoId,
            Precio = precioNuevo,
            VigenteDesde = DateTimeOffset.UtcNow,
            UsuarioId = usuarioId
        });

        producto.PrecioVenta = precioNuevo;

        await db.SaveChangesAsync();
        await tx.ConfirmarAsync();
    }
}
