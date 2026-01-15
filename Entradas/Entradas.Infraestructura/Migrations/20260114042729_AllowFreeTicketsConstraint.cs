using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entradas.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class AllowFreeTicketsConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Asegurar que la columna existe (por si acaso quedó de una migración fallida)
            migrationBuilder.Sql("ALTER TABLE entradas ADD COLUMN IF NOT EXISTS es_virtual boolean NOT NULL DEFAULT false;");
            
            // Actualizar la restricción de monto
            migrationBuilder.Sql("ALTER TABLE entradas DROP CONSTRAINT IF EXISTS ck_entradas_monto_positivo;");
            migrationBuilder.Sql("ALTER TABLE entradas DROP CONSTRAINT IF EXISTS ck_entradas_monto_no_negativo;");
            migrationBuilder.Sql("ALTER TABLE entradas ADD CONSTRAINT ck_entradas_monto_positivo CHECK (monto >= 0);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "es_virtual",
                table: "entradas");
        }
    }
}
