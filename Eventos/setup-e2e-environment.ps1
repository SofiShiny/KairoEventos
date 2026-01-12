# Script para configurar el entorno completo E2E
# Incluye: RabbitMQ, PostgreSQL (Eventos), MongoDB (Reportes), API Eventos, API Reportes

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Configuración Entorno E2E Completo" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar si Docker está corriendo
Write-Host "1. Verificando Docker..." -ForegroundColor Yellow
$dockerRunning = docker info 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Docker no está corriendo. Por favor inicia Docker Desktop." -ForegroundColor Red
    exit 1
}
Write-Host "✓ Docker está corriendo" -ForegroundColor Green
Write-Host ""

# Detener contenedores existentes
Write-Host "2. Limpiando contenedores existentes..." -ForegroundColor Yellow
Push-Location $PSScriptRoot
docker-compose down 2>&1 | Out-Null
Pop-Location

Push-Location "$PSScriptRoot\..\Reportes"
docker-compose down 2>&1 | Out-Null
Pop-Location

Write-Host "✓ Contenedores detenidos" -ForegroundColor Green
Write-Host ""

# Levantar RabbitMQ (compartido)
Write-Host "3. Levantando RabbitMQ..." -ForegroundColor Yellow
Push-Location $PSScriptRoot
docker-compose up -d rabbitmq
Pop-Location
Start-Sleep -Seconds 5
Write-Host "✓ RabbitMQ iniciado" -ForegroundColor Green
Write-Host ""

# Levantar PostgreSQL (Eventos)
Write-Host "4. Levantando PostgreSQL (Eventos)..." -ForegroundColor Yellow
Push-Location $PSScriptRoot
docker-compose up -d postgres
Pop-Location
Start-Sleep -Seconds 5
Write-Host "✓ PostgreSQL iniciado" -ForegroundColor Green
Write-Host ""

# Levantar MongoDB (Reportes)
Write-Host "5. Levantando MongoDB (Reportes)..." -ForegroundColor Yellow
Push-Location "$PSScriptRoot\..\Reportes"
docker-compose up -d mongodb
Pop-Location
Start-Sleep -Seconds 5
Write-Host "✓ MongoDB iniciado" -ForegroundColor Green
Write-Host ""

# Verificar health de servicios
Write-Host "6. Verificando health de servicios..." -ForegroundColor Yellow
Write-Host ""

# Verificar RabbitMQ
Write-Host "   Verificando RabbitMQ..." -ForegroundColor Gray
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
        Start-Sleep -Seconds 2
    }
}

if ($rabbitmqReady) {
    Write-Host "   ✓ RabbitMQ está listo" -ForegroundColor Green
} else {
    Write-Host "   ⚠ RabbitMQ no responde" -ForegroundColor Red
    exit 1
}

# Verificar PostgreSQL
Write-Host "   Verificando PostgreSQL..." -ForegroundColor Gray
$postgresReady = docker exec eventos-postgres pg_isready -U postgres 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "   ✓ PostgreSQL está listo" -ForegroundColor Green
} else {
    Write-Host "   ⚠ PostgreSQL no responde" -ForegroundColor Red
    exit 1
}

# Verificar MongoDB
Write-Host "   Verificando MongoDB..." -ForegroundColor Gray
$mongoReady = docker exec reportes-mongodb mongosh --eval "db.adminCommand('ping')" --quiet 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "   ✓ MongoDB está listo" -ForegroundColor Green
} else {
    Write-Host "   ⚠ MongoDB no responde" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Aplicar migraciones de PostgreSQL
Write-Host "7. Aplicando migraciones de PostgreSQL..." -ForegroundColor Yellow
Push-Location "$PSScriptRoot\backend\src\Services\Eventos\Eventos.API"
$env:POSTGRES_HOST = "localhost"
$env:POSTGRES_PORT = "5434"
$env:POSTGRES_DB = "eventsdb"
$env:POSTGRES_USER = "postgres"
$env:POSTGRES_PASSWORD = "postgres"
dotnet ef database update --project ..\Eventos.Infraestructura\Eventos.Infraestructura.csproj 2>&1 | Out-Null
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Migraciones aplicadas" -ForegroundColor Green
} else {
    Write-Host "⚠ Error aplicando migraciones (puede ser que ya estén aplicadas)" -ForegroundColor Yellow
}
Pop-Location
Write-Host ""

# Iniciar API de Eventos
Write-Host "8. Iniciando API de Eventos..." -ForegroundColor Yellow
Push-Location "$PSScriptRoot\backend\src\Services\Eventos\Eventos.API"
$env:POSTGRES_HOST = "localhost"
$env:POSTGRES_PORT = "5434"
$env:POSTGRES_DB = "eventsdb"
$env:POSTGRES_USER = "postgres"
$env:POSTGRES_PASSWORD = "postgres"
$env:RabbitMq__Host = "localhost"
$env:RabbitMq__Username = "guest"
$env:RabbitMq__Password = "guest"
$env:ASPNETCORE_URLS = "http://localhost:5000"
$env:ASPNETCORE_ENVIRONMENT = "Development"

