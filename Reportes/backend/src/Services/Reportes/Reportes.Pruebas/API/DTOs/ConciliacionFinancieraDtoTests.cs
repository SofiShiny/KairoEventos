using Reportes.API.DTOs;
using Xunit;

namespace Reportes.Pruebas.API.DTOs;

/// <summary>
/// Unit Tests para ConciliacionFinancieraDto y TransaccionDto
/// Valida propiedades y comportamiento de los DTOs
/// </summary>
public class ConciliacionFinancieraDtoTests
{
    #region ConciliacionFinancieraDto Tests

    [Fact]
    public void ConciliacionFinancieraDto_PropiedadesPorDefecto_ValoresCorrectos()
    {
        // Act
        var dto = new ConciliacionFinancieraDto();

        // Assert
        Assert.Equal(0m, dto.TotalIngresos);
        Assert.Equal(0, dto.CantidadTransacciones);
        Assert.NotNull(dto.DesglosePorCategoria);
        Assert.Empty(dto.DesglosePorCategoria);
        Assert.Equal(default(DateTime), dto.FechaInicio);
        Assert.Equal(default(DateTime), dto.FechaFin);
        Assert.NotNull(dto.Transacciones);
        Assert.Empty(dto.Transacciones);
    }

    [Fact]
    public void ConciliacionFinancieraDto_AsignarPropiedades_ValoresCorrectos()
    {
        // Arrange
        var totalIngresos = 15000.50m;
        var cantidadTransacciones = 25;
        var desglose = new Dictionary<string, decimal>
        {
            { "VIP", 8000m },
            { "General", 7000.50m }
        };
        var fechaInicio = DateTime.UtcNow.AddDays(-30);
        var fechaFin = DateTime.UtcNow;
        var transacciones = new List<TransaccionDto>
        {
            new TransaccionDto
            {
                EventoId = Guid.NewGuid(),
                TituloEvento = "Evento 1",
                Fecha = fechaInicio,
                CantidadReservas = 10,
                Monto = 5000m
            }
        };

        // Act
        var dto = new ConciliacionFinancieraDto
        {
            TotalIngresos = totalIngresos,
            CantidadTransacciones = cantidadTransacciones,
            DesglosePorCategoria = desglose,
            FechaInicio = fechaInicio,
            FechaFin = fechaFin,
            Transacciones = transacciones
        };

        // Assert
        Assert.Equal(totalIngresos, dto.TotalIngresos);
        Assert.Equal(cantidadTransacciones, dto.CantidadTransacciones);
        Assert.Equal(desglose, dto.DesglosePorCategoria);
        Assert.Equal(fechaInicio, dto.FechaInicio);
        Assert.Equal(fechaFin, dto.FechaFin);
        Assert.Equal(transacciones, dto.Transacciones);
        Assert.Single(dto.Transacciones);
    }

    [Fact]
    public void ConciliacionFinancieraDto_ValoresNegativos_PermiteAsignacion()
    {
        // Arrange & Act
        var dto = new ConciliacionFinancieraDto
        {
            TotalIngresos = -1000m,
            CantidadTransacciones = -5
        };

        // Assert
        Assert.Equal(-1000m, dto.TotalIngresos);
        Assert.Equal(-5, dto.CantidadTransacciones);
    }

    [Fact]
    public void ConciliacionFinancieraDto_ValoresMaximos_PermiteAsignacion()
    {
        // Arrange & Act
        var dto = new ConciliacionFinancieraDto
        {
            TotalIngresos = decimal.MaxValue,
            CantidadTransacciones = int.MaxValue
        };

        // Assert
        Assert.Equal(decimal.MaxValue, dto.TotalIngresos);
        Assert.Equal(int.MaxValue, dto.CantidadTransacciones);
    }

