# Gateway API - Kairo Microservices

API Gateway profesional construido con YARP (Yet Another Reverse Proxy) que actúa como punto de entrada único para todos los microservicios del sistema Kairo, con autenticación y autorización centralizada mediante Keycloak.

## 🏗️ Arquitectura

### Componentes Principales

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

### Características

- ✅ **Reverse Proxy con YARP** - Enrutamiento inteligente a microservicios
- ✅ **Autenticación JWT** - Validación de tokens con Keycloak
- ✅ **Autorización basada en roles** - Control de acceso granular (User, Admin, Organizator)
- ✅ **CORS** - Soporte para aplicaciones frontend
- ✅ **Health Checks** - Monitoreo de disponibilidad del Gateway y Keycloak
- ✅ **Logging estructurado** - Observabilidad completa con Serilog
- ✅ **Manejo de errores** - Respuestas JSON estructuradas para todos los errores
- ✅ **Variables de entorno** - Configuración flexible para diferentes ambientes

## 📁 Estructura del Proyecto

```
Gateway/
├── src/
│   └── Gateway.API/
│       ├── Configuration/          # Clases de configuración
│       │   ├── AuthenticationConfiguration.cs
│       │   ├── AuthorizationConfiguration.cs
│       │   ├── CorsConfiguration.cs
│       │   └── ConfigurationLoader.cs
│       ├── Middleware/             # Middlewares personalizados
│       │   ├── RequestLoggingMiddleware.cs
│       │   └── ExceptionHandlingMiddleware.cs
│       ├── HealthChecks/           # Health checks personalizados
│       │   └── KeycloakHealthCheck.cs
│       ├── Program.cs              # Punto de entrada
│       ├── appsettings.json        # Configuración base
│       └── appsettings.Development.json
├── tests/
│   └── Gateway.Tests/              # Tests unitarios e integración
├── Dockerfile                      # Imagen Docker del Gateway
├── .env.example                    # Variables de entorno de ejemplo
└── README.md
```

## 🚀 Inicio Rápido

### Prerrequisitos

- .NET 8 SDK
- Docker y Docker Compose (para ejecución con contenedores)
- Keycloak corriendo (automático con Docker Compose)

### Opción 1: Ejecutar Localmente (Desarrollo)

1. **Levantar infraestructura (Keycloak, PostgreSQL, etc.)**
   ```bash
   cd Infraestructura
   docker-compose up -d
   ```

2. **Esperar a que Keycloak esté listo**
   ```bash
   # Verificar que Keycloak está disponible
   curl http://localhost:8180/health/ready
   ```

3. **Ejecutar el Gateway**
   ```bash
   cd Gateway/src/Gateway.API
   dotnet restore
   dotnet run
   ```

El Gateway estará disponible en: **http://localhost:8080**

### Opción 2: Ejecutar con Docker

```bash
cd Infraestructura
docker-compose up -d
```

Esto levantará:
- Keycloak (puerto 8180)
- Gateway (puerto 8080)
- PostgreSQL, MongoDB, RabbitMQ

El Gateway estará disponible en: **http://localhost:8080**

### Verificar que Todo Funciona

```bash
# Health check del Gateway
curl http://localhost:8080/health

# Health check con verificación de Keycloak
curl http://localhost:8080/health/ready

# Liveness probe
curl http://localhost:8080/health/live
```

## 🔌 Endpoints Disponibles

### Microservicios Enrutados

El Gateway enruta peticiones a los siguientes microservicios:

| Ruta | Microservicio | Descripción |
|------|---------------|-------------|
| `/api/eventos/*` | Eventos API | Gestión de eventos |
| `/api/asientos/*` | Asientos API | Gestión de asientos y mapas |
| `/api/usuarios/*` | Usuarios API | Gestión de usuarios |
| `/api/entradas/*` | Entradas API | Gestión de entradas/tickets |
| `/api/reportes/*` | Reportes API | Generación de reportes |

**Ejemplo:**
```bash
# Petición al Gateway
GET http://localhost:8080/api/eventos/123

# Se enruta automáticamente a:
GET http://eventos-api:8080/api/eventos/123
```

### Health Checks

| Endpoint | Descripción | Uso |
|----------|-------------|-----|
| `GET /health` | Estado general del Gateway | Kubernetes liveness probe |
| `GET /health/ready` | Verifica conectividad con Keycloak | Kubernetes readiness probe |
| `GET /health/live` | Verifica que el proceso está vivo | Monitoreo básico |

**Respuesta de ejemplo:**
```json
{
  "status": "Healthy",
  "checks": {
    "keycloak": {
      "status": "Healthy",
      "description": "Keycloak is reachable"
    }
  },
  "timestamp": "2024-12-30T10:30:00Z"
}
```

## 🔐 Autenticación

El Gateway valida tokens JWT emitidos por Keycloak. Todos los endpoints de microservicios requieren autenticación.

