using Moq;
using Microsoft.AspNetCore.SignalR;
using Notificaciones.Infraestructura.Hubs;
using System.Security.Claims;
using FluentAssertions;
using Xunit;

namespace Notificaciones.Pruebas.Infraestructura.Hubs;

public class NotificacionesHubTests
{
    [Fact]
    public async Task OnConnectedAsync_CuandoUsuarioEstaAutenticado_DebeUnirseAGrupo()
    {
        // Arrange
        var userId = "user-123";
        var mockHubContext = new Mock<HubCallerContext>();
        var mockGroups = new Mock<IGroupManager>();
        
        // Setup User con su ID
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);
        
        mockHubContext.Setup(c => c.User).Returns(user);
        mockHubContext.Setup(c => c.ConnectionId).Returns("conn-123");

        var hub = new NotificacionesHub
        {
            Context = mockHubContext.Object,
            Groups = mockGroups.Object
        };

        // Act
        await hub.OnConnectedAsync();

        // Assert
        mockGroups.Verify(g => g.AddToGroupAsync("conn-123", userId, default), Times.Once);
    }

    [Fact]
    public async Task OnConnectedAsync_CuandoUserIdentifierEstaSeteado_DebeUsarlo()
    {
        // Arrange
        var userId = "direct-id";
        var mockHubContext = new Mock<HubCallerContext>();
        var mockGroups = new Mock<IGroupManager>();

        mockHubContext.Setup(c => c.UserIdentifier).Returns(userId);
        mockHubContext.Setup(c => c.ConnectionId).Returns("conn-456");

        var hub = new NotificacionesHub
        {
            Context = mockHubContext.Object,
            Groups = mockGroups.Object
        };

        // Act
        await hub.OnConnectedAsync();

        // Assert
        mockGroups.Verify(g => g.AddToGroupAsync("conn-456", userId, default), Times.Once);
    }

    [Fact]
    public async Task OnConnectedAsync_CuandoNoHayUserIdPeroHayClaim_DebeUsarClaim()
    {
        // Arrange
        var userId = "claim-id";
        var mockHubContext = new Mock<HubCallerContext>();
        var mockGroups = new Mock<IGroupManager>();

        // UserIdentifier es null
        mockHubContext.Setup(c => c.UserIdentifier).Returns((string?)null);
        
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);
        mockHubContext.Setup(c => c.User).Returns(user);
        mockHubContext.Setup(c => c.ConnectionId).Returns("conn-789");

        var hub = new NotificacionesHub
        {
            Context = mockHubContext.Object,
            Groups = mockGroups.Object
        };

        // Act
        await hub.OnConnectedAsync();

        // Assert
        mockGroups.Verify(g => g.AddToGroupAsync("conn-789", userId, default), Times.Once);
    }

    [Fact]
    public async Task OnConnectedAsync_CuandoNoHayUsuario_NoDebeHacerNada()
    {
        // Arrange
        var mockHubContext = new Mock<HubCallerContext>();
        var mockGroups = new Mock<IGroupManager>();

        mockHubContext.Setup(c => c.UserIdentifier).Returns((string?)null);
        mockHubContext.Setup(c => c.User).Returns((ClaimsPrincipal?)null); // User a null
        mockHubContext.Setup(c => c.ConnectionId).Returns("conn-999");

        var hub = new NotificacionesHub
        {
            Context = mockHubContext.Object,
            Groups = mockGroups.Object
        };

        // Act
        await hub.OnConnectedAsync();

        // Assert
        mockGroups.Verify(g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), default), Times.Never);
    }
    [Fact]
    public async Task OnConnectedAsync_CuandoUserNoTieneClaim_NoDebeHacerNada()
    {
        // Arrange
        var mockHubContext = new Mock<HubCallerContext>();
        var mockGroups = new Mock<IGroupManager>();

        mockHubContext.Setup(c => c.UserIdentifier).Returns((string?)null);
        // User está presente pero no tiene el claim buscado
        var identity = new ClaimsIdentity(new Claim[] { }, "TestAuth");
        var user = new ClaimsPrincipal(identity);
        mockHubContext.Setup(c => c.User).Returns(user);
        mockHubContext.Setup(c => c.ConnectionId).Returns("conn-000");

        var hub = new NotificacionesHub
        {
            Context = mockHubContext.Object,
            Groups = mockGroups.Object
        };

        // Act
        await hub.OnConnectedAsync();

        // Assert
        mockGroups.Verify(g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), default), Times.Never);
    }
}
