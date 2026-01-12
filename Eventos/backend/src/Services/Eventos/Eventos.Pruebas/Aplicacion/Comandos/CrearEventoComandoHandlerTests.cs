using Eventos.Aplicacion.Comandos;
using Eventos.Aplicacion.DTOs;
using Eventos.Aplicacion.Validators;
using Eventos.Dominio.Entidades;
using Eventos.Dominio.Repositorios;
using FluentAssertions;
using FluentValidation;
using Moq;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Eventos.Pruebas.Aplicacion.Comandos;

public class CrearEventoComandoHandlerTests
{
    private readonly Mock<IRepositorioEvento> _repo;
    private readonly IValidator<CrearEventoComando> _validator;
    private readonly Mock<ILogger<CrearEventoComandoHandler>> _logger;
    private readonly CrearEventoComandoHandler _handler;
    private readonly DateTime _inicio;
    private readonly DateTime _fin;
    private readonly UbicacionDto _ubicBase;
    private Evento? _capturado;
    private readonly string _org = "org-001";

    public CrearEventoComandoHandlerTests()
    {
        _repo = new Mock<IRepositorioEvento>(MockBehavior.Strict);
        _validator = new CrearEventoComandoValidator();
        _logger = new Mock<ILogger<CrearEventoComandoHandler>>();
        _handler = new CrearEventoComandoHandler(_repo.Object, _validator, _logger.Object);
        _inicio = DateTime.UtcNow.AddDays(5);
        _fin = _inicio.AddDays(1);
        _ubicBase = new UbicacionDto{ NombreLugar="Lugar", Direccion="Dir", Ciudad="Ciudad", Region="Reg", CodigoPostal="0000", Pais="Pais" };
    }

    private CrearEventoComando Cmd(string titulo="Conferencia", string desc="Desc", UbicacionDto? u=null, int max=100, DateTime? ini=null, DateTime? fin=null, string? org=null)
    {
        var fechaIni = ini ?? DateTime.UtcNow.AddDays(5);
        var fechaFin = fin ?? fechaIni.AddDays(1);
        return new(titulo, desc, u!, fechaIni, fechaFin, max, org ?? _org);
    }

    private void SetupAgregar() => _repo.Setup(r=>r.AgregarAsync(It.IsAny<Evento>(), It.IsAny<CancellationToken>()))
        .Callback<Evento, CancellationToken>((e,ct)=> _capturado = e)
        .Returns(Task.CompletedTask);

    [Fact]
    public async Task Handle_Valido_CreaEvento()
    {
        SetupAgregar();
        var cmd = Cmd(u:_ubicBase);
        var res = await _handler.Handle(cmd, CancellationToken.None);
        res.EsExitoso.Should().BeTrue();
        _capturado.Should().NotBeNull();
        res.Valor!.Titulo.Should().Be(cmd.Titulo);
        res.Valor.Ubicacion!.NombreLugar.Should().Be(_ubicBase.NombreLugar);
        _repo.VerifyAll();
    }

    [Fact]
    public async Task Handle_UbicacionNula_Falla()
    {
        var cmd = Cmd(u:null);
        var res = await _handler.Handle(cmd, CancellationToken.None);
        res.EsExitoso.Should().BeFalse();
        res.Error.Should().Contain("ubicacion");
    }

    [Fact]
    public async Task Handle_FechasInvalidas_Falla()
    {
        var inicio = DateTime.UtcNow.AddDays(2);
        var cmd = Cmd(u:_ubicBase, ini:inicio, fin:inicio);
        var res = await _handler.Handle(cmd, CancellationToken.None);
        res.EsExitoso.Should().BeFalse();
        res.Error.Should().Contain("fecha fin");
    }

    [Fact]
    public async Task Handle_MaximoCero_Falla()
    {
        var cmd = Cmd(u:_ubicBase, max:0);
        var res = await _handler.Handle(cmd, CancellationToken.None);
        res.EsExitoso.Should().BeFalse();
        res.Error.Should().Contain("maximo");
    }

    [Fact]
    public async Task Handle_TituloVacio_FallaDominio()
    {
        var cmd = Cmd(titulo:" ", u:_ubicBase);
        var res = await _handler.Handle(cmd, CancellationToken.None);
        res.EsExitoso.Should().BeFalse();
        res.Error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_OrganizadorVacio_FallaDominio()
    {
        var cmd = Cmd(org:" ", u:_ubicBase);
        var res = await _handler.Handle(cmd, CancellationToken.None);
        res.EsExitoso.Should().BeFalse();
        res.Error.Should().Contain("organizador");
    }

    [Fact]
    public async Task Handle_DescripcionVacia_FallaDominio()
    {
        // El validador no chequea descripción, pero el dominio sí.
        // Esto prueba el bloque try-catch del handler.
        var cmd = Cmd(desc:" ", u:_ubicBase);
        var res = await _handler.Handle(cmd, CancellationToken.None);
        res.EsExitoso.Should().BeFalse();
        res.Error.Should().Contain("descripción");
    }

    [Fact]
    public async Task Handle_MapUbicacion_CamposOpcionalesNulos_RegionCodigoPostalVacios()
    {
        SetupAgregar();
        var dto = new UbicacionDto{ NombreLugar="NL", Direccion="DIR", Ciudad="CI", Region=null, CodigoPostal=null, Pais="PA" };
        var cmd = Cmd(u:dto);
        var res = await _handler.Handle(cmd, CancellationToken.None);
        res.EsExitoso.Should().BeTrue();
        res.Valor!.Ubicacion!.Region.Should().BeEmpty();
        res.Valor.Ubicacion!.CodigoPostal.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_MapUbicacion_CamposRequeridosNulos_Falla()
    {
        var dto = new UbicacionDto{ NombreLugar=null, Direccion="DIR", Ciudad="CI", Pais="PA" };
        var cmd = Cmd(u:dto);
        var res = await _handler.Handle(cmd, CancellationToken.None);
        res.EsExitoso.Should().BeFalse();
        res.Error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_MapUbicacion_DireccionNula_Falla()
    {
        var dto = new UbicacionDto{ NombreLugar="L", Direccion=null, Ciudad="CI", Pais="PA" };
        var cmd = Cmd(u:dto);
        var res = await _handler.Handle(cmd, CancellationToken.None);
        res.EsExitoso.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_MapUbicacion_CiudadNula_Falla()
    {
        var dto = new UbicacionDto{ NombreLugar="L", Direccion="D", Ciudad=null, Pais="PA" };
        var cmd = Cmd(u:dto);
        var res = await _handler.Handle(cmd, CancellationToken.None);
        res.EsExitoso.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_MapUbicacion_PaisNulo_Falla()
    {
        var dto = new UbicacionDto{ NombreLugar="L", Direccion="D", Ciudad="C", Pais=null };
        var cmd = Cmd(u:dto);
        var res = await _handler.Handle(cmd, CancellationToken.None);
        res.EsExitoso.Should().BeFalse();
    }
}