using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Reportes.API.Middleware;
using System.Security.Claims;

namespace Reportes.API.Configuration;

public static class AuthenticationConfiguration
{
    public static WebApplicationBuilder ConfigureAuthentication(this WebApplicationBuilder builder)
    {
        // Authority desde env o default
        var authAuthority = Environment.GetEnvironmentVariable("AUTHENTICATION_AUTHORITY") 
                            ?? builder.Configuration["Authentication:Authority"] 
                            ?? "http://localhost:8180/realms/Kairo";
        
        var authAudience = Environment.GetEnvironmentVariable("AUTHENTICATION_AUDIENCE") 
                           ?? builder.Configuration["Authentication:Audience"] 
                           ?? "kairo-api";

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authAuthority;
                options.Audience = authAudience;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false, // Deshabilitar para desarrollo (mismatch entre localhost:8180 y keycloak:8080)
                    ValidateAudience = false, // Facilitar pruebas entre microservicios
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    NameClaimType = "preferred_username",
                    RoleClaimType = ClaimTypes.Role
                };
            });

        // Registrar el transformador de roles de Keycloak
        builder.Services.AddTransient<IClaimsTransformation, KeycloakRoleTransformer>();

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
            options.AddPolicy("OrganizatorOnly", policy => policy.RequireRole("Admin", "Organizator"));
        });

        return builder;
    }
}
