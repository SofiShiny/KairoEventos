using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Usuarios.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Two : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "Check_rol",
                table: "Usuarios");

            migrationBuilder.AddCheckConstraint(
                name: "Check_rol",
                table: "Usuarios",
                sql: "\"Rol\" in ('Organizador','Soporte','Usuario','Administrador')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "Check_rol",
                table: "Usuarios");

            migrationBuilder.AddCheckConstraint(
                name: "Check_rol",
                table: "Usuarios",
                sql: "\"Rol\" in ('Organizador','Soporte','Usuario')");
        }
    }
}
