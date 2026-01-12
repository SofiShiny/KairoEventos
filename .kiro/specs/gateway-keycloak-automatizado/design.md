# Design Document - Gateway y Keycloak Automatizado

## Overview

Este diseño implementa un API Gateway profesional usando YARP (Yet Another Reverse Proxy) de Microsoft con autenticación y autorización centralizada mediante Keycloak. La solución incluye la automatización completa de la configuración de Keycloak mediante un archivo realm-export.json que se importa automáticamente al iniciar Docker, eliminando la necesidad de configuración manual.

El Gateway actúa como punto de entrada único para todos los microservicios (Eventos, Asientos, Usuarios, Entradas, Reportes), manejando:
- Enrutamiento inteligente basado en rutas
- Validación de tokens JWT
- Autorización basada en roles
- CORS para frontends
- Health checks y observabilidad

## Architecture

### High-Level Architecture

```
┌─────────────────┐
│   Frontend      │
│  (React/Vite)   │
└────────┬────────┘
         │ HTTP + JWT
         ▼
┌─────────────────────────────────────────┐
│         API Gateway (YARP)              │
│  ┌───────────────────────────────────┐  │
│  │  Authentication Middleware        │  │
│  │  (JWT Validation)                 │  │
│  └───────────────────────────────────┘  │
│  ┌───────────────────────────────────┐  │
│  │  Authorization Middleware         │  │
│  │  (Role-Based Access Control)      │  │
│  └───────────────────────────────────┘  │
│  ┌───────────────────────────────────┐  │
│  │  YARP Reverse Proxy               │  │
│  │  (Route Matching & Forwarding)    │  │
│  └───────────────────────────────────┘  │
└──────┬──────┬──────┬──────┬──────┬──────┘
       │      │      │      │      │
       ▼      ▼      ▼      ▼      ▼
    ┌────┐ ┌────┐ ┌────┐ ┌────┐ ┌────┐
    │Evts│ │Asnt│ │Usrs│ │Entr│ │Rpts│
    └────┘ └────┘ └────┘ └────┘ └────┘
       
         ▲ JWT Validation
         │
    ┌────────────┐
    │  Keycloak  │
    │  (IAM)     │
    └────────────┘
```

### Component Diagram

```
Gateway.API/
├── Program.cs                    # Configuración principal
├── appsettings.json             # Configuración de rutas YARP
├── appsettings.Development.json # Configuración de desarrollo
├── Middleware/
│   ├── RequestLoggingMiddleware.cs
│   └── ExceptionHandlingMiddleware.cs
├── Configuration/
│   ├── YarpConfiguration.cs
│   ├── AuthenticationConfiguration.cs
│   └── AuthorizationConfiguration.cs
└── HealthChecks/
    └── KeycloakHealthCheck.cs
```

## Components and Interfaces

### 1. YARP Reverse Proxy Configuration

**Responsibility:** Configurar el enrutamiento de peticiones a microservicios

**Configuration Structure (appsettings.json):**

