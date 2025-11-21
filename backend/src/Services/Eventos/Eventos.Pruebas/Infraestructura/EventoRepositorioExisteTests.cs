using Eventos.Dominio.Entidades;
using Eventos.Dominio.ObjetosDeValor;
using Eventos.Infraestructura.Persistencia;
using Eventos.Infraestructura.Repositorios;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Eventos.Pruebas.Infraestructura;

public class EventoRepositorioExisteTests
{
 private readonly EventosDbContext _ctx;
 private readonly EventoRepository _repo;

 public EventoRepositorioExisteTests()
 {
 var options = new DbContextOptionsBuilder<EventosDbContext>()
 .UseInMemoryDatabase(Guid.NewGuid().ToString())
 .Options;
 _ctx = new EventosDbContext(options);
 _repo = new EventoRepository(_ctx);
 }

 private Evento Build()
 {
 var ubic = new Ubicacion("L","D","C","R","0","P");
 return new Evento("T","D", ubic, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2),10, "org");
 }

 [Fact]
 public async Task ExisteAsync_TrueCuandoExiste()
 {
 var e = Build();
 _ctx.Eventos.Add(e);
 await _ctx.SaveChangesAsync();
 (await _repo.ExisteAsync(e.Id)).Should().BeTrue();
 }

 [Fact]
 public async Task ExisteAsync_FalseCuandoNoExiste()
 {
 (await _repo.ExisteAsync(Guid.NewGuid())).Should().BeFalse();
 }
}
