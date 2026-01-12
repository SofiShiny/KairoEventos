# Test con pasos separados
$ErrorActionPreference = "Stop"

Write-Host "=== PASO 1: Crear evento ===" -ForegroundColor Cyan
$body = Get-Content test-evento.json -Raw
$response = Invoke-RestMethod -Uri "http://localhost:5000/api/eventos" -Method Post -Body $body -ContentType "application/json"
$eventoId = $response.id
Write-Host "Evento creado: $eventoId" -ForegroundColor Green
$eventoId | Out-File -FilePath "evento-id.txt" -NoNewline

Write-Host "`n=== PASO 2: Publicar evento ===" -ForegroundColor Cyan
Invoke-RestMethod -Uri "http://localhost:5000/api/eventos/$eventoId/publicar" -Method Patch | Out-Null
Write-Host "Evento publicado" -ForegroundColor Green

Write-Host "`nEsperando 5 segundos..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

Write-Host "`n=== PASO 3: Registrar asistente ===" -ForegroundColor Cyan
$asistente = '{"usuarioId":"user-001","nombre":"Juan Perez","correo":"juan@example.com"}'
try {
    $asistenteResp = Invoke-RestMethod -Uri "http://localhost:5000/api/eventos/$eventoId/asistentes" -Method Post -Body $asistente -ContentType "application/json"
    Write-Host "Asistente registrado exitosamente" -ForegroundColor Green
    Write-Host "Asistente ID: $($asistenteResp.id)" -ForegroundColor Cyan
} catch {
    Write-Host "ERROR al registrar asistente:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Respuesta del servidor: $responseBody" -ForegroundColor Red
    }
}

Write-Host "`n=== FIN ===" -ForegroundColor Cyan