```json
{
  "ReverseProxy": {
    "Routes": {
      "eventos-route": {
        "ClusterId": "eventos-cluster",
        "Match": {
          "Path": "/api/eventos/{**catch-all}"
        },
        "Transforms": [
          { "PathPattern": "/api/{**catch-all}" }
        ]
      },
      "asientos-route": {
        "ClusterId": "asientos-cluster",
        "Match": {
          "Path": "/api/asientos/{**catch-all}"
        },
        "Transforms": [
          { "PathPattern": "/api/{**catch-all}" }
        ]
      },
      "usuarios-route": {
        "ClusterId": "usuarios-cluster",
        "Match": {
          "Path": "/api/usuarios/{**catch-all}"
        },
        "Transforms": [
          { "PathPattern": "/api/{**catch-all}" }
        ]
      },
      "entradas-route": {
        "ClusterId": "entradas-cluster",
        "Match": {
          "Path": "/api/entradas/{**catch-all}"
        },
        "Transforms": [
          { "PathPattern": "/api/{**catch-all}" }
        ]
      },
      "reportes-route": {
        "ClusterId": "reportes-cluster",
        "Match": {
          "Path": "/api/reportes/{**catch-all}"
        },
        "Transforms": [
          { "PathPattern": "/api/{**catch-all}" }
        ]
      }
    },
    "Clusters": {
      "eventos-cluster": {
        "Destinations": {
          "destination1": {
            "Address": "http://eventos-api:8080"
          }
        }
      },
      "asientos-cluster": {
        "Destinations": {
          "destination1": {
            "Address": "http://asientos-api:8080"
          }
        }
      },
      "usuarios-cluster": {
        "Destinations": {
          "destination1": {
            "Address": "http://usuarios-api:8080"
          }
        }
      },
      "entradas-cluster": {
        "Destinations": {
          "destination1": {
            "Address": "http://entradas-api:8080"
          }
        }
      },
      "reportes-cluster": {
        "Destinations": {
          "destination1": {
            "Address": "http://reportes-api:8080"
          }
        }
      }
    }
  }
}
```

### 2. JWT Authentication Configuration

**Responsibility:** Validar tokens JWT emitidos por Keycloak

**Interface:**

```csharp
public static class AuthenticationConfiguration
{
    public static IServiceCollection AddKeycloakAuthentication(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = configuration["Keycloak:Authority"];
                options.Audience = configuration["Keycloak:Audience"];
                options.RequireHttpsMetadata = false; // Solo para desarrollo
                
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Keycloak:Authority"],
                    ValidAudience = configuration["Keycloak:Audience"],
                    RoleClaimType = "roles",
                    NameClaimType = "preferred_username"
                };
                
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        // Log authentication failures
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        // Log successful validations
                        return Task.CompletedTask;
                    }
                };
            });
        
        return services;
    }
}
```

**Configuration (appsettings.json):**

```json
{
  "Keycloak": {
    "Authority": "http://keycloak:8080/realms/Kairo",
    "Audience": "kairo-api",
    "MetadataAddress": "http://keycloak:8080/realms/Kairo/.well-known/openid-configuration"
  }
}
```

### 3. Authorization Policies

**Responsibility:** Definir políticas de autorización basadas en roles

**Interface:**

```csharp
public static class AuthorizationConfiguration
{
    public static IServiceCollection AddRoleBasedAuthorization(
        this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // Política para usuarios autenticados
            options.AddPolicy("Authenticated", policy =>
                policy.RequireAuthenticatedUser());
            
            // Política para usuarios con rol User
            options.AddPolicy("UserAccess", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireRole("User"));
            
            // Política para administradores
            options.AddPolicy("AdminAccess", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireRole("Admin"));
            
            // Política para organizadores
            options.AddPolicy("OrganizatorAccess", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireRole("Organizator"));
            
            // Política para organizadores o admins
            options.AddPolicy("EventManagement", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireRole("Admin", "Organizator"));
        });
        
        return services;
    }
}
```

### 4. CORS Configuration

**Responsibility:** Permitir peticiones desde frontends

**Interface:**

```csharp
public static class CorsConfiguration
{
    public static IServiceCollection AddCorsPolicy(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? new[] { "http://localhost:5173" };
        
        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontends", policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });
        
        return services;
    }
}
```

### 5. Request Logging Middleware

**Responsibility:** Registrar todas las peticiones HTTP

**Interface:**

