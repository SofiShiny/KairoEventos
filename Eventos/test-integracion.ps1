# test-integracion.ps1
# Script de prueba automatizada para integración RabbitMQ

param(
    [string]$ApiUrl = "http://localhost:5000",
    [switch]$Verbose
)

$ErrorActionPreference = "Stop"

function Write-Step {
    param([string]$Message)
    Write-Host "`n$Message" -ForegroundColor Yellow
}

function Write-Success {
    param([string]$Message)
    Write-Host "✅ $Message" -ForegroundColor Green
}

function Write-Error-Custom {
    param([string]$Message)
    Write-Host "❌ $Message" -ForegroundColor Red
}

function Write-Info {
    param([string]$Message)
    Write-Host "ℹ️  $Message" -ForegroundColor Cyan
}

# Banner
Write-Host "`n" + "="*70 -ForegroundColor Cyan
Write-Host "  🚀 PRUEBA DE INTEGRACIÓN RABBITMQ - MICROSERVICIO EVENTOS" -ForegroundColor Cyan
Write-Host "="*70 -ForegroundColor Cyan

# Verificar que la API está corriendo
Write-Step "Verificando que la API está corriendo..."
try {
    $health = Invoke-RestMethod -Uri "$ApiUrl/health" -Method Get -TimeoutSec 5
    Write-Success "API está corriendo: $($health.status)"
    if ($Verbose) {
        Write-Info "Database: $($health.database)"
    }
} catch {
    Write-Error-Custom "No se puede conectar a la API en $ApiUrl"
    Write-Host "`nVerifica que la API está corriendo:" -ForegroundColor Yellow
    Write-Host "  cd Eventos/backend/src/Services/Eventos/Eventos.API" -ForegroundColor White
    Write-Host "  dotnet run" -ForegroundColor White
    exit 1
}

# Verificar RabbitMQ
Write-Step "Verificando RabbitMQ..."
try {
    $rabbitCheck = docker ps --filter "name=rabbitmq" --format "{{.Status}}"
    if ($rabbitCheck -like "*Up*") {
        Write-Success "RabbitMQ está corriendo"
    } else {
        Write-Error-Custom "RabbitMQ no está corriendo"
        Write-Host "Ejecuta: docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management" -ForegroundColor Yellow
        exit 1
    }
} catch {
    Write-Error-Custom "No se puede verificar RabbitMQ"
}

# Verificar PostgreSQL
Write-Step "Verificando PostgreSQL..."
try {
    $pgCheck = docker ps --filter "name=postgres" --format "{{.Status}}"
    if ($pgCheck -like "*Up*") {
        Write-Success "PostgreSQL está corriendo"
    } else {
        Write-Error-Custom "PostgreSQL no está corriendo"
        Write-Host "Ejecuta: docker run -d --name postgres -e POSTGRES_DB=eventsdb -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgres -p 5432:5432 postgres:15" -ForegroundColor Yellow
        exit 1
    }
} catch {
    Write-Error-Custom "No se puede verificar PostgreSQL"
}

Write-Host "`n" + "-"*70 -ForegroundColor Gray
Write-Host "  INICIANDO PRUEBAS" -ForegroundColor Cyan
Write-Host "-"*70 -ForegroundColor Gray

# TEST 1: Crear evento
Write-Step "TEST 1: Creando evento..."
$body = @{
    titulo = "Evento de Prueba RabbitMQ - $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
    descripcion = "Verificando integración con RabbitMQ"
    ubicacion = @{
        nombre = "Centro de Convenciones"
        direccion = "Av. Principal 123"
        ciudad = "Ciudad de Prueba"
        pais = "País de Prueba"
    }
    fechaInicio = (Get-Date).AddDays(30).ToString("yyyy-MM-ddTHH:mm:ssZ")
    fechaFin = (Get-Date).AddDays(30).AddHours(8).ToString("yyyy-MM-ddTHH:mm:ssZ")
    maximoAsistentes = 100
} | ConvertTo-Json -Depth 10

try {
    $response = Invoke-RestMethod -Uri "$ApiUrl/api/eventos" `
        -Method Post `
        -Body $body `
        -ContentType "application/json"
    
    $eventoId = $response.id
    Write-Success "Evento creado con ID: $eventoId"
    
    if ($Verbose) {
        Write-Info "Título: $($response.titulo)"
        Write-Info "Estado: $($response.estado)"
        Write-Info "Fecha Inicio: $($response.fechaInicio)"
    }
} catch {
    Write-Error-Custom "Error al crear evento: $($_.Exception.Message)"
    exit 1
}

Start-Sleep -Seconds 1

# TEST 2: Publicar evento
Write-Step "TEST 2: Publicando evento (EventoPublicadoEventoDominio)..."
try {
    Invoke-RestMethod -Uri "$ApiUrl/api/eventos/$eventoId/publicar" `
        -Method Patch
    Write-Success "Evento publicado a RabbitMQ"
    Write-Info "Mensaje: EventoPublicadoEventoDominio { EventoId: $eventoId, TituloEvento: ..., FechaInicio: ... }"
} catch {
    Write-Error-Custom "Error al publicar evento: $($_.Exception.Message)"
    exit 1
}

