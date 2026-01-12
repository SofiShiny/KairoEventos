# Design Document

## Overview

Este documento describe el diseño de la refactorización del microservicio de Asientos para implementar correctamente el patrón CQRS, reorganizar eventos de dominio e integrar RabbitMQ con MassTransit.

La refactorización se enfoca en tres áreas principales:
1. **Corrección de violaciones CQRS**: Separación estricta entre Commands y Queries
2. **Reorganización de eventos**: Un archivo por evento con namespace consistente
3. **Integración RabbitMQ**: Publicación asíncrona de eventos de dominio

## Architecture

### Arquitectura Hexagonal con CQRS

```
┌─────────────────────────────────────────────────────────────┐
│                        API Layer                             │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │ Controllers  │  │   DTOs       │  │  Middleware  │      │
│  └──────┬───────┘  └──────────────┘  └──────────────┘      │
│         │                                                     │
└─────────┼─────────────────────────────────────────────────────┘
          │ MediatR
┌─────────┼─────────────────────────────────────────────────────┐
│         ▼          Application Layer                          │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │  Commands    │  │   Queries    │  │   Handlers   │      │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘      │
│         │                  │                  │               │
│         └──────────────────┴──────────────────┘               │
│                            │                                   │
└────────────────────────────┼───────────────────────────────────┘
                             │
┌────────────────────────────┼───────────────────────────────────┐
│                            ▼         Domain Layer              │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │  Aggregates  │  │   Entities   │  │    Events    │      │
│  └──────────────┘  └──────────────┘  └──────┬───────┘      │
│                                               │               │
└───────────────────────────────────────────────┼───────────────┘
                                                │
┌───────────────────────────────────────────────┼───────────────┐
│                            Infrastructure Layer                │
│  ┌──────────────┐  ┌──────────────┐  ┌──────▼───────┐      │
│  │ Repositories │  │   DbContext  │  │  MassTransit │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
└────────────────────────────────────────────────────────────────┘
```

### Flujo CQRS

**Commands (Escritura):**
```
Controller → MediatR → CommandHandler → Domain → Repository → DB
                                                            ↓
                                                      MassTransit → RabbitMQ
```

**Queries (Lectura):**
```
Controller → MediatR → QueryHandler → Repository → DB
                                    ↓
                                   DTO
```

## Components and Interfaces

### Commands

Todos los commands son `records` inmutables que implementan `IRequest<T>`:

```csharp
// Retorna Guid
public record CrearMapaAsientosComando(Guid EventoId) : IRequest<Guid>;
public record AgregarAsientoComando(Guid MapaId, int Fila, int Numero, string Categoria) : IRequest<Guid>;
public record AgregarCategoriaComando(Guid MapaId, string Nombre, decimal? PrecioBase, bool TienePrioridad) : IRequest<Guid>;

// Retorna Unit
public record ReservarAsientoComando(Guid MapaId, int Fila, int Numero) : IRequest;
public record LiberarAsientoComando(Guid MapaId, int Fila, int Numero) : IRequest;
```

### Queries

Todas las queries son `records` inmutables que retornan DTOs:

```csharp
public record ObtenerMapaAsientosQuery(Guid MapaId) : IRequest<MapaAsientosDto?>;

// DTOs inmutables
public record MapaAsientosDto(
    Guid MapaId,
    Guid EventoId,
    List<CategoriaDto> Categorias,
    List<AsientoDto> Asientos
);

public record CategoriaDto(string Nombre, decimal? PrecioBase, bool TienePrioridad);
public record AsientoDto(Guid Id, int Fila, int Numero, string Categoria, bool Reservado);
```

### Command Handlers

Todos los handlers siguen el patrón: **Persistir → Publicar**