```csharp
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    
    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        var startTime = DateTime.UtcNow;
        var requestId = Guid.NewGuid().ToString();
        
        _logger.LogInformation(
            "Request {RequestId}: {Method} {Path} started at {StartTime}",
            requestId,
            context.Request.Method,
            context.Request.Path,
            startTime);
        
        try
        {
            await _next(context);
            
            var duration = DateTime.UtcNow - startTime;
            _logger.LogInformation(
                "Request {RequestId}: {Method} {Path} completed with {StatusCode} in {Duration}ms",
                requestId,
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                duration.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Request {RequestId}: {Method} {Path} failed",
                requestId,
                context.Request.Method,
                context.Request.Path);
            throw;
        }
    }
}
```

### 6. Keycloak Health Check

**Responsibility:** Verificar conectividad con Keycloak

**Interface:**

```csharp
public class KeycloakHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<KeycloakHealthCheck> _logger;
    
    public KeycloakHealthCheck(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<KeycloakHealthCheck> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }
    
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var metadataUrl = _configuration["Keycloak:MetadataAddress"];
            
            var response = await client.GetAsync(metadataUrl, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy("Keycloak is reachable");
            }
            
            return HealthCheckResult.Degraded(
                $"Keycloak returned status code {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Keycloak health check failed");
            return HealthCheckResult.Unhealthy(
                "Keycloak is not reachable",
                ex);
        }
    }
}
```

## Data Models

### JWT Token Claims

```csharp
public class JwtClaims
{
    public string Sub { get; set; }              // User ID
    public string PreferredUsername { get; set; } // Username
    public string Email { get; set; }            // Email
    public string[] Roles { get; set; }          // User roles
    public long Exp { get; set; }                // Expiration timestamp
    public long Iat { get; set; }                // Issued at timestamp
    public string Iss { get; set; }              // Issuer (Keycloak)
    public string Aud { get; set; }              // Audience (kairo-api)
}
```

### Keycloak Realm Export Structure

```json
{
  "realm": "Kairo",
  "enabled": true,
  "sslRequired": "none",
  "registrationAllowed": false,
  "loginWithEmailAllowed": true,
  "duplicateEmailsAllowed": false,
  "resetPasswordAllowed": true,
  "editUsernameAllowed": false,
  "bruteForceProtected": true,
  
  "accessTokenLifespan": 300,
  "accessTokenLifespanForImplicitFlow": 900,
  "ssoSessionIdleTimeout": 1800,
  "ssoSessionMaxLifespan": 36000,
  "offlineSessionIdleTimeout": 2592000,
  "accessCodeLifespan": 60,
  "accessCodeLifespanUserAction": 300,
  "accessCodeLifespanLogin": 1800,
  
  "roles": {
    "realm": [
      {
        "name": "User",
        "description": "Usuario regular del sistema"
      },
      {
        "name": "Admin",
        "description": "Administrador con acceso completo"
      },
      {
        "name": "Organizator",
        "description": "Organizador de eventos"
      }
    ]
  },
  
  "clients": [
    {
      "clientId": "kairo-web",
      "enabled": true,
      "publicClient": true,
      "protocol": "openid-connect",
      "standardFlowEnabled": true,
      "implicitFlowEnabled": false,
      "directAccessGrantsEnabled": true,
      "serviceAccountsEnabled": false,
      "redirectUris": [
        "http://localhost:5173/*",
        "http://localhost:3000/*"
      ],
      "webOrigins": [
        "http://localhost:5173",
        "http://localhost:3000"
      ],
      "attributes": {
        "pkce.code.challenge.method": "S256"
      }
    },
    {
      "clientId": "kairo-api",
      "enabled": true,
      "publicClient": false,
      "protocol": "openid-connect",
      "bearerOnly": true,
      "standardFlowEnabled": false,
      "directAccessGrantsEnabled": false,
      "serviceAccountsEnabled": false
    }
  ],
  
  "users": [
    {
      "username": "admin",
      "enabled": true,
      "email": "admin@kairo.com",
      "emailVerified": true,
      "firstName": "Admin",
      "lastName": "User",
      "credentials": [
        {
          "type": "password",
          "value": "admin123",
          "temporary": false
        }
      ],
      "realmRoles": ["Admin", "User"],
      "clientRoles": {}
    },
    {
      "username": "organizador",
      "enabled": true,
      "email": "organizador@kairo.com",
      "emailVerified": true,
      "firstName": "Organizador",
      "lastName": "User",
      "credentials": [
        {
          "type": "password",
          "value": "org123",
          "temporary": false
        }
      ],
      "realmRoles": ["Organizator", "User"],
      "clientRoles": {}
    },
    {
      "username": "usuario",
      "enabled": true,
      "email": "usuario@kairo.com",
      "emailVerified": true,
      "firstName": "Usuario",
      "lastName": "Regular",
      "credentials": [
        {
          "type": "password",
          "value": "user123",
          "temporary": false
        }
      ],
      "realmRoles": ["User"],
      "clientRoles": {}
    }
  ]
}
```

