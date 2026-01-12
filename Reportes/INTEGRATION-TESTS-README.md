# Guía de Tests de Integración End-to-End

## Descripción

Esta guía describe cómo ejecutar los tests de integración end-to-end para el microservicio de Reportes. Los tests validan el flujo completo desde la publicación de eventos hasta la consulta de datos a través de la API.

## Requisitos Previos

- Docker y Docker Compose instalados
- .NET 8 SDK instalado
- PowerShell (Windows) o Bash (Linux/Mac)

## Arquitectura de Testing

```
┌─────────────────────────────────────────────────────────────┐
│                  Test Event Publisher                        │
│              (test-event-publisher/)                         │
│  Publica eventos de prueba a RabbitMQ                       │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                      RabbitMQ                                │
│  Enruta eventos a los consumidores                          │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                   Consumidores                               │
│  EventoPublicadoConsumer                                     │
│  AsistenteRegistradoConsumer                                 │
│  AsientoReservadoConsumer                                    │
│  AsientoLiberadoConsumer                                     │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                      MongoDB                                 │
│  Almacena modelos de lectura                                │
│  - metricas_evento                                           │
│  - historial_asistencia                                      │
│  - reportes_ventas_diarias                                   │
│  - logs_auditoria                                            │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                    API REST                                  │
│  Expone endpoints para consultar reportes                   │
│  GET /api/reportes/resumen-ventas                           │
│  GET /api/reportes/asistencia/{eventoId}                    │
│  GET /api/reportes/auditoria                                │
└─────────────────────────────────────────────────────────────┘
```

## Paso 1: Iniciar Servicios

### Windows (PowerShell)
```powershell
cd Reportes
docker-compose up -d
```

### Linux/Mac (Bash)
```bash
cd Reportes
docker-compose up -d
```

Espera unos segundos para que todos los servicios estén listos. Puedes verificar el estado con:
```bash
docker-compose ps
```

## Paso 2: Ejecutar Tests de Integración

### Opción A: Script Completo (Recomendado)

Este script ejecuta todos los pasos automáticamente:

#### Windows (PowerShell)
```powershell
.\run-integration-test.ps1
```

#### Linux/Mac (Bash)
```bash
chmod +x run-integration-test.sh
./run-integration-test.sh
```

El script realiza:
1. ✅ Verifica que todos los servicios estén corriendo
2. ✅ Limpia datos de pruebas anteriores
3. ✅ Compila el publicador de eventos
4. ✅ Publica eventos de prueba a RabbitMQ
5. ✅ Verifica que los datos se persistan en MongoDB
6. ✅ Prueba los endpoints de la API
7. ✅ Muestra un resumen de resultados

### Opción B: Pasos Manuales

Si prefieres ejecutar los pasos manualmente:

#### 1. Compilar el publicador de eventos
```bash
cd test-event-publisher
dotnet build --configuration Release
```

#### 2. Publicar eventos de prueba
```bash
dotnet run --configuration Release --no-build
```

#### 3. Verificar datos en MongoDB
```bash
# Métricas de eventos
docker exec reportes-mongodb mongosh reportes_db --eval "db.metricas_evento.find().pretty()"

# Historial de asistencia
docker exec reportes-mongodb mongosh reportes_db --eval "db.historial_asistencia.find().pretty()"

# Reportes de ventas
docker exec reportes-mongodb mongosh reportes_db --eval "db.reportes_ventas_diarias.find().pretty()"

# Logs de auditoría
docker exec reportes-mongodb mongosh reportes_db --eval "db.logs_auditoria.find().pretty()"
```

#### 4. Probar endpoints de API
```bash
# Resumen de ventas
curl http://localhost:5002/api/reportes/resumen-ventas

# Asistencia de un evento (reemplaza {eventoId} con un ID real)
curl http://localhost:5002/api/reportes/asistencia/{eventoId}

# Logs de auditoría
curl "http://localhost:5002/api/reportes/auditoria?pagina=1&tamañoPagina=10"

# Conciliación financiera
curl http://localhost:5002/api/reportes/conciliacion-financiera
```

## Paso 3: Interpretar Resultados

### Salida Exitosa

Un test exitoso mostrará:

```
=== Resumen de Prueba de Integración ===

Servicios verificados:
  ✓ MongoDB
  ✓ RabbitMQ
  ✓ API de Reportes

Datos persistidos:
  • Métricas de eventos: 2
  • Historial de asistencia: 1
  • Reportes de ventas: 1
  • Logs de auditoría: 10+

✅ Prueba de integración completada exitosamente
```

### Problemas Comunes

#### Error: "MongoDB no responde"
**Causa**: MongoDB no ha terminado de iniciar  
**Solución**: Espera 10-15 segundos más y vuelve a intentar

