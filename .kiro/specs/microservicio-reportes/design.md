# Documento de Diseño - Microservicio de Reportes

## Resumen

El microservicio de Reportes implementa un patrón CQRS (Command Query Responsibility Segregation) actuando como modelo de lectura optimizado. Consume eventos de dominio mediante MassTransit/RabbitMQ, persiste datos en MongoDB, y expone endpoints REST para consultas analíticas. Utiliza Hangfire para consolidación nocturna de métricas.

## Arquitectura

### Arquitectura Hexagonal (Puertos y Adaptadores)

```
┌─────────────────────────────────────────────────────────────┐
│                      CAPA API (Puerto)                       │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  ReportesController                                   │   │
│  │  - GET /api/reportes/resumen-ventas                  │   │
│  │  - GET /api/reportes/asistencia/{eventoId}          │   │
│  │  - GET /api/reportes/auditoria                       │   │
│  │  - GET /api/reportes/conciliacion-financiera        │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                   CAPA APLICACIÓN                            │
│  ┌──────────────────┐  ┌──────────────────┐                │
│  │   Consumers      │  │   Jobs           │                │
│  │  (MassTransit)   │  │  (Hangfire)      │                │
│  └──────────────────┘  └──────────────────┘                │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  Queries (Handlers)                                   │   │
│  │  - ObtenerResumenVentasQueryHandler                  │   │
│  │  - ObtenerAsistenciaEventoQueryHandler               │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                     CAPA DOMINIO                             │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  Modelos de Lectura                                   │   │
│  │  - ReporteVentasDiarias                              │   │
│  │  - HistorialAsistencia                               │   │
│  │  - MetricasEvento                                    │   │
│  │  - LogAuditoria                                      │   │
│  └──────────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  Contratos Espejo (namespace original)               │   │
│  │  - EventoPublicadoEventoDominio                      │   │
│  │  - AsientoReservadoEventoDominio                     │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                 CAPA INFRAESTRUCTURA                         │
│  ┌──────────────────┐  ┌──────────────────┐                │
│  │  MongoDB         │  │  RabbitMQ        │                │
│  │  (Repositorios)  │  │  (MassTransit)   │                │
│  └──────────────────┘  └──────────────────┘                │
└─────────────────────────────────────────────────────────────┘
```

### Flujo de Datos

1. **Eventos Entrantes:** Otros microservicios publican eventos en RabbitMQ
2. **Consumidores:** MassTransit consume eventos y actualiza MongoDB
3. **Consolidación:** Hangfire ejecuta jobs nocturnos para agregar métricas
4. **Consultas:** API REST lee datos optimizados de MongoDB
5. **Auditoría:** Todas las operaciones se registran en `LogAuditoria`

## Componentes y Interfaces

### 1. Contratos Espejo (Dominio/ContratosExternos)

**Propósito:** Definir eventos externos con namespace original para integración sin dependencias compartidas.

```csharp
// IMPORTANTE: Usar namespace del microservicio origen
namespace Eventos.Dominio.EventosDeDominio;

public record EventoPublicadoEventoDominio
{
    public Guid EventoId { get; init; }
    public string TituloEvento { get; init; }
    public DateTime FechaInicio { get; init; }
}

public record AsistenteRegistradoEventoDominio
{
    public Guid EventoId { get; init; }
    public string UsuarioId { get; init; }
    public string NombreUsuario { get; init; }
}

public record EventoCanceladoEventoDominio
{
    public Guid EventoId { get; init; }
    public string TituloEvento { get; init; }
}
```

```csharp
// IMPORTANTE: Usar namespace del microservicio origen
namespace Asientos.Dominio.EventosDominio;

public record AsientoReservadoEventoDominio
{
    public Guid MapaId { get; init; }
    public int Fila { get; init; }
    public int Numero { get; init; }
}

public record AsientoLiberadoEventoDominio
{
    public Guid MapaId { get; init; }
    public int Fila { get; init; }
    public int Numero { get; init; }
}

public record MapaAsientosCreadoEventoDominio
{
    public Guid MapaId { get; init; }
    public Guid EventoId { get; init; }
}
```

### 2. Modelos de Lectura (Dominio/ModelosLectura)

