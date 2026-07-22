using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Casor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EsquemaInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cliente",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RutCliente = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    RazonSocial = table.Column<string>(type: "text", nullable: true),
                    Giro = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    NumeroTelefonico = table.Column<string>(type: "text", nullable: true),
                    Direccion = table.Column<string>(type: "text", nullable: true),
                    CreadoEn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cliente", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "empresa",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Rut = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    RazonSocial = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Giro = table.Column<string>(type: "text", nullable: true),
                    DireccionMatriz = table.Column<string>(type: "text", nullable: true),
                    Activa = table.Column<bool>(type: "boolean", nullable: false),
                    CreadaEn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_empresa", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "principio_activo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_principio_activo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "promocion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    VigenteDesde = table.Column<DateOnly>(type: "date", nullable: false),
                    VigenteHasta = table.Column<DateOnly>(type: "date", nullable: false),
                    Activa = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promocion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tipo_presentacion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NombreMedida = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipo_presentacion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "usuario",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Rut = table.Column<string>(type: "text", nullable: true),
                    Username = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Rol = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "sucursal",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Direccion = table.Column<string>(type: "text", nullable: false),
                    Activa = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sucursal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sucursal_empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "producto",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CodigoBarra = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PrincipioActivoId = table.Column<Guid>(type: "uuid", nullable: true),
                    Concentracion = table.Column<string>(type: "text", nullable: true),
                    Linea = table.Column<string>(type: "text", nullable: true),
                    Refrigerado = table.Column<bool>(type: "boolean", nullable: false),
                    CondicionVenta = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    NombreIsp = table.Column<string>(type: "text", nullable: true),
                    TitularIsp = table.Column<string>(type: "text", nullable: true),
                    PrecioVenta = table.Column<decimal>(type: "numeric(12,0)", precision: 12, scale: 0, nullable: false),
                    CostoUnitario = table.Column<decimal>(type: "numeric(12,0)", precision: 12, scale: 0, nullable: true),
                    StockMinimo = table.Column<int>(type: "integer", nullable: false),
                    StockMaximo = table.Column<int>(type: "integer", nullable: false),
                    CantidadPresentacion = table.Column<int>(type: "integer", nullable: false),
                    PresentacionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_producto", x => x.Id);
                    table.CheckConstraint("ck_producto_precio", "\"PrecioVenta\" >= 0");
                    table.CheckConstraint("ck_producto_stockmin", "\"StockMinimo\" >= 0");
                    table.ForeignKey(
                        name: "FK_producto_principio_activo_PrincipioActivoId",
                        column: x => x.PrincipioActivoId,
                        principalTable: "principio_activo",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_producto_tipo_presentacion_PresentacionId",
                        column: x => x.PresentacionId,
                        principalTable: "tipo_presentacion",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "auditoria",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: true),
                    Accion = table.Column<string>(type: "text", nullable: false),
                    Entidad = table.Column<string>(type: "text", nullable: false),
                    EntidadId = table.Column<Guid>(type: "uuid", nullable: true),
                    Datos = table.Column<string>(type: "text", nullable: true),
                    Fecha = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auditoria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_auditoria_usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuario",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ajuste_inventario",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    SucursalId = table.Column<Guid>(type: "uuid", nullable: false),
                    Fecha = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Comprobante = table.Column<string>(type: "text", nullable: true),
                    Observacion = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ajuste_inventario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ajuste_inventario_sucursal_SucursalId",
                        column: x => x.SucursalId,
                        principalTable: "sucursal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ajuste_inventario_usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "caja_sesion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    SucursalId = table.Column<Guid>(type: "uuid", nullable: false),
                    FechaApertura = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FechaCierre = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CierreX = table.Column<bool>(type: "boolean", nullable: false),
                    CierreZ = table.Column<bool>(type: "boolean", nullable: false),
                    MontoApertura = table.Column<decimal>(type: "numeric(12,0)", precision: 12, scale: 0, nullable: false),
                    MontoCierreDeclarado = table.Column<decimal>(type: "numeric(12,0)", precision: 12, scale: 0, nullable: true),
                    MontoCierreSistema = table.Column<decimal>(type: "numeric(12,0)", precision: 12, scale: 0, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_caja_sesion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_caja_sesion_sucursal_SucursalId",
                        column: x => x.SucursalId,
                        principalTable: "sucursal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_caja_sesion_usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "folio_caf",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    SucursalId = table.Column<Guid>(type: "uuid", nullable: true),
                    TipoDocumento = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RangoDesde = table.Column<long>(type: "bigint", nullable: false),
                    RangoHasta = table.Column<long>(type: "bigint", nullable: false),
                    UltimoFolioUsado = table.Column<long>(type: "bigint", nullable: true),
                    XmlCaf = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_folio_caf", x => x.Id);
                    table.CheckConstraint("ck_caf_rango", "\"RangoHasta\" >= \"RangoDesde\"");
                    table.ForeignKey(
                        name: "FK_folio_caf_empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_folio_caf_sucursal_SucursalId",
                        column: x => x.SucursalId,
                        principalTable: "sucursal",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "precio_historico",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Precio = table.Column<decimal>(type: "numeric(12,0)", precision: 12, scale: 0, nullable: false),
                    VigenteDesde = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    VigenteHasta = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_precio_historico", x => x.Id);
                    table.ForeignKey(
                        name: "FK_precio_historico_producto_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "producto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_precio_historico_usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "stock_sucursal",
                columns: table => new
                {
                    ProductoId = table.Column<Guid>(type: "uuid", nullable: false),
                    SucursalId = table.Column<Guid>(type: "uuid", nullable: false),
                    CantidadActual = table.Column<int>(type: "integer", nullable: false),
                    FechaVencimientoProxima = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_sucursal", x => new { x.ProductoId, x.SucursalId });
                    table.CheckConstraint("ck_stock_no_negativo", "\"CantidadActual\" >= 0");
                    table.ForeignKey(
                        name: "FK_stock_sucursal_producto_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "producto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_stock_sucursal_sucursal_SucursalId",
                        column: x => x.SucursalId,
                        principalTable: "sucursal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "detalle_ajuste",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AjusteId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uuid", nullable: false),
                    CantidadSistema = table.Column<int>(type: "integer", nullable: false),
                    CantidadContada = table.Column<int>(type: "integer", nullable: false),
                    Delta = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_detalle_ajuste", x => x.Id);
                    table.ForeignKey(
                        name: "FK_detalle_ajuste_ajuste_inventario_AjusteId",
                        column: x => x.AjusteId,
                        principalTable: "ajuste_inventario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_detalle_ajuste_producto_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "producto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "venta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CajaSesionId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClienteId = table.Column<Guid>(type: "uuid", nullable: true),
                    VentaOrigenId = table.Column<Guid>(type: "uuid", nullable: true),
                    Folio = table.Column<long>(type: "bigint", nullable: true),
                    TipoDocumento = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    EstadoDte = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    EstadoVenta = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TotalNeto = table.Column<decimal>(type: "numeric(12,0)", precision: 12, scale: 0, nullable: false),
                    TotalBruto = table.Column<decimal>(type: "numeric(12,0)", precision: 12, scale: 0, nullable: false),
                    FechaVenta = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EmitidaOffline = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_venta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_venta_caja_sesion_CajaSesionId",
                        column: x => x.CajaSesionId,
                        principalTable: "caja_sesion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_venta_cliente_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "cliente",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_venta_usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_venta_venta_VentaOrigenId",
                        column: x => x.VentaOrigenId,
                        principalTable: "venta",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "detalle_venta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VentaId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "numeric(12,0)", precision: 12, scale: 0, nullable: false),
                    Descuento = table.Column<decimal>(type: "numeric(12,0)", precision: 12, scale: 0, nullable: false),
                    PromocionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Subtotal = table.Column<decimal>(type: "numeric(12,0)", precision: 12, scale: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_detalle_venta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_detalle_venta_producto_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "producto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_detalle_venta_promocion_PromocionId",
                        column: x => x.PromocionId,
                        principalTable: "promocion",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_detalle_venta_venta_VentaId",
                        column: x => x.VentaId,
                        principalTable: "venta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pago",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VentaId = table.Column<Guid>(type: "uuid", nullable: false),
                    MetodoPago = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(12,0)", precision: 12, scale: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pago", x => x.Id);
                    table.ForeignKey(
                        name: "FK_pago_venta_VentaId",
                        column: x => x.VentaId,
                        principalTable: "venta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "receta_retenida",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DetalleVentaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    FolioLibro = table.Column<long>(type: "bigint", nullable: false),
                    SerieRecetaCheque = table.Column<string>(type: "text", nullable: true),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    NombreMedico = table.Column<string>(type: "text", nullable: false),
                    RutMedico = table.Column<string>(type: "text", nullable: false),
                    NombrePaciente = table.Column<string>(type: "text", nullable: false),
                    RutPaciente = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_receta_retenida", x => x.Id);
                    table.ForeignKey(
                        name: "FK_receta_retenida_detalle_venta_DetalleVentaId",
                        column: x => x.DetalleVentaId,
                        principalTable: "detalle_venta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ajuste_inventario_SucursalId",
                table: "ajuste_inventario",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_ajuste_inventario_UsuarioId",
                table: "ajuste_inventario",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_auditoria_UsuarioId",
                table: "auditoria",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_caja_sesion_SucursalId",
                table: "caja_sesion",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_caja_sesion_UsuarioId",
                table: "caja_sesion",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_cliente_RutCliente",
                table: "cliente",
                column: "RutCliente",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_detalle_ajuste_AjusteId",
                table: "detalle_ajuste",
                column: "AjusteId");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_ajuste_ProductoId",
                table: "detalle_ajuste",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_venta_ProductoId",
                table: "detalle_venta",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_venta_PromocionId",
                table: "detalle_venta",
                column: "PromocionId");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_venta_VentaId",
                table: "detalle_venta",
                column: "VentaId");

            migrationBuilder.CreateIndex(
                name: "IX_empresa_Rut",
                table: "empresa",
                column: "Rut",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_folio_caf_EmpresaId",
                table: "folio_caf",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_folio_caf_SucursalId",
                table: "folio_caf",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_pago_VentaId",
                table: "pago",
                column: "VentaId");

            migrationBuilder.CreateIndex(
                name: "IX_precio_historico_ProductoId_VigenteDesde",
                table: "precio_historico",
                columns: new[] { "ProductoId", "VigenteDesde" });

            migrationBuilder.CreateIndex(
                name: "IX_precio_historico_UsuarioId",
                table: "precio_historico",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_principio_activo_Nombre",
                table: "principio_activo",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_producto_CodigoBarra",
                table: "producto",
                column: "CodigoBarra",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_producto_Descripcion",
                table: "producto",
                column: "Descripcion");

            migrationBuilder.CreateIndex(
                name: "IX_producto_PresentacionId",
                table: "producto",
                column: "PresentacionId");

            migrationBuilder.CreateIndex(
                name: "IX_producto_PrincipioActivoId",
                table: "producto",
                column: "PrincipioActivoId");

            migrationBuilder.CreateIndex(
                name: "IX_receta_retenida_DetalleVentaId",
                table: "receta_retenida",
                column: "DetalleVentaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_receta_retenida_Tipo_FolioLibro",
                table: "receta_retenida",
                columns: new[] { "Tipo", "FolioLibro" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_stock_sucursal_SucursalId",
                table: "stock_sucursal",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_sucursal_EmpresaId",
                table: "sucursal",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_tipo_presentacion_NombreMedida",
                table: "tipo_presentacion",
                column: "NombreMedida",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuario_Username",
                table: "usuario",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_venta_CajaSesionId",
                table: "venta",
                column: "CajaSesionId");

            migrationBuilder.CreateIndex(
                name: "IX_venta_ClienteId",
                table: "venta",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_venta_FechaVenta",
                table: "venta",
                column: "FechaVenta");

            migrationBuilder.CreateIndex(
                name: "IX_venta_TipoDocumento_Folio",
                table: "venta",
                columns: new[] { "TipoDocumento", "Folio" },
                unique: true,
                filter: "\"Folio\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_venta_UsuarioId",
                table: "venta",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_venta_VentaOrigenId",
                table: "venta",
                column: "VentaOrigenId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "auditoria");

            migrationBuilder.DropTable(
                name: "detalle_ajuste");

            migrationBuilder.DropTable(
                name: "folio_caf");

            migrationBuilder.DropTable(
                name: "pago");

            migrationBuilder.DropTable(
                name: "precio_historico");

            migrationBuilder.DropTable(
                name: "receta_retenida");

            migrationBuilder.DropTable(
                name: "stock_sucursal");

            migrationBuilder.DropTable(
                name: "ajuste_inventario");

            migrationBuilder.DropTable(
                name: "detalle_venta");

            migrationBuilder.DropTable(
                name: "producto");

            migrationBuilder.DropTable(
                name: "promocion");

            migrationBuilder.DropTable(
                name: "venta");

            migrationBuilder.DropTable(
                name: "principio_activo");

            migrationBuilder.DropTable(
                name: "tipo_presentacion");

            migrationBuilder.DropTable(
                name: "caja_sesion");

            migrationBuilder.DropTable(
                name: "cliente");

            migrationBuilder.DropTable(
                name: "sucursal");

            migrationBuilder.DropTable(
                name: "usuario");

            migrationBuilder.DropTable(
                name: "empresa");
        }
    }
}
