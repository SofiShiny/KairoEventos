using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Pagos.Aplicacion.DTOs;
using Pagos.Aplicacion.Jobs;
using Pagos.Dominio.Entidades;
using Pagos.Dominio.Interfaces;
using Pagos.Dominio.Modelos;
using Xunit;

namespace Pagos.Pruebas.Aplicacion;

public class AplicacionTests
{
    [Fact]
    public async Task ConciliacionJob_Ejecutar_LogueaInformacionYAdvertencias()
    {
        // Arrange
        var repoMock = new Mock<IRepositorioTransacciones>();
        var pasarelaMock = new Mock<IPasarelaPago>();
        var loggerMock = new Mock<ILogger<ConciliacionJob>>();

        var txPendiente = new Transaccion { Id = Guid.NewGuid(), Estado = EstadoTransaccion.Pendiente };
        repoMock.Setup(r => r.ObtenerTodasAsync()).ReturnsAsync(new List<Transaccion> { txPendiente });
        pasarelaMock.Setup(p => p.ObtenerMovimientosAsync()).ReturnsAsync(new List<object>());

        var sut = new ConciliacionJob(repoMock.Object, pasarelaMock.Object, loggerMock.Object);

        // Act
        await sut.EjecutarConciliacionDiariaAsync();

        // Assert
        repoMock.Verify(r => r.ObtenerTodasAsync(), Times.Once);
        pasarelaMock.Verify(p => p.ObtenerMovimientosAsync(), Times.Once);
        
        // Verify logger was called (at least for start, end and warning)
        loggerMock.Invocations.Count.Should().BeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public void Dtos_Propiedades_AsignanYRetornanValoresCorrectamente()
    {
        // Arrange & Act
        var crearDto = new CrearPagoDto(Guid.NewGuid(), Guid.NewGuid(), 100, "1234");
        var txDto = new TransaccionDto(Guid.NewGuid(), Guid.NewGuid(), 100, "Aprobado", "url");

        // Assert
        crearDto.Monto.Should().Be(100);
        crearDto.Tarjeta.Should().Be("1234");
        
        txDto.Estado.Should().Be("Aprobado");
        txDto.UrlFactura.Should().Be("url");
    }
}
