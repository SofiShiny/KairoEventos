using Eventos.Dominio.Entidades;
using Eventos.Dominio.Enumeraciones;
using Eventos.Dominio.EventosDeDominio;
using Eventos.Dominio.ObjetosDeValor;
using Eventos.Pruebas.Shared;
using FluentAssertions;
using System.Linq;
using System.Reflection;
using System;
using Xunit;

namespace Eventos.Pruebas.Dominio.Entidades;

// ========== Pruebas de EventoTests.cs ==========

public class EventoTests
{
    [Fact]
    public void CrearEvento_ConDatosValidos_DeberiaTenerExito()
    {
        // Preparar
        var titulo = "Taller de Arte";
        var descripcion = "Exposicion de Obras";
        var fechaInicio = DateTime.UtcNow.AddDays(30);
        var fechaFin = fechaInicio.AddDays(2);
        var ubicacion = new Ubicacion("Calle7", "El Marques", "Caracas", "DF", "1073", "Venezuela");
        var maximo = 500;
        var organizadorId = "organizador-001";

        // Actuar
        var evento = new Evento(titulo, descripcion, ubicacion, fechaInicio, fechaFin, maximo, organizadorId);

        // Comprobar
        evento.Should().NotBeNull();
        evento.Titulo.Should().Be(titulo);
        evento.Descripcion.Should().Be(descripcion);
        evento.Ubicacion.Should().Be(ubicacion);
        evento.FechaInicio.Should().Be(fechaInicio);
        evento.FechaFin.Should().Be(fechaFin);
        evento.MaximoAsistentes.Should().Be(maximo);
        evento.Estado.Should().Be(EstadoEvento.Borrador);
        evento.Asistentes.Should().BeEmpty();
    }

    [Fact]
    public void CrearEvento_ConFechaFinAntesDeFechaInicio_DeberiaLanzarExcepcion()
    {
        // Preparar
        var fechaInicio = DateTime.UtcNow.AddDays(30);
        var fechaFin = fechaInicio.AddDays(-1);
        var ubicacion = new Ubicacion("Calle7", "El Marques", "Caracas", "DF", "1073", "Venezuela");

        // Actuar
        Action act = () => new Evento("Titulo", "Descripcion", ubicacion, fechaInicio, fechaFin, 100, "organizador-001");

        // Comprobar
        act.Should().Throw<ArgumentException>()
            .WithMessage("*posterior a la fecha de inicio*");
    }

    [Fact]
    public void CrearEvento_ConMaximoAsistentesNegativo_DeberiaLanzarExcepcion()
    {
        // Preparar
        var fechaInicio = DateTime.UtcNow.AddDays(30);
        var fechaFin = fechaInicio.AddDays(2);
        var ubicacion = new Ubicacion("Calle7", "El Marques", "Caracas", "DF", "1073", "Venezuela");

        // Actuar
        Action act = () => new Evento("Titulo", "Descripcion", ubicacion, fechaInicio, fechaFin, -1, "organizador-001");

        // Comprobar
        act.Should().Throw<ArgumentException>()
            .WithMessage("*asistentes debe ser mayor que cero*");
    }

    [Fact]
    public void Publicar_EventoEnBorrador_DeberiaCambiarEstadoAPublicado()
    {
        // Preparar
        var fechaInicio = DateTime.UtcNow.AddDays(30);
        var fechaFin = fechaInicio.AddDays(2);
        var ubicacion = new Ubicacion("Calle7", "El Marques", "Caracas", "DF", "1073", "Venezuela");
        var evento = new Evento("Taller de Arte", "Exposicion", ubicacion, fechaInicio, fechaFin, 500, "organizador-001");

        // Actuar
        evento.Publicar();

        // Comprobar
        evento.Estado.Should().Be(EstadoEvento.Publicado);
    }

    [Fact]
    public void Publicar_EventoYaPublicado_DeberiaLanzarExcepcion()
    {
        // Preparar
        var fechaInicio = DateTime.UtcNow.AddDays(30);
        var fechaFin = fechaInicio.AddDays(2);
        var ubicacion = new Ubicacion("Calle7", "El Marques", "Caracas", "DF", "1073", "Venezuela");
        var evento = new Evento("Taller de Arte", "Exposicion", ubicacion, fechaInicio, fechaFin, 500, "organizador-001");
        evento.Publicar();

        // Actuar
        Action act = () => evento.Publicar();

        // Comprobar
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*No se puede publicar*estado*");
    }

