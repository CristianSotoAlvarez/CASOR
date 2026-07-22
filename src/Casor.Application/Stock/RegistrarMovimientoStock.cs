using Casor.Application.Comun;
using Casor.Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Casor.Application.Stock;

/// <summary>
/// EL ÚNICO punto por donde se mueve el inventario (regla de oro).
/// Recepciones suman, ventas restan, ajustes corrigen — todos llaman aquí.
/// Deja huella en auditoría y jamás permite stock negativo.
/// </summary>
public class RegistrarMovimientoStock(ICasorDb db)
{
    public async Task<int> EjecutarAsync(
        Guid productoId, Guid sucursalId, int delta, string motivo, Guid usuarioId)
    {
        if (delta == 0)
            throw new ArgumentException("Un movimiento de 0 unidades no es un movimiento");

        await using var tx = await db.IniciarTransaccionAsync();

        var stock = await db.StockSucursales
            .FirstOrDefaultAsync(s => s.ProductoId == productoId && s.SucursalId == sucursalId);

        if (stock is null)
        {
            if (delta < 0)
                throw new InvalidOperationException(
                    "No se puede descontar: el producto no tiene stock en esta sucursal");
            stock = new StockSucursal
                { ProductoId = productoId, SucursalId = sucursalId, CantidadActual = 0 };
            db.StockSucursales.Add(stock);
        }

        var nuevo = stock.CantidadActual + delta;
        if (nuevo < 0)
            throw new InvalidOperationException(
                $"Stock insuficiente: hay {stock.CantidadActual} y se intentó descontar {-delta}");

        stock.CantidadActual = nuevo;

        db.Auditorias.Add(new Auditoria
        {
            UsuarioId = usuarioId,
            Accion = "stock.movimiento",
            Entidad = "stock_sucursal",
            EntidadId = productoId,
            Datos = $"{{\"sucursal\":\"{sucursalId}\",\"delta\":{delta},\"resultado\":{nuevo},\"motivo\":\"{motivo}\"}}"
        });

        await db.SaveChangesAsync();
        await tx.ConfirmarAsync();
        return nuevo;
    }
}
