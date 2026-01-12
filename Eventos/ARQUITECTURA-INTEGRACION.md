# 🏗️ Arquitectura de Integración RabbitMQ

## 📐 Diagrama de Flujo

```
┌─────────────────────────────────────────────────────────────────────┐
│                         CLIENTE (HTTP)                               │
└────────────────────────────────┬────────────────────────────────────┘
                                 │
                                 ▼
┌─────────────────────────────────────────────────────────────────────┐
│                      EVENTOS.API (Controllers)                       │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │  EventosController                                            │  │
│  │  - POST   /api/eventos                                        │  │
│  │  - PATCH  /api/eventos/{id}/publicar      ◄── Publica        │  │
│  │  - POST   /api/eventos/{id}/asistentes    ◄── Publica        │  │
│  │  - PATCH  /api/eventos/{id}/cancelar      ◄── Publica        │  │
│  └──────────────────────────────────────────────────────────────┘  │
└────────────────────────────────┬────────────────────────────────────┘
                                 │ MediatR
                                 ▼
┌─────────────────────────────────────────────────────────────────────┐
│                   EVENTOS.APLICACION (Handlers)                      │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │  PublicarEventoComandoHandler                                 │  │
│  │  ├─ 1. Obtener evento del repositorio                         │  │
│  │  ├─ 2. evento.Publicar() [Dominio]                           │  │
│  │  ├─ 3. Guardar en PostgreSQL                                  │  │
│  │  └─ 4. Publicar a RabbitMQ ──────────────────────┐           │  │
│  │                                                    │           │  │
│  │  RegistrarAsistenteComandoHandler                 │           │  │
│  │  ├─ 1. Obtener evento del repositorio             │           │  │
│  │  ├─ 2. evento.RegistrarAsistente() [Dominio]     │           │  │
│  │  ├─ 3. Guardar en PostgreSQL                      │           │  │
│  │  └─ 4. Publicar a RabbitMQ ──────────────────────┤           │  │
│  │                                                    │           │  │
│  │  CancelarEventoComandoHandler                     │           │  │
│  │  ├─ 1. Obtener evento del repositorio             │           │  │
│  │  ├─ 2. evento.Cancelar() [Dominio]               │           │  │
│  │  ├─ 3. Guardar en PostgreSQL                      │           │  │
│  │  └─ 4. Publicar a RabbitMQ ──────────────────────┤           │  │
│  └──────────────────────────────────────────────────┼───────────┘  │
└─────────────────────────────────────────────────────┼───────────────┘
                                                       │
                    ┌──────────────────────────────────┤
                    │                                  │
                    ▼                                  ▼
┌──────────────────────────────────┐  ┌──────────────────────────────┐
│      POSTGRESQL                  │  │      RABBITMQ                │
│  ┌────────────────────────────┐  │  │  ┌────────────────────────┐ │
│  │  Eventos                   │  │  │  │  Exchange              │ │
│  │  - Id                      │  │  │  │  (MassTransit)         │ │
│  │  - Titulo                  │  │  │  └───────┬────────────────┘ │
│  │  - Estado                  │  │  │          │                  │
│  │  - ...                     │  │  │          ▼                  │
│  └────────────────────────────┘  │  │  ┌────────────────────────┐ │
│                                   │  │  │  Queues                │ │
│  ┌────────────────────────────┐  │  │  │  - EventoPublicado     │ │
│  │  Asistentes                │  │  │  │  - AsistenteRegistrado │ │
│  │  - Id                      │  │  │  │  - EventoCancelado     │ │
│  │  - EventoId                │  │  │  └────────────────────────┘ │
│  │  - UsuarioId               │  │  │                              │
│  │  - ...                     │  │  └──────────────────────────────┘
│  └────────────────────────────┘  │
└──────────────────────────────────┘
                                                       │
                                                       ▼
                                    ┌──────────────────────────────────┐
                                    │  OTROS MICROSERVICIOS            │
                                    │  (Consumidores)                  │
                                    │  - Reportes                      │
                                    │  - Asientos                      │
                                    │  - Notificaciones                │
                                    └──────────────────────────────────┘
```

## 🔄 Flujo de Datos Detallado

### 1. Publicar Evento

