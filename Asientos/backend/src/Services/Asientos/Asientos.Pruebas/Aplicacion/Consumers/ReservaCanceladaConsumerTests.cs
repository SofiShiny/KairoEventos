using Asientos.Aplicacion.Consumers;
using Asientos.Dominio.Agregados;
using Asientos.Dominio.Entidades;
using Asientos.Dominio.ObjetosDeValor;
using Asientos.Dominio.Repositorios;
using Entradas.Dominio.Eventos;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;

namespace Asientos.Pruebas.Aplicacion.Consumers;

public class ReservaCanceladaConsumerTests
{
    private readonly Mock<IRepositorioMapaAsientos> _repositorio;
    private readonly Mock<ILogger<ReservaCanceladaConsumer>> _logger;
    private readonly ReservaCanceladaConsumer _consumer;

    public ReservaCanceladaConsumerTests()
    {
        _repositorio = new Mock<IRepositorioMapaAsientos>();
        _logger = new Mock<ILogger<ReservaCanceladaConsumer>>();
        _consumer = new ReservaCanceladaConsumer(_repositorio.Object, _logger.Object);
    }

    private ReservaCanceladaEvento CreateEvento(Guid? asientoId)
    {
        return new ReservaCanceladaEvento(
            Guid.NewGuid(),
            asientoId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow
        );
    }

    [Fact]
    public async Task Consume_AsientoIdNull_NoHaceNada()
    {
        // Arrange
        var context = new Mock<ConsumeContext<ReservaCanceladaEvento>>();
        context.Setup(x => x.Message).Returns(CreateEvento(null));

        // Act
        await _consumer.Consume(context.Object);

        // Assert
        _repositorio.Verify(x => x.ObtenerAsientoPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Consume_AsientoNoExiste_NoHaceNada()
    {
        // Arrange
        var asientoId = Guid.NewGuid();
        var context = new Mock<ConsumeContext<ReservaCanceladaEvento>>();
        context.Setup(x => x.Message).Returns(CreateEvento(asientoId));
        _repositorio.Setup(x => x.ObtenerAsientoPorIdAsync(asientoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Asiento?)null);

        // Act
        await _consumer.Consume(context.Object);

        // Assert
        _repositorio.Verify(x => x.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Consume_AsientoNoReservado_NoHaceNada()
    {
        // Arrange
        var asientoId = Guid.NewGuid();
        var context = new Mock<ConsumeContext<ReservaCanceladaEvento>>();
        context.Setup(x => x.Message).Returns(CreateEvento(asientoId));
        
        var cat = CategoriaAsiento.Crear("VIP", 100, true);
        var asiento = new Asiento(Guid.NewGuid(), Guid.NewGuid(), 1, 1, cat, true);
        
        _repositorio.Setup(x => x.ObtenerAsientoPorIdAsync(asientoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(asiento);

        // Act
        await _consumer.Consume(context.Object);

        // Assert
        _repositorio.Verify(x => x.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Consume_MapaNoExiste_NoHaceNada()
    {
        // Arrange
        var asientoId = Guid.NewGuid();
        var mapaId = Guid.NewGuid();
        var context = new Mock<ConsumeContext<ReservaCanceladaEvento>>();
        context.Setup(x => x.Message).Returns(CreateEvento(asientoId));
        
        var cat = CategoriaAsiento.Crear("VIP", 100, true);
        var asiento = new Asiento(mapaId, Guid.NewGuid(), 1, 1, cat, true);
        asiento.Reservar(Guid.NewGuid());
        
        _repositorio.Setup(x => x.ObtenerAsientoPorIdAsync(asientoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(asiento);
        _repositorio.Setup(x => x.ObtenerPorIdAsync(mapaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MapaAsientos?)null);

        // Act
        await _consumer.Consume(context.Object);

        // Assert
        _repositorio.Verify(x => x.ActualizarAsync(It.IsAny<MapaAsientos>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Consume_Ok_LiberaAsiento()
    {
        // Arrange
        var asientoId = Guid.NewGuid();
        var eventoId = Guid.NewGuid();
        var context = new Mock<ConsumeContext<ReservaCanceladaEvento>>();
        context.Setup(x => x.Message).Returns(CreateEvento(asientoId));
        
        var mapa = MapaAsientos.Crear(eventoId);
        mapa.AgregarCategoria("VIP", 100, true);
        var asiento = mapa.AgregarAsiento(1, 1, "VIP");
        mapa.ReservarAsiento(1, 1, Guid.NewGuid());
        
        _repositorio.Setup(x => x.ObtenerAsientoPorIdAsync(asientoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(asiento);
        
        _repositorio.Setup(x => x.ObtenerPorIdAsync(mapa.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mapa);

        // Act
        await _consumer.Consume(context.Object);

        // Assert
        asiento.Reservado.Should().BeFalse();
        _repositorio.Verify(x => x.ActualizarAsync(mapa, It.IsAny<CancellationToken>()), Times.Once);
    }
}
