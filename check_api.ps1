$response = Invoke-WebRequest -Uri 'http://localhost:8080/api/eventos' -UseBasicParsing
$json = $response.Content | ConvertFrom-Json
$first = $json | Select-Object -First 1

Write-Host "=== PRIMER EVENTO ===" -ForegroundColor Cyan
Write-Host "ID: $($first.id)"
Write-Host "Titulo: $($first.titulo)"
Write-Host "EsVirtual: $($first.esVirtual)"
Write-Host "PrecioBase: $($first.precioBase)"
Write-Host ""
Write-Host "=== TODAS LAS PROPIEDADES ===" -ForegroundColor Yellow
$first | Get-Member -MemberType NoteProperty | Select-Object Name
