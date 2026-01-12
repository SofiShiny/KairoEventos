using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entradas.Infraestructura.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCamposSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_evento",
                schema: "entradas",
                table: "entradas",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "fila",
                schema: "entradas",
                table: "entradas",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "imagen_evento_url",
                schema: "entradas",
                table: "entradas",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "nombre_sector",
                schema: "entradas",
                table: "entradas",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "numero_asiento",
                schema: "entradas",
                table: "entradas",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "titulo_evento",
                schema: "entradas",
                table: "entradas",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "fecha_evento",
                schema: "entradas",
                table: "entradas");

            migrationBuilder.DropColumn(
                name: "fila",
                schema: "entradas",
                table: "entradas");

            migrationBuilder.DropColumn(
                name: "imagen_evento_url",
                schema: "entradas",
                table: "entradas");

            migrationBuilder.DropColumn(
                name: "nombre_sector",
                schema: "entradas",
                table: "entradas");

            migrationBuilder.DropColumn(
                name: "numero_asiento",
                schema: "entradas",
                table: "entradas");

            migrationBuilder.DropColumn(
                name: "titulo_evento",
                schema: "entradas",
                table: "entradas");
        }
    }
}
