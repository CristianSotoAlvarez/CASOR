namespace Casor.Domain.Entidades;

public class AjusteInventario
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public TipoAjuste Tipo { get; set; }
    public Guid UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public Guid SucursalId { get; set; }
    public Sucursal Sucursal { get; set; } = null!;
    public DateTimeOffset Fecha { get; set; } = DateTimeOffset.UtcNow;
    public string? Comprobante { get; set; }
    public string? Observacion { get; set; }

    public List<DetalleAjuste> Detalles { get; set; } = [];
}