    [Fact]
    public void Cancelar_EventoPublicado_DeberiaCambiarEstadoACancelado()
    {
        // Preparar
        var fechaInicio = DateTime.UtcNow.AddDays(30);
        var fechaFin = fechaInicio.AddDays(2);
        var ubicacion = new Ubicacion("Calle7", "El Marques", "Caracas", "DF", "1073", "Venezuela");
        var evento = new Evento("Taller de Arte", "Exposicion", ubicacion, fechaInicio, fechaFin, 500, "organizador-001");
        evento.Publicar();

        // Actuar
        evento.Cancelar();

        // Comprobar
        evento.Estado.Should().Be(EstadoEvento.Cancelado);
    }

    [Fact]
    public void RegistrarAsistente_ConCupoDisponible_DeberiaRegistrar()
    {
        // Preparar
        var fechaInicio = DateTime.UtcNow.AddDays(30);
        var fechaFin = fechaInicio.AddDays(2);
        var ubicacion = new Ubicacion("Calle7", "El Marques", "Caracas", "DF", "1073", "Venezuela");
        var evento = new Evento("Taller de Arte", "Exposicion", ubicacion, fechaInicio, fechaFin, 500, "organizador-001");
        evento.Publicar();
        var usuarioId = "usuario-001";
        var nombreUsuario = "Creonte Dioniso Lara Wilson";
        var correo = "cdlara@est.ucab.edu.ve";

        // Actuar
        evento.RegistrarAsistente(usuarioId, nombreUsuario, correo);

        // Comprobar
        evento.Asistentes.Should().ContainSingle();
        var asistente = evento.Asistentes.First();
        asistente.UsuarioId.Should().Be(usuarioId);
        asistente.NombreUsuario.Should().Be(nombreUsuario);
        asistente.Correo.Should().Be(correo);
    }

    [Fact]
    public void RegistrarAsistente_EventoNoPublicado_DeberiaLanzarExcepcion()
    {
        // Preparar
        var fechaInicio = DateTime.UtcNow.AddDays(30);
        var fechaFin = fechaInicio.AddDays(2);
        var ubicacion = new Ubicacion("Calle7", "El Marques", "Caracas", "DF", "1073", "Venezuela");
        var evento = new Evento("Taller de Arte", "Exposicion", ubicacion, fechaInicio, fechaFin, 500, "organizador-001");

        // Actuar
        Action act = () => evento.RegistrarAsistente("usuario-001", "Creonte", "cdlara@est.ucab.edu.ve");

        // Comprobar
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("No se puede registrar en un evento no publicado");
    }

    [Fact]
    public void RegistrarAsistente_EventoLleno_DeberiaLanzarExcepcion()
    {
        // Preparar
        var fechaInicio = DateTime.UtcNow.AddDays(30);
        var fechaFin = fechaInicio.AddDays(2);
        var ubicacion = new Ubicacion("Calle7", "El Marques", "Caracas", "DF", "1073", "Venezuela");
        var evento = new Evento("Taller de Arte", "Exposicion", ubicacion, fechaInicio, fechaFin, 1, "organizador-001");
        evento.Publicar();
        evento.RegistrarAsistente("usuario-001", "Creonte", "cdlara@est.ucab.edu.ve");

        // Actuar
        Action act = () => evento.RegistrarAsistente("usuario-002", "Electra", "electra@example.com");

        // Comprobar
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*evento est* completo*");
    }

    [Fact]
    public void EstaCompleto_AlAlcanzarCapacidad_DeberiaRetornarTrue()
    {
        // Preparar
        var fechaInicio = DateTime.UtcNow.AddDays(30);
        var fechaFin = fechaInicio.AddDays(2);
        var ubicacion = new Ubicacion("Calle7", "El Marques", "Caracas", "DF", "1073", "Venezuela");
        var evento = new Evento("Taller de Arte", "Exposicion", ubicacion, fechaInicio, fechaFin, 2, "organizador-001");
        evento.Publicar();
        evento.RegistrarAsistente("usuario-001", "Creonte", "cdlara@est.ucab.edu.ve");
        evento.RegistrarAsistente("usuario-002", "Electra", "electra@example.com");

        // Actuar
        var lleno = evento.EstaCompleto;

        // Comprobar
        lleno.Should().BeTrue();
    }

