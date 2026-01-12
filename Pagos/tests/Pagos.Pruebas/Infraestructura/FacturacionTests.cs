using FluentAssertions;
using Pagos.Dominio.Entidades;
using Pagos.Infraestructura.Facturacion;
using QuestPDF.Infrastructure;
using Xunit;

namespace Pagos.Pruebas.Infraestructura;

public class FacturacionTests
{
    [Fact]
    public void GenerarPdf_RetornaArrayDeBytesNoVacio()
    {
        // Arrange
        QuestPDF.Settings.License = LicenseType.Community;
        var generador = new GeneradorFacturaQuestPdf();
        var tx = new Transaccion
        {
            Id = Guid.NewGuid(),
            OrdenId = Guid.NewGuid(),
            Monto = 1500.50m,
            TarjetaMascara = "**** 4321",
            FechaCreacion = DateTime.UtcNow
        };

        // Act
        var pdf = generador.GenerarPdf(tx);

        // Assert
        pdf.Should().NotBeNull();
        pdf.Length.Should().BeGreaterThan(0);
    }
}
