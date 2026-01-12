# Script para detener el entorno E2E completo

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Deteniendo Entorno E2E" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Detener contenedores de Eventos
Write-Host "Deteniendo servicios de Eventos..." -ForegroundColor Yellow
Push-Location $PSScriptRoot
docker-compose down
Pop-Location
Write-Host "✓ Servicios de Eventos detenidos" -ForegroundColor Green
Write-Host ""

# Detener contenedores de Reportes
Write-Host "Deteniendo servicios de Reportes..." -ForegroundColor Yellow
Push-Location "$PSScriptRoot\..\Reportes"
docker-compose down
Pop-Location
Write-Host "✓ Servicios de Reportes detenidos" -ForegroundColor Green
Write-Host ""

Write-Host "Nota: Las APIs deben cerrarse manualmente en sus ventanas de PowerShell" -ForegroundColor Yellow
Write-Host ""
Write-Host "Entorno E2E detenido completamente" -ForegroundColor Green
Write-Host ""
