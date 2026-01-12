using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MassTransit;
using Encuestas.Aplicacion.Comandos;
using Encuestas.Dominio.Entidades;
using Encuestas.Dominio.Repositorios;
using Encuestas.Aplicacion.Eventos;

namespace Encuestas.Tests.Aplicacion;

public class ResponderEncuestaHandlerTests
{
    private readonly Mock<IRepositorioEncuestas> _repositorioMock;
    private readonly Mock<IVerificadorAsistencia> _verificadorMock;
    private readonly Mock<IPublishEndpoint> _publishMock;
    private readonly Mock<ILogger<ResponderEncuestaCommandHandler>> _loggerMock;
    private readonly ResponderEncuestaCommandHandler _handler;

    public ResponderEncuestaHandlerTests()
    {
        _repositorioMock = new Mock<IRepositorioEncuestas>();
        _verificadorMock = new Mock<IVerificadorAsistencia>();
        _publishMock = new Mock<IPublishEndpoint>();
        _loggerMock = new Mock<ILogger<ResponderEncuestaCommandHandler>>();
        
        _handler = new ResponderEncuestaCommandHandler(
            _repositorioMock.Object,
            _verificadorMock.Object,
            _publishMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ResponderEncuesta_SinAsistencia_DebeLanzarExcepcion()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var encuestaId = Guid.NewGuid();
        var eventoId = Guid.NewGuid();
        
        var encuesta = new Encuesta(encuestaId, eventoId, "Encuesta de Satisfacción");
        encuesta.Publicar();

        _repositorioMock.Setup(r => r.ObtenerPorIdAsync(encuestaId))
            .ReturnsAsync(encuesta);

        _repositorioMock.Setup(r => r.UsuarioYaRespondioAsync(encuestaId, usuarioId))
            .ReturnsAsync(false);

        _verificadorMock.Setup(v => v.VerificarAsistenciaAsync(usuarioId, eventoId))
            .ReturnsAsync(false); // Simulamos que el usuario NO asistió

        var command = new ResponderEncuestaCommand(encuestaId, usuarioId, new List<RespuestaDto>());

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Solo los usuarios que asistieron al evento pueden completar la encuesta");
        
        _repositorioMock.Verify(r => r.GuardarRespuestaAsync(It.IsAny<RespuestaUsuario>()), Times.Never);
    }

    [Fact]
    public async Task ResponderEncuesta_ConAsistenciaYTodaValida_DebeGuardarExitosamente()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var encuestaId = Guid.NewGuid();
        var eventoId = Guid.NewGuid();
        
        var encuesta = new Encuesta(encuestaId, eventoId, "Encuesta de Satisfacción");
        encuesta.Publicar();

        _repositorioMock.Setup(r => r.ObtenerPorIdAsync(encuestaId))
            .ReturnsAsync(encuesta);

        _repositorioMock.Setup(r => r.UsuarioYaRespondioAsync(encuestaId, usuarioId))
            .ReturnsAsync(false);

        _verificadorMock.Setup(v => v.VerificarAsistenciaAsync(usuarioId, eventoId))
            .ReturnsAsync(true); // Simulamos que el usuario SÍ asistió

        var command = new ResponderEncuestaCommand(encuestaId, usuarioId, new List<RespuestaDto> 
        { 
            new RespuestaDto(Guid.NewGuid(), "5") 
        });

        // Act
        var resultId = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultId.Should().NotBeEmpty();
        _repositorioMock.Verify(r => r.GuardarRespuestaAsync(It.IsAny<RespuestaUsuario>()), Times.Once);
        _publishMock.Verify(p => p.Publish(It.IsAny<EncuestaCompletadaEvento>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ResponderEncuesta_SiNoEstaPublicada_DebeLanzarExcepcion()
    {
        // Arrange
        var encuestaId = Guid.NewGuid();
        var encuesta = new Encuesta(encuestaId, Guid.NewGuid(), "Encuesta");
        // No se publica

        _repositorioMock.Setup(r => r.ObtenerPorIdAsync(encuestaId))
            .ReturnsAsync(encuesta);

        var command = new ResponderEncuestaCommand(encuestaId, Guid.NewGuid(), new List<RespuestaDto>());

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("La encuesta no existe o no está publicada");
    }
}
