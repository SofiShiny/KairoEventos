using FluentAssertions;
using Reportes.API.DTOs;
using Xunit;

namespace Reportes.Pruebas.API.DTOs;

public class MetricasEventoDtoTests
{
    [Fact]
    public void MetricasEventoDto_Constructor_InicializaPropiedadesCorrectamente()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var tituloEvento = "Concierto de Rock";
        var totalAsistentes = 150;
        var estado = "Activo";
        var fechaPublicacion = DateTime.Now;
        var ultimaActualizacion = DateTime.Now;

        // Act
        var dto = new MetricasEventoDto
        {
            EventoId = eventoId,
            TituloEvento = tituloEvento,
            TotalAsistentes = totalAsistentes,
            Estado = estado,
            FechaPublicacion = fechaPublicacion,
            UltimaActualizacion = ultimaActualizacion
        };

        // Assert
        dto.EventoId.Should().Be(eventoId);
        dto.TituloEvento.Should().Be(tituloEvento);
        dto.TotalAsistentes.Should().Be(totalAsistentes);
        dto.Estado.Should().Be(estado);
        dto.FechaPublicacion.Should().Be(fechaPublicacion);
        dto.UltimaActualizacion.Should().Be(ultimaActualizacion);
    }

    [Fact]
    public void MetricasEventoDto_PropiedadesNulas_PermiteValoresNulos()
    {
        // Act
        var dto = new MetricasEventoDto
        {
            EventoId = Guid.Empty,
            TituloEvento = null!,
            TotalAsistentes = 0,
            Estado = null!,
            FechaPublicacion = null,
            UltimaActualizacion = default
        };

        // Assert
        dto.EventoId.Should().Be(Guid.Empty);
        dto.TituloEvento.Should().BeNull();
        dto.TotalAsistentes.Should().Be(0);
        dto.Estado.Should().BeNull();
        dto.FechaPublicacion.Should().BeNull();
        dto.UltimaActualizacion.Should().Be(default);
    }

    [Fact]
    public void MetricasEventoDto_ValoresExtremos_ManejaCorrectamente()
    {
        // Arrange
        var dto = new MetricasEventoDto();

        // Act & Assert - Valores máximos
        dto.TotalAsistentes = int.MaxValue;
        dto.TotalAsistentes.Should().Be(int.MaxValue);

        // Act & Assert - Valores mínimos
        dto.TotalAsistentes = int.MinValue;
        dto.TotalAsistentes.Should().Be(int.MinValue);
    }

    [Fact]
    public void MetricasEventoDto_Igualdad_ComparaCorrectamente()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var fechaPublicacion = DateTime.Now;
        var ultimaActualizacion = DateTime.Now;
        
        var dto1 = new MetricasEventoDto
        {
            EventoId = eventoId,
            TituloEvento = "Evento Test",
            TotalAsistentes = 100,
            Estado = "Activo",
            FechaPublicacion = fechaPublicacion,
            UltimaActualizacion = ultimaActualizacion
        };

        var dto2 = new MetricasEventoDto
        {
            EventoId = eventoId,
            TituloEvento = "Evento Test",
            TotalAsistentes = 100,
            Estado = "Activo",
            FechaPublicacion = fechaPublicacion,
            UltimaActualizacion = ultimaActualizacion
        };

        // Act & Assert
        dto1.EventoId.Should().Be(dto2.EventoId);
        dto1.TituloEvento.Should().Be(dto2.TituloEvento);
        dto1.TotalAsistentes.Should().Be(dto2.TotalAsistentes);
        dto1.Estado.Should().Be(dto2.Estado);
        dto1.FechaPublicacion.Should().Be(dto2.FechaPublicacion);
        dto1.UltimaActualizacion.Should().Be(dto2.UltimaActualizacion);
    }

    [Theory]
    [InlineData("Activo")]
    [InlineData("Cancelado")]
    [InlineData("Finalizado")]
    [InlineData("Pendiente")]
    public void MetricasEventoDto_Estado_AceptaDiferentesValores(string estado)
    {
        // Act
        var dto = new MetricasEventoDto { Estado = estado };

        // Assert
        dto.Estado.Should().Be(estado);
    }

    [Fact]
    public void MetricasEventoDto_FechaPublicacion_ManejaFechasHistoricas()
    {
        // Arrange
        var fechaHistorica = new DateTime(2020, 1, 1);
        var fechaFutura = new DateTime(2030, 12, 31);

        // Act
        var dtoHistorico = new MetricasEventoDto { FechaPublicacion = fechaHistorica };
        var dtoFuturo = new MetricasEventoDto { FechaPublicacion = fechaFutura };

        // Assert
        dtoHistorico.FechaPublicacion.Should().Be(fechaHistorica);
        dtoFuturo.FechaPublicacion.Should().Be(fechaFutura);
    }

    [Fact]
    public void MetricasEventoDto_PropiedadesDefault_TienenValoresEsperados()
    {
        // Act
        var dto = new MetricasEventoDto();

        // Assert
        dto.EventoId.Should().Be(Guid.Empty);
        dto.TituloEvento.Should().Be(string.Empty);
        dto.TotalAsistentes.Should().Be(0);
        dto.Estado.Should().Be(string.Empty);
        dto.FechaPublicacion.Should().BeNull();
        dto.UltimaActualizacion.Should().Be(default);
    }
}