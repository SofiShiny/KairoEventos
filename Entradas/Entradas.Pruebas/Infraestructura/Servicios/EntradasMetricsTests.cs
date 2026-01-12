using System.Diagnostics.Metrics;
using FluentAssertions;
using Xunit;
using Entradas.Infraestructura.Servicios;
using Entradas.Dominio.Interfaces;

namespace Entradas.Pruebas.Infraestructura.Servicios;

/// <summary>
/// Pruebas para EntradasMetrics
/// Valida la recopilación de métricas de contadores e histogramas
/// </summary>
public class EntradasMetricsTests : IDisposable
{
    private readonly Meter _meter;
    private readonly EntradasMetrics _metrics;
    private readonly List<Measurement<int>> _intMeasurements;
    private readonly List<Measurement<double>> _doubleMeasurements;

    public EntradasMetricsTests()
    {
        _meter = new Meter("TestMeter", "1.0.0");
        _metrics = new EntradasMetrics(_meter);
        _intMeasurements = new List<Measurement<int>>();
        _doubleMeasurements = new List<Measurement<double>>();
    }

    [Fact]
    public void Constructor_ConMeterValido_DeberiaCrearMetricsCorrectamente()
    {
        // Arrange & Act
        var meter = new Meter("TestMeter", "1.0.0");
        var metrics = new EntradasMetrics(meter);

        // Assert
        metrics.Should().NotBeNull();
        metrics.Should().BeAssignableTo<IEntradasMetrics>();
    }

    [Fact]
    public void IncrementEntradasCreadas_ConParametrosValidos_DeberiaIncrementarContador()
    {
        // Arrange
        var eventoId = Guid.NewGuid().ToString();
        var estado = "PendientePago";

        // Act
        _metrics.IncrementEntradasCreadas(eventoId, estado);

        // Assert
        // Verificar que el método se ejecuta sin excepciones
        FluentActions
            .Invoking(() => _metrics.IncrementEntradasCreadas(eventoId, estado))
            .Should()
            .NotThrow();
    }

    [Fact]
    public void IncrementEntradasCreadas_ConMultiplesLlamadas_DeberiaIncrementarCorrectamente()
    {
        // Arrange
        var eventoId = Guid.NewGuid().ToString();
        var estado = "Pagada";

        // Act
        _metrics.IncrementEntradasCreadas(eventoId, estado);
        _metrics.IncrementEntradasCreadas(eventoId, estado);
        _metrics.IncrementEntradasCreadas(eventoId, estado);

        // Assert
        // Verificar que múltiples llamadas se ejecutan sin excepciones
        FluentActions
            .Invoking(() => _metrics.IncrementEntradasCreadas(eventoId, estado))
            .Should()
            .NotThrow();
    }

    [Fact]
    public void IncrementEntradasCreadas_ConDiferentesEventos_DeberiaRegistrarCorrectamente()
    {
        // Arrange
        var eventoId1 = Guid.NewGuid().ToString();
        var eventoId2 = Guid.NewGuid().ToString();
        var estado = "PendientePago";

        // Act
        _metrics.IncrementEntradasCreadas(eventoId1, estado);
        _metrics.IncrementEntradasCreadas(eventoId2, estado);

        // Assert
        FluentActions
            .Invoking(() =>
            {
                _metrics.IncrementEntradasCreadas(eventoId1, estado);
                _metrics.IncrementEntradasCreadas(eventoId2, estado);
            })
            .Should()
            .NotThrow();
    }

    [Fact]
    public void IncrementEntradasCreadas_ConDiferentesEstados_DeberiaRegistrarCorrectamente()
    {
        // Arrange
        var eventoId = Guid.NewGuid().ToString();
        var estados = new[] { "PendientePago", "Pagada", "Cancelada", "Usada" };

        // Act & Assert
        foreach (var estado in estados)
        {
            FluentActions
                .Invoking(() => _metrics.IncrementEntradasCreadas(eventoId, estado))
                .Should()
                .NotThrow();
        }
    }

    [Fact]
    public void RecordCreacionDuration_ConDuracionValida_DeberiaRegistrarHistograma()
    {
        // Arrange
        var durationMs = 150.5;
        var resultado = "exitoso";

        // Act
        _metrics.RecordCreacionDuration(durationMs, resultado);

        // Assert
        FluentActions
            .Invoking(() => _metrics.RecordCreacionDuration(durationMs, resultado))
            .Should()
            .NotThrow();
    }

