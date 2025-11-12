using Eventos.Aplicacion.Comandos;
using Eventos.Dominio.Entidades;
using Eventos.Dominio.Repositorios;
using Eventos.Dominio.ObjetosDeValor;
using FluentAssertions;
using Moq;
using Xunit;

namespace Eventos.Pruebas.Aplicacion.Comandos;

public class RegistrarAsistenteComandoHandlerTests
{
    private readonly Mock<IRepositorioEvento> _eventRepositoryMock;
    private readonly RegistrarAsistenteComandoHandler _handler;

    public RegistrarAsistenteComandoHandlerTests()
    {
        _eventRepositoryMock = new Mock<IRepositorioEvento>();
        _handler = new RegistrarAsistenteComandoHandler(_eventRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ConComandoValido_DeberiaRegistrarAsistente()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = "usuario-001";
        var nombreUsuario = "Creonte Dioniso Lara Wilson";
        var email = "cdlara@est.ucab.edu.ve";
        var comando = new RegistrarAsistenteComando(eventId, userId, nombreUsuario, email);
        
        var startDate = DateTime.UtcNow.AddDays(30);
        var endDate = startDate.AddDays(2);
        var direccion = new Ubicacion("Calle7", "El Marques", "Caracas", "DF", "1073", "Venezuela");
        var eventEntity = new Evento("Taller de Arte", "Exposicion de Obras", direccion, startDate, endDate,500, "organizador-001");
        
        var idProperty = typeof(Evento).GetProperty("Id");
        if (idProperty != null && idProperty.CanWrite)
        {
            idProperty.SetValue(eventEntity, eventId);
        }
        
        eventEntity.Publicar();

        _eventRepositoryMock
            .Setup(x => x.ObtenerPorIdAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventEntity);

        _eventRepositoryMock
            .Setup(x => x.ActualizarAsync(It.IsAny<Evento>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(comando, CancellationToken.None);

        // Assert
        result.EsExitoso.Should().BeTrue();
        eventEntity.Asistentes.Should().ContainSingle();
        _eventRepositoryMock.Verify(
            x => x.ActualizarAsync(It.IsAny<Evento>(), It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConEventoInexistente_DeberiaTirarException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var comando = new RegistrarAsistenteComando(eventId, "usuario-001", "Creonte Lara", "cdlara@est.ucab.edu.ve");

        _eventRepositoryMock
            .Setup(x => x.ObtenerPorIdAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Evento?)null);

        // Act
        var result = await _handler.Handle(comando, CancellationToken.None);

        // Assert
        result.EsExitoso.Should().BeFalse();
        result.Error.Should().Be("Evento no encontrado");
    }
}
