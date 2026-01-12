using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading;
using System.Linq;
using Asientos.Infraestructura.Persistencia;
using Asientos.Infraestructura.Repositorios;
using Asientos.Dominio.Repositorios;
using Asientos.Aplicacion.Comandos;
using Asientos.Aplicacion.Handlers;
using Asientos.Aplicacion.Hubs;
using Asientos.Dominio.Agregados;
using MassTransit;
using Moq;

namespace Asientos.Pruebas.Aplicacion;

public class ReservarAsientoComandoTests
{
    private DbContextOptions<AsientosDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<AsientosDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    private Mock<IHubContext<AsientosHub>> CreateMockHub()
    {
        var mockHub = new Mock<IHubContext<AsientosHub>>();
        var mockClients = new Mock<IHubClients>();
        var mockClientProxy = new Mock<IClientProxy>();

        mockHub.Setup(x => x.Clients).Returns(mockClients.Object);
        mockClients.Setup(x => x.Group(It.IsAny<string>())).Returns(mockClientProxy.Object);
        
        return mockHub;
    }

    [Fact]
    public async Task ReservarAsiento_Ok()
    {
        var opt = CreateOptions();
        var mockPublish = new Mock<IPublishEndpoint>();
        var mockHub = CreateMockHub();
        
        Guid mapaId;
        Guid asientoId;

        // Arrange - Persistence scope
        using (var db = new AsientosDbContext(opt))
        {
            var repo = new MapaAsientosRepository(db);
            var mapa = MapaAsientos.Crear(Guid.NewGuid());
            mapa.AgregarCategoria("VIP", 100m, true);
            var asiento = mapa.AgregarAsiento(1, 1, "VIP");
            await repo.AgregarAsync(mapa, CancellationToken.None);
            mapaId = mapa.Id;
            asientoId = asiento.Id;
        }
        
        // Act - Handler scope
        using (var db = new AsientosDbContext(opt))
        {
            var repo = new MapaAsientosRepository(db);
            var h = new ReservarAsientoComandoHandler(repo, mockPublish.Object, mockHub.Object);
            await h.Handle(new ReservarAsientoComando(mapaId, asientoId, Guid.NewGuid()), CancellationToken.None);
        }
        
        // Assert - Verify scope
        using (var db = new AsientosDbContext(opt))
        {
            var repo = new MapaAsientosRepository(db);
            var mapaResult = await repo.ObtenerPorIdAsync(mapaId, CancellationToken.None);
            mapaResult!.Asientos.First(a => a.Id == asientoId).Reservado.Should().BeTrue();
        }
    }

    [Fact]
    public async Task ReservarAsiento_MapaInexistente_Error()
    {
        var opt = CreateOptions();
        using var db = new AsientosDbContext(opt);
        var repo = new MapaAsientosRepository(db);
        var mockPublish = new Mock<IPublishEndpoint>();
        var mockHub = CreateMockHub();
        
        var h = new ReservarAsientoComandoHandler(repo, mockPublish.Object, mockHub.Object);
        
        Func<Task> act = () => h.Handle(
            new ReservarAsientoComando(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()), 
            CancellationToken.None);
        
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
