# Guía Rápida: Pruebas End-to-End

## Requisitos Previos

- Docker Desktop instalado y corriendo
- .NET 8 SDK instalado
- PowerShell 5.1 o superior
- Puertos disponibles: 5000, 5002, 5434, 5672, 15672, 27019

## Pasos para Ejecutar Pruebas E2E

### 1. Configurar el Entorno Completo

```powershell
cd Eventos
.\setup-e2e-environment.ps1
```

Este script:
- ✓ Levanta RabbitMQ (puerto 5672, Management UI en 15672)
- ✓ Levanta PostgreSQL para Eventos (puerto 5434)
- ✓ Levanta MongoDB para Reportes (puerto 27019)
- ✓ Aplica migraciones de base de datos
- ✓ Inicia API de Eventos (puerto 5000)
- ✓ Inicia API de Reportes (puerto 5002)
- ✓ Verifica health de todos los servicios

**Tiempo estimado:** 2-3 minutos

### 2. Ejecutar las Pruebas E2E

```powershell
.\run-e2e-tests.ps1
```

Este script ejecuta automáticamente:
1. ✓ Verificación del entorno
2. ✓ Prueba: Publicar Evento
3. ✓ Prueba: Registrar Asistente
4. ✓ Prueba: Cancelar Evento
5. ✓ Generación de reporte de resultados

**Tiempo estimado:** 30-45 segundos

### 3. Revisar Resultados

El script mostrará un resumen en consola:

```
========================================
Resumen de Pruebas E2E
========================================

Resultados:
  Setup Entorno:        ✓ PASS
  Publicar Evento:      ✓ PASS
  Registrar Asistente:  ✓ PASS
  Cancelar Evento:      ✓ PASS

Total: 4/4 pruebas pasadas (100%)

¡Todas las pruebas E2E pasaron exitosamente! ✓
```

### 4. Documentar Resultados

Completa el archivo `E2E-TEST-RESULTS.md` con:
- Capturas de pantalla de RabbitMQ Management UI
- Logs relevantes de ambas APIs
- Tiempos de procesamiento observados
- Problemas encontrados (si los hay)

### 5. Detener el Entorno

```powershell
.\stop-e2e-environment.ps1
```

Cierra manualmente las ventanas de PowerShell de las APIs.

## Verificación Manual (Opcional)

### Verificar RabbitMQ Management UI

1. Abrir: http://localhost:15672
2. Login: guest / guest
3. Ir a "Queues" tab
4. Verificar colas creadas y mensajes procesados

### Verificar APIs con Swagger

**API Eventos:**
- URL: http://localhost:5000/swagger
- Probar endpoints manualmente

**API Reportes:**
- URL: http://localhost:5002/swagger
- Consultar métricas y logs de auditoría

### Consultas Directas a Bases de Datos

**PostgreSQL (Eventos):**
```bash
docker exec -it eventos-postgres psql -U postgres -d eventsdb
```

```sql
SELECT * FROM "Eventos" ORDER BY "FechaCreacion" DESC LIMIT 5;
SELECT * FROM "Asistentes" ORDER BY "FechaRegistro" DESC LIMIT 5;
```

**MongoDB (Reportes):**
```bash
docker exec -it reportes-mongodb mongosh
```

```javascript
use reportes_db
db.metricas_evento.find().sort({UltimaActualizacion: -1}).limit(5)
db.historial_asistencia.find().sort({UltimaActualizacion: -1}).limit(5)
db.logs_auditoria.find().sort({Timestamp: -1}).limit(10)
```

## Troubleshooting

### Problema: Docker no está corriendo

**Solución:** Inicia Docker Desktop y espera a que esté completamente iniciado.

### Problema: Puerto ya en uso

**Solución:** 
```powershell
# Ver qué proceso está usando el puerto
netstat -ano | findstr :5000

# Detener el proceso (reemplaza PID con el número del proceso)
taskkill /PID <PID> /F
```

### Problema: API no responde en /health

**Solución:**
1. Verificar logs en la ventana de PowerShell de la API
2. Verificar que las variables de entorno están configuradas
3. Verificar que las bases de datos están disponibles
4. Reintentar después de 10-15 segundos

### Problema: Mensajes no se consumen en RabbitMQ

**Solución:**
1. Verificar que ambas APIs están corriendo
2. Verificar logs de la API de Reportes
3. Verificar en RabbitMQ Management UI que las colas existen
4. Verificar que no hay errores de deserialización en logs

### Problema: Pruebas fallan intermitentemente

**Solución:**
1. Aumentar tiempo de espera en el script (cambiar `Wait-ForProcessing -Seconds 5` a `-Seconds 10`)
2. Verificar que el sistema no está bajo carga alta
3. Verificar conexión de red estable

## Comandos Útiles

### Ver logs de contenedores Docker

```powershell
# RabbitMQ
docker logs eventos-rabbitmq -f

# PostgreSQL
docker logs eventos-postgres -f

# MongoDB
docker logs reportes-mongodb -f
```

### Reiniciar un servicio específico

```powershell
# Reiniciar RabbitMQ
docker restart eventos-rabbitmq

# Reiniciar PostgreSQL
docker restart eventos-postgres

# Reiniciar MongoDB
docker restart reportes-mongodb
```

### Limpiar todo y empezar de cero

```powershell
# Detener y eliminar contenedores
cd Eventos
docker-compose down -v

cd ..\Reportes
docker-compose down -v

# Volver a configurar
cd ..\Eventos
.\setup-e2e-environment.ps1
```

## Métricas de Éxito

Una ejecución exitosa debe cumplir:

- ✓ Todos los servicios de infraestructura están healthy
- ✓ Ambas APIs responden en /health
- ✓ Evento se crea y publica correctamente
- ✓ Mensaje aparece en RabbitMQ y se consume
- ✓ MetricasEvento se crea en MongoDB
- ✓ Asistente se registra correctamente
- ✓ Métricas se actualizan en MongoDB
- ✓ Evento se cancela correctamente
- ✓ Estado se actualiza en MongoDB
- ✓ LogAuditoria se registra correctamente
- ✓ Latencia end-to-end < 5 segundos por operación

## Contacto y Soporte

Si encuentras problemas:
1. Revisa los logs de las APIs
2. Revisa los logs de Docker
3. Consulta la documentación en `E2E-TEST-RESULTS.md`
4. Reporta el problema con logs completos

---

**Última actualización:** 2024-12-29