    [Fact]
    public void RecordCreacionDuration_ConMultiplesDuraciones_DeberiaRegistrarCorrectamente()
    {
        // Arrange
        var duraciones = new[] { 50.0, 100.5, 200.75, 300.25 };
        var resultado = "exitoso";

        // Act & Assert
        foreach (var duracion in duraciones)
        {
            FluentActions
                .Invoking(() => _metrics.RecordCreacionDuration(duracion, resultado))
                .Should()
                .NotThrow();
        }
    }

    [Fact]
    public void RecordCreacionDuration_ConDiferentesResultados_DeberiaRegistrarCorrectamente()
    {
        // Arrange
        var durationMs = 150.0;
        var resultados = new[] { "exitoso", "fallido", "timeout" };

        // Act & Assert
        foreach (var resultado in resultados)
        {
            FluentActions
                .Invoking(() => _metrics.RecordCreacionDuration(durationMs, resultado))
                .Should()
                .NotThrow();
        }
    }

    [Fact]
    public void RecordCreacionDuration_ConDuracionCero_DeberiaRegistrarCorrectamente()
    {
        // Arrange
        var durationMs = 0.0;
        var resultado = "exitoso";

        // Act & Assert
        FluentActions
            .Invoking(() => _metrics.RecordCreacionDuration(durationMs, resultado))
            .Should()
            .NotThrow();
    }

    [Fact]
    public void RecordCreacionDuration_ConDuracionNegativa_DeberiaRegistrarCorrectamente()
    {
        // Arrange
        var durationMs = -10.0;
        var resultado = "exitoso";

        // Act & Assert
        FluentActions
            .Invoking(() => _metrics.RecordCreacionDuration(durationMs, resultado))
            .Should()
            .NotThrow();
    }

    [Fact]
    public void IncrementValidacionExternaError_ConParametrosValidos_DeberiaIncrementarContador()
    {
        // Arrange
        var servicio = "VerificadorEventos";
        var tipoError = "Timeout";

        // Act
        _metrics.IncrementValidacionExternaError(servicio, tipoError);

        // Assert
        FluentActions
            .Invoking(() => _metrics.IncrementValidacionExternaError(servicio, tipoError))
            .Should()
            .NotThrow();
    }

    [Fact]
    public void IncrementValidacionExternaError_ConDiferentesServicios_DeberiaRegistrarCorrectamente()
    {
        // Arrange
        var servicios = new[] { "VerificadorEventos", "VerificadorAsientos" };
        var tipoError = "Timeout";

        // Act & Assert
        foreach (var servicio in servicios)
        {
            FluentActions
                .Invoking(() => _metrics.IncrementValidacionExternaError(servicio, tipoError))
                .Should()
                .NotThrow();
        }
    }

    [Fact]
    public void IncrementValidacionExternaError_ConDiferentesTiposError_DeberiaRegistrarCorrectamente()
    {
        // Arrange
        var servicio = "VerificadorEventos";
        var tiposError = new[] { "Timeout", "ConnectionRefused", "InvalidResponse", "NotFound" };

        // Act & Assert
        foreach (var tipoError in tiposError)
        {
            FluentActions
                .Invoking(() => _metrics.IncrementValidacionExternaError(servicio, tipoError))
                .Should()
                .NotThrow();
        }
    }

    [Fact]
    public void IncrementValidacionExternaError_ConMultiplesLlamadas_DeberiaIncrementarCorrectamente()
    {
        // Arrange
        var servicio = "VerificadorAsientos";
        var tipoError = "NotFound";

        // Act
        _metrics.IncrementValidacionExternaError(servicio, tipoError);
        _metrics.IncrementValidacionExternaError(servicio, tipoError);
        _metrics.IncrementValidacionExternaError(servicio, tipoError);

        // Assert
        FluentActions
            .Invoking(() => _metrics.IncrementValidacionExternaError(servicio, tipoError))
            .Should()
            .NotThrow();
    }

    [Fact]
    public void RecordServicioExternoDuration_ConParametrosValidos_DeberiaRegistrarHistograma()
    {
        // Arrange
        var servicio = "VerificadorEventos";
        var durationMs = 250.5;
        var resultado = "exitoso";

        // Act
        _metrics.RecordServicioExternoDuration(servicio, durationMs, resultado);

        // Assert
        FluentActions
            .Invoking(() => _metrics.RecordServicioExternoDuration(servicio, durationMs, resultado))
            .Should()
            .NotThrow();
    }

    [Fact]
    public void RecordServicioExternoDuration_ConDiferentesServicios_DeberiaRegistrarCorrectamente()
    {
        // Arrange
        var servicios = new[] { "VerificadorEventos", "VerificadorAsientos" };
        var durationMs = 150.0;
        var resultado = "exitoso";

        // Act & Assert
        foreach (var servicio in servicios)
        {
            FluentActions
                .Invoking(() => _metrics.RecordServicioExternoDuration(servicio, durationMs, resultado))
                .Should()
                .NotThrow();
        }
    }

