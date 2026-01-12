using FluentAssertions;
using Pagos.Infraestructura.Almacenamiento;
using Xunit;

namespace Pagos.Pruebas.Infraestructura;

public class AlmacenamientoTests : IDisposable
{
    private readonly string _testPath;

    public AlmacenamientoTests()
    {
        _testPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "facturas");
    }

    [Fact]
    public async Task GuardarAsync_CreaCarpetaYArchivoCorrectamente()
    {
        // Arrange
        var almacenador = new AlmacenadorLocal();
        var nombreFile = $"test_{Guid.NewGuid()}.pdf";
        var datos = new byte[] { 0, 1, 2, 3 };

        // Act
        var url = await almacenador.GuardarAsync(nombreFile, datos);

        // Assert
        url.Should().Be($"/facturas/{nombreFile}");
        var fullPath = Path.Combine(_testPath, nombreFile);
        File.Exists(fullPath).Should().BeTrue();
        (await File.ReadAllBytesAsync(fullPath)).Should().BeEquivalentTo(datos);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testPath))
        {
            Directory.Delete(_testPath, true);
        }
    }
}
