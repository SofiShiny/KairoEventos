using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Notificaciones.API.Hubs;
using Notificaciones.API.Services;
using Notificaciones.Aplicacion.Consumers;
using Notificaciones.Aplicacion.Interfaces;
using System.Security.Claims;

using Notificaciones.Dominio.Interfaces;
using Notificaciones.Infraestructura.Servicios;

var builder = WebApplication.CreateBuilder(args);

// ===== CORS =====
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Importante para SignalR
    });
});

// ===== Servicios de Infraestructura (Emails y Usuarios Externos) =====
builder.Services.Configure<ConfiguracionEmail>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IServicioEmail, ServicioEmailSmtp>();
builder.Services.AddTransient<IServicioRecibo, ServicioRecibo>();
builder.Services.AddScoped<IServicioUsuarios, ServicioUsuariosHttp>();
builder.Services.AddHttpClient<ServicioUsuariosHttp>();

// ===== Servicios de Aplicación =====
// Registramos el notificador que desacopla la lógica de consumo del Hub
builder.Services.AddTransient<INotificador, SignalRNotificador>();

// ===== Autenticación JWT =====
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var authority = builder.Configuration["Authentication:Authority"] 
                     ?? "http://keycloak:8080/realms/Kairo";
        
        options.Authority = authority;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,   // Crucial: Acepta tokens de localhost o keycloak
            ValidateAudience = false, // Ignora la audiencia para facilitar la conexión
            ValidateLifetime = true,
            NameClaimType = ClaimTypes.NameIdentifier
        };

        // Configuración especial para SignalR
        // Permite leer el token desde query string (para WebSockets)
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                
                if (!string.IsNullOrEmpty(accessToken) && 
                    path.StartsWithSegments("/hub"))
                {
                    context.Token = accessToken;
                }
                
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// ===== SignalR =====
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

// ===== MassTransit (RabbitMQ) =====
builder.Services.AddMassTransit(x =>
{
    // Registrar el Consumer
    x.AddConsumer<PagoAprobadoConsumer>();

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

        // Configurar el endpoint para el Consumer
        cfg.ReceiveEndpoint("notificaciones-pago-aprobado", e =>
        {
            e.ConfigureConsumer<PagoAprobadoConsumer>(context);
        });
    });
});

// ===== Controllers & Swagger =====
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ===== Health Checks =====
builder.Services.AddHealthChecks();

var app = builder.Build();

// ===== Middleware =====
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// ===== SignalR Hub Mapping =====
app.MapHub<NotificacionesHub>("/hub/notificaciones");

app.Run();
