namespace Casor.Domain.Servicios;

/// <summary>IVA chileno. Centralizado: nadie calcula impuestos "a mano" en la UI. (RF-09)</summary>
public static class CalculadoraIva
{
    public const decimal Tasa = 0.19m;

    public static decimal NetoDesdeBruto(decimal bruto) => Math.Round(bruto / (1 + Tasa), 0);
    public static decimal IvaDesdeBruto(decimal bruto)  => bruto - NetoDesdeBruto(bruto);
}
