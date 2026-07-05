using Casor.Domain.Entidades;
using Casor.Domain.Servicios;
using Xunit;

namespace Casor.Tests;

public class SelectorFefoTests
{
    private static Lote L(string n, int año, int mes, int dia, int cant) =>
        new() { NumeroLote = n, FechaVencimiento = new DateOnly(año, mes, dia), Cantidad = cant };

    [Fact]
    public void Selecciona_el_lote_que_vence_primero()
    {
        var hoy = new DateOnly(2026, 7, 1);
        var lotes = new[] { L("B", 2027, 3, 1, 10), L("A", 2026, 9, 1, 5), L("C", 2028, 1, 1, 20) };

        var elegido = SelectorFefo.SeleccionarLote(lotes, hoy);

        Assert.Equal("A", elegido!.NumeroLote);
    }

    [Fact]
    public void Ignora_lotes_vencidos_y_sin_stock()
    {
        var hoy = new DateOnly(2026, 7, 1);
        var lotes = new[] { L("Vencido", 2026, 6, 1, 10), L("SinStock", 2026, 8, 1, 0), L("Ok", 2027, 1, 1, 3) };

        var elegido = SelectorFefo.SeleccionarLote(lotes, hoy);

        Assert.Equal("Ok", elegido!.NumeroLote);
    }

    [Fact]
    public void Iva_cuadra_ida_y_vuelta()
    {
        var bruto = 11900m;
        Assert.Equal(10000m, CalculadoraIva.NetoDesdeBruto(bruto));
        Assert.Equal(1900m, CalculadoraIva.IvaDesdeBruto(bruto));
    }
}
