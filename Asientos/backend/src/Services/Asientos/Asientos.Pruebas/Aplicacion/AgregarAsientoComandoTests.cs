using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using Asientos.Infraestructura.Persistencia;
using Asientos.Infraestructura.Repositorios;
using Asientos.Dominio.Repositorios;
using Asientos.Aplicacion.Comandos;
using Asientos.Aplicacion.Handlers;
using MassTransit;
using Moq;

namespace Asientos.Pruebas.Aplicacion;

public class AgregarAsientoComandoTests
{
    private readonly IRepositorioMapaAsientos _repo;
    private readonly AsientosDbContext _db;

    public AgregarAsientoComandoTests()
    {
        var opt = new DbContextOptionsBuilder<AsientosDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new AsientosDbContext(opt);
        _repo = new MapaAsientosRepository(_db);
    }

    [Fact]
    public async Task AgregarAsiento_Ok()
    {
        var mockPublish = new Mock<IPublishEndpoint>();
        var mapaId = await new CrearMapaAsientosComandoHandler(_repo, mockPublish.Object)
            .Handle(new CrearMapaAsientosComando(Guid.NewGuid()), CancellationToken.None);
        
        var mapa = await _repo.ObtenerPorIdAsync(mapaId, CancellationToken.None);
        mapa!.AgregarCategoria("VIP", 100m, true);
        await _repo.ActualizarAsync(mapa, CancellationToken.None);
        
        var h = new AgregarAsientoComandoHandler(_repo, mockPublish.Object);
        var id = await h.Handle(new AgregarAsientoComando(mapa.Id, 1, 1, "VIP"), CancellationToken.None);
        
        id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task AgregarAsiento_CategoriaInexistente_Error()
    {
        var mockPublish = new Mock<IPublishEndpoint>();
        var mapaId = await new CrearMapaAsientosComandoHandler(_repo, mockPublish.Object)
            .Handle(new CrearMapaAsientosComando(Guid.NewGuid()), CancellationToken.None);
        
        var h = new AgregarAsientoComandoHandler(_repo, mockPublish.Object);
        
        Func<Task> act = () => h.Handle(new AgregarAsientoComando(mapaId, 1, 1, "VIP"), CancellationToken.None);
        
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task AgregarAsiento_MapaNoExiste_Error()
    {
        var mockPublish = new Mock<IPublishEndpoint>();
        var h = new AgregarAsientoComandoHandler(_repo, mockPublish.Object);
        
        Func<Task> act = () => h.Handle(new AgregarAsientoComando(Guid.NewGuid(), 1, 1, "VIP"), CancellationToken.None);
        
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
