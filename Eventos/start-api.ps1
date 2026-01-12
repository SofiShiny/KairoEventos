# Script para iniciar la API con variables de entorno
$env:POSTGRES_HOST="localhost"
$env:POSTGRES_DB="EventsDB"
$env:POSTGRES_USER="postgres"
$env:POSTGRES_PASSWORD="postgres"
$env:POSTGRES_PORT="5432"

Write-Host "Iniciando API con PostgreSQL..." -ForegroundColor Cyan
Write-Host "POSTGRES_HOST: $env:POSTGRES_HOST" -ForegroundColor Yellow

Set-Location "backend/src/Services/Eventos/Eventos.API"
dotnet run
