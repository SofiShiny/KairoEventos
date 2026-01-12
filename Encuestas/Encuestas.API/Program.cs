using Encuestas.Infraestructura;
using Microsoft.EntityFrameworkCore;
using Encuestas.Infraestructura.Persistencia;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Agregar Capas
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Encuestas.Aplicacion.Comandos.ResponderEncuestaCommand).Assembly));
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
    var context = scope.ServiceProvider.GetRequiredService<EncuestasDbContext>();
    try {
        context.Database.Migrate();
    } catch {
        // En un entorno dockerizado real, esperaríamos a que la DB esté lista
    }
}

app.UseAuthorization();
app.MapControllers();

app.Run();
