using Eventos.Dominio.Entidades;
using Eventos.Dominio.ObjetosDeValor;
using Eventos.Infraestructura.Persistencia;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;

using Xunit;

namespace Eventos.Pruebas.Infraestructura.Persistencia;

// ========== Pruebas de EventosDbContextoTests.cs ==========

public class EventosDbContextoTests
{
    private EventosDbContext CrearDbContexto()
    {
        var options = new DbContextOptionsBuilder<EventosDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new EventosDbContext(options);
    }

    [Fact]
    public void EventosDbContexto_DeberiaTenerDbSetEventos()
    {
        // Preparar
        using var context = CrearDbContexto();

        // Actuar & Comprobar
        context.Eventos.Should().NotBeNull();
    }

    [Fact]
    public async Task EventosDbContexto_DeberiaGuardarEvento()
    {
        // Preparar
        using var context = CrearDbContexto();
        var direccion = new Ubicacion("Calle7", "El Marques", "Caracas", "DF", "1073", "Venezuela");
        var @evento = new Evento(
            "Taller de Arte",
            "Exposicion de Obras",
            direccion,
            DateTime.UtcNow.AddDays(30),
            DateTime.UtcNow.AddDays(30).AddHours(4),
            100,
            "organizador-001");

        // Actuar
        context.Eventos.Add(@evento);
        await context.SaveChangesAsync();

        // Comprobar
        var savedEvento = await context.Eventos.FindAsync(@evento.Id);
        savedEvento.Should().NotBeNull();
        savedEvento!.Titulo.Should().Be("Taller de Arte");
    }

    [Fact]
    public async Task EventosDbContexto_DeberiaRastrearCambios()
    {
        // Preparar
        using var context = CrearDbContexto();
        var direccion = new Ubicacion("Calle7", "El Marques", "Caracas", "DF", "1073", "Venezuela");
        var @evento = new Evento(
            "Nombre Original",
            "Descripcion",
            direccion,
            DateTime.UtcNow.AddDays(30),
            DateTime.UtcNow.AddDays(30).AddHours(4),
            100,
            "organizador-001");

        context.Eventos.Add(@evento);
        await context.SaveChangesAsync();

        // Actuar
        @evento.Actualizar("Nombre Actualizado", "Nueva Descripcion", direccion, @evento.FechaInicio, @evento.FechaFin, 100);
        await context.SaveChangesAsync();

        // Comprobar
        var updatedEvento = await context.Eventos.FindAsync(@evento.Id);
        updatedEvento!.Titulo.Should().Be("Nombre Actualizado");
        updatedEvento.Descripcion.Should().Be("Nueva Descripcion");
    }

    [Fact]
    public async Task EventosDbContexto_DeberiaEliminarEvento()
    {
        // Preparar
        using var context = CrearDbContexto();
        var direccion = new Ubicacion("Calle7", "El Marques", "Caracas", "DF", "1073", "Venezuela");
        var @evento = new Evento(
            "Evento a Eliminar",
            "Descripcion",
            direccion,
            DateTime.UtcNow.AddDays(30),
            DateTime.UtcNow.AddDays(30).AddHours(4),
            100,
            "organizador-001");

        context.Eventos.Add(@evento);
        await context.SaveChangesAsync();

        // Actuar
        context.Eventos.Remove(@evento);
        await context.SaveChangesAsync();

        // Comprobar
        var deletedEvento = await context.Eventos.FindAsync(@evento.Id);
        deletedEvento.Should().BeNull();
    }
}

// ========== Pruebas de EventosDbContextoAsistentesPropertyTests.cs ==========

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