    [Fact]
    public void ConciliacionFinancieraDto_DesglosePorCategoriaVacio_PermiteAsignacion()
    {
        // Arrange & Act
        var dto = new ConciliacionFinancieraDto
        {
            DesglosePorCategoria = new Dictionary<string, decimal>()
        };

        // Assert
        Assert.NotNull(dto.DesglosePorCategoria);
        Assert.Empty(dto.DesglosePorCategoria);
    }

    [Fact]
    public void ConciliacionFinancieraDto_TransaccionesVacia_PermiteAsignacion()
    {
        // Arrange & Act
        var dto = new ConciliacionFinancieraDto
        {
            Transacciones = new List<TransaccionDto>()
        };

        // Assert
        Assert.NotNull(dto.Transacciones);
        Assert.Empty(dto.Transacciones);
    }

    #endregion

    #region TransaccionDto Tests

    [Fact]
    public void TransaccionDto_PropiedadesPorDefecto_ValoresCorrectos()
    {
        // Act
        var dto = new TransaccionDto();

        // Assert
        Assert.Equal(Guid.Empty, dto.EventoId);
        Assert.Equal(string.Empty, dto.TituloEvento);
        Assert.Equal(default(DateTime), dto.Fecha);
        Assert.Equal(0, dto.CantidadReservas);
        Assert.Equal(0m, dto.Monto);
    }

    [Fact]
    public void TransaccionDto_AsignarPropiedades_ValoresCorrectos()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var tituloEvento = "Concierto Rock";
        var fecha = DateTime.UtcNow;
        var cantidadReservas = 50;
        var monto = 2500.75m;

        // Act
        var dto = new TransaccionDto
        {
            EventoId = eventoId,
            TituloEvento = tituloEvento,
            Fecha = fecha,
            CantidadReservas = cantidadReservas,
            Monto = monto
        };

        // Assert
        Assert.Equal(eventoId, dto.EventoId);
        Assert.Equal(tituloEvento, dto.TituloEvento);
        Assert.Equal(fecha, dto.Fecha);
        Assert.Equal(cantidadReservas, dto.CantidadReservas);
        Assert.Equal(monto, dto.Monto);
    }

    [Fact]
    public void TransaccionDto_ValoresNegativos_PermiteAsignacion()
    {
        // Arrange & Act
        var dto = new TransaccionDto
        {
            CantidadReservas = -10,
            Monto = -500.50m
        };

        // Assert
        Assert.Equal(-10, dto.CantidadReservas);
        Assert.Equal(-500.50m, dto.Monto);
    }

    [Fact]
    public void TransaccionDto_ValoresMaximos_PermiteAsignacion()
    {
        // Arrange & Act
        var dto = new TransaccionDto
        {
            CantidadReservas = int.MaxValue,
            Monto = decimal.MaxValue
        };

        // Assert
        Assert.Equal(int.MaxValue, dto.CantidadReservas);
        Assert.Equal(decimal.MaxValue, dto.Monto);
    }

    [Fact]
    public void TransaccionDto_TituloEventoNull_PermiteAsignacion()
    {
        // Arrange & Act
        var dto = new TransaccionDto
        {
            TituloEvento = null!
        };

        // Assert
        Assert.Null(dto.TituloEvento);
    }

    [Fact]
    public void TransaccionDto_FechaMinMax_PermiteAsignacion()
    {
        // Arrange & Act
        var dtoMin = new TransaccionDto { Fecha = DateTime.MinValue };
        var dtoMax = new TransaccionDto { Fecha = DateTime.MaxValue };

        // Assert
        Assert.Equal(DateTime.MinValue, dtoMin.Fecha);
        Assert.Equal(DateTime.MaxValue, dtoMax.Fecha);
    }

    [Fact]
    public void TransaccionDto_MontoDecimal_PermiteAsignacion()
    {
        // Arrange & Act
        var dto = new TransaccionDto
        {
            Monto = 1234.5678m
        };

        // Assert
        Assert.Equal(1234.5678m, dto.Monto);
    }

    #endregion
}