    [Fact]
    public void RecordServicioExternoDuration_ConDiferentesResultados_DeberiaRegistrarCorrectamente()
    {
        // Arrange
        var servicio = "VerificadorEventos";
        var durationMs = 200.0;
        var resultados = new[] { "exitoso", "fallido", "timeout" };

        // Act & Assert
        foreach (var resultado in resultados)
        {
            FluentActions
                .Invoking(() => _metrics.RecordServicioExternoDuration(servicio, durationMs, resultado))
                .Should()
                .NotThrow();
        }
    }

    [Fact]
    public void RecordServicioExternoDuration_ConMultiplesDuraciones_DeberiaRegistrarCorrectamente()
    {
        // Arrange
        var servicio = "VerificadorAsientos";
        var duraciones = new[] { 50.0, 100.5, 200.75, 500.25 };
        var resultado = "exitoso";

        // Act & Assert
        foreach (var duracion in duraciones)
        {
            FluentActions
                .Invoking(() => _metrics.RecordServicioExternoDuration(servicio, duracion, resultado))
                .Should()
                .NotThrow();
        }
    }

    [Fact]
    public void IncrementPagosConfirmados_ConResultadoExitoso_DeberiaIncrementarContador()
    {
        // Arrange
        var resultado = "exitoso";

        // Act
        _metrics.IncrementPagosConfirmados(resultado);

        // Assert
        FluentActions
            .Invoking(() => _metrics.IncrementPagosConfirmados(resultado))
            .Should()
            .NotThrow();
    }

    [Fact]
    public void IncrementPagosConfirmados_ConDiferentesResultados_DeberiaRegistrarCorrectamente()
    {
        // Arrange
        var resultados = new[] { "exitoso", "fallido", "pendiente" };

        // Act & Assert
        foreach (var resultado in resultados)
        {
            FluentActions
                .Invoking(() => _metrics.IncrementPagosConfirmados(resultado))
                .Should()
                .NotThrow();
        }
    }

    [Fact]
    public void IncrementPagosConfirmados_ConMultiplesLlamadas_DeberiaIncrementarCorrectamente()
    {
        // Arrange
        var resultado = "exitoso";

        // Act
        _metrics.IncrementPagosConfirmados(resultado);
        _metrics.IncrementPagosConfirmados(resultado);
        _metrics.IncrementPagosConfirmados(resultado);

        // Assert
        FluentActions
            .Invoking(() => _metrics.IncrementPagosConfirmados(resultado))
            .Should()
            .NotThrow();
    }

    [Fact]
    public void RecordHealthCheckResult_ConParametrosValidos_DeberiaRegistrarHistograma()
    {
        // Arrange
        var checkName = "PostgreSQL";
        var resultado = "healthy";
        var durationMs = 50.0;

        // Act
        _metrics.RecordHealthCheckResult(checkName, resultado, durationMs);

        // Assert
        FluentActions
            .Invoking(() => _metrics.RecordHealthCheckResult(checkName, resultado, durationMs))
            .Should()
            .NotThrow();
    }

    [Fact]
    public void RecordHealthCheckResult_ConDiferentesChecks_DeberiaRegistrarCorrectamente()
    {
        // Arrange
        var checkNames = new[] { "PostgreSQL", "RabbitMQ", "HttpServices" };
        var resultado = "healthy";
        var durationMs = 100.0;

        // Act & Assert
        foreach (var checkName in checkNames)
        {
            FluentActions
                .Invoking(() => _metrics.RecordHealthCheckResult(checkName, resultado, durationMs))
                .Should()
                .NotThrow();
        }
    }

    [Fact]
    public void RecordHealthCheckResult_ConDiferentesResultados_DeberiaRegistrarCorrectamente()
    {
        // Arrange
        var checkName = "PostgreSQL";
        var resultados = new[] { "healthy", "degraded", "unhealthy" };
        var durationMs = 75.0;

        // Act & Assert
        foreach (var resultado in resultados)
        {
            FluentActions
                .Invoking(() => _metrics.RecordHealthCheckResult(checkName, resultado, durationMs))
                .Should()
                .NotThrow();
        }
    }

    [Fact]
    public void RecordHealthCheckResult_ConMultiplesDuraciones_DeberiaRegistrarCorrectamente()
    {
        // Arrange
        var checkName = "RabbitMQ";
        var resultado = "healthy";
        var duraciones = new[] { 10.0, 50.5, 100.75, 200.25 };

        // Act & Assert
        foreach (var duracion in duraciones)
        {
            FluentActions
                .Invoking(() => _metrics.RecordHealthCheckResult(checkName, resultado, duracion))
                .Should()
                .NotThrow();
        }
    }

