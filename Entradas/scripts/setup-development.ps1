# Script de configuración para desarrollo - Entradas.API
# PowerShell script para Windows

param(
    [switch]$Clean,
    [switch]$Seed,
    [switch]$Logs
)

Write-Host "=== Setup de Desarrollo - Entradas.API ===" -ForegroundColor Green

# Función para verificar si Docker está ejecutándose
function Test-DockerRunning {
    try {
        docker info | Out-Null
        return $true
    }
    catch {
        Write-Host "Error: Docker no está ejecutándose" -ForegroundColor Red
        return $false
    }
}

# Función para limpiar contenedores y volúmenes
function Clear-Environment {
    Write-Host "Limpiando entorno..." -ForegroundColor Yellow
    
    docker-compose -f docker-compose.yml -f docker-compose.override.yml down -v
    docker system prune -f
    
    # Limpiar logs locales
    if (Test-Path "logs") {
        Remove-Item -Path "logs\*" -Recurse -Force
        Write-Host "Logs locales limpiados" -ForegroundColor Green
    }
}

# Función para iniciar servicios
function Start-Services {
    Write-Host "Iniciando servicios..." -ForegroundColor Yellow
    
    # Crear directorios necesarios
    if (!(Test-Path "logs")) { New-Item -ItemType Directory -Path "logs" }
    if (!(Test-Path "mocks\eventos")) { New-Item -ItemType Directory -Path "mocks\eventos" -Force }
    if (!(Test-Path "mocks\asientos")) { New-Item -ItemType Directory -Path "mocks\asientos" -Force }
    
    # Iniciar servicios de infraestructura primero
    Write-Host "Iniciando PostgreSQL y RabbitMQ..." -ForegroundColor Cyan
    docker-compose up -d postgres rabbitmq
    
    # Esperar a que los servicios estén listos
    Write-Host "Esperando a que los servicios estén listos..." -ForegroundColor Cyan
    Start-Sleep -Seconds 30
    
    # Iniciar mocks
    Write-Host "Iniciando servicios mock..." -ForegroundColor Cyan
    docker-compose up -d eventos-api-mock asientos-api-mock
    
    # Iniciar aplicación
    Write-Host "Iniciando Entradas.API..." -ForegroundColor Cyan
    docker-compose up -d entradas-api
    
    # Iniciar herramientas de desarrollo
    Write-Host "Iniciando herramientas de desarrollo..." -ForegroundColor Cyan
    docker-compose up -d pgadmin
}

# Función para verificar el estado de los servicios
function Test-Services {
    Write-Host "Verificando estado de servicios..." -ForegroundColor Yellow
    
    $services = @("postgres", "rabbitmq", "entradas-api")
    
    foreach ($service in $services) {
        $status = docker-compose ps -q $service
        if ($status) {
            Write-Host "✓ $service está ejecutándose" -ForegroundColor Green
        } else {
            Write-Host "✗ $service no está ejecutándose" -ForegroundColor Red
        }
    }
    
    # Verificar health checks
    Write-Host "`nVerificando health checks..." -ForegroundColor Yellow
    Start-Sleep -Seconds 10
    
    try {
        $healthCheck = Invoke-RestMethod -Uri "http://localhost:8080/health" -TimeoutSec 10
        Write-Host "✓ API Health Check: OK" -ForegroundColor Green
    }
    catch {
        Write-Host "✗ API Health Check: FAILED" -ForegroundColor Red
    }
}

# Función para insertar datos de prueba
function Add-SeedData {
    Write-Host "Insertando datos de prueba..." -ForegroundColor Yellow
    
    # Esperar a que la aplicación esté lista
    Start-Sleep -Seconds 20
    
    # Ejecutar función de seed data
    $seedCommand = "SELECT entradas.insert_development_data();"
    docker exec entradas-postgres psql -U entradas_user -d entradas_db -c $seedCommand
    
    Write-Host "Datos de prueba insertados" -ForegroundColor Green
}

# Función para mostrar logs
function Show-Logs {
    Write-Host "Mostrando logs de la aplicación..." -ForegroundColor Yellow
    docker-compose logs -f entradas-api
}

# Verificar Docker
if (!(Test-DockerRunning)) {
    exit 1
}

# Ejecutar acciones según parámetros
if ($Clean) {
    Clear-Environment
}

Start-Services

Write-Host "`n=== Servicios iniciados ===" -ForegroundColor Green
Write-Host "API: http://localhost:8080" -ForegroundColor Cyan
Write-Host "Swagger: http://localhost:8080/swagger" -ForegroundColor Cyan
Write-Host "RabbitMQ Management: http://localhost:15672" -ForegroundColor Cyan
Write-Host "PgAdmin: http://localhost:8090" -ForegroundColor Cyan

Test-Services

if ($Seed) {
    Add-SeedData
}

if ($Logs) {
    Show-Logs
} else {
    Write-Host "`nUse -Logs para ver los logs en tiempo real" -ForegroundColor Yellow
    Write-Host "Use docker-compose logs -f [servicio] para logs específicos" -ForegroundColor Yellow
}

Write-Host "`n=== Setup completado ===" -ForegroundColor Green