# Arquitectura del Microservicio Comunidad

## 📐 Visión General

El microservicio **Comunidad.API** implementa un sistema de foros y comentarios para eventos, siguiendo los principios de **Arquitectura Hexagonal** (Ports & Adapters) con **Domain-Driven Design (DDD)**.

## 🏛️ Capas de la Arquitectura

```
┌─────────────────────────────────────────────────────────┐
│                    Comunidad.API                        │
│              (Controllers, Program.cs)                  │
│                   Puerto de Entrada                     │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│              Comunidad.Application                      │
│        (Comandos, Queries, DTOs, Handlers)             │
│                  Casos de Uso                           │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│               Comunidad.Domain                          │
│     (Entidades, Value Objects, Interfaces)             │
│                 Lógica de Negocio                       │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│            Comunidad.Infrastructure                     │
│  (Repositorios, MongoDB, RabbitMQ Consumers)           │
│                Puerto de Salida                         │
└─────────────────────────────────────────────────────────┘
```

## 🔷 Capa de Dominio (Domain)

### Entidades

#### Foro
```csharp
public class Foro
{
    public Guid Id { get; set; }
    public Guid EventoId { get; set; }
    public string Titulo { get; set; }
    public DateTime FechaCreacion { get; set; }
}
```

**Responsabilidades:**
- Representa un foro asociado a un evento
- Se crea automáticamente cuando se publica un evento
- Actúa como contenedor para comentarios

#### Comentario
```csharp
public class Comentario
{
    public Guid Id { get; set; }
    public Guid ForoId { get; set; }
    public Guid UsuarioId { get; set; }
    public string Contenido { get; set; }
    public bool EsVisible { get; set; }
    public DateTime FechaCreacion { get; set; }
    public List<Respuesta> Respuestas { get; set; }
}
```

**Responsabilidades:**
- Comentario principal en un foro
- Contiene respuestas embebidas (máximo 2 niveles)
- Soporta soft delete mediante `EsVisible`

### Interfaces de Repositorio

```csharp
public interface IForoRepository
{
    Task<Foro?> ObtenerPorEventoIdAsync(Guid eventoId);
    Task CrearAsync(Foro foro);
    Task<bool> ExistePorEventoIdAsync(Guid eventoId);
}

public interface IComentarioRepository
{
    Task<List<Comentario>> ObtenerPorForoIdAsync(Guid foroId);
    Task<Comentario?> ObtenerPorIdAsync(Guid id);
    Task CrearAsync(Comentario comentario);
    Task ActualizarAsync(Comentario comentario);
}
```

### Contratos Externos

```csharp
// IMPORTANTE: Usa el namespace del emisor para compatibilidad
namespace Eventos.Domain.Events;

public record EventoPublicadoEventoDominio
{
    public Guid EventoId { get; init; }
    public string TituloEvento { get; init; }
    public DateTime FechaInicio { get; init; }
}
```

## 🔶 Capa de Aplicación (Application)

### Comandos (Write Operations)

#### CrearComentarioComando
```csharp
public record CrearComentarioComando(
    Guid ForoId,
    Guid UsuarioId,
    string Contenido
) : IRequest<Guid>;
```

#### ResponderComentarioComando
```csharp
public record ResponderComentarioComando(
    Guid ComentarioId,
    Guid UsuarioId,
    string Contenido
) : IRequest<Unit>;
```

#### OcultarComentarioComando
```csharp
public record OcultarComentarioComando(
    Guid ComentarioId
) : IRequest<Unit>;
```

### Queries (Read Operations)

#### ObtenerComentariosQuery
```csharp
public record ObtenerComentariosQuery(
    Guid EventoId
) : IRequest<List<ComentarioDto>>;
```

### DTOs

