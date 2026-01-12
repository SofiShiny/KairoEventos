# Script para ejecutar pruebas de integración del Gateway
# Este script inicia los servicios necesarios y ejecuta las pruebas

Write-Host "=== Gateway Integration Tests Runner ===" -ForegroundColor Cyan
Write-Host ""

# Paso 1: Verificar que Docker está corriendo
Write-Host "1. Verificando Docker..." -ForegroundColor Yellow
$dockerRunning = docker info 2>&1 | Select-String "Server Version"
if (-not $dockerRunning) {
    Write-Host "ERROR: Docker no está corriendo. Por favor inicia Docker Desktop." -ForegroundColor Red
    exit 1
}
Write-Host "   ✓ Docker está corriendo" -ForegroundColor Green
Write-Host ""

# Paso 2: Iniciar infraestructura
Write-Host "2. Iniciando servicios de infraestructura..." -ForegroundColor Yellow
Push-Location ..\Infraestructura
docker-compose up -d postgres keycloak
Pop-Location
Write-Host "   ✓ Servicios iniciados" -ForegroundColor Green
Write-Host ""

# Paso 3: Esperar a que Keycloak esté listo
Write-Host "3. Esperando a que Keycloak esté listo..." -ForegroundColor Yellow
$maxAttempts = 30
$attempt = 0
$keycloakReady = $false

while ($attempt -lt $maxAttempts -and -not $keycloakReady) {
    $attempt++
    Write-Host "   Intento $attempt/$maxAttempts..." -NoNewline
    
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:8180/health/ready" -TimeoutSec 2 -ErrorAction SilentlyContinue
        if ($response.StatusCode -eq 200) {
            $keycloakReady = $true
            Write-Host " ✓" -ForegroundColor Green
        } else {
            Write-Host " ✗" -ForegroundColor Red
            Start-Sleep -Seconds 2
        }
    } catch {
        Write-Host " ✗" -ForegroundColor Red
        Start-Sleep -Seconds 2
    }
}

if (-not $keycloakReady) {
    Write-Host "ERROR: Keycloak no está listo después de $maxAttempts intentos" -ForegroundColor Red
    Write-Host "Verifica los logs: docker logs kairo-keycloak" -ForegroundColor Yellow
    exit 1
}
Write-Host "   ✓ Keycloak está listo" -ForegroundColor Green
Write-Host ""

# Paso 4: Ejecutar pruebas de integración
Write-Host "4. Ejecutando pruebas de integración..." -ForegroundColor Yellow
Push-Location src\Gateway.API
dotnet test Gateway.sln --filter "Category=Integration" --logger "console;verbosity=normal"
$testResult = $LASTEXITCODE
Pop-Location
Write-Host ""

# Paso 5: Mostrar resultado
if ($testResult -eq 0) {
    Write-Host "=== PRUEBAS DE INTEGRACIÓN EXITOSAS ===" -ForegroundColor Green
} else {
    Write-Host "=== ALGUNAS PRUEBAS FALLARON ===" -ForegroundColor Red
    Write-Host "Revisa los logs arriba para más detalles" -ForegroundColor Yellow
}
Write-Host ""

# Paso 6: Preguntar si detener servicios
$stopServices = Read-Host "¿Deseas detener los servicios? (s/n)"
if ($stopServices -eq "s" -or $stopServices -eq "S") {
    Write-Host "Deteniendo servicios..." -ForegroundColor Yellow
    Push-Location ..\Infraestructura
    docker-compose down
    Pop-Location
    Write-Host "   ✓ Servicios detenidos" -ForegroundColor Green
}

Write-Host ""
Write-Host "Script completado" -ForegroundColor Cyan
exit $testResult