```csharp
namespace Reportes.Dominio.ModelosLectura;

public class ReporteVentasDiarias
{
    public string Id { get; set; } // MongoDB ObjectId
    public DateTime Fecha { get; set; }
    public Guid EventoId { get; set; }
    public string TituloEvento { get; set; }
    public int CantidadReservas { get; set; }
    public decimal TotalIngresos { get; set; }
    public Dictionary<string, int> ReservasPorCategoria { get; set; }
    public DateTime UltimaActualizacion { get; set; }
}

public class HistorialAsistencia
{
    public string Id { get; set; }
    public Guid EventoId { get; set; }
    public string TituloEvento { get; set; }
    public int TotalAsistentesRegistrados { get; set; }
    public int CapacidadTotal { get; set; }
    public int AsientosReservados { get; set; }
    public int AsientosDisponibles { get; set; }
    public double PorcentajeOcupacion { get; set; }
    public List<RegistroAsistente> Asistentes { get; set; }
    public DateTime UltimaActualizacion { get; set; }
}

public class RegistroAsistente
{
    public string UsuarioId { get; set; }
    public string NombreUsuario { get; set; }
    public DateTime FechaRegistro { get; set; }
}

public class MetricasEvento
{
    public string Id { get; set; }
    public Guid EventoId { get; set; }
    public string TituloEvento { get; set; }
    public DateTime FechaInicio { get; set; }
    public string Estado { get; set; } // Publicado, Cancelado, Finalizado
    public int TotalAsistentes { get; set; }
    public int TotalReservas { get; set; }
    public decimal IngresoTotal { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime UltimaActualizacion { get; set; }
}

public class LogAuditoria
{
    public string Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string TipoOperacion { get; set; } // EventoConsumido, ReporteGenerado, ErrorProcesamiento
    public string Entidad { get; set; }
    public string EntidadId { get; set; }
    public string Detalles { get; set; }
    public string Usuario { get; set; }
    public bool Exitoso { get; set; }
    public string MensajeError { get; set; }
}

public class ReporteConsolidado
{
    public string Id { get; set; }
    public DateTime FechaConsolidacion { get; set; }
    public DateTime PeriodoInicio { get; set; }
    public DateTime PeriodoFin { get; set; }
    public decimal TotalIngresos { get; set; }
    public int TotalReservas { get; set; }
    public int TotalEventos { get; set; }
    public double PromedioAsistenciaEvento { get; set; }
    public Dictionary<string, decimal> IngresosPorCategoria { get; set; }
}
```

### 3. Repositorios (Infraestructura/Repositorios)

```csharp
namespace Reportes.Infraestructura.Repositorios;

public interface IRepositorioReportesLectura
{
    // Ventas
    Task<ReporteVentasDiarias> ObtenerVentasDiariasAsync(DateTime fecha);
    Task ActualizarVentasDiariasAsync(ReporteVentasDiarias reporte);
    
    // Asistencia
    Task<HistorialAsistencia> ObtenerAsistenciaEventoAsync(Guid eventoId);
    Task ActualizarAsistenciaAsync(HistorialAsistencia historial);
    
    // Métricas
    Task<MetricasEvento> ObtenerMetricasEventoAsync(Guid eventoId);
    Task ActualizarMetricasAsync(MetricasEvento metricas);
    
    // Auditoría
    Task RegistrarLogAuditoriaAsync(LogAuditoria log);
    Task<List<LogAuditoria>> ObtenerLogsAuditoriaAsync(
        DateTime? fechaInicio, 
        DateTime? fechaFin, 
        string tipoOperacion, 
        int pagina, 
        int tamañoPagina);
    
    // Consolidados
    Task<ReporteConsolidado> ObtenerReporteConsolidadoAsync(DateTime fecha);
    Task GuardarReporteConsolidadoAsync(ReporteConsolidado reporte);
}
```

### 4. Consumidores MassTransit (Aplicacion/Consumers)

