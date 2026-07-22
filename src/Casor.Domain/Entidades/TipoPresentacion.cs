namespace Casor.Domain.Entidades;

public class TipoPresentacion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string NombreMedida { get; set; }
}
