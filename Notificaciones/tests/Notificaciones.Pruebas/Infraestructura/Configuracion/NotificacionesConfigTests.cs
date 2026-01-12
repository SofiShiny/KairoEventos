using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Notificaciones.Infraestructura.Configuracion;
using Notificaciones.Dominio.Interfaces;
using FluentAssertions;
using Xunit;
using Moq;

namespace Notificaciones.Pruebas.Infraestructura.Configuracion;

public class NotificacionesConfigTests
{
    [Fact]
    public void AddNotificationServices_DebeRegistrarServicios()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSignalR(); // Necesario para HubContext
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
                {"Smtp:Host", "test"}
            })
            .Build();

        // Act
        services.AddSingleton<IConfiguration>(config);
        services.AddNotificationServices(config);

        // Assert
        var provider = services.BuildServiceProvider();
        provider.GetService<IServicioEmail>().Should().NotBeNull();
        provider.GetService<INotificadorRealTime>().Should().NotBeNull();
        provider.GetService<IServicioUsuarios>().Should().NotBeNull();
    }

    [Fact]
    public void AddMassTransitConfiguration_DebeRegistrarBus()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder().Build();

        // Act
        services.AddMassTransitConfiguration(config);

        // Assert
        var provider = services.BuildServiceProvider();
        // MassTransit registra IBus de forma predeterminada
        provider.GetService<IBusControl>().Should().NotBeNull();
    }

    [Fact]
    public async Task ConfigureJwtEvents_OnMessageReceived_DebeExtraerTokenDeQueryString()
    {
        // Arrange
        var options = new JwtBearerOptions();
        NotificacionesConfig.ConfigureJwtEvents(options);
        
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/notificacionesHub";
        httpContext.Request.QueryString = new QueryString("?access_token=secret-token");
        
        var context = new MessageReceivedContext(httpContext, new AuthenticationScheme("Bearer", null, typeof(JwtBearerHandler)), options);

        // Act
        if (options.Events?.OnMessageReceived != null)
        {
            await options.Events.OnMessageReceived(context);
        }

        // Assert
        context.Token.Should().Be("secret-token");
    }

    [Fact]
    public async Task ConfigureJwtEvents_OnMessageReceived_SinToken_NoDebeHacerNada()
    {
        // Arrange
        var options = new JwtBearerOptions();
        NotificacionesConfig.ConfigureJwtEvents(options);
        
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/notificacionesHub";
        
        var context = new MessageReceivedContext(httpContext, new AuthenticationScheme("Bearer", null, typeof(JwtBearerHandler)), options);

        // Act
        if (options.Events?.OnMessageReceived != null)
        {
            await options.Events.OnMessageReceived(context);
        }

        // Assert
        context.Token.Should().BeNull();
    }

    [Fact]
    public async Task ConfigureJwtEvents_OnMessageReceived_TokenVacio_NoDebeHacerNada()
    {
        // Arrange
        var options = new JwtBearerOptions();
        NotificacionesConfig.ConfigureJwtEvents(options);
        
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/notificacionesHub";
        httpContext.Request.QueryString = new QueryString("?access_token=");
        
        var context = new MessageReceivedContext(httpContext, new AuthenticationScheme("Bearer", null, typeof(JwtBearerHandler)), options);

        // Act
        if (options.Events?.OnMessageReceived != null)
        {
            await options.Events.OnMessageReceived(context);
        }

        // Assert
        context.Token.Should().BeNull();
    }

    [Fact]
    public async Task ConfigureJwtEvents_OnMessageReceived_PathIncorrecto_NoDebeExtraer()
    {
        // Arrange
        var options = new JwtBearerOptions();
        NotificacionesConfig.ConfigureJwtEvents(options);
        
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/otroPath";
        httpContext.Request.QueryString = new QueryString("?access_token=token");
        
        var context = new MessageReceivedContext(httpContext, new AuthenticationScheme("Bearer", null, typeof(JwtBearerHandler)), options);

        // Act
        if (options.Events?.OnMessageReceived != null)
        {
            await options.Events.OnMessageReceived(context);
        }

        // Assert
        context.Token.Should().BeNull();
    }
}
