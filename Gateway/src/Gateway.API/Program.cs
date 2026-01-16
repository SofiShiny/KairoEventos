using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar YARP
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Agregar soporte para controladores, HttpClient y HealthChecks
builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddHealthChecks();

// 2. Configurar Autenticación (JWT con Keycloak)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Keycloak:Authority"] ?? builder.Configuration["Authentication:Authority"]; // Fallback
        options.Audience = builder.Configuration["Keycloak:Audience"] ?? builder.Configuration["Authentication:Audience"];
        options.RequireHttpsMetadata = false; 
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false, // Critical for docker/localhost mismatch
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            NameClaimType = "preferred_username",
            RoleClaimType = "role"
        };
    });

builder.Services.AddAuthorization();

// 3. Configurar CORS para el Frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000") // Orígenes del frontend
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Necesario para SignalR
    });
});

var app = builder.Build();

// 4. Pipeline del Middleware
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// Enrutar controladores
app.MapControllers();

// Health Checks
app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

// 5. Habilitar WebSockets para SignalR
app.UseWebSockets();

// Enrutar al Proxy
app.MapReverseProxy();

app.Run();