```csharp
public class CrearMapaAsientosComandoHandler : IRequestHandler<CrearMapaAsientosComando, Guid>
{
    private readonly IRepositorioMapaAsientos _repo;
    private readonly IPublishEndpoint _publishEndpoint;
    
    public async Task<Guid> Handle(CrearMapaAsientosComando request, CancellationToken cancellationToken)
    {
        // 1. Lógica de dominio
        var mapa = MapaAsientos.Crear(request.EventoId);
        
        // 2. Persistir
        await _repo.AgregarAsync(mapa, cancellationToken);
        
        // 3. Publicar evento
        await _publishEndpoint.Publish(
            new MapaAsientosCreadoEventoDominio(mapa.Id, request.EventoId), 
            cancellationToken
        );
        
        return mapa.Id;
    }
}
```

### Query Handlers

Los query handlers encapsulan la transformación de entidades a DTOs:

```csharp
public class ObtenerMapaAsientosQueryHandler : IRequestHandler<ObtenerMapaAsientosQuery, MapaAsientosDto?>
{
    private readonly IRepositorioMapaAsientos _repo;
    
    public async Task<MapaAsientosDto?> Handle(ObtenerMapaAsientosQuery request, CancellationToken cancellationToken)
    {
        var mapa = await _repo.ObtenerPorIdAsync(request.MapaId, cancellationToken);
        if (mapa == null) return null;
        
        // Transformación a DTOs
        var asientos = mapa.Asientos
            .Select(a => new AsientoDto(a.Id, a.Fila, a.Numero, a.Categoria.Nombre, a.Reservado))
            .OrderBy(x => x.Fila)
            .ThenBy(x => x.Numero)
            .ToList();
            
        var categorias = mapa.Categorias
            .Select(c => new CategoriaDto(c.Nombre, c.PrecioBase, c.TienePrioridad))
            .OrderByDescending(c => c.TienePrioridad)
            .ToList();
            
        return new MapaAsientosDto(mapa.Id, mapa.EventoId, categorias, asientos);
    }
}
```

### Controllers

Los controladores son "thin" - solo orquestación:

```csharp
[ApiController]
[Route("api/asientos")]
public class AsientosController : ControllerBase
{
    private readonly IMediator _mediator;
    
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] AsientoCreateDto dto)
    {
        var asientoId = await _mediator.Send(
            new AgregarAsientoComando(dto.MapaId, dto.Fila, dto.Numero, dto.Categoria)
        );
        return Ok(new { asientoId });
    }
    
    [HttpPost("reservar")]
    public async Task<IActionResult> Reservar([FromBody] AsientoActionDto dto)
    {
        await _mediator.Send(new ReservarAsientoComando(dto.MapaId, dto.Fila, dto.Numero));
        return Ok();
    }
}
```

## Data Models

### Eventos de Dominio

Cada evento en su propio archivo con namespace `Asientos.Dominio.EventosDominio`:

**MapaAsientosCreadoEventoDominio.cs:**
```csharp
using BloquesConstruccion.Dominio;

namespace Asientos.Dominio.EventosDominio;

public class MapaAsientosCreadoEventoDominio : EventoDominio
{
    public Guid MapaId { get; }
    public Guid EventoId { get; }
    
    public MapaAsientosCreadoEventoDominio(Guid mapaId, Guid eventoId)
    {
        MapaId = mapaId;
        EventoId = eventoId;
        IdAgregado = mapaId;
    }
}
```

**CategoriaAgregadaEventoDominio.cs:**
```csharp
public class CategoriaAgregadaEventoDominio : EventoDominio
{
    public Guid MapaId { get; }
    public string NombreCategoria { get; }
    
    public CategoriaAgregadaEventoDominio(Guid mapaId, string nombreCategoria)
    {
        MapaId = mapaId;
        NombreCategoria = nombreCategoria;
        IdAgregado = mapaId;
    }
}
```

**AsientoAgregadoEventoDominio.cs:**
```csharp
public class AsientoAgregadoEventoDominio : EventoDominio
{
    public Guid MapaId { get; }
    public int Fila { get; }
    public int Numero { get; }
    public string Categoria { get; }
    
    public AsientoAgregadoEventoDominio(Guid mapaId, int fila, int numero, string categoria)
    {
        MapaId = mapaId;
        Fila = fila;
        Numero = numero;
        Categoria = categoria;
        IdAgregado = mapaId;
    }
}
```

