using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace Gateway.Tests.Keycloak;

public class RealmExportValidationTests
{
    private const string RealmExportPath = "../../../../../../Infraestructura/configs/keycloak/realm-export.json";

    [Fact]
    public void RealmExportFile_ShouldExist()
    {
        // Arrange & Act
        var fileExists = File.Exists(RealmExportPath);

        // Assert
        fileExists.Should().BeTrue("the realm-export.json file must exist for Keycloak configuration");
    }

    [Fact]
    public void RealmExportFile_ShouldBeValidJson()
    {
        // Arrange
        var jsonContent = File.ReadAllText(RealmExportPath);

        // Act
        var act = () => JsonDocument.Parse(jsonContent);

        // Assert
        act.Should().NotThrow("the realm-export.json must be valid JSON");
    }

    [Fact]
    public void RealmExport_ShouldHaveCorrectRealmName()
    {
        // Arrange
        var jsonContent = File.ReadAllText(RealmExportPath);
        var doc = JsonDocument.Parse(jsonContent);

        // Act
        var realmName = doc.RootElement.GetProperty("realm").GetString();

        // Assert
        realmName.Should().Be("Kairo", "the realm name must be 'Kairo' as specified in requirements");
    }

    [Fact]
    public void RealmExport_ShouldHaveCorrectTokenLifespans()
    {
        // Arrange
        var jsonContent = File.ReadAllText(RealmExportPath);
        var doc = JsonDocument.Parse(jsonContent);

        // Act
        var accessTokenLifespan = doc.RootElement.GetProperty("accessTokenLifespan").GetInt32();
        var ssoSessionIdleTimeout = doc.RootElement.GetProperty("ssoSessionIdleTimeout").GetInt32();

        // Assert
        accessTokenLifespan.Should().Be(300, "access token lifespan should be 5 minutes (300 seconds)");
        ssoSessionIdleTimeout.Should().Be(1800, "refresh token lifespan should be 30 minutes (1800 seconds)");
    }

    [Fact]
    public void RealmExport_ShouldHaveBruteForceProtectionEnabled()
    {
        // Arrange
        var jsonContent = File.ReadAllText(RealmExportPath);
        var doc = JsonDocument.Parse(jsonContent);

        // Act
        var bruteForceProtected = doc.RootElement.GetProperty("bruteForceProtected").GetBoolean();

        // Assert
        bruteForceProtected.Should().BeTrue("brute force protection must be enabled for security");
    }

    [Fact]
    public void RealmExport_ShouldHaveAllRequiredRoles()
    {
        // Arrange
        var jsonContent = File.ReadAllText(RealmExportPath);
        var doc = JsonDocument.Parse(jsonContent);
        var expectedRoles = new[] { "User", "Admin", "Organizator" };

        // Act
        var roles = doc.RootElement
            .GetProperty("roles")
            .GetProperty("realm")
            .EnumerateArray()
            .Select(r => r.GetProperty("name").GetString())
            .ToList();

        // Assert
        roles.Should().Contain(expectedRoles, "all required roles must be defined");
    }

    [Fact]
    public void RealmExport_UserRole_ShouldHaveCorrectDescription()
    {
        // Arrange
        var jsonContent = File.ReadAllText(RealmExportPath);
        var doc = JsonDocument.Parse(jsonContent);

        // Act
        var userRole = doc.RootElement
            .GetProperty("roles")
            .GetProperty("realm")
            .EnumerateArray()
            .FirstOrDefault(r => r.GetProperty("name").GetString() == "User");

        // Assert
        userRole.GetProperty("description").GetString()
            .Should().Contain("Usuario regular", "User role must have appropriate description");
    }

    [Fact]
    public void RealmExport_AdminRole_ShouldHaveCorrectDescription()
    {
        // Arrange
        var jsonContent = File.ReadAllText(RealmExportPath);
        var doc = JsonDocument.Parse(jsonContent);

        // Act
        var adminRole = doc.RootElement
            .GetProperty("roles")
            .GetProperty("realm")
            .EnumerateArray()
            .FirstOrDefault(r => r.GetProperty("name").GetString() == "Admin");

        // Assert
        adminRole.GetProperty("description").GetString()
            .Should().Contain("Administrador", "Admin role must have appropriate description");
    }

