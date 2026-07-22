namespace Casor.Domain.Entidades;

public class PrecioHistorico
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;
    public decimal Precio { get; set; }
    public DateTimeOffset VigenteDesde { get; set; }
    public DateTimeOffset? VigenteHasta { get; set; }    // null = vigente hoy
    public Guid UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;
}