**AsientoReservadoEventoDominio.cs:**
```csharp
public class AsientoReservadoEventoDominio : EventoDominio
{
    public Guid MapaId { get; }
    public int Fila { get; }
    public int Numero { get; }
    
    public AsientoReservadoEventoDominio(Guid mapaId, int fila, int numero)
    {
        MapaId = mapaId;
        Fila = fila;
        Numero = numero;
        IdAgregado = mapaId;
    }
}
```

**AsientoLiberadoEventoDominio.cs:**
```csharp
public class AsientoLiberadoEventoDominio : EventoDominio
{
    public Guid MapaId { get; }
    public int Fila { get; }
    public int Numero { get; }
    
    public AsientoLiberadoEventoDominio(Guid mapaId, int fila, int numero)
    {
        MapaId = mapaId;
        Fila = fila;
        Numero = numero;
        IdAgregado = mapaId;
    }
}
```

### Configuración de MassTransit

**Program.cs:**
```csharp
// MassTransit con RabbitMQ
var rabbitMqHost = builder.Configuration["RabbitMq:Host"] ?? "localhost";
builder.Services.AddMassTransit(x =>
{
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
```

**appsettings.json:**
```json
{
  "RabbitMq": {
    "Host": "localhost"
  }
}
```

## Correctness Properties

*A property is a characteristic or behavior that should hold true across all valid executions of a system—essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.*

### Property 1: Commands retornan solo Guid o Unit
*For any* Command ejecutado en el sistema, el tipo de retorno debe ser `Guid` o `Unit`, nunca una entidad de dominio completa.
**Validates: Requirements 1.1**

### Property 2: Queries retornan DTOs inmutables
*For any* Query ejecutada en el sistema, el tipo de retorno debe ser un DTO definido como `record` con propiedades `init-only`.
**Validates: Requirements 1.2, 4.2**

### Property 3: Eventos heredan de EventoDominio
*For any* evento de dominio en el namespace `Asientos.Dominio.EventosDominio`, debe heredar de la clase base `EventoDominio`.
**Validates: Requirements 2.3**

### Property 4: Configuración RabbitMQ con fallback
*For any* configuración de RabbitMQ (presente, ausente, vacía o nula), el sistema debe usar el valor configurado o "localhost" como fallback.
**Validates: Requirements 3.1, 7.2**

### Property 5: Handlers publican después de persistir
*For any* CommandHandler que modifica estado, la llamada a `_repo.Save()` debe ocurrir antes que la llamada a `_publishEndpoint.Publish()`.
**Validates: Requirements 3.3, 3.4**

### Property 6: Commands son records inmutables
*For all* Commands en el sistema, deben estar definidos como `record` con propiedades `init-only`.
**Validates: Requirements 5.1, 5.4**

### Property 7: Queries son records inmutables
*For all* Queries en el sistema, deben estar definidas como `record` con propiedades `init-only`.
**Validates: Requirements 5.2, 5.4**

### Property 8: DTOs son records inmutables
*For all* DTOs retornados por Queries, deben estar definidos como `record` con propiedades `init-only`.
**Validates: Requirements 5.3, 5.4**

### Property 9: Handlers pasan CancellationToken
*For any* Handler que publica eventos, debe pasar el `CancellationToken` recibido al método `Publish()`.
**Validates: Requirements 6.6**

### Property 10: Controllers retornan solo IDs
*For any* Controller que ejecuta un Command que retorna `Guid`, el Controller debe retornar solo ese `Guid` sin datos adicionales.
**Validates: Requirements 8.3**

### Property 11: Controllers retornan Ok() vacío para Unit
*For any* Controller que ejecuta un Command que retorna `Unit`, el Controller debe retornar `Ok()` sin datos adicionales.
**Validates: Requirements 8.4**

### Property 12: Eventos contienen propiedades requeridas
*For any* evento de dominio creado, debe contener todas las propiedades especificadas en su definición (MapaId siempre presente, más propiedades específicas según el tipo de evento).
**Validates: Requirements 9.1, 9.2, 9.3, 9.4, 9.5**