    [Fact]
    public void RealmExport_OrganizatorRole_ShouldHaveCorrectDescription()
    {
        // Arrange
        var jsonContent = File.ReadAllText(RealmExportPath);
        var doc = JsonDocument.Parse(jsonContent);

        // Act
        var organizatorRole = doc.RootElement
            .GetProperty("roles")
            .GetProperty("realm")
            .EnumerateArray()
            .FirstOrDefault(r => r.GetProperty("name").GetString() == "Organizator");

        // Assert
        organizatorRole.GetProperty("description").GetString()
            .Should().Contain("Organizador", "Organizator role must have appropriate description");
    }

    [Fact]
    public void RealmExport_ShouldHaveKairoWebClient()
    {
        // Arrange
        var jsonContent = File.ReadAllText(RealmExportPath);
        var doc = JsonDocument.Parse(jsonContent);

        // Act
        var clients = doc.RootElement
            .GetProperty("clients")
            .EnumerateArray()
            .Select(c => c.GetProperty("clientId").GetString())
            .ToList();

        // Assert
        clients.Should().Contain("kairo-web", "kairo-web client must be defined");
    }

    [Fact]
    public void RealmExport_KairoWebClient_ShouldBePublicClient()
    {
        // Arrange
        var jsonContent = File.ReadAllText(RealmExportPath);
        var doc = JsonDocument.Parse(jsonContent);

        // Act
        var kairoWebClient = doc.RootElement
            .GetProperty("clients")
            .EnumerateArray()
            .FirstOrDefault(c => c.GetProperty("clientId").GetString() == "kairo-web");

        // Assert
        kairoWebClient.GetProperty("publicClient").GetBoolean()
            .Should().BeTrue("kairo-web must be a public client for frontend applications");
    }

    [Fact]
    public void RealmExport_KairoWebClient_ShouldHaveAuthorizationCodeFlowEnabled()
    {
        // Arrange
        var jsonContent = File.ReadAllText(RealmExportPath);
        var doc = JsonDocument.Parse(jsonContent);

        // Act
        var kairoWebClient = doc.RootElement
            .GetProperty("clients")
            .EnumerateArray()
            .FirstOrDefault(c => c.GetProperty("clientId").GetString() == "kairo-web");

        // Assert
        kairoWebClient.GetProperty("standardFlowEnabled").GetBoolean()
            .Should().BeTrue("kairo-web must have Authorization Code flow enabled");
    }

    [Fact]
    public void RealmExport_KairoWebClient_ShouldHavePKCEConfigured()
    {
        // Arrange
        var jsonContent = File.ReadAllText(RealmExportPath);
        var doc = JsonDocument.Parse(jsonContent);

        // Act
        var kairoWebClient = doc.RootElement
            .GetProperty("clients")
            .EnumerateArray()
            .FirstOrDefault(c => c.GetProperty("clientId").GetString() == "kairo-web");

        var pkceMethod = kairoWebClient
            .GetProperty("attributes")
            .GetProperty("pkce.code.challenge.method")
            .GetString();

        // Assert
        pkceMethod.Should().Be("S256", "kairo-web must use PKCE with S256 challenge method");
    }

    [Fact]
    public void RealmExport_KairoWebClient_ShouldHaveCorrectRedirectUris()
    {
        // Arrange
        var jsonContent = File.ReadAllText(RealmExportPath);
        var doc = JsonDocument.Parse(jsonContent);
        var expectedUris = new[] { "http://localhost:5173/*", "http://localhost:3000/*" };

        // Act
        var kairoWebClient = doc.RootElement
            .GetProperty("clients")
            .EnumerateArray()
            .FirstOrDefault(c => c.GetProperty("clientId").GetString() == "kairo-web");

        var redirectUris = kairoWebClient
            .GetProperty("redirectUris")
            .EnumerateArray()
            .Select(u => u.GetString())
            .ToList();

        // Assert
        redirectUris.Should().Contain(expectedUris, "kairo-web must have correct redirect URIs");
    }

