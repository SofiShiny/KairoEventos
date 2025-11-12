using Eventos.Aplicacion.Comandos;
using Eventos.Aplicacion.DTOs;
using Eventos.Dominio.Entidades;
using Eventos.Dominio.Repositorios;
using Eventos.Dominio.ObjetosDeValor;
using FluentAssertions;
using Moq;
using Xunit;

namespace Eventos.Pruebas.Aplicacion.Comandos;

public class CrearEventoComandoHandlerTests
{
    private readonly Mock<IRepositorioEvento> _repositorioEventoMock;
    private readonly CrearEventoComandoHandler _handler;

    public CrearEventoComandoHandlerTests()
    {
        _repositorioEventoMock = new Mock<IRepositorioEvento>();
        _handler = new CrearEventoComandoHandler(_repositorioEventoMock.Object);
    }

    [Fact]
    public async Task Handle_ConComandoValido_DeberiaCrearEventoYDevolverExito()
    {
        // Arrange
        var ubicacionDto = new UbicacionDto
        {
            NombreLugar = "UCAB",
            Direccion = "Montalban",
            Ciudad = "Caracas",
            Region = "Distrito Capital",
            CodigoPostal = "1089",
            Pais = "Venezuela"
        }!;

        var comando = new CrearEventoComando(
            "Conferencia Hacking",
            "conferencia de hackeo etico",
            ubicacionDto,
            DateTime.UtcNow.AddDays(5),
            DateTime.UtcNow.AddDays(6),
            100,
            "evento-001"
        );

        var eventoEsperado = new Evento(
            comando.Titulo,
            comando.Descripcion,
            new Ubicacion(
                ubicacionDto.NombreLugar,
                ubicacionDto.Direccion,
                ubicacionDto.Ciudad,
                ubicacionDto.Region,
                ubicacionDto.CodigoPostal,
                ubicacionDto.Pais
            ),
            comando.FechaInicio,
            comando.FechaFin,
            comando.MaximoAsistentes,
            comando.OrganizadorId
        );

        // El repositorio agrega el evento y no devuelve valor
        _repositorioEventoMock
            .Setup(x => x.AgregarAsync(It.IsAny<Evento>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(comando, CancellationToken.None);

        // Assert
        result.IsExito.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Titulo.Should().Be(comando.Titulo);
        result.Value.Descripcion.Should().Be(comando.Descripcion);
        result.Value.Ubicacion.Should().NotBeNull();
        result.Value.Ubicacion.Ciudad.Should().Be("Caracas");

        _repositorioEventoMock.Verify(x => x.AgregarAsync(It.IsAny<Evento>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConUbicacionNula_DeberiaDevolverFalla()
    {
        // Arrange
        var comando = new CrearEventoComando(
            "Conferencia Hacking",
            "Conferencia de hackeo etico",
            null!,
            DateTime.UtcNow.AddDays(5),
            DateTime.UtcNow.AddDays(6),
            100,
            "evento-001"
        );

        // Act
        var result = await _handler.Handle(comando, CancellationToken.None);

        // Assert
        result.IsExito.Should().BeFalse();
        result.Error.Should().Be("La ubicación es obligatoria");
        _repositorioEventoMock.Verify(x => x.AgregarAsync(It.IsAny<Evento>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CuandoElRepositorioTiraArgumentException_DeberiaDevolverFalla()
    {
        // Arrange
        var ubicacionDto = new UbicacionDto
        {
            NombreLugar = "Auditorio Guido Arnal",
            Direccion = "UCAB",
            Ciudad = "Caracas",
            Region = "Montalban",
            CodigoPostal = "1089",
            Pais = "Venezuela"
        }!;

        var comando = new CrearEventoComando(
            "Charla de Ciberseguridad",
            "Charla sobre Ciberseguridad en la banca de Venezuela",
            ubicacionDto,
            DateTime.UtcNow.AddDays(5),
            DateTime.UtcNow.AddDays(6),
            100,
            "evento-002"
        );

        _repositorioEventoMock
            .Setup(x => x.AgregarAsync(It.IsAny<Evento>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("La fecha de inicio debe ser en el futuro"));

        // Act
        var result = await _handler.Handle(comando, CancellationToken.None);

        // Assert
        result.IsExito.Should().BeFalse();
        result.Error.Should().Be("La fecha de inicio debe ser en el futuro");
    }
}