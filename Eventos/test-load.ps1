# Script para prueba de carga basica - 100 eventos
# Requirements: 5.3, 5.4, 5.5

param(
    [int]$NumEventos = 100,
    [string]$ApiUrl = "http://localhost:5000"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "PRUEBA DE CARGA BASICA - RABBITMQ" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$ErrorActionPreference = "Continue"

# Metricas
$eventosCreados = 0
$eventosPublicados = 0
$erroresCreacion = 0
$erroresPublicacion = 0
$tiemposCreacion = @()
$tiemposPublicacion = @()
$tiempoInicio = Get-Date

function New-LoadTestEvent {
    param($index)
    
    $evento = @{
        titulo = "Evento Carga $index - $(Get-Date -Format 'HHmmss')"
        descripcion = "Evento de prueba de carga numero $index"
        fechaInicio = (Get-Date).AddDays(30 + $index).ToString("yyyy-MM-ddTHH:mm:ssZ")
        fechaFin = (Get-Date).AddDays(30 + $index).AddHours(2).ToString("yyyy-MM-ddTHH:mm:ssZ")
        ubicacion = @{
            nombreLugar = "Lugar Carga $index"
            direccion = "Calle Carga $index"
            ciudad = "Ciudad Carga"
            pais = "Pais Carga"
        }
        maximoAsistentes = 100
    }
    
    $json = $evento | ConvertTo-Json -Depth 10
    
    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    try {
        $response = Invoke-RestMethod -Uri "$ApiUrl/api/eventos" -Method Post -Body $json -ContentType "application/json" -TimeoutSec 10
        $sw.Stop()
        return @{
            Success = $true
            Id = $response.id
            Time = $sw.ElapsedMilliseconds
        }
    }
    catch {
        $sw.Stop()
        return @{
            Success = $false
            Error = $_.Exception.Message
            Time = $sw.ElapsedMilliseconds
        }
    }
}

function Publish-LoadTestEvent {
    param($eventoId)
    
    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    try {
        $response = Invoke-RestMethod -Uri "$ApiUrl/api/eventos/$eventoId/publicar" -Method Patch -TimeoutSec 10
        $sw.Stop()
        return @{
            Success = $true
            Time = $sw.ElapsedMilliseconds
        }
    }
    catch {
        $sw.Stop()
        return @{
            Success = $false
            Error = $_.Exception.Message
            Time = $sw.ElapsedMilliseconds
        }
    }
}

function Get-ResourceUsage {
    $apiContainer = "eventos-api"
    $rabbitContainer = "eventos-rabbitmq"
    
    try {
        $apiStats = docker stats $apiContainer --no-stream --format "{{.CPUPerc}},{{.MemUsage}}" 2>$null
        $rabbitStats = docker stats $rabbitContainer --no-stream --format "{{.CPUPerc}},{{.MemUsage}}" 2>$null
        
        if ($apiStats) {
            $apiParts = $apiStats -split ','
            $apiCpu = $apiParts[0]
            $apiMem = $apiParts[1]
        }
        else {
            $apiCpu = "N/A"
            $apiMem = "N/A"
        }
        
        if ($rabbitStats) {
            $rabbitParts = $rabbitStats -split ','
            $rabbitCpu = $rabbitParts[0]
            $rabbitMem = $rabbitParts[1]
        }
        else {
            $rabbitCpu = "N/A"
            $rabbitMem = "N/A"
        }
        
        return @{
            ApiCpu = $apiCpu
            ApiMem = $apiMem
            RabbitCpu = $rabbitCpu
            RabbitMem = $rabbitMem
        }
    }
    catch {
        return @{
            ApiCpu = "Error"
            ApiMem = "Error"
            RabbitCpu = "Error"
            RabbitMem = "Error"
        }
    }
}

Write-Host "Configuracion de prueba:" -ForegroundColor Yellow
Write-Host "  Numero de eventos: $NumEventos" -ForegroundColor White
Write-Host "  URL de API: $ApiUrl" -ForegroundColor White
Write-Host ""

# Verificar que la API esta corriendo
Write-Host "Verificando API..." -ForegroundColor Yellow
try {
    $health = Invoke-RestMethod -Uri "$ApiUrl/health" -Method Get -TimeoutSec 5
    Write-Host "OK API esta corriendo: $($health.status)" -ForegroundColor Green
}
catch {
    Write-Host "ERROR: No se puede conectar a la API" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "INICIANDO PRUEBA DE CARGA" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Capturar uso de recursos inicial
Write-Host "Capturando uso de recursos inicial..." -ForegroundColor Yellow
$recursosInicial = Get-ResourceUsage
Write-Host "  API - CPU: $($recursosInicial.ApiCpu), Memoria: $($recursosInicial.ApiMem)" -ForegroundColor Gray
Write-Host "  RabbitMQ - CPU: $($recursosInicial.RabbitCpu), Memoria: $($recursosInicial.RabbitMem)" -ForegroundColor Gray
Write-Host ""

# Fase 1: Crear eventos
Write-Host "FASE 1: Creando $NumEventos eventos..." -ForegroundColor Green
$eventosIds = @()

for ($i = 1; $i -le $NumEventos; $i++) {
    $resultado = New-LoadTestEvent -index $i
    
    if ($resultado.Success) {
        $eventosCreados++
        $eventosIds += $resultado.Id
        $tiemposCreacion += $resultado.Time
        
        if ($i % 10 -eq 0) {
            Write-Host "  Creados: $i/$NumEventos (Ultimo: $($resultado.Time)ms)" -ForegroundColor Gray
        }
    }
    else {
        $erroresCreacion++
        Write-Host "  Error creando evento $i : $($resultado.Error)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Resultados Fase 1:" -ForegroundColor Yellow
Write-Host "  Eventos creados: $eventosCreados/$NumEventos" -ForegroundColor $(if($eventosCreados -eq $NumEventos){'Green'}else{'Yellow'})
Write-Host "  Errores: $erroresCreacion" -ForegroundColor $(if($erroresCreacion -eq 0){'Green'}else{'Red'})

if ($tiemposCreacion.Count -gt 0) {
    $avgCreacion = ($tiemposCreacion | Measure-Object -Average).Average
    $minCreacion = ($tiemposCreacion | Measure-Object -Minimum).Minimum
    $maxCreacion = ($tiemposCreacion | Measure-Object -Maximum).Maximum
    Write-Host "  Tiempo promedio: $([math]::Round($avgCreacion, 2))ms" -ForegroundColor White
    Write-Host "  Tiempo minimo: $minCreacion ms" -ForegroundColor White
    Write-Host "  Tiempo maximo: $maxCreacion ms" -ForegroundColor White
}

Write-Host ""
Write-Host "Esperando 5 segundos antes de publicar..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

# Fase 2: Publicar eventos
Write-Host ""
Write-Host "FASE 2: Publicando $($eventosIds.Count) eventos..." -ForegroundColor Green

foreach ($eventoId in $eventosIds) {
    $resultado = Publish-LoadTestEvent -eventoId $eventoId
    
    if ($resultado.Success) {
        $eventosPublicados++
        $tiemposPublicacion += $resultado.Time
        
        if ($eventosPublicados % 10 -eq 0) {
            Write-Host "  Publicados: $eventosPublicados/$($eventosIds.Count) (Ultimo: $($resultado.Time)ms)" -ForegroundColor Gray
        }
    }
    else {
        $erroresPublicacion++
        Write-Host "  Error publicando evento: $($resultado.Error)" -ForegroundColor Red
    }
}

$tiempoFin = Get-Date
$tiempoTotal = ($tiempoFin - $tiempoInicio).TotalSeconds

Write-Host ""
Write-Host "Resultados Fase 2:" -ForegroundColor Yellow
Write-Host "  Eventos publicados: $eventosPublicados/$($eventosIds.Count)" -ForegroundColor $(if($eventosPublicados -eq $eventosIds.Count){'Green'}else{'Yellow'})
Write-Host "  Errores: $erroresPublicacion" -ForegroundColor $(if($erroresPublicacion -eq 0){'Green'}else{'Red'})

if ($tiemposPublicacion.Count -gt 0) {
    $avgPublicacion = ($tiemposPublicacion | Measure-Object -Average).Average
    $minPublicacion = ($tiemposPublicacion | Measure-Object -Minimum).Minimum
    $maxPublicacion = ($tiemposPublicacion | Measure-Object -Maximum).Maximum
    Write-Host "  Tiempo promedio: $([math]::Round($avgPublicacion, 2))ms" -ForegroundColor White
    Write-Host "  Tiempo minimo: $minPublicacion ms" -ForegroundColor White
    Write-Host "  Tiempo maximo: $maxPublicacion ms" -ForegroundColor White
}

Write-Host ""
Write-Host "Esperando 10 segundos para que RabbitMQ procese los mensajes..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Capturar uso de recursos final
Write-Host ""
Write-Host "Capturando uso de recursos final..." -ForegroundColor Yellow
$recursosFinal = Get-ResourceUsage
Write-Host "  API - CPU: $($recursosFinal.ApiCpu), Memoria: $($recursosFinal.ApiMem)" -ForegroundColor Gray
Write-Host "  RabbitMQ - CPU: $($recursosFinal.RabbitCpu), Memoria: $($recursosFinal.RabbitMem)" -ForegroundColor Gray

# Verificar colas en RabbitMQ
Write-Host ""
Write-Host "Verificando colas en RabbitMQ..." -ForegroundColor Yellow
try {
    $queues = Invoke-RestMethod -Uri "http://localhost:15672/api/queues" -Method Get -Credential (New-Object PSCredential("guest", (ConvertTo-SecureString "guest" -AsPlainText -Force))) -TimeoutSec 5
    
    Write-Host "  Colas encontradas: $($queues.Count)" -ForegroundColor White
    foreach ($queue in $queues) {
        $messages = $queue.messages
        $ready = $queue.messages_ready
        $unacked = $queue.messages_unacknowledged
        Write-Host "    - $($queue.name): Total=$messages, Ready=$ready, Unacked=$unacked" -ForegroundColor Gray
    }
}
catch {
    Write-Host "  No se pudo consultar RabbitMQ Management API" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "RESUMEN DE PRUEBA DE CARGA" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Metricas Generales:" -ForegroundColor Yellow
Write-Host "  Tiempo total: $([math]::Round($tiempoTotal, 2)) segundos" -ForegroundColor White
Write-Host "  Throughput: $([math]::Round($NumEventos / $tiempoTotal, 2)) eventos/segundo" -ForegroundColor White
Write-Host ""

Write-Host "Creacion de Eventos:" -ForegroundColor Yellow
Write-Host "  Exitosos: $eventosCreados/$NumEventos ($([math]::Round(($eventosCreados/$NumEventos)*100, 2))%)" -ForegroundColor $(if($eventosCreados -eq $NumEventos){'Green'}else{'Yellow'})
Write-Host "  Errores: $erroresCreacion" -ForegroundColor $(if($erroresCreacion -eq 0){'Green'}else{'Red'})
if ($tiemposCreacion.Count -gt 0) {
    Write-Host "  Tiempo promedio: $([math]::Round(($tiemposCreacion | Measure-Object -Average).Average, 2))ms" -ForegroundColor White
}
Write-Host ""

Write-Host "Publicacion de Eventos:" -ForegroundColor Yellow
Write-Host "  Exitosos: $eventosPublicados/$($eventosIds.Count) ($([math]::Round(($eventosPublicados/$eventosIds.Count)*100, 2))%)" -ForegroundColor $(if($eventosPublicados -eq $eventosIds.Count){'Green'}else{'Yellow'})
Write-Host "  Errores: $erroresPublicacion" -ForegroundColor $(if($erroresPublicacion -eq 0){'Green'}else{'Red'})
if ($tiemposPublicacion.Count -gt 0) {
    Write-Host "  Tiempo promedio: $([math]::Round(($tiemposPublicacion | Measure-Object -Average).Average, 2))ms" -ForegroundColor White
}
Write-Host ""

Write-Host "Uso de Recursos:" -ForegroundColor Yellow
Write-Host "  API:" -ForegroundColor White
Write-Host "    Inicial - CPU: $($recursosInicial.ApiCpu), Memoria: $($recursosInicial.ApiMem)" -ForegroundColor Gray
Write-Host "    Final   - CPU: $($recursosFinal.ApiCpu), Memoria: $($recursosFinal.ApiMem)" -ForegroundColor Gray
Write-Host "  RabbitMQ:" -ForegroundColor White
Write-Host "    Inicial - CPU: $($recursosInicial.RabbitCpu), Memoria: $($recursosInicial.RabbitMem)" -ForegroundColor Gray
Write-Host "    Final   - CPU: $($recursosFinal.RabbitCpu), Memoria: $($recursosFinal.RabbitMem)" -ForegroundColor Gray
Write-Host ""

Write-Host "Evaluacion:" -ForegroundColor Yellow
$tasaExito = ($eventosPublicados / $NumEventos) * 100
if ($tasaExito -ge 95 -and $tiemposPublicacion.Count -gt 0) {
    $avgTime = ($tiemposPublicacion | Measure-Object -Average).Average
    if ($avgTime -lt 200) {
        Write-Host "  OK EXCELENTE: Tasa de exito $([math]::Round($tasaExito, 2))%, tiempo promedio $([math]::Round($avgTime, 2))ms" -ForegroundColor Green
    }
    elseif ($avgTime -lt 500) {
        Write-Host "  OK BUENO: Tasa de exito $([math]::Round($tasaExito, 2))%, tiempo promedio $([math]::Round($avgTime, 2))ms" -ForegroundColor Green
    }
    else {
        Write-Host "  ACEPTABLE: Tasa de exito $([math]::Round($tasaExito, 2))%, pero tiempos altos ($([math]::Round($avgTime, 2))ms)" -ForegroundColor Yellow
    }
}
elseif ($tasaExito -ge 90) {
    Write-Host "  ACEPTABLE: Tasa de exito $([math]::Round($tasaExito, 2))%, algunos errores" -ForegroundColor Yellow
}
else {
    Write-Host "  PROBLEMAS: Tasa de exito baja $([math]::Round($tasaExito, 2))%" -ForegroundColor Red
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
