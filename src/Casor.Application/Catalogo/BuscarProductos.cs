using Casor.Application.Comun;
using Microsoft.EntityFrameworkCore;

namespace Casor.Application.Catalogo;

public record ProductoEncontrado(
    Guid Id, string CodigoBarra, string Descripcion, decimal PrecioVenta,
    string? PrincipioActivo, string? Concentracion, string CondicionVenta);

/// <summary>
/// La búsqueda que usará el POS (RNF-02): código de barras exacto primero
/// (el escáner), texto parcial después (el cajero tipeando).
/// </summary>
public class BuscarProductos(ICasorDb db)
{
    public async Task<List<ProductoEncontrado>> EjecutarAsync(string termino, int max = 20)
    {
        termino = termino.Trim();
        if (termino.Length == 0) return [];

        var q = db.Productos.Where(p => p.Activo);

        // ¿Parece un código de barras? (solo dígitos) → match exacto primero
        var query = termino.All(char.IsDigit)
            ? q.Where(p => p.CodigoBarra == termino)
            : q.Where(p => p.Descripcion.ToLower().Contains(termino.ToLower()));
        // (ToLower+Contains y no ILike: debe traducir igual en PostgreSQL y SQLite)

        return await query
            .OrderBy(p => p.Descripcion)
            .Take(max)
            .Select(p => new ProductoEncontrado(
                p.Id, p.CodigoBarra, p.Descripcion, p.PrecioVenta,
                p.PrincipioActivo != null ? p.PrincipioActivo.Nombre : null,
                p.Concentracion, p.CondicionVenta.ToString()))
            .ToListAsync();
    }
}
