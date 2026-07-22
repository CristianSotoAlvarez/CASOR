using Casor.Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Casor.Application.Comun;

/// <summary>
/// Lo que la capa de aplicación necesita de la base, SIN conocer la
/// implementación (regla de capas: Application no referencia Infrastructure).
/// CasorDbContext implementa esta interfaz.
/// </summary>
public interface ICasorDb
{
    DbSet<Producto> Productos { get; }
    DbSet<PrincipioActivo> PrincipiosActivos { get; }
    DbSet<TipoPresentacion> TiposPresentacion { get; }
    DbSet<PrecioHistorico> PreciosHistoricos { get; }
    DbSet<StockSucursal> StockSucursales { get; }
    DbSet<Sucursal> Sucursales { get; }
    DbSet<Auditoria> Auditorias { get; }
    DbSet<Proveedor> Proveedores { get; }
    DbSet<OrdenCompra> OrdenesCompra { get; }
    DbSet<Recepcion> Recepciones { get; }
    DbSet<AjusteInventario> AjustesInventario { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task<ITransaccion> IniciarTransaccionAsync();
}

/// <summary>Transacción abstracta: si se libera sin Confirmar, se revierte sola.</summary>
public interface ITransaccion : IAsyncDisposable
{
    Task ConfirmarAsync();
}
