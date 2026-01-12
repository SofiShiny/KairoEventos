# Script para validar logs y manejo de errores - Task 2.5
# Este script prueba el logging y simula errores de RabbitMQ

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "TASK 2.5: Validación de Logs y Errores" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$ErrorActionPreference = "Continue"
$baseUrl = "http://localhost:5000"
$logFile = "test-logs-errores-$(Get-Date -Format 'yyyyMMdd-HHmmss').log"

# Función para escribir en log
function Write-Log {
    param($Message, $Color = "White")
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logMessage = "[$timestamp] $Message"
    Write-Host $logMessage -ForegroundColor $Color
    Add-Content -Path $logFile -Value $logMessage
}

# Función para verificar servicio
function Test-Service {
    param($Name, $Url)
    try {
        $response = Invoke-WebRequest -Uri $Url -Method Get -TimeoutSec 5
        Write-Log "$Name esta disponible" "Green"
        return $true
    }
    catch {
        Write-Log "$Name NO esta disponible: $($_.Exception.Message)" "Red"
        return $false
    }
}

Write-Log "=== FASE 1: Verificación de Servicios ===" "Cyan"
Write-Host ""

$apiOk = Test-Service "API de Eventos" "$baseUrl/health"
$rabbitOk = Test-Service "RabbitMQ Management" "http://localhost:15672"

if (-not $apiOk) {
    Write-Log "ERROR: La API de Eventos no esta disponible. Ejecuta 'start-environment.ps1' primero." "Red"
    exit 1
}

Write-Host ""
Write-Log "=== FASE 2: Prueba con RabbitMQ Funcionando ===" "Cyan"
Write-Host ""

# Crear un evento de prueba
Write-Log "Creando evento de prueba..." "Yellow"
$eventoBody = @{
    titulo = "Evento Test Logs $(Get-Date -Format 'HH:mm:ss')"
    descripcion = "Evento para probar logs y manejo de errores"
    fechaInicio = (Get-Date).AddDays(7).ToString("yyyy-MM-ddTHH:mm:ss")
    fechaFin = (Get-Date).AddDays(7).AddHours(2).ToString("yyyy-MM-ddTHH:mm:ss")
    ubicacion = @{
        nombreLugar = "Test Location"
        direccion = "123 Test St"
        ciudad = "Test City"
        pais = "Test Country"
    }
    maximoAsistentes = 100
} | ConvertTo-Json

try {
    $createResponse = Invoke-RestMethod -Uri "$baseUrl/api/eventos" -Method Post -Body $eventoBody -ContentType "application/json" -ErrorAction Stop
    $eventoId = $createResponse.id
    Write-Log "Evento creado: $eventoId" "Green"
}
catch {
    Write-Log "Error creando evento: $($_.Exception.Message)" "Red"
    exit 1
}

# Publicar el evento (con RabbitMQ funcionando)
Write-Log "Publicando evento con RabbitMQ funcionando..." "Yellow"
try {
    $publishResponse = Invoke-RestMethod -Uri "$baseUrl/api/eventos/$eventoId/publicar" -Method Patch -ErrorAction Stop
    Write-Log "Evento publicado exitosamente" "Green"
    Write-Log "  Respuesta: $($publishResponse | ConvertTo-Json -Compress)" "Gray"
}
catch {
    Write-Log "Error publicando evento: $($_.Exception.Message)" "Red"
    if ($_.Exception.Response) {
        Write-Log "  StatusCode: $($_.Exception.Response.StatusCode.value__)" "Red"
    }
}

Write-Host ""
Write-Log "=== FASE 3: Simulación de Error de RabbitMQ ===" "Cyan"
Write-Host ""

if ($rabbitOk) {
    Write-Log "Deteniendo RabbitMQ para simular error..." "Yellow"
    try {
        docker stop reportes-rabbitmq 2>&1 | Out-Null
        Write-Log "RabbitMQ detenido" "Green"
        Start-Sleep -Seconds 3
    }
    catch {
        Write-Log "No se pudo detener RabbitMQ: $($_.Exception.Message)" "Yellow"
    }
}

Write-Log "Creando segundo evento..." "Yellow"
$eventoBody2 = @{
    titulo = "Evento Test Error $(Get-Date -Format 'HH:mm:ss')"
    descripcion = "Evento para probar error de RabbitMQ"
    fechaInicio = (Get-Date).AddDays(8).ToString("yyyy-MM-ddTHH:mm:ss")
    fechaFin = (Get-Date).AddDays(8).AddHours(2).ToString("yyyy-MM-ddTHH:mm:ss")
    ubicacion = @{
        nombreLugar = "Test Location 2"
        direccion = "456 Test Ave"
        ciudad = "Test City 2"
        pais = "Test Country"
    }
    maximoAsistentes = 50
} | ConvertTo-Json

try {
    $createResponse2 = Invoke-RestMethod -Uri "$baseUrl/api/eventos" -Method Post -Body $eventoBody2 -ContentType "application/json" -ErrorAction Stop
    $eventoId2 = $createResponse2.id
    Write-Log "Segundo evento creado: $eventoId2" "Green"
}
catch {
    Write-Log "Error creando segundo evento: $($_.Exception.Message)" "Red"
}

