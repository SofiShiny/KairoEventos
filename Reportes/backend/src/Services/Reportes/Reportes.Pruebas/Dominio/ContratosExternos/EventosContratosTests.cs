using FluentAssertions;
using Eventos.Dominio.EventosDeDominio;
using Xunit;

namespace Reportes.Pruebas.Dominio.ContratosExternos;

public class EventosContratosTests
{
    [Fact]
    public void EventoPublicadoEventoDominio_Constructor_InicializaPropiedadesCorrectamente()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var tituloEvento = "Concierto de Rock";
        var fechaInicio = DateTime.Now;

        // Act
        var evento = new EventoPublicadoEventoDominio
        {
            EventoId = eventoId,
            TituloEvento = tituloEvento,
            FechaInicio = fechaInicio
        };

        // Assert
        evento.EventoId.Should().Be(eventoId);
        evento.TituloEvento.Should().Be(tituloEvento);
        evento.FechaInicio.Should().Be(fechaInicio);
    }

    [Fact]
    public void AsistenteRegistradoEventoDominio_Constructor_InicializaPropiedadesCorrectamente()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var usuarioId = "user123";
        var nombreUsuario = "Juan Pérez";

        // Act
        var evento = new AsistenteRegistradoEventoDominio
        {
            EventoId = eventoId,
            UsuarioId = usuarioId,
            NombreUsuario = nombreUsuario
        };

        // Assert
        evento.EventoId.Should().Be(eventoId);
        evento.UsuarioId.Should().Be(usuarioId);
        evento.NombreUsuario.Should().Be(nombreUsuario);
    }

    [Fact]
    public void EventoCanceladoEventoDominio_Constructor_InicializaPropiedadesCorrectamente()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var tituloEvento = "Concierto Cancelado";

        // Act
        var evento = new EventoCanceladoEventoDominio
        {
            EventoId = eventoId,
            TituloEvento = tituloEvento
        };

        // Assert
        evento.EventoId.Should().Be(eventoId);
        evento.TituloEvento.Should().Be(tituloEvento);
    }

    [Theory]
    [InlineData("Concierto de Rock", "2024-12-31")]
    [InlineData("Festival de Jazz", "2025-06-15")]
    [InlineData("Obra de Teatro", "2025-03-20")]
    public void EventoPublicadoEventoDominio_DiferentesEventos_ManejaCorrectamente(string titulo, string fecha)
    {
        // Arrange
        var fechaInicio = DateTime.Parse(fecha);

        // Act
        var evento = new EventoPublicadoEventoDominio
        {
            TituloEvento = titulo,
            FechaInicio = fechaInicio
        };

        // Assert
        evento.TituloEvento.Should().Be(titulo);
        evento.FechaInicio.Should().Be(fechaInicio);
    }

    [Theory]
    [InlineData("user1", "Ana García")]
    [InlineData("user2", "Carlos López")]
    [InlineData("user3", "María Rodríguez")]
    public void AsistenteRegistradoEventoDominio_DiferentesUsuarios_ManejaCorrectamente(string usuarioId, string nombreUsuario)
    {
        // Act
        var evento = new AsistenteRegistradoEventoDominio
        {
            UsuarioId = usuarioId,
            NombreUsuario = nombreUsuario
        };

        // Assert
        evento.UsuarioId.Should().Be(usuarioId);
        evento.NombreUsuario.Should().Be(nombreUsuario);
    }

    [Fact]
    public void EventosContratos_PropiedadesNulas_PermiteValoresNulos()
    {
        // Act & Assert - EventoPublicadoEventoDominio
        var eventoPublicado = new EventoPublicadoEventoDominio 
        { 
            TituloEvento = null! 
        };
        eventoPublicado.TituloEvento.Should().BeNull();

        // Act & Assert - AsistenteRegistradoEventoDominio
        var asistenteRegistrado = new AsistenteRegistradoEventoDominio 
        { 
            UsuarioId = null!,
            NombreUsuario = null!
        };
        asistenteRegistrado.UsuarioId.Should().BeNull();
        asistenteRegistrado.NombreUsuario.Should().BeNull();

        // Act & Assert - EventoCanceladoEventoDominio
        var eventoCancelado = new EventoCanceladoEventoDominio 
        { 
            TituloEvento = null! 
        };
        eventoCancelado.TituloEvento.Should().BeNull();
    }

    [Fact]
    public void EventosContratos_PropiedadesDefault_TienenValoresEsperados()
    {
        // Act
        var eventoPublicado = new EventoPublicadoEventoDominio();
        var asistenteRegistrado = new AsistenteRegistradoEventoDominio();
        var eventoCancelado = new EventoCanceladoEventoDominio();

        // Assert
        eventoPublicado.EventoId.Should().Be(Guid.Empty);
        eventoPublicado.TituloEvento.Should().Be(string.Empty);
        eventoPublicado.FechaInicio.Should().Be(default);

        asistenteRegistrado.EventoId.Should().Be(Guid.Empty);
        asistenteRegistrado.UsuarioId.Should().Be(string.Empty);
        asistenteRegistrado.NombreUsuario.Should().Be(string.Empty);

        eventoCancelado.EventoId.Should().Be(Guid.Empty);
        eventoCancelado.TituloEvento.Should().Be(string.Empty);
    }

    [Fact]
    public void EventosContratos_Records_SonInmutables()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var titulo = "Evento Original";

        // Act
        var evento1 = new EventoPublicadoEventoDominio
        {
            EventoId = eventoId,
            TituloEvento = titulo
        };

        var evento2 = evento1 with { TituloEvento = "Evento Modificado" };

        // Assert
        evento1.TituloEvento.Should().Be(titulo);
        evento2.TituloEvento.Should().Be("Evento Modificado");
        evento1.EventoId.Should().Be(evento2.EventoId);
    }

    [Fact]
    public void EventosContratos_Igualdad_FuncionaCorrectamente()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var titulo = "Evento Test";

        var evento1 = new EventoPublicadoEventoDominio
        {
            EventoId = eventoId,
            TituloEvento = titulo
        };

        var evento2 = new EventoPublicadoEventoDominio
        {
            EventoId = eventoId,
            TituloEvento = titulo
        };

        // Act & Assert
        evento1.Should().Be(evento2);
        evento1.GetHashCode().Should().Be(evento2.GetHashCode());
    }

    [Fact]
    public void EventosContratos_FechasExtremas_ManejaCorrectamente()
    {
        // Arrange
        var fechaMinima = DateTime.MinValue;
        var fechaMaxima = DateTime.MaxValue;

        // Act
        var eventoMinimo = new EventoPublicadoEventoDominio
        {
            FechaInicio = fechaMinima
        };

        var eventoMaximo = new EventoPublicadoEventoDominio
        {
            FechaInicio = fechaMaxima
        };

        // Assert
        eventoMinimo.FechaInicio.Should().Be(fechaMinima);
        eventoMaximo.FechaInicio.Should().Be(fechaMaxima);
    }

    [Fact]
    public void EventosContratos_GuidsVacios_ManejaCorrectamente()
    {
        // Act
        var eventoConGuidVacio = new EventoPublicadoEventoDominio
        {
            EventoId = Guid.Empty
        };

        // Assert
        eventoConGuidVacio.EventoId.Should().Be(Guid.Empty);
    }
}