## Correctness Properties

*A property is a characteristic or behavior that should hold true across all valid executions of a system—essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.*

### Property 1: Route Matching Consistency
*For any* HTTP request with a path matching `/api/{service}/*` where service is one of [eventos, asientos, usuarios, entradas, reportes], the Gateway should route the request to the corresponding microservice cluster.
**Validates: Requirements 1.1, 1.2, 1.3, 1.4, 1.5**

### Property 2: Service Unavailability Handling
*For any* request to a microservice that is unavailable or unreachable, the Gateway should return HTTP 503 Service Unavailable.
**Validates: Requirements 1.6**

### Property 3: Valid Token Authentication
*For any* HTTP request with a valid JWT token in the Authorization header, the Gateway should successfully validate the token and extract user claims (sub, roles, email, username).
**Validates: Requirements 2.1, 2.5**

### Property 4: Role-Based Authorization
*For any* authenticated user with a specific role attempting to access an endpoint, the Gateway should grant or deny access based on the endpoint's required roles, where:
- Users with "Admin" role can access all endpoints
- Users with "Organizator" role can access event and seat management endpoints
- Users with "User" role can access user-level endpoints
**Validates: Requirements 3.1, 3.2, 3.3, 3.4**

### Property 5: Role Claim Extraction
*For any* valid JWT token, the Gateway should extract roles from the "roles" claim in the token payload.
**Validates: Requirements 3.5**

### Property 6: CORS Header Presence
*For any* HTTP request from an allowed origin (localhost:5173 or localhost:3000), the Gateway should include appropriate CORS headers (Access-Control-Allow-Origin, Access-Control-Allow-Methods, Access-Control-Allow-Headers) in the response.
**Validates: Requirements 6.1, 6.2, 6.3, 6.4, 6.5**

### Property 7: Request Logging Completeness
*For any* HTTP request processed by the Gateway, a log entry should be created containing the request method, path, timestamp, and response status code.
**Validates: Requirements 8.1, 8.5**

### Property 8: Authentication Logging
*For any* JWT token validation attempt, the Gateway should log the validation result (success or failure) with appropriate log level (Information for success, Warning for failure).
**Validates: Requirements 8.2, 8.3**

### Property 9: Authorization Logging
*For any* authorization decision (grant or deny), the Gateway should log the username and the resource being accessed.
**Validates: Requirements 8.4**

### Property 10: Environment Variable Configuration
*For any* required configuration value (Keycloak URL, microservice URLs, CORS origins), the Gateway should read the value from environment variables if present, otherwise use default values for development.
**Validates: Requirements 10.1, 10.2, 10.3, 10.4**

### Property 11: Startup Configuration Validation
*For any* Gateway startup, if required environment variables are missing and no defaults are available, the Gateway should fail to start with a clear error message indicating which variables are missing.
**Validates: Requirements 10.5**



## Error Handling

### Authentication Errors

**401 Unauthorized:**
- Missing Authorization header on protected endpoints
- Invalid JWT token format
- Expired JWT token
- Token signature validation failure
- Token issuer mismatch

