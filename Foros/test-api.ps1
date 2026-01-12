# Script de prueba para Comunidad API
$baseUrl = "http://localhost:5007"

Write-Host "=== Pruebas de Comunidad API ===" -ForegroundColor Cyan

# 1. Health Check
Write-Host "`n1. Verificando Health Check..." -ForegroundColor Yellow
try {
    $health = Invoke-RestMethod -Uri "$baseUrl/health" -Method Get
    Write-Host "✓ Health Check OK" -ForegroundColor Green
} catch {
    Write-Host "✗ Health Check falló: $_" -ForegroundColor Red
    exit 1
}

# 2. Crear un comentario de prueba
Write-Host "`n2. Creando comentario de prueba..." -ForegroundColor Yellow

# Generar GUIDs de prueba
$foroId = [guid]::NewGuid().ToString()
$usuarioId = [guid]::NewGuid().ToString()

$comentarioBody = @{
    foroId = $foroId
    usuarioId = $usuarioId
    contenido = "Este es un comentario de prueba desde PowerShell"
} | ConvertTo-Json

Write-Host "ForoId: $foroId" -ForegroundColor Gray
Write-Host "UsuarioId: $usuarioId" -ForegroundColor Gray

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/comunidad/comentarios" `
        -Method Post `
        -Body $comentarioBody `
        -ContentType "application/json"
    
    $comentarioId = $response
    Write-Host "✓ Comentario creado: $comentarioId" -ForegroundColor Green
} catch {
    Write-Host "✗ Error al crear comentario: $_" -ForegroundColor Red
    Write-Host "Nota: Esto es esperado si el foro no existe aún" -ForegroundColor Yellow
}

# 3. Obtener comentarios de un evento
Write-Host "`n3. Obteniendo comentarios..." -ForegroundColor Yellow
$eventoId = [guid]::NewGuid().ToString()

try {
    $comentarios = Invoke-RestMethod -Uri "$baseUrl/api/comunidad/foros/$eventoId" -Method Get
    Write-Host "✓ Comentarios obtenidos: $($comentarios.Count) comentarios" -ForegroundColor Green
    
    if ($comentarios.Count -gt 0) {
        Write-Host "`nPrimer comentario:" -ForegroundColor Cyan
        $comentarios[0] | ConvertTo-Json -Depth 3
    }
} catch {
    Write-Host "✗ Error al obtener comentarios: $_" -ForegroundColor Red
}

# 4. Verificar Swagger
Write-Host "`n4. Verificando Swagger UI..." -ForegroundColor Yellow
try {
    $swagger = Invoke-WebRequest -Uri "$baseUrl/swagger/index.html" -Method Get
    if ($swagger.StatusCode -eq 200) {
        Write-Host "✓ Swagger UI disponible en: $baseUrl/swagger" -ForegroundColor Green
    }
} catch {
    Write-Host "✗ Swagger UI no disponible: $_" -ForegroundColor Red
}

Write-Host "`n=== Resumen de Pruebas ===" -ForegroundColor Cyan
Write-Host "Para probar completamente el sistema:" -ForegroundColor White
Write-Host "1. Publica un evento desde el microservicio Eventos" -ForegroundColor White
Write-Host "2. Verifica que se creó el foro automáticamente" -ForegroundColor White
Write-Host "3. Crea comentarios usando el ForoId generado" -ForegroundColor White
Write-Host "4. Prueba responder a comentarios" -ForegroundColor White
Write-Host "5. Prueba ocultar comentarios (moderación)" -ForegroundColor White

Write-Host "`nAccede a Swagger para pruebas interactivas:" -ForegroundColor Yellow
Write-Host "$baseUrl/swagger" -ForegroundColor Cyan
