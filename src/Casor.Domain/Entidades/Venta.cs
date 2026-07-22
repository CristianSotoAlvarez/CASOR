namespace Casor.Domain.Entidades;

public class Venta
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CajaSesionId { get; set; }
    public CajaSesion CajaSesion { get; set; } = null!;
    public Guid UsuarioId { get; set; }                  // vendedor (RF-09)
    public Usuario Usuario { get; set; } = null!;
    public Guid? ClienteId { get; set; }                 // null = boleta anónima
    public Cliente? Cliente { get; set; }
    public Guid? VentaOrigenId { get; set; }             // NC → venta que anula
    public Venta? VentaOrigen { get; set; }
    public long? Folio { get; set; }
    public TipoDocumento TipoDocumento { get; set; }
    public EstadoDte EstadoDte { get; set; } = EstadoDte.Pendiente;
    public EstadoVenta EstadoVenta { get; set; } = EstadoVenta.Completada;
    public decimal TotalNeto { get; set; }
    public decimal TotalBruto { get; set; }
    public DateTimeOffset FechaVenta { get; set; } = DateTimeOffset.UtcNow;
    public bool EmitidaOffline { get; set; }

    public List<DetalleVenta> Detalles { get; set; } = [];
    public List<Pago> Pagos { get; set; } = [];
}
