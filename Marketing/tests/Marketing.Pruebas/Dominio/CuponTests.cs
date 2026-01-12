using Marketing.Dominio.Entidades;
using Marketing.Dominio.Enums;
using FluentAssertions;
using Xunit;

namespace Marketing.Pruebas.Dominio;

public class CuponTests
{
    [Fact]
    public void CrearCupon_ConDatosValidos_DebeInicializarCorrectamente()
    {
        // Arrange
        var codigo = "PROMO2024";
        var expiracion = DateTime.UtcNow.AddDays(10);

        // Act
        var cupon = new Cupon(codigo, TipoDescuento.Porcentaje, 15, expiracion);

        // Assert
        cupon.Codigo.Should().Be("PROMO2024");
        cupon.Estado.Should().Be(EstadoCupon.Disponible);
        cupon.EsValido().Should().BeTrue();
    }

    [Fact]
    public void AsignarADestinatario_CuandoEsValido_DebeCargarUsuario()
    {
        // Arrange
        var cupon = new Cupon("TEST", TipoDescuento.MontoFijo, 100, DateTime.UtcNow.AddDays(1));
        var userId = Guid.NewGuid();

        // Act
        cupon.AsignarADestinatario(userId);

        // Assert
        cupon.UsuarioDestinatarioId.Should().Be(userId);
    }

    [Fact]
    public void MarcarComoUsado_DebeCambiarEstadoYRegistrarFecha()
    {
        // Arrange
        var cupon = new Cupon("TEST", TipoDescuento.MontoFijo, 100, DateTime.UtcNow.AddDays(1));
        var userId = Guid.NewGuid();

        // Act
        cupon.MarcarComoUsado(userId);

        // Assert
        cupon.Estado.Should().Be(EstadoCupon.Usado);
        cupon.UsuarioQueLoUso.Should().Be(userId);
        cupon.FechaUso.Should().NotBeNull();
    }
}
