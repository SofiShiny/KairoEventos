using System.Reflection;

namespace Entradas.API.Configuration;

/// <summary>
/// Configuración para Swagger/OpenAPI
/// </summary>
public static class SwaggerConfiguration
{
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new()
            {
                Version = "v1",
                Title = "Entradas API",
                Description = "API para la gestión de entradas (tickets) del sistema de eventos"
            });

            // Incluir comentarios XML
            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }
        });

        return services;
    }

    public static IApplicationBuilder UseSwaggerConfiguration(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment() || env.IsStaging())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Entradas API v1");
                options.RoutePrefix = "swagger";
                options.DocumentTitle = "Entradas API - Documentación";
                options.DefaultModelsExpandDepth(2);
                options.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Example);
                options.DisplayRequestDuration();
                options.EnableTryItOutByDefault();
            });
        }

        return app;
    }
}