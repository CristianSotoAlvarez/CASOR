using Casor.Application.Catalogo;
using Casor.Application.Stock;
using Casor.Domain;
using Casor.Domain.Entidades;
using Casor.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Casor.Tests;

/// <summary>
/// Tests del dominio de inventario contra SQLite EN MEMORIA: una base real
/// (transacciones incluidas) que nace y muere con cada test. Es además la
/// prueba viviente de que el modelo corre en SQLite — el requisito offline.
/// </summary>
public class InventarioTests : IAsyncLifetime
{
    private SqliteConnection _conn = null!;
    private CasorDbContext _db = null!;
    private Guid _usuarioId, _sucursalId;

    public async Task InitializeAsync()
    {
        _conn = new SqliteConnection("DataSource=:memory:");
        await _conn.OpenAsync();
        var opt = new DbContextOptionsBuilder<CasorDbContext>().UseSqlite(_conn).Options;
        _db = new CasorDbContext(opt);
        await _db.Database.EnsureCreatedAsync();

        var empresa = new Empresa { Rut = "1-9", RazonSocial = "Test SpA" };
        var sucursal = new Sucursal { Empresa = empresa, Nombre = "Central", Direccion = "X 123" };
        var usuario = new Usuario { Nombre = "Tester", Username = "test",
            PasswordHash = "x", Rol = RolUsuario.Admin };
        _db.AddRange(empresa, sucursal, usuario);
        await _db.SaveChangesAsync();
        _usuarioId = usuario.Id; _sucursalId = sucursal.Id;
    }

    public async Task DisposeAsync() { await _db.DisposeAsync(); await _conn.DisposeAsync(); }

    private async Task<Guid> ProductoDePrueba(decimal precio = 1000m) =>
        await new CrearProducto(_db).EjecutarAsync(new CrearProductoRequest(
            Guid.NewGuid().ToString("N")[..13], "PARACETAMOL 500MG X16", precio,
            null, "PARACETAMOL", "500 mg", "FARMACIA", CondicionVenta.VentaDirecta,
            StockMinimo: 5), _usuarioId);

    [Fact]
    public async Task Crear_producto_genera_su_primer_precio_historico()
    {
        var id = await ProductoDePrueba(1990m);
        var hist = await _db.PreciosHistoricos.SingleAsync(p => p.ProductoId == id);
        Assert.Equal(1990m, hist.Precio);
        Assert.Null(hist.VigenteHasta);              // es el vigente
    }

    [Fact]
    public async Task Cambiar_precio_archiva_el_anterior()
    {
        var id = await ProductoDePrueba(1000m);
        await new CambiarPrecio(_db).EjecutarAsync(id, 1200m, _usuarioId);

        var historicos = await _db.PreciosHistoricos
            .Where(p => p.ProductoId == id).OrderBy(p => p.VigenteDesde).ToListAsync();
        Assert.Equal(2, historicos.Count);
        Assert.NotNull(historicos[0].VigenteHasta);  // el viejo quedó cerrado
        Assert.Null(historicos[1].VigenteHasta);     // el nuevo es el vigente
        Assert.Equal(1200m, (await _db.Productos.FindAsync(id))!.PrecioVenta);
    }

    [Fact]
    public async Task Codigo_de_barras_duplicado_se_rechaza()
    {
        var uc = new CrearProducto(_db);
        var req = new CrearProductoRequest("7801234567890", "PRODUCTO A", 500m,
            null, null, null, null, CondicionVenta.VentaDirecta);
        await uc.EjecutarAsync(req, _usuarioId);
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => uc.EjecutarAsync(req with { Descripcion = "PRODUCTO B" }, _usuarioId));
    }

    [Fact]
    public async Task Stock_nunca_queda_negativo()
    {
        var id = await ProductoDePrueba();
        var uc = new RegistrarMovimientoStock(_db);
        await uc.EjecutarAsync(id, _sucursalId, +10, "recepcion", _usuarioId);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => uc.EjecutarAsync(id, _sucursalId, -11, "venta", _usuarioId));

        // y tras el intento fallido, el stock sigue intacto (transacción revertida)
        var stock = await _db.StockSucursales
            .SingleAsync(s => s.ProductoId == id && s.SucursalId == _sucursalId);
        Assert.Equal(10, stock.CantidadActual);
    }

    [Fact]
    public async Task Alerta_aparece_bajo_el_minimo()
    {
        var id = await ProductoDePrueba();            // StockMinimo = 5
        var mov = new RegistrarMovimientoStock(_db);
        await mov.EjecutarAsync(id, _sucursalId, +4, "recepcion", _usuarioId);

        var alertas = await new ConsultarAlertas(_db).BajoMinimoAsync(_sucursalId);
        Assert.Single(alertas);
        Assert.Equal(4, alertas[0].CantidadActual);
    }

    [Fact]
    public async Task Bioequivalentes_exigen_misma_concentracion()
    {
        var uc = new CrearProducto(_db);
        var a = await uc.EjecutarAsync(new CrearProductoRequest("1111111111111",
            "MARCA CARA 500", 5000m, null, "PARACETAMOL", "500 mg", null,
            CondicionVenta.VentaDirecta), _usuarioId);
        var b = await uc.EjecutarAsync(new CrearProductoRequest("2222222222222",
            "GENERICO 500", 990m, null, "PARACETAMOL", "500 mg", null,
            CondicionVenta.VentaDirecta), _usuarioId);
        await uc.EjecutarAsync(new CrearProductoRequest("3333333333333",
            "OTRO 100MG", 990m, null, "PARACETAMOL", "100 mg", null,
            CondicionVenta.VentaDirecta), _usuarioId);   // distinta dosis: NO debe salir

        var alts = await new AlternativasBioequivalentes(_db).EjecutarAsync(a, _sucursalId);
        Assert.Single(alts);
        Assert.Equal(b, alts[0].Id);
    }
}
