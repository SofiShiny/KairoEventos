using Moq;
using Microsoft.AspNetCore.SignalR;
using Notificaciones.Infraestructura.Hubs;
using Notificaciones.Infraestructura.Servicios;
using FluentAssertions;
using Xunit;

namespace Notificaciones.Pruebas.Infraestructura.Servicios;

public class NotificadorSignalRTests
{
    [Fact]
    public async Task EnviarMensajeUsuarioAsync_DebeLlamarASendAsyncEnElGrupoCorrecto()
    {
        // Arrange
        var usuarioId = "user-123";
        var mensaje = "test message";
        var mockHubContext = new Mock<IHubContext<NotificacionesHub>>();
        var mockClients = new Mock<IHubClients>();
        var mockClientProxy = new Mock<IClientProxy>();

        mockHubContext.Setup(h => h.Clients).Returns(mockClients.Object);
        mockClients.Setup(c => c.Group(usuarioId)).Returns(mockClientProxy.Object);

        var notificador = new NotificadorSignalR(mockHubContext.Object);

        // Act
        await notificador.EnviarMensajeUsuarioAsync(usuarioId, mensaje);

        // Assert
        mockClientProxy.Verify(
            p => p.SendCoreAsync(
                "RecibirNotificacion",
                It.Is<object[]>(o => o.Length == 1 && (string)o[0] == mensaje),
                default),
            Times.Once);
    }
}
