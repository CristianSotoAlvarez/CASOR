namespace Casor.Domain.Entidades;

public class Usuario
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Nombre { get; set; }
    public string? Rut { get; set; }
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }   // Argon2id — JAMÁS texto plano
    public RolUsuario Rol { get; set; }
    public bool Activo { get; set; } = true;            // deshabilitar, nunca borrar
}
