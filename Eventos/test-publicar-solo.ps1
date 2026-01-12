# Test solo publicar evento
$ErrorActionPreference = "Stop"

Write-Host "=== PASO 1: Crear evento ===" -ForegroundColor Cyan
$body = Get-Content test-evento.json -Raw
$response = Invoke-RestMethod -Uri "http://localhost:5000/api/eventos" -Method Post -Body $body -ContentType "application/json"
$eventoId = $response.id
Write-Host "Evento creado: $eventoId" -ForegroundColor Green

Write-Host "`n=== PASO 2: Verificar en BD después de crear ===" -ForegroundColor Cyan
Start-Sleep -Seconds 1
$query = "SELECT COUNT(*) FROM \`"Eventos\`";"
$count = docker exec eventos-postgres psql -U postgres -d EventsDB -t -c $query
Write-Host "Eventos en BD: $count" -ForegroundColor Yellow

Write-Host "`n=== PASO 3: Publicar evento ===" -ForegroundColor Cyan
Invoke-RestMethod -Uri "http://localhost:5000/api/eventos/$eventoId/publicar" -Method Patch | Out-Null
Write-Host "Evento publicado" -ForegroundColor Green

Write-Host "`n=== PASO 4: Verificar en BD después de publicar ===" -ForegroundColor Cyan
Start-Sleep -Seconds 1
$count2 = docker exec eventos-postgres psql -U postgres -d EventsDB -t -c $query
Write-Host "Eventos en BD: $count2" -ForegroundColor Yellow

Write-Host "`n=== PASO 5: Obtener evento por API ===" -ForegroundColor Cyan
try {
    $eventoGet = Invoke-RestMethod -Uri "http://localhost:5000/api/eventos/$eventoId" -Method Get
    Write-Host "Evento recuperado por API: $($eventoGet.id)" -ForegroundColor Green
    Write-Host "Estado: $($eventoGet.estado)" -ForegroundColor Cyan
} catch {
    Write-Host "ERROR: No se pudo recuperar el evento" -ForegroundColor Red
}
