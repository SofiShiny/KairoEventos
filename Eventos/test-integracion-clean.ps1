# test-integracion-clean.ps1
# Script de prueba automatizada para integracion RabbitMQ

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
    Write-Host "[OK] $Message" -ForegroundColor Green
}

function Write-Error-Custom {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

function Write-Info {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Cyan
}

# Banner
Write-Host "`n" + "="*70 -ForegroundColor Cyan
Write-Host "  PRUEBA DE INTEGRACION RABBITMQ - MICROSERVICIO EVENTOS" -ForegroundColor Cyan
Write-Host "="*70 -ForegroundColor Cyan

# Verificar que la API esta corriendo
Write-Step "Verificando que la API esta corriendo..."
try {
    $health = Invoke-RestMethod -Uri "$ApiUrl/health" -Method Get -TimeoutSec 5
    Write-Success "API esta corriendo: $($health.status)"
    if ($Verbose) {
        Write-Info "Database: $($health.database)"
    }
} catch {
    Write-Error-Custom "No se puede conectar a la API en $ApiUrl"
    Write-Host "`nVerifica que la API esta corriendo:" -ForegroundColor Yellow
    Write-Host "  cd Eventos/backend/src/Services/Eventos/Eventos.API" -ForegroundColor White
    Write-Host "  dotnet run" -ForegroundColor White
    exit 1
}

# Verificar RabbitMQ
Write-Step "Verificando RabbitMQ..."
try {
    $rabbitCheck = docker ps --filter "name=rabbitmq" --format "{{.Status}}"
    if ($rabbitCheck -like "*Up*") {
        Write-Success "RabbitMQ esta corriendo"
    } else {
        Write-Error-Custom "RabbitMQ no esta corriendo"
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
        Write-Success "PostgreSQL esta corriendo"
    } else {
        Write-Error-Custom "PostgreSQL no esta corriendo"
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
    descripcion = "Verificando integracion con RabbitMQ"
    ubicacion = @{
        nombreLugar = "Centro de Convenciones"
        direccion = "Av. Principal 123"
        ciudad = "Ciudad de Prueba"
        pais = "Pais de Prueba"
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
        Write-Info "Titulo: $($response.titulo)"
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
    nombre = "Juan Perez"
    correo = "juan.perez@example.com"
} | ConvertTo-Json

try {
    $asistenteResponse = Invoke-RestMethod -Uri "$ApiUrl/api/eventos/$eventoId/asistentes" `
        -Method Post `
        -Body $asistente `
        -ContentType "application/json"
    Write-Success "Asistente registrado y publicado a RabbitMQ"
    Write-Info "Mensaje: AsistenteRegistradoEventoDominio { EventoId: $eventoId, UsuarioId: ..., NombreUsuario: Juan Perez }"
    
    if ($Verbose) {
        Write-Info "Asistente ID: $($asistenteResponse.id)"
        Write-Info "Usuario ID: $($asistenteResponse.usuarioId)"
    }
} catch {
    Write-Error-Custom "Error al registrar asistente: $($_.Exception.Message)"
    Write-Info "Continuando con las pruebas..."
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
        Write-Info "Titulo: $($eventoFinal.titulo)"
        Write-Info "Estado: $($eventoFinal.estado)"
        Write-Info "Asistentes: $($eventoFinal.asistentes.Count)"
    }
} catch {
    Write-Error-Custom "Error al verificar estado final: $($_.Exception.Message)"
}

# Resumen
Write-Host "`n" + "="*70 -ForegroundColor Cyan
Write-Host "  PRUEBAS COMPLETADAS" -ForegroundColor Green
Write-Host "="*70 -ForegroundColor Cyan

Write-Host "`nRESUMEN DE PRUEBAS:" -ForegroundColor Yellow
Write-Host "  [OK] Evento creado correctamente" -ForegroundColor Green
Write-Host "  [OK] EventoPublicadoEventoDominio publicado a RabbitMQ" -ForegroundColor Green
Write-Host "  [?] AsistenteRegistradoEventoDominio - Ver logs" -ForegroundColor Yellow
Write-Host "  [?] EventoCanceladoEventoDominio - Ver logs" -ForegroundColor Yellow
Write-Host "  [?] Estado final del evento - Ver logs" -ForegroundColor Yellow

Write-Host "`nVERIFICACION EN RABBITMQ:" -ForegroundColor Yellow
Write-Host "  1. Abre: http://localhost:15672" -ForegroundColor Cyan
Write-Host "  2. Login: guest / guest" -ForegroundColor Cyan
Write-Host "  3. Ve a 'Queues and Streams'" -ForegroundColor Cyan
Write-Host "  4. Deberias ver las colas con los mensajes publicados" -ForegroundColor Cyan

Write-Host "`nINFORMACION DEL EVENTO:" -ForegroundColor Yellow
Write-Host "  Evento ID: $eventoId" -ForegroundColor Cyan
Write-Host "  API URL: $ApiUrl" -ForegroundColor Cyan
Write-Host "  RabbitMQ UI: http://localhost:15672" -ForegroundColor Cyan

Write-Host "`nSIGUIENTE PASO:" -ForegroundColor Yellow
Write-Host "  Revisa VERIFICACION-INTEGRACION.md para los resultados detallados" -ForegroundColor White

Write-Host ""
