# 📋 Refactorización Microservicio Asientos - CQRS + RabbitMQ

## 🎯 Objetivo
Refactorizar el microservicio de Asientos aplicando correctamente el patrón CQRS, reorganizar eventos de dominio e integrar RabbitMQ con MassTransit.

---

## ✅ TAREA 1: AUDITORÍA Y CORRECCIÓN DE CQRS

### **Errores Encontrados y Corregidos:**

#### 1. ❌ **VIOLACIÓN CRÍTICA - Comando retornaba entidad completa**
**Problema:** `CrearMapaAsientosComando` retornaba `MapaAsientos` (entidad de dominio completa) violando CQRS.

**Solución:**
- ✅ Cambiado `IRequest<MapaAsientos>` → `IRequest<Guid>`
- ✅ Handler ahora retorna solo `mapa.Id`
- ✅ Controlador actualizado para recibir solo el Guid

**Archivos modificados:**
- `Asientos.Aplicacion/Comandos/CrearMapaAsientosComando.cs`
- `Asientos.Aplicacion/Handlers/CrearMapaAsientosComandoHandler.cs`

#### 2. ❌ **VIOLACIÓN - Controladores con lógica de presentación**
**Problema:** Los controladores construían ViewModels manualmente con objetos anónimos.

**Solución:**
- ✅ `AsientosController.Crear()` ahora retorna solo `{ asientoId }`
- ✅ `AsientosController.Reservar()` retorna `Ok()` sin datos adicionales
- ✅ `AsientosController.Liberar()` retorna `Ok()` sin datos adicionales
- ✅ Controladores ahora son "thin" - solo ejecutan `_mediator.Send()`

**Archivos modificados:**
- `Asientos.API/Controllers/AsientosController.cs`

#### 3. ❌ **VIOLACIÓN - Controlador inyectaba repositorio directamente**
**Problema:** `MapasAsientosController` inyectaba `IRepositorioMapaAsientos` para hacer queries, violando separación de responsabilidades.

**Solución:**
- ✅ Creada `ObtenerMapaAsientosQuery` con DTOs inmutables
- ✅ Creado `ObtenerMapaAsientosQueryHandler` que encapsula la lógica de lectura
- ✅ Controlador ahora usa `_mediator.Send(new ObtenerMapaAsientosQuery(id))`
- ✅ Separación completa entre Commands (escritura) y Queries (lectura)

**Archivos creados:**
- `Asientos.Aplicacion/Queries/ObtenerMapaAsientosQuery.cs`
- `Asientos.Aplicacion/Queries/ObtenerMapaAsientosQueryHandler.cs`

**Archivos modificados:**
- `Asientos.API/Controllers/MapasAsientosController.cs`

#### 4. ✅ **Comandos ya eran correctos**
- Todos los comandos ya eran `records` inmutables ✓
- Propiedades con `init` setters ✓

---

## ✅ TAREA 2: REFACTORIZACIÓN DE EVENTOS DE DOMINIO

### **Reorganización Completa:**

**Antes:** Todos los eventos en un solo archivo `DomainEvents.cs`

**Después:** Cada evento en su propio archivo con namespace consistente

### **Estructura de Archivos Creada:**

```
Asientos.Dominio/EventosDominio/
├── MapaAsientosCreadoEventoDominio.cs
├── CategoriaAgregadaEventoDominio.cs
├── AsientoAgregadoEventoDominio.cs
├── AsientoReservadoEventoDominio.cs
└── AsientoLiberadoEventoDominio.cs
```

### **Eventos Implementados:**

#### 1. **MapaAsientosCreadoEventoDominio**
```csharp
public class MapaAsientosCreadoEventoDominio : EventoDominio
{
    public Guid MapaId { get; }
    public Guid EventoId { get; }
}
```

#### 2. **CategoriaAgregadaEventoDominio**
```csharp
public class CategoriaAgregadaEventoDominio : EventoDominio
{
    public Guid MapaId { get; }
    public string NombreCategoria { get; }
}
```

