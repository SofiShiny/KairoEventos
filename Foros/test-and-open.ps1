# Script ultra-simplificado para ejecutar tests y abrir reporte

Write-Host "Ejecutando tests con cobertura..." -ForegroundColor Cyan

# Ejecutar tests con cobertura
dotnet test test/Comunidad.Tests/Comunidad.Tests.csproj `
  /p:CollectCoverage=true `
  /p:CoverletOutput=TestResults/coverage `
  /p:CoverletOutputFormat=cobertura `
  /p:Threshold=90 `
  /p:ThresholdType=line `
  /p:ThresholdStat=total `
  --verbosity quiet

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error en los tests" -ForegroundColor Red
    exit 1
}

# Generar reporte HTML
Write-Host "Generando reporte HTML..." -ForegroundColor Cyan
reportgenerator `
  -reports:test/Comunidad.Tests/TestResults/**/coverage.cobertura.xml `
  -targetdir:coverage-report `
  -reporttypes:Html `
  -verbosity:Error

# Abrir en navegador
$reportPath = Join-Path $PSScriptRoot "coverage-report\index.html"
Start-Process (Resolve-Path $reportPath)

Write-Host "Reporte abierto en el navegador" -ForegroundColor Green
