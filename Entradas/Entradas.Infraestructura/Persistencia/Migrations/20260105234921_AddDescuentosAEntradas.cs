using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entradas.Infraestructura.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class AddDescuentosAEntradas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "cupones_aplicados",
                schema: "entradas",
                table: "entradas",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "monto_descuento",
                schema: "entradas",
                table: "entradas",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "monto_original",
                schema: "entradas",
                table: "entradas",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cupones_aplicados",
                schema: "entradas",
                table: "entradas");

            migrationBuilder.DropColumn(
                name: "monto_descuento",
                schema: "entradas",
                table: "entradas");

            migrationBuilder.DropColumn(
                name: "monto_original",
                schema: "entradas",
                table: "entradas");
        }
    }
}
