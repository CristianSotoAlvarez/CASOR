using Casor.Application.Abastecimiento;
using Casor.Domain;
using Casor.Domain.Entidades;
using Casor.Application.Comun;
using System.Security.Claims;

namespace Casor.Api.Endpoints;

public static class AbastecimientoEndpoints
{
    private static Guid UsuarioId(ClaimsPrincipal user) =>
        Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? user.FindFirstValue("sub")!);

    public static void MapAbastecimiento(this WebApplication app)
    {
        var g = app.MapGroup("/api")
            .RequireAuthorization(p => p.RequireRole(nameof(RolUsuario.Admin)));

        g.MapPost("/proveedores", async (Proveedor p, ICasorDb db) =>
        {
            db.Proveedores.Add(p);
            await db.SaveChangesAsync();
            return Results.Created($"/api/proveedores/{p.Id}", new { p.Id });
        });

        g.MapPost("/recepciones", async (RegistrarRecepcionRequest req,
            RegistrarRecepcion uc, ClaimsPrincipal user) =>
        {
            var id = await uc.EjecutarAsync(req, UsuarioId(user));
            return Results.Created($"/api/recepciones/{id}", new { id });
        });

        g.MapPost("/ajustes", async (RegistrarAjusteRequest req,
            RegistrarAjuste uc, ClaimsPrincipal user) =>
        {
            var id = await uc.EjecutarAsync(req, UsuarioId(user));
            return Results.Created($"/api/ajustes/{id}", new { id });
        });
    }
}
