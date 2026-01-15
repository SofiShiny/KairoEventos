using Asientos.Infraestructura.Persistencia;
using Asientos.Infraestructura.Repositorios;
using Asientos.Dominio.Repositorios;
using MediatR;
using MassTransit;
using Asientos.Aplicacion.Consumers;
using Microsoft.EntityFrameworkCore;
using System;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(_ => true) // Necesario para SignalR con credenciales
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddSignalR();

// DbContext
builder.Services.AddDbContext<AsientosDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Host=localhost;Database=asientos_db;Username=asientos_user;Password=asientos_pass";
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorCodesToAdd: null);
    });
});

builder.Services.AddScoped<IRepositorioMapaAsientos, MapaAsientosRepository>();

// MediatR
builder.Services.AddMediatR(typeof(Asientos.Aplicacion.Handlers.CrearMapaAsientosComandoHandler).Assembly);

// MassTransit con RabbitMQ
var rabbitMqHost = builder.Configuration["RabbitMq:Host"] ?? "localhost";
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ReservaCanceladaConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitMqHost, h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// Aplicar migraciones automáticamente al iniciar
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AsientosDbContext>();
        if (context.Database.IsRelational())
        {
            context.Database.Migrate();
            Console.WriteLine("Migraciones aplicadas exitosamente.");
            
            // Diagnóstico de columnas
            using var command = context.Database.GetDbConnection().CreateCommand();
            command.CommandText = "SELECT column_name FROM information_schema.columns WHERE table_name = 'Asientos'";
            context.Database.OpenConnection();
            using var reader = command.ExecuteReader();
            Console.WriteLine("Columnas en tabla Asientos:");
            while (reader.Read())
            {
                Console.WriteLine($"- {reader.GetString(0)}");
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al aplicar las migraciones.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();
app.MapHub<Asientos.Aplicacion.Hubs.AsientosHub>("/hub/asientos");

app.MapGet("/health", () => Results.Ok(new { status = "UP", rabbitmq = rabbitMqHost }));

app.Run();
