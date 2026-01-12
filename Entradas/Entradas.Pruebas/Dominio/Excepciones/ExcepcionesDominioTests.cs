using Entradas.Dominio.Excepciones;
using FluentAssertions;
using Xunit;

namespace Entradas.Pruebas.Dominio.Excepciones;

public class ExcepcionesDominioTests
{
    [Fact]
    public void EntradaNoEncontradaException_ConId_DebeCrearMensajeCorrectamente()
    {
        // Arrange
        var id = Guid.NewGuid();
        var mensaje = "Entrada no encontrada";

        // Act
        var excepcion = new EntradaNoEncontradaException(id, mensaje);

        // Assert
        excepcion.Message.Should().Be(mensaje);
        excepcion.EntradaId.Should().Be(id);
    }

    [Fact]
    public void AsientoNoDisponibleException_ConMensaje_DebeCrearCorrectamente()
    {
        // Arrange
        var mensaje = "Asiento no disponible";

        // Act
        var excepcion = new AsientoNoDisponibleException(mensaje);

        // Assert
        excepcion.Message.Should().Be(mensaje);
    }

    [Fact]
    public void AsientoNoDisponibleException_ConEventoYAsiento_DebeCrearCorrectamente()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var asientoId = Guid.NewGuid();
        var mensaje = "Asiento no disponible";

        // Act
        var excepcion = new AsientoNoDisponibleException(eventoId, asientoId, mensaje);

        // Assert
        excepcion.Message.Should().Be(mensaje);
        excepcion.EventoId.Should().Be(eventoId);
        excepcion.AsientoId.Should().Be(asientoId);
    }

    [Fact]
    public void EstadoEntradaInvalidoException_ConMensaje_DebeCrearCorrectamente()
    {
        // Arrange
        var mensaje = "Estado de entrada inválido";

        // Act
        var excepcion = new EstadoEntradaInvalidoException(mensaje);

        // Assert
        excepcion.Message.Should().Be(mensaje);
    }

    [Fact]
    public void EventoNoDisponibleException_ConEventoId_DebeCrearMensajeCorrectamente()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var mensaje = "Evento no disponible";

        // Act
        var excepcion = new EventoNoDisponibleException(eventoId, mensaje);

        // Assert
        excepcion.Message.Should().Be(mensaje);
        excepcion.EventoId.Should().Be(eventoId);
    }

    [Fact]
    public void ServicioExternoNoDisponibleException_ConServicio_DebeCrearMensajeCorrectamente()
    {
        // Arrange
        var nombreServicio = "VerificadorAsientos";
        var mensaje = "Servicio externo no disponible";

        // Act
        var excepcion = new ServicioExternoNoDisponibleException(nombreServicio, mensaje);

        // Assert
        excepcion.Message.Should().Be(mensaje);
        excepcion.NombreServicio.Should().Be(nombreServicio);
    }
}