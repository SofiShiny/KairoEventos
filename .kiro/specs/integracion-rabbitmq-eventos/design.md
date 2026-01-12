# Design Document - Integración RabbitMQ en Microservicio de Eventos

## Overview

Este documento describe el diseño completo de la integración de RabbitMQ en el microservicio de Eventos, incluyendo la arquitectura, componentes, flujos de datos y estrategias de testing. La integración permite la comunicación asíncrona entre microservicios mediante eventos de dominio.

## Architecture

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                    MICROSERVICIO DE EVENTOS                      │
│                                                                   │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐      │
│  │   API Layer  │───▶│ Application  │───▶│   Domain     │      │
│  │  Controllers │    │   Handlers   │    │   Entities   │      │
│  └──────────────┘    └──────┬───────┘    └──────────────┘      │
│                              │                                    │
│                              ▼                                    │
│                    ┌──────────────────┐                          │
│                    │  IPublishEndpoint │                          │
│                    │   (MassTransit)   │                          │
│                    └─────────┬─────────┘                          │
└──────────────────────────────┼───────────────────────────────────┘
                               │
                               ▼
                    ┌──────────────────┐
                    │     RabbitMQ     │
                    │   Message Broker │
                    └─────────┬─────────┘
                               │
                ┌──────────────┼──────────────┐
                │              │              │
                ▼              ▼              ▼
        ┌──────────┐   ┌──────────┐   ┌──────────┐
        │ Reportes │   │ Asientos │   │  Otros   │
        │ Consumer │   │ Consumer │   │Consumers │
        └──────────┘   └──────────┘   └──────────┘
```

### Component Architecture

```
Eventos.API
├── Controllers
│   └── EventosController
│       ├── POST /api/eventos
│       ├── PATCH /api/eventos/{id}/publicar
│       ├── POST /api/eventos/{id}/asistentes
│       └── PATCH /api/eventos/{id}/cancelar
│
Eventos.Aplicacion
├── Comandos
│   ├── PublicarEventoComandoHandler
│   │   └── Publica: EventoPublicadoEventoDominio
│   ├── RegistrarAsistenteComandoHandler
│   │   └── Publica: AsistenteRegistradoEventoDominio
│   └── CancelarEventoComandoHandler
│       └── Publica: EventoCanceladoEventoDominio
│
Eventos.Dominio
├── Entidades
│   └── Evento
│       ├── Publicar()
│       ├── RegistrarAsistente()
│       └── Cancelar()
└── EventosDeDominio
    ├── EventoPublicadoEventoDominio
    ├── AsistenteRegistradoEventoDominio
    └── EventoCanceladoEventoDominio
```

## Components and Interfaces

### 1. MassTransit Configuration

**Ubicación:** `Eventos.API/Program.cs`

```csharp
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitHost = builder.Configuration["RabbitMq:Host"] ?? "localhost";
        cfg.Host(rabbitHost, h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
    });
});
```

**Responsabilidades:**
- Configurar conexión a RabbitMQ
- Establecer credenciales
- Configurar serialización de mensajes

### 2. Command Handlers con Publicación

#### PublicarEventoComandoHandler

```csharp
public class PublicarEventoComandoHandler : IRequestHandler<PublicarEventoComando, Resultado>
{
    private readonly IRepositorioEvento _repositorioEvento;
    private readonly IPublishEndpoint _publishEndpoint;

    public async Task<Resultado> Handle(PublicarEventoComando request, CancellationToken cancellationToken)
    {
        // 1. Obtener evento
        var evento = await _repositorioEvento.ObtenerPorIdAsync(request.EventoId, cancellationToken);
        
        // 2. Ejecutar lógica de dominio
        evento.Publicar();
        
        // 3. Persistir en PostgreSQL
        await _repositorioEvento.ActualizarAsync(evento, cancellationToken);
        
        // 4. Publicar a RabbitMQ
        await _publishEndpoint.Publish(new EventoPublicadoEventoDominio(
            evento.Id,
            evento.Titulo,
            evento.FechaInicio), cancellationToken);
        
        return Resultado.Exito();
    }
}
```

**Patrón:** Fire-and-Forget
**Orden de Operaciones:** Dominio → PostgreSQL → RabbitMQ

### 3. Domain Events

#### EventoPublicadoEventoDominio

```csharp
namespace Eventos.Dominio.EventosDeDominio;

