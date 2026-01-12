# Script para generar reporte de cobertura LIMPIO (sin migraciones)
# Uso: .\generar-cobertura.ps1

Write-Host "Limpiando reportes anteriores..." -ForegroundColor Cyan
Remove-Item -Recurse -Force TestResults, coverage-report, coverage.cobertura.xml -ErrorAction SilentlyContinue

Write-Host "Ejecutando pruebas con cobertura..." -ForegroundColor Cyan

# Usar coverlet.msbuild con exclusiones
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput=./coverage.cobertura.xml /p:ExcludeByFile='"**/Migrations/**/*.cs"' /p:Exclude='"[Eventos.Infraestructura]Eventos.Infraestructura.Persistencia.Migrations.*"'

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error al ejecutar las pruebas" -ForegroundColor Red
    exit 1
}

Write-Host "Generando reporte HTML..." -ForegroundColor Cyan
reportgenerator -reports:coverage.cobertura.xml -targetdir:coverage-report

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error al generar el reporte" -ForegroundColor Red
    exit 1
}

Write-Host "Reporte generado exitosamente!" -ForegroundColor Green
Write-Host "Abriendo reporte en el navegador..." -ForegroundColor Cyan

Start-Process "coverage-report\index.html"

Write-Host "Listo! El reporte se ha abierto en tu navegador." -ForegroundColor Green