#### 3. **AsientoAgregadoEventoDominio**
```csharp
public class AsientoAgregadoEventoDominio : EventoDominio
{
    public Guid MapaId { get; }
    public int Fila { get; }
    public int Numero { get; }
    public string Categoria { get; }
}
```

#### 4. **AsientoReservadoEventoDominio**
```csharp
public class AsientoReservadoEventoDominio : EventoDominio
{
    public Guid MapaId { get; }
    public int Fila { get; }
    public int Numero { get; }
}
```

#### 5. **AsientoLiberadoEventoDominio**
```csharp
public class AsientoLiberadoEventoDominio : EventoDominio
{
    public Guid MapaId { get; }
    public int Fila { get; }
    public int Numero { get; }
}
```

**Namespace consistente:** `Asientos.Dominio.EventosDominio`

**Archivo eliminado:**
- ❌ `DomainEvents.cs` (consolidado)

---

## ✅ TAREA 3: INTEGRACIÓN CON MASSTRANSIT (RABBITMQ)

### **1. Instalación de Paquetes:**

**Paquetes agregados:**
- ✅ `MassTransit.RabbitMQ` v8.1.3 en `Asientos.Aplicacion.csproj`
- ✅ `MassTransit.RabbitMQ` v8.1.3 en `Asientos.API.csproj`

### **2. Configuración en Program.cs:**

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

**Configuración leída de:** `configuration["RabbitMq:Host"]` con fallback a `"localhost"`

**Archivos de configuración creados:**
- ✅ `appsettings.json` con sección `RabbitMq`
- ✅ `appsettings.Development.json` con logging de MassTransit

### **3. Publicación en Handlers:**

**Patrón implementado:** `Save → Publish`

Todos los handlers ahora:
1. Inyectan `IPublishEndpoint`
2. Ejecutan la operación de persistencia
3. Publican el evento a RabbitMQ

#### **CrearMapaAsientosComandoHandler:**
```csharp
public async Task<Guid> Handle(CrearMapaAsientosComando request, CancellationToken cancellationToken)
{
    var mapa = MapaAsientos.Crear(request.EventoId);
    await _repo.AgregarAsync(mapa, cancellationToken);
    
    // Publicar evento a RabbitMQ
    await _publishEndpoint.Publish(new MapaAsientosCreadoEventoDominio(mapa.Id, request.EventoId), cancellationToken);
    
    return mapa.Id;
}
```

#### **AgregarAsientoComandoHandler:**
```csharp
public async Task<Guid> Handle(AgregarAsientoComando request, CancellationToken cancellationToken)
{
    var mapa = await _repo.ObtenerPorIdAsync(request.MapaId, cancellationToken) ?? throw new InvalidOperationException("Mapa no existe");
    var asiento = mapa.AgregarAsiento(request.Fila, request.Numero, request.Categoria);
    var id = await _repo.AgregarAsientoAsync(mapa, asiento, cancellationToken);
    
    // Publicar evento a RabbitMQ
    await _publishEndpoint.Publish(new AsientoAgregadoEventoDominio(request.MapaId, request.Fila, request.Numero, request.Categoria), cancellationToken);
    
    return id;
}
```

#### **AgregarCategoriaComandoHandler:**
```csharp
public async Task<Guid> Handle(AgregarCategoriaComando request, CancellationToken cancellationToken)
{
    var mapa = await _repo.ObtenerPorIdAsync(request.MapaId, cancellationToken) ?? throw new InvalidOperationException("Mapa no existe");
    var cat = mapa.AgregarCategoria(request.Nombre, request.PrecioBase, request.TienePrioridad);
    await _repo.ActualizarAsync(mapa, cancellationToken);
    
    // Publicar evento a RabbitMQ
    await _publishEndpoint.Publish(new CategoriaAgregadaEventoDominio(request.MapaId, request.Nombre), cancellationToken);
    
    return Guid.NewGuid();
}
```

