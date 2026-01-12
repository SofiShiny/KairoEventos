using FluentAssertions;
using Gateway.API.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Gateway.Tests.Configuration;

/// <summary>
/// Tests unitarios para la configuración de autorización basada en roles
/// Validates: Requirements 3.1, 3.2, 3.3, 3.4, 3.5
/// </summary>
public class AuthorizationConfigurationTests
{
    [Fact]
    public void AddRoleBasedAuthorization_ShouldRegisterAuthorizationServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(); // Add logging services required by authorization

        // Act
        services.AddRoleBasedAuthorization();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var authorizationService = serviceProvider.GetService<IAuthorizationService>();
        authorizationService.Should().NotBeNull();
    }

    [Fact]
    public void AddRoleBasedAuthorization_ShouldRegisterAuthenticatedPolicy()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddRoleBasedAuthorization();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var authorizationOptions = serviceProvider
            .GetService<Microsoft.Extensions.Options.IOptions<AuthorizationOptions>>();
        
        authorizationOptions.Should().NotBeNull();
        
        var policy = authorizationOptions!.Value.GetPolicy("Authenticated");
        policy.Should().NotBeNull();
        policy!.Requirements.Should().ContainSingle()
            .Which.Should().BeOfType<DenyAnonymousAuthorizationRequirement>();
    }

    [Fact]
    public void AddRoleBasedAuthorization_ShouldRegisterUserAccessPolicy()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddRoleBasedAuthorization();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var authorizationOptions = serviceProvider
            .GetService<Microsoft.Extensions.Options.IOptions<AuthorizationOptions>>();
        
        var policy = authorizationOptions!.Value.GetPolicy("UserAccess");
        policy.Should().NotBeNull();
        
        // Verificar que requiere autenticación
        var hasDenyAnonymous = policy!.Requirements.Any(r => r is DenyAnonymousAuthorizationRequirement);
        hasDenyAnonymous.Should().BeTrue();
        
        // Verificar que requiere el rol "User"
        var rolesRequirement = policy.Requirements.OfType<RolesAuthorizationRequirement>().FirstOrDefault();
        rolesRequirement.Should().NotBeNull();
        rolesRequirement!.AllowedRoles.Should().Contain("User");
    }

    [Fact]
    public void AddRoleBasedAuthorization_ShouldRegisterAdminAccessPolicy()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddRoleBasedAuthorization();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var authorizationOptions = serviceProvider
            .GetService<Microsoft.Extensions.Options.IOptions<AuthorizationOptions>>();
        
        var policy = authorizationOptions!.Value.GetPolicy("AdminAccess");
        policy.Should().NotBeNull();
        
        // Verificar que requiere autenticación
        var hasDenyAnonymous = policy!.Requirements.Any(r => r is DenyAnonymousAuthorizationRequirement);
        hasDenyAnonymous.Should().BeTrue();
        
        // Verificar que requiere el rol "Admin"
        var rolesRequirement = policy.Requirements.OfType<RolesAuthorizationRequirement>().FirstOrDefault();
        rolesRequirement.Should().NotBeNull();
        rolesRequirement!.AllowedRoles.Should().Contain("Admin");
    }

    [Fact]
    public void AddRoleBasedAuthorization_ShouldRegisterOrganizatorAccessPolicy()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddRoleBasedAuthorization();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var authorizationOptions = serviceProvider
            .GetService<Microsoft.Extensions.Options.IOptions<AuthorizationOptions>>();
        
        var policy = authorizationOptions!.Value.GetPolicy("OrganizatorAccess");
        policy.Should().NotBeNull();
        
        // Verificar que requiere autenticación
        var hasDenyAnonymous = policy!.Requirements.Any(r => r is DenyAnonymousAuthorizationRequirement);
        hasDenyAnonymous.Should().BeTrue();
        
        // Verificar que requiere el rol "Organizator"
        var rolesRequirement = policy.Requirements.OfType<RolesAuthorizationRequirement>().FirstOrDefault();
        rolesRequirement.Should().NotBeNull();
        rolesRequirement!.AllowedRoles.Should().Contain("Organizator");
    }

    [Fact]
    public void AddRoleBasedAuthorization_ShouldRegisterEventManagementPolicy()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddRoleBasedAuthorization();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var authorizationOptions = serviceProvider
            .GetService<Microsoft.Extensions.Options.IOptions<AuthorizationOptions>>();
        
        var policy = authorizationOptions!.Value.GetPolicy("EventManagement");
        policy.Should().NotBeNull();
        
        // Verificar que requiere autenticación
        var hasDenyAnonymous = policy!.Requirements.Any(r => r is DenyAnonymousAuthorizationRequirement);
        hasDenyAnonymous.Should().BeTrue();
        
        // Verificar que requiere los roles "Admin" o "Organizator"
        var rolesRequirement = policy.Requirements.OfType<RolesAuthorizationRequirement>().FirstOrDefault();
        rolesRequirement.Should().NotBeNull();
        rolesRequirement!.AllowedRoles.Should().Contain("Admin");
        rolesRequirement.AllowedRoles.Should().Contain("Organizator");
    }

    [Fact]
    public void AddRoleBasedAuthorization_ShouldRegisterAllRequiredPolicies()
    {
        // Arrange
        var services = new ServiceCollection();
        var expectedPolicies = new[]
        {
            "Authenticated",
            "UserAccess",
            "AdminAccess",
            "OrganizatorAccess",
            "EventManagement"
        };

        // Act
        services.AddRoleBasedAuthorization();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var authorizationOptions = serviceProvider
            .GetService<Microsoft.Extensions.Options.IOptions<AuthorizationOptions>>();
        
        authorizationOptions.Should().NotBeNull();

        foreach (var policyName in expectedPolicies)
        {
            var policy = authorizationOptions!.Value.GetPolicy(policyName);
            policy.Should().NotBeNull($"Policy '{policyName}' should be registered");
        }
    }

    [Fact]
    public void AddRoleBasedAuthorization_UserAccessPolicy_ShouldRequireOnlyUserRole()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddRoleBasedAuthorization();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var authorizationOptions = serviceProvider
            .GetService<Microsoft.Extensions.Options.IOptions<AuthorizationOptions>>();
        
        var policy = authorizationOptions!.Value.GetPolicy("UserAccess");
        
        var rolesRequirement = policy!.Requirements
            .OfType<RolesAuthorizationRequirement>()
            .FirstOrDefault();
        
        rolesRequirement.Should().NotBeNull();
        rolesRequirement!.AllowedRoles.Should().ContainSingle()
            .Which.Should().Be("User");
    }

    [Fact]
    public void AddRoleBasedAuthorization_AdminAccessPolicy_ShouldRequireOnlyAdminRole()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddRoleBasedAuthorization();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var authorizationOptions = serviceProvider
            .GetService<Microsoft.Extensions.Options.IOptions<AuthorizationOptions>>();
        
        var policy = authorizationOptions!.Value.GetPolicy("AdminAccess");
        
        var rolesRequirement = policy!.Requirements
            .OfType<RolesAuthorizationRequirement>()
            .FirstOrDefault();
        
        rolesRequirement.Should().NotBeNull();
        rolesRequirement!.AllowedRoles.Should().ContainSingle()
            .Which.Should().Be("Admin");
    }

    [Fact]
    public void AddRoleBasedAuthorization_OrganizatorAccessPolicy_ShouldRequireOnlyOrganizatorRole()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddRoleBasedAuthorization();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var authorizationOptions = serviceProvider
            .GetService<Microsoft.Extensions.Options.IOptions<AuthorizationOptions>>();
        
        var policy = authorizationOptions!.Value.GetPolicy("OrganizatorAccess");
        
        var rolesRequirement = policy!.Requirements
            .OfType<RolesAuthorizationRequirement>()
            .FirstOrDefault();
        
        rolesRequirement.Should().NotBeNull();
        rolesRequirement!.AllowedRoles.Should().ContainSingle()
            .Which.Should().Be("Organizator");
    }

    [Fact]
    public void AddRoleBasedAuthorization_EventManagementPolicy_ShouldRequireAdminOrOrganizatorRole()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddRoleBasedAuthorization();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var authorizationOptions = serviceProvider
            .GetService<Microsoft.Extensions.Options.IOptions<AuthorizationOptions>>();
        
        var policy = authorizationOptions!.Value.GetPolicy("EventManagement");
        
        var rolesRequirement = policy!.Requirements
            .OfType<RolesAuthorizationRequirement>()
            .FirstOrDefault();
        
        rolesRequirement.Should().NotBeNull();
        rolesRequirement!.AllowedRoles.Should().HaveCount(2);
        rolesRequirement.AllowedRoles.Should().Contain("Admin");
        rolesRequirement.AllowedRoles.Should().Contain("Organizator");
    }

    [Fact]
    public void AddRoleBasedAuthorization_AllPolicies_ShouldRequireAuthentication()
    {
        // Arrange
        var services = new ServiceCollection();
        var allPolicies = new[]
        {
            "Authenticated",
            "UserAccess",
            "AdminAccess",
            "OrganizatorAccess",
            "EventManagement"
        };

        // Act
        services.AddRoleBasedAuthorization();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var authorizationOptions = serviceProvider
            .GetService<Microsoft.Extensions.Options.IOptions<AuthorizationOptions>>();

        foreach (var policyName in allPolicies)
        {
            var policy = authorizationOptions!.Value.GetPolicy(policyName);
            policy.Should().NotBeNull($"Policy '{policyName}' should exist");
            
            var hasDenyAnonymous = policy!.Requirements.Any(r => r is DenyAnonymousAuthorizationRequirement);
            hasDenyAnonymous.Should().BeTrue($"Policy '{policyName}' should require authentication");
        }
    }
}
