using Xunit;using FluentAssertions;using Microsoft.EntityFrameworkCore;using System;using System.Threading;using Asientos.Infraestructura.Persistencia;using Asientos.Infraestructura.Repositorios;using Asientos.Dominio.Repositorios;using Asientos.Aplicacion.Comandos;using Asientos.Aplicacion.Handlers;using MassTransit;using Moq;
namespace Asientos.Pruebas.Aplicacion;
public class AgregarCategoriaComandoTests
{
    private readonly IRepositorioMapaAsientos _repo; private readonly AsientosDbContext _db;
    public AgregarCategoriaComandoTests(){ var opt=new DbContextOptionsBuilder<AsientosDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options; _db=new AsientosDbContext(opt); _repo=new MapaAsientosRepository(_db);}    
    [Fact] public async Task AgregarCategoria_Ok(){ var mockPublish=new Mock<IPublishEndpoint>(); var mapaId= await new CrearMapaAsientosComandoHandler(_repo,mockPublish.Object).Handle(new CrearMapaAsientosComando(Guid.NewGuid()),CancellationToken.None); var h=new AgregarCategoriaComandoHandler(_repo,mockPublish.Object); var id= await h.Handle(new AgregarCategoriaComando(mapaId,"VIP",100m,false),CancellationToken.None); id.Should().NotBe(Guid.Empty);}    
    [Fact] public async Task AgregarCategoria_MapaInexistente_Error(){ var mockPublish=new Mock<IPublishEndpoint>(); var h=new AgregarCategoriaComandoHandler(_repo,mockPublish.Object); Func<Task> act=()=>h.Handle(new AgregarCategoriaComando(Guid.NewGuid(),"VIP",null,false),CancellationToken.None); await act.Should().ThrowAsync<InvalidOperationException>(); }
}