Start-Process powershell -ArgumentList "-NoExit", "-Command", "dotnet run" -WindowStyle Normal
Pop-Location
Write-Host "✓ API de Eventos iniciando..." -ForegroundColor Green
Start-Sleep -Seconds 10
Write-Host ""

# Verificar API de Eventos
Write-Host "   Verificando API de Eventos..." -ForegroundColor Gray
$eventosApiReady = $false
$retryCount = 0
while ($retryCount -lt 15 -and -not $eventosApiReady) {
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5000/health" -TimeoutSec 2 -UseBasicParsing -ErrorAction SilentlyContinue
        if ($response.StatusCode -eq 200) {
            $eventosApiReady = $true
        }
    } catch {
        $retryCount++
        Start-Sleep -Seconds 2
    }
}

if ($eventosApiReady) {
    Write-Host "   ✓ API de Eventos está lista" -ForegroundColor Green
} else {
    Write-Host "   ⚠ API de Eventos no responde en /health" -ForegroundColor Yellow
}
Write-Host ""

# Iniciar API de Reportes
Write-Host "9. Iniciando API de Reportes..." -ForegroundColor Yellow
Push-Location "$PSScriptRoot\..\Reportes\backend\src\Services\Reportes\Reportes.API"
$env:MONGODB_CONNECTION_STRING = "mongodb://localhost:27019"
$env:MONGODB_DATABASE = "reportes_db"
$env:RABBITMQ_HOST = "localhost"
$env:RABBITMQ_PORT = "5672"
$env:RABBITMQ_USER = "guest"
$env:RABBITMQ_PASSWORD = "guest"
$env:ASPNETCORE_URLS = "http://localhost:5002"
$env:ASPNETCORE_ENVIRONMENT = "Development"

Start-Process powershell -ArgumentList "-NoExit", "-Command", "dotnet run" -WindowStyle Normal
Pop-Location
Write-Host "✓ API de Reportes iniciando..." -ForegroundColor Green
Start-Sleep -Seconds 10
Write-Host ""

# Verificar API de Reportes
Write-Host "   Verificando API de Reportes..." -ForegroundColor Gray
$reportesApiReady = $false
$retryCount = 0
while ($retryCount -lt 15 -and -not $reportesApiReady) {
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5002/health" -TimeoutSec 2 -UseBasicParsing -ErrorAction SilentlyContinue
        if ($response.StatusCode -eq 200) {
            $reportesApiReady = $true
        }
    } catch {
        $retryCount++
        Start-Sleep -Seconds 2
    }
}

if ($reportesApiReady) {
    Write-Host "   ✓ API de Reportes está lista" -ForegroundColor Green
} else {
    Write-Host "   ⚠ API de Reportes no responde en /health" -ForegroundColor Yellow
}
Write-Host ""

# Resumen
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Entorno E2E Configurado" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Servicios de Infraestructura:" -ForegroundColor White
Write-Host "  ✓ RabbitMQ Management: http://localhost:15672 (guest/guest)" -ForegroundColor Green
Write-Host "  ✓ PostgreSQL: localhost:5434 (postgres/postgres)" -ForegroundColor Green
Write-Host "  ✓ MongoDB: localhost:27019" -ForegroundColor Green
Write-Host ""
Write-Host "APIs:" -ForegroundColor White
if ($eventosApiReady) {
    Write-Host "  ✓ API Eventos: http://localhost:5000" -ForegroundColor Green
    Write-Host "    Swagger: http://localhost:5000/swagger" -ForegroundColor Cyan
    Write-Host "    Health: http://localhost:5000/health" -ForegroundColor Cyan
} else {
    Write-Host "  ⚠ API Eventos: http://localhost:5000 (verificar manualmente)" -ForegroundColor Yellow
}

if ($reportesApiReady) {
    Write-Host "  ✓ API Reportes: http://localhost:5002" -ForegroundColor Green
    Write-Host "    Swagger: http://localhost:5002/swagger" -ForegroundColor Cyan
    Write-Host "    Health: http://localhost:5002/health" -ForegroundColor Cyan
} else {
    Write-Host "  ⚠ API Reportes: http://localhost:5002 (verificar manualmente)" -ForegroundColor Yellow
}
Write-Host ""
Write-Host "Para ejecutar pruebas E2E:" -ForegroundColor White
Write-Host "  .\run-e2e-tests.ps1" -ForegroundColor Cyan
Write-Host ""
Write-Host "Para detener todo:" -ForegroundColor White
Write-Host "  .\stop-e2e-environment.ps1" -ForegroundColor Cyan
Write-Host ""
