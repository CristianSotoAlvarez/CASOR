using Casor.Domain.Entidades;

namespace Casor.Domain.Servicios;

/// <summary>
/// Regla FEFO (First Expired, First Out): al vender, sale primero
/// el lote con vencimiento más próximo que tenga stock. (RF-04)
/// Esta clase es el ejemplo de cómo se escribe TODA regla de negocio:
/// pura, sin dependencias, y con su test en Casor.Tests.
/// </summary>
public static class SelectorFefo
{
    public static Lote? SeleccionarLote(IEnumerable<Lote> lotes, DateOnly hoy) =>
        lotes.Where(l => l.Cantidad > 0 && l.FechaVencimiento >= hoy)
             .OrderBy(l => l.FechaVencimiento)
             .FirstOrDefault();

    /// <summary>Lotes que vencen dentro del umbral: alimenta las alertas del POS.</summary>
    public static IEnumerable<Lote> ProximosAVencer(IEnumerable<Lote> lotes, DateOnly hoy, int diasUmbral = 90) =>
        lotes.Where(l => l.Cantidad > 0
                      && l.FechaVencimiento >= hoy
                      && l.FechaVencimiento <= hoy.AddDays(diasUmbral))
             .OrderBy(l => l.FechaVencimiento);
}