### Obtener un Token

```bash
# Obtener token de Keycloak
curl -X POST http://localhost:8180/realms/Kairo/protocol/openid-connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "client_id=kairo-web" \
  -d "username=admin" \
  -d "password=admin123" \
  -d "grant_type=password"
```

**Respuesta:**
```json
{
  "access_token": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expires_in": 300,
  "refresh_expires_in": 1800,
  "refresh_token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "token_type": "Bearer"
}
```

### Usar el Token en Peticiones

```bash
# Usar token en peticiones al Gateway
curl -H "Authorization: Bearer <access_token>" \
  http://localhost:8080/api/eventos
```

### Usuarios por Defecto

Keycloak se configura automáticamente con estos usuarios:

| Usuario | Password | Roles | Descripción |
|---------|----------|-------|-------------|
| `admin` | `admin123` | Admin, User | Administrador con acceso completo |
| `organizador` | `org123` | Organizator, User | Organizador de eventos |
| `usuario` | `user123` | User | Usuario regular |

## 🛡️ Autorización

El Gateway implementa políticas de autorización basadas en roles extraídos del token JWT.

### Políticas Disponibles

| Política | Roles Requeridos | Descripción |
|----------|------------------|-------------|
| `Authenticated` | Cualquier usuario autenticado | Acceso básico |
| `UserAccess` | User | Acceso de usuario regular |
| `AdminAccess` | Admin | Acceso administrativo completo |
| `OrganizatorAccess` | Organizator | Acceso para organizadores |
| `EventManagement` | Admin, Organizator | Gestión de eventos |

### Ejemplo de Uso

```csharp
// En los microservicios, los endpoints pueden requerir roles específicos
[Authorize(Policy = "AdminAccess")]
public IActionResult DeleteEvento(int id) { ... }

[Authorize(Policy = "EventManagement")]
public IActionResult CreateEvento(EventoDto dto) { ... }
```

### Respuestas de Autorización

**403 Forbidden (sin permisos):**
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

## ⚙️ Configuración

### Variables de Entorno

El Gateway se configura mediante variables de entorno. Ver `.env.example` para la lista completa.

#### Keycloak

```bash
Keycloak__Authority=http://keycloak:8080/realms/Kairo
Keycloak__Audience=kairo-api
Keycloak__MetadataAddress=http://keycloak:8080/realms/Kairo/.well-known/openid-configuration
```

#### CORS

```bash
Cors__AllowedOrigins__0=http://localhost:5173
Cors__AllowedOrigins__1=http://localhost:3000
```

#### Microservicios

```bash
ReverseProxy__Clusters__eventos-cluster__Destinations__destination1__Address=http://eventos-api:8080
ReverseProxy__Clusters__asientos-cluster__Destinations__destination1__Address=http://asientos-api:8080
ReverseProxy__Clusters__usuarios-cluster__Destinations__destination1__Address=http://usuarios-api:8080
ReverseProxy__Clusters__entradas-cluster__Destinations__destination1__Address=http://entradas-api:8080
ReverseProxy__Clusters__reportes-cluster__Destinations__destination1__Address=http://reportes-api:8080
```

### Configuración de Rutas YARP

Las rutas se definen en `appsettings.json`:

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
      }
    },
    "Clusters": {
      "eventos-cluster": {
        "Destinations": {
          "destination1": {
            "Address": "http://eventos-api:8080"
          }
        }
      }
    }
  }
}
```

### Valores por Defecto

El Gateway proporciona valores por defecto para desarrollo local:
- Keycloak: `http://localhost:8180`
- CORS: `http://localhost:5173`
- Microservicios: `http://localhost:808X`

## 📊 Logging y Observabilidad

### Logging Estructurado

El Gateway usa Serilog para logging estructurado con los siguientes sinks:

- **Console** - Logs en consola (desarrollo)
- **File** - Logs en archivos rotativos (`logs/gateway-*.log`)

### Niveles de Log

| Nivel | Uso | Ejemplo |
|-------|-----|---------|
| `Debug` | Desarrollo | Detalles de configuración |
| `Information` | Producción | Peticiones HTTP, autenticación exitosa |
| `Warning` | Advertencias | Tokens expirados, autenticación fallida |
| `Error` | Errores | Excepciones, servicios no disponibles |

### Logs de Peticiones

Cada petición HTTP genera logs con:
- Request ID único
- Método HTTP
- Path
- Timestamp de inicio
- Duración
- Status code

**Ejemplo:**
```
[INF] Request abc123: GET /api/eventos started at 2024-12-30T10:30:00Z
[INF] Request abc123: GET /api/eventos completed with 200 in 45ms
```

### Ver Logs

```bash
# Logs en tiempo real (Docker)
docker logs -f kairo-gateway

# Logs en archivos (local)
tail -f Gateway/src/Gateway.API/logs/gateway-*.log
```

