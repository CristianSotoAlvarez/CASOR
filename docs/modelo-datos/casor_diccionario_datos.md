# CASOR — Diccionario de Datos v1.1

> **Cambios v1.1 (acuerdo con stakeholder, jul-2026):** se eliminó el manejo por LOTES (entidades LOTE y STOCK_LOTE) — el stock vive por producto+sucursal en STOCK_SUCURSAL. **Acuerdo firmado:** se renuncia a FEFO automático (RF-04), respuesta a recalls ISP por lote y trazabilidad por lote; reincorporarlos a futuro implica migración mayor. Además: tipos de receta chilenos completos, tabla PROMOCION, y razón social del receptor en CLIENTE.

Documento de respaldo del modelo: qué es cada entidad, para qué sirve cada atributo y por qué cada cardinalidad es la que es. Acompaña a `casor_schema.sql`. Cada entidad indica el requerimiento (RF/RNF) que la justifica.

Convenciones: dinero en `numeric(12,0)` (CLP sin decimales — jamás float, los errores de redondeo en dinero son inaceptables); IDs en `uuid` (permiten generar claves en la caja offline sin coordinar con el servidor, imposible con autoincrementales); nombres en snake_case (convención PostgreSQL; EF Core mapea automático desde PascalCase en C#).

---

## Nivel organizacional

### EMPRESA
**Qué es:** el cliente que contrata CASOR (ej: SANUVID SPA). **Por qué existe:** habilita el multi-tenant — hoy una fila, mañana N clientes sin rediseñar. El RUT emisor, certificado digital y folios pertenecen a este nivel, no al local.

| Atributo | Motivo |
|---|---|
| `id_empresa` | PK surrogate. |
| `rut` (único) | Identidad tributaria: es EL dato que el SII reconoce como emisor. |
| `razon_social`, `giro`, `direccion_matriz` | Datos obligatorios de la cabecera de cada DTE emitido. |
| `activa` | Baja lógica: nunca se borra un cliente con historial tributario. |

**Relaciones:** `EMPRESA 1──N SUCURSAL` (una empresa opera varios locales; un local pertenece a exactamente una empresa). `EMPRESA 1──N FOLIO_CAF` (los folios los autoriza el SII al RUT).

### SUCURSAL
**Qué es:** el local físico. **Por qué:** decisión de sesión — multi-sucursal desde el día 1; el stock, las cajas y los ajustes ocurren EN un lugar.

| Atributo | Motivo |
|---|---|
| `id_sucursal` | PK. |
| `id_empresa` (FK) | Dueño del local. |
| `nombre`, `direccion` | Identificación operativa y dato de la boleta impresa. |
| `activa` | Cerrar un local sin perder su historial. |

### USUARIO
**Qué es:** toda persona que opera el sistema (cajero, administrador). **Por qué:** RNF-03 exige segregación estricta Admin/POS y RF-11 el maestro de vendedores.

| Atributo | Motivo |
|---|---|
| `id_usuario` | PK. |
| `nombre`, `rut` | Identificación de la persona (el RUT del cajero aparece en auditorías). |
| `username` (único) | Credencial de login; único para que el login sea determinista. |
| `password_hash` | Argon2id. Jamás la contraseña en claro — si roban la BD, no roban claves. |
| `rol` (enum admin/pos) | La base de la segregación RNF-03. Enum y no boolean porque mañana habrá más roles (QF, supervisor). |
| `activo` | Deshabilitar, no borrar: sus ventas históricas lo referencian (integridad referencial). |

**Relaciones:** `USUARIO 1──N CAJA_SESION` (un cajero abre muchas sesiones a lo largo del tiempo; cada sesión la abre exactamente uno — responsabilidad individual del dinero). `USUARIO 1──N VENTA` (reporte de ventas por vendedor RF-09). `USUARIO 1──N AJUSTE_INVENTARIO` y `1──N PRECIO_HISTORICO` (quién tocó el inventario y los precios: auditoría).

---

## Catálogo

### TIPO_PRESENTACION
**Qué es:** unidad de presentación (caja, blíster, frasco, ml). **Por qué:** normalizada para que "caja" se escriba una sola vez y los reportes agrupen bien. `TIPO_PRESENTACION 1──N PRODUCTO`.

### PRINCIPIO_ACTIVO
**Qué es:** el compuesto químico (Paracetamol, Ibuprofeno). **Por qué existe como tabla (decisión D2):** el motor de bioequivalentes (RF-05) necesita "todos los productos con este mismo compuesto" como consulta indexada y sin errores de tipeo. Con texto libre, "paracetamol " con espacio rompe la equivalencia y se pierde la venta sugerida.

**Relación:** `PRINCIPIO_ACTIVO 1──N PRODUCTO`, opcional del lado producto (perfumería no tiene). **No es 1:1**: que muchos productos compartan principio es exactamente lo que permite sugerir alternativas.

### PRODUCTO
**Qué es:** el artículo del catálogo. Corazón de RF-01.

| Atributo | Motivo |
|---|---|
| `codigo_barra` (único) | La llave operativa del POS: el escáner busca por aquí (RNF-02). Único porque dos productos con el mismo código harían ambigua cada venta. |
| `descripcion` | Nombre comercial; indexada para búsqueda por texto en caja. |
| `id_principio_activo` (FK, null) | Ver PRINCIPIO_ACTIVO. Null = no medicamento. |
| `concentracion` | '500 mg'. Dos productos son bioequivalentes si comparten principio Y concentración — sin esto el motor sugeriría dosis equivocadas. |
| `linea` | Categorización comercial para reportes. |
| `refrigerado` | Alerta operativa de cadena de frío en recepción y venta. |
| `condicion_venta` (enum) | Venta directa / receta simple / receta retenida / retenida con control de stock / receta cheque (v1.1: tipología chilena completa). Es lo que dispara el flujo ISP en la caja (RF-06). |
| `nombre_isp`, `titular_isp` | Registro sanitario del producto: datos que exigen los libros legales. |
| `precio_venta` | Precio vigente al público, bruto (IVA incluido, decisión D1/ley del consumidor). Sin él la caja no puede cobrar. |
| `costo_unitario` | Último costo de compra al proveedor, NETO (v1.1: al no haber lotes, el costo vive aquí). Margen ≈ `(precio_venta ÷ 1.19) − costo_unitario`. La precisión por compra se recuperará con RECEPCION en Fase 1. |
| `stock_minimo`, `stock_maximo` | Umbrales de la alerta de stock crítico (RF-07): bajo el mínimo se sugiere comprar; sobre el máximo, capital inmovilizado. |
| `cantidad_presentacion` + `id_presentacion` (FK) | "Caja de 20 comprimidos": cómo se vende la unidad. |
| `activo` | Descatalogar sin borrar historial de ventas. |

### PRECIO_HISTORICO
**Qué es:** cada precio que el producto tuvo y cuándo. **Por qué:** RF-01 pide "precios históricos" explícito; además los reportes de resultados (RF-09) necesitan saber a qué precio de lista se vendía en una fecha para calcular márgenes correctos.

| Atributo | Motivo |
|---|---|
| `precio`, `vigente_desde`, `vigente_hasta` | Intervalo de vigencia. `vigente_hasta NULL` = es el precio actual (patrón estándar de historización). |
| `id_usuario` (FK) | Quién cambió el precio: los cambios de precio son un vector clásico de fraude interno. |

**Cardinalidad:** `PRODUCTO 1──N PRECIO_HISTORICO` — un producto acumula precios en el tiempo.

---

## Inventario

### STOCK_SUCURSAL
**Qué es:** cuánto queda de cada producto en cada sucursal. **La única fuente de verdad del inventario** (v1.1, sin lotes por acuerdo con stakeholder). PK compuesta (id_producto, id_sucursal): no puede haber dos filas para el mismo par.

| Atributo | Motivo |
|---|---|
| `cantidad_actual` | La cantidad viva. `CHECK >= 0`: el motor hace físicamente imposible el stock negativo. Ventas restan, recepciones/devoluciones suman, ajustes corrigen — siempre en la misma transacción que el documento que lo causa. |
| `fecha_vencimiento_proxima` | Parche manual de vencimientos al no existir lotes: el operario la actualiza al recibir mercadería. Alimenta la vista de vencimientos con precisión limitada (acuerdo v1.1). |

**Nota:** el costo vive en `producto.costo_unitario` (último costo de compra, NETO — el IVA de compra es crédito fiscal, no costo). El total de cadena es la vista `vista_stock_cadena`.

---

## Personas y caja

### CLIENTE
**Qué es:** perfil opcional del comprador (RF-10). **Por qué `id_cliente` surrogate y no el RUT como PK:** existen clientes sin RUT (extranjeros, ventas rápidas) y un RUT mal digitado no se puede corregir si es PK (cambiarlo rompería todas las referencias). RUT queda único-y-opcional.

| Atributo | Motivo |
|---|---|
| `rut_cliente` (único, null) | Identidad tributaria cuando existe (necesaria para factura). |
| `razon_social`, `giro` (null) | Datos del RECEPTOR que la factura electrónica exige (v1.1). Vacíos para boletas; obligatorios en la app al emitir factura. El emisor está en EMPRESA. |
| `nombre`, `email`, `numero_telefonico`, `direccion` | Contacto para cotizaciones y convenios. |

### CAJA_SESION
**Qué es:** el turno de un cajero en una caja: abre con un fondo, vende, cierra y cuadra (RF-08).

| Atributo | Motivo |
|---|---|
| `id_usuario` (FK) | El responsable del dinero de ese turno. (FK aquí y no en Usuario: un usuario abre N sesiones en su vida.) |
| `id_sucursal` (FK) | Dónde ocurre el turno. |
| `fecha_apertura`, `fecha_cierre` | Ventana temporal del turno; cierre null = sesión abierta. |
| `cierre_x` | Se emitió cierre parcial (arqueo intermedio sin cerrar el día). |
| `cierre_z` | Cierre diario definitivo: bloquea modificaciones del día — es la garantía contable. |
| `monto_apertura` | Fondo inicial de caja. |
| `monto_cierre_declarado` | Lo que el cajero contó físicamente. |
| `monto_cierre_sistema` | Lo que CASOR calculó (apertura + ventas efectivo − devoluciones). **La diferencia entre ambos ES el descuadre** — el número que el dueño mira cada noche. |

**Cardinalidades:** `SUCURSAL 1──N CAJA_SESION`, `CAJA_SESION 1──N VENTA` (toda venta ocurre dentro de un turno; sin sesión abierta no se vende).

---

## Ventas

### VENTA
**Qué es:** la transacción comercial y su documento tributario (RF-02).

| Atributo | Motivo |
|---|---|
| `id_caja_sesion` (FK) | En qué turno se vendió → alimenta la cuadratura. |
| `id_usuario` (FK) | El vendedor → reporte de ventas por vendedor (RF-09). |
| `id_cliente` (FK, **null**) | Null = boleta anónima (decisión D5: el caso normal en farmacia). |
| `id_venta_origen` (FK a sí misma, null) | Una nota de crédito es una VENTA que apunta a la venta que anula. Materializa el rombo ANULA: sin esta columna no hay devoluciones trazables (RF-08). |
| `folio` | Número asignado desde FOLIO_CAF. Índice único parcial (tipo_documento, folio): **un folio repetido es un problema legal**, la BD lo hace imposible. |
| `tipo_documento` (enum) | Boleta/factura/NC/ND — define el DTE que se emite. |
| `estado_dte` (enum) | Ciclo de vida ante el SII: pendiente → enviado → aceptado/rechazado. `pendiente_sync` = emitida offline esperando conexión. |
| `estado_venta` (enum) | Completada/anulada — estado comercial, independiente del tributario. |
| `total_neto`, `total_bruto` | Ambos guardados: los reportes con/sin IVA (RF-09) no deben recalcular hacia atrás con riesgo de redondeo. |
| `fecha_venta` | Indexada: casi todos los reportes filtran por fecha. |
| `emitida_offline` | Diagnóstico del modo offline: cuántas ventas ocurren sin red (mide la necesidad del timbre local V1.1). |

### DETALLE_VENTA
**Qué es:** cada línea de la boleta.

| Atributo | Motivo |
|---|---|
| `id_venta` (FK) | `VENTA 1──N DETALLE` — una venta tiene al menos una línea. |
| `id_producto` (FK) | Qué se vendió; alimenta los reportes por producto (RF-09). |
| `descuento` | Rebaja aplicada a la línea (0 por defecto). Persistida: la boleta histórica es auditable sin recálculo. |
| `id_promocion` (FK, null) | Por qué hubo descuento: enlaza a la promoción vigente que lo generó. NULL = descuento manual autorizado. |
| `cantidad` (`CHECK > 0`) | Línea de cero unidades no existe. |
| `precio_unitario` | **Congelado al momento de vender**: si mañana cambia el precio de lista, la boleta histórica no cambia. |
| `subtotal` | cantidad × precio − descuento, persistido para que la suma de la boleta sea auditable sin recálculo. |

### PAGO
**Qué es:** cómo se pagó. **Por qué tabla separada (decisión de sesión):** `VENTA 1──N PAGO` permite pagos mixtos (mitad efectivo, mitad tarjeta) y es lo que hace posible cuadrar el cierre de caja **por medio de pago** — el efectivo contado se compara solo contra los pagos en efectivo.

| Atributo | Motivo |
|---|---|
| `metodo_pago` (enum) | Efectivo/débito/crédito/transferencia/convenio. |
| `monto` (`CHECK > 0`) | La suma de pagos debe igualar el total de la venta (regla de aplicación). |

---

## Promociones

### PROMOCION
**Qué es:** cada oferta con vigencia (RF-10). **Por qué tabla y no solo una columna de descuento:** las promociones tienen fecha de inicio/término (mueren solas), se aplican automáticamente en caja sin depender del cajero, y se reportean ("¿cuánto costó el 3x2 de invierno?").

| Atributo | Motivo |
|---|---|
| `tipo` (enum) | porcentaje / monto_fijo / precio_oferta / mxn (ej: 3x2). |
| `valor` | El número que el tipo interpreta (15 = 15%, o el precio oferta, o el "paga n"). |
| `vigente_desde/hasta` | La vida de la promo; el POS solo aplica las vigentes y activas. |

`PROMOCION 1──N DETALLE_VENTA` (opcional): cada línea vendida con promo registra cuál fue.

---

## Normativo ISP

### RECETA_RETENIDA
**Qué es:** el registro legal de la venta de un medicamento controlado (RF-06).

| Atributo | Motivo |
|---|---|
| `id_detalle_venta` (FK, **única**) | Cuelga del detalle, no de la venta: se retiene por medicamento específico, no por boleta. El UNIQUE materializa el **1:1 parcial**: un detalle controlado genera exactamente una receta; los no controlados, ninguna. |
| `tipo` (enum) | Los tipos chilenos que generan registro: `retenida`, `retenida_control_stock` (psicotrópicos — exige cuadrar entradas/salidas en su libro) y `cheque` (estupefacientes, talonario ministerial). La receta *simple* se exhibe pero no se retiene → no genera fila; se controla con `producto.condicion_venta`. |
| `serie_receta_cheque` | Serie del talonario ministerial — obligatoria solo si tipo=cheque (CHECK en BD). |
| `folio_libro` | Correlativo del libro — el "con un clic" que promete el documento CASOR. Único por tipo. |
| `fecha` | Fecha legal del registro. |
| `nombre_medico`, `rut_medico` | Quién prescribió (exigencia del libro). Desnormalizado a propósito en V1: tabla MEDICO se evalúa si el volumen lo justifica. |
| `nombre_paciente`, `rut_paciente` | A quién se dispensó. **Cifrados en reposo a nivel de aplicación** — dato clínico sensible (Ley 19.628). |

*(Pendiente D9: validar campos exactos contra el libro físico real con un QF.)*

---

## Ajustes e inventario cíclico

### AJUSTE_INVENTARIO
**Qué es:** el evento-comprobante de una corrección de inventario (RF-07): retoma cíclica, merma, sobrante, empacado, liquidación de vencidos.

| Atributo | Motivo |
|---|---|
| `tipo` (enum) | Clasifica la causa — el reporte de mermas por tipo es el que detecta robo hormiga vs vencimiento. |
| `id_usuario`, `id_sucursal` (FK) | Quién contó y dónde: los ajustes son el segundo vector clásico de fraude interno. |
| `comprobante` | Número del documento de ajuste que exige el flujo Golan-equivalente. |

### DETALLE_AJUSTE
**Qué es:** cada producto corregido dentro de un ajuste (v1.1: por producto, sin lotes). `AJUSTE 1──N DETALLE` (la FK vive aquí, en el hijo).

| Atributo | Motivo |
|---|---|
| `id_producto` (FK) | El producto cuyo conteo se corrige — coherente con que el stock vive por producto+sucursal. |
| `cantidad_sistema` | Lo que el sistema creía. |
| `cantidad_contada` | Lo que se contó físicamente. |
| `delta` | La diferencia aplicada a STOCK_SUCURSAL. Guardar los tres números (y no solo el delta) hace cada conteo auditable años después. |

---

## Folios DTE

### FOLIO_CAF
**Qué es:** cada archivo CAF que el SII autorizó al RUT (decisión D13, modelo SimpleAPI).

| Atributo | Motivo |
|---|---|
| `id_empresa` (FK) | Los folios pertenecen al RUT emisor, no al local. |
| `id_sucursal` (FK, null) | Null = pool central; con valor = sub-rango asignado a esa caja para emitir **offline** sin chocar folios con otra caja. |
| `tipo_documento` | Cada tipo (boleta 39, factura 33, NC 61) tiene CAF separado. |
| `rango_desde`, `rango_hasta` | El rango autorizado. CHECK: hasta ≥ desde. |
| `ultimo_folio_usado` | Puntero de consumo; CHECK: dentro del rango. Con él se calcula "quedan N folios" → alerta al Admin (propuesta: umbral 500). |
| `xml_caf` | El archivo firmado por el SII: se necesita íntegro para timbrar cada documento. |

---

## Auditoría

### AUDITORIA
**Qué es:** bitácora append-only de acciones sensibles (RNF-03). **Por qué:** anulaciones, cambios de precio, deshabilitación de usuarios — todo lo que mueve dinero o permisos deja huella imborrable. La aplicación solo inserta; jamás se edita ni borra una fila (es la memoria legal del sistema).

| Atributo | Motivo |
|---|---|
| `accion`, `entidad`, `entidad_id` | Qué se hizo y sobre qué registro. |
| `datos` (jsonb) | Snapshot flexible del cambio (antes/después) sin esquema rígido. |

---

## Vistas (no son entidades)

| Vista | Utilidad |
|---|---|
| `vista_stock_cadena` | Total por producto en todas las sucursales — la vista del dueño multi-local. |
| `vista_vencimientos_proximos` | Productos con stock y fecha de vencimiento próxima (≤90 días) según el registro manual — precisión limitada sin lotes (acuerdo v1.1). |

---

## Resumen de cardinalidades y su porqué

| Relación | Cardinalidad | Por qué |
|---|---|---|
| Empresa—Sucursal | 1:N | Una cadena, varios locales; un local tiene un solo dueño. |
| Empresa—FolioCaf | 1:N | El SII autoriza folios al RUT. |
| PrincipioActivo—Producto | 1:N (opcional) | Muchas marcas comparten compuesto = motor de bioequivalentes. No-medicamentos van sin principio. |
| TipoPresentacion—Producto | 1:N | Unidad de medida normalizada. |
| Producto—PrecioHistorico | 1:N | Un producto acumula precios en el tiempo. |
| Producto—Sucursal (StockSucursal) | N:M con atributo | El inventario por local (v1.1 sin lotes); CantidadActual es EL stock. |
| Usuario—CajaSesion | 1:N | Un cajero abre muchos turnos; cada turno tiene un único responsable del dinero. |
| Sucursal—CajaSesion | 1:N | El turno ocurre en un local. |
| CajaSesion—Venta | 1:N | Toda venta pertenece a un turno (sin sesión no hay venta). |
| Usuario—Venta | 1:N | Reporte por vendedor (RF-09). |
| Cliente—Venta | 1:N, **opcional** | Boleta anónima es el caso normal (D5). |
| Venta—Venta (anula) | 1:N, opcional | La NC apunta a la venta que revierte; una venta puede tener varias NC parciales. |
| Venta—DetalleVenta | 1:N (mín. 1) | Boleta sin líneas no existe. |
| Producto—DetalleVenta | 1:N | Qué se vendió (reportes por producto). |
| Promocion—DetalleVenta | 1:N opcional | Qué promo explicó el descuento de la línea. |
| Venta—Pago | 1:N (mín. 1) | Pagos mixtos; cuadratura por medio de pago. |
| DetalleVenta—RecetaRetenida | 1:1 **parcial** | Solo los detalles de controlados generan registro legal; nunca más de uno. |
| Usuario—AjusteInventario | 1:N | Quién contó (antifraude). |
| Sucursal—AjusteInventario | 1:N | Dónde se ajustó. |
| AjusteInventario—DetalleAjuste | 1:N (mín. 1) | Un comprobante corrige uno o más productos. |
| Producto—DetalleAjuste | 1:N | La corrección es por producto (v1.1 sin lotes). |
| Sucursal—FolioCaf | 1:N opcional | Sub-rangos de folios por caja para emisión offline. |
