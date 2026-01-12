# Script para ejecutar pruebas End-to-End completas
# Valida la integración entre Eventos y Reportes a través de RabbitMQ

param(
    [switch]$SkipSetup = $false
)

$ErrorActionPreference = "Continue"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Pruebas End-to-End" -ForegroundColor Cyan
Write-Host "Eventos <-> RabbitMQ <-> Reportes" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Variables globales
$EventosApiUrl = "http://localhost:5000"
$ReportesApiUrl = "http://localhost:5002"
$RabbitMqUrl = "http://localhost:15672"
$EventoId = $null
$TestResults = @{
    Setup = $false
    PublicarEvento = $false
    RegistrarAsistente = $false
    CancelarEvento = $false
    TotalTests = 4
    PassedTests = 0
}

# Función para verificar servicios
function Test-ServiceHealth {
    param([string]$Url, [string]$ServiceName)
    
    try {
        $response = Invoke-WebRequest -Uri "$Url/health" -TimeoutSec 5 -UseBasicParsing -ErrorAction Stop
        if ($response.StatusCode -eq 200) {
            Write-Host "  ✓ $ServiceName está disponible" -ForegroundColor Green
            return $true
        }
    } catch {
        Write-Host "  ✗ $ServiceName no está disponible" -ForegroundColor Red
        return $false
    }
    return $false
}

# Función para esperar procesamiento
function Wait-ForProcessing {
    param([int]$Seconds = 5)
    Write-Host "  Esperando $Seconds segundos para procesamiento..." -ForegroundColor Gray
    Start-Sleep -Seconds $Seconds
}

# Función para verificar RabbitMQ
function Test-RabbitMqQueues {
    try {
        $creds = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("guest:guest"))
        $headers = @{
            Authorization = "Basic $creds"
        }
        $response = Invoke-RestMethod -Uri "$RabbitMqUrl/api/queues" -Headers $headers -Method Get -TimeoutSec 5
        
        Write-Host "  Colas en RabbitMQ:" -ForegroundColor Cyan
        foreach ($queue in $response) {
            Write-Host "    - $($queue.name): $($queue.messages) mensajes" -ForegroundColor Gray
        }
        return $true
    } catch {
        Write-Host "  ⚠ No se pudo consultar RabbitMQ: $($_.Exception.Message)" -ForegroundColor Yellow
        return $false
    }
}

# ============================================
# FASE 1: Verificar Entorno
# ============================================
Write-Host "FASE 1: Verificando Entorno" -ForegroundColor Yellow
Write-Host "----------------------------------------" -ForegroundColor Gray

$eventosHealthy = Test-ServiceHealth -Url $EventosApiUrl -ServiceName "API Eventos"
$reportesHealthy = Test-ServiceHealth -Url $ReportesApiUrl -ServiceName "API Reportes"

if (-not $eventosHealthy -or -not $reportesHealthy) {
    Write-Host ""
    Write-Host "ERROR: Algunos servicios no están disponibles" -ForegroundColor Red
    Write-Host "Ejecuta primero: .\setup-e2e-environment.ps1" -ForegroundColor Yellow
    exit 1
}

$TestResults.Setup = $true
$TestResults.PassedTests++
Write-Host ""

# ============================================
# FASE 2: Prueba E2E - Publicar Evento
# ============================================
Write-Host "FASE 2: Prueba E2E - Publicar Evento" -ForegroundColor Yellow
Write-Host "----------------------------------------" -ForegroundColor Gray

