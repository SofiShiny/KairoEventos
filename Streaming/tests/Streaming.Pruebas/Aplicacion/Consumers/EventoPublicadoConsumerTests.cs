using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Streaming.Aplicacion.Consumers;
using Streaming.Dominio.Entidades;
using Streaming.Dominio.Interfaces;
using Eventos.Domain.Events;
using Streaming.Dominio.Modelos;

namespace Streaming.Pruebas.Aplicacion.Consumers;

public class EventoPublicadoConsumerTests
{
    private readonly Mock<IRepositorioTransmisiones> _repositorioMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ILogger<EventoPublicadoConsumer>> _loggerMock;
    private readonly EventoPublicadoConsumer _consumer;

    public EventoPublicadoConsumerTests()
    {
        _repositorioMock = new Mock<IRepositorioTransmisiones>();
        _uowMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<EventoPublicadoConsumer>>();
        
        _consumer = new EventoPublicadoConsumer(
            _repositorioMock.Object,
            _uowMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Consume_CuandoEventoEsValido_DebeCrearTransmisionConLinkDeGoogleMeet()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var mensaje = new EventoPublicadoEventoDominio(
            eventoId,
            "Concierto Online",
            DateTime.Now.AddDays(7),
            true,
            50.0m
        );

        var contextMock = new Mock<ConsumeContext<EventoPublicadoEventoDominio>>();
        contextMock.Setup(x => x.Message).Returns(mensaje);

        _repositorioMock.Setup(x => x.ObtenerPorEventoIdAsync(eventoId))
            .ReturnsAsync((Transmision?)null);

        // Act
        await _consumer.Consume(contextMock.Object);

        // Assert
        _repositorioMock.Verify(r => r.AgregarAsync(It.Is<Transmision>(t => 
            t.EventoId == eventoId && 
            t.UrlAcceso.StartsWith("https://meet.google.com/")
        )), Times.Once);

        _uowMock.Verify(u => u.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Consume_CuandoStreamingYaExiste_NoDebeCrearOtro()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var mensaje = new EventoPublicadoEventoDominio(
            eventoId,
            "Concierto Online Duplicate",
            DateTime.Now.AddDays(7),
            true,
            50.0m
        );

        var contextMock = new Mock<ConsumeContext<EventoPublicadoEventoDominio>>();
        contextMock.Setup(x => x.Message).Returns(mensaje);

        var transmisionExistente = Transmision.Crear(eventoId, PlataformaTransmision.GoogleMeet);
        _repositorioMock.Setup(x => x.ObtenerPorEventoIdAsync(eventoId))
            .ReturnsAsync(transmisionExistente);

        // Act
        await _consumer.Consume(contextMock.Object);

        // Assert
        _repositorioMock.Verify(r => r.AgregarAsync(It.IsAny<Transmision>()), Times.Never);
        _uowMock.Verify(u => u.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
