using Microsoft.EntityFrameworkCore;
using Serilog;
using Usuarios.API.Filters;
using Usuarios.API.Middleware;
using Usuarios.Aplicacion;
using Usuarios.Infraestructura;
using Usuarios.Infraestructura.Persistencia;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "Usuarios.API")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Iniciando aplicación Usuarios.API");

    // Agregar servicios al contenedor
    builder.Services.AddControllers(options =>
    {
        options.Filters.Add<AuditoriaAttribute>();
    });
    builder.Services.AddEndpointsApiExplorer();
    
    // Configurar Swagger/OpenAPI
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "Usuarios API",
            Version = "v1",
            Description = "API para gestión de usuarios con Arquitectura Hexagonal y CQRS"
        });
        
        // Incluir comentarios XML si existen
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }
    });

    // Registrar servicios de Aplicación (MediatR, FluentValidation)
    builder.Services.AgregarAplicacion();

    // Registrar servicios de Infraestructura (DbContext, Repositorios, Keycloak)
    builder.Services.AgregarInfraestructura(builder.Configuration);

    // Configurar Health Checks
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<UsuariosDbContext>("database");

    // Configurar CORS (si es necesario)
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

    // Aplicar migraciones automáticamente al iniciar (solo para bases de datos relacionales)
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<UsuariosDbContext>();
            
            // Solo aplicar migraciones si es una base de datos relacional (no InMemory)
            if (dbContext.Database.IsRelational())
            {
                Log.Information("Aplicando migraciones de base de datos...");
                dbContext.Database.Migrate();
                Log.Information("Migraciones aplicadas exitosamente");
            }
            else
            {
                Log.Information("Usando base de datos InMemory, omitiendo migraciones");
                dbContext.Database.EnsureCreated();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error al aplicar migraciones de base de datos");
            throw;
        }
    }

    // Configurar el pipeline de HTTP request
    
    // Middleware de manejo de excepciones (debe ir primero)
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    // Swagger en todos los ambientes (para facilitar testing)
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Usuarios API v1");
        options.RoutePrefix = string.Empty; // Swagger en la raíz
    });

    // Logging de requests con Serilog
    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        };
    });

    app.UseHttpsRedirection();

    app.UseCors("AllowAll");

    app.UseAuthorization();

    app.MapControllers();

    // Health check endpoint
    app.MapHealthChecks("/health");

    Log.Information("Aplicación iniciada correctamente");
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación falló al iniciar");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

// Make the implicit Program class accessible to tests
public partial class Program { }