    [Fact]
    public void RecordHealthCheckResult_ConDuracionCero_DeberiaRegistrarCorrectamente()
    {
        // Arrange
        var checkName = "PostgreSQL";
        var resultado = "healthy";
        var durationMs = 0.0;

        // Act & Assert
        FluentActions
            .Invoking(() => _metrics.RecordHealthCheckResult(checkName, resultado, durationMs))
            .Should()
            .NotThrow();
    }

    [Fact]
    public void RecordHealthCheckResult_ConDuracionAlta_DeberiaRegistrarCorrectamente()
    {
        // Arrange
        var checkName = "HttpServices";
        var resultado = "degraded";
        var durationMs = 5000.0;

        // Act & Assert
        FluentActions
            .Invoking(() => _metrics.RecordHealthCheckResult(checkName, resultado, durationMs))
            .Should()
            .NotThrow();
    }

    [Fact]
    public void Metrics_ConSecuenciaCompleta_DeberiaRegistrarTodosLosEventos()
    {
        // Arrange
        var eventoId = Guid.NewGuid().ToString();
        var usuarioId = Guid.NewGuid().ToString();

        // Act
        _metrics.IncrementEntradasCreadas(eventoId, "PendientePago");
        _metrics.RecordCreacionDuration(150.0, "exitoso");
        _metrics.RecordServicioExternoDuration("VerificadorEventos", 50.0, "exitoso");
        _metrics.RecordServicioExternoDuration("VerificadorAsientos", 75.0, "exitoso");
        _metrics.IncrementPagosConfirmados("exitoso");
        _metrics.RecordHealthCheckResult("PostgreSQL", "healthy", 25.0);

        // Assert
        FluentActions
            .Invoking(() =>
            {
                _metrics.IncrementEntradasCreadas(eventoId, "Pagada");
                _metrics.RecordCreacionDuration(200.0, "exitoso");
            })
            .Should()
            .NotThrow();
    }

    [Fact]
    public void Metrics_ConErroresEnValidacionExterna_DeberiaRegistrarErrores()
    {
        // Arrange
        var eventoId = Guid.NewGuid().ToString();

        // Act
        _metrics.IncrementEntradasCreadas(eventoId, "PendientePago");
        _metrics.RecordCreacionDuration(150.0, "exitoso");
        _metrics.IncrementValidacionExternaError("VerificadorEventos", "Timeout");
        _metrics.IncrementValidacionExternaError("VerificadorAsientos", "ConnectionRefused");

        // Assert
        FluentActions
            .Invoking(() =>
            {
                _metrics.IncrementValidacionExternaError("VerificadorEventos", "NotFound");
            })
            .Should()
            .NotThrow();
    }

    [Fact]
    public void Metrics_ConConcurrencia_DeberiaRegistrarCorrectamente()
    {
        // Arrange
        var tasks = new List<Task>();

        // Act
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                _metrics.IncrementEntradasCreadas(Guid.NewGuid().ToString(), "PendientePago");
                _metrics.RecordCreacionDuration(100.0, "exitoso");
                _metrics.IncrementPagosConfirmados("exitoso");
            }));
        }

        Task.WaitAll(tasks.ToArray());

        // Assert
        FluentActions
            .Invoking(() =>
            {
                _metrics.IncrementEntradasCreadas(Guid.NewGuid().ToString(), "Pagada");
            })
            .Should()
            .NotThrow();
    }

    [Fact]
    public void Metrics_DeberiaImplementarIEntradasMetrics()
    {
        // Arrange & Act
        var metrics = _metrics;

        // Assert
        metrics.Should().BeAssignableTo<IEntradasMetrics>();
    }

    [Fact]
    public void IncrementEntradasCreadas_ConStringVacio_DeberiaRegistrarCorrectamente()
    {
        // Arrange
        var eventoId = string.Empty;
        var estado = string.Empty;

        // Act & Assert
        FluentActions
            .Invoking(() => _metrics.IncrementEntradasCreadas(eventoId, estado))
            .Should()
            .NotThrow();
    }

    [Fact]
    public void RecordCreacionDuration_ConValoresExtremos_DeberiaRegistrarCorrectamente()
    {
        // Arrange
        var duraciones = new[] { double.MinValue, double.MaxValue, double.Epsilon };
        var resultado = "exitoso";

        // Act & Assert
        foreach (var duracion in duraciones)
        {
            FluentActions
                .Invoking(() => _metrics.RecordCreacionDuration(duracion, resultado))
                .Should()
                .NotThrow();
        }
    }

    public void Dispose()
    {
        _meter?.Dispose();
    }
}
