# Microservicio Entradas.API

## Descripción

Microservicio para la gestión completa del ciclo de vida de entradas (tickets) digitales para eventos. Implementa Arquitectura Hexagonal estricta con Domain-Driven Design (DDD) y nomenclatura 100% en español.

## Estado del Proyecto

✅ **Completado:**
- Estructura de proyecto y solución
- Capa de dominio (entidades, interfaces, excepciones)
- Generador de códigos QR
- Capa de aplicación (commands, handlers, DTOs)
- Capa de infraestructura (EF Core, repositorios, servicios HTTP)
- Integración con RabbitMQ (MassTransit)
- Capa de API (controllers, middleware, configuración)
- Logging y observabilidad (Serilog, métricas, health checks)
- Swagger/OpenAPI documentation
- Containerización (Docker, docker-compose)

⚠️ **Pendiente:**
- Tests de integración (TestContainers)
- Mejora de cobertura de tests (actualmente 8.1%, objetivo >90%)
- Scripts de inicialización completos

## Cobertura de Tests

**Estado actual:** 12.7% (349/2735 líneas cubiertas)
**Objetivo:** >90%
**Tests ejecutándose:** 69 tests (todos pasando)

### Distribución de Tests
- **Tests de Dominio:** 20 tests (entidades, excepciones, enums)
- **Tests de Aplicación:** 6 tests (comandos, queries, DTOs)
- **Tests de Infraestructura:** 43 tests (servicios externos, generador QR)

### Áreas con Cobertura
- ✅ Entidades del dominio (Entrada, EntidadBase)
- ✅ Excepciones del dominio
- ✅ Enums y value objects
- ✅ DTOs y comandos de aplicación
- ✅ Servicios de infraestructura (GeneradorCodigoQr, VerificadorAsientos, VerificadorEventos)

### Áreas Pendientes de Cobertura
- ⚠️ Handlers de aplicación (CrearEntradaCommandHandler, ObtenerEntradaQueryHandler)
- ⚠️ Consumers de MassTransit (PagoConfirmadoConsumer)
- ⚠️ Controllers de API (EntradasController)
- ⚠️ Middleware (GlobalExceptionHandlerMiddleware, MetricsMiddleware)
- ⚠️ Repositorios (RepositorioEntradas)
- ⚠️ Configuraciones y servicios de infraestructura

## Arquitectura

El proyecto sigue los principios de **Arquitectura Hexagonal** con las siguientes capas:

```
┌─────────────────────────────────────────────────────────┐
│                    Entradas.API                         │
│                 (Controllers, DTOs)                     │
├─────────────────────────────────────────────────────────┤
│                Entradas.Aplicacion                      │
│         (Commands, Queries, Handlers, DTOs)             │
├─────────────────────────────────────────────────────────┤
│                 Entradas.Dominio                        │
│        (Entidades, Interfaces, Excepciones)             │
├─────────────────────────────────────────────────────────┤
│              Entradas.Infraestructura                   │
│    (EF Core, HttpClients, MassTransit, Repos)          │
├─────────────────────────────────────────────────────────┤
│                 Entradas.Pruebas                        │
│           (Unit Tests, Integration Tests)               │
└─────────────────────────────────────────────────────────┘
```

## Proyectos

### 1. Entradas.Dominio
- **Propósito**: Núcleo del dominio, sin dependencias externas
- **Contiene**: Entidades, Value Objects, Interfaces de servicios, Excepciones de dominio
- **Dependencias**: Ninguna (centro de la arquitectura hexagonal)

### 2. Entradas.Aplicacion  
- **Propósito**: Casos de uso y orquestación de la lógica de negocio
- **Contiene**: Commands, Queries, Handlers, DTOs, Validaciones
- **Dependencias**: Entradas.Dominio, MediatR, FluentValidation, MassTransit

### 3. Entradas.Infraestructura
- **Propósito**: Implementaciones de servicios externos y persistencia
- **Contiene**: Repositorios, DbContext, Servicios HTTP, Configuración RabbitMQ
- **Dependencias**: Entradas.Dominio, Entradas.Aplicacion, EF Core, PostgreSQL, MassTransit.RabbitMQ

### 4. Entradas.API
- **Propósito**: Capa de presentación y endpoints REST
- **Contiene**: Controllers, Middleware, Configuración, Swagger
- **Dependencias**: Todas las capas anteriores, ASP.NET Core, Serilog

### 5. Entradas.Pruebas
- **Propósito**: Testing comprehensivo con >90% cobertura
- **Contiene**: Unit Tests, Property-Based Tests, Integration Tests
- **Dependencias**: Todos los proyectos, xUnit, Moq, FluentAssertions, FsCheck, TestContainers

## Tecnologías

- **.NET 8**: Framework principal
- **PostgreSQL**: Base de datos principal
- **Entity Framework Core**: ORM con Code First
- **RabbitMQ**: Mensajería asíncrona
- **MassTransit**: Abstracción para RabbitMQ
- **MediatR**: Patrón Mediator para CQRS
- **FluentValidation**: Validación de comandos
- **Serilog**: Structured logging
- **Swagger/OpenAPI**: Documentación de API
- **xUnit**: Framework de testing
- **Moq**: Mocking framework
- **FluentAssertions**: Assertions fluidas
- **FsCheck**: Property-based testing
- **TestContainers**: Integration testing con contenedores

