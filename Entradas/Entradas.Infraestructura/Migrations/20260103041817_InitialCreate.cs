using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entradas.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "entradas");

            migrationBuilder.CreateTable(
                name: "entradas",
                schema: "entradas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    evento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    asiento_id = table.Column<Guid>(type: "uuid", nullable: true),
                    monto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    codigo_qr = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    estado = table.Column<int>(type: "integer", nullable: false),
                    fecha_compra = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_actualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entradas", x => x.id);
                    table.CheckConstraint("ck_entradas_estado_valido", "estado IN (1, 2, 3, 4)");
                    table.CheckConstraint("ck_entradas_monto_positivo", "monto > 0");
                });

            migrationBuilder.CreateIndex(
                name: "ix_entradas_codigo_qr",
                schema: "entradas",
                table: "entradas",
                column: "codigo_qr",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_entradas_estado",
                schema: "entradas",
                table: "entradas",
                column: "estado");

            migrationBuilder.CreateIndex(
                name: "ix_entradas_evento_id",
                schema: "entradas",
                table: "entradas",
                column: "evento_id");

            migrationBuilder.CreateIndex(
                name: "ix_entradas_fecha_compra",
                schema: "entradas",
                table: "entradas",
                column: "fecha_compra");

            migrationBuilder.CreateIndex(
                name: "ix_entradas_usuario_id",
                schema: "entradas",
                table: "entradas",
                column: "usuario_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "entradas",
                schema: "entradas");
        }
    }
}