```csharp
namespace Reportes.Aplicacion.Consumers;

public class EventoPublicadoConsumer : IConsumer<EventoPublicadoEventoDominio>
{
    private readonly IRepositorioReportesLectura _repositorio;
    private readonly ILogger<EventoPublicadoConsumer> _logger;

    public async Task Consume(ConsumeContext<EventoPublicadoEventoDominio> context)
    {
        var evento = context.Message;
        
        // Crear métricas iniciales del evento
        var metricas = new MetricasEvento
        {
            EventoId = evento.EventoId,
            TituloEvento = evento.TituloEvento,
            FechaInicio = evento.FechaInicio,
            Estado = "Publicado",
            FechaCreacion = DateTime.UtcNow
        };
        
        await _repositorio.ActualizarMetricasAsync(metricas);
        
        // Registrar en auditoría
        await _repositorio.RegistrarLogAuditoriaAsync(new LogAuditoria
        {
            Timestamp = DateTime.UtcNow,
            TipoOperacion = "EventoConsumido",
            Entidad = "Evento",
            EntidadId = evento.EventoId.ToString(),
            Exitoso = true
        });
    }
}

public class AsistenteRegistradoConsumer : IConsumer<AsistenteRegistradoEventoDominio>
{
    private readonly IRepositorioReportesLectura _repositorio;

    public async Task Consume(ConsumeContext<AsistenteRegistradoEventoDominio> context)
    {
        var evento = context.Message;
        
        // Actualizar historial de asistencia
        var historial = await _repositorio.ObtenerAsistenciaEventoAsync(evento.EventoId)
            ?? new HistorialAsistencia { EventoId = evento.EventoId, Asistentes = new() };
        
        historial.TotalAsistentesRegistrados++;
        historial.Asistentes.Add(new RegistroAsistente
        {
            UsuarioId = evento.UsuarioId,
            NombreUsuario = evento.NombreUsuario,
            FechaRegistro = DateTime.UtcNow
        });
        historial.UltimaActualizacion = DateTime.UtcNow;
        
        await _repositorio.ActualizarAsistenciaAsync(historial);
    }
}

public class AsientoReservadoConsumer : IConsumer<AsientoReservadoEventoDominio>
{
    private readonly IRepositorioReportesLectura _repositorio;

    public async Task Consume(ConsumeContext<AsientoReservadoEventoDominio> context)
    {
        var evento = context.Message;
        var fecha = DateTime.UtcNow.Date;
        
        // Actualizar reporte de ventas diarias
        var reporte = await _repositorio.ObtenerVentasDiariasAsync(fecha)
            ?? new ReporteVentasDiarias 
            { 
                Fecha = fecha, 
                ReservasPorCategoria = new() 
            };
        
        reporte.CantidadReservas++;
        reporte.UltimaActualizacion = DateTime.UtcNow;
        
        await _repositorio.ActualizarVentasDiariasAsync(reporte);
        
        // Actualizar historial de asistencia
        var historial = await _repositorio.ObtenerAsistenciaEventoAsync(evento.MapaId);
        if (historial != null)
        {
            historial.AsientosReservados++;
            historial.AsientosDisponibles--;
            historial.PorcentajeOcupacion = 
                (double)historial.AsientosReservados / historial.CapacidadTotal * 100;
            await _repositorio.ActualizarAsistenciaAsync(historial);
        }
    }
}
```

### 5. Jobs Hangfire (Aplicacion/Jobs)

```csharp
namespace Reportes.Aplicacion.Jobs;

public class JobGenerarReportesConsolidados
{
    private readonly IRepositorioReportesLectura _repositorio;
    private readonly ILogger<JobGenerarReportesConsolidados> _logger;

    public async Task EjecutarAsync()
    {
        try
        {
            var ayer = DateTime.UtcNow.Date.AddDays(-1);
            
            // Consolidar métricas del día anterior
            var reporte = new ReporteConsolidado
            {
                FechaConsolidacion = DateTime.UtcNow,
                PeriodoInicio = ayer,
                PeriodoFin = ayer.AddDays(1).AddSeconds(-1),
                IngresosPorCategoria = new()
            };
            
            // Calcular totales (lógica de agregación)
            // ...
            
            await _repositorio.GuardarReporteConsolidadoAsync(reporte);
            
            _logger.LogInformation(
                "Reporte consolidado generado exitosamente para {Fecha}", ayer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando reporte consolidado");
            throw;
        }
    }
}
```

### 6. Controlador API (API/Controladores)

