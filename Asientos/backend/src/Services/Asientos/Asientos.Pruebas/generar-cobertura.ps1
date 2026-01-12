$ErrorActionPreference = "Stop"

Write-Host "--- Generando Cobertura para Asientos (Modo ReportGenerator compatible) ---" -ForegroundColor Cyan

# 1. Limpiar reportes previos
if (Test-Path "TestResults") { Remove-Item -Recurse -Force "TestResults" }
if (Test-Path "coverage-report") { Remove-Item -Recurse -Force "coverage-report" }

# 2. Ejecutar pruebas con colector
# Nota: Se usa --settings para asegurar que las exclusiones de migraciones se apliquen
dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error durante la ejecución de pruebas." -ForegroundColor Red
    exit $LASTEXITCODE
}

# 3. Generar reporte HTML
reportgenerator -reports:TestResults/**/coverage.cobertura.xml -targetdir:coverage-report

Write-Host "`nReporte generado en: coverage-report\index.html" -ForegroundColor Green
