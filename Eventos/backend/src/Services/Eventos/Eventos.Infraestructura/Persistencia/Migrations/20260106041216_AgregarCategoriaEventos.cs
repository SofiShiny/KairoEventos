using Microsoft.EntityFrameworkCore.Migrations;


#nullable disable

namespace Eventos.Infraestructura.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCategoriaEventos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Categoria",
                table: "Eventos",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Categoria",
                table: "Eventos");
        }
    }
}
