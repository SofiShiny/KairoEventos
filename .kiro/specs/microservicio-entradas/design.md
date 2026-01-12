# Documento de Diseño - Microservicio Entradas.API

## Overview

El microservicio **Entradas.API** implementa un sistema robusto para la gestión completa del ciclo de vida de entradas digitales (tickets) siguiendo los principios de Arquitectura Hexagonal, Domain-Driven Design (DDD) y nomenclatura 100% en español. El sistema maneja tanto flujos síncronos (validaciones externas) como asíncronos (confirmación de pagos) para garantizar consistencia y performance.

## Architecture

### Arquitectura Hexagonal Estricta

El sistema se organiza en capas concéntricas donde el dominio permanece completamente aislado de detalles de infraestructura:

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

### Flujo de Datos y Control

**Flujo Síncrono (Creación de Entrada):**
1. API Controller recibe `CrearEntradaCommand`
2. Application Handler valida comando
3. Handler llama a `IVerificadorEventos` e `IVerificadorAsientos`
4. Si validaciones pasan, crea entidad `Entrada`
5. Persiste en base de datos
6. Publica `EntradaCreadaEvento`
7. Retorna DTO de respuesta

**Flujo Asíncrono (Confirmación de Pago):**
1. `PagoConfirmadoConsumer` recibe evento de RabbitMQ
2. Localiza entrada por ID
3. Cambia estado a `Pagada`
4. Persiste cambio en base de datos

## Components and Interfaces

### Capa de Dominio (Entradas.Dominio)

#### Entidades

```csharp
public class Entrada : EntidadBase
{
    public Guid Id { get; private set; }
    public Guid EventoId { get; private set; }
    public Guid UsuarioId { get; private set; }
    public Guid? AsientoId { get; private set; }
    public decimal Monto { get; private set; }
    public string CodigoQr { get; private set; }
    public EstadoEntrada Estado { get; private set; }
    public DateTime FechaCompra { get; private set; }
    
    // Factory method y métodos de negocio
    public static Entrada Crear(Guid eventoId, Guid usuarioId, decimal monto, 
                               Guid? asientoId, string codigoQr);
    public void ConfirmarPago();
    public void Cancelar();
    public void MarcarComoUsada();
}

public enum EstadoEntrada
{
    PendientePago = 1,
    Pagada = 2,
    Cancelada = 3,
    Usada = 4
}
```

#### Interfaces de Servicios Externos

```csharp
public interface IVerificadorEventos
{
    Task<bool> EventoExisteYDisponibleAsync(Guid eventoId, CancellationToken cancellationToken);
    Task<EventoInfo> ObtenerInfoEventoAsync(Guid eventoId, CancellationToken cancellationToken);
}

public interface IVerificadorAsientos
{
    Task<bool> AsientoDisponibleAsync(Guid eventoId, Guid? asientoId, CancellationToken cancellationToken);
    Task ReservarAsientoTemporalAsync(Guid eventoId, Guid asientoId, TimeSpan duracion, CancellationToken cancellationToken);
}

public interface IGeneradorCodigoQr
{
    string GenerarCodigoUnico();
}

public interface IRepositorioEntradas
{
    Task<Entrada> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Entrada> GuardarAsync(Entrada entrada, CancellationToken cancellationToken);
    Task<IEnumerable<Entrada>> ObtenerPorUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken);
}
```

#### Excepciones de Dominio

```csharp
public class EventoNoDisponibleException : DominioException
public class AsientoNoDisponibleException : DominioException  
public class EntradaNoEncontradaException : DominioException
public class EstadoEntradaInvalidoException : DominioException
```

### Capa de Aplicación (Entradas.Aplicacion)

#### Commands y Handlers

```csharp
public record CrearEntradaCommand(
    Guid EventoId,
    Guid UsuarioId, 
    Guid? AsientoId,
    decimal Monto
) : IRequest<EntradaCreadaDto>;

public class CrearEntradaCommandHandler : IRequestHandler<CrearEntradaCommand, EntradaCreadaDto>
{
    private readonly IVerificadorEventos _verificadorEventos;
    private readonly IVerificadorAsientos _verificadorAsientos;
    private readonly IGeneradorCodigoQr _generadorQr;
    private readonly IRepositorioEntradas _repositorio;
    private readonly IPublishEndpoint _publisher;
    
    public async Task<EntradaCreadaDto> Handle(CrearEntradaCommand request, CancellationToken cancellationToken)
    {
        // 1. Validar evento existe y disponible
        // 2. Validar asiento disponible (si aplica)
        // 3. Generar código QR único
        // 4. Crear entidad Entrada
        // 5. Persistir en base de datos
        // 6. Publicar evento EntradaCreadaEvento
        // 7. Retornar DTO
    }
}
```