public class EventoPublicadoEventoDominio : EventoDominio
{
    public Guid EventoId { get; }
    public string TituloEvento { get; }
    public DateTime FechaInicio { get; }
}
```

**Namespace:** `Eventos.Dominio.EventosDeDominio` (CRÍTICO para consumidores)

## Data Models

### Event Contracts

| Evento | Propiedades | Cuándo se Publica |
|--------|-------------|-------------------|
| EventoPublicadoEventoDominio | EventoId, TituloEvento, FechaInicio | Cuando un evento cambia de Borrador a Publicado |
| AsistenteRegistradoEventoDominio | EventoId, UsuarioId, NombreUsuario | Cuando un usuario se registra en un evento |
| EventoCanceladoEventoDominio | EventoId, TituloEvento | Cuando un evento se cancela |

### Message Flow

```
1. Cliente HTTP
   ↓
2. EventosController
   ↓
3. MediatR
   ↓
4. CommandHandler
   ↓
5. Domain Entity (genera evento de dominio)
   ↓
6. PostgreSQL (persistencia)
   ↓
7. IPublishEndpoint (MassTransit)
   ↓
8. RabbitMQ (exchange)
   ↓
9. Queues (una por consumidor)
   ↓
10. Consumers (otros microservicios)
```

## Correctness Properties

*A property is a characteristic or behavior that should hold true across all valid executions of a system—essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.*

### Property 1: Persistencia antes de Publicación

*For any* evento de dominio generado, el sistema debe persistir los cambios en PostgreSQL antes de publicar a RabbitMQ.

**Validates: Requirements 1.2**

**Rationale:** Garantiza que no se publiquen eventos de cambios que no están persistidos.

### Property 2: Publicación Exitosa

*For any* comando que modifica estado, si la persistencia es exitosa, entonces el evento debe publicarse a RabbitMQ.

**Validates: Requirements 1.1**

**Rationale:** Asegura que todos los cambios de estado se comunican a otros microservicios.

### Property 3: Estructura de Mensaje Correcta

*For any* evento publicado, el mensaje en RabbitMQ debe contener todas las propiedades definidas en el contrato.

**Validates: Requirements 3.2, 3.3, 3.4**

**Rationale:** Garantiza que los consumidores puedan deserializar correctamente los mensajes.

### Property 4: Namespace Consistente

*For all* eventos de dominio, el namespace debe ser `Eventos.Dominio.EventosDeDominio`.

**Validates: Requirements 3.1**

**Rationale:** Permite que los consumidores usen el mismo namespace para deserialización.

### Property 5: Idempotencia de Consumo

*For any* mensaje consumido múltiples veces, el estado final del sistema debe ser el mismo que si se consumiera una sola vez.

**Validates: Requirements 2.2, 2.3, 2.4**

**Rationale:** Protege contra procesamiento duplicado de mensajes.

### Property 6: Reconexión Automática

*For any* desconexión temporal de RabbitMQ, el sistema debe reconectarse automáticamente cuando el servicio esté disponible.

**Validates: Requirements 5.2**

**Rationale:** Mejora la resiliencia del sistema ante fallos temporales.

### Property 7: Orden de Procesamiento (Eventual Consistency)

*For any* secuencia de eventos del mismo agregado, el orden de procesamiento puede variar pero el estado final debe ser consistente.

**Validates: Requirements 2.1**

**Rationale:** Acepta eventual consistency en sistemas distribuidos.

### Property 8: Manejo de Errores

*For any* error en la publicación, el sistema debe registrar el error en logs sin afectar la persistencia en PostgreSQL.

**Validates: Requirements 1.5**

**Rationale:** Separa concerns de persistencia y mensajería.

## Error Handling

### Estrategia de Manejo de Errores

```csharp
try
{
    // 1. Lógica de dominio
    evento.Publicar();
    
    // 2. Persistencia
    await _repositorioEvento.ActualizarAsync(evento, cancellationToken);
    
    // 3. Publicación
    await _publishEndpoint.Publish(evento, cancellationToken);
    
    return Resultado.Exito();
}
catch (InvalidOperationException ex)
{
    // Error de dominio - retornar falla
    return Resultado.Falla(ex.Message);
}
catch (DbException ex)
{
    // Error de persistencia - registrar y retornar falla
    _logger.LogError(ex, "Error persistiendo evento");
    return Resultado.Falla("Error guardando cambios");
}
catch (Exception ex)
{
    // Error de publicación - registrar pero considerar éxito
    // (los cambios ya están en PostgreSQL)
    _logger.LogError(ex, "Error publicando a RabbitMQ");
    return Resultado.Exito(); // O implementar retry
}
```

### Escenarios de Error

| Escenario | Acción | Resultado |
|-----------|--------|-----------|
| Error de validación de dominio | Retornar falla | No se persiste ni publica |
| Error de persistencia | Retornar falla | No se publica |
| Error de publicación | Registrar log | Persistencia exitosa, publicación fallida |
| RabbitMQ no disponible | Registrar log | Considerar Outbox Pattern |

## Testing Strategy

### Dual Testing Approach

El sistema requiere dos tipos de pruebas complementarias:

1. **Unit Tests:** Verifican ejemplos específicos y casos edge
2. **Property Tests:** Verifican propiedades universales

### Unit Testing

**Framework:** xUnit + FluentAssertions

**Casos a Probar:**

```csharp
// Ejemplo: PublicarEventoComandoHandlerTests
[Fact]
public async Task Handle_EventoExistente_PublicaEvento()
{
    // Arrange
    var evento = CrearEventoEnBorrador();
    var handler = CrearHandler();
    
    // Act
    var resultado = await handler.Handle(
        new PublicarEventoComando(evento.Id), 
        CancellationToken.None);
    
    // Assert
    resultado.EsExitoso.Should().BeTrue();
    _publishEndpointMock.Verify(x => x.Publish(
        It.IsAny<EventoPublicadoEventoDominio>(), 
        It.IsAny<CancellationToken>()), 
        Times.Once);
}

