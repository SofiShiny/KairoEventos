using Servicios.Infraestructura;
using Microsoft.EntityFrameworkCore;
using Servicios.Infraestructura.Persistencia;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Agregar Capas
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Servicios.Aplicacion.Comandos.ReservarServicioCommand).Assembly));
builder.Services.AgregarInfraestructura(builder.Configuration);

// Background Workers
builder.Services.AddHostedService<Servicios.API.Workers.SincronizacionWorker>();

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
    var context = scope.ServiceProvider.GetRequiredService<ServiciosDbContext>();
    context.Database.Migrate();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
