# Script para levantar el entorno local de desarrollo
# Incluye RabbitMQ y PostgreSQL

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Iniciando Entorno de Desarrollo" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar si Docker está corriendo
Write-Host "Verificando Docker..." -ForegroundColor Yellow
$dockerRunning = docker info 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Docker no está corriendo. Por favor inicia Docker Desktop." -ForegroundColor Red
    exit 1
}
Write-Host "✓ Docker está corriendo" -ForegroundColor Green
Write-Host ""

# Detener contenedores existentes si los hay
Write-Host "Deteniendo contenedores existentes..." -ForegroundColor Yellow
docker-compose down 2>&1 | Out-Null
Write-Host "✓ Contenedores detenidos" -ForegroundColor Green
Write-Host ""

# Levantar solo RabbitMQ y PostgreSQL (sin la API)
Write-Host "Levantando RabbitMQ y PostgreSQL..." -ForegroundColor Yellow
docker-compose up -d rabbitmq postgres

# Esperar a que los servicios estén listos
Write-Host ""
Write-Host "Esperando a que los servicios estén listos..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

# Verificar RabbitMQ
Write-Host ""
Write-Host "Verificando RabbitMQ..." -ForegroundColor Yellow
$maxRetries = 30
$retryCount = 0
$rabbitmqReady = $false

while ($retryCount -lt $maxRetries -and -not $rabbitmqReady) {
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:15672" -TimeoutSec 2 -UseBasicParsing -ErrorAction SilentlyContinue
        if ($response.StatusCode -eq 200) {
            $rabbitmqReady = $true
        }
    } catch {
        $retryCount++
        Write-Host "  Intento $retryCount/$maxRetries..." -ForegroundColor Gray
        Start-Sleep -Seconds 2
    }
}

if ($rabbitmqReady) {
    Write-Host "✓ RabbitMQ está listo" -ForegroundColor Green
} else {
    Write-Host "⚠ RabbitMQ tardó más de lo esperado, pero puede estar iniciando" -ForegroundColor Yellow
}

# Verificar PostgreSQL
Write-Host ""
Write-Host "Verificando PostgreSQL..." -ForegroundColor Yellow
$postgresReady = docker exec eventos-postgres pg_isready -U postgres 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ PostgreSQL está listo" -ForegroundColor Green
} else {
    Write-Host "⚠ PostgreSQL puede estar iniciando todavía" -ForegroundColor Yellow
}

# Mostrar información de acceso
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Entorno Listo" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "RabbitMQ Management UI:" -ForegroundColor White
Write-Host "  URL: http://localhost:15672" -ForegroundColor Cyan
Write-Host "  Usuario: guest" -ForegroundColor Cyan
Write-Host "  Contraseña: guest" -ForegroundColor Cyan
Write-Host ""
Write-Host "PostgreSQL:" -ForegroundColor White
Write-Host "  Host: localhost" -ForegroundColor Cyan
Write-Host "  Puerto: 5434" -ForegroundColor Cyan
Write-Host "  Base de datos: eventsdb" -ForegroundColor Cyan
Write-Host "  Usuario: postgres" -ForegroundColor Cyan
Write-Host "  Contraseña: postgres" -ForegroundColor Cyan
Write-Host ""
Write-Host "Para ver los logs:" -ForegroundColor White
Write-Host "  docker-compose logs -f" -ForegroundColor Cyan
Write-Host ""
Write-Host "Para detener el entorno:" -ForegroundColor White
Write-Host "  docker-compose down" -ForegroundColor Cyan
Write-Host ""
Write-Host "Para ejecutar la API localmente:" -ForegroundColor White
Write-Host "  cd backend/src/Services/Eventos/Eventos.API" -ForegroundColor Cyan
Write-Host "  dotnet run" -ForegroundColor Cyan
Write-Host ""
