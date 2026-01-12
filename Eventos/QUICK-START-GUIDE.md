# 🚀 Guía de Inicio Rápido - Integración RabbitMQ

## ⚡ Inicio en 5 Minutos

### Paso 1: Levantar Infraestructura (2 minutos)

```powershell
# Terminal 1: RabbitMQ
docker run -d --name rabbitmq `
  -p 5672:5672 `
  -p 15672:15672 `
  rabbitmq:3-management

# Terminal 2: PostgreSQL
docker run -d --name postgres `
  -e POSTGRES_DB=eventsdb `
  -e POSTGRES_USER=postgres `
  -e POSTGRES_PASSWORD=postgres `
  -p 5432:5432 `
  postgres:15

# Verificar que están corriendo
docker ps
```

### Paso 2: Configurar Variables de Entorno (30 segundos)

```powershell
$env:RabbitMq:Host="localhost"
$env:POSTGRES_HOST="localhost"
```

### Paso 3: Ejecutar API de Eventos (2 minutos)

```powershell
cd Eventos/backend/src/Services/Eventos/Eventos.API
dotnet run
```

### Paso 4: Verificar (30 segundos)

Abrir en el navegador:
- ✅ Swagger: http://localhost:5000/swagger
- ✅ RabbitMQ UI: http://localhost:15672 (guest/guest)
- ✅ Health: http://localhost:5000/health

---

## 🧪 Primera Prueba (5 minutos)

### 1. Crear un Evento

```powershell
# PowerShell
$body = @{
    titulo = "Evento de Prueba RabbitMQ"
    descripcion = "Verificando integración"
    ubicacion = @{
        nombre = "Centro de Convenciones"
        direccion = "Av. Principal 123"
        ciudad = "Ciudad"
        pais = "País"
    }
    fechaInicio = "2025-02-01T10:00:00Z"
    fechaFin = "2025-02-01T18:00:00Z"
    maximoAsistentes = 100
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "http://localhost:5000/api/eventos" `
    -Method Post `
    -Body $body `
    -ContentType "application/json"

$eventoId = $response.id
Write-Host "Evento creado con ID: $eventoId"
```

### 2. Publicar el Evento

```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/eventos/$eventoId/publicar" `
    -Method Patch
