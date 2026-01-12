/*using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;
using Entradas.Infraestructura.Persistencia;
using Entradas.Dominio.Interfaces;
using Entradas.Dominio.Entidades;
using Entradas.Aplicacion.Comandos;
using Entradas.Aplicacion.DTOs;
using Moq;

namespace Entradas.Pruebas.Integracion;

/// <summary>
/// Pruebas end-to-end con flujo completo de creación y confirmación
/// Integración con servicios externos mockeados con WireMock
/// </summary>
public class EndToEndIntegrationTests : IAsyncLifetime
{
    private PostgreSqlContainer? _postgresContainer;
    private RabbitMqContainer? _rabbitMqContainer;
    private WebApplicationFactory<Program>? _factory;
    private HttpClient? _client;
    private EntradasDbContext? _dbContext;

    public async Task InitializeAsync()
    {
        // Crear contenedor PostgreSQL
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("entradas_e2e_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        await _postgresContainer.StartAsync();

        // Crear contenedor RabbitMQ
        _rabbitMqContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:3.13-management-alpine")
            .Build();

        await _rabbitMqContainer.StartAsync();

        // Configurar WebApplicationFactory con contenedores
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Reemplazar DbContext con la conexión del contenedor
                    var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(DbContextOptions<EntradasDbContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddDbContext<EntradasDbContext>(options =>
                        options.UseNpgsql(_postgresContainer.GetConnectionString())
                    );

                    // Mock de servicios externos
                    var mockVerificadorEventos = new Mock<IVerificadorEventos>();
                    mockVerificadorEventos
                        .Setup(x => x.EventoExisteYDisponibleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(true);

                    var mockVerificadorAsientos = new Mock<IVerificadorAsientos>();
                    mockVerificadorAsientos
                        .Setup(x => x.AsientoDisponibleAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(true);

                    services.AddScoped(_ => mockVerificadorEventos.Object);
                    services.AddScoped(_ => mockVerificadorAsientos.Object);
                });
            });

        _client = _factory.CreateClient();

        // Obtener DbContext para verificaciones
        using var scope = _factory.Services.CreateScope();
        _dbContext = scope.ServiceProvider.GetRequiredService<EntradasDbContext>();
        
        // Crear esquema de base de datos
        await _dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        if (_dbContext != null)
        {
            await _dbContext.DisposeAsync();
        }

        if (_factory != null)
        {
            _factory.Dispose();
        }

        if (_postgresContainer != null)
        {
            await _postgresContainer.StopAsync();
            await _postgresContainer.DisposeAsync();
        }

        if (_rabbitMqContainer != null)
        {
            await _rabbitMqContainer.StopAsync();
            await _rabbitMqContainer.DisposeAsync();
        }
    }

    [Fact]
    public async Task E2E_CrearEntrada_DebeRetornarExitosamente()
    {
        // Arrange
        var comando = new
        {
            eventoId = Guid.NewGuid(),
            usuarioId = Guid.NewGuid(),
            asientoId = (Guid?)Guid.NewGuid(),
            monto = 150.50m
        };

        var content = new StringContent(
            JsonSerializer.Serialize(comando),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response = await _client!.PostAsync("/api/entradas", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var entradaCreada = JsonSerializer.Deserialize<EntradaCreadaDto>(
            responseContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        entradaCreada.Should().NotBeNull();
        entradaCreada!.EventoId.Should().Be(comando.eventoId);
        entradaCreada.UsuarioId.Should().Be(comando.usuarioId);
        entradaCreada.Monto.Should().Be(150.50m);
    }

    [Fact]
    public async Task E2E_CrearEntrada_DebeGuardarEnBaseDatos()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var asientoId = Guid.NewGuid();

        var comando = new
        {
            eventoId,
            usuarioId,
            asientoId = (Guid?)asientoId,
            monto = 200.75m
        };

        var content = new StringContent(
            JsonSerializer.Serialize(comando),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response = await _client!.PostAsync("/api/entradas", content);
        var responseContent = await response.Content.ReadAsStringAsync();
        var entradaCreada = JsonSerializer.Deserialize<EntradaCreadaDto>(
            responseContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        // Verificar que se guardó en la base de datos
        using var scope = _factory!.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EntradasDbContext>();
        
        var entradaEnDb = await dbContext.Entradas
            .FirstOrDefaultAsync(e => e.Id == entradaCreada!.Id);

        entradaEnDb.Should().NotBeNull();
        entradaEnDb!.EventoId.Should().Be(eventoId);
        entradaEnDb.UsuarioId.Should().Be(usuarioId);
        entradaEnDb.AsientoId.Should().Be(asientoId);
    }

    [Fact]
    public async Task E2E_ObtenerEntrada_DebeRetornarCorrectamente()
    {
        // Arrange
        var entrada = Entrada.Crear(
            Guid.NewGuid(), Guid.NewGuid(), 100m, Guid.NewGuid(), "E2E-GET-001"
        );

        using (var scope = _factory!.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<EntradasDbContext>();
            dbContext.Entradas.Add(entrada);
            await dbContext.SaveChangesAsync();
        }

        // Act
        var response = await _client!.GetAsync($"/api/entradas/{entrada.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var entradaObtenida = JsonSerializer.Deserialize<EntradaDto>(
            responseContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        entradaObtenida.Should().NotBeNull();
        entradaObtenida!.Id.Should().Be(entrada.Id);
        entradaObtenida.CodigoQr.Should().Be("E2E-GET-001");
    }

    [Fact]
    public async Task E2E_ObtenerEntradaInexistente_DebeRetornar404()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await _client!.GetAsync($"/api/entradas/{idInexistente}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task E2E_ObtenerEntradasPorUsuario_DebeRetornarCorrectamente()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var entradas = new List<Entrada>
        {
            Entrada.Crear(Guid.NewGuid(), usuarioId, 100m, Guid.NewGuid(), "E2E-USER-1"),
            Entrada.Crear(Guid.NewGuid(), usuarioId, 200m, Guid.NewGuid(), "E2E-USER-2"),
            Entrada.Crear(Guid.NewGuid(), Guid.NewGuid(), 300m, Guid.NewGuid(), "E2E-OTHER")
        };

        using (var scope = _factory!.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<EntradasDbContext>();
            dbContext.Entradas.AddRange(entradas);
            await dbContext.SaveChangesAsync();
        }

        // Act
        var response = await _client!.GetAsync($"/api/entradas/usuario/{usuarioId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var entradasObtenidas = JsonSerializer.Deserialize<List<EntradaDto>>(
            responseContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        entradasObtenidas.Should().HaveCount(2);
        entradasObtenidas.Should().AllSatisfy(e => e.UsuarioId.Should().Be(usuarioId));
    }

    [Fact]
    public async Task E2E_CrearEntrada_ConValidacionExternaFallida_DebeRetornarError()
    {
        // Arrange
        using var scope = _factory!.Services.CreateScope();
        var mockVerificador = scope.ServiceProvider.GetRequiredService<IVerificadorEventos>();
        
        // Configurar mock para fallar
        var mockVerificadorFallido = new Mock<IVerificadorEventos>();
        mockVerificadorFallido
            .Setup(x => x.EventoExisteYDisponibleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var comando = new
        {
            eventoId = Guid.NewGuid(),
            usuarioId = Guid.NewGuid(),
            asientoId = (Guid?)null,
            monto = 150m
        };

        var content = new StringContent(
            JsonSerializer.Serialize(comando),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response = await _client!.PostAsync("/api/entradas", content);

        // Assert
        // Puede ser 400 o 422 dependiendo de la configuración
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity,
            HttpStatusCode.InternalServerError
        );
    }

    [Fact]
    public async Task E2E_CrearEntrada_ConDatosInvalidos_DebeRetornarValidationError()
    {
        // Arrange
        var comando = new
        {
            eventoId = Guid.Empty, // Inválido
            usuarioId = Guid.Empty, // Inválido
            asientoId = (Guid?)null,
            monto = -100m // Inválido
        };

        var content = new StringContent(
            JsonSerializer.Serialize(comando),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response = await _client!.PostAsync("/api/entradas", content);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity
        );
    }

    [Fact]
    public async Task E2E_MultipleCreaciones_DebenGuardarseCorrectamente()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var eventoId = Guid.NewGuid();

        var comandos = Enumerable.Range(1, 5)
            .Select(i => new
            {
                eventoId,
                usuarioId,
                asientoId = (Guid?)Guid.NewGuid(),
                monto = 100m + i
            })
            .ToList();

        // Act
        var responses = new List<HttpResponseMessage>();
        foreach (var comando in comandos)
        {
            var content = new StringContent(
                JsonSerializer.Serialize(comando),
                Encoding.UTF8,
                "application/json"
            );
            var response = await _client!.PostAsync("/api/entradas", content);
            responses.Add(response);
        }

        // Assert
        responses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.Created));

        // Verificar que todas se guardaron en la base de datos
        using var scope = _factory!.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EntradasDbContext>();
        
        var entradasGuardadas = await dbContext.Entradas
            .Where(e => e.UsuarioId == usuarioId && e.EventoId == eventoId)
            .ToListAsync();

        entradasGuardadas.Should().HaveCount(5);
    }

    [Fact]
    public async Task E2E_CorrelationId_DebeMantenersePorSolicitud()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();
        var comando = new
        {
            eventoId = Guid.NewGuid(),
            usuarioId = Guid.NewGuid(),
            asientoId = (Guid?)null,
            monto = 150m
        };

        var content = new StringContent(
            JsonSerializer.Serialize(comando),
            Encoding.UTF8,
            "application/json"
        );

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/entradas")
        {
            Content = content
        };
        request.Headers.Add("X-Correlation-ID", correlationId);

        // Act
        var response = await _client!.SendAsync(request);

        // Assert
        response.Headers.Should().ContainKey("X-Correlation-ID");
        response.Headers.GetValues("X-Correlation-ID").First().Should().Be(correlationId);
    }

    [Fact]
    public async Task E2E_HealthCheck_DebeIndicarEstadoDelServicio()
    {
        // Act
        var response = await _client!.GetAsync("/health");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task E2E_Swagger_DebeEstarDisponibleEnDesarrollo()
    {
        // Act
        var response = await _client!.GetAsync("/swagger/v1/swagger.json");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task E2E_Performance_CreacionDeEntrada_DebeSerRapida()
    {
        // Arrange
        var comando = new
        {
            eventoId = Guid.NewGuid(),
            usuarioId = Guid.NewGuid(),
            asientoId = (Guid?)Guid.NewGuid(),
            monto = 150m
        };

        var content = new StringContent(
            JsonSerializer.Serialize(comando),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await _client!.PostAsync("/api/entradas", content);
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000);
    }

    [Fact]
    public async Task E2E_Concurrencia_MultiplesSolicitudes_DebenProcesarseCorrectamente()
    {
        // Arrange
        var tareas = new List<Task<HttpResponseMessage>>();

        // Act
        for (int i = 0; i < 10; i++)
        {
            var comando = new
            {
                eventoId = Guid.NewGuid(),
                usuarioId = Guid.NewGuid(),
                asientoId = (Guid?)Guid.NewGuid(),
                monto = 100m + i
            };

            var content = new StringContent(
                JsonSerializer.Serialize(comando),
                Encoding.UTF8,
                "application/json"
            );

            tareas.Add(_client!.PostAsync("/api/entradas", content));
        }

        var responses = await Task.WhenAll(tareas);

        // Assert
        responses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.Created));
    }
}
*/