```
Cliente → API → Handler → Dominio → PostgreSQL → RabbitMQ → Consumidores
   │       │       │         │           │           │
   │       │       │         │           │           └─ EventoPublicadoEventoDominio
   │       │       │         │           │              {
   │       │       │         │           │                EventoId: guid,
   │       │       │         │           │                TituloEvento: string,
   │       │       │         │           │                FechaInicio: datetime
   │       │       │         │           │              }
   │       │       │         │           │
   │       │       │         │           └─ UPDATE eventos SET estado = 'Publicado'
   │       │       │         │
   │       │       │         └─ evento.Publicar()
   │       │       │            - Valida estado
   │       │       │            - Cambia estado a Publicado
   │       │       │            - Genera evento de dominio
   │       │       │
   │       │       └─ PublicarEventoComandoHandler
   │       │          - Obtiene evento
   │       │          - Ejecuta lógica de dominio
   │       │          - Persiste cambios
   │       │          - Publica a RabbitMQ
   │       │
   │       └─ EventosController.Publicar(id)
   │          PATCH /api/eventos/{id}/publicar
   │
   └─ HTTP PATCH Request
```

### 2. Registrar Asistente

```
Cliente → API → Handler → Dominio → PostgreSQL → RabbitMQ → Consumidores
   │       │       │         │           │           │
   │       │       │         │           │           └─ AsistenteRegistradoEventoDominio
   │       │       │         │           │              {
   │       │       │         │           │                EventoId: guid,
   │       │       │         │           │                UsuarioId: string,
   │       │       │         │           │                NombreUsuario: string
   │       │       │         │           │              }
   │       │       │         │           │
   │       │       │         │           └─ INSERT INTO asistentes (...)
   │       │       │         │
   │       │       │         └─ evento.RegistrarAsistente(...)
   │       │       │            - Valida estado del evento
   │       │       │            - Valida capacidad
   │       │       │            - Valida duplicados
   │       │       │            - Agrega asistente
   │       │       │            - Genera evento de dominio
   │       │       │
   │       │       └─ RegistrarAsistenteComandoHandler
   │       │          - Obtiene evento
   │       │          - Ejecuta lógica de dominio
   │       │          - Persiste cambios
   │       │          - Publica a RabbitMQ
   │       │
   │       └─ EventosController.RegistrarAsistente(id, dto)
   │          POST /api/eventos/{id}/asistentes
   │
   └─ HTTP POST Request
```

### 3. Cancelar Evento

```
Cliente → API → Handler → Dominio → PostgreSQL → RabbitMQ → Consumidores
   │       │       │         │           │           │
   │       │       │         │           │           └─ EventoCanceladoEventoDominio
   │       │       │         │           │              {
   │       │       │         │           │                EventoId: guid,
   │       │       │         │           │                TituloEvento: string
   │       │       │         │           │              }
   │       │       │         │           │
   │       │       │         │           └─ UPDATE eventos SET estado = 'Cancelado'
   │       │       │         │
   │       │       │         └─ evento.Cancelar()
   │       │       │            - Valida estado
   │       │       │            - Cambia estado a Cancelado
   │       │       │            - Genera evento de dominio
   │       │       │
   │       │       └─ CancelarEventoComandoHandler
   │       │          - Obtiene evento
   │       │          - Ejecuta lógica de dominio
   │       │          - Persiste cambios
   │       │          - Publica a RabbitMQ
   │       │
   │       └─ EventosController.Cancelar(id)
   │          PATCH /api/eventos/{id}/cancelar
   │
   └─ HTTP PATCH Request
```

## 🏛️ Capas de la Arquitectura

### 1. API Layer (Eventos.API)
- **Responsabilidad:** Exponer endpoints HTTP
- **Tecnologías:** ASP.NET Core, Swagger
- **Componentes:**
  - Controllers
  - Middleware
  - Configuración de servicios

### 2. Application Layer (Eventos.Aplicacion)
- **Responsabilidad:** Orquestar casos de uso
- **Tecnologías:** MediatR, MassTransit
- **Componentes:**
  - Command Handlers
  - Query Handlers
  - DTOs
  - Validators

### 3. Domain Layer (Eventos.Dominio)
- **Responsabilidad:** Lógica de negocio
- **Tecnologías:** C# puro
- **Componentes:**
  - Agregados (Evento)
  - Entidades (Asistente)
  - Value Objects (Ubicacion)
  - Domain Events
  - Repositorios (interfaces)

