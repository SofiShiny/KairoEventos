# Script para ejecutar la suite de pruebas unitarias del microservicio Comunidad.API

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  COMUNIDAD.API - SUITE DE PRUEBAS" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Ejecutar tests con verbosidad
Write-Host "Ejecutando tests..." -ForegroundColor Yellow
dotnet test --verbosity normal

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "  ✅ TODOS LOS TESTS PASARON" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    
    # Generar reporte de cobertura
    Write-Host "Generando reporte de cobertura..." -ForegroundColor Yellow
    dotnet test --collect:"XPlat Code Coverage" --verbosity quiet
    
    Write-Host ""
    Write-Host "Reporte de cobertura generado en:" -ForegroundColor Cyan
    Write-Host "test/Comunidad.Tests/TestResults/" -ForegroundColor White
} else {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "  ❌ ALGUNOS TESTS FALLARON" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
}