[Fact]
public async Task Handle_EventoNoExiste_RetornaFalla()
{
    // Test que el handler maneja eventos no encontrados
}

[Fact]
public async Task Handle_EventoYaPublicado_RetornaFalla()
{
    // Test que no se puede publicar un evento ya publicado
}
```

### Property-Based Testing

**Framework:** FsCheck o CsCheck

**Configuración:** Mínimo 100 iteraciones por propiedad

**Propiedades a Probar:**

```csharp
// Property 1: Persistencia antes de Publicación
[Property(Arbitrary = new[] { typeof(EventoGenerators) })]
public Property PersistenciaAntesDePublicacion(Evento evento)
{
    return Prop.ForAll(
        Arb.From<PublicarEventoComando>(),
        async comando =>
        {
            // Arrange
            var handler = CrearHandler();
            var ordenLlamadas = new List<string>();
            
            _repositorioMock
                .Setup(x => x.ActualizarAsync(It.IsAny<Evento>(), It.IsAny<CancellationToken>()))
                .Callback(() => ordenLlamadas.Add("Persistir"))
                .ReturnsAsync(evento);
            
            _publishEndpointMock
                .Setup(x => x.Publish(It.IsAny<EventoPublicadoEventoDominio>(), It.IsAny<CancellationToken>()))
                .Callback(() => ordenLlamadas.Add("Publicar"))
                .Returns(Task.CompletedTask);
            
            // Act
            await handler.Handle(comando, CancellationToken.None);
            
            // Assert
            return ordenLlamadas[0] == "Persistir" && 
                   ordenLlamadas[1] == "Publicar";
        });
}

// Property 2: Estructura de Mensaje Correcta
[Property]
public Property EstructuraMensajeCorrecta(Guid eventoId, string titulo, DateTime fechaInicio)
{
    // Verifica que el mensaje publicado contiene todas las propiedades
}

