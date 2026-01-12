using FluentAssertions;
using Microsoft.Extensions.Options;
using Reportes.Infraestructura.Configuracion;
using Reportes.Infraestructura.Persistencia;
using Xunit;

namespace Reportes.Pruebas.Infraestructura.Persistencia;

public class ReportesMongoDbContextTests
{
    private readonly MongoDbSettings _settings;

    public ReportesMongoDbContextTests()
    {
        _settings = new MongoDbSettings
        {
            ConnectionString = "mongodb://localhost:27017",
            DatabaseName = "test_reportes_db",
            ReportesVentasDiariasCollection = "reportes_ventas_diarias",
            HistorialAsistenciaCollection = "historial_asistencia",
            MetricasEventoCollection = "metricas_evento",
            LogsAuditoriaCollection = "logs_auditoria",
            ReportesConsolidadosCollection = "reportes_consolidados"
        };
    }

    [Fact]
    public void MongoDbSettings_PropiedadesDefault_TienenValoresCorrectos()
    {
        // Act
        var settings = new MongoDbSettings();

        // Assert
        settings.ConnectionString.Should().Be("mongodb://localhost:27017");
        settings.DatabaseName.Should().Be("reportes_db");
        settings.ReportesVentasDiariasCollection.Should().Be("reportes_ventas_diarias");
        settings.HistorialAsistenciaCollection.Should().Be("historial_asistencia");
        settings.MetricasEventoCollection.Should().Be("metricas_evento");
        settings.LogsAuditoriaCollection.Should().Be("logs_auditoria");
        settings.ReportesConsolidadosCollection.Should().Be("reportes_consolidados");
    }

    [Fact]
    public void MongoDbSettings_AsignacionPropiedades_FuncionaCorrectamente()
    {
        // Act
        var settings = new MongoDbSettings
        {
            ConnectionString = "mongodb://test:27017",
            DatabaseName = "test_db",
            ReportesVentasDiariasCollection = "ventas",
            HistorialAsistenciaCollection = "historial",
            MetricasEventoCollection = "metricas",
            LogsAuditoriaCollection = "logs",
            ReportesConsolidadosCollection = "consolidados"
        };

        // Assert
        settings.ConnectionString.Should().Be("mongodb://test:27017");
        settings.DatabaseName.Should().Be("test_db");
        settings.ReportesVentasDiariasCollection.Should().Be("ventas");
        settings.HistorialAsistenciaCollection.Should().Be("historial");
        settings.MetricasEventoCollection.Should().Be("metricas");
        settings.LogsAuditoriaCollection.Should().Be("logs");
        settings.ReportesConsolidadosCollection.Should().Be("consolidados");
    }

    [Theory]
    [InlineData("mongodb://localhost:27017", "test_db")]
    [InlineData("mongodb://test-server:27017", "production_db")]
    [InlineData("mongodb://user:pass@server:27017", "secure_db")]
    public void MongoDbSettings_DiferentesConfiguraciones_SeAsignanCorrectamente(string connectionString, string databaseName)
    {
        // Act
        var settings = new MongoDbSettings
        {
            ConnectionString = connectionString,
            DatabaseName = databaseName
        };

        // Assert
        settings.ConnectionString.Should().Be(connectionString);
        settings.DatabaseName.Should().Be(databaseName);
    }

    [Fact]
    public void MongoDbSettings_PropiedadesNulas_PermiteValoresNulos()
    {
        // Act
        var settings = new MongoDbSettings
        {
            ConnectionString = null,
            DatabaseName = null,
            ReportesVentasDiariasCollection = null,
            HistorialAsistenciaCollection = null,
            MetricasEventoCollection = null,
            LogsAuditoriaCollection = null,
            ReportesConsolidadosCollection = null
        };

        // Assert
        settings.ConnectionString.Should().BeNull();
        settings.DatabaseName.Should().BeNull();
        settings.ReportesVentasDiariasCollection.Should().BeNull();
        settings.HistorialAsistenciaCollection.Should().BeNull();
        settings.MetricasEventoCollection.Should().BeNull();
        settings.LogsAuditoriaCollection.Should().BeNull();
        settings.ReportesConsolidadosCollection.Should().BeNull();
    }

    [Fact]
    public void MongoDbSettings_CadenasVacias_PermiteValoresVacios()
    {
        // Act
        var settings = new MongoDbSettings
        {
            ConnectionString = string.Empty,
            DatabaseName = string.Empty,
            ReportesVentasDiariasCollection = string.Empty,
            HistorialAsistenciaCollection = string.Empty,
            MetricasEventoCollection = string.Empty,
            LogsAuditoriaCollection = string.Empty,
            ReportesConsolidadosCollection = string.Empty
        };

        // Assert
        settings.ConnectionString.Should().Be(string.Empty);
        settings.DatabaseName.Should().Be(string.Empty);
        settings.ReportesVentasDiariasCollection.Should().Be(string.Empty);
        settings.HistorialAsistenciaCollection.Should().Be(string.Empty);
        settings.MetricasEventoCollection.Should().Be(string.Empty);
        settings.LogsAuditoriaCollection.Should().Be(string.Empty);
        settings.ReportesConsolidadosCollection.Should().Be(string.Empty);
    }

    [Fact]
    public void MongoDbSettings_NombresColeccionesLargos_ManejaCorrectamente()
    {
        // Arrange
        var nombreLargo = new string('a', 100);

        // Act
        var settings = new MongoDbSettings
        {
            ReportesVentasDiariasCollection = nombreLargo,
            HistorialAsistenciaCollection = nombreLargo,
            MetricasEventoCollection = nombreLargo,
            LogsAuditoriaCollection = nombreLargo,
            ReportesConsolidadosCollection = nombreLargo
        };

        // Assert
        settings.ReportesVentasDiariasCollection.Should().Be(nombreLargo);
        settings.HistorialAsistenciaCollection.Should().Be(nombreLargo);
        settings.MetricasEventoCollection.Should().Be(nombreLargo);
        settings.LogsAuditoriaCollection.Should().Be(nombreLargo);
        settings.ReportesConsolidadosCollection.Should().Be(nombreLargo);
    }

    [Fact]
    public void MongoDbSettings_CaracteresEspeciales_ManejaCorrectamente()
    {
        // Arrange
        var connectionStringConCaracteresEspeciales = "mongodb://user:p@ssw0rd!@server:27017/db?authSource=admin";
        var nombreConCaracteresEspeciales = "colección_con-caracteres.especiales";

        // Act
        var settings = new MongoDbSettings
        {
            ConnectionString = connectionStringConCaracteresEspeciales,
            DatabaseName = nombreConCaracteresEspeciales
        };

        // Assert
        settings.ConnectionString.Should().Be(connectionStringConCaracteresEspeciales);
        settings.DatabaseName.Should().Be(nombreConCaracteresEspeciales);
    }
}