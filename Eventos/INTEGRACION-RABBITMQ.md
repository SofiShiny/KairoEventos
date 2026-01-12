# Integración de RabbitMQ en Microservicio de Eventos

## Resumen de Implementación

Se ha integrado exitosamente la publicación de mensajes hacia RabbitMQ en el microservicio de **Eventos** utilizando MassTransit.

## 1. Configuración Realizada

### 1.1 Dependencias Instaladas

Se agregó `MassTransit.RabbitMQ` versión 8.1.3 en:
- `Eventos.Aplicacion.csproj`
- `Eventos.API.csproj`

### 1.2 Configuración en Program.cs

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

**Variable de Entorno:** `RabbitMq:Host` (por defecto: "localhost")

## 2. Eventos de Dominio Utilizados

### Namespace: `Eventos.Dominio.EventosDeDominio`

Los siguientes eventos de dominio existentes se publican ahora a RabbitMQ:

### 2.1 EventoPublicadoEventoDominio
```csharp
public class EventoPublicadoEventoDominio : EventoDominio
{
    public Guid EventoId { get; }
    public string TituloEvento { get; }
    public DateTime FechaInicio { get; }
}
```

### 2.2 AsistenteRegistradoEventoDominio
```csharp
public class AsistenteRegistradoEventoDominio : EventoDominio
{
    public Guid EventoId { get; }
    public string UsuarioId { get; }
    public string NombreUsuario { get; }
}
```

### 2.3 EventoCanceladoEventoDominio
```csharp
public class EventoCanceladoEventoDominio : EventoDominio
{
    public Guid EventoId { get; }
    public string TituloEvento { get; }
}
```

## 3. Handlers Modificados

### 3.1 PublicarEventoComandoHandler

**Ubicación:** `Eventos.Aplicacion/Comandos/PublicarEventoComandoHandler.cs`

**Cambios:**
- Inyección de `IPublishEndpoint`
- Publicación del evento `EventoPublicadoEventoDominio` después de guardar en PostgreSQL

```csharp
await _publishEndpoint.Publish(new EventoPublicadoEventoDominio(
    evento.Id,
    evento.Titulo,
    evento.FechaInicio), cancellationToken);
```

### 3.2 RegistrarAsistenteComandoHandler

**Ubicación:** `Eventos.Aplicacion/Comandos/RegistrarAsistenteComandoHandler.cs`

**Cambios:**
- Inyección de `IPublishEndpoint`
- Publicación del evento `AsistenteRegistradoEventoDominio` después de guardar en PostgreSQL

```csharp
await _publishEndpoint.Publish(new AsistenteRegistradoEventoDominio(
    request.EventoId,
    request.UsuarioId,
    request.NombreUsuario), cancellationToken);
```

### 3.3 CancelarEventoComandoHandler (NUEVO)

**Ubicación:** `Eventos.Aplicacion/Comandos/CancelarEventoComandoHandler.cs`

**Descripción:** Handler creado para exponer la funcionalidad de cancelación de eventos que existía en el dominio pero no tenía un comando asociado.

**Cambios:**
- Nuevo comando: `CancelarEventoComando`
- Nuevo handler con inyección de `IPublishEndpoint`
- Publicación del evento `EventoCanceladoEventoDominio` después de guardar en PostgreSQL
- Nuevo endpoint: `PATCH /api/eventos/{id}/cancelar`

```csharp
await _publishEndpoint.Publish(new EventoCanceladoEventoDominio(
    evento.Id,
    evento.Titulo), cancellationToken);
```

## 4. Endpoints API

### Endpoints Existentes Modificados:
- `PATCH /api/eventos/{id}/publicar` - Ahora publica a RabbitMQ
- `POST /api/eventos/{id}/asistentes` - Ahora publica a RabbitMQ

### Nuevo Endpoint:
- `PATCH /api/eventos/{id}/cancelar` - Cancela un evento y publica a RabbitMQ

## 5. Estrategia de Publicación

**Patrón Implementado:** Publicación Simple (Fire-and-Forget)

1. Se ejecuta la lógica de negocio en el dominio
2. Se persiste el cambio en PostgreSQL
3. Se publica el evento a RabbitMQ inmediatamente después

**Nota:** Esta es una estrategia simple. Para mayor confiabilidad, considerar implementar:
- Outbox Pattern
- Transacciones distribuidas
- Retry policies
- Dead letter queues

## 6. Configuración para Otros Microservicios

### Namespace para Consumidores

Los microservicios que consuman estos eventos deben usar el namespace:

```csharp
namespace Eventos.Dominio.EventosDeDominio;
```

### Ejemplo de Configuración de Consumidor (MassTransit)

```csharp
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<EventoPublicadoConsumer>();
    x.AddConsumer<AsistenteRegistradoConsumer>();
    x.AddConsumer<EventoCanceladoConsumer>();
    
    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitHost = builder.Configuration["RabbitMq:Host"] ?? "localhost";
        cfg.Host(rabbitHost, h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        
        cfg.ConfigureEndpoints(context);
    });
});
```

## 7. Variables de Entorno

### Microservicio de Eventos

```bash
# RabbitMQ
RabbitMq:Host=localhost  # o la dirección de tu servidor RabbitMQ

# PostgreSQL (ya existentes)
POSTGRES_HOST=localhost
POSTGRES_DB=eventsdb
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
POSTGRES_PORT=5432
```

## 8. Pruebas

### Verificar Publicación de Eventos

1. Iniciar RabbitMQ
2. Iniciar el microservicio de Eventos
3. Ejecutar operaciones:
   - Publicar un evento
   - Registrar un asistente
   - Cancelar un evento
4. Verificar en RabbitMQ Management UI (http://localhost:15672) que los mensajes se publican

### Comandos de Prueba

```bash
# Publicar evento
curl -X PATCH http://localhost:5000/api/eventos/{id}/publicar

# Registrar asistente
curl -X POST http://localhost:5000/api/eventos/{id}/asistentes \
  -H "Content-Type: application/json" \
  -d '{"usuarioId":"user123","nombre":"Juan Pérez","correo":"juan@example.com"}'

# Cancelar evento
curl -X PATCH http://localhost:5000/api/eventos/{id}/cancelar
```

## 9. Próximos Pasos Recomendados

1. **Implementar Outbox Pattern** para garantizar consistencia eventual
2. **Agregar Retry Policies** en MassTransit para manejar fallos temporales
3. **Configurar Dead Letter Queues** para mensajes que fallan repetidamente
4. **Implementar Logging** de eventos publicados
5. **Agregar Métricas** de publicación de mensajes
6. **Implementar Circuit Breaker** para proteger contra fallos de RabbitMQ

## 10. Notas Importantes

- Los eventos se publican **después** de guardar en PostgreSQL
- No hay transacción distribuida entre PostgreSQL y RabbitMQ
- Si RabbitMQ falla, la operación en PostgreSQL ya se habrá completado
- Los eventos de dominio se generan automáticamente en los métodos del agregado
- La publicación a RabbitMQ es responsabilidad de los handlers de aplicación
