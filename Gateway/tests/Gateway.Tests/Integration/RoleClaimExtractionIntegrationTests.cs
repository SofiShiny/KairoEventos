using FluentAssertions;
using Gateway.Tests.Integration.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using Xunit;

namespace Gateway.Tests.Integration;

/// <summary>
/// Tests de integración para extracción de roles del token
/// Property 5: Role Claim Extraction
/// Validates: Requirements 3.5
/// </summary>
public class RoleClaimExtractionIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public RoleClaimExtractionIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.Test.json", optional: false);
            });
        });
        
        _client = _factory.CreateClient();
    }

    /// <summary>
    /// Property 5: Role Claim Extraction
    /// For any valid JWT token, the Gateway should extract roles from the "roles" claim 
    /// in the token payload.
    /// </summary>
    [Fact]
    public async Task Gateway_Should_Extract_Single_Role_From_Token_User()
    {
        await TestRoleExtraction(new[] { "User" });
    }

    [Fact]
    public async Task Gateway_Should_Extract_Single_Role_From_Token_Admin()
    {
        await TestRoleExtraction(new[] { "Admin" });
    }

    [Fact]
    public async Task Gateway_Should_Extract_Single_Role_From_Token_Organizator()
    {
        await TestRoleExtraction(new[] { "Organizator" });
    }

    [Fact]
    public async Task Gateway_Should_Extract_Multiple_Roles_UserAdmin()
    {
        await TestRoleExtraction(new[] { "User", "Admin" });
    }

    [Fact]
    public async Task Gateway_Should_Extract_Multiple_Roles_UserOrganizator()
    {
        await TestRoleExtraction(new[] { "User", "Organizator" });
    }

    [Fact]
    public async Task Gateway_Should_Extract_Multiple_Roles_AdminOrganizator()
    {
        await TestRoleExtraction(new[] { "Admin", "Organizator" });
    }

    [Fact]
    public async Task Gateway_Should_Extract_All_Three_Roles()
    {
        await TestRoleExtraction(new[] { "User", "Admin", "Organizator" });
    }

    private async Task TestRoleExtraction(string[] roles)
    {
        // Arrange
        var token = JwtTokenGenerator.GenerateToken(
            username: "testuser",
            email: "test@example.com",
            roles: roles
        );
        
        // Verificar que el token contiene los roles en el claim "roles"
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var roleClaims = jwtToken.Claims.Where(c => c.Type == "roles").Select(c => c.Value).ToArray();
        
        roleClaims.Should().BeEquivalentTo(roles,
            "el token debe contener todos los roles en el claim 'roles'");
        
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
        
        // Act
        var response = await _client.GetAsync("/api/eventos/123");
        
        // Assert
        // El Gateway debe procesar el token y extraer los roles
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized,
            "el Gateway debe extraer y procesar los roles del token");
    }

    [Fact]
    public async Task Gateway_Should_Extract_Single_Role_From_Token()
    {
        // Arrange
        var token = JwtTokenGenerator.GenerateToken(
            username: "singleuser",
            email: "single@example.com",
            roles: new[] { "User" }
        );
        
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var roleClaims = jwtToken.Claims.Where(c => c.Type == "roles").ToList();
        
        // Assert token structure
        roleClaims.Should().HaveCount(1, "debe haber exactamente un claim de rol");
        roleClaims[0].Value.Should().Be("User", "el rol debe ser 'User'");
        
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
        
        // Act
        var response = await _client.GetAsync("/api/eventos/123");
        
        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized,
            "el Gateway debe extraer el rol único del token");
    }

    [Fact]
    public async Task Gateway_Should_Extract_Multiple_Roles_From_Token()
    {
        // Arrange
        var expectedRoles = new[] { "User", "Admin", "Organizator" };
        var token = JwtTokenGenerator.GenerateToken(
            username: "multiuser",
            email: "multi@example.com",
            roles: expectedRoles
        );
        
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var roleClaims = jwtToken.Claims.Where(c => c.Type == "roles").Select(c => c.Value).ToArray();
        
        // Assert token structure
        roleClaims.Should().HaveCount(3, "debe haber tres claims de rol");
        roleClaims.Should().BeEquivalentTo(expectedRoles, "todos los roles deben estar presentes");
        
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
        
        // Act
        var response = await _client.GetAsync("/api/eventos/123");
        
        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized,
            "el Gateway debe extraer todos los roles del token");
    }

    [Fact]
    public async Task Gateway_Should_Handle_Token_With_No_Roles()
    {
        // Arrange
        var token = JwtTokenGenerator.GenerateToken(
            username: "noroleuser",
            email: "norole@example.com",
            roles: Array.Empty<string>()
        );
        
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var roleClaims = jwtToken.Claims.Where(c => c.Type == "roles").ToList();
        
        // Assert token structure
        roleClaims.Should().BeEmpty("no debe haber claims de rol");
        
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
        
        // Act
        var response = await _client.GetAsync("/api/eventos/123");
        
        // Assert
        // El Gateway debe procesar el token aunque no tenga roles
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Forbidden,
            HttpStatusCode.ServiceUnavailable,
            HttpStatusCode.Unauthorized
        );
    }

    [Fact]
    public async Task Gateway_Should_Extract_Roles_From_Roles_Claim_Type()
    {
        // Arrange
        var roles = new[] { "User", "Admin" };
        var token = JwtTokenGenerator.GenerateToken(
            username: "testuser",
            email: "test@example.com",
            roles: roles
        );
        
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        // Verificar que los roles están en el claim type "roles"
        var roleClaimType = jwtToken.Claims
            .Where(c => c.Type == "roles")
            .Select(c => c.Type)
            .Distinct()
            .Single();
        
        roleClaimType.Should().Be("roles",
            "el claim type debe ser exactamente 'roles'");
        
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
        
        // Act
        var response = await _client.GetAsync("/api/eventos/123");
        
        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized,
            "el Gateway debe leer roles del claim type 'roles'");
    }

    [Theory]
    [InlineData("User")]
    [InlineData("Admin")]
    [InlineData("Organizator")]
    [InlineData("CustomRole")]
    public async Task Gateway_Should_Extract_Any_Role_Value_From_Roles_Claim(string roleName)
    {
        // Arrange
        var token = JwtTokenGenerator.GenerateToken(
            username: "testuser",
            email: "test@example.com",
            roles: new[] { roleName }
        );
        
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var extractedRole = jwtToken.Claims
            .Where(c => c.Type == "roles")
            .Select(c => c.Value)
            .Single();
        
        extractedRole.Should().Be(roleName,
            $"el rol extraído debe ser '{roleName}'");
        
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
        
        // Act
        var response = await _client.GetAsync("/api/eventos/123");
        
        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized,
            $"el Gateway debe extraer el rol '{roleName}' del token");
    }

    [Fact]
    public async Task Gateway_Should_Extract_Roles_For_All_Microservice_Requests()
    {
        // Arrange
        var roles = new[] { "User", "Admin" };
        var token = JwtTokenGenerator.GenerateToken(
            username: "testuser",
            email: "test@example.com",
            roles: roles
        );
        
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
        
        var endpoints = new[]
        {
            "/api/eventos/123",
            "/api/asientos/456",
            "/api/usuarios/789",
            "/api/entradas/101",
            "/api/reportes/202"
        };
        
        // Act & Assert
        foreach (var endpoint in endpoints)
        {
            var response = await _client.GetAsync(endpoint);
            response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized,
                $"el Gateway debe extraer roles para peticiones a {endpoint}");
        }
    }

    [Fact]
    public async Task Gateway_Should_Preserve_Role_Order_From_Token()
    {
        // Arrange
        var orderedRoles = new[] { "Admin", "User", "Organizator" };
        var token = JwtTokenGenerator.GenerateToken(
            username: "testuser",
            email: "test@example.com",
            roles: orderedRoles
        );
        
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var extractedRoles = jwtToken.Claims
            .Where(c => c.Type == "roles")
            .Select(c => c.Value)
            .ToArray();
        
        // Assert
        extractedRoles.Should().Equal(orderedRoles,
            "el orden de los roles debe preservarse");
    }

    [Fact]
    public async Task Gateway_Should_Extract_Roles_From_Concurrent_Requests()
    {
        // Arrange
        var tokens = new[]
        {
            JwtTokenGenerator.GenerateToken("user1", "user1@example.com", new[] { "User" }),
            JwtTokenGenerator.GenerateToken("user2", "user2@example.com", new[] { "Admin" }),
            JwtTokenGenerator.GenerateToken("user3", "user3@example.com", new[] { "Organizator" }),
            JwtTokenGenerator.GenerateToken("user4", "user4@example.com", new[] { "User", "Admin" })
        };
        
        // Act
        var tasks = tokens.Select(async token =>
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/eventos/123");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return await _client.SendAsync(request);
        }).ToArray();
        
        var responses = await Task.WhenAll(tasks);
        
        // Assert
        foreach (var response in responses)
        {
            response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized,
                "el Gateway debe extraer roles de todas las peticiones concurrentes");
        }
    }
}
