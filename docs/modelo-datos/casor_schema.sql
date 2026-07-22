-- ============================================================================
-- CASOR — Esquema de Base de Datos v1.1
-- Motor: PostgreSQL 18 · Convención: snake_case · Dinero: numeric(12,0) CLP · IDs: uuid
-- Documentación de cada decisión: casor_diccionario_datos.md
--
-- CAMBIOS v1.1 (sesión con stakeholder, jul-2026):
--   * SIN LOTES: se eliminan lote y stock_lote; el stock vive en stock_sucursal
--     (producto+sucursal). ACUERDO: se renuncia a FEFO automático (RF-04),
--     respuesta a recalls ISP por lote y trazabilidad por lote.
--   * Recetas: tipos chilenos completos (simple, retenida, retenida con
--     control de stock, receta cheque con serie).
--   * Promociones: tabla promocion + descuento/id_promocion en detalle_venta.
--   * Facturas: razon_social y giro del RECEPTOR en cliente (el emisor ya
--     está en empresa).
-- ============================================================================

-- ============================ TIPOS ENUMERADOS ==============================

CREATE TYPE rol_usuario     AS ENUM ('admin', 'pos');
CREATE TYPE condicion_venta AS ENUM ('venta_directa', 'receta_simple', 'receta_retenida',
                                     'receta_retenida_control_stock', 'receta_cheque');
CREATE TYPE tipo_documento  AS ENUM ('boleta', 'boleta_exenta', 'factura', 'factura_exenta',
                                     'nota_credito', 'nota_debito');
CREATE TYPE estado_venta    AS ENUM ('completada', 'anulada');
CREATE TYPE estado_dte      AS ENUM ('pendiente', 'enviado', 'aceptado', 'aceptado_reparos',
                                     'rechazado', 'pendiente_sync');
CREATE TYPE medio_pago      AS ENUM ('efectivo', 'debito', 'credito', 'transferencia', 'convenio');
CREATE TYPE tipo_ajuste     AS ENUM ('retoma_ciclica', 'merma', 'sobrante', 'empacado', 'vencimiento');
CREATE TYPE tipo_receta     AS ENUM ('retenida', 'retenida_control_stock', 'cheque');
CREATE TYPE tipo_promocion  AS ENUM ('porcentaje', 'monto_fijo', 'precio_oferta', 'mxn');

-- ========================== NIVEL ORGANIZACIONAL ============================

