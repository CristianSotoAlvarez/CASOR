namespace Casor.Domain.Entidades;

public class CajaSesion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public Guid SucursalId { get; set; }
    public Sucursal Sucursal { get; set; } = null!;
    public DateTimeOffset FechaApertura { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? FechaCierre { get; set; }
    public bool CierreX { get; set; }
    public bool CierreZ { get; set; }                    // bloquea el día (RF-08)
    public decimal MontoApertura { get; set; }
    public decimal? MontoCierreDeclarado { get; set; }
    public decimal? MontoCierreSistema { get; set; }     // diferencia = descuadre
}
