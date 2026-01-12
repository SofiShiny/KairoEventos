using Reportes.API.DTOs;
using Xunit;

namespace Reportes.Pruebas.API.DTOs;

/// <summary>
/// Unit Tests para LogAuditoriaDto
/// Valida propiedades y comportamiento del DTO
/// </summary>
public class LogAuditoriaDtoTests
{
    [Fact]
    public void LogAuditoriaDto_PropiedadesPorDefecto_ValoresCorrectos()
    {
        // Act
        var dto = new LogAuditoriaDto();

        // Assert
        Assert.Equal(string.Empty, dto.Id);
        Assert.Equal(default(DateTime), dto.Timestamp);
        Assert.Equal(string.Empty, dto.TipoOperacion);
        Assert.Equal(string.Empty, dto.Entidad);
        Assert.Equal(string.Empty, dto.EntidadId);
        Assert.Equal(string.Empty, dto.Detalles);
        Assert.False(dto.Exitoso);
        Assert.Null(dto.MensajeError);
    }

    [Fact]
    public void LogAuditoriaDto_AsignarPropiedades_ValoresCorrectos()
    {
        // Arrange
        var id = Guid.NewGuid().ToString();
        var timestamp = DateTime.UtcNow;
        var tipoOperacion = "EventoConsumido";
        var entidad = "Evento";
        var entidadId = Guid.NewGuid().ToString();
        var detalles = "Evento procesado correctamente";
        var exitoso = true;
        var mensajeError = "Sin errores";

        // Act
        var dto = new LogAuditoriaDto
        {
            Id = id,
            Timestamp = timestamp,
            TipoOperacion = tipoOperacion,
            Entidad = entidad,
            EntidadId = entidadId,
            Detalles = detalles,
            Exitoso = exitoso,
            MensajeError = mensajeError
        };

        // Assert
        Assert.Equal(id, dto.Id);
        Assert.Equal(timestamp, dto.Timestamp);
        Assert.Equal(tipoOperacion, dto.TipoOperacion);
        Assert.Equal(entidad, dto.Entidad);
        Assert.Equal(entidadId, dto.EntidadId);
        Assert.Equal(detalles, dto.Detalles);
        Assert.Equal(exitoso, dto.Exitoso);
        Assert.Equal(mensajeError, dto.MensajeError);
    }

    [Fact]
    public void LogAuditoriaDto_PropiedadesNull_PermiteAsignacion()
    {
        // Arrange & Act
        var dto = new LogAuditoriaDto
        {
            Id = null!,
            TipoOperacion = null!,
            Entidad = null!,
            EntidadId = null!,
            Detalles = null!,
            MensajeError = null
        };

        // Assert
        Assert.Null(dto.Id);
        Assert.Null(dto.TipoOperacion);
        Assert.Null(dto.Entidad);
        Assert.Null(dto.EntidadId);
        Assert.Null(dto.Detalles);
        Assert.Null(dto.MensajeError);
    }

    [Fact]
    public void LogAuditoriaDto_ExitosoFalse_ConMensajeError()
    {
        // Arrange & Act
        var dto = new LogAuditoriaDto
        {
            Exitoso = false,
            MensajeError = "Error al procesar evento"
        };

        // Assert
        Assert.False(dto.Exitoso);
        Assert.Equal("Error al procesar evento", dto.MensajeError);
    }

    [Fact]
    public void LogAuditoriaDto_ExitosoTrue_SinMensajeError()
    {
        // Arrange & Act
        var dto = new LogAuditoriaDto
        {
            Exitoso = true,
            MensajeError = null
        };

        // Assert
        Assert.True(dto.Exitoso);
        Assert.Null(dto.MensajeError);
    }

    [Fact]
    public void LogAuditoriaDto_CadenasVacias_PermiteAsignacion()
    {
        // Arrange & Act
        var dto = new LogAuditoriaDto
        {
            Id = "",
            TipoOperacion = "",
            Entidad = "",
            EntidadId = "",
            Detalles = "",
            MensajeError = ""
        };

        // Assert
        Assert.Equal("", dto.Id);
        Assert.Equal("", dto.TipoOperacion);
        Assert.Equal("", dto.Entidad);
        Assert.Equal("", dto.EntidadId);
        Assert.Equal("", dto.Detalles);
        Assert.Equal("", dto.MensajeError);
    }

    [Fact]
    public void LogAuditoriaDto_TimestampMinMax_PermiteAsignacion()
    {
        // Arrange & Act
        var dtoMin = new LogAuditoriaDto { Timestamp = DateTime.MinValue };
        var dtoMax = new LogAuditoriaDto { Timestamp = DateTime.MaxValue };

        // Assert
        Assert.Equal(DateTime.MinValue, dtoMin.Timestamp);
        Assert.Equal(DateTime.MaxValue, dtoMax.Timestamp);
    }
}