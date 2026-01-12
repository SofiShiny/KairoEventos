# Resultados de Pruebas End-to-End (E2E)

## Información General

- **Fecha de Ejecución:** [Completar después de ejecutar]
- **Ejecutado por:** [Nombre del ejecutor]
- **Entorno:** Local Development
- **Versión:** 1.0.0

## Resumen Ejecutivo

| Métrica | Valor |
|---------|-------|
| Total de Pruebas | 4 |
| Pruebas Exitosas | [Completar] |
| Pruebas Fallidas | [Completar] |
| Tasa de Éxito | [Completar]% |
| Tiempo Total de Ejecución | [Completar] |

## Configuración del Entorno

### Servicios de Infraestructura

| Servicio | Estado | URL | Notas |
|----------|--------|-----|-------|
| RabbitMQ | ✓ / ✗ | http://localhost:15672 | Management UI (guest/guest) |
| PostgreSQL | ✓ / ✗ | localhost:5434 | Base de datos de Eventos |
| MongoDB | ✓ / ✗ | localhost:27019 | Base de datos de Reportes |

### APIs

| API | Estado | URL | Health Check |
|-----|--------|-----|--------------|
| Eventos API | ✓ / ✗ | http://localhost:5000 | /health |
| Reportes API | ✓ / ✗ | http://localhost:5002 | /health |

## Resultados Detallados

### Prueba 1: Setup del Entorno

**Objetivo:** Verificar que todos los servicios están disponibles y saludables.

**Resultado:** ✓ PASS / ✗ FAIL

**Detalles:**
- RabbitMQ: [Estado]
- PostgreSQL: [Estado]
- MongoDB: [Estado]
- API Eventos: [Estado]
- API Reportes: [Estado]

**Tiempo de Ejecución:** [X] segundos

**Notas:**
[Agregar observaciones]

---

### Prueba 2: Publicar Evento

**Objetivo:** Verificar que un evento publicado en la API de Eventos se propaga correctamente a través de RabbitMQ y se registra en el microservicio de Reportes.

**Resultado:** ✓ PASS / ✗ FAIL

**Pasos Ejecutados:**

1. **Crear Evento**
   - Request: POST /api/eventos
   - Response Status: [Código]
   - EventoId: [ID generado]
   - Tiempo: [X]ms

2. **Publicar Evento**
   - Request: PATCH /api/eventos/{id}/publicar
   - Response Status: [Código]
   - Tiempo: [X]ms

3. **Verificar RabbitMQ**
   - Cola: [Nombre de la cola]
   - Mensajes procesados: [Cantidad]
   - Estado: [Descripción]

4. **Verificar en Reportes**
   - Request: GET /api/reportes/metricas-evento/{id}
   - Response Status: [Código]
   - MetricasEvento encontrado: ✓ / ✗
   - Datos verificados:
     - EventoId: [Valor]
     - TituloEvento: [Valor]
     - Estado: [Valor]
     - FechaPublicacion: [Valor]

**Tiempo Total:** [X] segundos

**Capturas de Pantalla:**
- [Agregar captura de RabbitMQ Management UI]
- [Agregar captura de respuesta de Reportes API]

**Logs Relevantes:**
```
[Agregar logs de Eventos API]
[Agregar logs de Reportes API]
```

**Notas:**
[Agregar observaciones específicas]

---

### Prueba 3: Registrar Asistente

**Objetivo:** Verificar que el registro de un asistente se propaga correctamente y actualiza las métricas en Reportes.

**Resultado:** ✓ PASS / ✗ FAIL

**Pasos Ejecutados:**

1. **Registrar Asistente**
   - Request: POST /api/eventos/{id}/asistentes
   - EventoId: [ID del evento de la prueba anterior]
   - Response Status: [Código]
   - Tiempo: [X]ms

2. **Verificar RabbitMQ**
   - Cola: [Nombre de la cola]
   - Mensajes procesados: [Cantidad]
   - Estado: [Descripción]

3. **Verificar Actualización en Reportes**
   - Request: GET /api/reportes/metricas-evento/{id}
   - Response Status: [Código]
   - Datos verificados:
     - TotalAsistentes: [Valor] (debe ser > 0)
     - UltimaActualizacion: [Timestamp actualizado]

4. **Verificar HistorialAsistencia**
   - Request: GET /api/reportes/asistencia/{id}
   - Response Status: [Código]
   - Datos verificados:
     - TotalAsistentes: [Valor]
     - AsientosReservados: [Valor]
     - PorcentajeOcupacion: [Valor]%

**Tiempo Total:** [X] segundos

**Capturas de Pantalla:**
- [Agregar captura de métricas actualizadas]

**Logs Relevantes:**
```
[Agregar logs relevantes]
```

**Notas:**
[Agregar observaciones específicas]

---

### Prueba 4: Cancelar Evento

**Objetivo:** Verificar que la cancelación de un evento se propaga correctamente y se registra en auditoría.

**Resultado:** ✓ PASS / ✗ FAIL

**Pasos Ejecutados:**

1. **Cancelar Evento**
   - Request: PATCH /api/eventos/{id}/cancelar
   - EventoId: [ID del evento de las pruebas anteriores]
   - Response Status: [Código]
   - Tiempo: [X]ms

2. **Verificar RabbitMQ**
   - Cola: [Nombre de la cola]
   - Mensajes procesados: [Cantidad]
   - Estado: [Descripción]

3. **Verificar Estado en Reportes**
   - Request: GET /api/reportes/metricas-evento/{id}
   - Response Status: [Código]
   - Datos verificados:
     - Estado: "Cancelado" ✓ / ✗
     - UltimaActualizacion: [Timestamp actualizado]

