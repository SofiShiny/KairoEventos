using Reportes.API.DTOs;
using Xunit;

namespace Reportes.Pruebas.API.DTOs;

/// <summary>
/// Unit Tests para AsistenciaEventoDto
/// Valida propiedades y comportamiento del DTO
/// </summary>
public class AsistenciaEventoDtoTests
{
    [Fact]
    public void AsistenciaEventoDto_PropiedadesPorDefecto_ValoresCorrectos()
    {
        // Act
        var dto = new AsistenciaEventoDto();

        // Assert
        Assert.Equal(Guid.Empty, dto.EventoId);
        Assert.Equal(string.Empty, dto.TituloEvento);
        Assert.Equal(0, dto.TotalAsistentes);
        Assert.Equal(0, dto.AsientosReservados);
        Assert.Equal(0, dto.AsientosDisponibles);
        Assert.Equal(0, dto.CapacidadTotal);
        Assert.Equal(0.0, dto.PorcentajeOcupacion);
        Assert.Equal(default(DateTime), dto.UltimaActualizacion);
    }

    [Fact]
    public void AsistenciaEventoDto_AsignarPropiedades_ValoresCorrectos()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var titulo = "Evento Test";
        var totalAsistentes = 100;
        var asientosReservados = 75;
        var asientosDisponibles = 25;
        var capacidadTotal = 100;
        var porcentajeOcupacion = 75.0;
        var ultimaActualizacion = DateTime.UtcNow;

        // Act
        var dto = new AsistenciaEventoDto
        {
            EventoId = eventoId,
            TituloEvento = titulo,
            TotalAsistentes = totalAsistentes,
            AsientosReservados = asientosReservados,
            AsientosDisponibles = asientosDisponibles,
            CapacidadTotal = capacidadTotal,
            PorcentajeOcupacion = porcentajeOcupacion,
            UltimaActualizacion = ultimaActualizacion
        };

        // Assert
        Assert.Equal(eventoId, dto.EventoId);
        Assert.Equal(titulo, dto.TituloEvento);
        Assert.Equal(totalAsistentes, dto.TotalAsistentes);
        Assert.Equal(asientosReservados, dto.AsientosReservados);
        Assert.Equal(asientosDisponibles, dto.AsientosDisponibles);
        Assert.Equal(capacidadTotal, dto.CapacidadTotal);
        Assert.Equal(porcentajeOcupacion, dto.PorcentajeOcupacion);
        Assert.Equal(ultimaActualizacion, dto.UltimaActualizacion);
    }

    [Fact]
    public void AsistenciaEventoDto_ValoresNegativos_PermiteAsignacion()
    {
        // Arrange & Act
        var dto = new AsistenciaEventoDto
        {
            TotalAsistentes = -1,
            AsientosReservados = -5,
            AsientosDisponibles = -10,
            CapacidadTotal = -100,
            PorcentajeOcupacion = -50.0
        };

        // Assert
        Assert.Equal(-1, dto.TotalAsistentes);
        Assert.Equal(-5, dto.AsientosReservados);
        Assert.Equal(-10, dto.AsientosDisponibles);
        Assert.Equal(-100, dto.CapacidadTotal);
        Assert.Equal(-50.0, dto.PorcentajeOcupacion);
    }

    [Fact]
    public void AsistenciaEventoDto_ValoresMaximos_PermiteAsignacion()
    {
        // Arrange & Act
        var dto = new AsistenciaEventoDto
        {
            TotalAsistentes = int.MaxValue,
            AsientosReservados = int.MaxValue,
            AsientosDisponibles = int.MaxValue,
            CapacidadTotal = int.MaxValue,
            PorcentajeOcupacion = double.MaxValue
        };

        // Assert
        Assert.Equal(int.MaxValue, dto.TotalAsistentes);
        Assert.Equal(int.MaxValue, dto.AsientosReservados);
        Assert.Equal(int.MaxValue, dto.AsientosDisponibles);
        Assert.Equal(int.MaxValue, dto.CapacidadTotal);
        Assert.Equal(double.MaxValue, dto.PorcentajeOcupacion);
    }

    [Fact]
    public void AsistenciaEventoDto_TituloEventoNull_PermiteAsignacion()
    {
        // Arrange & Act
        var dto = new AsistenciaEventoDto
        {
            TituloEvento = null!
        };

        // Assert
        Assert.Null(dto.TituloEvento);
    }

    [Fact]
    public void AsistenciaEventoDto_PorcentajeOcupacionDecimal_PermiteAsignacion()
    {
        // Arrange & Act
        var dto = new AsistenciaEventoDto
        {
            PorcentajeOcupacion = 75.5555
        };

        // Assert
        Assert.Equal(75.5555, dto.PorcentajeOcupacion);
    }
}