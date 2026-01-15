using Pagos.Infraestructura;
using Pagos.Aplicacion.Jobs;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using MassTransit;
using QuestPDF.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Pagos.API.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// QuestPDF License
QuestPDF.Settings.License = LicenseType.Community;

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Autenticación JWT con Keycloak
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Authentication:Authority"]; // http://localhost:8180/realms/Kairo
        options.Audience = builder.Configuration["Authentication:Audience"];   // gateway-client o similar
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false, // Deshabilitar para desarrollo (mismatch entre localhost:8180 y keycloak:8080)
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            NameClaimType = "preferred_username",
            RoleClaimType = ClaimTypes.Role // Usar el tipo de claim estándar que llenará nuestro transformador
        };
    });

builder.Services.AddTransient<IClaimsTransformation, KeycloakRoleTransformer>();

builder.Services.AddAuthorization();

builder.Services.AddInfraestructura(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


// MassTransit
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMq:Host"] ?? "localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
    });
});

var app = builder.Build();

// Aplicar migraciones automáticamente
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<Pagos.Infraestructura.Persistencia.PagosDbContext>();
    context.Database.Migrate();
}


app.UseSwagger();
app.UseSwaggerUI();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Hangfire Dashboard
app.UseHangfireDashboard();

// TC-052: Programar Job de Conciliación
using (var scope = app.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    
    // Antiguo job diario
    recurringJobManager.AddOrUpdate<ConciliacionJob>(
        "ConciliacionDiaria", 
        job => job.EjecutarConciliacionDiariaAsync(), 
        Cron.Daily);

    // Nuevo job de conciliación automática cada hora
    recurringJobManager.AddOrUpdate<ConciliacionPagosJob>(
        "ConciliacionAutomaticaPendientes", 
        job => job.ConciliarPendientesAsync(), 
        Cron.Hourly);
}

app.Run();

// Hacer la clase Program accesible para tests de integración
public partial class Program { }
