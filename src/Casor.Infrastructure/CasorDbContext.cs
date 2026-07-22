using Casor.Application.Comun;
using Casor.Domain;
using Casor.Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Casor.Infrastructure;

/// <summary>
/// Mapa entre las entidades del dominio y la base de datos.
/// Referencia semántica: docs/modelo-datos/casor_schema.sql (v1.1).
/// Divergencias documentadas: enums como texto y JSON como texto,
/// para que el mismo modelo corra en PostgreSQL (central) y SQLite (cajas).
/// </summary>
public class CasorDbContext(DbContextOptions<CasorDbContext> options) : DbContext(options), ICasorDb
{
    public async Task<ITransaccion> IniciarTransaccionAsync() =>
        new TransaccionEf(await Database.BeginTransactionAsync());

    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Sucursal> Sucursales => Set<Sucursal>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<TipoPresentacion> TiposPresentacion => Set<TipoPresentacion>();
    public DbSet<PrincipioActivo> PrincipiosActivos => Set<PrincipioActivo>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<PrecioHistorico> PreciosHistoricos => Set<PrecioHistorico>();
    public DbSet<StockSucursal> StockSucursales => Set<StockSucursal>();
    public DbSet<Promocion> Promociones => Set<Promocion>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<CajaSesion> CajaSesiones => Set<CajaSesion>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<DetalleVenta> DetallesVenta => Set<DetalleVenta>();
    public DbSet<Pago> Pagos => Set<Pago>();
    public DbSet<RecetaRetenida> RecetasRetenidas => Set<RecetaRetenida>();
    public DbSet<AjusteInventario> AjustesInventario => Set<AjusteInventario>();
    public DbSet<DetalleAjuste> DetallesAjuste => Set<DetalleAjuste>();
    public DbSet<FolioCaf> FoliosCaf => Set<FolioCaf>();
    public DbSet<Auditoria> Auditorias => Set<Auditoria>();

    protected override void ConfigureConventions(ModelConfigurationBuilder b)
    {
        // Dinero SIEMPRE decimal(12,0): CLP sin decimales (regla de oro).
        b.Properties<decimal>().HavePrecision(12, 0);
        // Enums como texto legible en la base (portabilidad PG/SQLite).
        b.Properties<RolUsuario>().HaveConversion<string>().HaveMaxLength(30);
        b.Properties<CondicionVenta>().HaveConversion<string>().HaveMaxLength(40);
        b.Properties<TipoDocumento>().HaveConversion<string>().HaveMaxLength(30);
        b.Properties<EstadoVenta>().HaveConversion<string>().HaveMaxLength(20);
        b.Properties<EstadoDte>().HaveConversion<string>().HaveMaxLength(30);
        b.Properties<MedioPago>().HaveConversion<string>().HaveMaxLength(20);
        b.Properties<TipoAjuste>().HaveConversion<string>().HaveMaxLength(30);
        b.Properties<TipoReceta>().HaveConversion<string>().HaveMaxLength(30);
        b.Properties<TipoPromocion>().HaveConversion<string>().HaveMaxLength(20);
    }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Empresa>(e =>
        {
            e.ToTable("empresa");
            e.HasIndex(x => x.Rut).IsUnique();
            e.Property(x => x.Rut).HasMaxLength(12);
            e.Property(x => x.RazonSocial).HasMaxLength(120);
        });

        mb.Entity<Sucursal>(e =>
        {
            e.ToTable("sucursal");
            e.Property(x => x.Nombre).HasMaxLength(80);
        });

        mb.Entity<Usuario>(e =>
        {
            e.ToTable("usuario");
            e.HasIndex(x => x.Username).IsUnique();
            e.Property(x => x.Username).HasMaxLength(40);
        });

        mb.Entity<TipoPresentacion>(e =>
        {
            e.ToTable("tipo_presentacion");
            e.HasIndex(x => x.NombreMedida).IsUnique();
            e.Property(x => x.NombreMedida).HasMaxLength(40);
        });

