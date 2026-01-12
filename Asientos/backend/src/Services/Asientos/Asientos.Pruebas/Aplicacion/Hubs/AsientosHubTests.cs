using Asientos.Aplicacion.Hubs;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Asientos.Pruebas.Aplicacion.Hubs;

public class AsientosHubTests
{
    [Fact]
    public async Task JoinEvento_AgregaAHubGroup()
    {
        // Arrange
        var mockGroups = new Mock<IGroupManager>();
        var mockContext = new Mock<HubCallerContext>();
        mockContext.Setup(c => c.ConnectionId).Returns("test-connection");
        
        var hub = new AsientosHub
        {
            Groups = mockGroups.Object,
            Context = mockContext.Object
        };
        
        var eventoId = "evento-123";

        // Act
        await hub.JoinEvento(eventoId);

        // Assert
        mockGroups.Verify(g => g.AddToGroupAsync("test-connection", $"Evento_{eventoId}", default), Times.Once);
    }

    [Fact]
    public async Task LeaveEvento_RemueveDeHubGroup()
    {
        // Arrange
        var mockGroups = new Mock<IGroupManager>();
        var mockContext = new Mock<HubCallerContext>();
        mockContext.Setup(c => c.ConnectionId).Returns("test-connection");
        
        var hub = new AsientosHub
        {
            Groups = mockGroups.Object,
            Context = mockContext.Object
        };
        
        var eventoId = "evento-123";

        // Act
        await hub.LeaveEvento(eventoId);

        // Assert
        mockGroups.Verify(g => g.RemoveFromGroupAsync("test-connection", $"Evento_{eventoId}", default), Times.Once);
    }
}
