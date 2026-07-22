using Casor.Application.Comun;
using Casor.Domain;
using Casor.Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Casor.Application.Catalogo;

public record CrearProductoRequest(
    string CodigoBarra, string Descripcion, decimal PrecioVenta,
    decimal? CostoUnitario, string? NombrePrincipioActivo, string? Concentracion,
    string? Linea, CondicionVenta CondicionVenta,
    int StockMinimo = 0, int StockMaximo = 0);

/// <summary>Alta de producto (RF-01) con su primer precio histórico.</summary>
public class CrearProducto(ICasorDb db)
{
    public async Task<Guid> EjecutarAsync(CrearProductoRequest req, Guid usuarioId)
    {
        if (req.PrecioVenta < 0)
            throw new ArgumentException("El precio no puede ser negativo");
        if (await db.Productos.AnyAsync(p => p.CodigoBarra == req.CodigoBarra))
            throw new InvalidOperationException(
                $"Ya existe un producto con código de barras {req.CodigoBarra}");

        // Transacción: producto + histórico nacen juntos o no nace ninguno.
        await using var tx = await db.IniciarTransaccionAsync();

        // Patrón "buscar o crear" para el principio activo (tolerante a Golan:
        // muchos productos vienen sin principio — es opcional de verdad).
        Guid? principioId = null;
        if (!string.IsNullOrWhiteSpace(req.NombrePrincipioActivo))
        {
            var nombre = req.NombrePrincipioActivo.Trim().ToUpperInvariant();
            var pa = await db.PrincipiosActivos.FirstOrDefaultAsync(x => x.Nombre == nombre);
            if (pa is null)
            {
                pa = new PrincipioActivo { Nombre = nombre };
                db.PrincipiosActivos.Add(pa);
            }
            principioId = pa.Id;
        }

        var producto = new Producto
        {
            CodigoBarra = req.CodigoBarra.Trim(),
            Descripcion = req.Descripcion.Trim(),
            PrecioVenta = req.PrecioVenta,
            CostoUnitario = req.CostoUnitario,
            PrincipioActivoId = principioId,
            Concentracion = req.Concentracion?.Trim(),
            Linea = req.Linea?.Trim(),
            CondicionVenta = req.CondicionVenta,
            StockMinimo = req.StockMinimo,
            StockMaximo = req.StockMaximo
        };
        db.Productos.Add(producto);

        db.PreciosHistoricos.Add(new PrecioHistorico
        {
            ProductoId = producto.Id,
            Precio = req.PrecioVenta,
            VigenteDesde = DateTimeOffset.UtcNow,
            UsuarioId = usuarioId
        });

        await db.SaveChangesAsync();
        await tx.ConfirmarAsync();
        return producto.Id;
    }
}
