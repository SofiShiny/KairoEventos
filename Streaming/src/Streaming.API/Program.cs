using System.Text.Json.Serialization;
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

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Ejecutar migraciones automáticamente
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<StreamingDbContext>();
    // Asegurarse de que el esquema y las tablas existan
    db.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
