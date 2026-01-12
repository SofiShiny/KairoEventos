using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;
using Entradas.Infraestructura.Repositorios;
using Entradas.Infraestructura.Persistencia;
using Entradas.Dominio.Entidades;
using Entradas.Dominio.Enums;

namespace Entradas.Pruebas.Infraestructura.Repositorios;

public class RepositorioEntradasCoverageTests
{
    private readonly EntradasDbContext _context;
    private readonly RepositorioEntradas _repositorio;
    private readonly Mock<ILogger<RepositorioEntradas>> _mockLogger;

    public RepositorioEntradasCoverageTests()
    {
        var options = new DbContextOptionsBuilder<EntradasDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new EntradasDbContext(options);
        _mockLogger = new Mock<ILogger<RepositorioEntradas>>();
        _repositorio = new RepositorioEntradas(_context, _mockLogger.Object);
    }

    [Fact]
    public async Task ObtenerPorCodigoQrAsync_CuandoNoExiste_DebeRetornarNull()
    {
        // Act
        var result = await _repositorio.ObtenerPorCodigoQrAsync("NO-EXISTE");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerPorUsuarioAsync_CuandoIdEsVacio_DebeLanzarArgumentException()
    {
        // Act & Assert
        await FluentActions
            .Invoking(() => _repositorio.ObtenerPorUsuarioAsync(Guid.Empty))
            .Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage("*ID del usuario requerido*");
    }

    [Fact]
    public async Task ObtenerPorEventoAsync_CuandoIdEsVacio_DebeLanzarArgumentException()
    {
        // Act & Assert
        await FluentActions
            .Invoking(() => _repositorio.ObtenerPorEventoAsync(Guid.Empty))
            .Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage("*ID del evento requerido*");
    }

    [Fact]
    public async Task GuardarAsync_CuandoEntradaEsNula_DebeLanzarArgumentNullException()
    {
        // Act & Assert
        await FluentActions
            .Invoking(() => _repositorio.GuardarAsync(null!))
            .Should()
            .ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GuardarAsync_CuandoSeCambiaCodigoQrAunoExistente_DebeLanzarInvalidOperationException()
    {
        // Arrange
        var entrada1 = Entrada.Crear(Guid.NewGuid(), Guid.NewGuid(), 100m, Guid.NewGuid(), "QR-1");
        var entrada2 = Entrada.Crear(Guid.NewGuid(), Guid.NewGuid(), 200m, Guid.NewGuid(), "QR-2");
        
        await _repositorio.GuardarAsync(entrada1);
        await _repositorio.GuardarAsync(entrada2);

        // Act
        // Intentamos crear una nueva entrada (con ID diferente) pero usando el QR-1 que ya existe
        var entradaModificada = Entrada.Crear(entrada2.EventoId, entrada2.UsuarioId, entrada2.Monto, entrada2.AsientoId, "QR-1");

        // Act & Assert
        await FluentActions
            .Invoking(() => _repositorio.GuardarAsync(entradaModificada))
            .Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("*Ya existe una entrada con el código QR: QR-1*");
    }

    [Fact]
    public async Task EliminarAsync_CuandoIdEsVacio_DebeLanzarArgumentException()
    {
        // Act & Assert
        await FluentActions
            .Invoking(() => _repositorio.EliminarAsync(Guid.Empty))
            .Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage("*ID no puede ser vacío*");
    }

    [Fact]
    public async Task ExisteCodigoQrAsync_CuandoCodigoEsNulo_DebeLanzarArgumentException()
    {
        // Act & Assert
        await FluentActions
            .Invoking(() => _repositorio.ExisteCodigoQrAsync(null!))
            .Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage("*Código QR requerido*");
    }

    [Fact]
    public async Task ExisteCodigoQrAsync_CuandoCodigoEsVacio_DebeLanzarArgumentException()
    {
        // Act & Assert
        await FluentActions
            .Invoking(() => _repositorio.ExisteCodigoQrAsync("  "))
            .Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage("*Código QR requerido*");
    }

    [Fact]
    public async Task ContarPorEventoYEstadoAsync_CuandoEventoIdEsVacio_DebeLanzarArgumentException()
    {
        // Act & Assert
        await FluentActions
            .Invoking(() => _repositorio.ContarPorEventoYEstadoAsync(Guid.Empty, EstadoEntrada.Pagada))
            .Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage("*ID requerido*");
    }

    // Para cubrir los catch blocks, necesitamos un contexto que falle.
    // Una forma es usar un DbContext con una conexión cerrada o nula para algunas operaciones si es posible,
    // o simplemente mockear el DbContext si fuera posible (aunque es difícil).
    // Otra opción es usar un DbContext que lance una excepción al guardar.
}
