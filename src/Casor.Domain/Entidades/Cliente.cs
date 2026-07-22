namespace Casor.Domain.Entidades;

public class Cliente
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? RutCliente { get; set; }
    public required string Nombre { get; set; }
    public string? RazonSocial { get; set; }             // RECEPTOR de factura (v1.1)
    public string? Giro { get; set; }
    public string? Email { get; set; }
    public string? NumeroTelefonico { get; set; }
    public string? Direccion { get; set; }
    public DateTimeOffset CreadoEn { get; set; } = DateTimeOffset.UtcNow;
}
