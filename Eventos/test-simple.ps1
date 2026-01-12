# Test simple de integracion RabbitMQ
$ErrorActionPreference = "Stop"

Write-Host "=== PRUEBA DE INTEGRACION RABBITMQ ===" -ForegroundColor Cyan

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

# TEST 3: Registrar asistente
Write-Host "`nTEST 3: Registrando asistente..." -ForegroundColor Yellow
$asistente = '{"usuarioId":"user-001","nombre":"Juan Perez","correo":"juan@example.com"}'
$asistenteResp = Invoke-RestMethod -Uri "http://localhost:5000/api/eventos/$eventoId/asistentes" -Method Post -Body $asistente -ContentType "application/json"
Write-Host "Asistente registrado (AsistenteRegistradoEventoDominio)" -ForegroundColor Green
Start-Sleep -Seconds 2

# TEST 4: Cancelar evento
Write-Host "`nTEST 4: Cancelando evento..." -ForegroundColor Yellow
Invoke-RestMethod -Uri "http://localhost:5000/api/eventos/$eventoId/cancelar" -Method Patch | Out-Null
Write-Host "Evento cancelado (EventoCanceladoEventoDominio)" -ForegroundColor Green

Write-Host "`n=== PRUEBAS COMPLETADAS ===" -ForegroundColor Green
Write-Host "Evento ID: $eventoId" -ForegroundColor Cyan
Write-Host "Verifica RabbitMQ UI: http://localhost:15672" -ForegroundColor Cyan
