using System;
using System.Threading.Tasks;
using Eventos.Dominio.Entidades;
using Eventos.Dominio.ObjetosDeValor;
using Eventos.Infraestructura.Persistencia;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Eventos.Pruebas.Infraestructura;

public class EventosDbContextoAsistentesPropertyTests
{
 private EventosDbContext CrearDbContexto()
 {
 var options = new DbContextOptionsBuilder<EventosDbContext>()
 .UseInMemoryDatabase(Guid.NewGuid().ToString())
 .Options;
 return new EventosDbContext(options);
 }

 [Fact]
 public async Task AsistentesDbSet_PropiedadAccedida_CubreLinea15()
 {
 using var context = CrearDbContexto();
 var ubic = new Ubicacion("Lugar","Dir","Ciudad","Reg","CP","Pais");
 var inicio = DateTime.UtcNow.AddDays(3);
 var ev = new Evento("T","D", ubic, inicio, inicio.AddHours(2),10, "org");
 ev.Publicar();
 ev.RegistrarAsistente("u1","Nombre","a@b.com");
 context.Eventos.Add(ev);
 await context.SaveChangesAsync();

 // Acceso a la propiedad Asistentes (linea15) y consulta
 var asistentes = await context.Asistentes.ToListAsync();
 asistentes.Should().HaveCount(1);
 asistentes[0].UsuarioId.Should().Be("u1");
 }
}