## 🚨 Manejo de Errores

El Gateway proporciona respuestas JSON estructuradas para todos los errores.

### Errores de Autenticación (401)

```json
{
  "error": "Unauthorized",
  "message": "Invalid or missing authentication token",
  "timestamp": "2024-12-30T10:30:00Z",
  "path": "/api/eventos/123"
}
```

**Causas comunes:**
- Token ausente
- Token expirado
- Token inválido
- Firma inválida

### Errores de Autorización (403)

```json
{
  "error": "Forbidden",
  "message": "Insufficient permissions to access this resource",
  "requiredRoles": ["Admin"],
  "userRoles": ["User"],
  "timestamp": "2024-12-30T10:30:00Z",
  "path": "/api/eventos/123"
}
```

### Servicio No Disponible (503)

```json
{
  "error": "Service Unavailable",
  "message": "The requested service is temporarily unavailable",
  "service": "eventos-api",
  "timestamp": "2024-12-30T10:30:00Z",
  "path": "/api/eventos/123"
}
```

## 🧪 Testing

### Ejecutar Tests

```bash
# Todos los tests
dotnet test

# Tests unitarios
dotnet test --filter Category=Unit

# Tests de integración
dotnet test --filter Category=Integration

# Tests de propiedades (PBT)
dotnet test --filter Category=Property

# Con cobertura
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Cobertura de Tests

El proyecto tiene >90% de cobertura de código con:
- Tests unitarios para cada componente
- Tests de integración end-to-end
- Tests de propiedades (Property-Based Testing)

## 🐳 Docker

### Construir Imagen

```bash
cd Gateway
docker build -t kairo-gateway:latest .
```

### Ejecutar Contenedor

```bash
docker run -d \
  --name kairo-gateway \
  --network kairo-network \
  -p 8080:8080 \
  -e Keycloak__Authority=http://keycloak:8080/realms/Kairo \
  -e Keycloak__Audience=kairo-api \
  kairo-gateway:latest
```

### Docker Compose

Ver `Infraestructura/docker-compose.yml` para la configuración completa.

## 🔧 Troubleshooting

Ver [TROUBLESHOOTING.md](./TROUBLESHOOTING.md) para guía detallada de resolución de problemas.

### Problemas Comunes

**Gateway no inicia:**
```bash
# Verificar logs
docker logs kairo-gateway

# Verificar que Keycloak está disponible
curl http://localhost:8180/health/ready
```

**Tokens no se validan:**
```bash
# Verificar configuración de Keycloak
curl http://localhost:8180/realms/Kairo/.well-known/openid-configuration

# Verificar que el token es válido
# Decodificar en https://jwt.io
```

**Microservicio no responde:**
```bash
# Verificar que el microservicio está corriendo
docker ps | grep eventos-api

# Verificar conectividad
docker exec kairo-gateway curl http://eventos-api:8080/health
```

## 📈 Métricas y Monitoreo

### Métricas Clave

- Request rate (requests/second)
- Response time (p50, p95, p99)
- Error rate (4xx, 5xx)
- Authentication success/failure rate
- Authorization denial rate
- Downstream service availability

### Health Checks para Kubernetes

```yaml
livenessProbe:
  httpGet:
    path: /health/live
    port: 8080
  initialDelaySeconds: 30
  periodSeconds: 10

readinessProbe:
  httpGet:
    path: /health/ready
    port: 8080
  initialDelaySeconds: 10
  periodSeconds: 5
```

## 🔗 Referencias

- [YARP Documentation](https://microsoft.github.io/reverse-proxy/)
- [Keycloak Documentation](https://www.keycloak.org/documentation)
- [ASP.NET Core Authentication](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/)
- [Serilog](https://serilog.net/)
- [Especificación del Proyecto](./.kiro/specs/gateway-keycloak-automatizado/)

## 📝 Estado de Implementación

- [x] Tarea 1: Configuración básica del proyecto con YARP
- [x] Tarea 2: Configuración de rutas YARP
- [x] Tarea 3: Autenticación JWT con Keycloak
- [x] Tarea 4: Autorización basada en roles
- [x] Tarea 5: Configuración CORS
- [x] Tarea 6: Middleware de logging
- [x] Tarea 7: Middleware de manejo de excepciones
- [x] Tarea 8: Health checks
- [x] Tarea 9: Variables de entorno
- [x] Tarea 10: Checkpoint local
- [x] Tarea 11: Realm export de Keycloak
- [x] Tarea 12: Dockerfile
- [x] Tarea 13: Docker Compose
- [x] Tarea 14: .env.example
- [x] Tarea 15: Checkpoint Docker
- [x] Tarea 16: Tests de integración
- [ ] Tarea 17: Documentación
- [ ] Tarea 18: Checkpoint final

## 📄 Licencia

Este proyecto es parte del sistema Kairo Microservices.