// Property 3: Namespace Consistente
[Property]
public Property NamespaceConsistente()
{
    // Verifica que todos los eventos usan el mismo namespace
}
```

**Tags para Property Tests:**

```csharp
// Feature: integracion-rabbitmq-eventos, Property 1: Persistencia antes de Publicación
// Feature: integracion-rabbitmq-eventos, Property 2: Publicación Exitosa
```

### Integration Testing

**Escenarios E2E:**

1. **Test: Flujo Completo de Publicación**
   - Crear evento → Publicar → Verificar en RabbitMQ → Verificar consumo en Reportes

2. **Test: Flujo Completo de Registro**
   - Crear evento → Publicar → Registrar asistente → Verificar en RabbitMQ → Verificar en Reportes

3. **Test: Flujo Completo de Cancelación**
   - Crear evento → Publicar → Cancelar → Verificar en RabbitMQ → Verificar en Reportes

### Performance Testing

**Prueba de Carga:**
- 100 eventos publicados consecutivamente
- Verificar tiempos de respuesta < 200ms
- Verificar uso de memoria estable
- Verificar que todos los mensajes se procesan

## Implementation Notes

### Fase 1: Verificación (COMPLETADA ✅)

- [x] Configuración de MassTransit
- [x] Modificación de handlers
- [x] Creación de CancelarEventoComandoHandler
- [x] Compilación exitosa
- [x] Documentación completa

### Fase 2: Verificación Local (PENDIENTE)

- [ ] Ejecutar script de pruebas automatizado
- [ ] Verificar mensajes en RabbitMQ UI
- [ ] Validar estructura de mensajes
- [ ] Documentar resultados

### Fase 3: Integración con Reportes (PENDIENTE)

- [ ] Actualizar contratos en Reportes
- [ ] Crear EventoCanceladoConsumer
- [ ] Pruebas E2E completas
- [ ] Validar persistencia en MongoDB

### Fase 4: Mejoras Opcionales (FUTURO)

- [ ] Implementar Outbox Pattern
- [ ] Configurar Retry Policies
- [ ] Configurar Dead Letter Queues
- [ ] Implementar Circuit Breaker
- [ ] Agregar Observabilidad

## Configuration

### Variables de Entorno

```bash
# RabbitMQ
RabbitMq:Host=localhost
RabbitMq:Username=guest
RabbitMq:Password=guest

# PostgreSQL
POSTGRES_HOST=localhost
POSTGRES_DB=eventsdb
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
POSTGRES_PORT=5432

# API
ASPNETCORE_URLS=http://0.0.0.0:5000
ASPNETCORE_ENVIRONMENT=Development
```

### Docker Compose (Propuesto)

```yaml
version: '3.8'

services:
  rabbitmq:
    image: rabbitmq:3-management
    ports:
      - "5672:5672"
      - "15672:15672"
    environment:
      RABBITMQ_DEFAULT_USER: guest
      RABBITMQ_DEFAULT_PASS: guest
    healthcheck:
      test: ["CMD", "rabbitmq-diagnostics", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5

  postgres:
    image: postgres:15
    ports:
      - "5432:5432"
    environment:
      POSTGRES_DB: eventsdb
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
    volumes:
      - postgres_data:/var/lib/postgresql/data

  eventos-api:
    build: .
    ports:
      - "5000:5000"
    environment:
      RabbitMq:Host: rabbitmq
      POSTGRES_HOST: postgres
    depends_on:
      rabbitmq:
        condition: service_healthy
      postgres:
        condition: service_started

volumes:
  postgres_data:
```

## Security Considerations

1. **Credenciales:** Usar variables de entorno, nunca hardcodear
2. **SSL/TLS:** Considerar para producción
3. **Autenticación API:** Implementar JWT o similar
4. **Validación de Entrada:** Ya implementada con FluentValidation
5. **Rate Limiting:** Considerar para endpoints públicos

## Monitoring and Observability

### Métricas Clave

- Tasa de publicación de eventos (eventos/segundo)
- Latencia de publicación (ms)
- Tasa de errores de publicación (%)
- Tamaño de colas en RabbitMQ
- Tiempo de procesamiento de consumidores

### Logs Estructurados

```csharp
_logger.LogInformation(
    "Evento publicado: {EventoId}, Tipo: {TipoEvento}, Timestamp: {Timestamp}",
    eventoId,
    tipoEvento,
    DateTime.UtcNow);
```

### Health Checks

```csharp
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        var result = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description
            })
        });
        await context.Response.WriteAsync(result);
    }
});
```

## Future Enhancements

1. **Event Sourcing:** Almacenar todos los eventos de dominio
2. **CQRS Completo:** Separar modelos de lectura/escritura
3. **Saga Pattern:** Transacciones distribuidas
4. **API Gateway:** Punto de entrada único
5. **Service Mesh:** Istio o Linkerd

## References

- [MassTransit Documentation](https://masstransit-project.com/)
- [RabbitMQ Best Practices](https://www.rabbitmq.com/best-practices.html)
- [Microservices Patterns](https://microservices.io/patterns/)
- [Event-Driven Architecture](https://martinfowler.com/articles/201701-event-driven.html)