#### Consumers

```csharp
public class PagoConfirmadoConsumer : IConsumer<PagoConfirmadoEvento>
{
    private readonly IRepositorioEntradas _repositorio;
    
    public async Task Consume(ConsumeContext<PagoConfirmadoEvento> context)
    {
        var entrada = await _repositorio.ObtenerPorIdAsync(context.Message.EntradaId, context.CancellationToken);
        entrada.ConfirmarPago();
        await _repositorio.GuardarAsync(entrada, context.CancellationToken);
    }
}
```

#### DTOs

```csharp
public record EntradaCreadaDto(
    Guid Id,
    Guid EventoId,
    Guid UsuarioId,
    Guid? AsientoId,
    decimal Monto,
    string CodigoQr,
    EstadoEntrada Estado,
    DateTime FechaCompra
);

public record CrearEntradaDto(
    Guid EventoId,
    Guid UsuarioId,
    Guid? AsientoId,
    decimal Monto
);
```

### Capa de Infraestructura (Entradas.Infraestructura)

#### Entity Framework Core Configuration

```csharp
public class EntradasDbContext : DbContext
{
    public DbSet<Entrada> Entradas { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new EntradaConfiguration());
    }
}

public class EntradaConfiguration : IEntityTypeConfiguration<Entrada>
{
    public void Configure(EntityTypeBuilder<Entrada> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.CodigoQr).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Monto).HasPrecision(18, 2);
        builder.Property(e => e.Estado).HasConversion<int>();
        builder.HasIndex(e => e.CodigoQr).IsUnique();
    }
}
```

#### Implementaciones de Servicios Externos

```csharp
public class VerificadorEventosHttp : IVerificadorEventos
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<VerificadorEventosHttp> _logger;
    
    public async Task<bool> EventoExisteYDisponibleAsync(Guid eventoId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/eventos/{eventoId}/disponible", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error verificando evento {EventoId}", eventoId);
            throw new ServicioExternoNoDisponibleException("Eventos", ex);
        }
    }
}

public class VerificadorAsientosHttp : IVerificadorAsientos
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<VerificadorAsientosHttp> _logger;
    
    public async Task<bool> AsientoDisponibleAsync(Guid eventoId, Guid? asientoId, CancellationToken cancellationToken)
    {
        if (!asientoId.HasValue) return true; // Entrada general
        
        try
        {
            var response = await _httpClient.GetAsync($"/api/asientos/{asientoId}/disponible?eventoId={eventoId}", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error verificando asiento {AsientoId}", asientoId);
            throw new ServicioExternoNoDisponibleException("Asientos", ex);
        }
    }
}
```

#### Generador de Códigos QR

```csharp
public class GeneradorCodigoQr : IGeneradorCodigoQr
{
    public string GenerarCodigoUnico()
    {
        var guid = Guid.NewGuid().ToString("N")[..8].ToUpper();
        var random = Random.Shared.Next(1000, 9999);
        return $"TICKET-{guid}-{random}";
    }
}
```

### Capa de API (Entradas.API)

#### Controllers

```csharp
[ApiController]
[Route("api/[controller]")]
public class EntradasController : ControllerBase
{
    private readonly IMediator _mediator;
    
    [HttpPost]
    public async Task<ActionResult<EntradaCreadaDto>> CrearEntrada(
        [FromBody] CrearEntradaDto dto,
        CancellationToken cancellationToken)
    {
        var command = new CrearEntradaCommand(dto.EventoId, dto.UsuarioId, dto.AsientoId, dto.Monto);
        var resultado = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(ObtenerEntrada), new { id = resultado.Id }, resultado);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<EntradaDto>> ObtenerEntrada(Guid id, CancellationToken cancellationToken)
    {
        var query = new ObtenerEntradaQuery(id);
        var entrada = await _mediator.Send(query, cancellationToken);
        return Ok(entrada);
    }
}
```

## Data Models

### Modelo de Base de Datos

```sql
CREATE TABLE entradas (
    id UUID PRIMARY KEY,
    evento_id UUID NOT NULL,
    usuario_id UUID NOT NULL,
    asiento_id UUID NULL,
    monto DECIMAL(18,2) NOT NULL,
    codigo_qr VARCHAR(100) NOT NULL UNIQUE,
    estado INTEGER NOT NULL,
    fecha_compra TIMESTAMP NOT NULL,
    fecha_creacion TIMESTAMP NOT NULL DEFAULT NOW(),
    fecha_actualizacion TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_entradas_evento_id ON entradas(evento_id);
CREATE INDEX idx_entradas_usuario_id ON entradas(usuario_id);
CREATE INDEX idx_entradas_estado ON entradas(estado);
```

### Eventos de Dominio