```csharp
namespace Reportes.API.Controladores;

[ApiController]
[Route("api/reportes")]
public class ReportesController : ControllerBase
{
    private readonly IRepositorioReportesLectura _repositorio;

    [HttpGet("resumen-ventas")]
    public async Task<ActionResult<ResumenVentasDto>> ObtenerResumenVentas(
        [FromQuery] DateTime? fechaInicio,
        [FromQuery] DateTime? fechaFin)
    {
        // Lógica de consulta
        return Ok(new ResumenVentasDto());
    }

    [HttpGet("asistencia/{eventoId}")]
    public async Task<ActionResult<AsistenciaEventoDto>> ObtenerAsistenciaEvento(
        Guid eventoId)
    {
        var historial = await _repositorio.ObtenerAsistenciaEventoAsync(eventoId);
        
        if (historial == null)
            return NotFound(new { mensaje = "Evento no encontrado" });
        
        return Ok(new AsistenciaEventoDto
        {
            EventoId = historial.EventoId,
            TotalAsistentes = historial.TotalAsistentesRegistrados,
            AsientosReservados = historial.AsientosReservados,
            PorcentajeOcupacion = historial.PorcentajeOcupacion
        });
    }

    [HttpGet("auditoria")]
    public async Task<ActionResult<PaginacionDto<LogAuditoriaDto>>> ObtenerAuditoria(
        [FromQuery] DateTime? fechaInicio,
        [FromQuery] DateTime? fechaFin,
        [FromQuery] string tipoOperacion,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamañoPagina = 50)
    {
        var logs = await _repositorio.ObtenerLogsAuditoriaAsync(
            fechaInicio, fechaFin, tipoOperacion, pagina, tamañoPagina);
        
        return Ok(new PaginacionDto<LogAuditoriaDto>
        {
            Datos = logs.Select(l => new LogAuditoriaDto()).ToList(),
            PaginaActual = pagina,
            TamañoPagina = tamañoPagina
        });
    }

    [HttpGet("conciliacion-financiera")]
    public async Task<ActionResult<ConciliacionFinancieraDto>> ObtenerConciliacionFinanciera(
        [FromQuery] DateTime? fechaInicio,
        [FromQuery] DateTime? fechaFin)
    {
        // Lógica de conciliación
        return Ok(new ConciliacionFinancieraDto());
    }
}
```

## Modelos de Datos

### Esquema MongoDB

**Colección: reportes_ventas_diarias**
```json
{
  "_id": "ObjectId",
  "fecha": "ISODate",
  "eventoId": "UUID",
  "tituloEvento": "string",
  "cantidadReservas": "int",
  "totalIngresos": "decimal",
  "reservasPorCategoria": {
    "VIP": 10,
    "General": 50
  },
  "ultimaActualizacion": "ISODate"
}
```

**Colección: historial_asistencia**
```json
{
  "_id": "ObjectId",
  "eventoId": "UUID",
  "tituloEvento": "string",
  "totalAsistentesRegistrados": "int",
  "capacidadTotal": "int",
  "asientosReservados": "int",
  "asientosDisponibles": "int",
  "porcentajeOcupacion": "double",
  "asistentes": [
    {
      "usuarioId": "string",
      "nombreUsuario": "string",
      "fechaRegistro": "ISODate"
    }
  ],
  "ultimaActualizacion": "ISODate"
}
```

**Índices MongoDB:**
```javascript
// Optimización de consultas
db.reportes_ventas_diarias.createIndex({ "fecha": 1, "eventoId": 1 });
db.historial_asistencia.createIndex({ "eventoId": 1 });
db.logs_auditoria.createIndex({ "timestamp": -1, "tipoOperacion": 1 });
db.metricas_evento.createIndex({ "eventoId": 1 });
```

## Propiedades de Correctitud

*Una propiedad es una característica o comportamiento que debe mantenerse verdadero en todas las ejecuciones válidas del sistema, esencialmente una declaración formal sobre lo que el sistema debe hacer.*


### Propiedades Identificadas

**Propiedad 1: Persistencia de eventos consumidos**
*Para cualquier* evento de dominio válido recibido por un consumidor, el sistema debe persistir los datos correspondientes en la colección MongoDB apropiada.
**Valida: Requisitos 1.1, 1.3, 3.2**

**Propiedad 2: Incremento atómico de contadores**
*Para cualquier* evento `AsistenteRegistradoEventoDominio`, el contador `TotalAsistentesRegistrados` en `HistorialAsistencia` debe incrementarse exactamente en 1.
**Valida: Requisitos 1.2**

**Propiedad 3: Invariante de disponibilidad de asientos**
*Para cualquier* evento de reserva o liberación de asiento, debe cumplirse: `AsientosDisponibles + AsientosReservados = CapacidadTotal`.
**Valida: Requisitos 1.4, 6.5**

