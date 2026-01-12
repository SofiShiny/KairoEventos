using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using System.Text.Json;
using Usuarios.Application.Dtos.UsuarioKeycloakDto;
using Usuarios.Core.Services;
using Usuarios.Domain.Entidades;
using Usuarios.Infrastructure.Keycloak;

namespace Usuarios.Test.Infrastructure.Keycloak;

public class TestAccessManagementKeycloak
{
    private Guid idO;
    private string respuestaJson;
    private string respuestaJsonTwo;
    private string respuestaJsonThree;
    private UsuarioKeycloak usuario;
    private Mock<IWebRequest> webRequest;
    private AccessManagementKeycloak accessManagement;
    private Mock<ILogger<AccessManagementKeycloak>> logger;
    private string usuario_json;

    public TestAccessManagementKeycloak()
    {
        idO = Guid.NewGuid();
        respuestaJson = JsonSerializer.Serialize(new[] { new { id = "test" } });
        respuestaJsonTwo = JsonSerializer.Serialize(new { id = "test" });
        respuestaJsonThree = JsonSerializer.Serialize(new[] { new { id = idO } });
        logger = new();
        usuario = new()
        {
            Username = "test",
            Email = "test@gmail.com",
            Attributes = new Atributos()
            {
                Address = new List<string>() { "test" }, Name = new List<string>() { "test" },
                Number = new List<string>() { "12345678910" }
            },
            Credentials = new List<Credenciales>() { new Credenciales() { Value = "Test123456789*" } }
        };
        webRequest = new Mock<IWebRequest>();
        accessManagement = new AccessManagementKeycloak(webRequest.Object,logger.Object);

        usuario_json = @"
        {
            ""username"": ""test"",
            ""email"": ""test@gmail.com"",
            ""name"": ""test"",
            ""attributes"": 
            {
                ""telefono"": [""test""],
                ""direccion"": [""test""]
            }
        }";
    }


    [Fact]
    public async Task Test_AgregarUsuario_RetornaIdEsperado()
    {
        webRequest.SetupSequence(w => w.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>(), It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync(String.Empty).ReturnsAsync(String.Empty);
        webRequest.SetupSequence(w => w.GetAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync(respuestaJson.ToString).ReturnsAsync(respuestaJsonTwo.ToString).ReturnsAsync(respuestaJsonThree.ToString);

        var result = await accessManagement.AgregarUsuario(usuario, It.IsAny<string>());

        Assert.Equal(idO, result);
    }

    [Fact]
    public async Task Test_ModificarUsuario_RetornaIdEsperado()
    {
        webRequest.Setup(w => w.PutAsync(It.IsAny<string>(), It.IsAny<HttpContent>(), It.IsAny<Dictionary<string, string>>())).ReturnsAsync("Completed");

        await accessManagement.ModificarUsuario(usuario, It.IsAny<Guid>());

        webRequest.Verify(w => w.PutAsync(It.IsAny<string>(), It.IsAny<HttpContent>(), It.IsAny<Dictionary<string, string>>()), Times.Once);
    }

    [Fact]
    public async Task Test_EliminarUsuario_RetornaIdEsperado()
    {
        webRequest.Setup(w => w.DeleteAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>())).ReturnsAsync("Completed");

        await accessManagement.EliminarUsuario(It.IsAny<Guid>());

        webRequest.Verify(w => w.DeleteAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()), Times.Once);

    }
    [Fact]
    public async Task Test_ConsultarUsuarioPorId_RetornaElUsuarioDelTipoUsuarioKeycloak()
    {
        webRequest.Setup(w => w.GetAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>())).ReturnsAsync(usuario_json);

        await accessManagement.ConsultarUsuarioPorId(It.IsAny<string>());

        webRequest.Verify(w => w.GetAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()), Times.Once);
    }
}