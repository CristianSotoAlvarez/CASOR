using Casor.Application.Abastecimiento;
using Casor.Application.Catalogo;
using Casor.Application.Stock;
using Casor.Domain;
using Casor.Domain.Entidades;
using Casor.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Casor.Tests;

public class AbastecimientoTests : IAsyncLifetime
{
    private SqliteConnection _conn = null!;
    private CasorDbContext _db = null!;
    private Guid _usuarioId, _sucursalId, _proveedorId, _productoId;

    public async Task InitializeAsync()
    {
        _conn = new SqliteConnection("DataSource=:memory:");
        await _conn.OpenAsync();
        _db = new CasorDbContext(new DbContextOptionsBuilder<CasorDbContext>()
            .UseSqlite(_conn).Options);
        await _db.Database.EnsureCreatedAsync();

        var empresa = new Empresa { Rut = "1-9", RazonSocial = "Test" };
        var sucursal = new Sucursal { Empresa = empresa, Nombre = "C", Direccion = "X" };
        var usuario = new Usuario { Nombre = "T", Username = "t", PasswordHash = "x", Rol = RolUsuario.Admin };
        var proveedor = new Proveedor { RazonSocial = "Droguería Test" };
        _db.AddRange(empresa, sucursal, usuario, proveedor);
        await _db.SaveChangesAsync();
        _usuarioId = usuario.Id; _sucursalId = sucursal.Id; _proveedorId = proveedor.Id;

        _productoId = await new CrearProducto(_db).EjecutarAsync(new CrearProductoRequest(
            "7800000000001", "IBUPROFENO 400 X20", 2990m, null, "IBUPROFENO", "400 mg",
            "FARMACIA", CondicionVenta.VentaDirecta), _usuarioId);
    }

    public async Task DisposeAsync() { await _db.DisposeAsync(); await _conn.DisposeAsync(); }

    [Fact]
    public async Task Recepcion_suma_stock_actualiza_costo_y_vencimiento()
    {
        var uc = new RegistrarRecepcion(_db, new RegistrarMovimientoStock(_db));
        await uc.EjecutarAsync(new RegistrarRecepcionRequest(null, _proveedorId, _sucursalId,
            "F-1234", [new LineaRecepcion(_productoId, 50, 1200m, new DateOnly(2027, 3, 1))]),
            _usuarioId);

        var stock = await _db.StockSucursales.SingleAsync(s => s.ProductoId == _productoId);
        Assert.Equal(50, stock.CantidadActual);
        Assert.Equal(new DateOnly(2027, 3, 1), stock.FechaVencimientoProxima);
        Assert.Equal(1200m, (await _db.Productos.FindAsync(_productoId))!.CostoUnitario);
    }

    [Fact]
    public async Task Recepcion_sin_orden_es_valida_D11()
    {
        var uc = new RegistrarRecepcion(_db, new RegistrarMovimientoStock(_db));
        var id = await uc.EjecutarAsync(new RegistrarRecepcionRequest(null, _proveedorId,
            _sucursalId, null, [new LineaRecepcion(_productoId, 5, 1000m, null)]), _usuarioId);
        Assert.NotEqual(Guid.Empty, id);
        Assert.Null((await _db.Recepciones.SingleAsync(r => r.Id == id)).OrdenCompraId);
    }

    [Fact]
    public async Task Vencimiento_solo_baja_nunca_sube()
    {
        var uc = new RegistrarRecepcion(_db, new RegistrarMovimientoStock(_db));
        await uc.EjecutarAsync(new RegistrarRecepcionRequest(null, _proveedorId, _sucursalId,
            null, [new LineaRecepcion(_productoId, 10, 1000m, new DateOnly(2026, 12, 1))]), _usuarioId);
        // llega partida que vence DESPUÉS: el próximo vencimiento no debe cambiar
        await uc.EjecutarAsync(new RegistrarRecepcionRequest(null, _proveedorId, _sucursalId,
            null, [new LineaRecepcion(_productoId, 10, 1000m, new DateOnly(2028, 1, 1))]), _usuarioId);

        var stock = await _db.StockSucursales.SingleAsync(s => s.ProductoId == _productoId);
        Assert.Equal(new DateOnly(2026, 12, 1), stock.FechaVencimientoProxima);
    }

    [Fact]
    public async Task Retoma_ciclica_guarda_los_tres_numeros_y_corrige()
    {
        var mov = new RegistrarMovimientoStock(_db);
        await mov.EjecutarAsync(_productoId, _sucursalId, +20, "carga inicial", _usuarioId);

        // conteo físico encuentra 17 (merma de 3)
        var uc = new RegistrarAjuste(_db, mov);
        var id = await uc.EjecutarAsync(new RegistrarAjusteRequest(TipoAjuste.RetomaCiclica,
            _sucursalId, "AJ-001", null, [new LineaConteo(_productoId, 17)]), _usuarioId);

        var detalle = await _db.DetallesAjuste.SingleAsync(d => d.AjusteId == id);
        Assert.Equal(20, detalle.CantidadSistema);
        Assert.Equal(17, detalle.CantidadContada);
        Assert.Equal(-3, detalle.Delta);
        Assert.Equal(17, (await _db.StockSucursales
            .SingleAsync(s => s.ProductoId == _productoId)).CantidadActual);
    }
}
