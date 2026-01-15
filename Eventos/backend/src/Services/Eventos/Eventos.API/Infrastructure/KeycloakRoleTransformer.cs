using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Text.Json;

namespace Eventos.API.Infrastructure;

public class KeycloakRoleTransformer : IClaimsTransformation
{
    private readonly ILogger<KeycloakRoleTransformer> _logger;

    public KeycloakRoleTransformer(ILogger<KeycloakRoleTransformer> logger)
    {
        _logger = logger;
    }

    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.HasClaim(c => c.Type == "role_transformed"))
        {
            return Task.FromResult(principal);
        }

        if (principal.Identity == null || !principal.Identity.IsAuthenticated)
        {
            return Task.FromResult(principal);
        }

        var clone = principal.Clone();
        var identity = clone.Identity as ClaimsIdentity;
        if (identity == null)
        {
            return Task.FromResult(principal);
        }

        var rolesToAdd = new List<string>();

        // 1. Extraer desde realm_access.roles
        var realmAccessClaim = principal.FindFirst("realm_access");
        if (realmAccessClaim != null)
        {
            try
            {
                using var doc = JsonDocument.Parse(realmAccessClaim.Value);
                if (doc.RootElement.TryGetProperty("roles", out var roles))
                {
                    foreach (var role in roles.EnumerateArray())
                    {
                        var roleName = role.GetString();
                        if (!string.IsNullOrEmpty(roleName)) rolesToAdd.Add(roleName);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing realm_access from Keycloak token");
            }
        }

        // 2. Extraer desde resource_access
        var resourceAccessClaim = principal.FindFirst("resource_access");
        if (resourceAccessClaim != null)
        {
            try
            {
                using var doc = JsonDocument.Parse(resourceAccessClaim.Value);
                foreach (var client in doc.RootElement.EnumerateObject())
                {
                    if (client.Value.TryGetProperty("roles", out var roles))
                    {
                        foreach (var role in roles.EnumerateArray())
                        {
                            var roleName = role.GetString();
                            if (!string.IsNullOrEmpty(roleName)) rolesToAdd.Add(roleName);
                        }
                    }
                }
            }
            catch { }
        }

        // 3. Extraer de claims planos 'role' o 'roles'
        var flatRoleClaims = principal.FindAll(c => c.Type == "role" || c.Type == "roles").ToList();
        foreach (var claim in flatRoleClaims)
        {
            rolesToAdd.Add(claim.Value);
        }

        // 4. Normalizar y añadir claims
        foreach (var roleName in rolesToAdd.Distinct())
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
            identity.AddClaim(new Claim("role", roleName));
            
            // Compatibilidad para roles en minúsculas (ej: Admin -> admin)
            if (roleName != roleName.ToLower())
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, roleName.ToLower()));
            }
        }

        identity.AddClaim(new Claim("role_transformed", "true"));
        return Task.FromResult(clone);
    }
}
