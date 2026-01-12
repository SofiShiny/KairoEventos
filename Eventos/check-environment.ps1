# Simple environment verification script

Write-Host "Checking Environment..." -ForegroundColor Cyan
Write-Host ""

# Check RabbitMQ
Write-Host "RabbitMQ:" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "http://localhost:15672" -UseBasicParsing -TimeoutSec 3 -ErrorAction Stop
    Write-Host "  Status: Running" -ForegroundColor Green
    Write-Host "  Management UI: http://localhost:15672" -ForegroundColor Green
    Write-Host "  AMQP Port: 5672" -ForegroundColor Green
} catch {
    Write-Host "  Status: Not accessible" -ForegroundColor Red
}
Write-Host ""

# Check PostgreSQL
Write-Host "PostgreSQL:" -ForegroundColor Yellow
try {
    $result = docker exec eventos-postgres pg_isready -U postgres 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  Status: Running" -ForegroundColor Green
        Write-Host "  Host: localhost:5434" -ForegroundColor Green
        Write-Host "  Database: eventsdb" -ForegroundColor Green
    } else {
        Write-Host "  Status: Not responding" -ForegroundColor Red
    }
} catch {
    Write-Host "  Status: Not accessible" -ForegroundColor Red
}
Write-Host ""

# Check configuration files
Write-Host "Configuration:" -ForegroundColor Yellow
if (Test-Path "backend/src/Services/Eventos/Eventos.API/appsettings.json") {
    Write-Host "  appsettings.json: Found" -ForegroundColor Green
} else {
    Write-Host "  appsettings.json: Not found" -ForegroundColor Red
}

if (Test-Path "docker-compose.yml") {
    Write-Host "  docker-compose.yml: Found" -ForegroundColor Green
} else {
    Write-Host "  docker-compose.yml: Not found" -ForegroundColor Red
}
Write-Host ""

Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Access RabbitMQ UI: http://localhost:15672 (guest/guest)" -ForegroundColor White
Write-Host "2. Run API: cd backend/src/Services/Eventos/Eventos.API && dotnet run" -ForegroundColor White
Write-Host ""
