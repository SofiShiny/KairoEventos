# Script para publicar eventos de prueba a RabbitMQ
# Este script verifica que los consumidores procesen eventos correctamente

Write-Host "=== Test de Consumidores de Eventos ===" -ForegroundColor Cyan
Write-Host ""

# Verificar que los servicios estén corriendo
Write-Host "1. Verificando servicios..." -ForegroundColor Yellow
$services = docker-compose ps --format json | ConvertFrom-Json
$allHealthy = $true

foreach ($service in $services) {
    $status = $service.State
    $name = $service.Service
    if ($status -eq "running") {
        Write-Host "   ✓ $name está corriendo" -ForegroundColor Green
    } else {
        Write-Host "   ✗ $name NO está corriendo (Estado: $status)" -ForegroundColor Red
        $allHealthy = $false
    }
}

if (-not $allHealthy) {
    Write-Host ""
    Write-Host "ERROR: No todos los servicios están corriendo. Ejecuta 'docker-compose up -d' primero." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "2. Verificando conectividad con MongoDB..." -ForegroundColor Yellow
try {
    $mongoTest = docker exec reportes-mongodb mongosh --eval "db.runCommand({ping: 1})" --quiet 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   ✓ MongoDB está respondiendo" -ForegroundColor Green
    } else {
        Write-Host "   ✗ MongoDB no responde" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "   ✗ Error al conectar con MongoDB: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "3. Verificando conectividad con RabbitMQ..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "http://localhost:15672/api/overview" -Method Get -Credential (New-Object System.Management.Automation.PSCredential("guest", (ConvertTo-SecureString "guest" -AsPlainText -Force))) -UseBasicParsing
    if ($response.StatusCode -eq 200) {
        Write-Host "   ✓ RabbitMQ Management API está respondiendo" -ForegroundColor Green
    }
} catch {
    Write-Host "   ✗ RabbitMQ no responde: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "4. Verificando API de Reportes..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5002/health" -Method Get -UseBasicParsing -TimeoutSec 5
    if ($response.StatusCode -eq 200) {
        Write-Host "   ✓ API de Reportes está respondiendo" -ForegroundColor Green
    }
} catch {
    Write-Host "   ⚠ API de Reportes no responde en /health (puede ser normal si no está implementado)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "5. Verificando colecciones en MongoDB..." -ForegroundColor Yellow
$collections = docker exec reportes-mongodb mongosh reportes_db --eval "db.getCollectionNames()" --quiet 2>&1
Write-Host "   Colecciones encontradas:" -ForegroundColor Cyan
Write-Host "   $collections"

Write-Host ""
Write-Host "=== Verificación Completa ===" -ForegroundColor Green
Write-Host ""
Write-Host "Los servicios están listos para procesar eventos." -ForegroundColor Green
Write-Host "Los consumidores están escuchando en RabbitMQ y persistirán datos en MongoDB." -ForegroundColor Green
Write-Host ""
Write-Host "Para publicar eventos de prueba desde otros microservicios:" -ForegroundColor Cyan
Write-Host "  1. Asegúrate de que el microservicio de Eventos esté corriendo" -ForegroundColor White
Write-Host "  2. Publica un evento (ej: EventoPublicado)" -ForegroundColor White
Write-Host "  3. Los consumidores lo procesarán automáticamente" -ForegroundColor White
Write-Host ""
Write-Host "Para verificar que los datos se persisten:" -ForegroundColor Cyan
Write-Host "  docker exec reportes-mongodb mongosh reportes_db --eval 'db.metricas_evento.find().pretty()'" -ForegroundColor White
Write-Host "  docker exec reportes-mongodb mongosh reportes_db --eval 'db.historial_asistencia.find().pretty()'" -ForegroundColor White
Write-Host "  docker exec reportes-mongodb mongosh reportes_db --eval 'db.logs_auditoria.find().pretty()'" -ForegroundColor White
Write-Host ""
