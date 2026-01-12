using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Servicios.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class AgregarProveedoresServicios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "proveedores_servicios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServicioId = table.Column<Guid>(type: "uuid", nullable: false),
                    NombreProveedor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Precio = table.Column<decimal>(type: "numeric", nullable: false),
                    EstaDisponible = table.Column<bool>(type: "boolean", nullable: false),
                    ExternalId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proveedores_servicios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_proveedores_servicios_servicios_globales_ServicioId",
                        column: x => x.ServicioId,
                        principalTable: "servicios_globales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_proveedores_servicios_ServicioId",
                table: "proveedores_servicios",
                column: "ServicioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "proveedores_servicios");
        }
    }
}
