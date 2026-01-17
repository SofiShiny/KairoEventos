using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Asientos.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class AddPagoToAsiento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Asientos_Mapas_MapaAsientosId",
                table: "Asientos");

            migrationBuilder.DropIndex(
                name: "IX_MapasCategorias_MapaId_Nombre",
                table: "MapasCategorias");

            migrationBuilder.DropIndex(
                name: "IX_Asientos_MapaAsientosId",
                table: "Asientos");

            migrationBuilder.DropIndex(
                name: "IX_Asientos_MapaId_Fila_Numero",
                table: "Asientos");

            migrationBuilder.DropColumn(
                name: "MapaAsientosId",
                table: "Asientos");

            migrationBuilder.AddColumn<bool>(
                name: "Pagado",
                table: "Asientos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_MapasCategorias_MapaId",
                table: "MapasCategorias",
                column: "MapaId");

            migrationBuilder.CreateIndex(
                name: "IX_Asientos_MapaId",
                table: "Asientos",
                column: "MapaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MapasCategorias_MapaId",
                table: "MapasCategorias");

            migrationBuilder.DropIndex(
                name: "IX_Asientos_MapaId",
                table: "Asientos");

            migrationBuilder.DropColumn(
                name: "Pagado",
                table: "Asientos");

            migrationBuilder.AddColumn<Guid>(
                name: "MapaAsientosId",
                table: "Asientos",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MapasCategorias_MapaId_Nombre",
                table: "MapasCategorias",
                columns: new[] { "MapaId", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Asientos_MapaAsientosId",
                table: "Asientos",
                column: "MapaAsientosId");

            migrationBuilder.CreateIndex(
                name: "IX_Asientos_MapaId_Fila_Numero",
                table: "Asientos",
                columns: new[] { "MapaId", "Fila", "Numero" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Asientos_Mapas_MapaAsientosId",
                table: "Asientos",
                column: "MapaAsientosId",
                principalTable: "Mapas",
                principalColumn: "Id");
        }
    }
}
