# Script para probar reconexion automatica a RabbitMQ
# Requirements: 5.1, 5.2

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "PRUEBA DE RECONEXION A RABBITMQ" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$ErrorActionPreference = "Continue"
$baseUrl = "http://localhost:5000"
$rabbitContainer = "eventos-rabbitmq"

function Test-ContainerRunning {
    param($containerName)
    $result = docker ps --filter "name=$containerName" --format "{{.Names}}" 2>$null
    return $result -eq $containerName
}

function New-TestEvent {
    $evento = @{
        titulo = "Evento Prueba Reconexion $(Get-Date -Format 'HHmmss')"
        descripcion = "Evento para probar reconexion automatica"
        fechaInicio = (Get-Date).AddDays(30).ToString("yyyy-MM-ddTHH:mm:ssZ")
        fechaFin = (Get-Date).AddDays(30).AddHours(2).ToString("yyyy-MM-ddTHH:mm:ssZ")
        ubicacion = @{
            nombreLugar = "Lugar Prueba"
            direccion = "Calle Prueba 123"
            ciudad = "Ciudad Prueba"
            pais = "Pais Prueba"
        }
        maximoAsistentes = 100
    }
    
    $json = $evento | ConvertTo-Json -Depth 10
    
    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/api/eventos" -Method Post -Body $json -ContentType "application/json"
        return $response.id
    }
    catch {
        Write-Host "Error creando evento: $_" -ForegroundColor Red
        return $null
    }
}

function Publish-Event {
    param($eventoId)
    
    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/api/eventos/$eventoId/publicar" -Method Patch
        return $true
    }
    catch {
        Write-Host "Error publicando evento: $_" -ForegroundColor Red
        Write-Host "Detalles: $($_.Exception.Message)" -ForegroundColor Yellow
        return $false
    }
}

function Get-ApiLogs {
    param($lines = 20)
    
    Write-Host "`nUltimos logs de la API:" -ForegroundColor Yellow
    docker logs eventos-api --tail $lines 2>&1 | Select-Object -Last $lines
}

Write-Host "PASO 1: Verificar que todos los servicios estan corriendo" -ForegroundColor Green
Write-Host "==========================================================" -ForegroundColor Green

$containers = @("eventos-rabbitmq", "eventos-postgres", "eventos-api")
$allRunning = $true

foreach ($container in $containers) {
    if (Test-ContainerRunning $container) {
        Write-Host "OK $container esta corriendo" -ForegroundColor Green
    }
    else {
        Write-Host "ERROR $container NO esta corriendo" -ForegroundColor Red
        $allRunning = $false
    }
}

if (-not $allRunning) {
    Write-Host "`nError: No todos los servicios estan corriendo." -ForegroundColor Red
    Write-Host "Ejecuta: docker-compose up -d" -ForegroundColor Yellow
    exit 1
}

Write-Host "`nEsperando 5 segundos para asegurar que los servicios estan listos..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

Write-Host "`nPASO 2: Crear y publicar evento inicial (con RabbitMQ activo)" -ForegroundColor Green
Write-Host "================================================================" -ForegroundColor Green

$eventoId1 = New-TestEvent
if (-not $eventoId1) {
    Write-Host "Error: No se pudo crear el evento inicial" -ForegroundColor Red
    exit 1
}

Write-Host "OK Evento creado: $eventoId1" -ForegroundColor Green

$published1 = Publish-Event $eventoId1
if ($published1) {
    Write-Host "OK Evento publicado exitosamente (RabbitMQ activo)" -ForegroundColor Green
}
else {
    Write-Host "ERROR publicando evento inicial" -ForegroundColor Red
}

Write-Host "`nPASO 3: Detener RabbitMQ" -ForegroundColor Green
Write-Host "=========================" -ForegroundColor Green

Write-Host "Deteniendo contenedor de RabbitMQ..." -ForegroundColor Yellow
docker stop $rabbitContainer | Out-Null

Start-Sleep -Seconds 2

if (-not (Test-ContainerRunning $rabbitContainer)) {
    Write-Host "OK RabbitMQ detenido exitosamente" -ForegroundColor Green
}
else {
    Write-Host "ERROR: RabbitMQ sigue corriendo" -ForegroundColor Red
    exit 1
}

Write-Host "`nPASO 4: Intentar publicar evento (con RabbitMQ detenido)" -ForegroundColor Green
Write-Host "==========================================================" -ForegroundColor Green

$eventoId2 = New-TestEvent
if (-not $eventoId2) {
    Write-Host "Error: No se pudo crear el segundo evento" -ForegroundColor Red
    exit 1
}

Write-Host "OK Evento creado: $eventoId2" -ForegroundColor Green
Write-Host "Intentando publicar con RabbitMQ detenido..." -ForegroundColor Yellow

$published2 = Publish-Event $eventoId2

Write-Host "`nPASO 5: Verificar logs de error" -ForegroundColor Green
Write-Host "=================================" -ForegroundColor Green

Get-ApiLogs 30

Write-Host "`nBuscando errores de conexion a RabbitMQ en los logs..." -ForegroundColor Yellow
$logs = docker logs eventos-api --tail 50 2>&1
$connectionErrors = $logs | Select-String -Pattern "RabbitMQ|Connection|refused|failed" -CaseSensitive:$false

if ($connectionErrors) {
    Write-Host "OK Se encontraron logs de error de conexion:" -ForegroundColor Green
    $connectionErrors | ForEach-Object { Write-Host "  $_" -ForegroundColor Yellow }
}
else {
    Write-Host "AVISO No se encontraron logs especificos de error de RabbitMQ" -ForegroundColor Yellow
    Write-Host "  (Esto puede ser normal si MassTransit maneja los errores silenciosamente)" -ForegroundColor Gray
}

