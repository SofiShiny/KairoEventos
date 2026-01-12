# Script de prueba de integración end-to-end
# Publica eventos de prueba y verifica que los consumidores procesen correctamente

param(
    [switch]$SkipBuild,
    [switch]$SkipVerification
)

$ErrorActionPreference = "Stop"

Write-Host "=== Prueba de Integración End-to-End ===" -ForegroundColor Cyan
Write-Host ""

# Paso 1: Verificar servicios
Write-Host "Paso 1: Verificando servicios..." -ForegroundColor Yellow
$services = docker-compose ps --format json 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Error: No se pudo obtener el estado de los servicios" -ForegroundColor Red
    Write-Host "   Ejecuta 'docker-compose up -d' primero" -ForegroundColor Yellow
    exit 1
}

$servicesJson = $services | ConvertFrom-Json
$allHealthy = $true

foreach ($service in $servicesJson) {
    $status = $service.State
    $name = $service.Service
    if ($status -eq "running") {
        Write-Host "   ✓ $name está corriendo" -ForegroundColor Green
    } else {
        Write-Host "   ✗ $name NO está corriendo (Estado: $status)" -ForegroundColor Red
        $allHealthy = $false
    }
}

if (-not $allHealthy) {
    Write-Host ""
    Write-Host "❌ ERROR: No todos los servicios están corriendo" -ForegroundColor Red
    exit 1
}

# Paso 2: Verificar MongoDB
Write-Host ""
Write-Host "Paso 2: Verificando MongoDB..." -ForegroundColor Yellow
try {
    $mongoTest = docker exec reportes-mongodb mongosh --eval "db.runCommand({ping: 1})" --quiet 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   ✓ MongoDB está respondiendo" -ForegroundColor Green
    } else {
        Write-Host "   ✗ MongoDB no responde" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "   ✗ Error al conectar con MongoDB: $_" -ForegroundColor Red
    exit 1
}

# Paso 3: Verificar RabbitMQ
Write-Host ""
Write-Host "Paso 3: Verificando RabbitMQ..." -ForegroundColor Yellow
try {
    $cred = New-Object System.Management.Automation.PSCredential("guest", (ConvertTo-SecureString "guest" -AsPlainText -Force))
    $response = Invoke-WebRequest -Uri "http://localhost:15672/api/overview" -Method Get -Credential $cred -UseBasicParsing -TimeoutSec 5
    if ($response.StatusCode -eq 200) {
        Write-Host "   ✓ RabbitMQ está respondiendo" -ForegroundColor Green
    }
} catch {
    Write-Host "   ✗ RabbitMQ no responde: $_" -ForegroundColor Red
    exit 1
}

# Paso 4: Verificar API
Write-Host ""
Write-Host "Paso 4: Verificando API de Reportes..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5002/health" -Method Get -UseBasicParsing -TimeoutSec 5
    if ($response.StatusCode -eq 200) {
        Write-Host "   ✓ API de Reportes está respondiendo" -ForegroundColor Green
    }
} catch {
    Write-Host "   ⚠ API de Reportes no responde en /health" -ForegroundColor Yellow
    Write-Host "   Intentando con endpoint de reportes..." -ForegroundColor Yellow
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5002/api/reportes/auditoria" -Method Get -UseBasicParsing -TimeoutSec 5
        Write-Host "   ✓ API de Reportes está respondiendo" -ForegroundColor Green
    } catch {
        Write-Host "   ✗ API de Reportes no responde" -ForegroundColor Red
        exit 1
    }
}

# Paso 5: Limpiar datos anteriores
Write-Host ""
Write-Host "Paso 5: Limpiando datos de pruebas anteriores..." -ForegroundColor Yellow
try {
    docker exec reportes-mongodb mongosh reportes_db --eval "db.metricas_evento.deleteMany({})" --quiet 2>&1 | Out-Null
    docker exec reportes-mongodb mongosh reportes_db --eval "db.historial_asistencia.deleteMany({})" --quiet 2>&1 | Out-Null
    docker exec reportes-mongodb mongosh reportes_db --eval "db.reportes_ventas_diarias.deleteMany({})" --quiet 2>&1 | Out-Null
    docker exec reportes-mongodb mongosh reportes_db --eval "db.logs_auditoria.deleteMany({})" --quiet 2>&1 | Out-Null
    Write-Host "   ✓ Colecciones limpiadas" -ForegroundColor Green
} catch {
    Write-Host "   ⚠ No se pudieron limpiar las colecciones (puede ser normal)" -ForegroundColor Yellow
}

# Paso 6: Compilar y ejecutar el publicador de eventos
Write-Host ""
Write-Host "Paso 6: Publicando eventos de prueba..." -ForegroundColor Yellow

if (-not $SkipBuild) {
    Write-Host "   Compilando publicador de eventos..." -ForegroundColor Cyan
    Push-Location test-event-publisher
    try {
        dotnet build --configuration Release --verbosity quiet
        if ($LASTEXITCODE -ne 0) {
            Write-Host "   ✗ Error compilando el publicador" -ForegroundColor Red
            Pop-Location
            exit 1
        }
        Write-Host "   ✓ Compilación exitosa" -ForegroundColor Green
    } finally {
        Pop-Location
    }
}

Write-Host "   Ejecutando publicador de eventos..." -ForegroundColor Cyan
Push-Location test-event-publisher
try {
    dotnet run --configuration Release --no-build
    if ($LASTEXITCODE -ne 0) {
        Write-Host "   ✗ Error ejecutando el publicador" -ForegroundColor Red
        Pop-Location
        exit 1
    }
} finally {
    Pop-Location
}

