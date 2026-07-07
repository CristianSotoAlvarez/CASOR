-- ============================================================================
-- CASOR — Esquema de Base de Datos v1.0
-- Motor: PostgreSQL 18 · Convención: snake_case (EF Core mapea PascalCase→snake_case)
-- Dinero: numeric(12,0) — CLP sin decimales · IDs: uuid
-- Documentación completa de cada decisión: casor_diccionario_datos.md
-- ============================================================================

-- ============================ TIPOS ENUMERADOS ==============================

CREATE TYPE rol_usuario     AS ENUM ('admin', 'pos');
CREATE TYPE condicion_venta AS ENUM ('venta_directa', 'receta_simple', 'receta_retenida', 'estupefaciente');
CREATE TYPE tipo_documento  AS ENUM ('boleta', 'boleta_exenta', 'factura', 'factura_exenta', 'nota_credito', 'nota_debito');
CREATE TYPE estado_venta    AS ENUM ('completada', 'anulada');
CREATE TYPE estado_dte      AS ENUM ('pendiente', 'enviado', 'aceptado', 'aceptado_reparos', 'rechazado', 'pendiente_sync');
CREATE TYPE medio_pago      AS ENUM ('efectivo', 'debito', 'credito', 'transferencia', 'convenio');
CREATE TYPE tipo_ajuste     AS ENUM ('retoma_ciclica', 'merma', 'sobrante', 'empacado', 'vencimiento');
CREATE TYPE tipo_receta     AS ENUM ('retenida', 'estupefaciente');

-- ========================== NIVEL ORGANIZACIONAL ============================

CREATE TABLE empresa (
    id_empresa       uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    rut              varchar(12) NOT NULL UNIQUE,
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
    password_hash varchar(200) NOT NULL,          -- Argon2id; JAMÁS texto plano
    rol           rol_usuario NOT NULL,
    activo        boolean NOT NULL DEFAULT true   -- deshabilitar, nunca borrar
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
    concentracion         varchar(40),               -- ej '500 mg' — define bioequivalencia junto al principio
    linea                 varchar(60),
    refrigerado           boolean NOT NULL DEFAULT false,
    condicion_venta       condicion_venta NOT NULL DEFAULT 'venta_directa',
    nombre_isp            varchar(120),
    titular_isp           varchar(120),
    precio_venta          numeric(12,0) NOT NULL CHECK (precio_venta >= 0),  -- bruto (IVA incl.) s/ D1
    stock_minimo          integer NOT NULL DEFAULT 0 CHECK (stock_minimo >= 0),
    stock_maximo          integer NOT NULL DEFAULT 0 CHECK (stock_maximo >= 0),
    cantidad_presentacion integer NOT NULL DEFAULT 1,
    id_presentacion       uuid REFERENCES tipo_presentacion(id_presentacion),
    activo                boolean NOT NULL DEFAULT true
);

CREATE INDEX idx_producto_descripcion ON producto (descripcion);
CREATE INDEX idx_producto_principio   ON producto (id_principio_activo);  -- motor bioequivalentes RF-05

CREATE TABLE precio_historico (
    id_precio_historico uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    id_producto         uuid NOT NULL REFERENCES producto(id_producto),
    precio              numeric(12,0) NOT NULL CHECK (precio >= 0),
    vigente_desde       timestamptz NOT NULL,
    vigente_hasta       timestamptz,                 -- NULL = vigente hoy
    id_usuario          uuid NOT NULL REFERENCES usuario(id_usuario)  -- quién cambió el precio
);

CREATE INDEX idx_precio_hist_producto ON precio_historico (id_producto, vigente_desde);

-- ============================ INVENTARIO ====================================

CREATE TABLE lote (
    id_lote            uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    id_producto        uuid NOT NULL REFERENCES producto(id_producto),
    numero_lote        varchar(40) NOT NULL,
    fecha_vencimiento  date NOT NULL,
    cantidad_ingresada integer NOT NULL CHECK (cantidad_ingresada > 0), -- histórico de recepción
    costo_unitario     numeric(12,0) NOT NULL CHECK (costo_unitario >= 0) -- lo pagado al proveedor: base del margen (RF-09)
);

