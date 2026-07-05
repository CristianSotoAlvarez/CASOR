namespace Casor.Domain.Entidades;

/// <summary>
/// Producto del catálogo. Los atributos variables (principio activo,
/// acción terapéutica, bioequivalencia, condición de venta) viven en
/// AtributosJson y se persisten como JSONB en PostgreSQL. (RF-01)
/// </summary>
public class Producto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string CodigoBarras { get; set; }
    public required string Nombre { get; set; }
    public decimal PrecioVenta { get; set; }          // dinero SIEMPRE en decimal
    public int StockMinimo { get; set; }
    public int StockMaximo { get; set; }
    public string AtributosJson { get; set; } = "{}"; // JSONB en Postgres
    public List<Lote> Lotes { get; set; } = new();
}
