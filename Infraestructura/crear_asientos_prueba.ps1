# Script para crear asientos de prueba
# Ejecutar desde PowerShell

$mapaId = "de370d5a-e088-4cfc-9b88-dafd642c1c98"
$baseUrl = "http://localhost:5003/api/asientos"

# Crear 3 filas con 5 asientos cada una
for ($fila = 1; $fila -le 3; $fila++) {
    for ($numero = 1; $numero -le 5; $numero++) {
        $body = @{
            mapaId    = $mapaId
            fila      = $fila
            numero    = $numero
            categoria = "vip"
        } | ConvertTo-Json

        Write-Host "Creando asiento Fila $fila, Numero $numero..."
        
        try {
            $response = Invoke-WebRequest -Uri $baseUrl -Method POST -Body $body -ContentType "application/json"
            Write-Host "✓ Asiento creado: $($response.Content)" -ForegroundColor Green
        }
        catch {
            Write-Host "✗ Error: $_" -ForegroundColor Red
        }
    }
}

Write-Host "`n¡Listo! Se crearon 15 asientos (3 filas x 5 asientos)" -ForegroundColor Cyan
