using Asientos.Aplicacion.Queries;
using Asientos.Dominio.Agregados;
using Asientos.Dominio.Repositorios;
using FluentAssertions;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Asientos.Pruebas.Aplicacion.Queries;

public class ObtenerMapaAsientosQueryHandlerTests
{
    private readonly Mock<IRepositorioMapaAsientos> _repo;
    private readonly ObtenerMapaAsientosQueryHandler _handler;

    public ObtenerMapaAsientosQueryHandlerTests()
    {
        _repo = new Mock<IRepositorioMapaAsientos>();
        _handler = new ObtenerMapaAsientosQueryHandler(_repo.Object);
    }

    [Fact]
    public async Task Handle_MapaNoExiste_RetornaNull()
    {
        // Arrange
        var query = new ObtenerMapaAsientosQuery(Guid.NewGuid());
        _repo.Setup(x => x.ObtenerPorIdAsync(query.MapaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MapaAsientos?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_Ok_RetornaDto()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var mapa = MapaAsientos.Crear(eventoId);
        mapa.AgregarCategoria("VIP", 100m, true);
        mapa.AgregarCategoria("General", 50m, false);
        mapa.AgregarAsiento(1, 1, "VIP");
        mapa.AgregarAsiento(1, 2, "VIP");
        mapa.AgregarAsiento(2, 1, "General");

        var query = new ObtenerMapaAsientosQuery(mapa.Id);
        _repo.Setup(x => x.ObtenerPorIdAsync(mapa.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mapa);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.MapaId.Should().Be(mapa.Id);
        result.EventoId.Should().Be(eventoId);
        result.Categorias.Should().HaveCount(2);
        result.Asientos.Should().HaveCount(3);
        
        // Verificar orden de categorías (Prioridad primero)
        result.Categorias.First().Nombre.Should().Be("VIP");
        
        // Verificar orden de asientos (Fila, Numero)
        result.Asientos[0].Fila.Should().Be(1);
        result.Asientos[0].Numero.Should().Be(1);
        result.Asientos[1].Fila.Should().Be(1);
        result.Asientos[1].Numero.Should().Be(2);
        result.Asientos[2].Fila.Should().Be(2);
    }
}