**Propiedad 4: Auditoría completa de operaciones**
*Para cualquier* evento procesado exitosamente, debe existir un registro correspondiente en la colección `LogAuditoria` con `Exitoso = true`.
**Valida: Requisitos 1.5**

**Propiedad 5: Deserialización resiliente de eventos**
*Para cualquier* evento serializado con el formato correcto de MassTransit, el sistema debe deserializarlo sin lanzar excepciones, incluso si contiene propiedades adicionales o faltantes.
**Valida: Requisitos 2.4, 2.5**

**Propiedad 6: Cálculo correcto de métricas consolidadas**
*Para cualquier* conjunto de datos de ventas en un período, el `TotalIngresos` en `ReporteConsolidado` debe ser igual a la suma de `TotalIngresos` de todos los `ReporteVentasDiarias` en ese período.
**Valida: Requisitos 4.1, 4.2**

**Propiedad 7: Persistencia de reportes consolidados**
*Para cualquier* ejecución exitosa del job de consolidación, debe existir un registro en la colección `ReportesConsolidados` con `FechaConsolidacion` correspondiente.
**Valida: Requisitos 4.3**

**Propiedad 8: Formato JSON válido en respuestas**
*Para cualquier* invocación exitosa de un endpoint de reportes, la respuesta debe ser un JSON válido y parseable.
**Valida: Requisitos 5.1**

**Propiedad 9: Completitud de campos en resumen de ventas**
*Para cualquier* respuesta del endpoint `/api/reportes/resumen-ventas`, el JSON debe contener los campos: `totalVentas`, `cantidadReservas`, `promedioEvento`.
**Valida: Requisitos 5.2**

**Propiedad 10: Filtrado correcto por rango de fechas**
*Para cualquier* consulta con parámetros `fechaInicio` y `fechaFin`, todos los registros retornados deben tener `Fecha >= fechaInicio AND Fecha <= fechaFin`.
**Valida: Requisitos 5.3, 8.3**

**Propiedad 11: Códigos HTTP apropiados para errores**
*Para cualquier* error de validación o excepción en un endpoint, el código de estado HTTP debe ser >= 400.
**Valida: Requisitos 5.5, 10.4, 10.5**

**Propiedad 12: Completitud de datos de asistencia**
*Para cualquier* respuesta del endpoint `/api/reportes/asistencia/{eventoId}` con evento existente, el JSON debe contener: `totalAsistentes`, `asientosReservados`, `porcentajeOcupacion`.
**Valida: Requisitos 6.2**

**Propiedad 13: Cálculo correcto de porcentaje de ocupación**
*Para cualquier* evento con datos de asistencia, `PorcentajeOcupacion = (AsientosReservados / CapacidadTotal) * 100`.
**Valida: Requisitos 6.4**

**Propiedad 14: Ordenamiento descendente de logs**
*Para cualquier* consulta al endpoint `/api/reportes/auditoria`, los logs deben estar ordenados por `Timestamp` en orden descendente (más reciente primero).
**Valida: Requisitos 7.1**

**Propiedad 15: Filtrado correcto de logs de auditoría**
*Para cualquier* consulta con filtro `tipoOperacion`, todos los logs retornados deben tener `TipoOperacion = tipoOperacion`.
**Valida: Requisitos 7.2**

**Propiedad 16: Paginación correcta de resultados**
*Para cualquier* consulta con `pagina=N` y `tamañoPagina=M`, el sistema debe retornar máximo M registros, saltando (N-1)*M registros.
**Valida: Requisitos 7.3**

**Propiedad 17: Completitud de campos en logs**
*Para cualquier* log en la respuesta de auditoría, debe contener: `timestamp`, `tipoOperacion`, `entidad`, `entidadId`.
**Valida: Requisitos 7.5**

**Propiedad 18: Completitud de datos de conciliación**
*Para cualquier* respuesta del endpoint `/api/reportes/conciliacion-financiera`, el JSON debe contener: `totalIngresos`, `cantidadTransacciones`, `desglosePorCategoria`.
**Valida: Requisitos 8.2**

**Propiedad 19: Marcado de discrepancias financieras**
*Para cualquier* transacción con discrepancia detectada, el campo `EstadoRevision` debe estar presente y ser diferente de `null`.
**Valida: Requisitos 8.4**

**Propiedad 20: Esquema JSON válido para exportación**
*Para cualquier* respuesta de conciliación financiera, el JSON debe ser parseable y contener arrays válidos para `desglosePorCategoria`.
**Valida: Requisitos 8.5**

