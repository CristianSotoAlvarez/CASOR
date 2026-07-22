namespace Casor.Domain.Entidades;

public class DetalleVenta
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid VentaId { get; set; }
    public Venta Venta { get; set; } = null!;
    public Guid ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }          // congelado al vender
    public decimal Descuento { get; set; }
    public Guid? PromocionId { get; set; }               // por qué hubo descuento
    public Promocion? Promocion { get; set; }
    public decimal Subtotal { get; set; }
}