        mb.Entity<PrincipioActivo>(e =>
        {
            e.ToTable("principio_activo");
            e.HasIndex(x => x.Nombre).IsUnique();      // sin duplicados: motor RF-05
            e.Property(x => x.Nombre).HasMaxLength(120);
        });

        mb.Entity<Producto>(e =>
        {
            e.ToTable("producto", t =>
            {
                t.HasCheckConstraint("ck_producto_precio", "\"PrecioVenta\" >= 0");
                t.HasCheckConstraint("ck_producto_stockmin", "\"StockMinimo\" >= 0");
            });
            e.HasIndex(x => x.CodigoBarra).IsUnique(); // la llave del escáner (RNF-02)
            e.HasIndex(x => x.Descripcion);
            e.HasIndex(x => x.PrincipioActivoId);      // bioequivalentes (RF-05)
            e.Property(x => x.CodigoBarra).HasMaxLength(20);
            e.Property(x => x.Descripcion).HasMaxLength(200);
        });

        mb.Entity<PrecioHistorico>(e =>
        {
            e.ToTable("precio_historico");
            e.HasIndex(x => new { x.ProductoId, x.VigenteDesde });
        });

        mb.Entity<StockSucursal>(e =>
        {
            e.ToTable("stock_sucursal", t =>
                t.HasCheckConstraint("ck_stock_no_negativo", "\"CantidadActual\" >= 0"));
            e.HasKey(x => new { x.ProductoId, x.SucursalId });   // PK compuesta
        });

        mb.Entity<Promocion>(e =>
        {
            e.ToTable("promocion");
            e.Property(x => x.Valor).HasPrecision(12, 2);        // única con decimales (%)
        });

        mb.Entity<Cliente>(e =>
        {
            e.ToTable("cliente");
            e.HasIndex(x => x.RutCliente).IsUnique();
            e.Property(x => x.RutCliente).HasMaxLength(12);
        });

        mb.Entity<CajaSesion>(e => e.ToTable("caja_sesion"));

        mb.Entity<Venta>(e =>
        {
            e.ToTable("venta");
            // folio jamás repetido por tipo de documento (obligación legal)
            e.HasIndex(x => new { x.TipoDocumento, x.Folio }).IsUnique()
             .HasFilter("\"Folio\" IS NOT NULL");
            e.HasIndex(x => x.FechaVenta);
            e.HasOne(x => x.VentaOrigen).WithMany()             // NC → venta anulada
             .HasForeignKey(x => x.VentaOrigenId);
            e.HasOne(x => x.Usuario).WithMany()
             .HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        });

        mb.Entity<DetalleVenta>(e =>
        {
            e.ToTable("detalle_venta");
            e.HasIndex(x => x.VentaId);
            e.HasIndex(x => x.ProductoId);
        });

        mb.Entity<Pago>(e =>
        {
            e.ToTable("pago");
            e.HasIndex(x => x.VentaId);
        });

        mb.Entity<RecetaRetenida>(e =>
        {
            e.ToTable("receta_retenida");
            e.HasIndex(x => x.DetalleVentaId).IsUnique();        // 1:1 parcial
            e.HasIndex(x => new { x.Tipo, x.FolioLibro }).IsUnique(); // un correlativo por libro
        });

        mb.Entity<AjusteInventario>(e => e.ToTable("ajuste_inventario"));

        mb.Entity<DetalleAjuste>(e => e.ToTable("detalle_ajuste"));

        mb.Entity<FolioCaf>(e =>
        {
            e.ToTable("folio_caf", t =>
                t.HasCheckConstraint("ck_caf_rango", "\"RangoHasta\" >= \"RangoDesde\""));
        });

        mb.Entity<Auditoria>(e =>
        {
            e.ToTable("auditoria");
            e.Property(x => x.Id).ValueGeneratedOnAdd();         // identity
        });
    }
}
