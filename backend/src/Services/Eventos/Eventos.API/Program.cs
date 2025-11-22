using Eventos.Infraestructura.Persistencia;
using Eventos.Dominio.Repositorios;
using Eventos.Infraestructura.Repositorios;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Filters;
using System.Text.Json.Serialization;
using Eventos.Aplicacion;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuracion de servicios
var urls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "http://0.0.0.0:5000";
builder.WebHost.UseUrls(urls);

var pathBase = Environment.GetEnvironmentVariable("PATH_BASE");

builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddEndpointsApiExplorer();
// Aadir ejemplos
builder.Services.AddSwaggerExamplesFromAssemblies(typeof(Program).Assembly);
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new() { Title = "Eventos API", Version = "v1" });
    o.ExampleFilters();
});

var cs = builder.Configuration.GetConnectionString("DefaultConnection");
var pgHost = Environment.GetEnvironmentVariable("POSTGRES_HOST");
if (!string.IsNullOrWhiteSpace(pgHost))
{
    var pgDb = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "eventsdb";
    var pgUser = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "postgres";
    var pgPass = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "postgres";
    var pgPort = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5432";
    cs = $"Host={pgHost};Port={pgPort};Database={pgDb};Username={pgUser};Password={pgPass}";
}

var useInMemory = string.IsNullOrWhiteSpace(cs) || cs.Equals("InMemory", StringComparison.OrdinalIgnoreCase);
builder.Services.AddDbContext<EventosDbContext>(options =>
{
    if (useInMemory)
    {
        options.UseInMemoryDatabase("EventosDb");
    }
    else
    {
        options.UseNpgsql(cs!);
    }
});

// Registrar servicios de la capa de Aplicación
builder.Services.AddEventAplicacionServices();

builder.Services.AddScoped<IRepositorioEvento, EventoRepository>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// 2. Construccion y configuracion del pipeline
var app = builder.Build();

if (!string.IsNullOrWhiteSpace(pathBase))
{
    app.UsePathBase(pathBase);
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EventosDbContext>();
    try
    {
        if (!useInMemory && db.Database.GetPendingMigrations().Any())
        {
            await db.Database.MigrateAsync();
        }
        else if (!useInMemory)
        {
            await db.Database.EnsureCreatedAsync();
        }
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error inicializando la base de datos.");
    }
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Eventos API v1");
    c.RoutePrefix = "swagger";
});

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.MapGet("/health", (EventosDbContext db) => Results.Ok(new { 
    status = "healthy",
    database = db.Database.IsNpgsql() ? "PostgreSQL" : "InMemory"
}));

app.MapGet("/health/db", async (EventosDbContext db) =>
{
    try
    {
        return await db.Database.CanConnectAsync()
            ? Results.Ok(new { status = "healthy" })
            : Results.Problem("No se puede conectar a la base de datos.", statusCode: 503);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message, statusCode: 500);
    }
});

app.Run();
