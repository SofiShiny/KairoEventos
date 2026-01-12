# Script para ejecutar tests con cobertura usando Coverlet (similar a Foros)
# Genera reporte HTML y lo abre en el explorador

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  REPORTES - COBERTURA DE CODIGO" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
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
Write-Host "Paso 1: Ejecutando tests con cobertura..." -ForegroundColor Yellow

# Ejecutar tests con coverlet.collector
dotnet test backend/src/Services/Reportes/Reportes.Pruebas/Reportes.Pruebas.csproj `
    --collect:"XPlat Code Coverage" `
    --results-directory:./TestResults

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "Algunos tests fallaron, pero continuaremos con el reporte de cobertura..." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Tests ejecutados" -ForegroundColor Green
Write-Host ""

# Verificar que existe reportgenerator
Write-Host "Paso 2: Verificando herramienta reportgenerator..." -ForegroundColor Yellow

$reportGeneratorExists = Get-Command reportgenerator -ErrorAction SilentlyContinue

if (-not $reportGeneratorExists) {
    Write-Host "reportgenerator no está instalado. Instalando..." -ForegroundColor Yellow
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

Write-Host "Paso 3: Generando reporte HTML..." -ForegroundColor Yellow

Write-Host "Paso 3: Generando reporte HTML..." -ForegroundColor Yellow

# Generar reporte HTML con reportgenerator
reportgenerator `
    -reports:TestResults/**/coverage.cobertura.xml `
    -targetdir:coverage-report `
    -reporttypes:Html

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "ERROR: Falló la generación del reporte" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Reporte generado exitosamente" -ForegroundColor Green
Write-Host ""

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

# Abrir el reporte en el navegador predeterminado
Start-Process $fullPath

Write-Host "Reporte abierto en el navegador" -ForegroundColor Green
