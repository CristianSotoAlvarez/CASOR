namespace Casor.Domain.Entidades;

public class Proveedor
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Rut { get; set; }
    public required string RazonSocial { get; set; }
    public string? Contacto { get; set; }
    public bool Activo { get; set; } = true;
}
