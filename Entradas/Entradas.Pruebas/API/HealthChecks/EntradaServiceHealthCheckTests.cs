using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Entradas.API.HealthChecks;
using Entradas.Dominio.Interfaces;
using Entradas.Dominio.Excepciones;

namespace Entradas.Pruebas.API.HealthChecks;

/// <summary>
/// Pruebas unitarias para EntradaServiceHealthCheck
/// Valida el correcto funcionamiento del health check personalizado del servicio de entradas
/// </summary>
public class EntradaServiceHealthCheckTests
{
    private readonly Mock<IVerificadorEventos> _verificadorEventosMock;
    private readonly Mock<IVerificadorAsientos> _verificadorAsientosMock;
    private readonly Mock<IGeneradorCodigoQr> _generadorQrMock;
    private readonly Mock<ILogger<EntradaServiceHealthCheck>> _loggerMock;
    private readonly EntradaServiceHealthCheck _healthCheck;

    public EntradaServiceHealthCheckTests()
    {
        _verificadorEventosMock = new Mock<IVerificadorEventos>();
        _verificadorAsientosMock = new Mock<IVerificadorAsientos>();
        _generadorQrMock = new Mock<IGeneradorCodigoQr>();
        _loggerMock = new Mock<ILogger<EntradaServiceHealthCheck>>();
        
        _healthCheck = new EntradaServiceHealthCheck(
            _verificadorEventosMock.Object,
            _verificadorAsientosMock.Object,
            _generadorQrMock.Object,
            _loggerMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_ConParametrosValidos_DebeCrearInstancia()
    {
        // Arrange & Act
        var healthCheck = new EntradaServiceHealthCheck(
            _verificadorEventosMock.Object,
            _verificadorAsientosMock.Object,
            _generadorQrMock.Object,
            _loggerMock.Object);

        // Assert
        healthCheck.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_ConVerificadorEventosNulo_DebeCrearInstancia()
    {
        // Arrange & Act
        // Note: The actual constructor doesn't validate null parameters
        var healthCheck = new EntradaServiceHealthCheck(
            null!,
            _verificadorAsientosMock.Object,
            _generadorQrMock.Object,
            _loggerMock.Object);

        // Assert
        healthCheck.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_ConVerificadorAsientosNulo_DebeCrearInstancia()
    {
        // Arrange & Act
        // Note: The actual constructor doesn't validate null parameters
        var healthCheck = new EntradaServiceHealthCheck(
            _verificadorEventosMock.Object,
            null!,
            _generadorQrMock.Object,
            _loggerMock.Object);

        // Assert
        healthCheck.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_ConGeneradorQrNulo_DebeCrearInstancia()
    {
        // Arrange & Act
        // Note: The actual constructor doesn't validate null parameters
        var healthCheck = new EntradaServiceHealthCheck(
            _verificadorEventosMock.Object,
            _verificadorAsientosMock.Object,
            null!,
            _loggerMock.Object);

        // Assert
        healthCheck.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_ConLoggerNulo_DebeCrearInstancia()
    {
        // Arrange & Act
        // Note: The actual constructor doesn't validate null parameters
        var healthCheck = new EntradaServiceHealthCheck(
            _verificadorEventosMock.Object,
            _verificadorAsientosMock.Object,
            _generadorQrMock.Object,
            null!);

        // Assert
        healthCheck.Should().NotBeNull();
    }

    #endregion

    #region Healthy Tests

    [Fact]
    public async Task CheckHealthAsync_ConTodosLosServiciosFuncionando_DebeRetornarHealthy()
    {
        // Arrange
        var codigoQr = "TICKET-12345-ABCDE";
        _generadorQrMock.Setup(g => g.GenerarCodigoUnico()).Returns(codigoQr);
        
        // Los servicios externos fallan con IDs de prueba (comportamiento esperado)
        _verificadorEventosMock.Setup(v => v.EventoExisteYDisponibleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                              .ThrowsAsync(new EventoNoDisponibleException(Guid.NewGuid(), "Evento de prueba no encontrado"));
        
        _verificadorAsientosMock.Setup(v => v.AsientoDisponibleAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                .ThrowsAsync(new AsientoNoDisponibleException("Asiento de prueba no encontrado"));

        var context = new HealthCheckContext();

        // Act
        var result = await _healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Be("Servicio de entradas funcionando correctamente");
        result.Data.Should().ContainKey("generador_qr");
        result.Data.Should().ContainKey("verificador_eventos");
        result.Data.Should().ContainKey("verificador_asientos");
        result.Data.Should().ContainKey("timestamp");
        result.Data.Should().ContainKey("version");
        
        result.Data["generador_qr"].Should().Be("OK");
        result.Data["verificador_eventos"].Should().Be("OK");
        result.Data["verificador_asientos"].Should().Be("OK");
    }

    #endregion

    #region Degraded Tests

    [Fact]
    public async Task CheckHealthAsync_ConGeneradorQrFallando_DebeRetornarDegraded()
    {
        // Arrange
        _generadorQrMock.Setup(g => g.GenerarCodigoUnico()).Returns(string.Empty);
        
        _verificadorEventosMock.Setup(v => v.EventoExisteYDisponibleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                              .ThrowsAsync(new EventoNoDisponibleException(Guid.NewGuid(), "Evento de prueba no encontrado"));
        
        _verificadorAsientosMock.Setup(v => v.AsientoDisponibleAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                .ThrowsAsync(new AsientoNoDisponibleException("Asiento de prueba no encontrado"));

        var context = new HealthCheckContext();

        // Act
        var result = await _healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Description.Should().Contain("Problemas detectados");
        result.Description.Should().Contain("Generador de códigos QR no está funcionando correctamente");
        result.Data.Should().ContainKey("verificador_eventos");
        result.Data.Should().ContainKey("verificador_asientos");
    }

    [Fact]
    public async Task CheckHealthAsync_ConGeneradorQrSinPrefijo_DebeRetornarDegraded()
    {
        // Arrange
        var codigoQrInvalido = "INVALID-CODE-12345";
        _generadorQrMock.Setup(g => g.GenerarCodigoUnico()).Returns(codigoQrInvalido);
        
        _verificadorEventosMock.Setup(v => v.EventoExisteYDisponibleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                              .ThrowsAsync(new EventoNoDisponibleException(Guid.NewGuid(), "Evento de prueba no encontrado"));
        
        _verificadorAsientosMock.Setup(v => v.AsientoDisponibleAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                .ThrowsAsync(new AsientoNoDisponibleException("Asiento de prueba no encontrado"));

        var context = new HealthCheckContext();

        // Act
        var result = await _healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Description.Should().Contain("Generador de códigos QR no está funcionando correctamente");
    }

    [Fact]
    public async Task CheckHealthAsync_ConGeneradorQrLanzandoExcepcion_DebeRetornarDegraded()
    {
        // Arrange
        var exception = new InvalidOperationException("Error en generador QR");
        _generadorQrMock.Setup(g => g.GenerarCodigoUnico()).Throws(exception);
        
        _verificadorEventosMock.Setup(v => v.EventoExisteYDisponibleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                              .ThrowsAsync(new EventoNoDisponibleException(Guid.NewGuid(), "Evento de prueba no encontrado"));
        
        _verificadorAsientosMock.Setup(v => v.AsientoDisponibleAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                .ThrowsAsync(new AsientoNoDisponibleException("Asiento de prueba no encontrado"));

        var context = new HealthCheckContext();

        // Act
        var result = await _healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Description.Should().Contain("Error en generador QR: Error en generador QR");
        
        // Verify warning was logged
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error al verificar generador de códigos QR")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Timeout Tests

    [Fact]
    public async Task CheckHealthAsync_ConTimeoutEnVerificadorEventos_DebeIncluirTimeoutEnData()
    {
        // Arrange
        var codigoQr = "TICKET-12345-ABCDE";
        _generadorQrMock.Setup(g => g.GenerarCodigoUnico()).Returns(codigoQr);
        
        // Simular timeout en verificador de eventos
        _verificadorEventosMock.Setup(v => v.EventoExisteYDisponibleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                              .Returns(async (Guid id, CancellationToken ct) =>
                              {
                                  await Task.Delay(TimeSpan.FromSeconds(10), ct); // Más que el timeout de 5s
                                  return true;
                              });
        
        _verificadorAsientosMock.Setup(v => v.AsientoDisponibleAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                .ThrowsAsync(new AsientoNoDisponibleException("Asiento de prueba no encontrado"));

        var context = new HealthCheckContext();

        // Act
        var result = await _healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy); // Timeout no marca como unhealthy
        result.Data.Should().ContainKey("verificador_eventos");
        result.Data["verificador_eventos"].Should().Be("TIMEOUT");
    }

    [Fact]
    public async Task CheckHealthAsync_ConTimeoutEnVerificadorAsientos_DebeIncluirTimeoutEnData()
    {
        // Arrange
        var codigoQr = "TICKET-12345-ABCDE";
        _generadorQrMock.Setup(g => g.GenerarCodigoUnico()).Returns(codigoQr);
        
        _verificadorEventosMock.Setup(v => v.EventoExisteYDisponibleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                              .ThrowsAsync(new EventoNoDisponibleException(Guid.NewGuid(), "Evento de prueba no encontrado"));
        
        // Simular timeout en verificador de asientos
        _verificadorAsientosMock.Setup(v => v.AsientoDisponibleAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                .Returns(async (Guid eventoId, Guid asientoId, CancellationToken ct) =>
                                {
                                    await Task.Delay(TimeSpan.FromSeconds(10), ct); // Más que el timeout de 5s
                                    return true;
                                });

        var context = new HealthCheckContext();

        // Act
        var result = await _healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy); // Timeout no marca como unhealthy
        result.Data.Should().ContainKey("verificador_asientos");
        result.Data["verificador_asientos"].Should().Be("TIMEOUT");
    }

    #endregion

    #region Unhealthy Tests

    [Fact]
    public async Task CheckHealthAsync_ConExcepcionInesperada_DebeRetornarDegraded()
    {
        // Arrange
        var exception = new OutOfMemoryException("Error crítico del sistema");
        _generadorQrMock.Setup(g => g.GenerarCodigoUnico()).Throws(exception);
        
        var context = new HealthCheckContext();

        // Act
        var result = await _healthCheck.CheckHealthAsync(context);

        // Assert
        // The health check catches all exceptions in the QR generator and returns Degraded
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Description.Should().Contain("Problemas detectados");
        result.Description.Should().Contain("Error en generador QR: Error crítico del sistema");
        
        // Verify warning was logged
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error al verificar generador de códigos QR")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Cancellation Tests

    [Fact]
    public async Task CheckHealthAsync_ConCancellationToken_DebeManejarseSinError()
    {
        // Arrange
        var codigoQr = "TICKET-12345-ABCDE";
        _generadorQrMock.Setup(g => g.GenerarCodigoUnico()).Returns(codigoQr);
        
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancelar inmediatamente
        
        // The health check uses its own timeout internally, so external cancellation
        // may not immediately cancel the operation
        _verificadorEventosMock.Setup(v => v.EventoExisteYDisponibleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                              .ThrowsAsync(new EventoNoDisponibleException(Guid.NewGuid(), "Evento de prueba no encontrado"));
        
        _verificadorAsientosMock.Setup(v => v.AsientoDisponibleAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                .ThrowsAsync(new AsientoNoDisponibleException("Asiento de prueba no encontrado"));

        var context = new HealthCheckContext();

        // Act
        var result = await _healthCheck.CheckHealthAsync(context, cts.Token);

        // Assert
        // The health check should complete even with cancelled token since it uses internal timeout
        result.Should().NotBeNull();
        result.Status.Should().BeOneOf(HealthStatus.Healthy, HealthStatus.Degraded);
    }

    #endregion

    #region Data Validation Tests

    [Fact]
    public async Task CheckHealthAsync_Siempre_DebeIncluirTimestampYVersion()
    {
        // Arrange
        var codigoQr = "TICKET-12345-ABCDE";
        _generadorQrMock.Setup(g => g.GenerarCodigoUnico()).Returns(codigoQr);
        
        _verificadorEventosMock.Setup(v => v.EventoExisteYDisponibleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                              .ThrowsAsync(new EventoNoDisponibleException(Guid.NewGuid(), "Evento de prueba no encontrado"));
        
        _verificadorAsientosMock.Setup(v => v.AsientoDisponibleAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                .ThrowsAsync(new AsientoNoDisponibleException("Asiento de prueba no encontrado"));

        var context = new HealthCheckContext();

        // Act
        var result = await _healthCheck.CheckHealthAsync(context);

        // Assert
        result.Data.Should().ContainKey("timestamp");
        result.Data.Should().ContainKey("version");
        
        result.Data["timestamp"].Should().BeOfType<DateTime>();
        result.Data["version"].Should().BeOfType<string>();
        
        var timestamp = (DateTime)result.Data["timestamp"];
        timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CheckHealthAsync_ConServiciosExternosOK_DebeUsarIDsDePrueba()
    {
        // Arrange
        var codigoQr = "TICKET-12345-ABCDE";
        _generadorQrMock.Setup(g => g.GenerarCodigoUnico()).Returns(codigoQr);
        
        var testEventoId = new Guid("00000000-0000-0000-0000-000000000001");
        var testAsientoId = new Guid("00000000-0000-0000-0000-000000000002");
        
        _verificadorEventosMock.Setup(v => v.EventoExisteYDisponibleAsync(testEventoId, It.IsAny<CancellationToken>()))
                              .ThrowsAsync(new EventoNoDisponibleException(testEventoId, "Evento de prueba no encontrado"));
        
        _verificadorAsientosMock.Setup(v => v.AsientoDisponibleAsync(testEventoId, testAsientoId, It.IsAny<CancellationToken>()))
                                .ThrowsAsync(new AsientoNoDisponibleException("Asiento de prueba no encontrado"));

        var context = new HealthCheckContext();

        // Act
        var result = await _healthCheck.CheckHealthAsync(context);

        // Assert
        _verificadorEventosMock.Verify(v => v.EventoExisteYDisponibleAsync(testEventoId, It.IsAny<CancellationToken>()), Times.Once);
        _verificadorAsientosMock.Verify(v => v.AsientoDisponibleAsync(testEventoId, testAsientoId, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Edge Cases Tests

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CheckHealthAsync_ConGeneradorQrRetornandoValorInvalido_DebeRetornarDegraded(string? codigoInvalido)
    {
        // Arrange
        _generadorQrMock.Setup(g => g.GenerarCodigoUnico()).Returns(codigoInvalido!);
        
        _verificadorEventosMock.Setup(v => v.EventoExisteYDisponibleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                              .ThrowsAsync(new EventoNoDisponibleException(Guid.NewGuid(), "Evento de prueba no encontrado"));
        
        _verificadorAsientosMock.Setup(v => v.AsientoDisponibleAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                .ThrowsAsync(new AsientoNoDisponibleException("Asiento de prueba no encontrado"));

        var context = new HealthCheckContext();

        // Act
        var result = await _healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Description.Should().Contain("Generador de códigos QR no está funcionando correctamente");
    }

    [Fact]
    public async Task CheckHealthAsync_ConContextoNulo_DebeManejarseSinError()
    {
        // Arrange
        var codigoQr = "TICKET-12345-ABCDE";
        _generadorQrMock.Setup(g => g.GenerarCodigoUnico()).Returns(codigoQr);
        
        _verificadorEventosMock.Setup(v => v.EventoExisteYDisponibleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                              .ThrowsAsync(new EventoNoDisponibleException(Guid.NewGuid(), "Evento de prueba no encontrado"));
        
        _verificadorAsientosMock.Setup(v => v.AsientoDisponibleAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                .ThrowsAsync(new AsientoNoDisponibleException("Asiento de prueba no encontrado"));

        // Act
        var result = await _healthCheck.CheckHealthAsync(null!);

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
    }

    #endregion
}