**Response Format:**
```json
{
  "error": "Unauthorized",
  "message": "Invalid or missing authentication token",
  "timestamp": "2024-12-30T10:30:00Z",
  "path": "/api/eventos/123"
}
```

### Authorization Errors

**403 Forbidden:**
- User lacks required role for endpoint
- User account is disabled
- Token is valid but user doesn't have permission

**Response Format:**
```json
{
  "error": "Forbidden",
  "message": "Insufficient permissions to access this resource",
  "requiredRoles": ["Admin", "Organizator"],
  "userRoles": ["User"],
  "timestamp": "2024-12-30T10:30:00Z",
  "path": "/api/eventos/123"
}
```

### Service Unavailability Errors

**503 Service Unavailable:**
- Downstream microservice is not responding
- Downstream microservice returned 5xx error
- Connection timeout to microservice

**Response Format:**
```json
{
  "error": "Service Unavailable",
  "message": "The requested service is temporarily unavailable",
  "service": "eventos-api",
  "timestamp": "2024-12-30T10:30:00Z",
  "path": "/api/eventos/123"
}
```

### CORS Errors

**403 Forbidden (CORS):**
- Request from disallowed origin
- Preflight request failed

**Response:** Standard CORS error with missing CORS headers

### Health Check Errors

**503 Service Unavailable:**
- Keycloak is not reachable
- Critical dependency is down

**Response Format:**
```json
{
  "status": "Unhealthy",
  "checks": {
    "keycloak": {
      "status": "Unhealthy",
      "description": "Keycloak is not reachable",
      "error": "Connection timeout"
    }
  },
  "timestamp": "2024-12-30T10:30:00Z"
}
```

### Error Handling Middleware

```csharp
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (SecurityTokenExpiredException ex)
        {
            await HandleAuthenticationErrorAsync(context, ex, "Token has expired");
        }
        catch (SecurityTokenInvalidSignatureException ex)
        {
            await HandleAuthenticationErrorAsync(context, ex, "Invalid token signature");
        }
        catch (SecurityTokenException ex)
        {
            await HandleAuthenticationErrorAsync(context, ex, "Invalid token");
        }
        catch (HttpRequestException ex)
        {
            await HandleServiceUnavailableAsync(context, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in Gateway");
            await HandleInternalErrorAsync(context, ex);
        }
    }
    
    private async Task HandleAuthenticationErrorAsync(
        HttpContext context, 
        Exception ex, 
        string message)
    {
        _logger.LogWarning(ex, "Authentication error: {Message}", message);
        
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";
        
        var response = new
        {
            error = "Unauthorized",
            message = message,
            timestamp = DateTime.UtcNow,
            path = context.Request.Path.Value
        };
        
        await context.Response.WriteAsJsonAsync(response);
    }
    
    private async Task HandleServiceUnavailableAsync(
        HttpContext context, 
        Exception ex)
    {
        _logger.LogError(ex, "Service unavailable error");
        
        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        context.Response.ContentType = "application/json";
        
        var response = new
        {
            error = "Service Unavailable",
            message = "The requested service is temporarily unavailable",
            timestamp = DateTime.UtcNow,
            path = context.Request.Path.Value
        };
        
        await context.Response.WriteAsJsonAsync(response);
    }
}
```

## Testing Strategy

### Unit Tests

Unit tests will focus on specific components and their behavior in isolation:

**Configuration Tests:**
- Test that YARP routes are loaded correctly from appsettings.json
- Test that authentication configuration is applied correctly
- Test that authorization policies are registered correctly
- Test that CORS policies are configured correctly
- Test that environment variables are read and defaults are applied

**Middleware Tests:**
- Test RequestLoggingMiddleware logs all requests
- Test ExceptionHandlingMiddleware handles different exception types
- Test that authentication middleware validates tokens correctly
- Test that authorization middleware enforces role-based access

