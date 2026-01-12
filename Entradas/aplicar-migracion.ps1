# =====================================================
# Script para aplicar la migración AgregarCamposSnapshot
# =====================================================

Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "  MIGRACIÓN: Agregar Campos Snapshot" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host ""

# Verificar si Docker está corriendo
Write-Host "1. Verificando Docker..." -ForegroundColor Yellow
$dockerRunning = docker info 2>$null
if (-not $dockerRunning) {
    Write-Host "   ❌ Docker no está corriendo. Por favor inicia Docker Desktop." -ForegroundColor Red
    exit 1
}
Write-Host "   ✅ Docker está corriendo" -ForegroundColor Green

# Verificar si el contenedor ya existe
Write-Host ""
Write-Host "2. Verificando contenedor PostgreSQL..." -ForegroundColor Yellow
$containerExists = docker ps -a --filter "name=entradas-postgres" --format "{{.Names}}"

if ($containerExists) {
    Write-Host "   ⚠️  Contenedor 'entradas-postgres' ya existe" -ForegroundColor Yellow
    $remove = Read-Host "   ¿Deseas eliminarlo y crear uno nuevo? (s/n)"
    if ($remove -eq 's' -or $remove -eq 'S') {
        Write-Host "   Eliminando contenedor existente..." -ForegroundColor Yellow
        docker rm -f entradas-postgres
        Write-Host "   ✅ Contenedor eliminado" -ForegroundColor Green
    }
}

# Crear y arrancar PostgreSQL
Write-Host ""
Write-Host "3. Creando contenedor PostgreSQL..." -ForegroundColor Yellow
docker run --name entradas-postgres -d `
    -e POSTGRES_DB=entradas_db `
    -e POSTGRES_USER=entradas_user `
    -e POSTGRES_PASSWORD=entradas_pass `
    -p 5432:5432 `
    postgres:15

if ($LASTEXITCODE -ne 0) {
    Write-Host "   ❌ Error al crear el contenedor" -ForegroundColor Red
    exit 1
}
Write-Host "   ✅ Contenedor creado" -ForegroundColor Green

# Esperar a que PostgreSQL esté listo
Write-Host ""
Write-Host "4. Esperando a que PostgreSQL esté listo..." -ForegroundColor Yellow
$maxAttempts = 30
$attempt = 0
$ready = $false

while ($attempt -lt $maxAttempts -and -not $ready) {
    $attempt++
    Write-Host "   Intento $attempt/$maxAttempts..." -ForegroundColor Gray
    
    $result = docker exec entradas-postgres pg_isready -U entradas_user 2>$null
    if ($result -match "accepting connections") {
        $ready = $true
        Write-Host "   ✅ PostgreSQL está listo" -ForegroundColor Green
    } else {
        Start-Sleep -Seconds 2
    }
}

if (-not $ready) {
    Write-Host "   ❌ PostgreSQL no respondió a tiempo" -ForegroundColor Red
    exit 1
}

# Aplicar migración
Write-Host ""
Write-Host "5. Aplicando migración..." -ForegroundColor Yellow
Write-Host ""

$migrationResult = dotnet ef database update `
    --project Entradas.Infraestructura `
    --startup-project Entradas.API `
    --context EntradasDbContext 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "   ✅ Migración aplicada exitosamente" -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "   ❌ Error al aplicar la migración" -ForegroundColor Red
    Write-Host "   Detalles: $migrationResult" -ForegroundColor Red
    Write-Host ""
    Write-Host "   Puedes aplicar el SQL manualmente desde:" -ForegroundColor Yellow
    Write-Host "   Entradas.Infraestructura\Persistencia\Migrations\20260106024629_AgregarCamposSnapshot.sql" -ForegroundColor Cyan
    exit 1
}

# Verificar columnas
Write-Host ""
Write-Host "6. Verificando columnas agregadas..." -ForegroundColor Yellow

$verifyQuery = @"
SELECT column_name, data_type, character_maximum_length 
FROM information_schema.columns 
WHERE table_schema = 'entradas' 
  AND table_name = 'entradas' 
  AND column_name IN ('titulo_evento', 'imagen_evento_url', 'fecha_evento', 'nombre_sector', 'fila', 'numero_asiento')
ORDER BY column_name;
"@

docker exec -i entradas-postgres psql -U entradas_user -d entradas_db -c "$verifyQuery"

Write-Host ""
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "  ✅ MIGRACIÓN COMPLETADA" -ForegroundColor Green
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Información del contenedor:" -ForegroundColor Yellow
Write-Host "  Nombre: entradas-postgres" -ForegroundColor White
Write-Host "  Puerto: 5432" -ForegroundColor White
Write-Host "  Base de datos: entradas_db" -ForegroundColor White
Write-Host "  Usuario: entradas_user" -ForegroundColor White
Write-Host "  Password: entradas_pass" -ForegroundColor White
Write-Host ""
Write-Host "Para detener el contenedor:" -ForegroundColor Yellow
Write-Host "  docker stop entradas-postgres" -ForegroundColor Cyan
Write-Host ""
Write-Host "Para eliminar el contenedor:" -ForegroundColor Yellow
Write-Host "  docker rm -f entradas-postgres" -ForegroundColor Cyan
Write-Host ""
