using Recomendaciones.Infraestructura;
using Microsoft.EntityFrameworkCore;
using Recomendaciones.Infraestructura.Persistencia;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Agregar Capas
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Recomendaciones.Aplicacion.Queries.ObtenerRecomendacionesQuery).Assembly));
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

app.UseAuthorization();
app.MapControllers();

app.Run();
