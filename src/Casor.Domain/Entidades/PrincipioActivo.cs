namespace Casor.Domain.Entidades;

/// <summary>Base del motor de bioequivalentes (RF-05, decisión D2).</summary>
public class PrincipioActivo
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Nombre { get; set; }
}