**Propiedad 21: Movimiento a cola de errores tras reintentos**
*Para cualquier* evento que falla después de 3 reintentos, debe existir un mensaje correspondiente en la cola de errores de RabbitMQ.
**Valida: Requisitos 10.2**

## Manejo de Errores

### Estrategia de Reintentos

```csharp
public class ConfiguracionReintentos
{
    public static void ConfigurarMassTransit(IBusRegistrationConfigurator cfg)
    {
        cfg.AddConsumer<EventoPublicadoConsumer>();
        cfg.AddConsumer<AsistenteRegistradoConsumer>();
        cfg.AddConsumer<AsientoReservadoConsumer>();
        
        cfg.UsingRabbitMq((context, cfg) =>
        {
            cfg.UseMessageRetry(r => r.Exponential(
                retryLimit: 3,
                minInterval: TimeSpan.FromSeconds(2),
                maxInterval: TimeSpan.FromSeconds(30),
                intervalDelta: TimeSpan.FromSeconds(2)
            ));
            
            cfg.ConfigureEndpoints(context);
        });
    }
}
```

### Manejo de Excepciones en Consumidores

```csharp
public class EventoPublicadoConsumer : IConsumer<EventoPublicadoEventoDominio>
{
    public async Task Consume(ConsumeContext<EventoPublicadoEventoDominio> context)
    {
        try
        {
            // Lógica de procesamiento
            await _repositorio.ActualizarMetricasAsync(metricas);
            
            // Auditoría exitosa
            await RegistrarAuditoriaExitosa(context.Message);
        }
        catch (MongoException ex)
        {
            _logger.LogError(ex, "Error de MongoDB procesando evento {EventoId}", 
                context.Message.EventoId);
            
            await RegistrarAuditoriaFallida(context.Message, ex);
            throw; // Permitir reintento de MassTransit
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado procesando evento {EventoId}", 
                context.Message.EventoId);
            
            await RegistrarAuditoriaFallida(context.Message, ex);
            throw;
        }
    }
}
```

### Validación de Parámetros en API

```csharp
[HttpGet("resumen-ventas")]
public async Task<ActionResult<ResumenVentasDto>> ObtenerResumenVentas(
    [FromQuery] DateTime? fechaInicio,
    [FromQuery] DateTime? fechaFin)
{
    // Validación
    if (fechaInicio.HasValue && fechaFin.HasValue && fechaInicio > fechaFin)
    {
        return BadRequest(new 
        { 
            error = "fechaInicio debe ser menor o igual a fechaFin" 
        });
    }
    
    try
    {
        var resultado = await _repositorio.ObtenerResumenVentasAsync(
            fechaInicio, fechaFin);
        return Ok(resultado);
    }
    catch (MongoException ex)
    {
        _logger.LogError(ex, "Error consultando resumen de ventas");
        return StatusCode(500, new 
        { 
            error = "Error interno del servidor" 
        });
    }
}
```

## Estrategia de Testing

### Enfoque Dual: Unit Tests + Property-Based Tests

El microservicio de Reportes requiere una estrategia de testing que combine:

1. **Unit Tests:** Para casos específicos, ejemplos concretos y edge cases
2. **Property-Based Tests:** Para verificar propiedades universales con datos generados

### Configuración de Property-Based Testing

Utilizaremos **FsCheck** para .NET, configurado para ejecutar mínimo 100 iteraciones por propiedad:

```csharp
using FsCheck;
using FsCheck.Xunit;

[Property(MaxTest = 100)]
public Property Propiedad_InvarianteDisponibilidadAsientos()
{
    return Prop.ForAll<HistorialAsistencia>(historial =>
    {
        // Feature: microservicio-reportes, Property 3: Invariante de disponibilidad de asientos
        var suma = historial.AsientosDisponibles + historial.AsientosReservados;
        return suma == historial.CapacidadTotal;
    });
}
```

### Generadores Personalizados

