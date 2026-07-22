namespace Casor.Domain.Entidades;

public class DetalleAjuste
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AjusteId { get; set; }
    public AjusteInventario Ajuste { get; set; } = null!;
    public Guid ProductoId { get; set; }                 // v1.1: por producto (sin lotes)
    public Producto Producto { get; set; } = null!;
    public int CantidadSistema { get; set; }
    public int CantidadContada { get; set; }
    public int Delta { get; set; }                       // contada - sistema
}
