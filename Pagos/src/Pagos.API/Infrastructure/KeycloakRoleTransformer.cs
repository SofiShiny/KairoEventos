using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;

namespace Pagos.API.Infrastructure;

public class KeycloakRoleTransformer : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        // 1. Verificar si ya fue transformado
        if (principal.HasClaim(c => c.Type == "role_transformed"))
        {
            return Task.FromResult(principal);
        }

        // 2. Clonar el principal para no modificar el original directamente si es posible,
        // aunque IClaimsTransformation suele permitir modificar el principal recibido.
        var clone = principal.Clone();
        var identity = (ClaimsIdentity)clone.Identity!;

        Console.WriteLine($"--- INICIANDO TRANSFORMACIÓN DE ROLES ---");

        // Lista temporal para recolectar roles y evitar modificar la colección mientras iteramos
        var rolesToAdd = new List<string>();

        // 3. Extraer de realm_access.roles
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
            catch { }
        }

        // 4. Extraer de resource_access
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

        // 5. Extraer de claims planos 'role' o 'roles' (convertir a Lista para evitar error de enumeración)
        var flatRoleClaims = principal.FindAll(c => c.Type == "role" || c.Type == "roles").ToList();
        foreach (var claim in flatRoleClaims)
        {
            rolesToAdd.Add(claim.Value);
        }

        // 6. Añadir todos los roles encontrados como Claims de tipo Role estándar
        // Usamos Distinct para no duplicar roles
        foreach (var roleName in rolesToAdd.Distinct())
        {
            Console.WriteLine($"[TRANSFORMER] Añadiendo Rol: {roleName}");
            identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
            identity.AddClaim(new Claim("role", roleName));
            
            // Si el rol no está en minúsculas, añadir también la versión en minúsculas para compatibilidad
            if (roleName != roleName.ToLower())
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, roleName.ToLower()));
            }
        }

        // Marcar como transformado
        identity.AddClaim(new Claim("role_transformed", "true"));
        
        Console.WriteLine($"--- TRANSFORMACIÓN COMPLETADA ---");

        return Task.FromResult(clone);
    }
}