#### Error: "RabbitMQ no responde"
**Causa**: RabbitMQ no ha terminado de iniciar  
**Solución**: Espera 10-15 segundos más y vuelve a intentar

#### Error: "API de Reportes no responde"
**Causa**: La API no ha terminado de iniciar o hay un error  
**Solución**: Verifica los logs:
```bash
docker logs reportes-api --tail 50
```

#### Error: "No se encontraron datos en MongoDB"
**Causa**: Los consumidores no procesaron los eventos  
**Solución**: 
1. Verifica que RabbitMQ esté corriendo
2. Verifica los logs de la API para ver si hay errores en los consumidores
3. Verifica que los eventos se publicaron correctamente

## Paso 4: Limpiar Datos de Prueba

Para limpiar los datos de prueba y empezar de nuevo:

```bash
# Limpiar todas las colecciones
docker exec reportes-mongodb mongosh reportes_db --eval "db.metricas_evento.deleteMany({})"
docker exec reportes-mongodb mongosh reportes_db --eval "db.historial_asistencia.deleteMany({})"
docker exec reportes-mongodb mongosh reportes_db --eval "db.reportes_ventas_diarias.deleteMany({})"
docker exec reportes-mongodb mongosh reportes_db --eval "db.logs_auditoria.deleteMany({})"
```

O simplemente ejecuta el script de integración de nuevo (limpia automáticamente).

## Escenarios de Prueba Implementados

### Escenario 1: Publicar Evento
- **Evento**: `EventoPublicadoEventoDominio`
- **Validación**: Métricas del evento se crean en MongoDB
- **Endpoint**: `GET /api/reportes/resumen-ventas`

### Escenario 2: Crear Mapa de Asientos
- **Evento**: `MapaAsientosCreadoEventoDominio`
- **Validación**: Historial de asistencia se inicializa
- **Endpoint**: `GET /api/reportes/asistencia/{eventoId}`

### Escenario 3: Registrar Asistentes
- **Evento**: `AsistenteRegistradoEventoDominio` (múltiples)
- **Validación**: Contador de asistentes se incrementa
- **Endpoint**: `GET /api/reportes/asistencia/{eventoId}`

### Escenario 4: Reservar Asientos
- **Evento**: `AsientoReservadoEventoDominio` (múltiples)
- **Validación**: 
  - Reportes de ventas se actualizan
  - Asientos reservados se incrementan
  - Asientos disponibles se decrementan
  - Porcentaje de ocupación se recalcula
- **Endpoint**: `GET /api/reportes/resumen-ventas`

### Escenario 5: Liberar Asiento
- **Evento**: `AsientoLiberadoEventoDominio`
- **Validación**: 
  - Asientos disponibles se incrementan
  - Invariante se mantiene: `AsientosDisponibles + AsientosReservados = CapacidadTotal`
- **Endpoint**: `GET /api/reportes/asistencia/{eventoId}`

### Escenario 6: Múltiples Eventos
- **Eventos**: Varios eventos publicados
- **Validación**: Sistema maneja múltiples eventos correctamente
- **Endpoint**: `GET /api/reportes/resumen-ventas`

## Requisitos Validados

Los tests de integración validan los siguientes requisitos:

- ✅ **Requisito 1.1**: Consumo de EventoPublicadoEventoDominio
- ✅ **Requisito 1.2**: Consumo de AsistenteRegistradoEventoDominio
- ✅ **Requisito 1.3**: Consumo de AsientoReservadoEventoDominio
- ✅ **Requisito 1.4**: Consumo de AsientoLiberadoEventoDominio
- ✅ **Requisito 1.5**: Registro en auditoría
- ✅ **Requisito 3.2**: Persistencia en MongoDB
- ✅ **Requisito 5.1-5.5**: Endpoints de resumen de ventas
- ✅ **Requisito 6.1-6.5**: Endpoints de asistencia
- ✅ **Requisito 7.1-7.5**: Endpoints de auditoría

## Próximos Pasos

Después de ejecutar los tests de integración exitosamente:

1. **Ejecutar tests unitarios**: `dotnet test` en el proyecto de pruebas
2. **Ejecutar tests de propiedades**: Verificar que todos los property tests pasen
3. **Verificar cobertura de código**: Generar reporte de cobertura
4. **Ejecutar job de consolidación**: Probar el job de Hangfire manualmente

## Soporte

Si encuentras problemas:

1. Verifica los logs de los servicios:
   ```bash
   docker logs reportes-api
   docker logs reportes-mongodb
   docker logs reportes-rabbitmq
   ```

2. Verifica el estado de los servicios:
   ```bash
   docker-compose ps
   ```

3. Reinicia los servicios si es necesario:
   ```bash
   docker-compose down
   docker-compose up -d
   ```

4. Consulta la documentación en `Reportes/backend/src/Services/Reportes/Reportes.Pruebas/Integracion/IntegrationTestsDocumentation.md`
