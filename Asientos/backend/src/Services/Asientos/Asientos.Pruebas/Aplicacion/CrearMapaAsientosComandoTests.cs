using Xunit;using FluentAssertions;using Microsoft.EntityFrameworkCore;using System;using System.Threading;using Asientos.Infraestructura.Persistencia;using Asientos.Infraestructura.Repositorios;using Asientos.Dominio.Repositorios;using Asientos.Aplicacion.Comandos;using Asientos.Aplicacion.Handlers;using MassTransit;using Moq;
namespace Asientos.Pruebas.Aplicacion;
public class CrearMapaAsientosComandoTests
{
    private readonly IRepositorioMapaAsientos _repo; private readonly AsientosDbContext _db;
    public CrearMapaAsientosComandoTests(){ var opt=new DbContextOptionsBuilder<AsientosDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options; _db=new AsientosDbContext(opt); _repo=new MapaAsientosRepository(_db);}    
    [Fact] public async Task CrearMapa_Ok(){ var mockPublish=new Mock<IPublishEndpoint>(); var h=new CrearMapaAsientosComandoHandler(_repo,mockPublish.Object); var mapaId= await h.Handle(new CrearMapaAsientosComando(Guid.NewGuid()),CancellationToken.None); mapaId.Should().NotBe(Guid.Empty);}    
}
