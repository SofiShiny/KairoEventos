using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eventos.Infraestructura.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class AddUrlImagenAEvento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Eventos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Ubicacion_NombreLugar = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Ubicacion_Direccion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Ubicacion_Ciudad = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Ubicacion_Region = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Ubicacion_CodigoPostal = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Ubicacion_Pais = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MaximoAsistentes = table.Column<int>(type: "integer", nullable: false),
                    Estado = table.Column<string>(type: "text", nullable: false),
                    OrganizadorId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UrlImagen = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActualizadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Eventos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Asistentes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventoId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NombreUsuario = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Correo = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    RegistradoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asistentes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Asistentes_Eventos_EventoId",
                        column: x => x.EventoId,
                        principalTable: "Eventos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Asistentes_EventoId_UsuarioId",
                table: "Asistentes",
                columns: new[] { "EventoId", "UsuarioId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Eventos_Estado",
                table: "Eventos",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Eventos_FechaInicio",
                table: "Eventos",
                column: "FechaInicio");

            migrationBuilder.CreateIndex(
                name: "IX_Eventos_OrganizadorId",
                table: "Eventos",
                column: "OrganizadorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Asistentes");

            migrationBuilder.DropTable(
                name: "Eventos");
        }
    }
}
