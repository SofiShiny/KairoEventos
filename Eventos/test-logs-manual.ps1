# Script manual simplificado para validar logs - Task 2.5

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "TASK 2.5: Validacion de Logs (Manual)" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$baseUrl = "http://localhost:5000"

Write-Host "=== PASO 1: Crear y Publicar Evento (RabbitMQ Funcionando) ===" -ForegroundColor Yellow
Write-Host ""
Write-Host "Ejecuta estos comandos en otra terminal PowerShell:" -ForegroundColor White
Write-Host ""
Write-Host '$body = @{' -ForegroundColor Gray
Write-Host '    titulo = "Test Logs"' -ForegroundColor Gray
Write-Host '    descripcion = "Test"' -ForegroundColor Gray
Write-Host '    fechaInicio = (Get-Date).AddDays(7).ToString("yyyy-MM-ddTHH:mm:ss")' -ForegroundColor Gray
Write-Host '    fechaFin = (Get-Date).AddDays(7).AddHours(2).ToString("yyyy-MM-ddTHH:mm:ss")' -ForegroundColor Gray
Write-Host '    ubicacion = @{' -ForegroundColor Gray
Write-Host '        nombreLugar = "Test"' -ForegroundColor Gray
Write-Host '        direccion = "123 Test"' -ForegroundColor Gray
Write-Host '        ciudad = "City"' -ForegroundColor Gray
Write-Host '        pais = "Country"' -ForegroundColor Gray
Write-Host '    }' -ForegroundColor Gray
Write-Host '    maximoAsistentes = 100' -ForegroundColor Gray
Write-Host '} | ConvertTo-Json -Depth 3' -ForegroundColor Gray
Write-Host ""
Write-Host '$r = Invoke-RestMethod -Uri "http://localhost:5000/api/eventos" -Method Post -Body $body -ContentType "application/json"' -ForegroundColor Gray
Write-Host '$eventoId = $r.id' -ForegroundColor Gray
Write-Host 'Write-Host "Evento creado: $eventoId"' -ForegroundColor Gray
Write-Host ""
Write-Host 'Invoke-RestMethod -Uri "http://localhost:5000/api/eventos/$eventoId/publicar" -Method Patch' -ForegroundColor Gray
Write-Host ""
Write-Host "LOGS ESPERADOS EN LA CONSOLA DE LA API:" -ForegroundColor Cyan
Write-Host "  [INFO] Iniciando publicacion de evento {EventoId}" -ForegroundColor Green
Write-Host "  [INFO] Evento {EventoId} encontrado, estado actual: Borrador" -ForegroundColor Green
Write-Host "  [INFO] Evento {EventoId} marcado como publicado, guardando en BD..." -ForegroundColor Green
Write-Host "  [INFO] Evento {EventoId} guardado exitosamente en BD" -ForegroundColor Green
Write-Host "  [INFO] Verificacion OK: Evento {EventoId} existe con estado Publicado" -ForegroundColor Green
Write-Host "  [INFO] Publicando evento {EventoId} a RabbitMQ..." -ForegroundColor Green
Write-Host "  [INFO] Evento {EventoId} publicado exitosamente a RabbitMQ" -ForegroundColor Green
Write-Host ""

Write-Host "Presiona Enter cuando hayas ejecutado los comandos y revisado los logs..." -ForegroundColor Yellow
Read-Host

Write-Host ""
Write-Host "=== PASO 2: Simular Error de RabbitMQ ===" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. Detener RabbitMQ:" -ForegroundColor White
Write-Host "   docker stop reportes-rabbitmq" -ForegroundColor Gray
Write-Host ""
Write-Host "2. Crear y publicar otro evento (usa los mismos comandos de arriba)" -ForegroundColor White
Write-Host ""
Write-Host "LOGS ESPERADOS EN LA CONSOLA DE LA API:" -ForegroundColor Cyan
Write-Host "  [INFO] Iniciando publicacion de evento {EventoId}" -ForegroundColor Green
Write-Host "  [INFO] Evento {EventoId} encontrado, estado actual: Borrador" -ForegroundColor Green
Write-Host "  [INFO] Evento {EventoId} marcado como publicado, guardando en BD..." -ForegroundColor Green
Write-Host "  [INFO] Evento {EventoId} guardado exitosamente en BD" -ForegroundColor Green
Write-Host "  [INFO] Verificacion OK: Evento {EventoId} existe con estado Publicado" -ForegroundColor Green
Write-Host "  [INFO] Publicando evento {EventoId} a RabbitMQ..." -ForegroundColor Green
Write-Host "  [ERROR] Error inesperado al publicar evento {EventoId}. Tipo: ..., Mensaje: ..." -ForegroundColor Red
Write-Host ""
Write-Host "NOTA: El error debe incluir:" -ForegroundColor Yellow
Write-Host "  - Tipo de excepcion (ej: RabbitMqConnectionException)" -ForegroundColor Gray
Write-Host "  - Mensaje detallado del error" -ForegroundColor Gray
Write-Host "  - Stack trace completo" -ForegroundColor Gray
Write-Host ""

Write-Host "Presiona Enter cuando hayas ejecutado los comandos y revisado los logs..." -ForegroundColor Yellow
Read-Host

Write-Host ""
Write-Host "=== PASO 3: Verificar Recuperacion ===" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. Reiniciar RabbitMQ:" -ForegroundColor White
Write-Host "   docker start reportes-rabbitmq" -ForegroundColor Gray
Write-Host ""
Write-Host "2. Esperar 10 segundos" -ForegroundColor White
Write-Host ""
Write-Host "3. Crear y publicar otro evento" -ForegroundColor White
Write-Host ""
Write-Host "LOGS ESPERADOS: Deben ser exitosos nuevamente (como en PASO 1)" -ForegroundColor Cyan
Write-Host ""

Write-Host "Presiona Enter cuando hayas completado la verificacion..." -ForegroundColor Yellow
Read-Host

Write-Host ""
Write-Host "=== RESUMEN DE VERIFICACION ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Verifica que los logs incluyan:" -ForegroundColor Yellow
Write-Host "  [OK] Logs informativos para operaciones exitosas" -ForegroundColor White
Write-Host "  [OK] Logs de error cuando RabbitMQ esta caido" -ForegroundColor White
Write-Host "  [OK] Tipo de excepcion en logs de error" -ForegroundColor White
Write-Host "  [OK] Mensaje detallado en logs de error" -ForegroundColor White
Write-Host "  [OK] Stack trace en logs de error" -ForegroundColor White
Write-Host "  [OK] Sistema se recupera cuando RabbitMQ vuelve" -ForegroundColor White
Write-Host ""
Write-Host "Documentar resultados en: VERIFICACION-LOGS-ERRORES-TASK-2.5.md" -ForegroundColor Cyan
Write-Host ""
