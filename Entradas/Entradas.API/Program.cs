using Entradas.API.Configuration;
using Entradas.API.Middleware;
using Entradas.API.HealthChecks;
using Entradas.Aplicacion;
using Entradas.Infraestructura;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using Serilog;
using Serilog.Context;
using Hangfire;
using Entradas.Aplicacion.Jobs;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog con enrichers adicionales
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "Entradas.API")
    .Enrich.WithProperty("Version", typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0")
    .CreateLogger();

builder.Host.UseSerilog();

// Log de inicio de aplicación
Log.Information("Iniciando Entradas.API v{Version} en entorno {Environment}", 
    typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0", 
    builder.Environment.EnvironmentName);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// Configurar FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Configurar MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Entradas.Aplicacion.Handlers.CrearEntradaCommandHandler).Assembly));

// Configurar capas de aplicación
builder.Services.AddAplicacion();

// Configurar infraestructura
builder.Services.AgregarInfraestructura(builder.Configuration);

// Configurar métricas y telemetría
builder.Services.AddMetricsConfiguration();

// Configurar health checks
builder.Services.AddHealthChecksConfiguration(builder.Configuration);

// Registrar health check personalizado del servicio de entradas
builder.Services.AddHealthChecks()
    .AddCheck<EntradaServiceHealthCheck>("entrada-service", tags: new[] { "business", "entradas" });

// Configurar Swagger/OpenAPI
builder.Services.AddSwaggerConfiguration();

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Aplicar migraciones automáticamente
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<Entradas.Infraestructura.Persistencia.EntradasDbContext>();
        context.Database.Migrate();
        Log.Information("Migraciones aplicadas exitosamente para EntradasDbContext");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Ocurrió un error al aplicar las migraciones para EntradasDbContext");
    }
}


// Configure the HTTP request pipeline
app.UseCorrelationId();
app.UseMetrics();
app.UseGlobalExceptionHandler();

// Middleware para logging de requests
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.FirstOrDefault());
        
        if (httpContext.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId))
        {
            diagnosticContext.Set("CorrelationId", correlationId.FirstOrDefault());
        }
    };
});

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerConfiguration(app.Environment);
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseRouting();

// Configurar Dashboard de Hangfire (Solo en desarrollo por seguridad, o añadir política)
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new Hangfire.Dashboard.LocalRequestsOnlyAuthorizationFilter() }
});

// Registrar Job recurrente (cada minuto para revisar expiraciones de 15 min)
using (var scope = app.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    recurringJobManager.AddOrUpdate<ExpiracionReservasJob>(
        "expiracion-reservas",
        job => job.EjecutarAsync(),
        Cron.Minutely());
}

app.MapControllers();

// Configurar endpoints de health checks
app.UseHealthChecksConfiguration();

Log.Information("Entradas.API iniciada exitosamente en {Environment}", app.Environment.EnvironmentName);

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Aplicación terminó inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
