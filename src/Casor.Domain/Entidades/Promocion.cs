namespace Casor.Domain.Entidades;

public class Promocion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Nombre { get; set; }
    public TipoPromocion Tipo { get; set; }
    public decimal Valor { get; set; }                   // % / monto / precio oferta según tipo
    public DateOnly VigenteDesde { get; set; }
    public DateOnly VigenteHasta { get; set; }
    public bool Activa { get; set; } = true;
}