Write-Host "`nPASO 6: Reiniciar RabbitMQ" -ForegroundColor Green
Write-Host "===========================" -ForegroundColor Green

Write-Host "Reiniciando contenedor de RabbitMQ..." -ForegroundColor Yellow
docker start $rabbitContainer | Out-Null

Write-Host "Esperando que RabbitMQ este listo (30 segundos)..." -ForegroundColor Yellow
Start-Sleep -Seconds 30

if (Test-ContainerRunning $rabbitContainer) {
    Write-Host "OK RabbitMQ reiniciado exitosamente" -ForegroundColor Green
}
else {
    Write-Host "ERROR: RabbitMQ no se reinicio correctamente" -ForegroundColor Red
    exit 1
}

Write-Host "Verificando que RabbitMQ esta aceptando conexiones..." -ForegroundColor Yellow
$maxAttempts = 10
$attempt = 0
$rabbitReady = $false

while ($attempt -lt $maxAttempts -and -not $rabbitReady) {
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:15672" -TimeoutSec 2 -ErrorAction SilentlyContinue
        if ($response.StatusCode -eq 200) {
            $rabbitReady = $true
            Write-Host "OK RabbitMQ Management UI responde" -ForegroundColor Green
        }
    }
    catch {
        $attempt++
        Write-Host "  Intento $attempt/$maxAttempts..." -ForegroundColor Gray
        Start-Sleep -Seconds 3
    }
}

if (-not $rabbitReady) {
    Write-Host "AVISO RabbitMQ Management UI no responde, pero continuamos..." -ForegroundColor Yellow
}

Write-Host "`nPASO 7: Esperar reconexion automatica de la API" -ForegroundColor Green
Write-Host "=================================================" -ForegroundColor Green

Write-Host "Esperando 10 segundos para que la API se reconecte..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

Write-Host "`nPASO 8: Publicar evento despues de reconexion" -ForegroundColor Green
Write-Host "===============================================" -ForegroundColor Green

$eventoId3 = New-TestEvent
if (-not $eventoId3) {
    Write-Host "Error: No se pudo crear el tercer evento" -ForegroundColor Red
    exit 1
}

Write-Host "OK Evento creado: $eventoId3" -ForegroundColor Green
Write-Host "Intentando publicar despues de reconexion..." -ForegroundColor Yellow

$published3 = Publish-Event $eventoId3

if ($published3) {
    Write-Host "OK Evento publicado exitosamente despues de reconexion" -ForegroundColor Green
}
else {
    Write-Host "ERROR publicando evento despues de reconexion" -ForegroundColor Red
    Write-Host "AVISO La reconexion automatica puede tardar mas tiempo" -ForegroundColor Yellow
}

Write-Host "`nPASO 9: Verificar logs finales" -ForegroundColor Green
Write-Host "================================" -ForegroundColor Green

Get-ApiLogs 30

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "RESUMEN DE LA PRUEBA DE RECONEXION" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

Write-Host "`nEventos creados:" -ForegroundColor White
Write-Host "  1. $eventoId1 - Publicado con RabbitMQ activo: $(if($published1){'OK SI'}else{'ERROR NO'})" -ForegroundColor $(if($published1){'Green'}else{'Red'})
Write-Host "  2. $eventoId2 - Publicado con RabbitMQ detenido: $(if($published2){'OK SI'}else{'OK NO (esperado)'})" -ForegroundColor $(if(-not $published2){'Green'}else{'Yellow'})
Write-Host "  3. $eventoId3 - Publicado despues de reconexion: $(if($published3){'OK SI'}else{'ERROR NO'})" -ForegroundColor $(if($published3){'Green'}else{'Red'})

Write-Host "`nResultados:" -ForegroundColor White
if ($published1 -and -not $published2 -and $published3) {
    Write-Host "OK PRUEBA EXITOSA: La reconexion automatica funciona correctamente" -ForegroundColor Green
    Write-Host "  - La API maneja errores cuando RabbitMQ no esta disponible" -ForegroundColor Green
    Write-Host "  - La API se reconecta automaticamente cuando RabbitMQ vuelve" -ForegroundColor Green
}
elseif ($published1 -and $published3) {
    Write-Host "OK PRUEBA PARCIALMENTE EXITOSA: La reconexion funciona" -ForegroundColor Yellow
    Write-Host "  - La API se reconecta automaticamente" -ForegroundColor Green
    Write-Host "  - Revisar manejo de errores cuando RabbitMQ no esta disponible" -ForegroundColor Yellow
}
else {
    Write-Host "ERROR PRUEBA FALLIDA: Problemas con la reconexion" -ForegroundColor Red
    if (-not $published1) {
        Write-Host "  - La publicacion inicial fallo" -ForegroundColor Red
    }
    if (-not $published3) {
        Write-Host "  - La reconexion automatica no funciono o tardo mas de lo esperado" -ForegroundColor Red
    }
}

Write-Host "`nNotas:" -ForegroundColor White
Write-Host "  - MassTransit maneja la reconexion automaticamente" -ForegroundColor Gray
Write-Host "  - Los errores de conexion pueden no aparecer en logs si se manejan internamente" -ForegroundColor Gray
Write-Host "  - La reconexion puede tardar varios segundos" -ForegroundColor Gray

Write-Host "`n========================================" -ForegroundColor Cyan
