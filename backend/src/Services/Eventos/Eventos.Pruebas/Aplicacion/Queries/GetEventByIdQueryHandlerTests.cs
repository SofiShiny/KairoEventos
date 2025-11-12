/*using Eventos.Aplicacion.Queries;
using Eventos.Dominio.Entidades;
using Eventos.Dominio.Enumerados;
using Eventos.Dominio.Repositorios;
using Eventos.Dominio.ObjetosValor;
using FluentAssertions;
using Moq;
using Xunit;

namespace Eventos.Pruebas.Aplicacion.Queries;

public class GetEventoByIdQueryHandlerTests
{
    private readonly Mock<IEventoRepository> _repositoryMock;
    private readonly GetEventoByIdQueryHandler _handler;

    public GetEventoByIdQueryHandlerTests()
    {
        _repositoryMock = new Mock<IEventoRepository>();
        _handler = new GetEventoByIdQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnEventoDto_WhenEventoExists()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var direccion = new Location("Conference Center", "123 Main St", "Tech Ciudad", "CA", "94000", "USA");
        var startDate = DateTime.UtcNow.AddMonths(1);
        var endDate = startDate.AddHours(8);
        var @evento = new Evento(
            "Tech Conference 2024",
            "Annual tech conference",
            direccion,
            startDate,
            endDate,
            100,
            "organizador-001");

        typeof(Evento).GetProperty("Id")!.SetValue(@evento, eventId);

        _repositoryMock.Setup(x => x.GetByIdAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(@evento);

        var query = new GetEventoByIdQuery(eventId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(@evento.Id);
        result.Title.Should().Be("Tech Conference 2024");
        result.Description.Should().Be("Annual tech conference");
        result.Status.Should().Be(EventoStatus.Draft.ToString());
        
        _repositoryMock.Verify(x => x.GetByIdAsync(eventId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenEventoDoesNotExist()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        _repositoryMock.Setup(x => x.GetByIdAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Evento?)null);

        var query = new GetEventoByIdQuery(eventId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _repositoryMock.Verify(x => x.GetByIdAsync(eventId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldMapLocationCorrectly_WhenEventoHasLocation()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var direccion = new Location("Venue Nombre", "Direccion Line", "Ciudad", "CA", "90000", "USA");
        var startDate = DateTime.UtcNow.AddDays(30);
        var endDate = startDate.AddHours(4);
        var @evento = new Evento(
            "Conference",
            "Description",
            direccion,
            startDate,
            endDate,
            50,
            "organizador-001");

        typeof(Evento).GetProperty("Id")!.SetValue(@evento, eventId);

        _repositoryMock.Setup(x => x.GetByIdAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(@evento);

        var query = new GetEventoByIdQuery(eventId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Location.Should().NotBeNull();
        result.Location.VenueNombre.Should().Be("Venue Nombre");
        result.Location.Direccion.Should().Be("Direccion Line");
        result.Location.Ciudad.Should().Be("Ciudad");
        result.Location.State.Should().Be("CA");
        result.Location.ZipCode.Should().Be("90000");
        result.Location.Pais.Should().Be("USA");
    }

    [Fact]
    public async Task Handle_ShouldHandleCancellation_WhenTokenIsCancelled()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _repositoryMock.Setup(x => x.GetByIdAsync(eventId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var query = new GetEventoByIdQuery(eventId);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => 
            _handler.Handle(query, cts.Token));
    }

    [Fact]
    public async Task Handle_ShouldIncludeAsistentesCount_WhenEventoHasAsistentes()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var direccion = new Location("Venue", "Direccion", "Ciudad", "CA", "12345", "USA");
        var startDate = DateTime.UtcNow.AddDays(10);
        var endDate = startDate.AddHours(2);
        var @evento = new Evento("Evento", "Desc", direccion, startDate, endDate, 100, "organizador-001");

        typeof(Evento).GetProperty("Id")!.SetValue(@evento, eventId);
        @evento.Publicar();

        // Registrar asistentes
        @evento.RegistrarAsistente("user-001", "John Doe", "john@example.com");
        @evento.RegistrarAsistente("user-002", "Jane Smith", "jane@example.com");

        _repositoryMock.Setup(x => x.GetByIdAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(@evento);

        var query = new GetEventoByIdQuery(eventId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Asistentes.Should().HaveCount(2);
        result.Asistentes.Should().Contain(a => a.NombreUsuario == "John Doe");
        result.Asistentes.Should().Contain(a => a.NombreUsuario == "Jane Smith");
    }
}
*/