## Comandos Útiles

### Desarrollo

```bash
# Compilar la solución
dotnet build

# Restaurar paquetes
dotnet restore

# Ejecutar la API
dotnet run --project Entradas.API

# Ejecutar en modo watch (recarga automática)
dotnet watch run --project Entradas.API
```

### Testing

```bash
# Ejecutar todos los tests
dotnet test

# Ejecutar tests con cobertura
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults

# Generar reporte de cobertura HTML
reportgenerator -reports:"TestResults\*\coverage.cobertura.xml" -targetdir:"TestResults\CoverageReport" -reporttypes:Html

# Ejecutar tests específicos
dotnet test --filter "FullyQualifiedName~GeneradorCodigoQr"
```

### Base de Datos

```bash
# Crear migración
dotnet ef migrations add NombreMigracion --project Entradas.Infraestructura --startup-project Entradas.API

# Aplicar migraciones
dotnet ef database update --project Entradas.Infraestructura --startup-project Entradas.API

# Eliminar base de datos
dotnet ef database drop --project Entradas.Infraestructura --startup-project Entradas.API
```

### Docker

```bash
# Construir imagen
docker build -t entradas-api .

# Ejecutar con docker-compose
docker-compose up -d

# Ver logs
docker-compose logs -f entradas-api

# Parar servicios
docker-compose down
```

## API Endpoints

### Entradas

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| POST | `/api/entradas` | Crear nueva entrada |
| GET | `/api/entradas/{id}` | Obtener entrada por ID |
| GET | `/api/entradas/usuario/{usuarioId}` | Obtener entradas de un usuario |

### Health Checks

| Endpoint | Descripción |
|----------|-------------|
| `/health` | Estado general del servicio |
| `/health/ready` | Readiness probe |
| `/health/live` | Liveness probe |

### Documentación

| Endpoint | Descripción |
|----------|-------------|
| `/swagger` | Swagger UI |
| `/swagger/v1/swagger.json` | OpenAPI specification |

## Flujos de Negocio

### Creación de Entrada

1. **Validación síncrona**: Verificar evento y asiento disponibles
2. **Generación de código QR**: Crear código único
3. **Persistencia**: Guardar entrada en estado PendientePago
4. **Evento**: Publicar EntradaCreadaEvento a RabbitMQ
5. **Respuesta**: Retornar entrada creada

### Confirmación de Pago

1. **Consumo**: Recibir PagoConfirmadoEvento de RabbitMQ
2. **Localización**: Buscar entrada por ID
3. **Actualización**: Cambiar estado a Pagada
4. **Persistencia**: Guardar cambios en base de datos

## Monitoreo y Observabilidad

### Logs Estructurados

- **Serilog** con formato JSON
- **Correlation IDs** para tracing distribuido
- **Niveles**: Debug, Information, Warning, Error, Fatal

### Métricas

- **Contadores**: Entradas creadas, errores de validación
- **Histogramas**: Tiempo de respuesta de endpoints
- **Gauges**: Conexiones activas, memoria utilizada

### Health Checks

- **PostgreSQL**: Conectividad y latencia
- **RabbitMQ**: Estado de conexión y colas
- **Servicios externos**: Disponibilidad de APIs

## Configuración y Setup

### Prerrequisitos

- .NET 8 SDK
- PostgreSQL 13+
- RabbitMQ 3.8+
- Docker y Docker Compose (recomendado)

### Setup Rápido con Docker

1. **Clonar el repositorio y navegar al directorio:**
   ```bash
   cd Entradas
   ```

2. **Iniciar servicios de infraestructura:**
   ```bash
   docker-compose up -d postgres rabbitmq
   ```

3. **Ejecutar migraciones de base de datos:**
   ```bash
   dotnet ef database update --project Entradas.Infraestructura --startup-project Entradas.API
   ```

4. **Ejecutar la aplicación:**
   ```bash
   dotnet run --project Entradas.API
   ```

5. **Acceder a la documentación de API:**
   - Swagger UI: http://localhost:5000/swagger
   - Health Checks: http://localhost:5000/health

### Setup Manual

1. **Configurar PostgreSQL:**
   ```sql
   CREATE DATABASE entradas_db;
   CREATE USER entradas_user WITH PASSWORD 'entradas_password';
   GRANT ALL PRIVILEGES ON DATABASE entradas_db TO entradas_user;
   ```

2. **Configurar RabbitMQ:**
   - Instalar RabbitMQ
   - Habilitar Management Plugin: `rabbitmq-plugins enable rabbitmq_management`
   - Acceder a: http://localhost:15672 (guest/guest)

3. **Configurar variables de entorno:**
   ```bash
   export ConnectionStrings__DefaultConnection="Host=localhost;Database=entradas_db;Username=entradas_user;Password=entradas_password"
   export RabbitMQ__Host="localhost"
   export RabbitMQ__Username="guest"
   export RabbitMQ__Password="guest"
   ```

### Configuración de Desarrollo

Copiar y configurar el archivo de configuración:
```bash
cp Entradas.Infraestructura/appsettings.example.json Entradas.API/appsettings.Development.json
```

Editar las cadenas de conexión según tu entorno local.

---

**Nota**: Este proyecto sigue estrictamente los principios de Arquitectura Hexagonal, Domain-Driven Design y utiliza nomenclatura 100% en español según los requerimientos del sistema.