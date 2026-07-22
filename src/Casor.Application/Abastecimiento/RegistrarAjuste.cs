using Casor.Application.Comun;
using Casor.Application.Stock;
using Casor.Domain;
using Casor.Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Casor.Application.Abastecimiento;

public record LineaConteo(Guid ProductoId, int CantidadContada);
public record RegistrarAjusteRequest(TipoAjuste Tipo, Guid SucursalId,
    string? Comprobante, string? Observacion, List<LineaConteo> Conteos);

/// <summary>
/// Inventario cíclico y ajustes (RF-07): compara lo contado contra el sistema,
/// guarda los TRES números (sistema/contada/delta — auditable años después)
/// y aplica la corrección por el punto único de stock.
/// </summary>
public class RegistrarAjuste(ICasorDb db, RegistrarMovimientoStock movimiento)
{
    public async Task<Guid> EjecutarAsync(RegistrarAjusteRequest req, Guid usuarioId)
    {
        if (req.Conteos.Count == 0)
            throw new ArgumentException("Un ajuste sin conteos no es un ajuste");
        if (req.Conteos.Any(c => c.CantidadContada < 0))
            throw new ArgumentException("Lo contado no puede ser negativo");

        await using var tx = await db.IniciarTransaccionAsync();

        var ajuste = new AjusteInventario
        {
            Tipo = req.Tipo,
            UsuarioId = usuarioId,
            SucursalId = req.SucursalId,
            Comprobante = req.Comprobante,
            Observacion = req.Observacion
        };
        db.AjustesInventario.Add(ajuste);

        foreach (var conteo in req.Conteos)
        {
            var actual = await db.StockSucursales
                .Where(s => s.ProductoId == conteo.ProductoId && s.SucursalId == req.SucursalId)
                .Select(s => s.CantidadActual)
                .FirstOrDefaultAsync();               // sin fila = 0 en sistema

            var delta = conteo.CantidadContada - actual;

            ajuste.Detalles.Add(new DetalleAjuste
            {
                ProductoId = conteo.ProductoId,
                CantidadSistema = actual,
                CantidadContada = conteo.CantidadContada,
                Delta = delta
            });

            if (delta != 0)
                await movimiento.EjecutarAsync(conteo.ProductoId, req.SucursalId,
                    delta, $"ajuste:{req.Tipo}:{ajuste.Id}", usuarioId);
        }

        await db.SaveChangesAsync();
        await tx.ConfirmarAsync();
        return ajuste.Id;
    }
}