    [Fact]
    public void EstaCompleto_PorDebajoDeCapacidad_DeberiaRetornarFalse()
    {
        // Preparar
        var fechaInicio = DateTime.UtcNow.AddDays(30);
        var fechaFin = fechaInicio.AddDays(2);
        var ubicacion = new Ubicacion("Calle7", "El Marques", "Caracas", "DF", "1073", "Venezuela");
        var evento = new Evento("Taller de Arte", "Exposicion", ubicacion, fechaInicio, fechaFin, 500, "organizador-001");
        evento.Publicar();
        evento.RegistrarAsistente("usuario-001", "Creonte", "cdlara@est.ucab.edu.ve");

        // Actuar
        var lleno = evento.EstaCompleto;

        // Comprobar
        lleno.Should().BeFalse();
    }
}

// ========== Pruebas de EventoAggregateTests.cs ==========

public class EventoAggregateTests
{
    private readonly Ubicacion _ubicBase;
    private readonly DateTime _inicioBase;
    private readonly DateTime _finBase;

    public EventoAggregateTests()
    {
        _ubicBase = new Ubicacion("Lugar", "Direccion", "Ciudad", "Region", "0000", "Pais");
        _inicioBase = DateTime.UtcNow.AddDays(5);
        _finBase = _inicioBase.AddDays(1);
    }

    private Evento Build(int max = 10, DateTime? inicio = null, DateTime? fin = null, string titulo = "Titulo", string desc = "Descripcion", string org = "organizador-001")
        => new(titulo, desc, _ubicBase, inicio ?? _inicioBase, fin ?? _finBase, max, org);

