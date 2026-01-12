# Test debug
$ErrorActionPreference = "Continue"

# Crear y publicar evento
$body = Get-Content test-evento.json -Raw
$response = Invoke-RestMethod -Uri "http://localhost:5000/api/eventos" -Method Post -Body $body -ContentType "application/json"
$eventoId = $response.id
Write-Host "Evento creado: $eventoId"

Invoke-RestMethod -Uri "http://localhost:5000/api/eventos/$eventoId/publicar" -Method Patch | Out-Null
Write-Host "Evento publicado"

Start-Sleep -Seconds 2

# Intentar registrar asistente
$asistente = '{"usuarioId":"user-003","nombre":"Carlos Ruiz","correo":"carlos@example.com"}'
try {
    $result = Invoke-WebRequest -Uri "http://localhost:5000/api/eventos/$eventoId/asistentes" -Method Post -Body $asistente -ContentType "application/json"
    Write-Host "SUCCESS: Asistente registrado"
} catch {
    Write-Host "ERROR Status: $($_.Exception.Response.StatusCode.value__)"
    $stream = $_.Exception.Response.GetResponseStream()
    $reader = New-Object System.IO.StreamReader($stream)
    $errorBody = $reader.ReadToEnd()
    Write-Host "ERROR Body: $errorBody"
}