try {
    # Crear evento
    Write-Host "1. Creando evento..." -ForegroundColor Cyan
    $nuevoEvento = @{
        titulo = "Concierto E2E Test $(Get-Date -Format 'yyyyMMdd-HHmmss')"
        descripcion = "Evento de prueba E2E"
        fechaInicio = (Get-Date).AddDays(30).ToString("yyyy-MM-ddTHH:mm:ss")
        fechaFin = (Get-Date).AddDays(30).AddHours(3).ToString("yyyy-MM-ddTHH:mm:ss")
        ubicacion = @{
            direccion = "Calle Test 123"
            ciudad = "Ciudad Test"
            pais = "País Test"
        }
        capacidadMaxima = 100
        organizadorId = [Guid]::NewGuid().ToString()
    } | ConvertTo-Json

    $response = Invoke-RestMethod -Uri "$EventosApiUrl/api/eventos" -Method Post -Body $nuevoEvento -ContentType "application/json" -TimeoutSec 10
    $EventoId = $response.id
    Write-Host "  ✓ Evento creado: $EventoId" -ForegroundColor Green

    # Publicar evento
    Write-Host "2. Publicando evento..." -ForegroundColor Cyan
    $publishResponse = Invoke-RestMethod -Uri "$EventosApiUrl/api/eventos/$EventoId/publicar" -Method Patch -TimeoutSec 10
    Write-Host "  ✓ Evento publicado" -ForegroundColor Green

    # Esperar procesamiento
    Wait-ForProcessing -Seconds 5

    # Verificar en RabbitMQ
    Write-Host "3. Verificando RabbitMQ..." -ForegroundColor Cyan
    Test-RabbitMqQueues | Out-Null

    # Verificar en Reportes
    Write-Host "4. Verificando en API de Reportes..." -ForegroundColor Cyan
    try {
        $reportesResponse = Invoke-RestMethod -Uri "$ReportesApiUrl/api/reportes/metricas-evento/$EventoId" -Method Get -TimeoutSec 10
        if ($reportesResponse) {
            Write-Host "  ✓ MetricasEvento encontrado en MongoDB" -ForegroundColor Green
            Write-Host "    - EventoId: $($reportesResponse.eventoId)" -ForegroundColor Gray
            Write-Host "    - Título: $($reportesResponse.tituloEvento)" -ForegroundColor Gray
            $TestResults.PublicarEvento = $true
            $TestResults.PassedTests++
        } else {
            Write-Host "  ✗ MetricasEvento no encontrado" -ForegroundColor Red
        }
    } catch {
        Write-Host "  ✗ Error consultando Reportes: $($_.Exception.Message)" -ForegroundColor Red
    }

} catch {
    Write-Host "  ✗ Error en prueba de publicación: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# ============================================
# FASE 3: Prueba E2E - Registrar Asistente
# ============================================
Write-Host "FASE 3: Prueba E2E - Registrar Asistente" -ForegroundColor Yellow
Write-Host "----------------------------------------" -ForegroundColor Gray

if ($EventoId) {
    try {
        # Registrar asistente
        Write-Host "1. Registrando asistente..." -ForegroundColor Cyan
        $nuevoAsistente = @{
            usuarioId = [Guid]::NewGuid().ToString()
            nombreUsuario = "Usuario Test E2E"
            email = "test@e2e.com"
        } | ConvertTo-Json

        $response = Invoke-RestMethod -Uri "$EventosApiUrl/api/eventos/$EventoId/asistentes" -Method Post -Body $nuevoAsistente -ContentType "application/json" -TimeoutSec 10
        Write-Host "  ✓ Asistente registrado" -ForegroundColor Green

        # Esperar procesamiento
        Wait-ForProcessing -Seconds 5

        # Verificar en RabbitMQ
        Write-Host "2. Verificando RabbitMQ..." -ForegroundColor Cyan
        Test-RabbitMqQueues | Out-Null

        # Verificar en Reportes
        Write-Host "3. Verificando en API de Reportes..." -ForegroundColor Cyan
        try {
            $reportesResponse = Invoke-RestMethod -Uri "$ReportesApiUrl/api/reportes/metricas-evento/$EventoId" -Method Get -TimeoutSec 10
            if ($reportesResponse -and $reportesResponse.totalAsistentes -gt 0) {
                Write-Host "  ✓ Métricas actualizadas en MongoDB" -ForegroundColor Green
                Write-Host "    - Total Asistentes: $($reportesResponse.totalAsistentes)" -ForegroundColor Gray
                $TestResults.RegistrarAsistente = $true
                $TestResults.PassedTests++
            } else {
                Write-Host "  ✗ Métricas no actualizadas" -ForegroundColor Red
            }
        } catch {
            Write-Host "  ✗ Error consultando Reportes: $($_.Exception.Message)" -ForegroundColor Red
        }

    } catch {
        Write-Host "  ✗ Error en prueba de registro: $($_.Exception.Message)" -ForegroundColor Red
    }
} else {
    Write-Host "  ⊘ Prueba omitida (no hay EventoId)" -ForegroundColor Yellow
}
Write-Host ""

# ============================================
# FASE 4: Prueba E2E - Cancelar Evento
# ============================================
Write-Host "FASE 4: Prueba E2E - Cancelar Evento" -ForegroundColor Yellow
Write-Host "----------------------------------------" -ForegroundColor Gray

if ($EventoId) {
    try {
        # Cancelar evento
        Write-Host "1. Cancelando evento..." -ForegroundColor Cyan
        $response = Invoke-RestMethod -Uri "$EventosApiUrl/api/eventos/$EventoId/cancelar" -Method Patch -TimeoutSec 10
        Write-Host "  ✓ Evento cancelado" -ForegroundColor Green

        # Esperar procesamiento
        Wait-ForProcessing -Seconds 5

        # Verificar en RabbitMQ
        Write-Host "2. Verificando RabbitMQ..." -ForegroundColor Cyan
        Test-RabbitMqQueues | Out-Null

        # Verificar en Reportes
        Write-Host "3. Verificando en API de Reportes..." -ForegroundColor Cyan
        try {
            $reportesResponse = Invoke-RestMethod -Uri "$ReportesApiUrl/api/reportes/metricas-evento/$EventoId" -Method Get -TimeoutSec 10
            if ($reportesResponse -and $reportesResponse.estado -eq "Cancelado") {
                Write-Host "  ✓ Estado actualizado en MongoDB" -ForegroundColor Green
                Write-Host "    - Estado: $($reportesResponse.estado)" -ForegroundColor Gray
                
                # Verificar LogAuditoria
                Write-Host "4. Verificando LogAuditoria..." -ForegroundColor Cyan
                $logsResponse = Invoke-RestMethod -Uri "$ReportesApiUrl/api/reportes/logs-auditoria?eventoId=$EventoId" -Method Get -TimeoutSec 10
                if ($logsResponse -and $logsResponse.Count -gt 0) {
                    Write-Host "  ✓ LogAuditoria encontrado" -ForegroundColor Green
                    Write-Host "    - Total logs: $($logsResponse.Count)" -ForegroundColor Gray
                    $TestResults.CancelarEvento = $true
                    $TestResults.PassedTests++
                } else {
                    Write-Host "  ✗ LogAuditoria no encontrado" -ForegroundColor Red
                }
            } else {
                Write-Host "  ✗ Estado no actualizado correctamente" -ForegroundColor Red
            }
        } catch {
            Write-Host "  ✗ Error consultando Reportes: $($_.Exception.Message)" -ForegroundColor Red
        }

    } catch {
        Write-Host "  ✗ Error en prueba de cancelación: $($_.Exception.Message)" -ForegroundColor Red
    }
} else {
    Write-Host "  ⊘ Prueba omitida (no hay EventoId)" -ForegroundColor Yellow
}
Write-Host ""

# ============================================
# RESUMEN
# ============================================
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Resumen de Pruebas E2E" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Resultados:" -ForegroundColor White
Write-Host "  Setup Entorno:        $(if ($TestResults.Setup) { '✓ PASS' } else { '✗ FAIL' })" -ForegroundColor $(if ($TestResults.Setup) { 'Green' } else { 'Red' })
Write-Host "  Publicar Evento:      $(if ($TestResults.PublicarEvento) { '✓ PASS' } else { '✗ FAIL' })" -ForegroundColor $(if ($TestResults.PublicarEvento) { 'Green' } else { 'Red' })
Write-Host "  Registrar Asistente:  $(if ($TestResults.RegistrarAsistente) { '✓ PASS' } else { '✗ FAIL' })" -ForegroundColor $(if ($TestResults.RegistrarAsistente) { 'Green' } else { 'Red' })
Write-Host "  Cancelar Evento:      $(if ($TestResults.CancelarEvento) { '✓ PASS' } else { '✗ FAIL' })" -ForegroundColor $(if ($TestResults.CancelarEvento) { 'Green' } else { 'Red' })
Write-Host ""

$successRate = [math]::Round(($TestResults.PassedTests / $TestResults.TotalTests) * 100, 2)
Write-Host "Total: $($TestResults.PassedTests)/$($TestResults.TotalTests) pruebas pasadas ($successRate%)" -ForegroundColor $(if ($successRate -eq 100) { 'Green' } elseif ($successRate -ge 75) { 'Yellow' } else { 'Red' })
Write-Host ""

if ($TestResults.PassedTests -eq $TestResults.TotalTests) {
    Write-Host "¡Todas las pruebas E2E pasaron exitosamente! ✓" -ForegroundColor Green
    exit 0
} else {
    Write-Host "Algunas pruebas fallaron. Revisa los logs arriba." -ForegroundColor Yellow
    exit 1
}
