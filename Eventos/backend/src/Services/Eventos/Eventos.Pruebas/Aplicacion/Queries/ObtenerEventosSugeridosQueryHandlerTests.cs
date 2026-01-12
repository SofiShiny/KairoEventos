using Eventos.Aplicacion.Queries;
using Eventos.Aplicacion.DTOs;
using Eventos.Dominio.Entidades;
using Eventos.Dominio.Repositorios;
using Eventos.Dominio.ObjetosDeValor;
using FluentAssertions;
using Moq;
using AutoMapper;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Eventos.Pruebas.Aplicacion.Queries;

public class ObtenerEventosSugeridosQueryHandlerTests
{
    private readonly Mock<IRepositorioEvento> _repo;
    private readonly Mock<IMapper> _mapper;
    private readonly ObtenerEventosSugeridosQueryHandler _handler;

    public ObtenerEventosSugeridosQueryHandlerTests()
    {
        _repo = new Mock<IRepositorioEvento>();
        _mapper = new Mock<IMapper>();
        _handler = new ObtenerEventosSugeridosQueryHandler(_repo.Object, _mapper.Object);
    }

    private Evento CreateEvento(string titulo, string categoria, DateTime inicio)
    {
        var e = new Evento(titulo, "Desc", new Ubicacion("L", "D", "C", "R", "0", "P"), inicio, inicio.AddHours(2), 10, "org", categoria);
        e.Publicar();
        return e;
    }

    [Fact]
    public async Task Handle_ExistenEventosDeLaCategoria_RetornaSugeridos()
    {
        // Arrange
        var futuro = DateTime.UtcNow.AddDays(10);
        var eventos = new List<Evento>
        {
            CreateEvento("E1", "Musica", futuro),
            CreateEvento("E2", "Musica", futuro.AddDays(1)),
            CreateEvento("E3", "Deporte", futuro)
        };

        _repo.Setup(r => r.ObtenerEventosPublicadosAsync(It.IsAny<CancellationToken>())).ReturnsAsync(eventos);
        _mapper.Setup(m => m.Map<IEnumerable<EventoDto>>(It.IsAny<IEnumerable<Evento>>()))
               .Returns((IEnumerable<Evento> src) => src.Select(e => new EventoDto { Titulo = e.Titulo }));

        var query = new ObtenerEventosSugeridosQuery(Categoria: "Musica", FechaDesde: futuro.AddDays(-1), Top: 5);

        // Act
        var res = await _handler.Handle(query, CancellationToken.None);

        // Assert
        res.EsExitoso.Should().BeTrue();
        res.Valor.Should().HaveCount(2);
        res.Valor.First().Titulo.Should().Be("E1");
    }

    [Fact]
    public async Task Handle_NoExistenDeLaCategoria_RetornaFallbackGenerales()
    {
        // Arrange
        var futuro = DateTime.UtcNow.AddDays(10);
        var eventos = new List<Evento>
        {
            // Ninguno es "Musica"
            CreateEvento("E1", "Deporte", futuro),
            CreateEvento("E2", "Cultura", futuro.AddDays(1))
        };

        _repo.Setup(r => r.ObtenerEventosPublicadosAsync(It.IsAny<CancellationToken>())).ReturnsAsync(eventos);
        _mapper.Setup(m => m.Map<IEnumerable<EventoDto>>(It.IsAny<IEnumerable<Evento>>()))
               .Returns((IEnumerable<Evento> src) => src.Select(e => new EventoDto { Titulo = e.Titulo }));

        // Categoria que no existe en la lista
        var query = new ObtenerEventosSugeridosQuery(Categoria: "Musica", FechaDesde: futuro.AddDays(-1), Top: 5);

        // Act
        var res = await _handler.Handle(query, CancellationToken.None);

        // Assert
        res.EsExitoso.Should().BeTrue();
        // Debería retornar los 2 eventos como fallback
        res.Valor.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ConFechaDesde_FiltraEventosViejos()
    {
        // Arrange
        var futuro = DateTime.UtcNow.AddDays(10);
        var evPasado = CreateEvento("E1", "Musica", futuro);
        typeof(Evento).GetProperty("FechaInicio")!.SetValue(evPasado, DateTime.UtcNow.AddDays(-5));

        var eventos = new List<Evento>
        {
            evPasado,
            CreateEvento("E2", "Musica", futuro.AddDays(2))
        };

        _repo.Setup(r => r.ObtenerEventosPublicadosAsync(It.IsAny<CancellationToken>())).ReturnsAsync(eventos);
        _mapper.Setup(m => m.Map<IEnumerable<EventoDto>>(It.IsAny<IEnumerable<Evento>>()))
               .Returns((IEnumerable<Evento> src) => src.Select(e => new EventoDto { Titulo = e.Titulo }));

        var query = new ObtenerEventosSugeridosQuery(Categoria: "Musica", FechaDesde: DateTime.UtcNow.AddDays(1), Top: 5);

        // Act
        var res = await _handler.Handle(query, CancellationToken.None);

        // Assert
        res.EsExitoso.Should().BeTrue();
        res.Valor.Should().HaveCount(1);
        res.Valor.First().Titulo.Should().Be("E2");
    }
}