Write-Host "✅ Evento publicado"
```

### 3. Verificar en RabbitMQ

1. Ir a http://localhost:15672
2. Login: `guest` / `guest`
3. Click en "Queues and Streams"
4. Deberías ver una cola con el mensaje

### 4. Registrar un Asistente

```powershell
$asistente = @{
    usuarioId = "user-001"
    nombre = "Juan Pérez"
    correo = "juan@example.com"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/eventos/$eventoId/asistentes" `
    -Method Post `
    -Body $asistente `
    -ContentType "application/json"
Write-Host "✅ Asistente registrado"
```

### 5. Cancelar el Evento

```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/eventos/$eventoId/cancelar" `
    -Method Patch
Write-Host "✅ Evento cancelado"
```

### 6. Verificar Resultados

En RabbitMQ UI deberías ver 3 mensajes publicados:
- ✅ EventoPublicadoEventoDominio
- ✅ AsistenteRegistradoEventoDominio
- ✅ EventoCanceladoEventoDominio

---

## 🔧 Script Completo de Prueba

Guarda esto como `test-integracion.ps1`:

```powershell
# test-integracion.ps1
Write-Host "🚀 Iniciando prueba de integración RabbitMQ..." -ForegroundColor Green

# 1. Crear evento
Write-Host "`n📝 Creando evento..." -ForegroundColor Yellow
$body = @{
    titulo = "Evento de Prueba RabbitMQ"
    descripcion = "Verificando integración"
    ubicacion = @{
        nombre = "Centro de Convenciones"
        direccion = "Av. Principal 123"
        ciudad = "Ciudad"
        pais = "País"
    }
    fechaInicio = "2025-02-01T10:00:00Z"
    fechaFin = "2025-02-01T18:00:00Z"
    maximoAsistentes = 100
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/eventos" `
        -Method Post `
        -Body $body `
        -ContentType "application/json"
    
    $eventoId = $response.id
    Write-Host "✅ Evento creado con ID: $eventoId" -ForegroundColor Green
    
    # 2. Publicar evento
    Write-Host "`n📢 Publicando evento..." -ForegroundColor Yellow
    Invoke-RestMethod -Uri "http://localhost:5000/api/eventos/$eventoId/publicar" `
        -Method Patch
    Write-Host "✅ Evento publicado a RabbitMQ" -ForegroundColor Green
    
    Start-Sleep -Seconds 2
    
    # 3. Registrar asistente
    Write-Host "`n👤 Registrando asistente..." -ForegroundColor Yellow
    $asistente = @{
        usuarioId = "user-test-001"
        nombre = "Juan Pérez"
        correo = "juan.perez@example.com"
    } | ConvertTo-Json
    
    Invoke-RestMethod -Uri "http://localhost:5000/api/eventos/$eventoId/asistentes" `
        -Method Post `
        -Body $asistente `
        -ContentType "application/json"
    Write-Host "✅ Asistente registrado y publicado a RabbitMQ" -ForegroundColor Green
    
    Start-Sleep -Seconds 2
    
    # 4. Cancelar evento
    Write-Host "`n❌ Cancelando evento..." -ForegroundColor Yellow
    Invoke-RestMethod -Uri "http://localhost:5000/api/eventos/$eventoId/cancelar" `
        -Method Patch
    Write-Host "✅ Evento cancelado y publicado a RabbitMQ" -ForegroundColor Green
    
    # Resumen
    Write-Host "`n" + "="*60 -ForegroundColor Cyan
    Write-Host "✅ PRUEBA COMPLETADA EXITOSAMENTE" -ForegroundColor Green
    Write-Host "="*60 -ForegroundColor Cyan
    Write-Host "`nVerifica en RabbitMQ Management UI:" -ForegroundColor Yellow
    Write-Host "http://localhost:15672" -ForegroundColor Cyan
    Write-Host "`nDeberías ver 3 mensajes publicados:" -ForegroundColor Yellow
    Write-Host "  1. EventoPublicadoEventoDominio" -ForegroundColor White
    Write-Host "  2. AsistenteRegistradoEventoDominio" -ForegroundColor White
    Write-Host "  3. EventoCanceladoEventoDominio" -ForegroundColor White
    Write-Host "`nEvento ID: $eventoId" -ForegroundColor Cyan
    
} catch {
    Write-Host "`n❌ ERROR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "`nVerifica que:" -ForegroundColor Yellow
    Write-Host "  1. La API está corriendo (http://localhost:5000/swagger)" -ForegroundColor White
    Write-Host "  2. RabbitMQ está corriendo (docker ps | grep rabbitmq)" -ForegroundColor White
    Write-Host "  3. PostgreSQL está corriendo (docker ps | grep postgres)" -ForegroundColor White
}
```

Ejecutar:
```powershell
.\test-integracion.ps1
```

---

## 🐛 Troubleshooting Rápido

### Problema: API no inicia

```powershell
# Verificar puerto
netstat -ano | findstr :5000

# Si está ocupado, matar el proceso
taskkill /PID <PID> /F

# O cambiar puerto
$env:ASPNETCORE_URLS="http://0.0.0.0:5001"
```

### Problema: No se conecta a RabbitMQ

```powershell
# Verificar que RabbitMQ está corriendo
docker ps | findstr rabbitmq

# Ver logs
docker logs rabbitmq

# Reiniciar
docker restart rabbitmq
```

### Problema: No se conecta a PostgreSQL

```powershell
# Verificar que PostgreSQL está corriendo
docker ps | findstr postgres

# Probar conexión
docker exec -it postgres psql -U postgres -d eventsdb -c "SELECT 1;"

# Reiniciar
docker restart postgres
```

### Problema: Mensajes no aparecen en RabbitMQ

```powershell
# Verificar variable de entorno
echo $env:RabbitMq:Host

# Debe ser "localhost"
# Si no está configurada:
$env:RabbitMq:Host="localhost"

# Reiniciar la API
```

---

## 📊 Comandos Útiles

### Ver logs de la API
```powershell
# Los logs aparecen en la consola donde ejecutaste dotnet run
```

### Ver logs de RabbitMQ
```powershell
docker logs rabbitmq -f
```

### Ver logs de PostgreSQL
```powershell
docker logs postgres -f
```

### Limpiar todo y empezar de nuevo
```powershell
# Detener y eliminar contenedores
docker stop rabbitmq postgres
docker rm rabbitmq postgres

# Eliminar volúmenes (CUIDADO: borra datos)
docker volume prune -f

# Volver a empezar desde Paso 1
```

### Verificar estado de servicios
```powershell
# Ver contenedores corriendo
docker ps

# Ver todos los contenedores
docker ps -a

# Ver uso de recursos
docker stats
```

---

## 🎯 Checklist de Verificación

Después de ejecutar las pruebas, verifica:

- [ ] API de Eventos responde en http://localhost:5000/swagger
- [ ] RabbitMQ UI accesible en http://localhost:15672
- [ ] Se creó un evento exitosamente
- [ ] Se publicó el evento sin errores
- [ ] Mensaje de EventoPublicadoEventoDominio visible en RabbitMQ
- [ ] Se registró un asistente exitosamente
- [ ] Mensaje de AsistenteRegistradoEventoDominio visible en RabbitMQ
- [ ] Se canceló el evento exitosamente
- [ ] Mensaje de EventoCanceladoEventoDominio visible en RabbitMQ
- [ ] No hay errores en logs de la API
- [ ] No hay errores en logs de RabbitMQ
- [ ] Datos persistidos correctamente en PostgreSQL

---

## 📚 Siguiente Paso

Una vez que todo funcione:

1. ✅ Revisar `PLAN-SIGUIENTES-PASOS.md` para el plan completo
2. ✅ Seguir con Fase 2: Actualización del Microservicio de Reportes
3. ✅ Realizar pruebas End-to-End completas

---

**¡Listo para empezar! 🚀**
