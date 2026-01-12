using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using System.Reflection;

namespace Entradas.Aplicacion;

/// <summary>
/// Configuración de inyección de dependencias para la capa de aplicación
/// </summary>
public static class InyeccionDependencias
{
    public static IServiceCollection AddAplicacion(this IServiceCollection services)
    {
        // Registrar MediatR handlers
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // Registrar validadores de FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Registrar Jobs de Hangfire
        services.AddScoped<Jobs.ExpiracionReservasJob>();

        return services;
    }
}