using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using FluentAssertions;
using Streaming.API.Controllers;
using Streaming.Dominio.Entidades;
using Streaming.Dominio.Interfaces;
using Streaming.Dominio.Modelos;
using Streaming.Aplicacion.DTOs;

namespace Streaming.Pruebas.API.Controllers;

public class StreamingControllerTests
{
    private readonly Mock<IRepositorioTransmisiones> _repositorioMock;
    private readonly StreamingController _controller;

    public StreamingControllerTests()
    {
        _repositorioMock = new Mock<IRepositorioTransmisiones>();
        _controller = new StreamingController(_repositorioMock.Object);
    }

    [Fact]
    public async Task ObtenerPorEvento_DebeRetornarOk_CuandoExisteTransmision()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var transmision = Transmision.Crear(eventoId, PlataformaTransmision.GoogleMeet);
        
        _repositorioMock.Setup(r => r.ObtenerPorEventoIdAsync(eventoId))
            .ReturnsAsync(transmision);

        // Act
        var result = await _controller.ObtenerPorEvento(eventoId);

        // Assert
        var okResult = result.Result.As<OkObjectResult>();
        okResult.Should().NotBeNull();
        var dto = okResult.Value.As<TransmisionDto>();
        dto.EventoId.Should().Be(eventoId);
        dto.UrlAcceso.Should().Be(transmision.UrlAcceso);
    }

    [Fact]
    public async Task ObtenerPorEvento_DebeRetornarNotFound_CuandoNoExisteTransmision()
    {
        // Arrange
        _repositorioMock.Setup(r => r.ObtenerPorEventoIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Transmision?)null);

        // Act
        var result = await _controller.ObtenerPorEvento(Guid.NewGuid());

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }
}