```csharp
public class ComentarioDto
{
    public Guid Id { get; set; }
    public Guid ForoId { get; set; }
    public Guid UsuarioId { get; set; }
    public string Contenido { get; set; }
    public DateTime FechaCreacion { get; set; }
    public List<RespuestaDto> Respuestas { get; set; }
}
```

## 🔸 Capa de Infraestructura (Infrastructure)

### MongoDB Context

```csharp
public class MongoDbContext
{
    private readonly IMongoDatabase _database;
    
    public IMongoCollection<Foro> Foros { get; }
    public IMongoCollection<Comentario> Comentarios { get; }
}
```

### Repositorios

Implementan las interfaces definidas en el dominio:
- `ForoRepository`
- `ComentarioRepository`

### RabbitMQ Consumer

```csharp
public class EventoPublicadoConsumer : IConsumer<EventoPublicadoEventoDominio>
{
    public async Task Consume(ConsumeContext<EventoPublicadoEventoDominio> context)
    {
        // 1. Verificar si ya existe el foro
        // 2. Crear foro automáticamente
        // 3. Registrar en logs
    }
}
```

**Configuración:**
- Cola: `comunidad-evento-publicado`
- Exchange: Automático por MassTransit
- Idempotencia: Verifica existencia antes de crear

## 🔺 Capa de API

### Controllers

```csharp
[ApiController]
[Route("api/comunidad")]
public class ComentariosController : ControllerBase
{
    // GET /api/comunidad/foros/{eventoId}
    // POST /api/comunidad/comentarios
    // POST /api/comunidad/comentarios/{id}/responder
    // DELETE /api/comunidad/comentarios/{id}
}
```

## 🔄 Flujos de Datos

### Flujo 1: Creación Automática de Foro

```
┌──────────────┐         ┌──────────────┐         ┌──────────────┐
│   Eventos    │ Publica │   RabbitMQ   │ Consume │  Comunidad   │
│ Microservicio│────────>│   Exchange   │────────>│   Consumer   │
└──────────────┘         └──────────────┘         └──────┬───────┘
                                                          │
                                                          ▼
                                                   ┌──────────────┐
                                                   │   MongoDB    │
                                                   │ Crea Foro    │
                                                   └──────────────┘
```

### Flujo 2: Crear Comentario

```
┌──────────┐  POST    ┌──────────────┐  MediatR  ┌──────────────┐
│ Cliente  │─────────>│ Controller   │──────────>│   Handler    │
└──────────┘          └──────────────┘           └──────┬───────┘
                                                         │
                                                         ▼
                                                  ┌──────────────┐
                                                  │  Repository  │
                                                  │   MongoDB    │
                                                  └──────────────┘
```

### Flujo 3: Obtener Comentarios

```
┌──────────┐   GET    ┌──────────────┐  MediatR  ┌──────────────┐
│ Cliente  │─────────>│ Controller   │──────────>│Query Handler │
└──────────┘          └──────────────┘           └──────┬───────┘
                                                         │
                                                         ▼
                                                  ┌──────────────┐
                                                  │  Repository  │
                                                  │   MongoDB    │
                                                  │ (Solo visibles)
                                                  └──────────────┘
```

## 🗄️ Modelo de Datos MongoDB

### Diseño de Colecciones

#### Colección: Foros
```json
{
  "_id": ObjectId("..."),
  "eventoId": "guid",
  "titulo": "Conferencia Tech 2024",
  "fechaCreacion": ISODate("2024-01-15T10:00:00Z")
}
```

**Índices:**
- `eventoId` (único)

#### Colección: Comentarios
```json
{
  "_id": ObjectId("..."),
  "foroId": "guid",
  "usuarioId": "guid",
  "contenido": "Excelente evento...",
  "esVisible": true,
  "fechaCreacion": ISODate("2024-01-15T11:30:00Z"),
  "respuestas": [
    {
      "usuarioId": "guid",
      "contenido": "Gracias por tu comentario",
      "fechaCreacion": ISODate("2024-01-15T12:00:00Z")
    }
  ]
}
```

