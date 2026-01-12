using System;
using System.IO;
using System.Threading.Tasks;
using Eventos.Infraestructura.Servicios;
using Microsoft.AspNetCore.Hosting;
using Moq;
using Xunit;
using FluentAssertions;

namespace Eventos.Pruebas.Infraestructura.Servicios;

public class GestorArchivosLocalTests : IDisposable
{
    private readonly Mock<IWebHostEnvironment> _env;
    private readonly GestorArchivosLocal _gestor;
    private readonly string _tempPath;

    public GestorArchivosLocalTests()
    {
        _env = new Mock<IWebHostEnvironment>();
        _tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempPath);
        _env.Setup(e => e.WebRootPath).Returns(_tempPath);
        _gestor = new GestorArchivosLocal(_env.Object);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempPath))
        {
            Directory.Delete(_tempPath, true);
        }
    }

    [Fact]
    public async Task SubirImagenAsync_CreaArchivoYRetornaRuta()
    {
        // Arrange
        var content = "test content";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        var fileName = "test.jpg";
        var folder = "images";

        // Act
        var result = await _gestor.SubirImagenAsync(stream, fileName, folder);

        // Assert
        result.Should().StartWith("/images/");
        result.Should().EndWith(".jpg");
        
        var fullPath = Path.Combine(_tempPath, result.TrimStart('/'));
        File.Exists(fullPath).Should().BeTrue();
        File.ReadAllText(fullPath).Should().Be(content);
    }

    [Fact]
    public async Task BorrarImagenAsync_EliminaArchivoExistente()
    {
        // Arrange
        var folder = Path.Combine(_tempPath, "images");
        Directory.CreateDirectory(folder);
        var fileName = "to-delete.jpg";
        var fullPath = Path.Combine(folder, fileName);
        File.WriteAllText(fullPath, "content");
        
        var relativePath = "/images/to-delete.jpg";

        // Act
        await _gestor.BorrarImagenAsync(relativePath);

        // Assert
        File.Exists(fullPath).Should().BeFalse();
    }

    [Fact]
    public async Task BorrarImagenAsync_RutaVacia_NoHaceNada()
    {
        // Act
        await _gestor.BorrarImagenAsync("");
        // No debería fallar
    }
}
