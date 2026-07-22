namespace Casor.Domain;

// Se persisten como TEXTO en la base (HasConversion<string>) para que el mismo
// modelo funcione en PostgreSQL (central) y SQLite (cajas offline).
// Referencia semántica: docs/modelo-datos/casor_schema.sql

public enum RolUsuario { Admin, Pos }

public enum CondicionVenta
{
    VentaDirecta, RecetaSimple, RecetaRetenida,
    RecetaRetenidaControlStock, RecetaCheque
}

public enum TipoDocumento { Boleta, BoletaExenta, Factura, FacturaExenta, NotaCredito, NotaDebito }

public enum EstadoVenta { Completada, Anulada }

public enum EstadoDte { Pendiente, Enviado, Aceptado, AceptadoReparos, Rechazado, PendienteSync }

public enum MedioPago { Efectivo, Debito, Credito, Transferencia, Convenio }

public enum TipoAjuste { RetomaCiclica, Merma, Sobrante, Empacado, Vencimiento }

public enum TipoReceta { Retenida, RetenidaControlStock, Cheque }

public enum TipoPromocion { Porcentaje, MontoFijo, PrecioOferta, Mxn }
