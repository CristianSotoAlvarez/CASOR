namespace Casor.Domain.Entidades;

/// <summary>Stock por lote: la unidad real de inventario. Hace posible FEFO y libros ISP. (RF-04)</summary>
public class Lote
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductoId { get; set; }
    public required string NumeroLote { get; set; }
    public DateOnly FechaVencimiento { get; set; }
    public int Cantidad { get; set; }
}
