namespace Casor.Domain.Entidades;

public class Sucursal
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;
    public required string Nombre { get; set; }
    public required string Direccion { get; set; }
    public bool Activa { get; set; } = true;
}
