namespace Casor.Domain.Entidades;

/// <summary>Bitácora append-only (RNF-03): la app solo INSERTa, jamás edita ni borra.</summary>
public class Auditoria
{
    public long Id { get; set; }                         // identity
    public Guid? UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public required string Accion { get; set; }          // 'venta.anular', 'precio.cambiar'...
    public required string Entidad { get; set; }
    public Guid? EntidadId { get; set; }
    public string? Datos { get; set; }                   // snapshot JSON del cambio
    public DateTimeOffset Fecha { get; set; } = DateTimeOffset.UtcNow;
}
