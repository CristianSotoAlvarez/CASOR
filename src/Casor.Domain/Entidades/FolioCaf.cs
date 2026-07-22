namespace Casor.Domain.Entidades;

/// <summary>Rangos de folios autorizados por el SII (D13, modelo SimpleAPI).</summary>
public class FolioCaf
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;
    public Guid? SucursalId { get; set; }                // null = pool central
    public Sucursal? Sucursal { get; set; }
    public TipoDocumento TipoDocumento { get; set; }
    public long RangoDesde { get; set; }
    public long RangoHasta { get; set; }
    public long? UltimoFolioUsado { get; set; }
    public required string XmlCaf { get; set; }
}
