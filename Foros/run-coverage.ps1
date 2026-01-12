# Script para ejecutar pruebas con cobertura y generar reporte HTML para Comunidad.API

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  COMUNIDAD.API - COBERTURA DE CODIGO" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Paso 1: Ejecutar tests con cobertura
Write-Host "Paso 1: Ejecutando tests con cobertura..." -ForegroundColor Yellow
dotnet test test/Comunidad.Tests/Comunidad.Tests.csproj `
  --collect:"XPlat Code Coverage" `
  --results-directory:./TestResults

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "Error al ejecutar los tests" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Tests ejecutados exitosamente" -ForegroundColor Green
Write-Host ""

# Paso 2: Verificar que existe reportgenerator
Write-Host "Paso 2: Verificando herramienta reportgenerator..." -ForegroundColor Yellow

$reportGeneratorExists = Get-Command reportgenerator -ErrorAction SilentlyContinue

if (-not $reportGeneratorExists) {
    Write-Host "reportgenerator no esta instalado. Instalando..." -ForegroundColor Yellow
    dotnet tool install -g dotnet-reportgenerator-globaltool
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host ""
        Write-Host "Error al instalar reportgenerator" -ForegroundColor Red
        Write-Host "Intenta instalarlo manualmente con:" -ForegroundColor Yellow
        Write-Host "  dotnet tool install -g dotnet-reportgenerator-globaltool" -ForegroundColor White
        exit 1
    }
}

Write-Host "reportgenerator disponible" -ForegroundColor Green
Write-Host ""

# Paso 3: Generar reporte HTML
Write-Host "Paso 3: Generando reporte HTML..." -ForegroundColor Yellow

reportgenerator `
  -reports:TestResults/**/coverage.cobertura.xml `
  -targetdir:coverage-report `
  -reporttypes:Html

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "Error al generar el reporte" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Reporte generado exitosamente" -ForegroundColor Green
Write-Host ""

# Paso 4: Abrir reporte en el navegador
Write-Host "Paso 4: Abriendo reporte en el navegador..." -ForegroundColor Yellow

$reportPath = Join-Path $PSScriptRoot "coverage-report\index.html"
$fullPath = Resolve-Path $reportPath

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  PROCESO COMPLETADO" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Reporte de cobertura:" -ForegroundColor Cyan
Write-Host "  $fullPath" -ForegroundColor White
Write-Host ""

# Abrir en el explorador
Start-Process $fullPath

Write-Host "Reporte abierto en el navegador" -ForegroundColor Green