#### **ReservarAsientoComandoHandler:**
```csharp
public async Task<Unit> Handle(ReservarAsientoComando request, CancellationToken cancellationToken)
{
    var mapa = await _repo.ObtenerPorIdAsync(request.MapaId, cancellationToken) ?? throw new InvalidOperationException("Mapa no existe");
    mapa.ReservarAsiento(request.Fila, request.Numero);
    await _repo.ActualizarAsync(mapa, cancellationToken);
    
    // Publicar evento a RabbitMQ
    await _publishEndpoint.Publish(new AsientoReservadoEventoDominio(request.MapaId, request.Fila, request.Numero), cancellationToken);
    
    return Unit.Value;
}
```

#### **LiberarAsientoComandoHandler:**
```csharp
public async Task<Unit> Handle(LiberarAsientoComando request, CancellationToken cancellationToken)
{
    var mapa = await _repo.ObtenerPorIdAsync(request.MapaId, cancellationToken) ?? throw new InvalidOperationException("Mapa no existe");
    mapa.LiberarAsiento(request.Fila, request.Numero);
    await _repo.ActualizarAsync(mapa, cancellationToken);
    
    // Publicar evento a RabbitMQ
    await _publishEndpoint.Publish(new AsientoLiberadoEventoDominio(request.MapaId, request.Fila, request.Numero), cancellationToken);
    
    return Unit.Value;
}
```

**Handlers modificados:**
- ✅ `CrearMapaAsientosComandoHandler.cs`
- ✅ `AgregarAsientoComandoHandler.cs`
- ✅ `AgregarCategoriaComandoHandler.cs`
- ✅ `ReservarAsientoComandoHandler.cs`
- ✅ `LiberarAsientoComandoHandler.cs`

---

## 📊 RESUMEN DE CAMBIOS

### **Archivos Creados (9):**
1. `Asientos.Aplicacion/Queries/ObtenerMapaAsientosQuery.cs`
2. `Asientos.Aplicacion/Queries/ObtenerMapaAsientosQueryHandler.cs`
3. `Asientos.Dominio/EventosDominio/MapaAsientosCreadoEventoDominio.cs`
4. `Asientos.Dominio/EventosDominio/CategoriaAgregadaEventoDominio.cs`
5. `Asientos.Dominio/EventosDominio/AsientoAgregadoEventoDominio.cs`
6. `Asientos.Dominio/EventosDominio/AsientoReservadoEventoDominio.cs`
7. `Asientos.Dominio/EventosDominio/AsientoLiberadoEventoDominio.cs`
8. `Asientos.API/appsettings.json`
9. `Asientos.API/appsettings.Development.json`

### **Archivos Modificados (11):**
1. `Asientos.Aplicacion/Comandos/CrearMapaAsientosComando.cs`
2. `Asientos.Aplicacion/Handlers/CrearMapaAsientosComandoHandler.cs`
3. `Asientos.Aplicacion/Handlers/AgregarAsientoComandoHandler.cs`
4. `Asientos.Aplicacion/Handlers/AgregarCategoriaComandoHandler.cs`
5. `Asientos.Aplicacion/Handlers/ReservarAsientoComandoHandler.cs`
6. `Asientos.Aplicacion/Handlers/LiberarAsientoComandoHandler.cs`
7. `Asientos.API/Controllers/AsientosController.cs`
8. `Asientos.API/Controllers/MapasAsientosController.cs`
9. `Asientos.API/Program.cs`
10. `Asientos.Aplicacion/Asientos.Aplicacion.csproj`
11. `Asientos.API/Asientos.API.csproj`

### **Archivos Eliminados (1):**
1. `Asientos.Dominio/EventosDominio/DomainEvents.cs`

---

## 🏗️ ARQUITECTURA RESULTANTE

### **Separación CQRS Estricta:**
```
Commands (Escritura)          Queries (Lectura)
├── CrearMapaAsientosComando  ├── ObtenerMapaAsientosQuery
├── AgregarAsientoComando     └── ObtenerMapaAsientosQueryHandler
├── AgregarCategoriaComando
├── ReservarAsientoComando
└── LiberarAsientoComando
```

