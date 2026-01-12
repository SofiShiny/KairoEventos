# Script para ejecutar tests con cobertura y generar reporte HTML
# Similar al usado en el microservicio Foros

Write-Host "=== Ejecutando tests con cobertura de código ===" -ForegroundColor Cyan
Write-Host ""

# Limpiar resultados anteriores
if (Test-Path "TestResults") {
    Write-Host "Limpiando resultados anteriores..." -ForegroundColor Yellow
    Remove-Item -Recurse -Force "TestResults"
}

if (Test-Path "coverage-report") {
    Write-Host "Limpiando reporte anterior..." -ForegroundColor Yellow
    Remove-Item -Recurse -Force "coverage-report"
}

Write-Host ""
Write-Host "Ejecutando tests..." -ForegroundColor Green

# Ejecutar tests con cobertura usando XPlat Code Coverage
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "Algunos tests fallaron, pero continuaremos con el reporte de cobertura..." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=== Generando reporte HTML ===" -ForegroundColor Cyan

# Buscar el archivo de cobertura
$coverageFile = Get-ChildItem -Path "TestResults" -Filter "coverage.cobertura.xml" -Recurse | Select-Object -First 1

if ($null -eq $coverageFile) {
    Write-Host "ERROR: No se encontró el archivo de cobertura" -ForegroundColor Red
    exit 1
}

Write-Host "Archivo de cobertura encontrado: $($coverageFile.FullName)" -ForegroundColor Green

# Generar reporte HTML con reportgenerator
Write-Host "Generando reporte HTML..." -ForegroundColor Green
dotnet reportgenerator `
    -reports:"$($coverageFile.FullName)" `
    -targetdir:"coverage-report" `
    -reporttypes:"Html;TextSummary"

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Falló la generación del reporte" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=== Resumen de Cobertura ===" -ForegroundColor Cyan
Get-Content "coverage-report\Summary.txt"

Write-Host ""
Write-Host "=== Reporte generado exitosamente ===" -ForegroundColor Green
Write-Host "Ubicación: $(Resolve-Path 'coverage-report\index.html')" -ForegroundColor Cyan
Write-Host ""
Write-Host "Abriendo reporte en el navegador..." -ForegroundColor Green

# Abrir el reporte en el navegador predeterminado
Start-Process "coverage-report\index.html"

Write-Host ""
Write-Host "Proceso completado!" -ForegroundColor Green
