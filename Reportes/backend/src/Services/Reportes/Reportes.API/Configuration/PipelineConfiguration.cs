using Hangfire;
using Reportes.API.Hangfire;
using Reportes.API.Middleware;
using Reportes.Aplicacion.Jobs;
using Reportes.Infraestructura.Persistencia;
using Serilog;

namespace Reportes.API.Configuration;

public static class PipelineConfiguration
{
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.ConfigureMongoDbConnection()
           .ConfigureMiddlewarePipeline()
           .ConfigureDevelopmentPipeline()
           .ConfigureHangfirePipeline()
           .ConfigureEndpointsPipeline();
           
        return app;
    }

    private static WebApplication ConfigureMongoDbConnection(this WebApplication app)
    {
        // Verificar conexión a MongoDB al iniciar
        using (var scope = app.Services.CreateScope())
        {
            var mongoContext = scope.ServiceProvider.GetRequiredService<ReportesMongoDbContext>();
            try
            {
                var conectado = mongoContext.VerificarConexionAsync().GetAwaiter().GetResult();
                if (conectado)
                {
                    Log.Information("Conexión a MongoDB establecida correctamente");
                }
                else
                {
                    Log.Warning("No se pudo conectar a MongoDB");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error verificando conexión a MongoDB");
            }
        }
        
        return app;
    }

    private static WebApplication ConfigureMiddlewarePipeline(this WebApplication app)
    {
        app.UseCorrelationId();
        app.UseRouting(); // Asegurar enrutamiento explicito antes de otros middleware
        app.UseGlobalExceptionHandler();
        
        return app;
    }

    private static WebApplication ConfigureDevelopmentPipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Reportes API v1");
                c.RoutePrefix = "swagger";
            });
        }
        
        return app;
    }

    private static WebApplication ConfigureHangfirePipeline(this WebApplication app)
    {
        var enableHangfire = app.Configuration.GetValue<bool>("Hangfire:Enabled", true);
        if (enableHangfire)
        {
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new HangfireAuthorizationFilter() }
            });

            // Configurar job recurrente de consolidación (diariamente a las 2 AM)
            RecurringJob.AddOrUpdate<JobGenerarReportesConsolidados>(
                "generar-reportes-consolidados",
                job => job.EjecutarAsync(),
                Cron.Daily(2), // 2 AM todos los días
                new RecurringJobOptions
                {
                    TimeZone = TimeZoneInfo.Utc
                });

            // Job para dashboard en tiempo real (cada minuto)
            RecurringJob.AddOrUpdate<MetricasTiempoRealJob>(
                "actualizar-metricas-dashboard",
                job => job.ActualizarDashboardAsync(),
                Cron.Minutely);
        }
        
        return app;
    }

    private static WebApplication ConfigureEndpointsPipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging();
        app.UseCors("AllowAll");
        app.UseAuthorization();
        app.MapControllers();
        app.MapHub<Reportes.API.Hubs.ReportesHub>("/hub/reportes");

        // Health checks con respuesta detallada
        var enableHealthChecks = app.Configuration.GetValue<bool>("HealthChecks:Enabled", true);
        if (enableHealthChecks)
        {
            app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    var result = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        status = report.Status.ToString(),
                        timestamp = DateTime.UtcNow,
                        checks = report.Entries.Select(e => new
                        {
                            name = e.Key,
                            status = e.Value.Status.ToString(),
                            description = e.Value.Description,
                            duration = e.Value.Duration.TotalMilliseconds,
                            exception = e.Value.Exception?.Message
                        }),
                        totalDuration = report.TotalDuration.TotalMilliseconds
                    }, new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                    });
                    await context.Response.WriteAsync(result);
                }
            });
        }
        else
        {
            // Health check simplificado para tests
            app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }));
        }

        app.MapGet("/", () => Results.Ok(new 
        { 
            servicio = "Reportes API",
            version = "1.0.0",
            estado = "activo"
        }));

        var urls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "http://0.0.0.0:5002";
        Log.Information("Iniciando Reportes API en {Urls}", urls);
        
        return app;
    }
}