**Health Check Tests:**
- Test KeycloakHealthCheck returns Healthy when Keycloak is reachable
- Test KeycloakHealthCheck returns Unhealthy when Keycloak is down
- Test health check endpoints return correct status codes

### Integration Tests

Integration tests will verify the Gateway works correctly with real dependencies:

**End-to-End Routing Tests:**
- Test requests to /api/eventos/* are routed to Eventos service
- Test requests to /api/asientos/* are routed to Asientos service
- Test requests to /api/usuarios/* are routed to Usuarios service
- Test requests to /api/entradas/* are routed to Entradas service
- Test requests to /api/reportes/* are routed to Reportes service

**Authentication Integration Tests:**
- Test Gateway validates real JWT tokens from Keycloak
- Test Gateway rejects expired tokens
- Test Gateway rejects invalid tokens
- Test Gateway extracts claims correctly from valid tokens

**Authorization Integration Tests:**
- Test users with Admin role can access all endpoints
- Test users with Organizator role can access event endpoints
- Test users with User role are denied access to admin endpoints
- Test unauthenticated requests are rejected

**CORS Integration Tests:**
- Test preflight requests from allowed origins succeed
- Test requests from disallowed origins are rejected
- Test CORS headers are present in responses

**Health Check Integration Tests:**
- Test /health endpoint returns 200 when all services are up
- Test /health/ready endpoint checks Keycloak connectivity
- Test /health/live endpoint always returns 200

### Property-Based Tests

Property-based tests will verify universal properties across many generated inputs:

**Property Test 1: Route Matching Consistency**
- Generate random paths under /api/{service}/*
- Verify all paths are routed to the correct cluster
- Run 100+ iterations with different path combinations

**Property Test 2: Valid Token Authentication**
- Generate valid JWT tokens with different claims
- Verify all tokens are validated successfully
- Verify claims are extracted correctly
- Run 100+ iterations with different token payloads

**Property Test 3: Role-Based Authorization**
- Generate users with different role combinations
- Generate endpoints with different role requirements
- Verify authorization decisions are correct
- Run 100+ iterations with different role/endpoint combinations

**Property Test 4: CORS Header Presence**
- Generate requests from different origins
- Verify CORS headers are present for allowed origins
- Verify CORS headers are absent for disallowed origins
- Run 100+ iterations with different origins

**Property Test 5: Request Logging Completeness**
- Generate random HTTP requests
- Verify all requests produce log entries
- Verify log entries contain required fields
- Run 100+ iterations with different request types

### Testing Framework

- **Unit Tests:** xUnit + Moq + FluentAssertions
- **Integration Tests:** xUnit + WebApplicationFactory + Testcontainers
- **Property-Based Tests:** FsCheck.Xunit
- **Coverage Target:** >90% code coverage

### Test Execution

```bash
# Run all tests
dotnet test

# Run unit tests only
dotnet test --filter Category=Unit

# Run integration tests only
dotnet test --filter Category=Integration

# Run property-based tests only
dotnet test --filter Category=Property

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## Deployment Configuration

### Docker Compose Configuration

**File: Infraestructura/docker-compose.yml**

```yaml
version: '3.8'

networks:
  kairo-network:
    driver: bridge
    name: kairo-network

services:
  # Keycloak - Identity and Access Management
  keycloak:
    image: quay.io/keycloak/keycloak:23.0
    container_name: kairo-keycloak
    restart: unless-stopped
    environment:
      KEYCLOAK_ADMIN: admin
      KEYCLOAK_ADMIN_PASSWORD: admin
      KC_DB: postgres
      KC_DB_URL: jdbc:postgresql://postgres:5432/keycloak
      KC_DB_USERNAME: postgres
      KC_DB_PASSWORD: postgres
      KC_HOSTNAME_STRICT: false
      KC_HTTP_ENABLED: true
      KC_HEALTH_ENABLED: true
    command:
      - start-dev
      - --import-realm
    ports:
      - "8180:8080"
    volumes:
      - ./keycloak/realm-export.json:/opt/keycloak/data/import/realm-export.json:ro
    networks:
      - kairo-network
    depends_on:
      postgres:
        condition: service_healthy
    healthcheck:
      test: ["CMD-SHELL", "exec 3<>/dev/tcp/localhost/8080 && echo -e 'GET /health/ready HTTP/1.1\\r\\nHost: localhost\\r\\nConnection: close\\r\\n\\r\\n' >&3 && cat <&3 | grep -q '200 OK'"]
      interval: 10s
      timeout: 5s
      retries: 10
      start_period: 60s

  # API Gateway
  gateway:
    build:
      context: ../Gateway
      dockerfile: Dockerfile
    container_name: kairo-gateway
    restart: unless-stopped
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      ASPNETCORE_URLS: http://+:8080
      Keycloak__Authority: http://keycloak:8080/realms/Kairo
      Keycloak__Audience: kairo-api
      Keycloak__MetadataAddress: http://keycloak:8080/realms/Kairo/.well-known/openid-configuration
      Cors__AllowedOrigins__0: http://localhost:5173
      Cors__AllowedOrigins__1: http://localhost:3000
      ReverseProxy__Clusters__eventos-cluster__Destinations__destination1__Address: http://eventos-api:8080
      ReverseProxy__Clusters__asientos-cluster__Destinations__destination1__Address: http://asientos-api:8080
      ReverseProxy__Clusters__usuarios-cluster__Destinations__destination1__Address: http://usuarios-api:8080
      ReverseProxy__Clusters__entradas-cluster__Destinations__destination1__Address: http://entradas-api:8080
      ReverseProxy__Clusters__reportes-cluster__Destinations__destination1__Address: http://reportes-api:8080
    ports:
      - "8080:8080"
    networks:
      - kairo-network
    depends_on:
      keycloak:
        condition: service_healthy
    healthcheck:
      test: ["CMD-SHELL", "curl -f http://localhost:8080/health || exit 1"]
      interval: 10s
      timeout: 5s
      retries: 5
      start_period: 30s

  # PostgreSQL (existing)
  postgres:
    image: postgres:16-alpine
    container_name: kairo-postgres
    restart: unless-stopped
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
      POSTGRES_DB: postgres
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
      - ./configs/postgres/init.sql:/docker-entrypoint-initdb.d/init.sql
    networks:
      - kairo-network
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 10s
      timeout: 5s
      retries: 5

  # MongoDB (existing)
  mongodb:
    image: mongo:7
    container_name: kairo-mongodb
    restart: unless-stopped
    environment:
      MONGO_INITDB_DATABASE: kairo_reportes
    ports:
      - "27017:27017"
    volumes:
      - mongodb_data:/data/db
    networks:
      - kairo-network
    healthcheck:
      test: ["CMD", "mongosh", "--eval", "db.adminCommand('ping')"]
      interval: 10s
      timeout: 5s
      retries: 5

  # RabbitMQ (existing)
  rabbitmq:
    image: rabbitmq:3-management
    container_name: kairo-rabbitmq
    restart: unless-stopped
    environment:
      RABBITMQ_DEFAULT_USER: guest
      RABBITMQ_DEFAULT_PASS: guest
    ports:
      - "5672:5672"
      - "15672:15672"
    volumes:
      - rabbitmq_data:/var/lib/rabbitmq
    networks:
      - kairo-network
    healthcheck:
      test: ["CMD", "rabbitmq-diagnostics", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5

volumes:
  postgres_data:
    name: kairo_postgres_data
  mongodb_data:
    name: kairo_mongodb_data
  rabbitmq_data:
    name: kairo_rabbitmq_data
```

### Gateway Dockerfile

**File: Gateway/Dockerfile**

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Gateway.API/Gateway.API.csproj", "src/Gateway.API/"]
RUN dotnet restore "src/Gateway.API/Gateway.API.csproj"
COPY . .
WORKDIR "/src/src/Gateway.API"
RUN dotnet build "Gateway.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Gateway.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

ENTRYPOINT ["dotnet", "Gateway.API.dll"]
```

### Environment Variables

**File: Infraestructura/.env.example**

```bash
# Keycloak Configuration
KEYCLOAK_ADMIN=admin
KEYCLOAK_ADMIN_PASSWORD=admin
KEYCLOAK_DB_URL=jdbc:postgresql://postgres:5432/keycloak
KEYCLOAK_DB_USERNAME=postgres
KEYCLOAK_DB_PASSWORD=postgres

# Gateway Configuration
ASPNETCORE_ENVIRONMENT=Production
GATEWAY_PORT=8080

# Keycloak URLs (for Gateway)
KEYCLOAK_AUTHORITY=http://keycloak:8080/realms/Kairo
KEYCLOAK_AUDIENCE=kairo-api
KEYCLOAK_METADATA_ADDRESS=http://keycloak:8080/realms/Kairo/.well-known/openid-configuration

# CORS Configuration
CORS_ALLOWED_ORIGIN_1=http://localhost:5173
CORS_ALLOWED_ORIGIN_2=http://localhost:3000

# Microservice URLs
EVENTOS_API_URL=http://eventos-api:8080
ASIENTOS_API_URL=http://asientos-api:8080
USUARIOS_API_URL=http://usuarios-api:8080
ENTRADAS_API_URL=http://entradas-api:8080
REPORTES_API_URL=http://reportes-api:8080
```

## Implementation Notes

### Keycloak Realm Import

The realm-export.json file will be automatically imported when Keycloak starts for the first time. The `--import-realm` flag in the Docker command ensures this happens automatically.

**Key Points:**
- The import is idempotent - if the realm already exists, it won't be duplicated
- Users, roles, and clients are created automatically
- No manual configuration is needed after the first startup
- The realm file should be mounted as read-only to prevent accidental modifications

### YARP Configuration

YARP (Yet Another Reverse Proxy) is Microsoft's official reverse proxy library. It provides:
- Dynamic configuration reloading
- Load balancing
- Health checks
- Request/response transformation
- Session affinity
- Rate limiting

**Benefits over other solutions:**
- Native .NET integration
- High performance
- Active Microsoft support
- Extensive documentation
- Built-in observability

### Security Considerations

**Production Checklist:**
1. Enable HTTPS (RequireHttpsMetadata = true)
2. Use strong passwords for Keycloak admin
3. Rotate JWT signing keys regularly
4. Implement rate limiting
5. Add request size limits
6. Enable audit logging
7. Use secrets management (Azure Key Vault, AWS Secrets Manager)
8. Implement IP whitelisting for admin endpoints
9. Enable CORS only for known origins
10. Use short-lived access tokens (5 minutes)

### Performance Considerations

**Optimization Strategies:**
1. Enable response caching for static content
2. Use connection pooling for downstream services
3. Implement circuit breakers for failing services
4. Add request/response compression
5. Use HTTP/2 for better performance
6. Implement distributed caching (Redis)
7. Monitor and optimize JWT validation performance
8. Use async/await throughout the pipeline

### Monitoring and Observability

**Recommended Tools:**
- **Logging:** Serilog with structured logging
- **Metrics:** Prometheus + Grafana
- **Tracing:** OpenTelemetry + Jaeger
- **APM:** Application Insights or Elastic APM
- **Health Checks:** ASP.NET Core Health Checks UI

**Key Metrics to Monitor:**
- Request rate (requests/second)
- Response time (p50, p95, p99)
- Error rate (4xx, 5xx)
- Authentication success/failure rate
- Authorization denial rate
- Downstream service availability
- JWT validation time
- Memory and CPU usage
