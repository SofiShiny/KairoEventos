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
            migrationBuilder.CreateTable(
                name: "entradas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    evento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    asiento_id = table.Column<Guid>(type: "uuid", nullable: true),
                    monto_original = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    monto_descuento = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    monto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    cupones_aplicados = table.Column<string>(type: "jsonb", nullable: true),
                    codigo_qr = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    estado = table.Column<int>(type: "integer", nullable: false),
                    fecha_compra = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    titulo_evento = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    imagen_evento_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    categoria = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    fecha_evento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    nombre_sector = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    fila = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    numero_asiento = table.Column<int>(type: "integer", nullable: true),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_actualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entradas", x => x.id);
                    table.CheckConstraint("ck_entradas_estado_valido", "estado IN (0, 1, 2, 3, 4)");
                    table.CheckConstraint("ck_entradas_monto_positivo", "monto > 0");
                });

            migrationBuilder.CreateIndex(
                name: "ix_entradas_codigo_qr",
                table: "entradas",
                column: "codigo_qr",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_entradas_estado",
                table: "entradas",
                column: "estado");

            migrationBuilder.CreateIndex(
                name: "ix_entradas_evento_id",
                table: "entradas",
                column: "evento_id");

            migrationBuilder.CreateIndex(
                name: "ix_entradas_fecha_compra",
                table: "entradas",
                column: "fecha_compra");

            migrationBuilder.CreateIndex(
                name: "ix_entradas_usuario_id",
                table: "entradas",
                column: "usuario_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "entradas");
        }
    }
}
