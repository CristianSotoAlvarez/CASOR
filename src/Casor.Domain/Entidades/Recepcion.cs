namespace Casor.Domain.Entidades;

/// <summary>
/// Entrada de mercadería (RF-07): el momento donde se captura el costo real
/// (neto, de la factura del proveedor) y el vencimiento próximo.
/// </summary>
public class Recepcion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? OrdenCompraId { get; set; }             // null = llegó sin orden (D11)
    public OrdenCompra? OrdenCompra { get; set; }
    public Guid ProveedorId { get; set; }
    public Proveedor Proveedor { get; set; } = null!;
    public Guid SucursalId { get; set; }
    public Sucursal Sucursal { get; set; } = null!;
    public Guid UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public string? NumeroFactura { get; set; }
    public DateTimeOffset Fecha { get; set; } = DateTimeOffset.UtcNow;

    public List<DetalleRecepcion> Detalles { get; set; } = [];
}

public class DetalleRecepcion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RecepcionId { get; set; }
    public Recepcion Recepcion { get; set; } = null!;
    public Guid ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;
    public int Cantidad { get; set; }
    public decimal CostoUnitario { get; set; }           // NETO, de la factura
    public DateOnly? FechaVencimiento { get; set; }      // actualiza el parche de vencimientos
}
