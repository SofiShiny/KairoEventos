using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Configuration;
using System.Text.Json;
using Usuarios.API.Middlewares;
using Usuarios.Application.Dtos;
using Usuarios.Application.Dtos.UsuarioKeycloakDto;
using Usuarios.Core.Date;
using Usuarios.Core.Repository;
using Usuarios.Core.Services;
using Usuarios.Domain.Entidades;
using Usuarios.Infrastructure.Date;
using Usuarios.Infrastructure.Keycloak;

namespace Usuarios.Test.Infrastructure.Keycloak;

public class TestTokenBackGround
{
    private string tokenstrVar;
    private string token;
    private Mock<IWebRequest> webRequest;
    private Mock<IConfiguration> configuracion;
    private Mock<IServiceProvider> serviceProvider;
    private Mock<IDateTime> dateTime;
    private Mock<IAccesManagement<UsuarioKeycloak>> accEsManagement;
    private Mock<IServiceScopeFactory> factory;
    private Mock<IServiceScope> scope;
    private TokenBackGround tokenBackGround;
    private CancellationTokenSource tokencan;
    private Mock<ILogger<TokenBackGround>> logger;

    public TestTokenBackGround()
    {
        tokenstrVar = "test";
        token = JsonSerializer.Serialize(new { access_token = "test" });
        webRequest = new();
        configuracion = new();
        serviceProvider = new();
        dateTime = new();
        accEsManagement = new();
        factory = new Mock<IServiceScopeFactory>();
        scope = new Mock<IServiceScope>();

        logger = new();
        tokenBackGround = new TokenBackGround(webRequest.Object, configuracion.Object, serviceProvider.Object, dateTime.Object, logger.Object);
        tokencan = new CancellationTokenSource();

    }

    [Fact]
    public async Task Test_TokenBackground_RefrescarTokenExitoso()
    {
        webRequest.Setup(w => w.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>(), It.IsAny<Dictionary<string, string>>())).ReturnsAsync(token);

        dateTime.SetupSequence(d => d.Now).Returns(new DateTime(2025, 11, 4)).Returns(new DateTime(2025, 11, 4));

        accEsManagement.Setup(a => a.SetToken(It.IsAny<string>()));

        scope.Setup(s => s.ServiceProvider).Returns(serviceProvider.Object);
        factory.Setup(f => f.CreateScope()).Returns(scope.Object);
        serviceProvider.Setup(s => s.GetService(typeof(IServiceScopeFactory))).Returns(factory.Object);
        serviceProvider.Setup(s => s.GetService(typeof(IAccesManagement<UsuarioKeycloak>))).Returns(accEsManagement.Object);

        await tokenBackGround.StartAsync(tokencan.Token);
        tokencan.Cancel();
        await tokenBackGround.StopAsync(tokencan.Token);

        accEsManagement.Verify(a => a.SetToken(tokenstrVar),Times.Once);
    }
}