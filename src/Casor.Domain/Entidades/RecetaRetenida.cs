namespace Casor.Domain.Entidades;

/// <summary>Registro legal ISP (RF-06). 1:1 parcial con DetalleVenta. ⚠ D9 pendiente con QF.</summary>
public class RecetaRetenida
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DetalleVentaId { get; set; }             // único → 1:1
    public DetalleVenta DetalleVenta { get; set; } = null!;
    public TipoReceta Tipo { get; set; }
    public long FolioLibro { get; set; }                 // correlativo del libro legal
    public string? SerieRecetaCheque { get; set; }       // obligatoria si Tipo=Cheque
    public DateOnly Fecha { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public required string NombreMedico { get; set; }
    public required string RutMedico { get; set; }
    public required string NombrePaciente { get; set; }  // cifrar en reposo (app-level)
    public required string RutPaciente { get; set; }     // cifrar en reposo (app-level)
}
