using Hangfire;
using Hangfire.PostgreSql;
using Servicios.API.Hubs;
using Servicios.Infraestructura;
using Microsoft.EntityFrameworkCore;
using Servicios.Infraestructura.Persistencia;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ServiciosDbContext>("database");

// Hangfire
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"))));

builder.Services.AddHangfireServer();

// Jobs
builder.Services.AddScoped<Servicios.API.Jobs.NotificationJob>();

// Agregar Capas
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Servicios.Aplicacion.Comandos.ReservarServicioCommand).Assembly));
builder.Services.AgregarInfraestructura(builder.Configuration);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Authority desde env o default
        options.Authority = builder.Configuration["Authentication:Authority"] ?? "http://localhost:8180/realms/Kairo";
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false, // Deshabilitar para desarrollo (mismatch entre localhost:8180 y keycloak:8080)
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            NameClaimType = "preferred_username",
            RoleClaimType = "role" // O ClaimTypes.Role si usas System.Security.Claims
        };
        
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("AUTH_FAIL: " + context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("AUTH_SUCCESS: " + context.SecurityToken);
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Console.WriteLine("AUTH_CHALLENGE: " + context.Error + " " + context.ErrorDescription);
                return Task.CompletedTask;
            }
        };
    });

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
    Servicios.API.DbInitializer.Initialize(context);
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/live");
app.MapHub<ServiciosHub>("/hub/servicios");
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new MyDashboardAuthorizationFilter() }
});

app.Run();

public class MyDashboardAuthorizationFilter : Hangfire.Dashboard.IDashboardAuthorizationFilter
{
    public bool Authorize(Hangfire.Dashboard.DashboardContext context) => true;
}
