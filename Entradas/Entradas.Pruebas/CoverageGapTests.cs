using System;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;
using FluentAssertions;
using Entradas.Dominio.Excepciones;
using Entradas.Infraestructura.ServiciosExternos;
using System.Text.Json;
using Polly.CircuitBreaker;
using Entradas.Dominio.Interfaces;
using Entradas.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;
using Entradas.Dominio.Entidades;

namespace Entradas.Pruebas;

public class CoverageGapTests
{
    private readonly Mock<ILogger<VerificadorAsientosHttp>> _mockLoggerAsientos;
    private readonly Mock<ILogger<VerificadorEventosHttp>> _mockLoggerEventos;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;

    public CoverageGapTests()
    {
        _mockLoggerAsientos = new Mock<ILogger<VerificadorAsientosHttp>>();
        _mockLoggerEventos = new Mock<ILogger<VerificadorEventosHttp>>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://localhost:5000")
        };
    }

    [Fact]
    public void EventoInfo_Coverage()
    {
        var info = new EventoInfo(Guid.NewGuid(), "Nombre", DateTime.Now, true, 100m);
        info.Nombre.Should().Be("Nombre");
    }

    [Fact]
    public void DbContext_Sync_SaveChanges_And_Configuring()
    {
        var options = new DbContextOptionsBuilder<EntradasDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new EntradasDbContext(options);
        var entrada = Entrada.Crear(Guid.NewGuid(), Guid.NewGuid(), 100m, Guid.NewGuid(), "SYNC-TEST");
        context.Entradas.Add(entrada);
        
        // Call sync SaveChanges
        var result = context.SaveChanges();
        result.Should().Be(1);

        // Test modified state in sync SaveChanges
        entrada.ConfirmarPago();
        context.Entradas.Update(entrada);
        context.SaveChanges();
    }

    [Fact]
    public async Task VerificadorEventos_ExecuteHttpWithResilienceAsync_Scenarios()
    {
        var options = new VerificadorEventosOptions { BaseUrl = "http://localhost:5001", ApiKey = "test-key" };
        var mockOptions = new Mock<IOptions<VerificadorEventosOptions>>();
        mockOptions.Setup(x => x.Value).Returns(options);
        var verificador = new VerificadorEventosHttp(_httpClient, _mockLoggerEventos.Object, mockOptions.Object);

        // BrokenCircuitException
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new BrokenCircuitException());

        await FluentActions.Awaiting(() => verificador.EventoExisteYDisponibleAsync(Guid.NewGuid()))
            .Should().ThrowAsync<ServicioExternoNoDisponibleException>()
            .WithMessage("*no está disponible temporalmente*");

        // TaskCanceledException (Timeout)
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Timeout", new TimeoutException()));

        await FluentActions.Awaiting(() => verificador.EventoExisteYDisponibleAsync(Guid.NewGuid()))
            .Should().ThrowAsync<ServicioExternoNoDisponibleException>()
            .WithMessage("*Timeout*");

        // General Exception
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Unexpected"));

        await FluentActions.Awaiting(() => verificador.EventoExisteYDisponibleAsync(Guid.NewGuid()))
            .Should().ThrowAsync<ServicioExternoNoDisponibleException>()
            .WithMessage("*Error inesperado*");
    }

    [Fact]
    public async Task VerificadorAsientos_Success_Scenarios()
    {
        var options = new VerificadorAsientosOptions { BaseUrl = "http://localhost:5002", ApiKey = "test-key" };
        var mockOptions = new Mock<IOptions<VerificadorAsientosOptions>>();
        mockOptions.Setup(x => x.Value).Returns(options);
        var verificador = new VerificadorAsientosHttp(_httpClient, _mockLoggerAsientos.Object, mockOptions.Object);

        var eventoId = Guid.NewGuid();
        var asientoId = Guid.NewGuid();
        var json = "{\"id\":\"" + asientoId + "\",\"eventoId\":\"" + eventoId + "\",\"seccion\":\"A\",\"fila\":\"1\",\"numero\":\"10\",\"categoriaId\":\"" + Guid.NewGuid() + "\",\"precio\":50.0,\"estaDisponible\":true}";
        
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) });

        var result = await verificador.ObtenerInfoAsientoAsync(eventoId, asientoId);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerInfoEventoAsync_Success_DebeRetornarInfo()
    {
        var options = new VerificadorEventosOptions { BaseUrl = "http://localhost:5001" };
        var mockOptions = new Mock<IOptions<VerificadorEventosOptions>>();
        mockOptions.Setup(x => x.Value).Returns(options);
        var verificador = new VerificadorEventosHttp(_httpClient, _mockLoggerEventos.Object, mockOptions.Object);

        var eventoId = Guid.NewGuid();
        var json = "{\"id\":\"" + eventoId + "\",\"titulo\":\"Test\",\"fechaInicio\":\"2025-01-01\",\"estaDisponible\":true,\"precioBase\":10.0}";
        
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) });

        var result = await verificador.ObtenerInfoEventoAsync(eventoId);
        result.Should().NotBeNull();
        result!.Nombre.Should().Be("Test");
    }

    [Fact]
    public async Task ObtenerInfoEventoAsync_NotFound_DebeRetornarNull()
    {
        var options = new VerificadorEventosOptions { BaseUrl = "http://localhost:5001" };
        var mockOptions = new Mock<IOptions<VerificadorEventosOptions>>();
        mockOptions.Setup(x => x.Value).Returns(options);
        var verificador = new VerificadorEventosHttp(_httpClient, _mockLoggerEventos.Object, mockOptions.Object);

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

        var result = await verificador.ObtenerInfoEventoAsync(Guid.NewGuid());
        result.Should().BeNull();
    }

    [Fact]
    public void ExceptionConstructors_Coverage()
    {
        // AsientoNoDisponibleException
        new AsientoNoDisponibleException("m");
        new AsientoNoDisponibleException(Guid.NewGuid(), Guid.NewGuid(), "m");
        new AsientoNoDisponibleException(Guid.NewGuid(), Guid.NewGuid(), "m", new Exception());
        
        // EntradaNoEncontradaException
        new EntradaNoEncontradaException(Guid.NewGuid(), "m");
        new EntradaNoEncontradaException(Guid.NewGuid(), "m", new Exception());

        // EventoNoDisponibleException
        new EventoNoDisponibleException(Guid.NewGuid(), "m");
        new EventoNoDisponibleException(Guid.NewGuid(), "m", new Exception());

        // ServicioExternoNoDisponibleException
        new ServicioExternoNoDisponibleException("s", "m");
        new ServicioExternoNoDisponibleException("s", "m", new Exception());
        
        // EstadoEntradaInvalidoException
        new EstadoEntradaInvalidoException("m");
        new EstadoEntradaInvalidoException("m", new Exception());
    }
}
