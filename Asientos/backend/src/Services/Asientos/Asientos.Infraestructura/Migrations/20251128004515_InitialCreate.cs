using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Asientos.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Mapas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventoId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mapas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Asientos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MapaId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Fila = table.Column<int>(type: "integer", nullable: false),
                    Numero = table.Column<int>(type: "integer", nullable: false),
                    CategoriaNombre = table.Column<string>(type: "text", nullable: false),
                    CategoriaPrecioBase = table.Column<decimal>(type: "numeric", nullable: true),
                    CategoriaTienePrioridad = table.Column<bool>(type: "boolean", nullable: false),
                    Reservado = table.Column<bool>(type: "boolean", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asientos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Asientos_Mapas_MapaId",
                        column: x => x.MapaId,
                        principalTable: "Mapas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MapasCategorias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    PrecioBase = table.Column<decimal>(type: "numeric", nullable: true),
                    TienePrioridad = table.Column<bool>(type: "boolean", nullable: false),
                    MapaId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MapasCategorias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MapasCategorias_Mapas_MapaId",
                        column: x => x.MapaId,
                        principalTable: "Mapas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Asientos_MapaId_Fila_Numero",
                table: "Asientos",
                columns: new[] { "MapaId", "Fila", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MapasCategorias_MapaId_Nombre",
                table: "MapasCategorias",
                columns: new[] { "MapaId", "Nombre" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Asientos");

            migrationBuilder.DropTable(
                name: "MapasCategorias");

            migrationBuilder.DropTable(
                name: "Mapas");
        }
    }
}
