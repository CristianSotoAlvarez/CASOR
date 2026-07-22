using Casor.Application.Comun;
using Casor.Application.Stock;
using Casor.Domain;
using Casor.Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Casor.Application.Abastecimiento;

public record LineaRecepcion(Guid ProductoId, int Cantidad, decimal CostoUnitario,
    DateOnly? FechaVencimiento);
public record RegistrarRecepcionRequest(Guid? OrdenCompraId, Guid ProveedorId,
    Guid SucursalId, string? NumeroFactura, List<LineaRecepcion> Lineas);

/// <summary>
/// Entrada de mercadería (RF-07). En UNA transacción: crea la recepción,
/// suma stock por el punto único (RegistrarMovimientoStock), actualiza el
/// costo del producto y el vencimiento próximo, y cierra la orden si la había.
/// </summary>
public class RegistrarRecepcion(ICasorDb db, RegistrarMovimientoStock movimiento)
{
    public async Task<Guid> EjecutarAsync(RegistrarRecepcionRequest req, Guid usuarioId)
    {
        if (req.Lineas.Count == 0)
            throw new ArgumentException("Una recepción sin líneas no es una recepción");
        if (req.Lineas.Any(l => l.Cantidad <= 0 || l.CostoUnitario < 0))
            throw new ArgumentException("Cantidades deben ser > 0 y costos >= 0");

        await using var tx = await db.IniciarTransaccionAsync();

        var recepcion = new Recepcion
        {
            OrdenCompraId = req.OrdenCompraId,        // null permitido (D11)
            ProveedorId = req.ProveedorId,
            SucursalId = req.SucursalId,
            UsuarioId = usuarioId,
            NumeroFactura = req.NumeroFactura
        };
        db.Recepciones.Add(recepcion);

        foreach (var linea in req.Lineas)
        {
            recepcion.Detalles.Add(new DetalleRecepcion
            {
                ProductoId = linea.ProductoId,
                Cantidad = linea.Cantidad,
                CostoUnitario = linea.CostoUnitario,
                FechaVencimiento = linea.FechaVencimiento
            });

            // stock por el ÚNICO punto de entrada — misma transacción ambiente
            await movimiento.EjecutarAsync(linea.ProductoId, req.SucursalId,
                +linea.Cantidad, $"recepcion:{recepcion.Id}", usuarioId);

            // costo real de la última compra (v1.1: el costo vive en producto)
            var producto = await db.Productos.FirstAsync(p => p.Id == linea.ProductoId);
            producto.CostoUnitario = linea.CostoUnitario;

            // vencimiento próximo: solo si esta partida vence ANTES que lo registrado
            if (linea.FechaVencimiento is not null)
            {
                var stock = await db.StockSucursales.FirstAsync(
                    s => s.ProductoId == linea.ProductoId && s.SucursalId == req.SucursalId);
                if (stock.FechaVencimientoProxima is null
                    || linea.FechaVencimiento < stock.FechaVencimientoProxima)
                    stock.FechaVencimientoProxima = linea.FechaVencimiento;
            }
        }

        if (req.OrdenCompraId is not null)
        {
            var orden = await db.OrdenesCompra.FirstOrDefaultAsync(o => o.Id == req.OrdenCompraId)
                ?? throw new InvalidOperationException("La orden de compra no existe");
            orden.Estado = EstadoOrdenCompra.RecibidaTotal;   // V1: recepción parcial = V1.1
        }

        await db.SaveChangesAsync();
        await tx.ConfirmarAsync();
        return recepcion.Id;
    }
}
