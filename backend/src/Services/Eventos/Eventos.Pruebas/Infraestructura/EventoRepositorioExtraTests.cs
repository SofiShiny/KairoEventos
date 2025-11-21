using Eventos.Dominio.Entidades;
using Eventos.Dominio.Enumeraciones;
using Eventos.Dominio.ObjetosDeValor;
using Eventos.Infraestructura.Persistencia;
using Eventos.Infraestructura.Repositorios;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Eventos.Pruebas.Infraestructura;

public class EventoRepositorioExtraTests
{
 private readonly EventosDbContext _ctx;
 private readonly EventoRepository _repo;
 private readonly Ubicacion _ubic;

 public EventoRepositorioExtraTests()
 {
 var options = new DbContextOptionsBuilder<EventosDbContext>()
 .UseInMemoryDatabase(Guid.NewGuid().ToString())
 .Options;
 _ctx = new EventosDbContext(options);
 _repo = new EventoRepository(_ctx);
 _ubic = new Ubicacion("Lugar","Dir","Ciudad","Reg","0000","Pais");
 }

 private Evento Build(string titulo, int offsetDias)
 {
 var ini = DateTime.UtcNow.AddDays(offsetDias);
 var fin = ini.AddDays(1);
 return new Evento(titulo, "D", _ubic, ini, fin,10, "org-1");
 }

 [Fact]
 public async Task ObtenerEventosPublicadosAsync_FiltraYOrdena()
 {
 var e1 = Build("E1",3); var e2 = Build("E2",1);
 e1.Publicar(); e2.Publicar();
 _ctx.Eventos.AddRange(e1, e2);
 await _ctx.SaveChangesAsync();
 var list = await _repo.ObtenerEventosPublicadosAsync();
 list.Should().HaveCount(2);
 list.First().FechaInicio.Should().BeBefore(list.Last().FechaInicio);
 }

 [Fact]
 public async Task ObtenerEventosPorOrganizadorAsync_FiltraYOrdenaDesc()
 {
 var e1 = Build("E1",1); var e2 = Build("E2",5); var e3 = new Evento("E3","D", _ubic, DateTime.UtcNow.AddDays(7), DateTime.UtcNow.AddDays(8),10, "org-y");
 _ctx.Eventos.AddRange(e1, e2, e3);
 await _ctx.SaveChangesAsync();
 var list = await _repo.ObtenerEventosPorOrganizadorAsync("org-1");
 list.Should().HaveCount(2);
 list.First().FechaInicio.Should().BeAfter(list.Last().FechaInicio);
 list.Should().OnlyContain(e => e.OrganizadorId == "org-1");
 }
}
