namespace Casor.Domain.Entidades;

/// <summary>Razón social EMISORA de los DTE. Multi-tenant preparado (una fila en V1).</summary>
public class Empresa
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Rut { get; set; }
    public required string RazonSocial { get; set; }
    public string? Giro { get; set; }
    public string? DireccionMatriz { get; set; }
    public bool Activa { get; set; } = true;
    public DateTimeOffset CreadaEn { get; set; } = DateTimeOffset.UtcNow;

    public List<Sucursal> Sucursales { get; set; } = [];
}
