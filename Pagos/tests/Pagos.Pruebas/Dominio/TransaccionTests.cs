using FluentAssertions;
using Pagos.Dominio.Entidades;
using Pagos.Dominio.Modelos;
using Xunit;

namespace Pagos.Pruebas.Dominio;

public class TransaccionTests
{
    [Fact]
    public void Transaccion_Propiedades_FuncionanCorrectamente()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ordenId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        
        // Act
        var tx = new Transaccion
        {
            Id = id,
            OrdenId = ordenId,
            UsuarioId = usuarioId,
            Monto = 500.25m,
            TarjetaMascara = "**** 5555",
            Estado = EstadoTransaccion.Pendiente,
            FechaCreacion = DateTime.UtcNow
        };

        // Assert
        tx.Id.Should().Be(id);
        tx.OrdenId.Should().Be(ordenId);
        tx.UsuarioId.Should().Be(usuarioId);
        tx.Monto.Should().Be(500.25m);
        tx.TarjetaMascara.Should().Be("**** 5555");
        tx.Estado.Should().Be(EstadoTransaccion.Pendiente);
    }

    [Fact]
    public void Aprobar_CambiaEstadoYUrl()
    {
        // Arrange
        var tx = new Transaccion();

        // Act
        tx.Aprobar("http://factura.pdf");

        // Assert
        tx.Estado.Should().Be(EstadoTransaccion.Aprobado);
        tx.UrlFactura.Should().Be("http://factura.pdf");
        tx.MensajeError.Should().BeNull();
    }

    [Fact]
    public void Rechazar_CambiaEstadoYMotivo()
    {
        // Arrange
        var tx = new Transaccion();

        // Act
        tx.Rechazar("Error de prueba");

        // Assert
        tx.Estado.Should().Be(EstadoTransaccion.Rechazado);
        tx.MensajeError.Should().Be("Error de prueba");
        tx.UrlFactura.Should().BeNull();
    }
}
