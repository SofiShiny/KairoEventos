using Eventos.Dominio.Entidades;
using Eventos.Dominio.Enumeraciones;
using Eventos.Dominio.ObjetosDeValor;
using Eventos.Infraestructura.Persistencia;
using Eventos.Infraestructura.Repositorios;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Eventos.Pruebas.Infraestructura.Repositorios;

// ========== Pruebas de EventoRepositorioTests.cs ==========

public class EventoRepositorioTests
{
    private readonly EventosDbContext _contexto;
    private readonly EventoRepository _repositorio;

    public EventoRepositorioTests()
    {
        var options = new DbContextOptionsBuilder<EventosDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _contexto = new EventosDbContext(options);
        _repositorio = new EventoRepository(_contexto);
    }

    [Fact]
    public async Task AgregarAsync_DeberiaAgregarEventoEnBaseDeDatos()
    {
        // Preparar
        var fechaInicio = DateTime.UtcNow.AddDays(30);
        var fechaFin = fechaInicio.AddDays(2);
        var direccion = new Ubicacion("Calle7", "El Marques", "Caracas", "DF", "1073", "Venezuela");
        var evento = new Evento("Taller de Arte", "Exposicion", direccion, fechaInicio, fechaFin,500, "organizador-001");

        // Actuar
        await _repositorio.AgregarAsync(evento, CancellationToken.None);

        // Comprobar
        var savedEvento = await _contexto.Eventos.FindAsync(evento.Id);
        savedEvento.Should().NotBeNull();
        savedEvento!.Titulo.Should().Be("Taller de Arte");
    }

    [Fact]
    public async Task ObtenerPorIdAsync_CuandoExiste_DeberiaRetornarEvento()
    {
        // Preparar
        var fechaInicio = DateTime.UtcNow.AddDays(30);
        var fechaFin = fechaInicio.AddDays(2);
        var direccion = new Ubicacion("Calle7", "El Marques", "Caracas", "DF", "1073", "Venezuela");
        var evento = new Evento("Taller de Arte", "Exposicion", direccion, fechaInicio, fechaFin,500, "organizador-001");
        await _contexto.Eventos.AddAsync(evento);
        await _contexto.SaveChangesAsync();

        // Actuar
        var result = await _repositorio.ObtenerPorIdAsync(evento.Id, CancellationToken.None);

        // Comprobar
        result.Should().NotBeNull();
        result!.Id.Should().Be(evento.Id);
        result.Titulo.Should().Be("Taller de Arte");
    }

    [Fact]
    public async Task ObtenerPorIdAsync_CuandoNoExiste_DeberiaRetornarNull()
    {
        // Preparar
        var id = Guid.NewGuid();

        // Actuar
        var result = await _repositorio.ObtenerPorIdAsync(id, CancellationToken.None);

        // Comprobar
        result.Should().BeNull();
    }

    [Fact]
    public async Task ActualizarAsync_DeberiaActualizarEvento()
    {
        // Preparar
        var fechaInicio = DateTime.UtcNow.AddDays(30);
        var fechaFin = fechaInicio.AddDays(2);
        var direccion = new Ubicacion("Calle7", "El Marques", "Caracas", "DF", "1073", "Venezuela");
        var evento = new Evento("Original", "Descripcion", direccion, fechaInicio, fechaFin,500, "organizador-001");
        await _contexto.Eventos.AddAsync(evento);
        await _contexto.SaveChangesAsync();

        // Actuar
        evento.Publicar();
        await _repositorio.ActualizarAsync(evento, CancellationToken.None);

        // Comprobar
        var updatedEvento = await _contexto.Eventos.FindAsync(evento.Id);
        updatedEvento.Should().NotBeNull();
        updatedEvento!.Estado.Should().Be(EstadoEvento.Publicado);
    }

    [Fact]
    public async Task EliminarAsync_DeberiaEliminarEvento()
    {
        // Preparar
        var fechaInicio = DateTime.UtcNow.AddDays(30);
        var fechaFin = fechaInicio.AddDays(2);
        var direccion = new Ubicacion("Calle7", "El Marques", "Caracas", "DF", "1073", "Venezuela");
        var evento = new Evento("Taller de Arte", "Exposicion", direccion, fechaInicio, fechaFin,500, "organizador-001");
        await _contexto.Eventos.AddAsync(evento);
        await _contexto.SaveChangesAsync();

        // Actuar
        await _repositorio.EliminarAsync(evento.Id, CancellationToken.None);

        // Comprobar
        var deletedEvento = await _contexto.Eventos.FindAsync(evento.Id);
        deletedEvento.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerTodosAsync_DeberiaRetornarTodos()
    {
        // Preparar
        var fechaInicio = DateTime.UtcNow.AddDays(30);
        var fechaFin = fechaInicio.AddDays(2);
        var direccion = new Ubicacion("Calle7", "El Marques", "Caracas", "DF", "1073", "Venezuela");

        var e1 = new Evento("Evento1", "Evento de cine", direccion, fechaInicio, fechaFin,100, "organizador-001");
        var e2 = new Evento("Evento2", "Evento de conferencia de blockchain", direccion, fechaInicio.AddDays(10), fechaFin.AddDays(10),200, "organizador-002");
        await _contexto.Eventos.AddRangeAsync(e1, e2);
        await _contexto.SaveChangesAsync();

        // Actuar
        var resultados = await _repositorio.ObtenerTodosAsync(CancellationToken.None);

        // Comprobar
        resultados.Should().HaveCount(2);
        resultados.Should().Contain(e => e.Titulo == "Evento1");
        resultados.Should().Contain(e => e.Titulo == "Evento2");
    }
}

// ========== Pruebas de EventoRepositorioExisteTests.cs ==========

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

// ========== Pruebas de EventoRepositorioExtraTests.cs ==========

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