```csharp
public record EntradaCreadaEvento(
    Guid EntradaId,
    Guid EventoId,
    Guid UsuarioId,
    decimal Monto,
    DateTime FechaCreacion
);

public record PagoConfirmadoEvento(
    Guid EntradaId,
    Guid TransaccionId,
    decimal MontoConfirmado,
    DateTime FechaPago
);
```

## Correctness Properties

*Una propiedad es una característica o comportamiento que debe mantenerse verdadero a través de todas las ejecuciones válidas del sistema - esencialmente, una declaración formal sobre lo que el sistema debe hacer.*

### Property 1: Estructura de Entidad Entrada
*Para cualquier* instancia de Entrada creada por el sistema, debe contener todas las propiedades requeridas (Id, EventoId, UsuarioId, AsientoId, Monto, CodigoQr, Estado, FechaCompra) con los tipos de datos correctos
**Validates: Requirements 1.1**

### Property 2: Estados Válidos de Entrada  
*Para cualquier* entrada en el sistema, su estado debe ser uno de los valores válidos: PendientePago, Pagada, Cancelada, o Usada
**Validates: Requirements 1.2**

### Property 3: Estado Inicial de Entrada
*Para cualquier* entrada recién creada, el estado inicial debe ser PendientePago
**Validates: Requirements 1.3**

### Property 4: Formato de Código QR
*Para cualquier* código QR generado por el sistema, debe seguir el formato exacto "TICKET-{Guid}-{Random}" donde Guid es una cadena hexadecimal y Random es un número
**Validates: Requirements 2.1**

### Property 5: Unicidad de Códigos QR
*Para cualquier* conjunto de códigos QR generados por el sistema, todos deben ser únicos (no debe haber duplicados)
**Validates: Requirements 2.2**

### Property 6: Validación Externa Obligatoria
*Para cualquier* CrearEntradaCommand procesado, el sistema debe llamar tanto a IVerificadorEventos como a IVerificadorAsientos antes de crear la entrada
**Validates: Requirements 3.1**

### Property 7: Rechazo por Validación Externa Fallida
*Para cualquier* CrearEntradaCommand donde las validaciones externas fallan, el sistema debe rechazar la creación y lanzar una excepción descriptiva
**Validates: Requirements 3.3**

### Property 8: Creación Exitosa de Entrada
*Para cualquier* CrearEntradaCommand con validaciones externas exitosas, el sistema debe crear una nueva entrada en estado PendientePago
**Validates: Requirements 4.1**

### Property 9: Publicación de Eventos
*Para cualquier* entrada persistida exitosamente, el sistema debe publicar un EntradaCreadaEvento con los datos correctos
**Validates: Requirements 4.3**

### Property 10: Atomicidad de Transacciones
*Para cualquier* operación de creación de entrada que falla en cualquier punto, no debe haber cambios persistidos en la base de datos (rollback completo)
**Validates: Requirements 4.5**

### Property 11: Localización en Consumer
*Para cualquier* PagoConfirmadoEvento recibido, el consumer debe buscar la entrada correspondiente usando el ID del evento
**Validates: Requirements 5.2**

### Property 12: Transición de Estado por Pago
*Para cualquier* entrada en estado PendientePago que recibe confirmación de pago, debe cambiar su estado a Pagada
**Validates: Requirements 5.3**

### Property 13: Validación de Comandos
*Para cualquier* comando inválido enviado al sistema, debe ser rechazado con mensajes de error descriptivos antes del procesamiento
**Validates: Requirements 9.4, 14.1**

### Property 14: Códigos de Estado HTTP Apropiados
*Para cualquier* request HTTP al sistema, la respuesta debe incluir el código de estado HTTP apropiado según el resultado de la operación (200 para éxito, 400 para errores de validación, 500 para errores internos)
**Validates: Requirements 13.2**

## Error Handling

### Estrategia de Manejo de Errores

El sistema implementa una estrategia de manejo de errores en capas:

#### Excepciones de Dominio
- `EventoNoDisponibleException`: Cuando el evento no existe o no está disponible
- `AsientoNoDisponibleException`: Cuando el asiento solicitado no está disponible  
- `EntradaNoEncontradaException`: Cuando se busca una entrada que no existe
- `EstadoEntradaInvalidoException`: Cuando se intenta una transición de estado inválida

#### Excepciones de Infraestructura
- `ServicioExternoNoDisponibleException`: Cuando servicios externos (Eventos/Asientos) no responden
- `BaseDatosNoDisponibleException`: Cuando hay problemas de conectividad con PostgreSQL
- `RabbitMqNoDisponibleException`: Cuando hay problemas con la cola de mensajes

#### Middleware de Manejo Global
```csharp
public class GlobalExceptionHandlerMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (DominioException ex)
        {
            await HandleDominioExceptionAsync(context, ex);
        }
        catch (ValidationException ex)
        {
            await HandleValidationExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            await HandleGenericExceptionAsync(context, ex);
        }
    }
}
```

