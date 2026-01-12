# Tests de Integración End-to-End

## Resumen

Los tests de integración end-to-end para el microservicio de Reportes validan el flujo completo:
**Evento → Consumidor → MongoDB → API**

## Requisitos Validados

- **Requisitos 1.1-1.5**: Consumo de eventos de dominio
- **Requisitos 4.1-4.3**: Generación de reportes consolidados
- **Requisitos 10.1-10.5**: Manejo de errores y resiliencia

## Estrategia de Testing

### 1. Tests Automatizados con Scripts

Los scripts de integración (`run-integration-test.ps1` y `run-integration-test.sh`) proporcionan:

- **Publicación de eventos de prueba**: Usa el proyecto `test-event-publisher` para publicar eventos reales a RabbitMQ
- **Verificación de consumidores**: Confirma que los consumidores procesan eventos correctamente
- **Verificación de persistencia**: Valida que los datos se guardan en MongoDB
- **Verificación de API**: Prueba que los endpoints retornan datos esperados

### 2. Flujos de Prueba Implementados

#### Flujo 1: Evento Publicado
```
EventoPublicadoEventoDominio → EventoPublicadoConsumer → MongoDB (metricas_evento) → GET /api/reportes/resumen-ventas
```

**Validación**:
- Métricas del evento se crean en MongoDB
- Log de auditoría registra la operación
- API retorna datos del evento

#### Flujo 2: Asistente Registrado
```
AsistenteRegistradoEventoDominio → AsistenteRegistradoConsumer → MongoDB (historial_asistencia) → GET /api/reportes/asistencia/{eventoId}
```

**Validación**:
- Contador de asistentes se incrementa
- Asistente se agrega a la lista
- API retorna datos de asistencia actualizados

#### Flujo 3: Asiento Reservado
```
AsientoReservadoEventoDominio → AsientoReservadoConsumer → MongoDB (reportes_ventas_diarias, historial_asistencia) → GET /api/reportes/resumen-ventas
```

**Validación**:
- Reporte de ventas se actualiza
- Contador de asientos reservados se incrementa
- Asientos disponibles se decrementan
- Porcentaje de ocupación se recalcula

#### Flujo 4: Asiento Liberado
```
AsientoLiberadoEventoDominio → AsientoLiberadoConsumer → MongoDB (historial_asistencia) → GET /api/reportes/asistencia/{eventoId}
```

**Validación**:
- Asientos disponibles se incrementan
- Asientos reservados se decrementan
- Invariante se mantiene: `AsientosDisponibles + AsientosReservados = CapacidadTotal`

#### Flujo 5: Consolidación Nocturna
```
Hangfire Job → JobGenerarReportesConsolidados → MongoDB (reportes_consolidados) → GET /api/reportes/conciliacion-financiera
```

**Validación**:
- Reporte consolidado se genera con datos del día anterior
- Totales se calculan correctamente
- Log de auditoría registra la operación

### 3. Ejecución de Tests de Integración

#### Windows (PowerShell)
```powershell
# Iniciar servicios
cd Reportes
docker-compose up -d

# Ejecutar tests de integración
.\run-integration-test.ps1

# Ver resultados
docker exec reportes-mongodb mongosh reportes_db --eval "db.metricas_evento.find().pretty()"
docker exec reportes-mongodb mongosh reportes_db --eval "db.historial_asistencia.find().pretty()"
```

#### Linux/Mac (Bash)
```bash
# Iniciar servicios
cd Reportes
docker-compose up -d

# Ejecutar tests de integración
chmod +x run-integration-test.sh
./run-integration-test.sh

# Ver resultados
docker exec reportes-mongodb mongosh reportes_db --eval "db.metricas_evento.find().pretty()"
docker exec reportes-mongodb mongosh reportes_db --eval "db.historial_asistencia.find().pretty()"
```

### 4. Verificación Manual de Endpoints

Después de ejecutar los scripts de integración, puedes verificar manualmente los endpoints:

```bash
# Resumen de ventas
curl http://localhost:5002/api/reportes/resumen-ventas

# Asistencia de un evento específico
curl http://localhost:5002/api/reportes/asistencia/{eventoId}

# Logs de auditoría
curl "http://localhost:5002/api/reportes/auditoria?pagina=1&tamañoPagina=10"

# Conciliación financiera
curl http://localhost:5002/api/reportes/conciliacion-financiera
```

### 5. Tests de Manejo de Errores

Los scripts de integración también validan:

- **Resiliencia ante MongoDB no disponible**: El sistema debe encolar operaciones
- **Reintentos con backoff exponencial**: Hasta 3 intentos antes de mover a cola de errores
- **Logging de errores**: Todos los errores se registran en auditoría
- **Recuperación automática**: El sistema se recupera cuando los servicios vuelven

### 6. Métricas de Éxito

Un test de integración exitoso debe mostrar:

✅ Todos los servicios (MongoDB, RabbitMQ, API) están corriendo  
✅ Eventos se publican correctamente a RabbitMQ  
✅ Consumidores procesan eventos sin errores  
✅ Datos se persisten en MongoDB  
✅ API retorna datos correctos  
✅ Logs de auditoría registran todas las operaciones  

### 7. Troubleshooting

#### Problema: Consumidores no procesan eventos
**Solución**: Verificar que RabbitMQ esté corriendo y que los consumidores estén registrados:
```bash
docker logs reportes-api
```

#### Problema: Datos no aparecen en MongoDB
**Solución**: Verificar conexión a MongoDB y permisos:
```bash
docker exec reportes-mongodb mongosh reportes_db --eval "db.getCollectionNames()"
```

#### Problema: API retorna 500
**Solución**: Verificar logs de la API:
```bash
docker logs reportes-api --tail 100
```

## Conclusión

Los tests de integración end-to-end están implementados mediante:

1. **Scripts automatizados** (`run-integration-test.ps1` / `.sh`) que publican eventos y verifican resultados
2. **Proyecto de publicación de eventos** (`test-event-publisher`) que simula eventos reales
3. **Verificación manual** de endpoints y datos en MongoDB

Esta estrategia proporciona cobertura completa del flujo end-to-end sin requerir tests unitarios complejos que dependan de infraestructura externa.
