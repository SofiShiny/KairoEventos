using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eventos.Infraestructura.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class AgregarEsVirtual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EsVirtual",
                table: "Eventos",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EsVirtual",
                table: "Eventos");
        }
    }
}