4. **Verificar LogAuditoria**
   - Request: GET /api/reportes/logs-auditoria?eventoId={id}
   - Response Status: [Código]
   - Logs encontrados: [Cantidad]
   - Datos verificados:
     - TipoOperacion: "EventoCancelado"
     - Exitoso: true
     - Detalles: [Descripción]

**Tiempo Total:** [X] segundos

**Capturas de Pantalla:**
- [Agregar captura de estado cancelado]
- [Agregar captura de logs de auditoría]

**Logs Relevantes:**
```
[Agregar logs relevantes]
```

**Notas:**
[Agregar observaciones específicas]

---

## Análisis de Rendimiento

### Tiempos de Procesamiento

| Operación | Tiempo Promedio | Tiempo Máximo | Notas |
|-----------|-----------------|---------------|-------|
| Crear Evento | [X]ms | [X]ms | |
| Publicar Evento | [X]ms | [X]ms | |
| Propagación a RabbitMQ | [X]ms | [X]ms | |
| Consumo en Reportes | [X]ms | [X]ms | |
| Registrar Asistente | [X]ms | [X]ms | |
| Cancelar Evento | [X]ms | [X]ms | |

### Latencia End-to-End

| Flujo | Latencia Total | Objetivo | Estado |
|-------|----------------|----------|--------|
| Publicar → Reportes | [X]ms | < 5000ms | ✓ / ✗ |
| Registrar → Reportes | [X]ms | < 5000ms | ✓ / ✗ |
| Cancelar → Reportes | [X]ms | < 5000ms | ✓ / ✗ |

## Problemas Encontrados

### Problema 1: [Título]

**Severidad:** Alta / Media / Baja

**Descripción:**
[Descripción detallada del problema]

**Pasos para Reproducir:**
1. [Paso 1]
2. [Paso 2]
3. [Paso 3]

**Comportamiento Esperado:**
[Descripción]

**Comportamiento Actual:**
[Descripción]

**Logs/Errores:**
```
[Agregar logs o mensajes de error]
```

**Solución Propuesta:**
[Descripción de la solución]

**Estado:** Abierto / En Progreso / Resuelto

---

## Verificación de Requisitos

### Requirement 2.1: Comunicación entre Eventos y Reportes

**Estado:** ✓ Cumplido / ✗ No Cumplido / ⚠ Parcialmente Cumplido

**Evidencia:**
- [Descripción de la evidencia]

### Requirement 2.2: EventoPublicadoEventoDominio → MetricasEvento

**Estado:** ✓ Cumplido / ✗ No Cumplido / ⚠ Parcialmente Cumplido

**Evidencia:**
- [Descripción de la evidencia]

### Requirement 2.3: AsistenteRegistradoEventoDominio → HistorialAsistencia

**Estado:** ✓ Cumplido / ✗ No Cumplido / ⚠ Parcialmente Cumplido

**Evidencia:**
- [Descripción de la evidencia]

### Requirement 2.4: EventoCanceladoEventoDominio → Estado Actualizado

**Estado:** ✓ Cumplido / ✗ No Cumplido / ⚠ Parcialmente Cumplido

**Evidencia:**
- [Descripción de la evidencia]

### Requirement 2.5: Consulta API Reportes

**Estado:** ✓ Cumplido / ✗ No Cumplido / ⚠ Parcialmente Cumplido

**Evidencia:**
- [Descripción de la evidencia]

## Conclusiones

### Resumen

[Resumen general de los resultados de las pruebas E2E]

### Puntos Positivos

- [Punto 1]
- [Punto 2]
- [Punto 3]

### Áreas de Mejora

- [Área 1]
- [Área 2]
- [Área 3]

### Recomendaciones

1. [Recomendación 1]
2. [Recomendación 2]
3. [Recomendación 3]

### Próximos Pasos

- [ ] [Acción 1]
- [ ] [Acción 2]
- [ ] [Acción 3]

## Anexos

### Anexo A: Configuración de Variables de Entorno

```bash
# Eventos API
POSTGRES_HOST=localhost
POSTGRES_PORT=5434
POSTGRES_DB=eventsdb
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
RabbitMq__Host=localhost
RabbitMq__Username=guest
RabbitMq__Password=guest
ASPNETCORE_URLS=http://localhost:5000

# Reportes API
MONGODB_CONNECTION_STRING=mongodb://localhost:27019
MONGODB_DATABASE=reportes_db
RABBITMQ_HOST=localhost
RABBITMQ_PORT=5672
RABBITMQ_USER=guest
RABBITMQ_PASSWORD=guest
ASPNETCORE_URLS=http://localhost:5002
```

### Anexo B: Comandos Ejecutados

```powershell
# Setup del entorno
.\setup-e2e-environment.ps1

# Ejecutar pruebas
.\run-e2e-tests.ps1

# Detener entorno
.\stop-e2e-environment.ps1
```

### Anexo C: Estructura de Mensajes RabbitMQ

#### EventoPublicadoEventoDominio
```json
{
  "eventoId": "guid",
  "tituloEvento": "string",
  "fechaInicio": "datetime"
}
```

#### AsistenteRegistradoEventoDominio
```json
{
  "eventoId": "guid",
  "usuarioId": "guid",
  "nombreUsuario": "string"
}
```

#### EventoCanceladoEventoDominio
```json
{
  "eventoId": "guid",
  "tituloEvento": "string"
}
```

---

**Documento generado:** [Fecha]
**Última actualización:** [Fecha]
