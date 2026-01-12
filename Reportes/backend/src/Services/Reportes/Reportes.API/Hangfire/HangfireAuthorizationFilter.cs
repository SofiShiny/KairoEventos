using Hangfire.Dashboard;

namespace Reportes.API.Hangfire;

/// <summary>
/// Filtro de autorización para el dashboard de Hangfire.
/// En producción, se debe implementar autenticación real.
/// </summary>
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // En desarrollo, permitir acceso sin autenticación
        // En producción, implementar autenticación real
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        
        if (environment == "Development")
        {
            return true;
        }

        // En producción, requerir autenticación
        // Por ahora, permitir acceso (cambiar en producción)
        return true;
    }
}