### **Flujo de Eventos:**
```
1. Controller recibe Request
2. Controller ejecuta Command via MediatR
3. Handler ejecuta lógica de negocio
4. Handler persiste cambios en DB
5. Handler publica evento a RabbitMQ
6. Otros microservicios consumen eventos
```

### **Controladores "Thin":**
```csharp
// ✅ CORRECTO - Solo orquestación
public async Task<IActionResult> Crear([FromBody] AsientoCreateDto dto)
{
    var asientoId = await _mediator.Send(new AgregarAsientoComando(...));
    return Ok(new { asientoId });
}

// ❌ INCORRECTO - Lógica de presentación
public async Task<IActionResult> Crear([FromBody] AsientoCreateDto dto)
{
    var id = await _mediator.Send(...);
    return Ok(new { asientoId = id, dto.MapaId, dto.Fila, ... }); // ❌
}
```

---

## 🔧 CONFIGURACIÓN REQUERIDA

### **Variables de Entorno:**
```bash
# PostgreSQL (ya existentes)
POSTGRES_HOST=localhost
POSTGRES_DB=asientosdb
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
POSTGRES_PORT=5432

# RabbitMQ (nueva)
RabbitMq__Host=localhost  # o usar appsettings.json
```

### **appsettings.json:**
```json
{
  "RabbitMq": {
    "Host": "localhost"
  }
}
```

### **Docker Compose (ejemplo):**
```yaml
services:
  rabbitmq:
    image: rabbitmq:3-management
    ports:
      - "5672:5672"
      - "15672:15672"
    environment:
      RABBITMQ_DEFAULT_USER: guest
      RABBITMQ_DEFAULT_PASS: guest
```

---

## ✅ VERIFICACIÓN

### **1. Compilación:**
```bash
cd Asientos/backend/src/Services/Asientos
dotnet build
```

### **2. Verificar Eventos Publicados:**
- Acceder a RabbitMQ Management: http://localhost:15672
- Usuario: `guest` / Password: `guest`
- Verificar exchanges y queues creados por MassTransit

### **3. Health Check:**
```bash
curl http://localhost:5000/health
```

Respuesta esperada:
```json
{
  "status": "healthy",
  "db": "postgres",
  "rabbitmq": "localhost"
}
```

---

## 📚 PRINCIPIOS APLICADOS

### **CQRS:**
- ✅ Separación estricta Commands/Queries
- ✅ Commands retornan solo IDs o Unit
- ✅ Queries retornan DTOs inmutables
- ✅ Sin lógica de negocio en controladores

### **Hexagonal:**
- ✅ Dominio independiente de infraestructura
- ✅ Eventos de dominio en capa de dominio
- ✅ Handlers en capa de aplicación
- ✅ Controladores en capa de API

### **Event-Driven:**
- ✅ Eventos publicados después de persistencia
- ✅ Eventos inmutables con propiedades readonly
- ✅ Un evento por archivo
- ✅ Namespace consistente

---

## 🎯 PRÓXIMOS PASOS

1. **Crear Consumers en microservicio Reportes** para escuchar estos eventos
2. **Implementar retry policies** en MassTransit para resiliencia
3. **Agregar logging** de eventos publicados
4. **Implementar tests de integración** con RabbitMQ
5. **Configurar dead-letter queues** para eventos fallidos

---

## 📝 NOTAS TÉCNICAS

- **MassTransit v8.1.3** utiliza convenciones automáticas para nombres de exchanges/queues
- Los eventos se publican al exchange `Asientos.Dominio.EventosDominio:NombreEvento`
- Consumers en otros microservicios deben implementar `IConsumer<TEvento>`
- La configuración `cfg.ConfigureEndpoints(context)` auto-descubre consumers

---

**Fecha:** 29 de Diciembre de 2024  
**Arquitecto:** Sistema de Eventos - Microservicio Asientos  
**Estado:** ✅ Completado
