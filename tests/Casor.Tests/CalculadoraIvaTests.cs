using Casor.Domain.Servicios;
using Xunit;

namespace Casor.Tests;

public class CalculadoraIvaTests
{
    [Fact]
    public void Iva_cuadra_ida_y_vuelta()
    {
        var bruto = 11900m;
        Assert.Equal(10000m, CalculadoraIva.NetoDesdeBruto(bruto));
        Assert.Equal(1900m, CalculadoraIva.IvaDesdeBruto(bruto));
    }

    [Fact]
    public void Margen_neto_contra_neto()
    {
        // precio venta bruto 1990, costo neto 900 → margen = neto(1990) - 900
        var margen = CalculadoraIva.NetoDesdeBruto(1990m) - 900m;
        Assert.Equal(772m, margen);   // neto(1990)=1672 → 1672-900
    }
}
