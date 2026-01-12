# Script de inicio rápido para Comunidad API
Write-Host "=== Comunidad API - Inicio Rápido ===" -ForegroundColor Cyan

# Verificar si existe la red kairo-network
Write-Host "`nVerificando red Docker..." -ForegroundColor Yellow
$networkExists = docker network ls --filter name=kairo-network --format "{{.Name}}"

if (-not $networkExists) {
    Write-Host "Creando red kairo-network..." -ForegroundColor Yellow
    docker network create kairo-network
    Write-Host "Red creada exitosamente" -ForegroundColor Green
} else {
    Write-Host "Red kairo-network ya existe" -ForegroundColor Green
}

# Levantar servicios
Write-Host "`nLevantando servicios..." -ForegroundColor Yellow
docker-compose up -d

# Esperar a que los servicios estén listos
Write-Host "`nEsperando a que los servicios estén listos..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Verificar estado
Write-Host "`nEstado de los servicios:" -ForegroundColor Cyan
docker-compose ps

Write-Host "`n=== Servicios Disponibles ===" -ForegroundColor Green
Write-Host "API:                http://localhost:5007" -ForegroundColor White
Write-Host "Swagger:            http://localhost:5007/swagger" -ForegroundColor White
Write-Host "Health Check:       http://localhost:5007/health" -ForegroundColor White
Write-Host "MongoDB:            localhost:27020" -ForegroundColor White
Write-Host "RabbitMQ Management: http://localhost:15675 (guest/guest)" -ForegroundColor White

Write-Host "`n=== Comandos Útiles ===" -ForegroundColor Cyan
Write-Host "Ver logs:           docker logs -f comunidad-api" -ForegroundColor White
Write-Host "Detener servicios:  docker-compose down" -ForegroundColor White
Write-Host "Reiniciar:          docker-compose restart" -ForegroundColor White

Write-Host "`n¡Listo! El servicio está corriendo." -ForegroundColor Green
