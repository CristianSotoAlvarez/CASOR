namespace Casor.Domain.Entidades;

public class Producto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string CodigoBarra { get; set; }
    public required string Descripcion { get; set; }
    public Guid? PrincipioActivoId { get; set; }         // null = no medicamento
    public PrincipioActivo? PrincipioActivo { get; set; }
    public string? Concentracion { get; set; }           // '500 mg' — define bioequivalencia
    public string? Linea { get; set; }
    public bool Refrigerado { get; set; }
    public CondicionVenta CondicionVenta { get; set; } = CondicionVenta.VentaDirecta;
    public string? NombreIsp { get; set; }
    public string? TitularIsp { get; set; }
    public decimal PrecioVenta { get; set; }             // BRUTO (IVA incluido, D1)
    public decimal? CostoUnitario { get; set; }          // NETO, último costo de compra (v1.1)
    public int StockMinimo { get; set; }
    public int StockMaximo { get; set; }
    public int CantidadPresentacion { get; set; } = 1;
    public Guid? PresentacionId { get; set; }
    public TipoPresentacion? Presentacion { get; set; }
    public bool Activo { get; set; } = true;
}
