namespace Casor.Domain.Entidades;

/// <summary>Pedido al proveedor (RF-07). Opcional: una recepción puede llegar sin orden (D11).</summary>
public class OrdenCompra
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProveedorId { get; set; }
    public Proveedor Proveedor { get; set; } = null!;
    public Guid UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public EstadoOrdenCompra Estado { get; set; } = EstadoOrdenCompra.Borrador;
    public DateTimeOffset Fecha { get; set; } = DateTimeOffset.UtcNow;

    public List<DetalleOrdenCompra> Detalles { get; set; } = [];
}

public class DetalleOrdenCompra
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrdenCompraId { get; set; }
    public OrdenCompra OrdenCompra { get; set; } = null!;
    public Guid ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;
    public int CantidadPedida { get; set; }
    public decimal? CostoUnitarioEsperado { get; set; }
}
