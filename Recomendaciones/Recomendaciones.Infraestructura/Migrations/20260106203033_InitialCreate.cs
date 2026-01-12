using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recomendaciones.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "afinidades_usuario",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    Categoria = table.Column<string>(type: "text", nullable: false),
                    Puntuacion = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_afinidades_usuario", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "eventos_proyecciones",
                columns: table => new
                {
                    EventoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Categoria = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eventos_proyecciones", x => x.EventoId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_afinidades_usuario_UsuarioId_Categoria",
                table: "afinidades_usuario",
                columns: new[] { "UsuarioId", "Categoria" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "afinidades_usuario");

            migrationBuilder.DropTable(
                name: "eventos_proyecciones");
        }
    }
}