### Property 13: IdAgregado igual a MapaId
*For any* evento de dominio creado, la propiedad `IdAgregado` debe ser igual a `MapaId`.
**Validates: Requirements 9.6**

### Property 14: Health check retorna información completa
*For any* llamada al endpoint `/health`, la respuesta debe contener los campos `status`, `db` y `rabbitmq`.
**Validates: Requirements 12.2, 12.3, 12.4**

## Error Handling

### Errores de Dominio

Los errores de dominio se lanzan como `InvalidOperationException` con mensajes descriptivos:

```csharp
var mapa = await _repo.ObtenerPorIdAsync(request.MapaId, cancellationToken) 
    ?? throw new InvalidOperationException("Mapa no existe");
```

### Errores de Validación

Las validaciones de entrada se manejan con Data Annotations en los DTOs:

```csharp
public record AsientoCreateDto(
    [Required] Guid MapaId,
    [Range(1, int.MaxValue)] int Fila,
    [Range(1, int.MaxValue)] int Numero,
    [Required] string Categoria
);
```

### Errores de RabbitMQ

MassTransit maneja automáticamente:
- Reintentos con backoff exponencial
- Dead letter queues para mensajes fallidos
- Circuit breaker para proteger el sistema

### Manejo Global de Excepciones

El middleware `UseDeveloperExceptionPage()` captura y formatea excepciones en desarrollo.

## Testing Strategy

### Dual Testing Approach

El sistema utiliza dos tipos de tests complementarios:

**Unit Tests:**
- Verifican ejemplos específicos y casos edge
- Prueban handlers individuales con mocks
- Validan transformaciones de DTOs
- Verifican comportamiento de controladores

**Property-Based Tests:**
- Verifican propiedades universales
- Generan inputs aleatorios (Commands, Queries, Events)
- Validan invariantes del sistema
- Ejecutan mínimo 100 iteraciones por propiedad

### Property-Based Testing con FsCheck

Usaremos **FsCheck** para .NET como framework de property-based testing.

**Configuración:**
```xml
<PackageReference Include="FsCheck.Xunit" Version="2.16.5" />
```

**Ejemplo de Property Test:**
```csharp
[Property(MaxTest = 100)]
public Property Commands_Should_Return_Guid_Or_Unit()
{
    return Prop.ForAll(
        Arb.From<IRequest>(),
        command =>
        {
            var returnType = command.GetType().GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>))
                .GetGenericArguments()[0];
                
            return returnType == typeof(Guid) || returnType == typeof(Unit);
        }
    );
}
```

### Test Tags

Cada property test debe incluir un comentario con el tag:

```csharp
// Feature: refactorizacion-asientos-cqrs-rabbitmq, Property 1: Commands retornan solo Guid o Unit
[Property(MaxTest = 100)]
public Property Commands_Return_Guid_Or_Unit() { ... }
```

### Tests de Integración

**Tests con RabbitMQ:**
- Usar Testcontainers para levantar RabbitMQ
- Verificar que eventos se publican correctamente
- Validar que consumers reciben eventos

**Tests de Base de Datos:**
- Usar base de datos InMemory para tests rápidos
- Usar PostgreSQL con Testcontainers para tests de integración completos

### Tests de Compilación

Verificar que el sistema compila sin errores:

```csharp
[Fact]
public void System_Should_Compile_Successfully()
{
    var result = ExecuteCommand("dotnet build");
    Assert.Equal(0, result.ExitCode);
    Assert.True(result.Duration < TimeSpan.FromSeconds(10));
}
```

### Tests de Estructura de Archivos

Verificar que los eventos están en archivos separados:

```csharp
[Fact]
public void Should_Have_Five_Separate_Event_Files()
{
    var eventFiles = Directory.GetFiles(
        "Asientos.Dominio/EventosDominio", 
        "*EventoDominio.cs"
    );
    Assert.Equal(5, eventFiles.Length);
}

[Fact]
public void Should_Not_Have_Consolidated_DomainEvents_File()
{
    var consolidatedFile = Path.Combine(
        "Asientos.Dominio/EventosDominio", 
        "DomainEvents.cs"
    );
    Assert.False(File.Exists(consolidatedFile));
}
```