    [Fact]
    public void RealmExport_KairoWebClient_ShouldHaveCorrectWebOrigins()
    {
        // Arrange
        var jsonContent = File.ReadAllText(RealmExportPath);
        var doc = JsonDocument.Parse(jsonContent);
        var expectedOrigins = new[] { "http://localhost:5173", "http://localhost:3000" };

        // Act
        var kairoWebClient = doc.RootElement
            .GetProperty("clients")
            .EnumerateArray()
            .FirstOrDefault(c => c.GetProperty("clientId").GetString() == "kairo-web");

        var webOrigins = kairoWebClient
            .GetProperty("webOrigins")
            .EnumerateArray()
            .Select(o => o.GetString())
            .ToList();

        // Assert
        webOrigins.Should().Contain(expectedOrigins, "kairo-web must have correct CORS origins");
    }

    [Fact]
    public void RealmExport_ShouldHaveKairoApiClient()
    {
        // Arrange
        var jsonContent = File.ReadAllText(RealmExportPath);
        var doc = JsonDocument.Parse(jsonContent);

        // Act
        var clients = doc.RootElement
            .GetProperty("clients")
            .EnumerateArray()
            .Select(c => c.GetProperty("clientId").GetString())
            .ToList();

        // Assert
        clients.Should().Contain("kairo-api", "kairo-api client must be defined");
    }

    [Fact]
    public void RealmExport_KairoApiClient_ShouldBeBearerOnly()
    {
        // Arrange
        var jsonContent = File.ReadAllText(RealmExportPath);
        var doc = JsonDocument.Parse(jsonContent);

        // Act
        var kairoApiClient = doc.RootElement
            .GetProperty("clients")
            .EnumerateArray()
            .FirstOrDefault(c => c.GetProperty("clientId").GetString() == "kairo-api");

        // Assert
        kairoApiClient.GetProperty("bearerOnly").GetBoolean()
            .Should().BeTrue("kairo-api must be a bearer-only client");
    }

    [Fact]
    public void RealmExport_KairoApiClient_ShouldHaveStandardFlowDisabled()
    {
        // Arrange
        var jsonContent = File.ReadAllText(RealmExportPath);
        var doc = JsonDocument.Parse(jsonContent);

        // Act
        var kairoApiClient = doc.RootElement
            .GetProperty("clients")
            .EnumerateArray()
            .FirstOrDefault(c => c.GetProperty("clientId").GetString() == "kairo-api");

        // Assert
        kairoApiClient.GetProperty("standardFlowEnabled").GetBoolean()
            .Should().BeFalse("kairo-api should not have standard flow enabled");
    }

    [Fact]
    public void RealmExport_ShouldHaveAllRequiredUsers()
    {
        // Arrange
        var jsonContent = File.ReadAllText(RealmExportPath);
        var doc = JsonDocument.Parse(jsonContent);
        var expectedUsers = new[] { "admin", "organizador", "usuario" };

        // Act
        var users = doc.RootElement
            .GetProperty("users")
            .EnumerateArray()
            .Select(u => u.GetProperty("username").GetString())
            .ToList();

        // Assert
        users.Should().Contain(expectedUsers, "all required default users must be defined");
    }

    [Fact]
    public void RealmExport_AdminUser_ShouldHaveAdminRole()
    {
        // Arrange
        var jsonContent = File.ReadAllText(RealmExportPath);
        var doc = JsonDocument.Parse(jsonContent);

        // Act
        var adminUser = doc.RootElement
            .GetProperty("users")
            .EnumerateArray()
            .FirstOrDefault(u => u.GetProperty("username").GetString() == "admin");

        var roles = adminUser
            .GetProperty("realmRoles")
            .EnumerateArray()
            .Select(r => r.GetString())
            .ToList();

        // Assert
        roles.Should().Contain("Admin", "admin user must have Admin role");
    }

    [Fact]
    public void RealmExport_OrganizadorUser_ShouldHaveOrganizatorRole()
    {
        // Arrange
        var jsonContent = File.ReadAllText(RealmExportPath);
        var doc = JsonDocument.Parse(jsonContent);

        // Act
        var organizadorUser = doc.RootElement
            .GetProperty("users")
            .EnumerateArray()
            .FirstOrDefault(u => u.GetProperty("username").GetString() == "organizador");

        var roles = organizadorUser
            .GetProperty("realmRoles")
            .EnumerateArray()
            .Select(r => r.GetString())
            .ToList();

        // Assert
        roles.Should().Contain("Organizator", "organizador user must have Organizator role");
    }

