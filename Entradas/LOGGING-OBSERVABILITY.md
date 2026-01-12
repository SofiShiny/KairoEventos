# Logging y Observabilidad - Microservicio Entradas.API

## Overview

El microservicio Entradas.API implementa un sistema comprehensivo de logging estructurado, métricas de performance y health checks para garantizar observabilidad completa en entornos distribuidos.

## Structured Logging

### Configuración Serilog

- **Sinks configurados**: Console y File con rotación diaria
- **Enrichers**: CorrelationId, MachineName, ThreadId, EnvironmentName
- **Formato estructurado**: JSON con timestamps, correlation IDs y contexto
- **Niveles por ambiente**:
  - Development: Debug
  - Staging: Information
  - Production: Information

### Correlation IDs

- **Header**: `X-Correlation-ID`
- **Propagación**: Automática a través de middleware
- **Tracing distribuido**: Integrado con ActivitySource
- **Logging contextual**: Incluido en todos los logs

### Logging por Capas

#### API Layer
- Request/Response logging con Serilog
- Métricas de performance por endpoint
- Manejo de errores centralizado

#### Application Layer
- Logging detallado en handlers
- Métricas de duración de operaciones
- Tracking de validaciones externas

#### Infrastructure Layer
- Logging de llamadas a servicios externos
- Circuit breaker events
- Retry policies con contexto

## Health Checks

### Endpoints Disponibles

| Endpoint | Descripción | Uso |
|----------|-------------|-----|
| `/health` | Health check general | Load balancers |
| `/health/detailed` | Información detallada | Monitoring |
| `/health/database` | Solo base de datos | DB monitoring |
| `/health/messaging` | Solo RabbitMQ | Message queue monitoring |
| `/health/external` | Servicios externos | Dependency monitoring |
| `/health/live` | Liveness probe | Kubernetes |
| `/health/ready` | Readiness probe | Kubernetes |

### Health Checks Implementados

1. **Self Check**: Verifica que la API está funcionando
2. **PostgreSQL**: Conectividad y disponibilidad de BD
3. **RabbitMQ**: Conectividad con message broker
4. **Servicio Eventos**: Disponibilidad del servicio externo
5. **Servicio Asientos**: Disponibilidad del servicio externo
6. **Business Logic**: Funcionalidad del servicio de entradas

### Estados de Health

- **Healthy**: Todo funcionando correctamente
- **Degraded**: Servicios externos no disponibles pero funcionalidad core OK
- **Unhealthy**: Problemas críticos que impiden el funcionamiento

## Métricas y Telemetría

### Métricas Implementadas

#### Contadores
- `entradas_creadas_total`: Número de entradas creadas por evento/estado
- `validacion_externa_errores_total`: Errores en validaciones externas
- `pagos_confirmados_total`: Pagos procesados por resultado

#### Histogramas
- `entrada_creacion_duration_ms`: Duración de creación de entradas
- `servicio_externo_duration_ms`: Duración de llamadas externas
- `health_check_duration_ms`: Duración de health checks

### Distributed Tracing

- **ActivitySource**: `Entradas.API`
- **Spans principales**:
  - `CrearEntrada`: Proceso completo de creación
  - `ProcesarPagoConfirmado`: Procesamiento de pagos
  - HTTP requests con contexto completo

### Tags y Contexto

- **Correlation IDs**: Propagación automática
- **Business Context**: EventoId, UsuarioId, EntradaId
- **Technical Context**: StatusCode, Duration, Resultado
- **Error Context**: Exception type, message, stack trace

## Configuración por Ambiente

### Development
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug"
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {CorrelationId} {Level:u3}] {SourceContext} {Message:lj}"
        }
      }
    ]
  }
}
```

### Production
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information"
    },
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "logs/entradas-api-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      }
    ]
  }
}
```

## Monitoreo y Alertas

### Métricas Clave para Alertas

1. **Error Rate**: `validacion_externa_errores_total` / `entradas_creadas_total`
2. **Response Time**: P95 de `entrada_creacion_duration_ms`
3. **Health Status**: Estado de health checks críticos
4. **External Dependencies**: Disponibilidad de servicios externos

### Dashboards Recomendados

#### Business Metrics
- Entradas creadas por hora/día
- Distribución por eventos
- Tasa de conversión (creadas vs pagadas)

#### Technical Metrics
- Response times por endpoint
- Error rates por tipo
- Health check status
- External service availability

#### Infrastructure Metrics
- CPU/Memory usage
- Database connection pool
- RabbitMQ queue depth

## Troubleshooting

### Logs Estructurados

Todos los logs incluyen contexto estructurado para facilitar búsquedas:

```json
{
  "timestamp": "2024-01-15T10:30:00.123Z",
  "level": "Information",
  "correlationId": "abc123-def456",
  "sourceContext": "Entradas.Aplicacion.Handlers.CrearEntradaCommandHandler",
  "message": "Entrada {EntradaId} persistida exitosamente",
  "properties": {
    "EntradaId": "550e8400-e29b-41d4-a716-446655440000",
    "EventoId": "550e8400-e29b-41d4-a716-446655440001",
    "UsuarioId": "550e8400-e29b-41d4-a716-446655440002"
  }
}
```

### Queries Útiles

#### Buscar por Correlation ID
```
CorrelationId:"abc123-def456"
```

#### Errores en creación de entradas
```
SourceContext:"CrearEntradaCommandHandler" AND Level:"Error"
```

#### Performance issues
```
Message:"responded" AND Elapsed > 1000
```

## Integración con Herramientas

### Prometheus/Grafana
- Métricas exportadas vía OpenTelemetry
- Dashboards pre-configurados disponibles
- Alertas basadas en SLIs/SLOs

### ELK Stack
- Logs estructurados compatibles con Elasticsearch
- Kibana dashboards para análisis
- Alertas basadas en patrones de logs

### Application Performance Monitoring
- Distributed tracing compatible con Jaeger/Zipkin
- Correlation entre logs, métricas y traces
- End-to-end visibility

## Mejores Prácticas

### Logging
1. Usar structured logging siempre
2. Incluir correlation IDs en todas las operaciones
3. Log de entrada y salida en boundaries
4. Contexto de negocio en todos los logs críticos

### Métricas
1. Métricas de negocio y técnicas
2. Histogramas para latencias
3. Contadores para eventos
4. Labels consistentes

### Health Checks
1. Checks rápidos (<10s timeout)
2. Diferentes niveles de criticidad
3. Información diagnóstica útil
4. Estados apropiados (Healthy/Degraded/Unhealthy)

### Alertas
1. Basadas en SLIs de negocio
2. Escalación apropiada
3. Runbooks documentados
4. Reducción de ruido