using Eventos.Infraestructura.Persistencia;
using Eventos.Dominio.Repositorios;
using Eventos.Infraestructura.Repositorios;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Filters;
using Eventos.API.Swagger.Examples;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Puerto configurable vía ASPNETCORE_URLS (docker-compose lo establecerá). Si no viene, usar http://0.0.0.0:5000
var urls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? builder.Configuration["ASPNETCORE_URLS"];
if (string.IsNullOrWhiteSpace(urls))
{
 urls = "http://0.0.0.0:5000"; //0.0.0.0 para exponer fuera del contenedor
}
builder.WebHost.UseUrls(urls);

builder.Services.AddControllers().AddJsonOptions(o =>
{
 o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerExamplesFromAssemblies(typeof(EventoCrearEjemploOperacion).Assembly);
builder.Services.AddSwaggerGen(o =>
{
 o.SwaggerDoc("v1", new() { Title = "Eventos API", Version = "v1" });
 o.ExampleFilters();
});

// Construir cadena de conexión priorizando variables de entorno en Docker
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

var useInMemory = string.IsNullOrWhiteSpace(cs) || cs.Equals("InMemory", StringComparison.OrdinalIgnoreCase) ||
 (Environment.GetEnvironmentVariable("USE_INMEMORY_DB")?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false);

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

builder.Services.AddMediatR(cfg =>
 cfg.RegisterServicesFromAssembly(typeof(Eventos.Aplicacion.AssemblyReference).Assembly));

builder.Services.AddScoped<IRepositorioEvento, EventoRepository>();

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

// Crear / migrar base de datos (migraciones > EnsureCreated para prod). Si no hay migraciones, cae a EnsureCreated.
using (var scope = app.Services.CreateScope())
{
 var db = scope.ServiceProvider.GetRequiredService<EventosDbContext>();
 try
 {
 if (!useInMemory)
 {
 // Intentar migrar; si no hay migraciones todavía, usar EnsureCreated
 var pending = (await db.Database.GetPendingMigrationsAsync()).Any();
 if (pending)
 {
 await db.Database.MigrateAsync();
 }
 else
 {
 await db.Database.EnsureCreatedAsync();
 }
 }
 }
 catch (Exception ex)
 {
 Console.WriteLine($"[DB-INIT] Error inicializando la base de datos: {ex.Message}");
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

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "eventos", db = useInMemory ? "in-memory" : "postgres" }));

app.MapGet("/health/db", async (EventosDbContext db) =>
{
 try
 {
 var can = await db.Database.CanConnectAsync();
 return can
 ? Results.Ok(new { ok = true, provider = db.Database.ProviderName })
 : Results.Problem("No se puede conectar a la base de datos", statusCode:503);
 }
 catch (Exception ex)
 {
 return Results.Problem(ex.Message, statusCode:500);
 }
});

app.Run();