    [Fact]
    public void Constructor_ParametrosValidos_CreaEnBorrador()
    {
        var ev = Build();
        ev.Estado.Should().Be(EstadoEvento.Borrador);
        ev.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_TituloVacio_LanzaExcepcion()
    {
        Action act = () => Build(titulo: "", inicio: DateTime.UtcNow.AddDays(2), fin: DateTime.UtcNow.AddDays(3));
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_DescripcionVacia_LanzaExcepcion()
    {
        Action act = () => Build(desc: "", inicio: DateTime.UtcNow.AddDays(2), fin: DateTime.UtcNow.AddDays(3));
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_UbicacionNula_LanzaExcepcion()
    {
        Action act = () => new Evento("Titulo", "Descripcion", null!, DateTime.UtcNow.AddDays(2), DateTime.UtcNow.AddDays(3), 10, "org");
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_FechaInicioPasada_LanzaExcepcion()
    {
        Action act = () => new Evento("Titulo", "Descripcion", _ubicBase, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddDays(1), 10, "org");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_FechaFinAnteriorALaInicio_LanzaExcepcion()
    {
        var inicio = DateTime.UtcNow.AddDays(2);
        Action act = () => new Evento("Titulo", "Descripcion", _ubicBase, inicio, inicio, 10, "org");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_MaximoAsistentesCero_LanzaExcepcion()
    {
        Action act = () => new Evento("Titulo", "Descripcion", _ubicBase, DateTime.UtcNow.AddDays(2), DateTime.UtcNow.AddDays(3), 0, "org");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_OrganizadorVacio_LanzaExcepcion()
    {
        Action act = () => new Evento("Titulo", "Descripcion", _ubicBase, DateTime.UtcNow.AddDays(2), DateTime.UtcNow.AddDays(3), 10, "");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Publicar_DesdeBorrador_GeneraEventoDominio()
    {
        var ev = Build();
        ev.Publicar();
        ev.Estado.Should().Be(EstadoEvento.Publicado);
        ev.EventosDominio.Should().ContainSingle().Which.Should().BeOfType<EventoPublicadoEventoDominio>();
    }

    [Fact]
    public void Publicar_NoEnBorrador_LanzaExcepcion()
    {
        var ev = Build();
        ev.Publicar();
        Action act = () => ev.Publicar();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Cancelar_Publicado_GeneraEventoDominio()
    {
        var ev = Build();
        ev.Publicar();
        ev.Cancelar();
        ev.Estado.Should().Be(EstadoEvento.Cancelado);
        ev.EventosDominio.Should().Contain(e => e is EventoCanceladoEventoDominio);
    }

    [Fact]
    public void RegistrarAsistente_NoPublicado_LanzaExcepcion()
    {
        var ev = Build();
        Action act = () => ev.RegistrarAsistente("u1", "Nombre", "correo@demo.com");
        act.Should().Throw<InvalidOperationException>().WithMessage("*no publicado*");
    }

    [Fact]
    public void RegistrarAsistente_Publicado_GeneraEventoDominioYIncrementaConteo()
    {
        var ev = Build();
        ev.Publicar();
        ev.RegistrarAsistente("u1", "Nombre", "a@b.com");
        ev.ConteoAsistentesActual.Should().Be(1);
        ev.EventosDominio.Should().Contain(e => e is AsistenteRegistradoEventoDominio);
    }

    [Fact]
    public void RegistrarAsistente_Completo_LanzaExcepcion()
    {
        var ev = Build(1);
        ev.Publicar();
        ev.RegistrarAsistente("u1", "Nombre", "a@b.com");
        Action act = () => ev.RegistrarAsistente("u2", "Nombre2", "c@d.com");
        act.Should().Throw<InvalidOperationException>().WithMessage("*completo*");
    }

    [Fact]
    public void RegistrarAsistente_Duplicado_LanzaExcepcion()
    {
        var ev = Build();
        ev.Publicar();
        ev.RegistrarAsistente("u1", "Nombre", "a@b.com");
        Action act = () => ev.RegistrarAsistente("u1", "Nombre", "a@b.com");
        act.Should().Throw<InvalidOperationException>().WithMessage("*ya est*");
    }

    [Fact]
    public void Actualizar_ReduciendoMaximoPorDebajoDelConteo_LanzaExcepcion()
    {
        var ev = Build(3);
        ev.Publicar();
        ev.RegistrarAsistente("u1", "Nombre", "a@b.com");
        ev.RegistrarAsistente("u2", "Nombre2", "b@c.com");
        Action act = () => ev.Actualizar("Titulo2", "Desc", _ubicBase, ev.FechaInicio, ev.FechaFin, 1);
        act.Should().Throw<ArgumentException>().WithMessage("*reducir*");
    }

    [Fact]
    public void LimpiarEventosDominio_DespuesDePublicar_DejaListaVacia()
    {
        var ev = Build();
        ev.Publicar();
        ev.EventosDominio.Should().NotBeEmpty();
        ev.LimpiarEventosDominio();
        ev.EventosDominio.Should().BeEmpty();
    }

    [Fact]
    public void RegistrarAsistente_EventoCancelado_LanzaExcepcion()
    {
        var ev = Build();
        ev.Publicar();
        ev.Cancelar();
        Action act = () => ev.RegistrarAsistente("u1", "Nombre", "correo@demo.com");
        act.Should().Throw<InvalidOperationException>().WithMessage("*cancelado*");
    }

    [Fact]
    public void AnularRegistroAsistente_Exitoso_CubreLinea122()
    {
        var ev = Build();
        ev.Publicar();
        ev.RegistrarAsistente("u1", "Nombre", "a@b.com");
        ev.ConteoAsistentesActual.Should().Be(1);
        ev.AnularRegistroAsistente("u1");
        ev.ConteoAsistentesActual.Should().Be(0);
    }
}

// ========== Pruebas de EventoStatusTests.cs ==========

public class EventoStatusTests
{
    [Fact]
    public void EventoStatus_DeberiaTenerValorDraft()
    {
        // Actuar
        var status = EstadoEvento.Borrador;

        // Comprobar
        status.Should().Be(EstadoEvento.Borrador);
        ((int)status).Should().Be(0);
    }

    [Fact]
    public void EventoStatus_DeberiaTenerValorPublicado()
    {
        // Actuar
        var status = EstadoEvento.Publicado;

        // Comprobar
        status.Should().Be(EstadoEvento.Publicado);
        ((int)status).Should().Be(1);
    }

    [Fact]
    public void EventoStatus_DeberiaTenerValorCancelado()
    {
        // Actuar
        var status = EstadoEvento.Cancelado;

        // Comprobar
        status.Should().Be(EstadoEvento.Cancelado);
        ((int)status).Should().Be(2);
    }

    [Fact]
    public void EventoStatus_DeberiaTenerValorCompletado()
    {
        // Actuar
        var status = EstadoEvento.Completado;

        // Comprobar
        status.Should().Be(EstadoEvento.Completado);
        ((int)status).Should().Be(3);
    }

    [Theory]
    [InlineData(EstadoEvento.Borrador, "Borrador")]
    [InlineData(EstadoEvento.Publicado, "Publicado")]
    [InlineData(EstadoEvento.Cancelado, "Cancelado")]
    [InlineData(EstadoEvento.Completado, "Completado")]
    public void EventoStatus_DeberiaConvertirAString(EstadoEvento status, string expected)
    {
        // Actuar
        var result = status.ToString();

        // Comprobar
        result.Should().Be(expected);
    }

    [Fact]
    public void EventoStatus_DeberiaPermitirComparacion()
    {
        // Preparar
        var draft = EstadoEvento.Borrador;
        var published = EstadoEvento.Publicado;

        // Actuar & Comprobar
        draft.Should().NotBe(published);
        draft.Should().Be(EstadoEvento.Borrador);
    }

    [Fact]
    public void EventoStatus_DeberiaSoportarTodosLosValores()
    {
        // Preparar & Actuar
        var values = Enum.GetValues<EstadoEvento>();

        // Comprobar
        values.Should().HaveCount(4);
        values.Should().Contain(EstadoEvento.Borrador);
        values.Should().Contain(EstadoEvento.Publicado);
        values.Should().Contain(EstadoEvento.Cancelado);
        values.Should().Contain(EstadoEvento.Completado);
    }
}

// ========== Pruebas de EventoExtraBranchesTests.cs ==========

public class EventoBranchesTests
{
    private readonly Ubicacion _ubic;
    private readonly DateTime _inicio;
    private readonly DateTime _fin;

    public EventoBranchesTests()
    {
        _ubic = new Ubicacion("Lugar", "Dir", "Ciudad", "Reg", "0000", "Pais");
        _inicio = DateTime.UtcNow.AddDays(2);
        _fin = _inicio.AddDays(1);
    }

    private Evento Build() => new("Titulo", "Desc", _ubic, _inicio, _fin, 5, "org-1");

    [Fact]
    public void Cancelar_YaCancelado_LanzaExcepcion()
    {
        var ev = Build();
        ev.Publicar();
        ev.Cancelar();
        Action act = () => ev.Cancelar();
        act.Should().Throw<InvalidOperationException>().WithMessage("*ya est*cancelad*");
    }

    [Fact]
    public void Cancelar_Completado_LanzaExcepcion()
    {
        var ev = Build();
        ev.Publicar();
        typeof(Evento).GetProperty("Estado")!.SetValue(ev, EstadoEvento.Completado);
        Action act = () => ev.Cancelar();
        act.Should().Throw<InvalidOperationException>().WithMessage("*completado*");
    }

    [Fact]
    public void Actualizar_EventoCancelado_LanzaExcepcion()
    {
        var ev = Build();
        ev.Publicar();
        ev.Cancelar();
        Action act = () => ev.Actualizar("T2", "D2", _ubic, ev.FechaInicio, ev.FechaFin, 5);
        act.Should().Throw<InvalidOperationException>().WithMessage("*cancelad*");
    }

    [Fact]
    public void Actualizar_EventoCompletado_LanzaExcepcion()
    {
        var ev = Build();
        ev.Publicar();
        typeof(Evento).GetProperty("Estado")!.SetValue(ev, EstadoEvento.Completado);
        Action act = () => ev.Actualizar("T2", "D2", _ubic, ev.FechaInicio, ev.FechaFin, 5);
        act.Should().Throw<InvalidOperationException>().WithMessage("*completad*");
    }

    [Fact]
    public void Actualizar_TituloVacio_LanzaExcepcion()
    {
        var ev = Build();
        Action act = () => ev.Actualizar(" ", "D2", _ubic, ev.FechaInicio, ev.FechaFin, 5);
        act.Should().Throw<ArgumentException>().WithMessage("*t*tulo*");
    }

    [Fact]
    public void AnularRegistro_NoExiste_LanzaExcepcion()
    {
        var ev = Build();
        ev.Publicar();
        Action act = () => ev.AnularRegistroAsistente("u1");
        act.Should().Throw<InvalidOperationException>().WithMessage("*no est*registrad*");
    }
}

// ========== Pruebas de EventoPrivateCtorTests.cs ==========

public class EventoPrivateCtorTests
{
    [Fact]
    public void PrivateParameterlessConstructor_CreaInstancia()
    {
        var ctor = typeof(Evento).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
        ctor.Should().NotBeNull();
        var inst = (Evento)ctor!.Invoke(null);
        inst.Should().NotBeNull();
        inst.Titulo.Should().Be(string.Empty);
    }

    [Fact]
    public void BuildEventoHelper_CreaValido()
    {
        var ev = TestHelpers.BuildEvento();
        ev.Titulo.Should().Be("Titulo");
        ev.EstaPublicado.Should().BeFalse();
    }
}
