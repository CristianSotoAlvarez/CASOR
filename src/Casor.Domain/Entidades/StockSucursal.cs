namespace Casor.Domain.Entidades;

/// <summary>Única fuente de verdad del inventario (v1.1 sin lotes). PK compuesta.</summary>
public class StockSucursal
{
    public Guid ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;
    public Guid SucursalId { get; set; }
    public Sucursal Sucursal { get; set; } = null!;
    public int CantidadActual { get; set; }              // CHECK >= 0 en la base
    public DateOnly? FechaVencimientoProxima { get; set; } // parche manual de vencimientos
}
