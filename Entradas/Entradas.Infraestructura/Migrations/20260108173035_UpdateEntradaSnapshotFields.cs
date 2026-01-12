using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entradas.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEntradaSnapshotFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "categoria",
                schema: "entradas",
                table: "entradas",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "categoria",
                schema: "entradas",
                table: "entradas");
        }
    }
}
