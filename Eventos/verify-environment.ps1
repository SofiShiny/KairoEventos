# Script para verificar el estado del entorno local
# Verifica RabbitMQ, PostgreSQL y sus configuraciones

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Verificación del Entorno Local" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$allGood = $true

# 1. Verificar Docker
Write-Host "1. Verificando Docker..." -ForegroundColor Yellow
try {
    docker info | Out-Null
    Write-Host "   ✓ Docker está corriendo" -ForegroundColor Green
} catch {
    Write-Host "   ✗ Docker no está corriendo" -ForegroundColor Red
    $allGood = $false
}
Write-Host ""

# 2. Verificar RabbitMQ
Write-Host "2. Verificando RabbitMQ..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "http://localhost:15672" -UseBasicParsing -TimeoutSec 5
    if ($response.StatusCode -eq 200) {
        Write-Host "   ✓ RabbitMQ Management UI accesible en http://localhost:15672" -ForegroundColor Green
        Write-Host "   ✓ Puerto AMQP: 5672" -ForegroundColor Green
        Write-Host "   ✓ Puerto Management: 15672" -ForegroundColor Green
        
        # Verificar contenedor
        $container = docker ps --filter "name=rabbitmq" --format "{{.Names}}" | Select-Object -First 1
        if ($container) {
            Write-Host "   ✓ Contenedor: $container" -ForegroundColor Green
        }
    }
} catch {
    Write-Host "   ✗ RabbitMQ no está accesible" -ForegroundColor Red
    Write-Host "   Ejecuta: docker-compose up -d rabbitmq" -ForegroundColor Yellow
    $allGood = $false
}
Write-Host ""

# 3. Verificar PostgreSQL
Write-Host "3. Verificando PostgreSQL..." -ForegroundColor Yellow
try {
    $pgCheck = docker exec eventos-postgres pg_isready -U postgres 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   ✓ PostgreSQL está aceptando conexiones" -ForegroundColor Green
        Write-Host "   ✓ Host: localhost" -ForegroundColor Green
        Write-Host "   ✓ Puerto: 5434" -ForegroundColor Green
        Write-Host "   ✓ Base de datos: eventsdb" -ForegroundColor Green
        Write-Host "   ✓ Usuario: postgres" -ForegroundColor Green
    } else {
        Write-Host "   ✗ PostgreSQL no está respondiendo" -ForegroundColor Red
        $allGood = $false
    }
} catch {
    Write-Host "   ✗ PostgreSQL no está accesible" -ForegroundColor Red
    Write-Host "   Ejecuta: docker-compose up -d postgres" -ForegroundColor Yellow
    $allGood = $false
}
Write-Host ""

# 4. Verificar configuración de la API
Write-Host "4. Verificando configuración de la API..." -ForegroundColor Yellow
$appsettingsPath = "backend/src/Services/Eventos/Eventos.API/appsettings.json"
if (Test-Path $appsettingsPath) {
    $appsettings = Get-Content $appsettingsPath | ConvertFrom-Json
    if ($appsettings.RabbitMq) {
        Write-Host "   ✓ Configuración RabbitMq encontrada en appsettings.json" -ForegroundColor Green
        Write-Host "     - Host: $($appsettings.RabbitMq.Host)" -ForegroundColor Gray
        Write-Host "     - Username: $($appsettings.RabbitMq.Username)" -ForegroundColor Gray
    } else {
        Write-Host "   ⚠ Configuración RabbitMq no encontrada en appsettings.json" -ForegroundColor Yellow
    }
} else {
    Write-Host "   ✗ No se encontró appsettings.json" -ForegroundColor Red
    $allGood = $false
}
Write-Host ""

# 5. Verificar variables de entorno en docker-compose
Write-Host "5. Verificando docker-compose.yml..." -ForegroundColor Yellow
$dockerComposePath = "docker-compose.yml"
if (Test-Path $dockerComposePath) {
    $dockerComposeContent = Get-Content $dockerComposePath -Raw
    if ($dockerComposeContent -match "rabbitmq:") {
        Write-Host "   ✓ Servicio RabbitMQ configurado en docker-compose.yml" -ForegroundColor Green
    } else {
        Write-Host "   ⚠ Servicio RabbitMQ no encontrado en docker-compose.yml" -ForegroundColor Yellow
    }
    
    if ($dockerComposeContent -match "RabbitMq__Host") {
        Write-Host "   ✓ Variables de entorno RabbitMQ configuradas para eventos-api" -ForegroundColor Green
    } else {
        Write-Host "   ⚠ Variables de entorno RabbitMQ no configuradas" -ForegroundColor Yellow
    }
} else {
    Write-Host "   ✗ No se encontró docker-compose.yml" -ForegroundColor Red
    $allGood = $false
}
Write-Host ""

# Resumen
Write-Host "========================================" -ForegroundColor Cyan
if ($allGood) {
    Write-Host "✓ Entorno Configurado Correctamente" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Próximos pasos:" -ForegroundColor White
    Write-Host "1. Accede a RabbitMQ Management UI: http://localhost:15672" -ForegroundColor Cyan
    Write-Host "   Usuario: guest / Contraseña: guest" -ForegroundColor Gray
    Write-Host ""
    Write-Host "2. Ejecuta la API de Eventos:" -ForegroundColor Cyan
    Write-Host "   cd backend/src/Services/Eventos/Eventos.API" -ForegroundColor Gray
    Write-Host "   dotnet run" -ForegroundColor Gray
    Write-Host ""
    Write-Host "3. Verifica Swagger UI: http://localhost:5000/swagger" -ForegroundColor Cyan
    Write-Host ""
} else {
    Write-Host "✗ Hay Problemas en el Entorno" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Revisa los errores arriba y ejecuta:" -ForegroundColor Yellow
    Write-Host "  ./start-environment.ps1" -ForegroundColor Cyan
    Write-Host ""
}
