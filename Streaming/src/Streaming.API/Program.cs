using MassTransit;
using Microsoft.EntityFrameworkCore;
using Streaming.Aplicacion.Consumers;
using Streaming.Dominio.Interfaces;
using Streaming.Infraestructura.Persistencia;

var builder = WebApplication.CreateBuilder(args);

// Configuración de Base de Datos
builder.Services.AddDbContext<StreamingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registro de Dependencias (Arquitectura Hexagonal)
builder.Services.AddScoped<IRepositorioTransmisiones, RepositorioTransmisiones>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Configuración de MassTransit (RabbitMQ)
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<EventoPublicadoConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
        });

        cfg.ReceiveEndpoint("streaming-evento-publicado", e =>
        {
            e.ConfigureConsumer<EventoPublicadoConsumer>(context);
        });
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Ejecutar migraciones automáticamente (Opcional, pero útil para microservicios)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<StreamingDbContext>();
    db.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
