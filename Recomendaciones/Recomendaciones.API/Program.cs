using Recomendaciones.Infraestructura;
using Microsoft.EntityFrameworkCore;
using Recomendaciones.Infraestructura.Persistencia;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

// Configurar HttpClients para comunicación con otros microservicios
builder.Services.AddHttpClient("EventosApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:EventosApi"] ?? "http://localhost:5001");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient("EntradasApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:EntradasApi"] ?? "http://localhost:5009");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Agregar Capas
builder.Services.AddMediatR(cfg => 
{
    cfg.RegisterServicesFromAssembly(typeof(Recomendaciones.Aplicacion.Queries.ObtenerTendenciasQuery).Assembly);
});
builder.Services.AgregarInfraestructura(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Aplicar migraciones al inicio
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<RecomendacionesDbContext>();
    // Intentar aplicar migraciones. Nota: En producción esto debería estar en un pipeline de CI/CD
    try {
        context.Database.Migrate();
    } catch {
        // Log error or ignore if DB not ready
    }
}

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
