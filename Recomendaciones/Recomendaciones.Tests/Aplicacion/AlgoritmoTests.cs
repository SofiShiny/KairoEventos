using Moq;
using FluentAssertions;
using Recomendaciones.Aplicacion.Queries;
using Recomendaciones.Dominio.Entidades;
using Recomendaciones.Dominio.Repositorios;

namespace Recomendaciones.Tests.Aplicacion;

public class AlgoritmoRecomendacionTests
{
    private readonly Mock<IRepositorioRecomendaciones> _repositorioMock;
    private readonly ObtenerRecomendacionesQueryHandler _handler;

    public AlgoritmoRecomendacionTests()
    {
        _repositorioMock = new Mock<IRepositorioRecomendaciones>();
        _handler = new ObtenerRecomendacionesQueryHandler(_repositorioMock.Object);
    }

    [Fact]
    public async Task ObtenerRecomendaciones_DebePriorizarCategoriasConMayorPuntaje()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        
        // Afinidades: Rock (50), Jazz (0)
        var afinidadRock = new AfinidadUsuario(usuarioId, "Rock");
        afinidadRock.SumarPuntos(50);
        
        var afinidadJazz = new AfinidadUsuario(usuarioId, "Jazz");
        // Jazz se queda en 0

        var afinidades = new List<AfinidadUsuario> { afinidadRock, afinidadJazz };

        // Eventos en BD
        var eventoJazz = new EventoProyeccion(Guid.NewGuid(), "Concierto Jazz Suave", "Jazz", DateTime.UtcNow.AddDays(1));
        var eventoRock = new EventoProyeccion(Guid.NewGuid(), "Mega Festival Rock", "Rock", DateTime.UtcNow.AddDays(2));

        _repositorioMock.Setup(r => r.ObtenerAfinidadesPorUsuarioAsync(usuarioId))
            .ReturnsAsync(afinidades);

        _repositorioMock.Setup(r => r.ObtenerEventosPorCategoriasAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<EventoProyeccion> { eventoJazz, eventoRock });

        // Act
        var result = await _handler.Handle(new ObtenerRecomendacionesQuery(usuarioId), CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.First().Categoria.Should().Be("Rock"); // Rock debe ser primero por tener 50 puntos vs Jazz 0
        result.Last().Categoria.Should().Be("Jazz");
    }

    [Fact]
    public async Task ColdStart_DebeRetornarEventosProximos()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        _repositorioMock.Setup(r => r.ObtenerAfinidadesPorUsuarioAsync(usuarioId))
            .ReturnsAsync(new List<AfinidadUsuario>()); // Sin historial

        var eventos = new List<EventoProyeccion>
        {
            new EventoProyeccion(Guid.NewGuid(), "Evento 1", "Cat1", DateTime.UtcNow.AddDays(1)),
            new EventoProyeccion(Guid.NewGuid(), "Evento 2", "Cat2", DateTime.UtcNow.AddDays(2))
        };

        _repositorioMock.Setup(r => r.ObtenerEventosFuturosAsync())
            .ReturnsAsync(eventos);

        // Act
        var result = await _handler.Handle(new ObtenerRecomendacionesQuery(usuarioId), CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        _repositorioMock.Verify(r => r.ObtenerEventosFuturosAsync(), Times.Once);
    }
}
 public class EntradaCompradaConsumerTests
 {
    // Aquí irían tests para los consumidores...
 }
