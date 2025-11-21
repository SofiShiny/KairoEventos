using Eventos.Aplicacion.Comandos;
using Eventos.Dominio.Entidades;
using Eventos.Dominio.Repositorios;
using Eventos.Dominio.ObjetosDeValor;
using FluentAssertions;
using Moq;
using Xunit;

namespace Eventos.Pruebas.Aplicacion.Comandos;

public class RegistrarAsistenteComandoHandlerTests
{
    private readonly Mock<IRepositorioEvento> _repo;
    private readonly RegistrarAsistenteComandoHandler _handler;
    private readonly Evento _eventoPublicado;
    private readonly Evento _eventoNoPublicado;
    private readonly Evento _eventoPublicadoConAsistente;
    private readonly DateTime _inicio;
    private readonly DateTime _fin;

    public RegistrarAsistenteComandoHandlerTests()
    {
        _repo = new Mock<IRepositorioEvento>();
        _handler = new RegistrarAsistenteComandoHandler(_repo.Object);
        _inicio = DateTime.UtcNow.AddDays(30);
        _fin = _inicio.AddDays(2);
        var ubic = new Ubicacion("Calle7", "El Marques", "Caracas", "DF", "1073", "Venezuela");
        _eventoPublicado = new Evento("Taller de Arte", "Exposicion", ubic, _inicio, _fin,500, "organizador-001");
        _eventoPublicado.Publicar();
        _eventoNoPublicado = new Evento("NoPub", "Desc", ubic, _inicio, _fin,500, "organizador-001");
        _eventoPublicadoConAsistente = new Evento("ConAsistente", "Desc", ubic, _inicio, _fin,500, "organizador-001");
        _eventoPublicadoConAsistente.Publicar();
        _eventoPublicadoConAsistente.RegistrarAsistente("usuario-001","Nombre","a@b.com");
    }

    private RegistrarAsistenteComando Cmd(Guid id, string usuario="usuario-001", string nombre="Nombre", string correo="a@b.com")
        => new(id, usuario, nombre, correo);

    [Fact]
    public async Task Handle_Valido_RegistraAsistente()
    {
        // Arrange
        var id = Guid.NewGuid();
        typeof(Evento).GetProperty("Id")!.SetValue(_eventoPublicado, id);
        _repo.Setup(r => r.ObtenerPorIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(_eventoPublicado);
        _repo.Setup(r => r.ActualizarAsync(_eventoPublicado, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var res = await _handler.Handle(Cmd(id), CancellationToken.None);

        // Assert
        res.EsExitoso.Should().BeTrue();
        _eventoPublicado.Asistentes.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_EventoNoExiste_Falla()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repo.Setup(r => r.ObtenerPorIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Evento?)null);

        // Act
        var res = await _handler.Handle(Cmd(id), CancellationToken.None);

        // Assert
        res.EsExitoso.Should().BeFalse();
        res.Error.Should().Be("Evento no encontrado");
    }

    [Fact]
    public async Task Handle_NoPublicado_Falla()
    {
        // Arrange
        var id = Guid.NewGuid();
        typeof(Evento).GetProperty("Id")!.SetValue(_eventoNoPublicado, id);
        _repo.Setup(r => r.ObtenerPorIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(_eventoNoPublicado);

        // Act
        var res = await _handler.Handle(Cmd(id), CancellationToken.None);

        // Assert
        res.EsExitoso.Should().BeFalse();
        res.Error.Should().Contain("no publicado");
    }

    [Fact]
    public async Task Handle_Duplicado_Falla()
    {
        // Arrange
        var id = Guid.NewGuid();
        typeof(Evento).GetProperty("Id")!.SetValue(_eventoPublicadoConAsistente, id);
        _repo.Setup(r => r.ObtenerPorIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(_eventoPublicadoConAsistente);

        // Act
        var res = await _handler.Handle(Cmd(id, "usuario-001", "Nombre", "a@b.com"), CancellationToken.None);

        // Assert
        res.EsExitoso.Should().BeFalse();
        res.Error.Should().Contain("ya est");
    }
}
