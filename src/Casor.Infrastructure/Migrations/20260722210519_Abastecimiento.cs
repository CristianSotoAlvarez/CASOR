using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Casor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Abastecimiento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "proveedor",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Rut = table.Column<string>(type: "text", nullable: true),
                    RazonSocial = table.Column<string>(type: "text", nullable: false),
                    Contacto = table.Column<string>(type: "text", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proveedor", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "orden_compra",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProveedorId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Fecha = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orden_compra", x => x.Id);
                    table.ForeignKey(
                        name: "FK_orden_compra_proveedor_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "proveedor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_orden_compra_usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "detalle_orden_compra",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrdenCompraId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uuid", nullable: false),
                    CantidadPedida = table.Column<int>(type: "integer", nullable: false),
                    CostoUnitarioEsperado = table.Column<decimal>(type: "numeric(12,0)", precision: 12, scale: 0, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_detalle_orden_compra", x => x.Id);
                    table.ForeignKey(
                        name: "FK_detalle_orden_compra_orden_compra_OrdenCompraId",
                        column: x => x.OrdenCompraId,
                        principalTable: "orden_compra",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_detalle_orden_compra_producto_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "producto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "recepcion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrdenCompraId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProveedorId = table.Column<Guid>(type: "uuid", nullable: false),
                    SucursalId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    NumeroFactura = table.Column<string>(type: "text", nullable: true),
                    Fecha = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recepcion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_recepcion_orden_compra_OrdenCompraId",
                        column: x => x.OrdenCompraId,
                        principalTable: "orden_compra",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_recepcion_proveedor_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "proveedor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_recepcion_sucursal_SucursalId",
                        column: x => x.SucursalId,
                        principalTable: "sucursal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_recepcion_usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "detalle_recepcion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RecepcionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    CostoUnitario = table.Column<decimal>(type: "numeric(12,0)", precision: 12, scale: 0, nullable: false),
                    FechaVencimiento = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_detalle_recepcion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_detalle_recepcion_producto_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "producto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_detalle_recepcion_recepcion_RecepcionId",
                        column: x => x.RecepcionId,
                        principalTable: "recepcion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_detalle_orden_compra_OrdenCompraId",
                table: "detalle_orden_compra",
                column: "OrdenCompraId");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_orden_compra_ProductoId",
                table: "detalle_orden_compra",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_recepcion_ProductoId",
                table: "detalle_recepcion",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_recepcion_RecepcionId",
                table: "detalle_recepcion",
                column: "RecepcionId");

            migrationBuilder.CreateIndex(
                name: "IX_orden_compra_ProveedorId",
                table: "orden_compra",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_orden_compra_UsuarioId",
                table: "orden_compra",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_proveedor_Rut",
                table: "proveedor",
                column: "Rut",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recepcion_OrdenCompraId",
                table: "recepcion",
                column: "OrdenCompraId");

            migrationBuilder.CreateIndex(
                name: "IX_recepcion_ProveedorId",
                table: "recepcion",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_recepcion_SucursalId",
                table: "recepcion",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_recepcion_UsuarioId",
                table: "recepcion",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "detalle_orden_compra");

            migrationBuilder.DropTable(
                name: "detalle_recepcion");

            migrationBuilder.DropTable(
                name: "recepcion");

            migrationBuilder.DropTable(
                name: "orden_compra");

            migrationBuilder.DropTable(
                name: "proveedor");
        }
    }
}
