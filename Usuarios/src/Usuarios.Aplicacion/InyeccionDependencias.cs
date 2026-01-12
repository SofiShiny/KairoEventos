using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Usuarios.Aplicacion.Behaviors;

namespace Usuarios.Aplicacion;

public static class InyeccionDependencias
{
    public static IServiceCollection AgregarAplicacion(this IServiceCollection services)
    {
        // Registrar MediatR con todos los handlers del assembly actual
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        // Registrar FluentValidation con todos los validators del assembly actual
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Registrar PipelineBehavior para validación automática
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