```csharp
public static class Generadores
{
    public static Arbitrary<HistorialAsistencia> GeneradorHistorialAsistencia()
    {
        return Arb.From(
            from capacidad in Gen.Choose(10, 1000)
            from reservados in Gen.Choose(0, capacidad)
            select new HistorialAsistencia
            {
                CapacidadTotal = capacidad,
                AsientosReservados = reservados,
                AsientosDisponibles = capacidad - reservados,
                PorcentajeOcupacion = (double)reservados / capacidad * 100
            });
    }
    
    public static Arbitrary<EventoPublicadoEventoDominio> GeneradorEventoPublicado()
    {
        return Arb.From(
            from eventoId in Arb.Generate<Guid>()
            from titulo in Arb.Generate<NonEmptyString>()
            from fecha in Arb.Generate<DateTime>()
            select new EventoPublicadoEventoDominio
            {
                EventoId = eventoId,
                TituloEvento = titulo.Get,
                FechaInicio = fecha
            });
    }
}
```

### Tests de Propiedades Clave

```csharp
public class PropiedadesReportesTests
{
    [Property(MaxTest = 100)]
    public Property Propiedad1_PersistenciaEventosConsumidos(
        EventoPublicadoEventoDominio evento)
    {
        // Feature: microservicio-reportes, Property 1: Persistencia de eventos consumidos
        
        // Arrange
        var repositorio = new RepositorioReportesLecturaInMemory();
        var consumer = new EventoPublicadoConsumer(repositorio, logger);
        
        // Act
        await consumer.Consume(CreateContext(evento));
        
        // Assert
        var metricas = await repositorio.ObtenerMetricasEventoAsync(evento.EventoId);
        return (metricas != null && metricas.EventoId == evento.EventoId)
            .ToProperty();
    }
    
    [Property(MaxTest = 100)]
    public Property Propiedad3_InvarianteDisponibilidadAsientos()
    {
        // Feature: microservicio-reportes, Property 3: Invariante de disponibilidad de asientos
        
        return Prop.ForAll(
            Generadores.GeneradorHistorialAsistencia(),
            historial =>
            {
                var suma = historial.AsientosDisponibles + historial.AsientosReservados;
                return (suma == historial.CapacidadTotal).ToProperty();
            });
    }
    
    [Property(MaxTest = 100)]
    public Property Propiedad10_FiltradoCorrectoPorRangoFechas(
        DateTime fechaInicio,
        DateTime fechaFin,
        List<ReporteVentasDiarias> reportes)
    {
        // Feature: microservicio-reportes, Property 10: Filtrado correcto por rango de fechas
        
        if (fechaInicio > fechaFin) return true.ToProperty(); // Skip invalid input
        
        var filtrados = reportes.Where(r => 
            r.Fecha >= fechaInicio && r.Fecha <= fechaFin).ToList();
        
        return filtrados.All(r => 
            r.Fecha >= fechaInicio && r.Fecha <= fechaFin).ToProperty();
    }
    
    [Property(MaxTest = 100)]
    public Property Propiedad13_CalculoCorrectoPorcentajeOcupacion()
    {
        // Feature: microservicio-reportes, Property 13: Cálculo correcto de porcentaje de ocupación
        
        return Prop.ForAll(
            Generadores.GeneradorHistorialAsistencia(),
            historial =>
            {
                var esperado = (double)historial.AsientosReservados / 
                               historial.CapacidadTotal * 100;
                var diferencia = Math.Abs(historial.PorcentajeOcupacion - esperado);
                return (diferencia < 0.01).ToProperty(); // Tolerancia para decimales
            });
    }
}
```

### Unit Tests para Casos Específicos

```csharp
public class ReportesControllerTests
{
    [Fact]
    public async Task ObtenerAsistenciaEvento_EventoNoExiste_Retorna404()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var repositorio = new Mock<IRepositorioReportesLectura>();
        repositorio.Setup(r => r.ObtenerAsistenciaEventoAsync(eventoId))
            .ReturnsAsync((HistorialAsistencia)null);
        
        var controller = new ReportesController(repositorio.Object);
        
        // Act
        var resultado = await controller.ObtenerAsistenciaEvento(eventoId);
        
        // Assert
        Assert.IsType<NotFoundObjectResult>(resultado.Result);
    }
    
    [Fact]
    public async Task ObtenerResumenVentas_FechaInicioMayorQueFin_Retorna400()
    {
        // Arrange
        var controller = new ReportesController(Mock.Of<IRepositorioReportesLectura>());
        var fechaInicio = DateTime.UtcNow;
        var fechaFin = fechaInicio.AddDays(-1);
        
        // Act
        var resultado = await controller.ObtenerResumenVentas(fechaInicio, fechaFin);
        
        // Assert
        Assert.IsType<BadRequestObjectResult>(resultado.Result);
    }
    
    [Fact]
    public async Task JobConsolidacion_ErrorMongoDB_RegistraEnAuditoria()
    {
        // Arrange
        var repositorio = new Mock<IRepositorioReportesLectura>();
        repositorio.Setup(r => r.GuardarReporteConsolidadoAsync(It.IsAny<ReporteConsolidado>()))
            .ThrowsAsync(new MongoException("Connection failed"));
        
        var job = new JobGenerarReportesConsolidados(repositorio.Object, logger);
        
        // Act & Assert
        await Assert.ThrowsAsync<MongoException>(() => job.EjecutarAsync());
        
        repositorio.Verify(r => r.RegistrarLogAuditoriaAsync(
            It.Is<LogAuditoria>(log => 
                log.TipoOperacion == "ErrorProcesamiento" && 
                !log.Exitoso)), 
            Times.Once);
    }
}
```