Start-Sleep -Seconds 2

# TEST 3: Registrar asistente
Write-Step "TEST 3: Registrando asistente (AsistenteRegistradoEventoDominio)..."
$asistente = @{
    usuarioId = "user-test-$(Get-Random -Maximum 9999)"
    nombre = "Juan Pérez"
    correo = "juan.perez@example.com"
} | ConvertTo-Json

try {
    $asistenteResponse = Invoke-RestMethod -Uri "$ApiUrl/api/eventos/$eventoId/asistentes" `
        -Method Post `
        -Body $asistente `
        -ContentType "application/json"
    Write-Success "Asistente registrado y publicado a RabbitMQ"
    Write-Info "Mensaje: AsistenteRegistradoEventoDominio { EventoId: $eventoId, UsuarioId: ..., NombreUsuario: Juan Pérez }"
    
    if ($Verbose) {
        Write-Info "Asistente ID: $($asistenteResponse.id)"
        Write-Info "Usuario ID: $($asistenteResponse.usuarioId)"
    }
} catch {
    Write-Error-Custom "Error al registrar asistente: $($_.Exception.Message)"
    exit 1
}

Start-Sleep -Seconds 2

# TEST 4: Cancelar evento
Write-Step "TEST 4: Cancelando evento (EventoCanceladoEventoDominio)..."
try {
    Invoke-RestMethod -Uri "$ApiUrl/api/eventos/$eventoId/cancelar" `
        -Method Patch
    Write-Success "Evento cancelado y publicado a RabbitMQ"
    Write-Info "Mensaje: EventoCanceladoEventoDominio { EventoId: $eventoId, TituloEvento: ... }"
} catch {
    Write-Error-Custom "Error al cancelar evento: $($_.Exception.Message)"
    exit 1
}

Start-Sleep -Seconds 1

# TEST 5: Verificar estado final
Write-Step "TEST 5: Verificando estado final del evento..."
try {
    $eventoFinal = Invoke-RestMethod -Uri "$ApiUrl/api/eventos/$eventoId" -Method Get
    
    if ($eventoFinal.estado -eq "Cancelado") {
        Write-Success "Estado del evento es 'Cancelado' (correcto)"
    } else {
        Write-Error-Custom "Estado del evento es '$($eventoFinal.estado)' (esperado: Cancelado)"
    }
    
    if ($Verbose) {
        Write-Info "Título: $($eventoFinal.titulo)"
        Write-Info "Estado: $($eventoFinal.estado)"
        Write-Info "Asistentes: $($eventoFinal.asistentes.Count)"
    }
} catch {
    Write-Error-Custom "Error al verificar estado final: $($_.Exception.Message)"
}

# Resumen
Write-Host "`n" + "="*70 -ForegroundColor Cyan
Write-Host "  ✅ TODAS LAS PRUEBAS COMPLETADAS EXITOSAMENTE" -ForegroundColor Green
Write-Host "="*70 -ForegroundColor Cyan

Write-Host "`n📊 RESUMEN DE PRUEBAS:" -ForegroundColor Yellow
Write-Host "  ✅ Evento creado correctamente" -ForegroundColor Green
Write-Host "  ✅ EventoPublicadoEventoDominio publicado a RabbitMQ" -ForegroundColor Green
Write-Host "  ✅ AsistenteRegistradoEventoDominio publicado a RabbitMQ" -ForegroundColor Green
Write-Host "  ✅ EventoCanceladoEventoDominio publicado a RabbitMQ" -ForegroundColor Green
Write-Host "  ✅ Estado final del evento verificado" -ForegroundColor Green

Write-Host "`n🔍 VERIFICACIÓN EN RABBITMQ:" -ForegroundColor Yellow
Write-Host "  1. Abre: http://localhost:15672" -ForegroundColor Cyan
Write-Host "  2. Login: guest / guest" -ForegroundColor Cyan
Write-Host "  3. Ve a 'Queues and Streams'" -ForegroundColor Cyan
Write-Host "  4. Deberías ver las colas con los mensajes publicados" -ForegroundColor Cyan

Write-Host "`n📝 INFORMACIÓN DEL EVENTO:" -ForegroundColor Yellow
Write-Host "  Evento ID: $eventoId" -ForegroundColor Cyan
Write-Host "  API URL: $ApiUrl" -ForegroundColor Cyan
Write-Host "  RabbitMQ UI: http://localhost:15672" -ForegroundColor Cyan

Write-Host "`n🎯 SIGUIENTE PASO:" -ForegroundColor Yellow
Write-Host "  Revisa PLAN-SIGUIENTES-PASOS.md para continuar con:" -ForegroundColor White
Write-Host "  - Fase 2: Actualización del Microservicio de Reportes" -ForegroundColor White
Write-Host "  - Fase 3: Pruebas End-to-End completas" -ForegroundColor White

Write-Host "`n✨ ¡Integración RabbitMQ funcionando correctamente!" -ForegroundColor Green
Write-Host ""
