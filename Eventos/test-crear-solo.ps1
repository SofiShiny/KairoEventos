# Test solo crear evento
$ErrorActionPreference = "Stop"

Write-Host "=== Creando evento ===" -ForegroundColor Cyan
$body = Get-Content test-evento.json -Raw
$response = Invoke-RestMethod -Uri "http://localhost:5000/api/eventos" -Method Post -Body $body -ContentType "application/json"
$eventoId = $response.id
Write-Host "Evento creado: $eventoId" -ForegroundColor Green
Write-Host "Estado: $($response.estado)" -ForegroundColor Cyan

Write-Host "`n=== Verificando en BD ===" -ForegroundColor Cyan
Start-Sleep -Seconds 2
$query = "SELECT COUNT(*) FROM \`"Eventos\`";"
$count = docker exec eventos-postgres psql -U postgres -d EventsDB -t -c $query
Write-Host "Eventos en BD: $count" -ForegroundColor Yellow

Write-Host "`n=== Obteniendo evento por API ===" -ForegroundColor Cyan
try {
    $eventoGet = Invoke-RestMethod -Uri "http://localhost:5000/api/eventos/$eventoId" -Method Get
    Write-Host "Evento recuperado: $($eventoGet.id)" -ForegroundColor Green
    Write-Host "Título: $($eventoGet.titulo)" -ForegroundColor Cyan
} catch {
    Write-Host "ERROR: No se pudo recuperar el evento" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}