### 4. Infrastructure Layer (Eventos.Infraestructura)
- **Responsabilidad:** Implementación técnica
- **Tecnologías:** Entity Framework Core, PostgreSQL
- **Componentes:**
  - DbContext
  - Repositorios (implementaciones)
  - Migrations

## 🔌 Integración con MassTransit

### Configuración

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

### Publicación

```csharp
await _publishEndpoint.Publish(new EventoPublicadoEventoDominio(
    evento.Id,
    evento.Titulo,
    evento.FechaInicio), 
    cancellationToken);
```

### Convenciones de Nombres

MassTransit crea automáticamente:
- **Exchange:** Basado en el namespace y nombre del tipo
- **Queue:** Una por cada tipo de mensaje y consumidor
- **Routing Key:** Automático basado en el tipo de mensaje

Ejemplo:
- Tipo: `Eventos.Dominio.EventosDeDominio.EventoPublicadoEventoDominio`
- Exchange: `Eventos.Dominio.EventosDeDominio:EventoPublicadoEventoDominio`
- Queue: `{ConsumerName}_{MessageType}`

## 🔐 Consideraciones de Seguridad

1. **Credenciales de RabbitMQ:** Usar variables de entorno, no hardcodear
2. **Conexión SSL/TLS:** Considerar para producción
3. **Autenticación de API:** Implementar JWT o similar
4. **Validación de entrada:** Ya implementada con FluentValidation

## 📊 Monitoreo y Observabilidad

### Puntos de Monitoreo

1. **API Endpoints:**
   - Latencia de respuesta
   - Tasa de errores
   - Throughput

2. **PostgreSQL:**
   - Conexiones activas
   - Tiempo de queries
   - Tamaño de base de datos

3. **RabbitMQ:**
   - Mensajes publicados
   - Mensajes consumidos
   - Mensajes en cola
   - Tasa de errores

4. **Handlers:**
   - Tiempo de ejecución
   - Tasa de éxito/fallo
   - Excepciones

### Herramientas Recomendadas

- **Logs:** Serilog, ELK Stack
- **Métricas:** Prometheus, Grafana
- **Tracing:** OpenTelemetry, Jaeger
- **APM:** Application Insights, New Relic

## 🚀 Escalabilidad

### Horizontal Scaling

```
┌─────────────┐
│  Load       │
│  Balancer   │
└──────┬──────┘
       │
   ┌───┴───┬───────┬───────┐
   │       │       │       │
   ▼       ▼       ▼       ▼
┌─────┐ ┌─────┐ ┌─────┐ ┌─────┐
│ API │ │ API │ │ API │ │ API │
│  1  │ │  2  │ │  3  │ │  N  │
└──┬──┘ └──┬──┘ └──┬──┘ └──┬──┘
   │       │       │       │
   └───┬───┴───┬───┴───┬───┘
       │       │       │
       ▼       ▼       ▼
   ┌─────────────────────┐
   │    PostgreSQL       │
   │    (Primary)        │
   └─────────────────────┘
       │
       ▼
   ┌─────────────────────┐
   │    RabbitMQ         │
   │    (Cluster)        │
   └─────────────────────┘
```

### Consideraciones

1. **Stateless API:** Cada instancia es independiente
2. **Shared Database:** PostgreSQL como fuente única de verdad
3. **Message Broker:** RabbitMQ distribuye mensajes entre consumidores
4. **Idempotencia:** Importante para manejar reintentos

## 📝 Patrones Implementados

1. ✅ **Hexagonal Architecture** (Ports & Adapters)
2. ✅ **Domain-Driven Design** (DDD)
3. ✅ **CQRS** (Command Query Responsibility Segregation)
4. ✅ **Mediator Pattern** (MediatR)
5. ✅ **Repository Pattern**
6. ✅ **Domain Events**
7. ✅ **Event-Driven Architecture**

## 🔮 Mejoras Futuras

1. **Outbox Pattern:** Garantizar consistencia eventual
2. **Saga Pattern:** Transacciones distribuidas
3. **Event Sourcing:** Historial completo de cambios
4. **CQRS Completo:** Separar modelos de lectura/escritura
5. **API Gateway:** Punto de entrada único
6. **Service Mesh:** Istio, Linkerd
7. **Circuit Breaker:** Resilience4j, Polly

---

**Arquitectura:** ✅ IMPLEMENTADA Y DOCUMENTADA
