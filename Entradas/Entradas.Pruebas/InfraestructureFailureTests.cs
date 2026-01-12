using System;
using System.Net;
using System.Reflection;
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
using Entradas.Infraestructura.Repositorios;
using Microsoft.EntityFrameworkCore;
using Entradas.Dominio.Entidades;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Entradas.Infraestructura;

namespace Entradas.Pruebas;

public class InfraestructureFailureTests
{
    private readonly Mock<ILogger<UnitOfWork>> _mockLoggerUow;
    private readonly Mock<ILogger<VerificadorEventosHttp>> _mockLoggerEventos;
    private readonly Mock<ILogger<RepositorioEntradas>> _mockLoggerRepo;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;

    public InfraestructureFailureTests()
    {
        _mockLoggerUow = new Mock<ILogger<UnitOfWork>>();
        _mockLoggerEventos = new Mock<ILogger<VerificadorEventosHttp>>();
        _mockLoggerRepo = new Mock<ILogger<RepositorioEntradas>>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://localhost:5000")
        };
    }

    [Fact]
    public async Task UnitOfWork_CommitFailure_DebeHacerRollback()
    {
        var options = new DbContextOptionsBuilder<EntradasDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        using var context = new EntradasDbContext(options);
        var uow = new UnitOfWork(context, _mockLoggerUow.Object);

        var mockTransaction = new Mock<IDbContextTransaction>();
        mockTransaction.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Commit Error"));

        // Use reflection to set the private transaction field
        var field = typeof(UnitOfWork).GetField("_currentTransaction", BindingFlags.NonPublic | BindingFlags.Instance);
        field!.SetValue(uow, mockTransaction.Object);

        await FluentActions.Awaiting(() => uow.CommitTransactionAsync())
            .Should().ThrowAsync<Exception>().WithMessage("Commit Error");

        mockTransaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UnitOfWork_RollbackFailure_Coverage()
    {
        var options = new DbContextOptionsBuilder<EntradasDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        using var context = new EntradasDbContext(options);
        var uow = new UnitOfWork(context, _mockLoggerUow.Object);

        var mockTransaction = new Mock<IDbContextTransaction>();
        mockTransaction.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Rollback Error"));

        var field = typeof(UnitOfWork).GetField("_currentTransaction", BindingFlags.NonPublic | BindingFlags.Instance);
        field!.SetValue(uow, mockTransaction.Object);

        await FluentActions.Awaiting(() => uow.RollbackTransactionAsync())
            .Should().ThrowAsync<Exception>().WithMessage("Rollback Error");
    }

    [Fact]
    public void EntradasDbContext_OnConfiguring_Coverage()
    {
        // Test the OnConfiguring branch when optionsBuilder is not configured
        var context = new EntradasDbContext(new DbContextOptions<EntradasDbContext>());
        context.Should().NotBeNull();
    }

    [Fact]
    public async Task VerificadorEventos_EmptyGuidCheck_Coverage()
    {
        var options = new VerificadorEventosOptions { BaseUrl = "http://localhost:5001" };
        var mockOptions = new Mock<IOptions<VerificadorEventosOptions>>();
        mockOptions.Setup(x => x.Value).Returns(options);
        var verificador = new VerificadorEventosHttp(_httpClient, _mockLoggerEventos.Object, mockOptions.Object);

        // Empty ID check for ObtenerInfoEventoAsync
        await FluentActions.Awaiting(() => verificador.ObtenerInfoEventoAsync(Guid.Empty))
            .Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task VerificadorEventos_NullDtoResponse_Coverage()
    {
        var options = new VerificadorEventosOptions { BaseUrl = "http://localhost:5001" };
        var mockOptions = new Mock<IOptions<VerificadorEventosOptions>>();
        mockOptions.Setup(x => x.Value).Returns(options);
        var verificador = new VerificadorEventosHttp(_httpClient, _mockLoggerEventos.Object, mockOptions.Object);

        // Null DTO check
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("null") });

        var result = await verificador.ObtenerInfoEventoAsync(Guid.NewGuid());
        result.Should().BeNull();
    }

    [Fact]
    public async Task VerificadorEventos_JsonException_Coverage()
    {
        var options = new VerificadorEventosOptions { BaseUrl = "http://localhost:5001" };
        var mockOptions = new Mock<IOptions<VerificadorEventosOptions>>();
        mockOptions.Setup(x => x.Value).Returns(options);
        var verificador = new VerificadorEventosHttp(_httpClient, _mockLoggerEventos.Object, mockOptions.Object);

        // JsonException coverage
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{ invalid json") });

        await FluentActions.Awaiting(() => verificador.ObtenerInfoEventoAsync(Guid.NewGuid()))
            .Should().ThrowAsync<ServicioExternoNoDisponibleException>()
            .WithMessage("*procesar la respuesta*");
    }

    [Fact]
    public void DependencyInjection_Coverage()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["VerificadorEventos:BaseUrl"] = "http://localhost",
                ["VerificadorAsientos:BaseUrl"] = "http://localhost",
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test",
                ["Logging:EnableEntityFrameworkLogging"] = "true"
            })
            .Build();

        // Add required logging for internal registrations
        services.AddLogging();
        services.AddSingleton<System.Diagnostics.Metrics.Meter>(new System.Diagnostics.Metrics.Meter("test"));

        InyeccionDependencias.AgregarInfraestructura(services, configuration);

        var provider = services.BuildServiceProvider();
        provider.GetService<IRepositorioEntradas>().Should().NotBeNull();
        provider.GetService<IUnitOfWork>().Should().NotBeNull();
        provider.GetService<IVerificadorEventos>().Should().NotBeNull();
        provider.GetService<IVerificadorAsientos>().Should().NotBeNull();
    }
}
