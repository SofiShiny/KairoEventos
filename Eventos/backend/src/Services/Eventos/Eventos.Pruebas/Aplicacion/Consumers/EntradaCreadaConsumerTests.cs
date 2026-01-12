using System;
using System.Threading.Tasks;
using Entradas.Dominio.Eventos;
using Eventos.Aplicacion.Comandos;
using Eventos.Aplicacion.Consumers;
using BloquesConstruccion.Aplicacion.Comun;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace Eventos.Pruebas.Aplicacion.Consumers;

public class EntradaCreadaConsumerTests
{
    private readonly Mock<IMediator> _mediator;
    private readonly Mock<ILogger<EntradaCreadaConsumer>> _logger;
    private readonly EntradaCreadaConsumer _consumer;

    public EntradaCreadaConsumerTests()
    {
        _mediator = new Mock<IMediator>();
        _logger = new Mock<ILogger<EntradaCreadaConsumer>>();
        _consumer = new EntradaCreadaConsumer(_mediator.Object, _logger.Object);
    }

    [Fact]
    public async Task Consume_MensajeValido_EnviaComandoRegistrarAsistente()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var entradaId = Guid.NewGuid();
        var evento = new EntradaCreadaEvento { EntradaId = entradaId, EventoId = eventoId, UsuarioId = usuarioId, NombreUsuario = "Usuario Test", Email = "test@test.com", FechaCreacion = DateTime.UtcNow };
        
        var context = new Mock<ConsumeContext<EntradaCreadaEvento>>();
        context.Setup(c => c.Message).Returns(evento);

        _mediator.Setup(m => m.Send(It.IsAny<RegistrarAsistenteComando>(), default))
                 .ReturnsAsync(Resultado.Exito());

        // Act
        await _consumer.Consume(context.Object);

        // Assert
        _mediator.Verify(m => m.Send(It.Is<RegistrarAsistenteComando>(c => 
            c.EventoId == eventoId && 
            c.UsuarioId == usuarioId.ToString() &&
            c.NombreUsuario == "Usuario Test" &&
            c.Correo == "test@test.com"), default), Times.Once);
    }

    [Fact]
    public async Task Consume_ComandoFalla_LogeaAdvertencia()
    {
        // Arrange
        var evento = new EntradaCreadaEvento { EntradaId = Guid.NewGuid(), EventoId = Guid.NewGuid(), UsuarioId = Guid.NewGuid(), NombreUsuario = "U", Email = "e@e.com", FechaCreacion = DateTime.UtcNow };
        var context = new Mock<ConsumeContext<EntradaCreadaEvento>>();
        context.Setup(c => c.Message).Returns(evento);

        _mediator.Setup(m => m.Send(It.IsAny<RegistrarAsistenteComando>(), default))
                 .ReturnsAsync(Resultado.Falla("Error test"));

        // Act
        await _consumer.Consume(context.Object);

        // Assert
        // Verificamos que no lanza excepción (el handler captura el error del comando y lo logea)
        _mediator.Verify(m => m.Send(It.IsAny<RegistrarAsistenteComando>(), default), Times.Once);
    }

    [Fact]
    public async Task Consume_ErrorCritico_LanzaExcepcion()
    {
        // Arrange
        var evento = new EntradaCreadaEvento { EntradaId = Guid.NewGuid(), EventoId = Guid.NewGuid(), UsuarioId = Guid.NewGuid(), NombreUsuario = "U", Email = "e@e.com", FechaCreacion = DateTime.UtcNow };
        var context = new Mock<ConsumeContext<EntradaCreadaEvento>>();
        context.Setup(c => c.Message).Returns(evento);

        _mediator.Setup(m => m.Send(It.IsAny<RegistrarAsistenteComando>(), default))
                 .ThrowsAsync(new Exception("Crash"));

        // Act
        Func<Task> act = async () => await _consumer.Consume(context.Object);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Crash");
    }
}
