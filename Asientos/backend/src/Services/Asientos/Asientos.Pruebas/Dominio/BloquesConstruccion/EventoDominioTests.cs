using BloquesConstruccion.Dominio;
using System;
using Xunit;

namespace Asientos.Pruebas.Dominio.BloquesConstruccion;

// Clase concreta para probar la clase abstracta EventoDominio
file class EventoConcreto : EventoDominio
{
    public EventoConcreto(Guid idAgregado)
    {
        IdAgregado = idAgregado;
    }
}

public class EventoDominioTests
{
    [Fact]
    public void Creacion_DebeInicializarPropiedadesCorrectamente()
    {
        // Arrange
        var idAgregado = Guid.NewGuid();
        var ahora = DateTime.UtcNow;

        // Act
        var evento = new EventoConcreto(idAgregado);

        // Assert
        Assert.NotEqual(Guid.Empty, evento.Id);
        Assert.Equal(idAgregado, evento.IdAgregado);
        Assert.True(evento.OcurrioEn >= ahora);
    }
}
