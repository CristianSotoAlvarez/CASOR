using Casor.Application.Comun;
using Microsoft.EntityFrameworkCore;

namespace Casor.Application.Catalogo;

public record Alternativa(
    Guid Id, string Descripcion, decimal PrecioVenta, int StockEnSucursal);

/// <summary>
/// Motor de bioequivalentes (RF-05): mismo principio activo Y misma
/// concentración (sin la concentración sugeriríamos dosis equivocadas).
/// Ordena por precio: la alternativa más conveniente primero.
/// </summary>
public class AlternativasBioequivalentes(ICasorDb db)
{
    public async Task<List<Alternativa>> EjecutarAsync(Guid productoId, Guid sucursalId)
    {
        var origen = await db.Productos.FirstOrDefaultAsync(p => p.Id == productoId)
            ?? throw new InvalidOperationException("Producto no existe");

        if (origen.PrincipioActivoId is null)
            return [];   // sin principio activo no hay equivalencia posible

        return await db.Productos
            .Where(p => p.Activo
                     && p.Id != productoId
                     && p.PrincipioActivoId == origen.PrincipioActivoId
                     && p.Concentracion == origen.Concentracion)
            .Select(p => new Alternativa(
                p.Id, p.Descripcion, p.PrecioVenta,
                db.StockSucursales
                    .Where(s => s.ProductoId == p.Id && s.SucursalId == sucursalId)
                    .Select(s => s.CantidadActual)
                    .FirstOrDefault()))
            .OrderBy(a => a.PrecioVenta)
            .ToListAsync();
    }
}
