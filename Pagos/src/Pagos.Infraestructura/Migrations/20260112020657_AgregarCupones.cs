using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pagos.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCupones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cupones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PorcentajeDescuento = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    EventoId = table.Column<Guid>(type: "uuid", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaExpiracion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: true),
                    FechaUso = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ContadorUsos = table.Column<int>(type: "integer", nullable: false),
                    LimiteUsos = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cupones", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cupones_Codigo",
                table: "Cupones",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cupones_Estado",
                table: "Cupones",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Cupones_EventoId",
                table: "Cupones",
                column: "EventoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cupones");
        }
    }
}
