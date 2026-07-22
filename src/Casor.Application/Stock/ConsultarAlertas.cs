using Casor.Application.Comun;
using Microsoft.EntityFrameworkCore;

namespace Casor.Application.Stock;

public record AlertaStock(Guid ProductoId, string Descripcion, int CantidadActual,
    int StockMinimo, DateOnly? VencimientoProximo);

/// <summary>Alertas de RF-07: bajo mínimo y vencimientos próximos (≤90 días).</summary>
public class ConsultarAlertas(ICasorDb db)
{
    public async Task<List<AlertaStock>> BajoMinimoAsync(Guid sucursalId) =>
        await db.StockSucursales
            .Where(s => s.SucursalId == sucursalId
                     && s.Producto.StockMinimo > 0
                     && s.CantidadActual < s.Producto.StockMinimo)
            .OrderBy(s => s.CantidadActual)
            .Select(s => new AlertaStock(s.ProductoId, s.Producto.Descripcion,
                s.CantidadActual, s.Producto.StockMinimo, s.FechaVencimientoProxima))
            .ToListAsync();

    public async Task<List<AlertaStock>> VencimientosProximosAsync(Guid sucursalId, int dias = 90)
    {
        var limite = DateOnly.FromDateTime(DateTime.Today.AddDays(dias));
        return await db.StockSucursales
            .Where(s => s.SucursalId == sucursalId
                     && s.CantidadActual > 0
                     && s.FechaVencimientoProxima != null
                     && s.FechaVencimientoProxima <= limite)
            .OrderBy(s => s.FechaVencimientoProxima)
            .Select(s => new AlertaStock(s.ProductoId, s.Producto.Descripcion,
                s.CantidadActual, s.Producto.StockMinimo, s.FechaVencimientoProxima))
            .ToListAsync();
    }
}
