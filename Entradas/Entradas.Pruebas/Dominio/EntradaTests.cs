using Entradas.Dominio.Entidades;
using Entradas.Dominio.Enums;
using Entradas.Dominio.Excepciones;
using FluentAssertions;
using Xunit;

namespace Entradas.Pruebas.Dominio;

public class EntradaTests
{
    [Fact]
    public void CrearEntrada_ConDatosValidos_DebeCrearCorrectamente()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var asientoId = Guid.NewGuid();
        var monto = 100.50m;
        var codigoQr = "QR123456";

        // Act
        var entrada = Entrada.Crear(eventoId, usuarioId, monto, asientoId, codigoQr);

        // Assert
        entrada.EventoId.Should().Be(eventoId);
        entrada.UsuarioId.Should().Be(usuarioId);
        entrada.AsientoId.Should().Be(asientoId);
        entrada.Monto.Should().Be(monto);
        entrada.Estado.Should().Be(EstadoEntrada.PendientePago);
        entrada.CodigoQr.Should().Be(codigoQr);
        entrada.FechaCompra.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void CrearEntrada_ConEventoIdVacio_DebeLanzarExcepcion()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var monto = 100.50m;
        var codigoQr = "QR123456";

        // Act & Assert
        var accion = () => Entrada.Crear(Guid.Empty, usuarioId, monto, null, codigoQr);
        accion.Should().Throw<ArgumentException>().WithMessage("*evento*");
    }

    [Fact]
    public void CrearEntrada_ConUsuarioIdVacio_DebeLanzarExcepcion()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var monto = 100.50m;
        var codigoQr = "QR123456";

        // Act & Assert
        var accion = () => Entrada.Crear(eventoId, Guid.Empty, monto, null, codigoQr);
        accion.Should().Throw<ArgumentException>().WithMessage("*usuario*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void CrearEntrada_ConMontoInvalido_DebeLanzarExcepcion(decimal montoInvalido)
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var codigoQr = "QR123456";

        // Act & Assert
        var accion = () => Entrada.Crear(eventoId, usuarioId, montoInvalido, null, codigoQr);
        accion.Should().Throw<ArgumentException>().WithMessage("*monto*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void CrearEntrada_ConCodigoQrInvalido_DebeLanzarExcepcion(string codigoQrInvalido)
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var monto = 100m;

        // Act & Assert
        var accion = () => Entrada.Crear(eventoId, usuarioId, monto, null, codigoQrInvalido);
        accion.Should().Throw<ArgumentException>().WithMessage("*código QR*");
    }

    [Fact]
    public void CrearEntrada_ConCodigoQrNulo_DebeLanzarExcepcion()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var monto = 100m;

        // Act & Assert
        var accion = () => Entrada.Crear(eventoId, usuarioId, monto, null, null!);
        accion.Should().Throw<ArgumentException>().WithMessage("*código QR*");
    }

    [Fact]
    public void ConfirmarPago_ConEstadoPendientePago_DebeActualizarEstado()
    {
        // Arrange
        var entrada = Entrada.Crear(Guid.NewGuid(), Guid.NewGuid(), 100m, null, "QR123");

        // Act
        entrada.ConfirmarPago();

        // Assert
        entrada.Estado.Should().Be(EstadoEntrada.Pagada);
    }

    [Fact]
    public void ConfirmarPago_ConEstadoPagada_DebeLanzarExcepcion()
    {
        // Arrange
        var entrada = Entrada.Crear(Guid.NewGuid(), Guid.NewGuid(), 100m, null, "QR123");
        entrada.ConfirmarPago();

        // Act & Assert
        var accion = () => entrada.ConfirmarPago();
        accion.Should().Throw<EstadoEntradaInvalidoException>();
    }

    [Fact]
    public void Cancelar_ConEstadoPendientePago_DebeActualizarEstado()
    {
        // Arrange
        var entrada = Entrada.Crear(Guid.NewGuid(), Guid.NewGuid(), 100m, null, "QR123");

        // Act
        entrada.Cancelar();

        // Assert
        entrada.Estado.Should().Be(EstadoEntrada.Cancelada);
    }

    [Fact]
    public void Cancelar_ConEstadoPagada_DebeActualizarEstado()
    {
        // Arrange
        var entrada = Entrada.Crear(Guid.NewGuid(), Guid.NewGuid(), 100m, null, "QR123");
        entrada.ConfirmarPago();

        // Act
        entrada.Cancelar();

        // Assert
        entrada.Estado.Should().Be(EstadoEntrada.Cancelada);
    }

    [Fact]
    public void Cancelar_ConEstadoUsada_DebeLanzarExcepcion()
    {
        // Arrange
        var entrada = Entrada.Crear(Guid.NewGuid(), Guid.NewGuid(), 100m, null, "QR123");
        entrada.ConfirmarPago();
        entrada.MarcarComoUsada();

        // Act & Assert
        var accion = () => entrada.Cancelar();
        accion.Should().Throw<EstadoEntradaInvalidoException>();
    }

    [Fact]
    public void MarcarComoUsada_ConEstadoPagada_DebeActualizarEstado()
    {
        // Arrange
        var entrada = Entrada.Crear(Guid.NewGuid(), Guid.NewGuid(), 100m, null, "QR123");
        entrada.ConfirmarPago();

        // Act
        entrada.MarcarComoUsada();

        // Assert
        entrada.Estado.Should().Be(EstadoEntrada.Usada);
    }

    [Fact]
    public void MarcarComoUsada_ConEstadoPendientePago_DebeLanzarExcepcion()
    {
        // Arrange
        var entrada = Entrada.Crear(Guid.NewGuid(), Guid.NewGuid(), 100m, null, "QR123");

        // Act & Assert
        var accion = () => entrada.MarcarComoUsada();
        accion.Should().Throw<EstadoEntradaInvalidoException>();
    }
}