#### Circuit Breaker Pattern
Para servicios externos se implementa circuit breaker con Polly:
- **Closed**: Operación normal
- **Open**: Después de 5 fallos consecutivos, bloquea llamadas por 30 segundos
- **Half-Open**: Permite una llamada de prueba después del timeout

#### Retry Policy
- **Servicios HTTP**: 3 reintentos con backoff exponencial (1s, 2s, 4s)
- **Base de Datos**: 2 reintentos inmediatos para errores transitorios
- **RabbitMQ**: Reintento automático con dead letter queue después de 3 fallos

## Testing Strategy

### Enfoque Dual de Testing

El sistema implementa una estrategia dual que combina unit tests y property-based tests para cobertura comprehensiva:

#### Unit Tests
**Propósito**: Verificar ejemplos específicos, casos edge y condiciones de error
**Herramientas**: xUnit, Moq, FluentAssertions
**Cobertura objetivo**: >90%

**Casos de prueba específicos**:
- Creación exitosa de entrada con datos válidos
- Rechazo de entrada cuando evento no existe
- Rechazo de entrada cuando asiento no disponible
- Confirmación de pago cambia estado correctamente
- Manejo de errores de servicios externos

#### Property-Based Tests  
**Propósito**: Verificar propiedades universales a través de muchas entradas generadas
**Herramientas**: FsCheck.NET
**Configuración**: Mínimo 100 iteraciones por propiedad

**Generadores personalizados**:
```csharp
public static class Generators
{
    public static Arbitrary<CrearEntradaCommand> ComandosValidos() =>
        Arb.From(
            from eventoId in Arb.Generate<Guid>()
            from usuarioId in Arb.Generate<Guid>()
            from asientoId in Gen.OptionalOf(Arb.Generate<Guid>())
            from monto in Gen.Choose(1, 10000).Select(x => (decimal)x)
            select new CrearEntradaCommand(eventoId, usuarioId, asientoId, monto)
        );
        
    public static Arbitrary<CrearEntradaCommand> ComandosInvalidos() =>
        Arb.From(
            from eventoId in Gen.Constant(Guid.Empty)
            from usuarioId in Arb.Generate<Guid>()
            from monto in Gen.Choose(-1000, 0).Select(x => (decimal)x)
            select new CrearEntradaCommand(eventoId, usuarioId, null, monto)
        );
}
```

#### Integration Tests
**Propósito**: Verificar integración entre componentes reales
**Scope**: 
- Base de datos PostgreSQL (usando TestContainers)
- RabbitMQ (usando TestContainers)
- Servicios HTTP (usando WireMock)

#### Test Configuration
Cada property test debe incluir tag de referencia:
```csharp
[Property]
[Trait("Feature", "microservicio-entradas")]
[Trait("Property", "1: Estructura de Entidad Entrada")]
public void Entrada_DebeContenerTodasLasPropiedadesRequeridas(CrearEntradaCommand comando)
{
    // Test implementation
}
```

### Estrategia de Mocking

#### Servicios Externos
```csharp
// Mock para IVerificadorEventos
Mock<IVerificadorEventos> mockVerificadorEventos = new();
mockVerificadorEventos
    .Setup(x => x.EventoExisteYDisponibleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(true);

// Mock para IVerificadorAsientos  
Mock<IVerificadorAsientos> mockVerificadorAsientos = new();
mockVerificadorAsientos
    .Setup(x => x.AsientoDisponibleAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(true);
```

#### Base de Datos
Para unit tests se usa repositorio en memoria:
```csharp
public class RepositorioEntradasEnMemoria : IRepositorioEntradas
{
    private readonly Dictionary<Guid, Entrada> _entradas = new();
    
    public Task<Entrada> GuardarAsync(Entrada entrada, CancellationToken cancellationToken)
    {
        _entradas[entrada.Id] = entrada;
        return Task.FromResult(entrada);
    }
}
```

### Métricas de Calidad

**Cobertura de Código**: >90% líneas cubiertas
**Cobertura de Branches**: >85% branches cubiertos  
**Cobertura de Propiedades**: 100% de las 14 propiedades implementadas
**Performance**: Creación de entrada <200ms (P95)
**Reliability**: <0.1% error rate en operaciones críticas

### Continuous Testing

**Pre-commit hooks**: Ejecutar unit tests rápidos
**CI Pipeline**: 
1. Unit tests (paralelos)
2. Property tests (100 iteraciones mínimo)
3. Integration tests (con TestContainers)
4. Coverage report
5. Quality gates (cobertura >90%)

**Nightly builds**: Property tests extendidos (1000 iteraciones)