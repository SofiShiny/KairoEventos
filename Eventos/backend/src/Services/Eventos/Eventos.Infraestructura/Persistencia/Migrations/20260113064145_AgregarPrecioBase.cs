using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eventos.Infraestructura.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class AgregarPrecioBase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PrecioBase",
                table: "Eventos",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrecioBase",
                table: "Eventos");
        }
    }
}
