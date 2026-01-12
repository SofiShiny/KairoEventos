using Eventos.Aplicacion.Queries;
using Eventos.Aplicacion; // EventoMappingProfile
using Eventos.Dominio.Entidades;
using Eventos.Dominio.Repositorios;
using Eventos.Dominio.ObjetosDeValor;
using FluentAssertions;
using Moq;
using Xunit;
using Eventos.Dominio.Enumeraciones;
using AutoMapper;

namespace Eventos.Pruebas.Aplicacion.Queries;

public class ObtenerEventoPorIdQueryHandlerTests
{
    private readonly Mock<IRepositorioEvento> _repo;
    private readonly ObtenerEventoPorIdQueryHandler _handler;
    private readonly Evento _eventoBase;
    private readonly Evento _eventoSinubicacion;
    private readonly DateTime _inicio;
    private readonly DateTime _fin;
    private readonly IMapper _mapper;

    public ObtenerEventoPorIdQueryHandlerTests()
    {
        _repo = new Mock<IRepositorioEvento>(MockBehavior.Strict);
        var config = new MapperConfiguration(cfg => cfg.AddProfile(new EventoMappingProfile()));
        _mapper = config.CreateMapper();
        _handler = new ObtenerEventoPorIdQueryHandler(_repo.Object, _mapper);
        _inicio = DateTime.UtcNow.AddMonths(1);
        _fin = _inicio.AddHours(8);
        _eventoBase = new Evento("ArtCraft", "Evento de arte", new Ubicacion("Av Principal123","Sucre","Caracas","DF","1029","Venezuela"), _inicio, _fin,100, "organizador-001");
        _eventoSinubicacion = new Evento("SinUbic","Desc", new Ubicacion("L","D","C","R","0","P"), _inicio, _fin,10, "org");
        typeof(Evento).GetProperty("Ubicacion")!.SetValue(_eventoSinubicacion, null);
    }

    private ObtenerEventoPorIdQuery Cmd(Guid id) => new(id);

    [Fact]
    public async Task Handle_EventoExiste_MapeaDto()
    {
        _repo.Setup(r=>r.ObtenerPorIdAsync(_eventoBase.Id, It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(_eventoBase);
        var res = await _handler.Handle(Cmd(_eventoBase.Id), CancellationToken.None);
        res.EsExitoso.Should().BeTrue();
        res.Valor!.Id.Should().Be(_eventoBase.Id);
        res.Valor.Estado.Should().Be(EstadoEvento.Borrador.ToString());
    }

    [Fact]
    public async Task Handle_EventoNoExiste_Falla()
    {
        _repo.Setup(r=>r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync((Evento?)null);
        var res = await _handler.Handle(Cmd(Guid.NewGuid()), CancellationToken.None);
        res.EsExitoso.Should().BeFalse();
        res.Error.Should().Contain("no encontrado");
    }

    [Fact]
    public async Task Handle_MapeaUbicacion()
    {
        _repo.Setup(r=>r.ObtenerPorIdAsync(_eventoBase.Id, It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(_eventoBase);
        var res = await _handler.Handle(Cmd(_eventoBase.Id), CancellationToken.None);
        res.EsExitoso.Should().BeTrue();
        res.Valor!.Ubicacion!.NombreLugar.Should().Be("Av Principal123");
    }

    [Fact]
    public async Task Handle_IncluyeAsistentes()
    {
        _eventoBase.Publicar();
        _eventoBase.RegistrarAsistente("usuario-001", "Creonte", "c@d.com");
        _eventoBase.RegistrarAsistente("usuario-002", "Electra", "e@f.com");
        _repo.Setup(r=>r.ObtenerPorIdAsync(_eventoBase.Id, It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(_eventoBase);
        var res = await _handler.Handle(Cmd(_eventoBase.Id), CancellationToken.None);
        res.EsExitoso.Should().BeTrue();
        res.Valor!.Asistentes.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_EventoSinUbicacion_Falla()
    {
        _repo.Setup(r=>r.ObtenerPorIdAsync(_eventoSinubicacion.Id, It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(_eventoSinubicacion);
        var res = await _handler.Handle(Cmd(_eventoSinubicacion.Id), CancellationToken.None);
        res.EsExitoso.Should().BeFalse();
        res.Error.Should().Contain("ubicacion");
    }
}

