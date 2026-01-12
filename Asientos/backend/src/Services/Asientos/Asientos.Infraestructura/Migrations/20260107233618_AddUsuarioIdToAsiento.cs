using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Asientos.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class AddUsuarioIdToAsiento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioId",
                table: "Asientos",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Asientos");
        }
    }
}
