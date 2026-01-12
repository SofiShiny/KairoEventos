using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Usuarios.Dominio.Entidades;
using Usuarios.Dominio.Enums;
using Usuarios.Dominio.ObjetosValor;
using Usuarios.Infraestructura.Servicios;

namespace Usuarios.Pruebas.Infraestructura
{
    public class ServicioKeycloakTests
    {
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly Mock<ILogger<ServicioKeycloak>> _loggerMock;
        private readonly ServicioKeycloak _servicio;

        public ServicioKeycloakTests()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            
            var configurationData = new Dictionary<string, string?>
            {
                { "Keycloak:Authority", "http://localhost:8080/realms/Kairo" },
                { "Keycloak:AdminUrl", "http://localhost:8080/admin/realms/Kairo" },
                { "Keycloak:ClientId", "admin-cli" },
                { "Keycloak:ClientSecret", "test-secret" }
            };
            
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configurationData)
                .Build();
            
            _loggerMock = new Mock<ILogger<ServicioKeycloak>>();
            _servicio = new ServicioKeycloak(_httpClient, _configuration, _loggerMock.Object);
        }

        [Fact]
        public async Task CrearUsuarioAsync_DebeEnviarRequestCorrectoAKeycloak()
        {
            // Arrange
            var correo = Correo.Crear("test@example.com");
            var telefono = Telefono.Crear("1234567890");
            var direccion = Direccion.Crear("Calle Test 123");
            
            var usuario = Usuario.Crear(
                "testuser",
                "Test User",
                correo,
                telefono,
                direccion,
                Rol.User
            );

            // Mock token response
            var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new
                {
                    access_token = "test-token",
                    token_type = "Bearer"
                }))
            };

            // Mock create user response
            var createUserResponse = new HttpResponseMessage(HttpStatusCode.Created);

            // Mock get role response
            var getRoleResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new
                {
                    id = "role-id-123",
                    name = "User"
                }))
            };

            // Mock assign role response
            var assignRoleResponse = new HttpResponseMessage(HttpStatusCode.NoContent);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.RequestUri!.ToString().Contains("/protocol/openid-connect/token")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(tokenResponse);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri!.ToString().EndsWith("/users")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(createUserResponse);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString().Contains("/roles/")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(getRoleResponse);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri!.ToString().Contains("/role-mappings/realm")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(assignRoleResponse);

            // Act
            var resultado = await _servicio.CrearUsuarioAsync(usuario, "password123");

            // Assert
            resultado.Should().Be(usuario.Id.ToString());
            
            _httpMessageHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.ToString().EndsWith("/users")),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task ActualizarUsuarioAsync_DebeEnviarRequestCorrecto()
        {
            // Arrange
            var correo = Correo.Crear("updated@example.com");
            var telefono = Telefono.Crear("9876543210");
            var direccion = Direccion.Crear("Nueva Calle 456");
            
            var usuario = Usuario.Crear(
                "updateduser",
                "Updated User",
                correo,
                telefono,
                direccion,
                Rol.Admin
            );

            // Mock token response
            var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new
                {
                    access_token = "test-token",
                    token_type = "Bearer"
                }))
            };

            // Mock update user response
            var updateUserResponse = new HttpResponseMessage(HttpStatusCode.NoContent);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.RequestUri!.ToString().Contains("/protocol/openid-connect/token")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(tokenResponse);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Put &&
                        req.RequestUri!.ToString().Contains($"/users/{usuario.Id}")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(updateUserResponse);

            // Act
            await _servicio.ActualizarUsuarioAsync(usuario);

            // Assert
            _httpMessageHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Put &&
                    req.RequestUri!.ToString().Contains($"/users/{usuario.Id}")),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task DesactivarUsuarioAsync_DebeEnviarRequestCorrecto()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();

            // Mock token response
            var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new
                {
                    access_token = "test-token",
                    token_type = "Bearer"
                }))
            };

            // Mock deactivate user response
            var deactivateUserResponse = new HttpResponseMessage(HttpStatusCode.NoContent);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.RequestUri!.ToString().Contains("/protocol/openid-connect/token")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(tokenResponse);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Put &&
                        req.RequestUri!.ToString().Contains($"/users/{usuarioId}")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(deactivateUserResponse);

            // Act
            await _servicio.DesactivarUsuarioAsync(usuarioId);

            // Assert
            _httpMessageHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Put &&
                    req.RequestUri!.ToString().Contains($"/users/{usuarioId}")),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task AsignarRolAsync_DebeEnviarRequestCorrecto()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();
            var rol = Rol.Organizator;

            // Mock token response
            var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new
                {
                    access_token = "test-token",
                    token_type = "Bearer"
                }))
            };

            // Mock get role response
            var getRoleResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new
                {
                    id = "organizator-role-id",
                    name = "Organizator"
                }))
            };

            // Mock assign role response
            var assignRoleResponse = new HttpResponseMessage(HttpStatusCode.NoContent);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.RequestUri!.ToString().Contains("/protocol/openid-connect/token")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(tokenResponse);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString().Contains("/roles/Organizator")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(getRoleResponse);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri!.ToString().Contains($"/users/{usuarioId}/role-mappings/realm")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(assignRoleResponse);

            // Act
            await _servicio.AsignarRolAsync(usuarioId, rol);

            // Assert
            _httpMessageHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.ToString().Contains($"/users/{usuarioId}/role-mappings/realm")),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task CrearUsuarioAsync_CuandoKeycloakFalla_DebeLanzarHttpRequestException()
        {
            // Arrange
            var correo = Correo.Crear("error@example.com");
            var telefono = Telefono.Crear("1234567890");
            var direccion = Direccion.Crear("Calle Error 123");
            
            var usuario = Usuario.Crear(
                "erroruser",
                "Error User",
                correo,
                telefono,
                direccion,
                Rol.User
            );

            // Mock token response
            var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new
                {
                    access_token = "test-token",
                    token_type = "Bearer"
                }))
            };

            // Mock failed create user response
            var createUserResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("Keycloak internal error")
            };

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.RequestUri!.ToString().Contains("/protocol/openid-connect/token")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(tokenResponse);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri!.ToString().EndsWith("/users")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(createUserResponse);

            // Act
            var act = async () => await _servicio.CrearUsuarioAsync(usuario, "password123");

            // Assert
            await act.Should().ThrowAsync<HttpRequestException>()
                .WithMessage("*Error al crear usuario en Keycloak*");
        }
    }
}