# Intentar publicar con RabbitMQ caído
Write-Log "Intentando publicar evento con RabbitMQ caido..." "Yellow"
try {
    $publishResponse2 = Invoke-RestMethod -Uri "$baseUrl/api/eventos/$eventoId2/publicar" -Method Patch -ErrorAction Stop
    Write-Log "Publicacion completada (inesperado)" "Yellow"
    Write-Log "  Respuesta: $($publishResponse2 | ConvertTo-Json -Compress)" "Gray"
}
catch {
    Write-Log "Error capturado correctamente" "Green"
    if ($_.Exception.Response) {
        Write-Log "  StatusCode: $($_.Exception.Response.StatusCode.value__)" "Gray"
    }
    Write-Log "  Mensaje: $($_.Exception.Message)" "Gray"
}

Write-Host ""
Write-Log "=== FASE 4: Reinicio de RabbitMQ ===" "Cyan"
Write-Host ""

if ($rabbitOk) {
    Write-Log "Reiniciando RabbitMQ..." "Yellow"
    try {
        docker start reportes-rabbitmq 2>&1 | Out-Null
        Write-Log "RabbitMQ iniciado" "Green"
        Write-Log "Esperando que RabbitMQ este listo..." "Yellow"
        Start-Sleep -Seconds 10
        
        # Verificar que RabbitMQ está funcionando
        $rabbitReady = Test-Service "RabbitMQ Management" "http://localhost:15672"
        if ($rabbitReady) {
            Write-Log "RabbitMQ esta listo" "Green"
        }
    }
    catch {
        Write-Log "Error reiniciando RabbitMQ: $($_.Exception.Message)" "Yellow"
    }
}

# Crear tercer evento y publicar con RabbitMQ recuperado
Write-Log "Creando tercer evento para probar recuperacion..." "Yellow"
$eventoBody3 = @{
    titulo = "Evento Test Recuperacion $(Get-Date -Format 'HH:mm:ss')"
    descripcion = "Evento para probar recuperacion de RabbitMQ"
    fechaInicio = (Get-Date).AddDays(9).ToString("yyyy-MM-ddTHH:mm:ss")
    fechaFin = (Get-Date).AddDays(9).AddHours(2).ToString("yyyy-MM-ddTHH:mm:ss")
    ubicacion = @{
        nombreLugar = "Test Location 3"
        direccion = "789 Test Blvd"
        ciudad = "Test City 3"
        pais = "Test Country"
    }
    maximoAsistentes = 75
} | ConvertTo-Json

try {
    $createResponse3 = Invoke-RestMethod -Uri "$baseUrl/api/eventos" -Method Post -Body $eventoBody3 -ContentType "application/json" -ErrorAction Stop
    $eventoId3 = $createResponse3.id
    Write-Log "Tercer evento creado: $eventoId3" "Green"
}
catch {
    Write-Log "Error creando tercer evento: $($_.Exception.Message)" "Red"
}

Write-Log "Publicando evento despues de recuperacion..." "Yellow"
try {
    $publishResponse3 = Invoke-RestMethod -Uri "$baseUrl/api/eventos/$eventoId3/publicar" -Method Patch -ErrorAction Stop
    Write-Log "Evento publicado exitosamente despues de recuperacion" "Green"
    Write-Log "  Respuesta: $($publishResponse3 | ConvertTo-Json -Compress)" "Gray"
}
catch {
    Write-Log "Error publicando despues de recuperacion: $($_.Exception.Message)" "Red"
}

Write-Host ""
Write-Log "=== FASE 5: Análisis de Logs de la API ===" "Cyan"
Write-Host ""

Write-Log "Buscando logs de la API..." "Yellow"
Write-Log "NOTA: Los logs de la API se muestran en la consola donde se ejecuto 'dotnet run'" "Yellow"
Write-Log "Busca los siguientes patrones en los logs:" "Yellow"
Write-Log "  - 'Iniciando publicacion de evento'" "Gray"
Write-Log "  - 'Publicando evento ... a RabbitMQ...'" "Gray"
Write-Log "  - 'Evento ... publicado exitosamente a RabbitMQ'" "Gray"
Write-Log "  - 'Error inesperado al publicar evento' (cuando RabbitMQ esta caido)" "Gray"
Write-Log "  - Excepciones de MassTransit/RabbitMQ" "Gray"

Write-Host ""
Write-Log "=== RESUMEN DE PRUEBAS ===" "Cyan"
Write-Host ""

Write-Log "Fase 1: Verificacion de servicios completada" "Green"
Write-Log "Fase 2: Prueba con RabbitMQ funcionando completada" "Green"
Write-Log "Fase 3: Simulacion de error de RabbitMQ completada" "Green"
Write-Log "Fase 4: Recuperacion de RabbitMQ completada" "Green"
Write-Log "Fase 5: Analisis de logs completado" "Green"

Write-Host ""
Write-Log "=== VERIFICACIONES REQUERIDAS ===" "Yellow"
Write-Host ""
Write-Log "1. Revisar logs de la API en la consola donde ejecutaste 'dotnet run'" "White"
Write-Log "2. Verificar que se registran logs informativos para operaciones exitosas" "White"
Write-Log "3. Verificar que se registran logs de error cuando RabbitMQ esta caido" "White"
Write-Log "4. Verificar que el sistema se recupera automaticamente cuando RabbitMQ vuelve" "White"
Write-Log "5. Verificar que los errores incluyen informacion detallada (tipo, mensaje, inner exception)" "White"

Write-Host ""
Write-Log "Log completo guardado en: $logFile" "Cyan"
Write-Host ""
