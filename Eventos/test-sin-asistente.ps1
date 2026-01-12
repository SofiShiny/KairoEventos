# Test sin registro de asistente
$ErrorActionPreference = "Stop"

Write-Host "=== PRUEBA DE INTEGRACION RABBITMQ (SIN ASISTENTE) ===" -ForegroundColor Cyan

# TEST 1: Crear evento
Write-Host "`nTEST 1: Creando evento..." -ForegroundColor Yellow
$body = Get-Content test-evento.json -Raw
$response = Invoke-RestMethod -Uri "http://localhost:5000/api/eventos" -Method Post -Body $body -ContentType "application/json"
$eventoId = $response.id
Write-Host "Evento creado: $eventoId" -ForegroundColor Green

# TEST 2: Publicar evento
Write-Host "`nTEST 2: Publicando evento..." -ForegroundColor Yellow
Invoke-RestMethod -Uri "http://localhost:5000/api/eventos/$eventoId/publicar" -Method Patch | Out-Null
Write-Host "Evento publicado (EventoPublicadoEventoDominio)" -ForegroundColor Green
Start-Sleep -Seconds 2

# TEST 3: Cancelar evento
Write-Host "`nTEST 3: Cancelando evento..." -ForegroundColor Yellow
Invoke-RestMethod -Uri "http://localhost:5000/api/eventos/$eventoId/cancelar" -Method Patch | Out-Null
Write-Host "Evento cancelado (EventoCanceladoEventoDominio)" -ForegroundColor Green

Write-Host "`n=== PRUEBAS COMPLETADAS ===" -ForegroundColor Green
Write-Host "Evento ID: $eventoId" -ForegroundColor Cyan
Write-Host "Verifica RabbitMQ UI: http://localhost:15672" -ForegroundColor Cyan
Write-Host "`nSe publicaron 2 eventos:" -ForegroundColor Yellow
Write-Host "  1. EventoPublicadoEventoDominio" -ForegroundColor White
Write-Host "  2. EventoCanceladoEventoDominio" -ForegroundColor White