**Índices:**
- `foroId`
- `esVisible`

### Ventajas del Diseño Embebido

1. **Performance:** Una sola consulta para obtener comentario + respuestas
2. **Simplicidad:** No hay joins complejos
3. **Atomicidad:** Actualizaciones atómicas del documento
4. **Limitación Natural:** 2 niveles previenen anidación infinita

## 🔐 Patrones de Diseño Aplicados

### 1. Hexagonal Architecture (Ports & Adapters)
- **Puertos:** Interfaces en Domain
- **Adaptadores:** Implementaciones en Infrastructure
- **Beneficio:** Independencia de frameworks y bases de datos

### 2. CQRS (Command Query Responsibility Segregation)
- **Comandos:** Modifican estado (Create, Update, Delete)
- **Queries:** Solo lectura (Get)
- **Beneficio:** Separación clara de responsabilidades

### 3. Mediator Pattern (MediatR)
- **Desacoplamiento:** Controllers no conocen handlers directamente
- **Beneficio:** Fácil testing y mantenimiento

### 4. Repository Pattern
- **Abstracción:** Interfaces en Domain, implementación en Infrastructure
- **Beneficio:** Cambiar MongoDB por otra DB sin afectar lógica de negocio

### 5. Consumer Pattern (MassTransit)
- **Event-Driven:** Reacciona a eventos externos
- **Beneficio:** Desacoplamiento entre microservicios

## 🚀 Decisiones de Arquitectura

### 1. ¿Por qué MongoDB?
- Documentos embebidos para comentarios + respuestas
- Esquema flexible para evolución futura
- Excelente performance para lecturas

### 2. ¿Por qué 2 Niveles de Comentarios?
- Simplicidad en UI (estilo YouTube)
- Evita complejidad de recursión infinita
- Mejor performance con documentos embebidos

### 3. ¿Por qué Soft Delete?
- Auditoría: mantener historial
- Posibilidad de restaurar
- Cumplimiento legal (GDPR)

### 4. ¿Por qué el "Truco del Namespace"?
- MassTransit usa el namespace completo para routing
- Permite consumir eventos sin duplicar contratos
- Mantiene compatibilidad con el emisor

## 📊 Escalabilidad

### Horizontal Scaling
- API stateless: múltiples instancias sin problema
- MongoDB: Replica Sets para alta disponibilidad
- RabbitMQ: Clustering para distribución de carga

### Optimizaciones Futuras
- Caché de comentarios frecuentes (Redis)
- Paginación de comentarios
- Índices compuestos en MongoDB
- Rate limiting por usuario

## 🔍 Monitoreo y Observabilidad

### Logs Estructurados
- Nivel Information para operaciones normales
- Nivel Warning para situaciones anómalas
- Nivel Error para fallos

### Health Checks
- Endpoint `/health` para verificar estado
- Integración con orquestadores (Kubernetes)

### Métricas Sugeridas
- Comentarios creados por minuto
- Tiempo de respuesta de queries
- Tasa de moderación (comentarios ocultos)
- Latencia de consumer RabbitMQ

## 🧪 Testing Strategy

### Unit Tests
- Handlers de comandos y queries
- Lógica de dominio (entidades)
- Validaciones

### Integration Tests
- Repositorios con MongoDB real
- Consumer con RabbitMQ real
- Controllers con API real

### E2E Tests
- Flujo completo: Evento → Foro → Comentarios
- Moderación end-to-end
- Respuestas a comentarios

## 📚 Referencias

- [Hexagonal Architecture](https://alistair.cockburn.us/hexagonal-architecture/)
- [Domain-Driven Design](https://martinfowler.com/bliki/DomainDrivenDesign.html)
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)
- [MassTransit Documentation](https://masstransit-project.com/)
- [MongoDB Best Practices](https://www.mongodb.com/docs/manual/core/data-modeling-introduction/)