    [Fact]
    public void RealmExport_UsuarioUser_ShouldHaveUserRole()
    {
        // Arrange
        var jsonContent = File.ReadAllText(RealmExportPath);
        var doc = JsonDocument.Parse(jsonContent);

        // Act
        var usuarioUser = doc.RootElement
            .GetProperty("users")
            .EnumerateArray()
            .FirstOrDefault(u => u.GetProperty("username").GetString() == "usuario");

        var roles = usuarioUser
            .GetProperty("realmRoles")
            .EnumerateArray()
            .Select(r => r.GetString())
            .ToList();

        // Assert
        roles.Should().Contain("User", "usuario user must have User role");
    }

    [Fact]
    public void RealmExport_AdminUser_ShouldHaveCorrectCredentials()
    {
        // Arrange
        var jsonContent = File.ReadAllText(RealmExportPath);
        var doc = JsonDocument.Parse(jsonContent);

        // Act
        var adminUser = doc.RootElement
            .GetProperty("users")
            .EnumerateArray()
            .FirstOrDefault(u => u.GetProperty("username").GetString() == "admin");

        var credentials = adminUser.GetProperty("credentials").EnumerateArray().First();
        var password = credentials.GetProperty("value").GetString();
        var temporary = credentials.GetProperty("temporary").GetBoolean();

        // Assert
        password.Should().Be("admin123", "admin user must have correct password");
        temporary.Should().BeFalse("admin password should not be temporary");
    }

    [Fact]
    public void RealmExport_OrganizadorUser_ShouldHaveCorrectCredentials()
    {
        // Arrange
        var jsonContent = File.ReadAllText(RealmExportPath);
        var doc = JsonDocument.Parse(jsonContent);

        // Act
        var organizadorUser = doc.RootElement
            .GetProperty("users")
            .EnumerateArray()
            .FirstOrDefault(u => u.GetProperty("username").GetString() == "organizador");

        var credentials = organizadorUser.GetProperty("credentials").EnumerateArray().First();
        var password = credentials.GetProperty("value").GetString();
        var temporary = credentials.GetProperty("temporary").GetBoolean();

        // Assert
        password.Should().Be("org123", "organizador user must have correct password");
        temporary.Should().BeFalse("organizador password should not be temporary");
    }

    [Fact]
    public void RealmExport_UsuarioUser_ShouldHaveCorrectCredentials()
    {
        // Arrange
        var jsonContent = File.ReadAllText(RealmExportPath);
        var doc = JsonDocument.Parse(jsonContent);

        // Act
        var usuarioUser = doc.RootElement
            .GetProperty("users")
            .EnumerateArray()
            .FirstOrDefault(u => u.GetProperty("username").GetString() == "usuario");

        var credentials = usuarioUser.GetProperty("credentials").EnumerateArray().First();
        var password = credentials.GetProperty("value").GetString();
        var temporary = credentials.GetProperty("temporary").GetBoolean();

        // Assert
        password.Should().Be("user123", "usuario user must have correct password");
        temporary.Should().BeFalse("usuario password should not be temporary");
    }

    [Fact]
    public void RealmExport_AllUsers_ShouldBeEnabled()
    {
        // Arrange
        var jsonContent = File.ReadAllText(RealmExportPath);
        var doc = JsonDocument.Parse(jsonContent);

        // Act
        var allUsersEnabled = doc.RootElement
            .GetProperty("users")
            .EnumerateArray()
            .All(u => u.GetProperty("enabled").GetBoolean());

        // Assert
        allUsersEnabled.Should().BeTrue("all default users must be enabled");
    }

    [Fact]
    public void RealmExport_AllUsers_ShouldHaveEmailVerified()
    {
        // Arrange
        var jsonContent = File.ReadAllText(RealmExportPath);
        var doc = JsonDocument.Parse(jsonContent);

        // Act
        var allEmailsVerified = doc.RootElement
            .GetProperty("users")
            .EnumerateArray()
            .All(u => u.GetProperty("emailVerified").GetBoolean());

        // Assert
        allEmailsVerified.Should().BeTrue("all default users must have verified emails");
    }
}
