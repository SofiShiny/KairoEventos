using Entradas.Aplicacion.DTOs;
using Entradas.Dominio.Enums;
using FluentAssertions;
using Xunit;

namespace Entradas.Pruebas.Aplicacion.DTOs;

public class EntradaDtoTests
{
    [Fact]
    public void EntradaDto_Constructor_DebeAsignarPropiedadesCorrectamente()
    {
        // Arrange
        var id = Guid.NewGuid();
        var eventoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var asientoId = Guid.NewGuid();
        var monto = 100.50m;
        var codigoQr = "QR123456";
        var estado = EstadoEntrada.Pagada;
        var fechaCompra = DateTime.UtcNow;

        // Act
        var dto = new EntradaDto(id, eventoId, usuarioId, asientoId, monto, codigoQr, estado, fechaCompra);

        // Assert
        dto.Id.Should().Be(id);
        dto.EventoId.Should().Be(eventoId);
        dto.UsuarioId.Should().Be(usuarioId);
        dto.AsientoId.Should().Be(asientoId);
        dto.Monto.Should().Be(monto);
        dto.CodigoQr.Should().Be(codigoQr);
        dto.Estado.Should().Be(estado);
        dto.FechaCompra.Should().Be(fechaCompra);
    }

    [Fact]
    public void EntradaCreadaDto_Constructor_DebeAsignarPropiedadesCorrectamente()
    {
        // Arrange
        var id = Guid.NewGuid();
        var eventoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var asientoId = Guid.NewGuid();
        var monto = 100.50m;
        var codigoQr = "QR123456";
        var estado = EstadoEntrada.PendientePago;
        var fechaCompra = DateTime.UtcNow;

        // Act
        var dto = new EntradaCreadaDto(id, eventoId, usuarioId, asientoId, monto, codigoQr, estado, fechaCompra);

        // Assert
        dto.Id.Should().Be(id);
        dto.EventoId.Should().Be(eventoId);
        dto.UsuarioId.Should().Be(usuarioId);
        dto.AsientoId.Should().Be(asientoId);
        dto.Monto.Should().Be(monto);
        dto.CodigoQr.Should().Be(codigoQr);
        dto.Estado.Should().Be(estado);
        dto.FechaCompra.Should().Be(fechaCompra);
    }
}