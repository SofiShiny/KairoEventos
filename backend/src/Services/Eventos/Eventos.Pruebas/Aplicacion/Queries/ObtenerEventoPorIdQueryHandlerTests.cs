using Eventos.Aplicacion.Queries;
using Eventos.Dominio.Entidades;
using Eventos.Dominio.Enumeraciones;
using Eventos.Dominio.Repositorios;
using Eventos.Dominio.ObjetosDeValor;
using FluentAssertions;
using Moq;
using Xunit;

namespace Eventos.Pruebas.Aplicacion.Queries;

public class ObtenerEventoPorIdQueryHandlerTests
{
    private readonly Mock<IRepositorioEvento> _repositoryMock;
    private readonly ObtenerEventoPorIdQueryHandler _handler;

    public ObtenerEventoPorIdQueryHandlerTests()
    {
        _repositoryMock = new Mock<IRepositorioEvento>();
        _handler = new ObtenerEventoPorIdQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_DeberiaDevolverEventoDto_CuandoEventoExiste()
    {
        // Preparar
        var eventId = Guid.NewGuid();
        var direccion = new Ubicacion("Av Principal123", "Sucre", "Caracas", "DF", "1029", "Venezuela");
        var fechaInicio = DateTime.UtcNow.AddMonths(1);
        var fechaFin = fechaInicio.AddHours(8);
        var @evento = new Evento(
            "ArtCraft",
            "Evento de arte",
            direccion,
            fechaInicio,
            fechaFin,
            100,
            "organizador-001");

        typeof(Evento).GetProperty("Id")!.SetValue(@evento, eventId);

        _repositoryMock.Setup(x => x.ObtenerPorIdAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(@evento);

        var query = new ObtenerEventoPorIdQuery(eventId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        var dto = result!.Value!;
        dto.Id.Should().Be(@evento.Id);
        dto.Titulo.Should().Be("ArtCraft");
        dto.Descripcion.Should().Be("Evento de arte");
        dto.Estado.Should().Be(EstadoEvento.Borrador.ToString());

        _repositoryMock.Verify(x => x.ObtenerPorIdAsync(eventId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DeberiaDevolverNull_CuandoEventoNoExiste()
    {
        // Preparar
        var eventId = Guid.NewGuid();
        _repositoryMock.Setup(x => x.ObtenerPorIdAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Evento?)null);

        var query = new ObtenerEventoPorIdQuery(eventId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        // Handler returns a failure Resultado, not null
        result.Should().NotBeNull();
        result!.EsExitoso.Should().BeFalse();
        result.Error.Should().Be("Evento no encontrado");
        _repositoryMock.Verify(x => x.ObtenerPorIdAsync(eventId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DeberiaMapearUbicacion_CuandoEventoTieneUbicacion()
    {
        // Preparar
        var eventId = Guid.NewGuid();
        var direccion = new Ubicacion("Av Principal123", "Sucre", "Caracas", "DF", "1029", "Venezuela");
        var fechaInicio = DateTime.UtcNow.AddDays(30);
        var fechaFin = fechaInicio.AddHours(4);
        var @evento = new Evento(
            "Conference",
            "Description",
            direccion,
            fechaInicio,
            fechaFin,
            50,
            "organizador-001");

        typeof(Evento).GetProperty("Id")!.SetValue(@evento, eventId);

        _repositoryMock.Setup(x => x.ObtenerPorIdAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(@evento);

        var query = new ObtenerEventoPorIdQuery(eventId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        var dto = result!.Value!;
        dto.Ubicacion.Should().NotBeNull();
        dto.Ubicacion.NombreLugar.Should().Be("Av Principal123");
        dto.Ubicacion.Direccion.Should().Be("Sucre");
        dto.Ubicacion.Ciudad.Should().Be("Caracas");
        dto.Ubicacion.Region.Should().Be("DF");
        dto.Ubicacion.CodigoPostal.Should().Be("1029");
        dto.Ubicacion.Pais.Should().Be("Venezuela");
    }

    [Fact]
    public async Task Handle_DeberiaPropagarOperacionCancelada_CuandoElTokenEstaCancelado()
    {
        // Preparar
        var eventId = Guid.NewGuid();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _repositoryMock.Setup(x => x.ObtenerPorIdAsync(eventId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var query = new ObtenerEventoPorIdQuery(eventId);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _handler.Handle(query, cts.Token));
    }

    [Fact]
    public async Task Handle_DeberiaIncluirAsistentes_CuandoEventoTieneAsistentes()
    {
        // Preparar
        var eventId = Guid.NewGuid();
        var direccion = new Ubicacion("Av principal123", "Sucre", "Caracas", "DF", "1029", "Venezuela");
        var fechaInicio = DateTime.UtcNow.AddDays(10);
        var fechaFin = fechaInicio.AddHours(2);
        var @evento = new Evento("ArtCraft", "Evento de arte", direccion, fechaInicio, fechaFin, 100, "organizador-001");

        typeof(Evento).GetProperty("Id")!.SetValue(@evento, eventId);
        @evento.Publicar();

        // Registrar asistentes
        @evento.RegistrarAsistente("usuario-001", "Creonte Lara", "cdlara@est.ucab.edu.ve");
        @evento.RegistrarAsistente("usuario-002", "Electra Wilson", "eywilson@est.ucab.edu.ve");

        _repositoryMock.Setup(x => x.ObtenerPorIdAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(@evento);

        var query = new ObtenerEventoPorIdQuery(eventId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        var dto = result!.Value!;
        dto.Asistentes.Should().HaveCount(2);
        // Mapping sets NombreUsuario on DTO
        dto.Asistentes.Should().Contain(a => a.NombreUsuario == "Creonte Lara");
        dto.Asistentes.Should().Contain(a => a.NombreUsuario == "Electra Wilson");
    }
}
