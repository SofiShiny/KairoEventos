# Script de inicio rápido para la infraestructura Kairo
# PowerShell Script

Write-Host "🏗️  Iniciando Infraestructura Kairo..." -ForegroundColor Cyan
Write-Host ""

# Verificar si Docker está corriendo
Write-Host "Verificando Docker..." -ForegroundColor Yellow
$dockerRunning = docker info 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Docker no está corriendo. Por favor inicia Docker Desktop." -ForegroundColor Red
    exit 1
}
Write-Host "✅ Docker está corriendo" -ForegroundColor Green
Write-Host ""

# Verificar si la red existe, si no, crearla
Write-Host "Verificando red kairo-network..." -ForegroundColor Yellow
$networkExists = docker network ls --filter name=kairo-network --format "{{.Name}}" | Select-String -Pattern "kairo-network"

if (-not $networkExists) {
    Write-Host "Creando red kairo-network..." -ForegroundColor Yellow
    docker network create kairo-network
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Red kairo-network creada" -ForegroundColor Green
    } else {
        Write-Host "❌ Error al crear la red" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "✅ Red kairo-network ya existe" -ForegroundColor Green
}
Write-Host ""

# Levantar servicios
Write-Host "Levantando servicios de infraestructura..." -ForegroundColor Yellow
docker-compose up -d

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "✅ Infraestructura iniciada correctamente" -ForegroundColor Green
    Write-Host ""
    Write-Host "📊 Servicios disponibles:" -ForegroundColor Cyan
    Write-Host "  - PostgreSQL:        localhost:5432" -ForegroundColor White
    Write-Host "  - MongoDB:           localhost:27017" -ForegroundColor White
    Write-Host "  - RabbitMQ AMQP:     localhost:5672" -ForegroundColor White
    Write-Host "  - RabbitMQ UI:       http://localhost:15672 (guest/guest)" -ForegroundColor White
    Write-Host ""
    Write-Host "🔍 Verificando health checks..." -ForegroundColor Yellow
    Start-Sleep -Seconds 5
    docker-compose ps
    Write-Host ""
    Write-Host "💡 Usa 'docker-compose logs -f' para ver los logs" -ForegroundColor Cyan
} else {
    Write-Host "❌ Error al iniciar la infraestructura" -ForegroundColor Red
    exit 1
}