CREATE INDEX idx_lote_fefo ON lote (id_producto, fecha_vencimiento);   -- índice FEFO RF-04

CREATE TABLE stock_lote (
    id_lote         uuid NOT NULL REFERENCES lote(id_lote),
    id_sucursal     uuid NOT NULL REFERENCES sucursal(id_sucursal),
    cantidad_actual integer NOT NULL CHECK (cantidad_actual >= 0),      -- stock JAMÁS negativo
    PRIMARY KEY (id_lote, id_sucursal)
);

-- ====================== PERSONAS Y SESIONES DE CAJA =========================

CREATE TABLE cliente (
    id_cliente        uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    rut_cliente       varchar(12) UNIQUE,            -- opcional: clientes sin RUT existen
    nombre            varchar(120) NOT NULL,
    email             varchar(120),
    numero_telefonico varchar(20),
    direccion         varchar(200),
    creado_en         timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE caja_sesion (
    id_caja_sesion          uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    id_usuario              uuid NOT NULL REFERENCES usuario(id_usuario),   -- cajero responsable
    id_sucursal             uuid NOT NULL REFERENCES sucursal(id_sucursal),
    fecha_apertura          timestamptz NOT NULL DEFAULT now(),
    fecha_cierre            timestamptz,
    cierre_x                boolean NOT NULL DEFAULT false,   -- cierre parcial emitido
    cierre_z                boolean NOT NULL DEFAULT false,   -- cierre diario: bloquea el día RF-08
    monto_apertura          numeric(12,0) NOT NULL CHECK (monto_apertura >= 0),
    monto_cierre_declarado  numeric(12,0),                    -- lo que contó el cajero
    monto_cierre_sistema    numeric(12,0)                     -- lo que calculó CASOR; diferencia = descuadre
);

-- ============================ VENTAS ========================================

CREATE TABLE venta (
    id_venta        uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    id_caja_sesion  uuid NOT NULL REFERENCES caja_sesion(id_caja_sesion),
    id_usuario      uuid NOT NULL REFERENCES usuario(id_usuario),    -- vendedor (reporte RF-09)
    id_cliente      uuid REFERENCES cliente(id_cliente),             -- NULL = boleta anónima (D5)
    id_venta_origen uuid REFERENCES venta(id_venta),                 -- NC referencia la venta anulada
    folio           bigint,                                          -- asignado desde folio_caf
    tipo_documento  tipo_documento NOT NULL,
    estado_dte      estado_dte NOT NULL DEFAULT 'pendiente',
    estado_venta    estado_venta NOT NULL DEFAULT 'completada',
    total_neto      numeric(12,0) NOT NULL CHECK (total_neto >= 0),
    total_bruto     numeric(12,0) NOT NULL CHECK (total_bruto >= 0),
    fecha_venta     timestamptz NOT NULL DEFAULT now(),
    emitida_offline boolean NOT NULL DEFAULT false
);

CREATE UNIQUE INDEX uq_venta_folio ON venta (tipo_documento, folio) WHERE folio IS NOT NULL; -- folio jamás repetido
CREATE INDEX idx_venta_fecha ON venta (fecha_venta);
CREATE INDEX idx_venta_caja  ON venta (id_caja_sesion);

CREATE TABLE detalle_venta (
    id_detalle_venta uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    id_venta         uuid NOT NULL REFERENCES venta(id_venta),
    id_producto      uuid NOT NULL REFERENCES producto(id_producto),
    id_lote          uuid NOT NULL REFERENCES lote(id_lote),   -- lote exacto que salió: FEFO/devoluciones/ISP
    cantidad         integer NOT NULL CHECK (cantidad > 0),
    precio_unitario  numeric(12,0) NOT NULL CHECK (precio_unitario >= 0),  -- congelado al vender
    subtotal         numeric(12,0) NOT NULL CHECK (subtotal >= 0)
);

CREATE INDEX idx_detalle_venta_venta    ON detalle_venta (id_venta);
CREATE INDEX idx_detalle_venta_producto ON detalle_venta (id_producto);
CREATE INDEX idx_detalle_venta_lote     ON detalle_venta (id_lote);       -- respuesta a recalls ISP

CREATE TABLE pago (
    id_pago       uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    id_venta      uuid NOT NULL REFERENCES venta(id_venta),
    metodo_pago   medio_pago NOT NULL,
    monto         numeric(12,0) NOT NULL CHECK (monto > 0)
);

CREATE INDEX idx_pago_venta ON pago (id_venta);

-- ======================== NORMATIVO ISP (RF-06) =============================

CREATE TABLE receta_retenida (
    id_receta        uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    id_detalle_venta uuid NOT NULL UNIQUE REFERENCES detalle_venta(id_detalle_venta), -- 1:1 parcial
    tipo             tipo_receta NOT NULL,
    folio_libro      bigint NOT NULL,               -- correlativo del libro legal
    fecha            date NOT NULL DEFAULT current_date,
    nombre_medico    varchar(120) NOT NULL,
    rut_medico       varchar(12) NOT NULL,
    nombre_paciente  varchar(120) NOT NULL,         -- cifrar en reposo (app-level)
    rut_paciente     varchar(12) NOT NULL,          -- cifrar en reposo (app-level)
    UNIQUE (tipo, folio_libro)                       -- un correlativo por libro
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
    id_ajuste         uuid NOT NULL REFERENCES ajuste_inventario(id_ajuste),  -- FK en el hijo ✓
    id_lote           uuid NOT NULL REFERENCES lote(id_lote),
    cantidad_sistema  integer NOT NULL,
    cantidad_contada  integer NOT NULL CHECK (cantidad_contada >= 0),
    delta             integer NOT NULL               -- contada - sistema; lo que se aplica a stock_lote
);

-- ======================= FOLIOS DTE (D13, SimpleAPI) ========================

CREATE TABLE folio_caf (
    id_folio_caf       uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    id_empresa         uuid NOT NULL REFERENCES empresa(id_empresa),  -- los folios son del RUT
    id_sucursal        uuid REFERENCES sucursal(id_sucursal),         -- NULL = pool central; valor = sub-rango de esa caja
    tipo_documento     tipo_documento NOT NULL,
    rango_desde        bigint NOT NULL,
    rango_hasta        bigint NOT NULL,
    ultimo_folio_usado bigint,
    xml_caf            text NOT NULL,                                 -- el archivo CAF firmado por el SII
    CHECK (rango_hasta >= rango_desde),
    CHECK (ultimo_folio_usado IS NULL OR (ultimo_folio_usado BETWEEN rango_desde AND rango_hasta))
);

-- ============================ AUDITORÍA (RNF-03) ============================

CREATE TABLE auditoria (
    id_auditoria bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    id_usuario   uuid REFERENCES usuario(id_usuario),
    accion       varchar(60) NOT NULL,      -- 'venta.anular', 'precio.cambiar', ...
    entidad      varchar(40) NOT NULL,
    entidad_id   uuid,
    datos        jsonb,                     -- snapshot del cambio
    fecha        timestamptz NOT NULL DEFAULT now()
);
-- Append-only: la aplicación solo INSERTa. Nunca UPDATE/DELETE sobre esta tabla.

-- ====================== VISTAS DE STOCK (no son tablas) =====================

-- Total por producto y sucursal: reemplaza a la extinta STOCKSUCURSAL
CREATE VIEW vista_stock_sucursal AS
SELECT l.id_producto,
       s.id_sucursal,
       SUM(s.cantidad_actual) AS cantidad_total
FROM stock_lote s
JOIN lote l ON l.id_lote = s.id_lote
GROUP BY l.id_producto, s.id_sucursal;

-- Total por producto en toda la cadena
CREATE VIEW vista_stock_cadena AS
SELECT l.id_producto,
       SUM(s.cantidad_actual) AS cantidad_total
FROM stock_lote s
JOIN lote l ON l.id_lote = s.id_lote
GROUP BY l.id_producto;

-- Lotes próximos a vencer (alimenta alertas FEFO del POS/Admin)
CREATE VIEW vista_vencimientos_proximos AS
SELECT l.id_producto, l.id_lote, l.numero_lote, l.fecha_vencimiento,
       s.id_sucursal, s.cantidad_actual
FROM lote l
JOIN stock_lote s ON s.id_lote = l.id_l