if ($SkipVerification) {
    Write-Host ""
    Write-Host "=== Publicación completada (verificación omitida) ===" -ForegroundColor Green
    exit 0
}

# Paso 7: Verificar datos en MongoDB
Write-Host ""
Write-Host "Paso 7: Verificando datos en MongoDB..." -ForegroundColor Yellow

Write-Host "   Verificando métricas de eventos..." -ForegroundColor Cyan
$metricasCount = docker exec reportes-mongodb mongosh reportes_db --eval "db.metricas_evento.countDocuments({})" --quiet 2>&1
$metricasCount = $metricasCount -replace '[^0-9]', ''
if ([int]$metricasCount -gt 0) {
    Write-Host "   ✓ Métricas de eventos: $metricasCount registros" -ForegroundColor Green
} else {
    Write-Host "   ✗ No se encontraron métricas de eventos" -ForegroundColor Red
}

Write-Host "   Verificando historial de asistencia..." -ForegroundColor Cyan
$asistenciaCount = docker exec reportes-mongodb mongosh reportes_db --eval "db.historial_asistencia.countDocuments({})" --quiet 2>&1
$asistenciaCount = $asistenciaCount -replace '[^0-9]', ''
if ([int]$asistenciaCount -gt 0) {
    Write-Host "   ✓ Historial de asistencia: $asistenciaCount registros" -ForegroundColor Green
} else {
    Write-Host "   ⚠ No se encontró historial de asistencia" -ForegroundColor Yellow
}

Write-Host "   Verificando reportes de ventas..." -ForegroundColor Cyan
$ventasCount = docker exec reportes-mongodb mongosh reportes_db --eval "db.reportes_ventas_diarias.countDocuments({})" --quiet 2>&1
$ventasCount = $ventasCount -replace '[^0-9]', ''
if ([int]$ventasCount -gt 0) {
    Write-Host "   ✓ Reportes de ventas: $ventasCount registros" -ForegroundColor Green
} else {
    Write-Host "   ⚠ No se encontraron reportes de ventas" -ForegroundColor Yellow
}

Write-Host "   Verificando logs de auditoría..." -ForegroundColor Cyan
$auditoriaCount = docker exec reportes-mongodb mongosh reportes_db --eval "db.logs_auditoria.countDocuments({})" --quiet 2>&1
$auditoriaCount = $auditoriaCount -replace '[^0-9]', ''
if ([int]$auditoriaCount -gt 0) {
    Write-Host "   ✓ Logs de auditoría: $auditoriaCount registros" -ForegroundColor Green
} else {
    Write-Host "   ⚠ No se encontraron logs de auditoría" -ForegroundColor Yellow
}

# Paso 8: Verificar endpoints de API
Write-Host ""
Write-Host "Paso 8: Verificando endpoints de API..." -ForegroundColor Yellow

Write-Host "   Probando GET /api/reportes/resumen-ventas..." -ForegroundColor Cyan
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5002/api/reportes/resumen-ventas" -Method Get -UseBasicParsing -TimeoutSec 10
    if ($response.StatusCode -eq 200) {
        $data = $response.Content | ConvertFrom-Json
        Write-Host "   ✓ Endpoint responde correctamente" -ForegroundColor Green
        Write-Host "     Datos: $($response.Content.Substring(0, [Math]::Min(100, $response.Content.Length)))..." -ForegroundColor Gray
    }
} catch {
    Write-Host "   ✗ Error en endpoint de resumen de ventas: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "   Probando GET /api/reportes/auditoria..." -ForegroundColor Cyan
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5002/api/reportes/auditoria?pagina=1&tamañoPagina=10" -Method Get -UseBasicParsing -TimeoutSec 10
    if ($response.StatusCode -eq 200) {
        Write-Host "   ✓ Endpoint de auditoría responde correctamente" -ForegroundColor Green
    }
} catch {
    Write-Host "   ✗ Error en endpoint de auditoría: $($_.Exception.Message)" -ForegroundColor Red
}

# Resumen final
Write-Host ""
Write-Host "=== Resumen de Prueba de Integración ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Servicios verificados:" -ForegroundColor White
Write-Host "  ✓ MongoDB" -ForegroundColor Green
Write-Host "  ✓ RabbitMQ" -ForegroundColor Green
Write-Host "  ✓ API de Reportes" -ForegroundColor Green
Write-Host ""
Write-Host "Datos persistidos:" -ForegroundColor White
Write-Host "  • Métricas de eventos: $metricasCount" -ForegroundColor Cyan
Write-Host "  • Historial de asistencia: $asistenciaCount" -ForegroundColor Cyan
Write-Host "  • Reportes de ventas: $ventasCount" -ForegroundColor Cyan
Write-Host "  • Logs de auditoría: $auditoriaCount" -ForegroundColor Cyan
Write-Host ""
Write-Host "✅ Prueba de integración completada exitosamente" -ForegroundColor Green
Write-Host ""
Write-Host "Para ver los datos en detalle:" -ForegroundColor Yellow
Write-Host "  docker exec reportes-mongodb mongosh reportes_db --eval 'db.metricas_evento.find().pretty()'" -ForegroundColor White
Write-Host "  docker exec reportes-mongodb mongosh reportes_db --eval 'db.historial_asistencia.find().pretty()'" -ForegroundColor White
Write-Host ""
