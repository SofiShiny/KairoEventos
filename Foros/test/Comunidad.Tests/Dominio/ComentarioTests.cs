using Comunidad.Domain.Entidades;
using FluentAssertions;
using Xunit;

namespace Comunidad.Tests.Dominio;

public class ComentarioTests
{
    [Fact]
    public void Crear_InicializaConEsVisibleTrue()
    {
        // Arrange
        var foroId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var contenido = "Comentario de prueba";

        // Act
        var comentario = Comentario.Crear(foroId, usuarioId, contenido);

        // Assert
        comentario.Id.Should().NotBeEmpty();
        comentario.ForoId.Should().Be(foroId);
        comentario.UsuarioId.Should().Be(usuarioId);
        comentario.Contenido.Should().Be(contenido);
        comentario.EsVisible.Should().BeTrue();
        comentario.FechaCreacion.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        comentario.Respuestas.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_InicializaPropiedadesCorrectamente()
    {
        // Act
        var comentario = new Comentario();

        // Assert
        comentario.Id.Should().NotBeEmpty();
        comentario.EsVisible.Should().BeTrue();
        comentario.FechaCreacion.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        comentario.Respuestas.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void AgregarRespuesta_AgregaRespuestaALista()
    {
        // Arrange
        var comentario = Comentario.Crear(Guid.NewGuid(), Guid.NewGuid(), "Comentario");
        var usuarioRespuesta = Guid.NewGuid();
        var contenidoRespuesta = "Esta es una respuesta";

        // Act
        comentario.AgregarRespuesta(usuarioRespuesta, contenidoRespuesta);

        // Assert
        comentario.Respuestas.Should().HaveCount(1);
        comentario.Respuestas[0].UsuarioId.Should().Be(usuarioRespuesta);
        comentario.Respuestas[0].Contenido.Should().Be(contenidoRespuesta);
        comentario.Respuestas[0].FechaCreacion.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void AgregarRespuesta_PermiteMultiplesRespuestas()
    {
        // Arrange
        var comentario = Comentario.Crear(Guid.NewGuid(), Guid.NewGuid(), "Comentario");

        // Act
        comentario.AgregarRespuesta(Guid.NewGuid(), "Respuesta 1");
        comentario.AgregarRespuesta(Guid.NewGuid(), "Respuesta 2");
        comentario.AgregarRespuesta(Guid.NewGuid(), "Respuesta 3");

        // Assert
        comentario.Respuestas.Should().HaveCount(3);
        comentario.Respuestas[0].Contenido.Should().Be("Respuesta 1");
        comentario.Respuestas[1].Contenido.Should().Be("Respuesta 2");
        comentario.Respuestas[2].Contenido.Should().Be("Respuesta 3");
    }

    [Fact]
    public void Ocultar_CambiaEsVisibleAFalse()
    {
        // Arrange
        var comentario = Comentario.Crear(Guid.NewGuid(), Guid.NewGuid(), "Comentario");

        // Act
        comentario.Ocultar();

        // Assert
        comentario.EsVisible.Should().BeFalse();
    }

    [Fact]
    public void Ocultar_LlamadoMultiplesVeces_MantieneFalse()
    {
        // Arrange
        var comentario = Comentario.Crear(Guid.NewGuid(), Guid.NewGuid(), "Comentario");

        // Act
        comentario.Ocultar();
        comentario.Ocultar();
        comentario.Ocultar();

        // Assert
        comentario.EsVisible.Should().BeFalse();
    }

    [Fact]
    public void Crear_ConContenidoVacio_CreaComentarioConContenidoVacio()
    {
        // Arrange
        var foroId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();

        // Act
        var comentario = Comentario.Crear(foroId, usuarioId, "");

        // Assert
        comentario.Contenido.Should().BeEmpty();
        comentario.EsVisible.Should().BeTrue();
    }

    [Fact]
    public void AgregarRespuesta_ConContenidoVacio_AgregaRespuestaVacia()
    {
        // Arrange
        var comentario = Comentario.Crear(Guid.NewGuid(), Guid.NewGuid(), "Comentario");

        // Act
        comentario.AgregarRespuesta(Guid.NewGuid(), "");

        // Assert
        comentario.Respuestas.Should().HaveCount(1);
        comentario.Respuestas[0].Contenido.Should().BeEmpty();
    }

    [Fact]
    public void Crear_GeneraIdsUnicos()
    {
        // Arrange
        var foroId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();

        // Act
        var comentario1 = Comentario.Crear(foroId, usuarioId, "Comentario 1");
        var comentario2 = Comentario.Crear(foroId, usuarioId, "Comentario 2");

        // Assert
        comentario1.Id.Should().NotBe(comentario2.Id);
    }
}
