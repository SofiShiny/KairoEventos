using Comunidad.Application.Comandos;
using Comunidad.Domain.Repositorios;
using Comunidad.Infrastructure.Consumers;
using Comunidad.Infrastructure.Persistencia;
using Comunidad.Infrastructure.Repositorios;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Configuración de MongoDB
var mongoConnectionString = builder.Configuration.GetConnectionString("MongoDB") 
    ?? "mongodb://mongodb:27017";
var mongoDatabaseName = builder.Configuration["MongoDB:DatabaseName"] ?? "ComunidadDB";

builder.Services.AddSingleton(new MongoDbContext(mongoConnectionString, mongoDatabaseName));

// Repositorios
builder.Services.AddScoped<IForoRepository, ForoRepository>();
builder.Services.AddScoped<IComentarioRepository, ComentarioRepository>();

// MediatR
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(CrearComentarioComando).Assembly));

// MassTransit con RabbitMQ
builder.Services.AddMassTransit(x =>
{
    // Registrar el consumer
    x.AddConsumer<EventoPublicadoConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitHost = builder.Configuration["RabbitMQ:Host"] ?? "rabbitmq";
        var rabbitUser = builder.Configuration["RabbitMQ:Username"] ?? "guest";
        var rabbitPass = builder.Configuration["RabbitMQ:Password"] ?? "guest";

        cfg.Host(rabbitHost, "/", h =>
        {
            h.Username(rabbitUser);
            h.Password(rabbitPass);
        });

        // Configurar endpoint para el consumer
        cfg.ReceiveEndpoint("comunidad-evento-publicado", e =>
        {
            e.ConfigureConsumer<EventoPublicadoConsumer>(context);
        });
    });
});

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "Comunidad API", 
        Version = "v1",
        Description = "API de Foros y Comentarios para Eventos"
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Health Checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