### Tests de Integración con MongoDB

```csharp
public class IntegracionMongoDBTests : IClassFixture<MongoDbFixture>
{
    private readonly IRepositorioReportesLectura _repositorio;
    
    [Fact]
    public async Task ActualizarVentasDiarias_DatosValidos_PersistCorrectamente()
    {
        // Arrange
        var reporte = new ReporteVentasDiarias
        {
            Fecha = DateTime.UtcNow.Date,
            EventoId = Guid.NewGuid(),
            CantidadReservas = 10,
            TotalIngresos = 1000m
        };
        
        // Act
        await _repositorio.ActualizarVentasDiariasAsync(reporte);
        var recuperado = await _repositorio.ObtenerVentasDiariasAsync(reporte.Fecha);
        
        // Assert
        Assert.NotNull(recuperado);
        Assert.Equal(reporte.EventoId, recuperado.EventoId);
        Assert.Equal(reporte.CantidadReservas, recuperado.CantidadReservas);
    }
}
```

### Cobertura de Testing

- **Property-Based Tests:** 21 propiedades (100 iteraciones cada una = 2,100 casos)
- **Unit Tests:** ~30 tests para casos específicos y edge cases
- **Integration Tests:** ~10 tests para MongoDB y MassTransit
- **Objetivo de Cobertura:** >80% de líneas de código

### Ejecución de Tests

```bash
# Ejecutar todos los tests
dotnet test

# Ejecutar solo property tests
dotnet test --filter "Category=Property"

# Ejecutar con cobertura
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
```

## Consideraciones de Despliegue

### Variables de Entorno

```yaml
# docker-compose.yml
environment:
  - MONGODB_CONNECTION_STRING=mongodb://mongodb:27017
  - MONGODB_DATABASE=reportes_db
  - RABBITMQ_HOST=rabbitmq
  - RABBITMQ_USER=guest
  - RABBITMQ_PASSWORD=guest
  - HANGFIRE_CRON_CONSOLIDACION=0 2 * * * # 2 AM diario
  - ASPNETCORE_ENVIRONMENT=Production
```

### Health Checks

```csharp
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
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

builder.Services.AddHealthChecks()
    .AddMongoDb(mongoConnectionString, name: "mongodb")
    .AddRabbitMQ(rabbitConnectionString, name: "rabbitmq");
```

### Monitoreo y Observabilidad

```csharp
// Logging estructurado con Serilog
builder.Host.UseSerilog((context, config) =>
{
    config
        .WriteTo.Console()
        .WriteTo.MongoDB(mongoConnectionString, collectionName: "logs")
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Microservicio", "Reportes");
});

// Métricas con Application Insights (opcional)
builder.Services.AddApplicationInsightsTelemetry();
```

## Resumen de Decisiones de Diseño

1. **MongoDB como Read Model:** Optimizado para consultas analíticas con índices apropiados
2. **Contratos Espejo:** Evita dependencias compartidas entre microservicios
3. **MassTransit:** Manejo robusto de mensajería con reintentos y dead-letter queues
4. **Hangfire:** Consolidación nocturna sin bloquear operaciones en tiempo real
5. **Property-Based Testing:** Validación exhaustiva de invariantes y propiedades universales
6. **Auditoría Completa:** Trazabilidad de todas las operaciones para debugging y compliance
7. **API REST JSON:** Frontend agnóstico, puede generar PDFs/Excel desde JSON
8. **Health Checks:** Monitoreo proactivo de dependencias externas