## Deployment Considerations

### Variables de Entorno

```bash
# PostgreSQL
POSTGRES_HOST=localhost
POSTGRES_DB=asientosdb
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
POSTGRES_PORT=5432

# RabbitMQ
RabbitMq__Host=localhost
```

### Docker Compose

```yaml
version: '3.8'
services:
  postgres:
    image: postgres:15
    environment:
      POSTGRES_DB: asientosdb
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
    ports:
      - "5432:5432"
  
  rabbitmq:
    image: rabbitmq:3-management
    ports:
      - "5672:5672"
      - "15672:15672"
    environment:
      RABBITMQ_DEFAULT_USER: guest
      RABBITMQ_DEFAULT_PASS: guest
  
  asientos-api:
    build: .
    environment:
      POSTGRES_HOST: postgres
      RabbitMq__Host: rabbitmq
    ports:
      - "5000:8080"
    depends_on:
      - postgres
      - rabbitmq
```

### Health Checks

El endpoint `/health` permite monitoreo:

```json
{
  "status": "healthy",
  "db": "postgres",
  "rabbitmq": "localhost"
}
```

### Logging

Configurar logging estructurado en `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "MassTransit": "Debug"
    }
  }
}
```

## Performance Considerations

### Async/Await

Todos los handlers usan `async/await` para operaciones I/O:
- Acceso a base de datos
- Publicación de eventos a RabbitMQ
- Llamadas HTTP

### Connection Pooling

Entity Framework Core maneja automáticamente el connection pooling para PostgreSQL.

### Message Batching

MassTransit puede configurarse para batch publishing:

```csharp
cfg.PublishTopology.PublishBatchSize = 100;
```

### Retry Policies

Configurar retry policies para resiliencia:

```csharp
cfg.UseMessageRetry(r => r.Exponential(
    retryLimit: 5,
    minInterval: TimeSpan.FromSeconds(1),
    maxInterval: TimeSpan.FromSeconds(30),
    intervalDelta: TimeSpan.FromSeconds(2)
));
```

## Security Considerations

### Input Validation

Usar Data Annotations para validación básica:
- `[Required]` para campos obligatorios
- `[Range]` para valores numéricos
- `[StringLength]` para longitud de strings

### SQL Injection

Entity Framework Core previene SQL injection usando parámetros.

### Message Security

Para producción, configurar TLS en RabbitMQ:

```csharp
cfg.Host(rabbitMqHost, h =>
{
    h.Username("user");
    h.Password("password");
    h.UseSsl(s =>
    {
        s.Protocol = SslProtocols.Tls12;
    });
});
```

### CORS

Configurar CORS apropiadamente para producción:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins("https://app.example.com")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
```

## Migration Strategy

### Paso 1: Corrección de CQRS
1. Modificar Commands para retornar solo Guid o Unit
2. Crear Queries con DTOs inmutables
3. Actualizar Controllers para usar Queries

### Paso 2: Reorganización de Eventos
1. Crear archivos individuales para cada evento
2. Actualizar imports en handlers
3. Eliminar archivo consolidado

### Paso 3: Integración RabbitMQ
1. Instalar paquete MassTransit.RabbitMQ
2. Configurar MassTransit en Program.cs
3. Actualizar handlers para publicar eventos
4. Crear archivos de configuración

### Paso 4: Verificación
1. Compilar el proyecto
2. Ejecutar tests unitarios
3. Ejecutar tests de integración
4. Verificar publicación de eventos en RabbitMQ Management

## Documentation

La refactorización incluye documentación completa:

1. **REFACTORIZACION-CQRS-RABBITMQ.md**: Documento técnico detallado
2. **RESUMEN-EJECUTIVO-REFACTORIZACION.md**: Resumen ejecutivo
3. **README.md**: Guía de uso actualizada
4. **Diagramas de arquitectura**: Flujos CQRS y eventos
5. **Ejemplos de código**: Snippets de implementación
