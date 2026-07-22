using Casor.Application.Catalogo;
using Casor.Application.Stock;
using Casor.Domain;
using System.Security.Claims;

namespace Casor.Api.Endpoints;

public static class InventarioEndpoints
{
    /// <summary>Id del usuario autenticado, leído del carnet JWT.</summary>
    private static Guid UsuarioId(ClaimsPrincipal user) =>
        Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? user.FindFirstValue("sub")!);

    public static void MapInventario(this WebApplication app)
    {
        var admin = app.MapGroup("/api")
            .RequireAuthorization(p => p.RequireRole(nameof(RolUsuario.Admin)));

        // --- Catálogo (solo Admin) ---
        admin.MapPost("/productos", async (CrearProductoRequest req,
            CrearProducto uc, ClaimsPrincipal user) =>
        {
            var id = await uc.EjecutarAsync(req, UsuarioId(user));
            return Results.Created($"/api/productos/{id}", new { id });
        });

        admin.MapPut("/productos/{id:guid}/precio", async (Guid id, CambioPrecioBody body,
            CambiarPrecio uc, ClaimsPrincipal user) =>
        {
            await uc.EjecutarAsync(id, body.PrecioNuevo, UsuarioId(user));
            return Results.NoContent();
        });

        admin.MapPost("/stock/movimiento", async (MovimientoBody body,
            RegistrarMovimientoStock uc, ClaimsPrincipal user) =>
        {
            var resultado = await uc.EjecutarAsync(body.ProductoId, body.SucursalId,
                body.Delta, body.Motivo, UsuarioId(user));
            return Results.Ok(new { stockResultante = resultado });
        });

        admin.MapGet("/stock/alertas/{sucursalId:guid}", async (Guid sucursalId,
            ConsultarAlertas uc) => Results.Ok(new
        {
            bajoMinimo = await uc.BajoMinimoAsync(sucursalId),
            vencimientos = await uc.VencimientosProximosAsync(sucursalId)
        }));

        // --- Búsqueda y bioequivalentes: también para el POS (rol Pos) ---
        var pos = app.MapGroup("/api")
            .RequireAuthorization(p => p.RequireRole(
                nameof(RolUsuario.Admin), nameof(RolUsuario.Pos)));

        pos.MapGet("/productos/buscar", async (string q, BuscarProductos uc) =>
            Results.Ok(await uc.EjecutarAsync(q)));

        pos.MapGet("/productos/{id:guid}/alternativas/{sucursalId:guid}",
            async (Guid id, Guid sucursalId, AlternativasBioequivalentes uc) =>
                Results.Ok(await uc.EjecutarAsync(id, sucursalId)));
    }

    public record CambioPrecioBody(decimal PrecioNuevo);
    public record MovimientoBody(Guid ProductoId, Guid SucursalId, int Delta, string Motivo);
}