CREATE TABLE empresa (
    id_empresa       uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    rut              varchar(12) NOT NULL UNIQUE,     -- razón social EMISORA de los DTE
    razon_social     varchar(120) NOT NULL,
    giro             varchar(120),
    direccion_matriz varchar(200),
    activa           boolean NOT NULL DEFAULT true,
    creada_en        timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE sucursal (
    id_sucursal uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    id_empresa  uuid NOT NULL REFERENCES empresa(id_empresa),
    nombre      varchar(80) NOT NULL,
    direccion   varchar(200) NOT NULL,
    activa      boolean NOT NULL DEFAULT true
);

CREATE TABLE usuario (
    id_usuario    uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    nombre        varchar(100) NOT NULL,
    rut           varchar(12),
    username      varchar(40) NOT NULL UNIQUE,
    password_hash varchar(200) NOT NULL,               -- Argon2id; JAMÁS texto plano
    rol           rol_usuario NOT NULL,
    activo        boolean NOT NULL DEFAULT true        -- deshabilitar, nunca borrar
);

-- ============================ CATÁLOGO ======================================

CREATE TABLE tipo_presentacion (
    id_presentacion uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    nombre_medida   varchar(40) NOT NULL UNIQUE
);

CREATE TABLE principio_activo (
    id_principio_activo uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    nombre              varchar(120) NOT NULL UNIQUE
);

CREATE TABLE producto (
    id_producto           uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    codigo_barra          varchar(20) NOT NULL UNIQUE,
    descripcion           varchar(200) NOT NULL,
    id_principio_activo   uuid REFERENCES principio_activo(id_principio_activo), -- NULL: perfumería
    concentracion         varchar(40),                 -- '500 mg': define bioequivalencia
    linea                 varchar(60),
    refrigerado           boolean NOT NULL DEFAULT false,
    condicion_venta       condicion_venta NOT NULL DEFAULT 'venta_directa',
    nombre_isp            varchar(120),
    titular_isp           varchar(120),
    precio_venta          numeric(12,0) NOT NULL CHECK (precio_venta >= 0),   -- BRUTO (IVA incl.)
    costo_unitario        numeric(12,0) CHECK (costo_unitario >= 0),          -- NETO, último costo de compra
    stock_minimo          integer NOT NULL DEFAULT 0 CHECK (stock_minimo >= 0),
    stock_maximo          integer NOT NULL DEFAULT 0 CHECK (stock_maximo >= 0),
    cantidad_presentacion integer NOT NULL DEFAULT 1,
    id_presentacion       uuid REFERENCES tipo_presentacion(id_presentacion),
    activo                boolean NOT NULL DEFAULT true
);

CREATE INDEX idx_producto_descripcion ON producto (descripcion);
CREATE INDEX idx_producto_principio   ON producto (id_principio_activo);  -- bioequivalentes RF-05

CREATE TABLE precio_historico (
    id_precio_historico uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    id_producto         uuid NOT NULL REFERENCES producto(id_producto),
    precio              numeric(12,0) NOT NULL CHECK (precio >= 0),
    vigente_desde       timestamptz NOT NULL,
    vigente_hasta       timestamptz,                   -- NULL = vigente hoy
    id_usuario          uuid NOT NULL REFERENCES usuario(id_usuario)
);

CREATE INDEX idx_precio_hist_producto ON precio_historico (id_producto, vigente_desde);

-- ============================ INVENTARIO (sin lotes, v1.1) ==================

CREATE TABLE stock_sucursal (
    id_producto               uuid NOT NULL REFERENCES producto(id_producto),
    id_sucursal               uuid NOT NULL REFERENCES sucursal(id_sucursal),
    cantidad_actual           integer NOT NULL CHECK (cantidad_actual >= 0),  -- stock JAMÁS negativo
    fecha_vencimiento_proxima date,                    -- parche manual de vencimientos (sin lotes)
    PRIMARY KEY (id_producto, id_sucursal)
);

-- ============================ PROMOCIONES (RF-10) ===========================

CREATE TABLE promocion (
    id_promocion  uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    nombre        varchar(100) NOT NULL,
    tipo          tipo_promocion NOT NULL,
    valor         numeric(12,2) NOT NULL,              -- % , monto, precio oferta o 'paga n' según tipo
    vigente_desde date NOT NULL,
    vigente_hasta date NOT NULL,
    activa        boolean NOT NULL DEFAULT true,
    CHECK (vigente_hasta >= vigente_desde)
);

-- ====================== PERSONAS Y SESIONES DE CAJA =========================

CREATE TABLE cliente (
    id_cliente        uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    rut_cliente       varchar(12) UNIQUE,              -- opcional
    nombre            varchar(120) NOT NULL,
    razon_social      varchar(120),                    -- RECEPTOR de factura (obligatoria si factura)
    giro              varchar(120),                    -- ídem
    email             varchar(120),
    numero_telefonico varchar(20),
    direccion         varchar(200),
    creado_en         timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE caja_sesion (
    id_caja_sesion          uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    id_usuario              uuid NOT NULL REFERENCES usuario(id_usuario),
    id_sucursal             uuid NOT NULL REFERENCES sucursal(id_sucursal),
    fecha_apertura          timestamptz NOT NULL DEFAULT now(),
    fecha_cierre            timestamptz,
    cierre_x                boolean NOT NULL DEFAULT false,
    cierre_z                boolean NOT NULL DEFAULT false,   -- bloquea el día (RF-08)
    monto_apertura          numeric(12,0) NOT NULL CHECK (monto_apertura >= 0),
    monto_cierre_declarado  numeric(12,0),
    monto_cierre_sistema    numeric(12,0)              -- diferencia con declarado = descuadre
);

-- ============================ VENTAS ========================================

CREATE TABLE venta (
    id_venta        uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    id_caja_sesion  uuid NOT NULL REFERENCES caja_sesion(id_caja_sesion),
    id_usuario      uuid NOT NULL REFERENCES usuario(id_usuario),    -- vendedor (RF-09)
    id_cliente      uuid REFERENCES cliente(id_cliente),             -- NULL = boleta anónima
    id_venta_origen uuid REFERENCES venta(id_venta),                 -- NC → venta anulada
    folio           bigint,
    tipo_documento  tipo_documento NOT NULL,
    estado_dte      estado_dte NOT NULL DEFAULT 'pendiente',
    estado_venta    estado_venta NOT NULL DEFAULT 'completada',
    total_neto      numeric(12,0) NOT NULL CHECK (total_neto >= 0),
    total_bruto     numeric(12,0) NOT NULL CHECK (total_bruto >= 0),
    fecha_venta     timestamptz NOT NULL DEFAULT now(),
    emitida_offline boolean NOT NULL DEFAULT false
);

CREATE UNIQUE INDEX uq_venta_folio ON venta (tipo_documento, folio) WHERE folio IS NOT NULL;
CREATE INDEX idx_venta_fecha ON venta (fecha_venta);
CREATE INDEX idx_venta_caja  ON venta (id_caja_sesion);

CREATE TABLE detalle_venta (
    id_detalle_venta uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    id_venta         uuid NOT NULL REFERENCES venta(id_venta),
    id_producto      uuid NOT NULL REFERENCES producto(id_producto),
    cantidad         integer NOT NULL CHECK (cantidad > 0),
    precio_unitario  numeric(12,0) NOT NULL CHECK (precio_unitario >= 0),  -- congelado al vender
    descuento        numeric(12,0) NOT NULL DEFAULT 0 CHECK (descuento >= 0),
    id_promocion     uuid REFERENCES promocion(id_promocion),              -- por qué hubo descuento
    subtotal         numeric(12,0) NOT NULL CHECK (subtotal >= 0)
);

CREATE INDEX idx_detalle_venta_venta    ON detalle_venta (id_venta);
CREATE INDEX idx_detalle_venta_producto ON detalle_venta (id_producto);

CREATE TABLE pago (
    id_pago     uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    id_venta    uuid NOT NULL REFERENCES venta(id_venta),
    metodo_pago medio_pago NOT NULL,
    monto       numeric(12,0) NOT NULL CHECK (monto > 0)
);

CREATE INDEX idx_pago_venta ON pago (id_venta);

-- ======================== NORMATIVO ISP (RF-06) =============================
-- Tipos de receta en Chile: simple (se exhibe, no genera registro),
-- retenida, retenida con control de stock (psicotrópicos) y receta cheque
-- (estupefacientes, talonario ministerial con serie). Cada tipo retenido
-- alimenta su propio libro legal. ⚠ D9: validar campos con QF antes de congelar.

CREATE TABLE receta_retenida (
    id_receta          uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    id_detalle_venta   uuid NOT NULL UNIQUE REFERENCES detalle_venta(id_detalle_venta), -- 1:1 parcial
    tipo               tipo_receta NOT NULL,
    folio_libro        bigint NOT NULL,
    serie_receta_cheque varchar(30),                   -- serie del talonario (solo tipo 'cheque')
    fecha              date NOT NULL DEFAULT current_date,
    nombre_medico      varchar(120) NOT NULL,
    rut_medico         varchar(12) NOT NULL,
    nombre_paciente    varchar(120) NOT NULL,          -- cifrar en reposo (app-level)
    rut_paciente       varchar(12) NOT NULL,           -- cifrar en reposo (app-level)
    UNIQUE (tipo, folio_libro),                        -- un correlativo por libro
    CHECK (tipo <> 'cheque' OR serie_receta_cheque IS NOT NULL)  -- cheque exige serie
);

-- ===================== AJUSTES E INVENTARIO CÍCLICO (RF-07) =================

CREATE TABLE ajuste_inventario (
    id_ajuste   uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tipo        tipo_ajuste NOT NULL,
    id_usuario  uuid NOT NULL REFERENCES usuario(id_usuario),
    id_sucursal uuid NOT NULL REFERENCES sucursal(id_sucursal),
    fecha       timestamptz NOT NULL DEFAULT now(),
    comprobante varchar(40),
    observacion text
);

CREATE TABLE detalle_ajuste (
    id_detalle_ajuste uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    id_ajuste         uuid NOT NULL REFERENCES ajuste_inventario(id_ajuste),
    id_producto       uuid NOT NULL REFERENCES producto(id_producto),   -- sin lotes: por producto
    cantidad_sistema  integer NOT NULL,
    cantidad_contada  integer NOT NULL CHECK (cantidad_contada >= 0),
    delta             integer NOT NULL                 -- contada - sistema → se aplica a stock_sucursal
);

-- ======================= FOLIOS DTE (D13, SimpleAPI) ========================

CREATE TABLE folio_caf (
    id_folio_caf       uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    id_empresa         uuid NOT NULL REFERENCES empresa(id_empresa),
    id_sucursal        uuid REFERENCES sucursal(id_sucursal),   -- NULL = pool central
    tipo_documento     tipo_documento NOT NULL,
    rango_desde        bigint NOT NULL,
    rango_hasta        bigint NOT NULL,
    ultimo_folio_usado bigint,
    xml_caf            text NOT NULL,
    CHECK (rango_hasta >= rango_desde),
    CHECK (ultimo_folio_usado IS NULL OR (ultimo_folio_usado BETWEEN rango_desde AND rango_hasta))
);

-- ============================ AUDITORÍA (RNF-03) ============================

CREATE TABLE auditoria (
    id_auditoria bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    id_usuario   uuid REFERENCES usuario(id_usuario),
    accion       varchar(60) NOT NULL,
    entidad      varchar(40) NOT NULL,
    entidad_id   uuid,
    datos        jsonb,
    fecha        timestamptz NOT NULL DEFAULT now()
);
-- Append-only: la aplicación solo INSERTa. Nunca UPDATE/DELETE.

-- ============================== VISTAS ======================================

-- Total por producto en toda la cadena
CREATE VIEW vista_stock_cadena AS
SELECT id_producto, SUM(cantidad_actual) AS cantidad_total
FROM stock_sucursal
GROUP BY id_producto;

-- Vencimientos próximos (sobre el parche manual; precisión limitada sin lotes)
CREATE VIEW vista_vencimientos_proximos AS
SELECT s.id_producto, s.id_sucursal, s.cantidad_actual, s.fecha_vencimiento_proxima
FROM stock_sucursal s
WHERE s.cantidad_actual > 0
  AND s.fecha_vencimiento_proxima IS NOT NULL
  AND s.fecha_vencimiento_proxima <= current_date + INTERVAL '90 days'
ORDER BY s.fecha_vencimiento_proxima;
