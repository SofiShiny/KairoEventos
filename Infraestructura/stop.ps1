# Script para detener la infraestructura Kairo
# PowerShell Script

Write-Host "🛑 Deteniendo Infraestructura Kairo..." -ForegroundColor Yellow
Write-Host ""

docker-compose down

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "✅ Infraestructura detenida correctamente" -ForegroundColor Green
    Write-Host ""
    Write-Host "💡 Los datos persisten en volúmenes Docker" -ForegroundColor Cyan
    Write-Host "💡 Para eliminar también los volúmenes usa: docker-compose down -v" -ForegroundColor Cyan
} else {
    Write-Host "❌ Error al detener la infraestructura" -ForegroundColor Red
    exit 1
}
