# Guía Rápida: Pruebas de Resiliencia

## Descripción

Esta guía proporciona instrucciones rápidas para ejecutar las pruebas de resiliencia de la integración RabbitMQ.

---

## Prerequisitos

1. Docker Desktop instalado y corriendo
2. PowerShell 5.1 o superior
3. Servicios levantados con docker-compose

---

## Levantar Servicios

```powershell
# Navegar al directorio de Eventos
cd Eventos

# Levantar todos los servicios
docker-compose up -d

# Verificar que los servicios están corriendo
docker ps
```

Deberías ver:
- `eventos-api`
- `eventos-rabbitmq`
- `eventos-postgres`

---

## Prueba 1: Reconexión Automática

### Ejecutar

```powershell
./test-reconnection.ps1
```

### Qué Hace

1. Verifica que todos los servicios están corriendo
2. Crea y publica un evento con RabbitMQ activo (baseline)
3. Detiene RabbitMQ
4. Intenta publicar un evento (debería manejar el error gracefully)
5. Reinicia RabbitMQ
6. Espera reconexión automática
7. Publica otro evento (debería funcionar)

### Tiempo Estimado

~2 minutos

### Resultado Esperado

```
OK PRUEBA EXITOSA: La reconexión automática funciona correctamente
  - La API maneja errores cuando RabbitMQ no está disponible
  - La API se reconecta automáticamente cuando RabbitMQ vuelve
```

---

## Prueba 2: Carga Básica

### Ejecutar (100 eventos)

```powershell
./test-load.ps1
```

### Ejecutar (Personalizado)

```powershell
# 50 eventos
./test-load.ps1 -NumEventos 50

# 200 eventos
./test-load.ps1 -NumEventos 200

# URL personalizada
./test-load.ps1 -NumEventos 100 -ApiUrl "http://localhost:5000"
```

### Qué Hace

1. Verifica que la API está corriendo
2. Captura uso de recursos inicial
3. Crea N eventos secuencialmente
4. Publica todos los eventos a RabbitMQ
5. Espera procesamiento de mensajes
6. Captura uso de recursos final
7. Verifica estado de colas en RabbitMQ

### Tiempo Estimado

- 100 eventos: ~15 segundos
- 200 eventos: ~30 segundos
- 500 eventos: ~75 segundos

### Resultado Esperado

```
OK EXCELENTE: Tasa de éxito 100%, tiempo promedio < 10ms
```

---

## Verificar Resultados

### Logs de la API

```powershell
docker logs eventos-api --tail 50
```

### RabbitMQ Management UI

Abrir en navegador: http://localhost:15672

- Usuario: `guest`
- Contraseña: `guest`

### PostgreSQL

```powershell
docker exec -it eventos-postgres psql -U postgres -d eventsdb -c "SELECT COUNT(*) FROM \"Eventos\";"
```

---

## Solución de Problemas

### Error: "No todos los servicios están corriendo"

```powershell
# Verificar estado de contenedores
docker ps -a

# Reiniciar servicios
docker-compose down
docker-compose up -d

# Esperar 30 segundos para que inicien
Start-Sleep -Seconds 30
```

### Error: "No se puede conectar a la API"

```powershell
# Verificar logs de la API
docker logs eventos-api

# Verificar health endpoint
Invoke-RestMethod -Uri "http://localhost:5000/health"
```

### Error: "Port is already allocated"

```powershell
# Detener otros contenedores que usen los mismos puertos
docker stop reportes-rabbitmq

# O cambiar puertos en docker-compose.yml
```

### Limpiar Datos de Prueba

```powershell
# Detener servicios
docker-compose down

# Eliminar volúmenes (CUIDADO: elimina todos los datos)
docker-compose down -v

# Reiniciar limpio
docker-compose up -d
```

---

## Interpretación de Resultados

### Prueba de Reconexión

| Resultado | Significado | Acción |
|-----------|-------------|--------|
| ✅ 3/3 eventos publicados | Excelente | Ninguna |
| ✅ 2/3 eventos publicados (fallo esperado con RabbitMQ detenido) | Bueno | Verificar logs |
| ❌ < 2/3 eventos publicados | Problema | Revisar configuración |

### Prueba de Carga

| Métrica | Excelente | Bueno | Requiere Atención |
|---------|-----------|-------|-------------------|
| **Tasa de Éxito** | 100% | > 95% | < 95% |
| **Tiempo Promedio (Publicación)** | < 10 ms | < 50 ms | > 50 ms |
| **Tiempo Promedio (Creación)** | < 50 ms | < 100 ms | > 100 ms |
| **CPU API** | < 5% | < 20% | > 20% |
| **Memoria API** | < 200 MiB | < 500 MiB | > 500 MiB |

---

## Documentación Completa

Para análisis detallado, consultar:
- `PRUEBAS-RESILIENCIA.md` - Resultados completos y análisis
- `TASK-5-COMPLETION-SUMMARY.md` - Resumen de completación

---

## Comandos Útiles

### Monitoreo en Tiempo Real

```powershell
# Logs en tiempo real
docker logs -f eventos-api

# Uso de recursos en tiempo real
docker stats eventos-api eventos-rabbitmq
```

### Reiniciar Servicios Específicos

```powershell
# Solo la API
docker-compose restart eventos-api

# Solo RabbitMQ
docker-compose restart rabbitmq
```

### Verificar Conectividad

```powershell
# Health de la API
Invoke-RestMethod -Uri "http://localhost:5000/health"

# RabbitMQ Management
Invoke-WebRequest -Uri "http://localhost:15672"

# PostgreSQL
docker exec eventos-postgres pg_isready -U postgres
```

---

## Notas Importantes

1. **Tiempo de Espera:** Los servicios pueden tardar 10-30 segundos en estar completamente listos después de `docker-compose up -d`

2. **Puertos:** Asegúrate de que los puertos 5000, 5672, 15672 y 5434 no estén en uso por otros servicios

3. **Recursos:** Las pruebas son ligeras, pero asegúrate de tener al menos 2 GB de RAM disponible

4. **Limpieza:** Después de las pruebas, puedes detener los servicios con `docker-compose down` para liberar recursos

---

**Última actualización:** 29 de Diciembre de 2024
