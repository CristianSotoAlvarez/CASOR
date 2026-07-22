namespace Casor.Domain.Entidades;

public class Pago
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid VentaId { get; set; }
    public Venta Venta { get; set; } = null!;
    public MedioPago MetodoPago { get; set; }
    public decimal Monto { get; set; }
}
