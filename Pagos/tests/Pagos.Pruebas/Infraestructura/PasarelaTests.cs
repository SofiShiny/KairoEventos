using FluentAssertions;
using Pagos.Infraestructura.Pasarela;
using Xunit;

namespace Pagos.Pruebas.Infraestructura;

public class PasarelaTests
{
    private readonly SimuladorPasarela _sut = new();

    [Fact]
    public async Task CobrarAsync_ConTarjeta0000_RetornaExito()
    {
        // Act
        var resultado = await _sut.CobrarAsync(100, "12340000");

        // Assert
        resultado.Exitoso.Should().BeTrue();
        resultado.TransaccionIdExterno.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CobrarAsync_ConTarjeta9999_RetornaRechazo()
    {
        // Act
        var resultado = await _sut.CobrarAsync(100, "12349999");

        // Assert
        resultado.Exitoso.Should().BeFalse();
        resultado.MotivoRechazo.Should().Be("Fondos Insuficientes");
    }

    [Fact]
    public async Task CobrarAsync_ConTarjeta5000_LanzaExcepcion()
    {
        // Act & Assert
        await _sut.Awaiting(s => s.CobrarAsync(100, "12345000"))
            .Should().ThrowAsync<HttpRequestException>()
            .WithMessage("*temporal*");
    }

    [Fact]
    public async Task CobrarAsync_ConCualquierOtraTarjeta_RetornaExito()
    {
        // Act
        var resultado = await _sut.CobrarAsync(100, "11111111");

        // Assert
        resultado.Exitoso.Should().BeTrue();
    }

    [Fact]
    public async Task ObtenerMovimientosAsync_RetornaVacio()
    {
        // Act
        var resultado = await _sut.ObtenerMovimientosAsync();

        // Assert
        resultado.Should().BeEmpty();
    }
}
