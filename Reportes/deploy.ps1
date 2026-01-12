###############################################################################
# Script de Deployment - Microservicio de Reportes (PowerShell)
# 
# Este script automatiza el proceso de build y deployment del microservicio
# Soporta múltiples ambientes: development, staging, production
#
# Uso:
#   .\deploy.ps1 -Ambiente <ambiente> [opciones]
#
# Ejemplos:
#   .\deploy.ps1 -Ambiente development
#   .\deploy.ps1 -Ambiente production -SkipTests
#   .\deploy.ps1 -Ambiente staging -BuildOnly
###############################################################################

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("development", "staging", "production")]
    [string]$Ambiente = "development",
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipTests,
    
    [Parameter(Mandatory=$false)]
    [switch]$BuildOnly,
    
    [Parameter(Mandatory=$false)]
    [string]$DockerRegistry = "",
    
    [Parameter(Mandatory=$false)]
    [string]$ImageTag = "latest"
)

# Configuración de colores
$ErrorActionPreference = "Stop"

# Funciones de utilidad
function Write-Info {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Blue
}

function Write-Success {
    param([string]$Message)
    Write-Host "[SUCCESS] $Message" -ForegroundColor Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "[WARNING] $Message" -ForegroundColor Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

# Validar ambiente
function Validate-Environment {
    Write-Info "Validando ambiente: $Ambiente"
    Write-Success "Ambiente válido: $Ambiente"
}

# Cargar variables de entorno según ambiente
function Load-EnvironmentVariables {
    Write-Info "Cargando variables de entorno para: $Ambiente"
    
    $envFile = ".env.$Ambiente"
    
    if (Test-Path $envFile) {
        Get-Content $envFile | ForEach-Object {
            if ($_ -match '^([^=]+)=(.*)$') {
                $name = $matches[1]
                $value = $matches[2]
                [Environment]::SetEnvironmentVariable($name, $value, "Process")
            }
        }
        Write-Success "Variables de entorno cargadas desde $envFile"
    }
    else {
        Write-Warning "Archivo $envFile no encontrado, usando valores por defecto"
    }
    
    # Variables por defecto si no están definidas
    if (-not $env:MONGODB_CONNECTION_STRING) {
        $env:MONGODB_CONNECTION_STRING = "mongodb://localhost:27017"
    }
    if (-not $env:MONGODB_DATABASE) {
        $env:MONGODB_DATABASE = "reportes_db"
    }
    if (-not $env:RABBITMQ_HOST) {
        $env:RABBITMQ_HOST = "localhost"
    }
    if (-not $env:RABBITMQ_PORT) {
        $env:RABBITMQ_PORT = "5672"
    }
    if (-not $env:RABBITMQ_USER) {
        $env:RABBITMQ_USER = "guest"
    }
    if (-not $env:RABBITMQ_PASSWORD) {
        $env:RABBITMQ_PASSWORD = "guest"
    }
    if (-not $env:ASPNETCORE_ENVIRONMENT) {
        $env:ASPNETCORE_ENVIRONMENT = "Development"
    }
}

# Ejecutar tests
function Run-Tests {
    if ($SkipTests) {
        Write-Warning "Saltando tests (-SkipTests especificado)"
        return
    }
    
    Write-Info "Ejecutando tests..."
    
    Push-Location "backend\src\Services\Reportes"
    
    try {
        # Restaurar dependencias
        dotnet restore
        
        # Ejecutar tests con cobertura
        dotnet test `
            --configuration Release `
            --no-restore `
            /p:CollectCoverage=true `
            /p:CoverletOutputFormat=cobertura `
            /p:Threshold=80 `
            /p:ThresholdType=line
        
        if ($LASTEXITCODE -eq 0) {
            Write-Success "Tests ejecutados exitosamente"
        }
        else {
            throw "Tests fallaron"
        }
    }
    finally {
        Pop-Location
    }
}

# Build de la aplicación
function Build-Application {
    Write-Info "Construyendo aplicación..."
    
    Push-Location "backend\src\Services\Reportes\Reportes.API"
    
    try {
        # Limpiar builds anteriores
        dotnet clean --configuration Release
        
        # Build
        dotnet build `
            --configuration Release `
            --no-restore
        
        if ($LASTEXITCODE -eq 0) {
            Write-Success "Build completado exitosamente"
        }
        else {
            throw "Build falló"
        }
    }
    finally {
        Pop-Location
    }
}

# Build de imagen Docker
function Build-DockerImage {
    Write-Info "Construyendo imagen Docker..."
    
    $imageName = "reportes-api"
    $fullImageName = "${imageName}:${ImageTag}"
    
    if ($DockerRegistry) {
        $fullImageName = "${DockerRegistry}/${fullImageName}"
    }
    
    docker build `
        -t $fullImageName `
        -f Dockerfile `
        --build-arg ASPNETCORE_ENVIRONMENT=$env:ASPNETCORE_ENVIRONMENT `
        .
    
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Imagen Docker construida: $fullImageName"
    }
    else {
        throw "Build de imagen Docker falló"
    }
    
    # Tag adicional con timestamp para versionado
    $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $versionedImage = "${imageName}:${Ambiente}-${timestamp}"
    
    if ($DockerRegistry) {
        $versionedImage = "${DockerRegistry}/${versionedImage}"
    }
    
    docker tag $fullImageName $versionedImage
    Write-Success "Imagen taggeada: $versionedImage"
}

# Push de imagen a registry
function Push-DockerImage {
    if (-not $DockerRegistry) {
        Write-Warning "No se especificó registry, saltando push"
        return
    }
    
    Write-Info "Pusheando imagen a registry..."
    
    $imageName = "reportes-api"
    $fullImageName = "${DockerRegistry}/${imageName}:${ImageTag}"
    
    docker push $fullImageName
    
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Imagen pusheada exitosamente"
    }
    else {
        throw "Push de imagen falló"
    }
}

# Deploy con docker-compose
function Deploy-WithCompose {
    if ($BuildOnly) {
        Write-Warning "Modo build-only, saltando deployment"
        return
    }
    
    Write-Info "Desplegando con docker-compose..."
    
    # Detener servicios existentes
    docker-compose down
    
    # Levantar servicios
    docker-compose up -d --build
    
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Servicios desplegados exitosamente"
    }
    else {
        throw "Deployment falló"
    }
    
    # Esperar a que los servicios estén listos
    Write-Info "Esperando a que los servicios estén listos..."
    Start-Sleep -Seconds 10
    
    # Verificar health checks
    Check-Health
}

# Verificar health checks
function Check-Health {
    Write-Info "Verificando health checks..."
    
    $maxRetries = 30
    $retryCount = 0
    
    while ($retryCount -lt $maxRetries) {
        try {
            $response = Invoke-WebRequest -Uri "http://localhost:5002/health" -UseBasicParsing -TimeoutSec 2
            
            if ($response.StatusCode -eq 200) {
                Write-Success "Health check OK"
                return
            }
        }
        catch {
            # Ignorar errores y reintentar
        }
        
        $retryCount++
        Write-Info "Esperando health check... ($retryCount/$maxRetries)"
        Start-Sleep -Seconds 2
    }
    
    throw "Health check falló después de $maxRetries intentos"
}

# Mostrar información de deployment
function Show-DeploymentInfo {
    Write-Success "==================================="
    Write-Success "Deployment completado exitosamente"
    Write-Success "==================================="
    Write-Host ""
    Write-Info "Ambiente: $Ambiente"
    Write-Info "Imagen: reportes-api:$ImageTag"
    Write-Host ""
    Write-Info "Servicios disponibles:"
    Write-Info "  - API: http://localhost:5002"
    Write-Info "  - Swagger: http://localhost:5002/swagger"
    Write-Info "  - Health: http://localhost:5002/health"
    Write-Info "  - Hangfire: http://localhost:5002/hangfire"
    Write-Info "  - RabbitMQ Management: http://localhost:15672"
    Write-Host ""
    Write-Info "Para ver logs:"
    Write-Info "  docker-compose logs -f reportes-api"
    Write-Host ""
}

# Main
function Main {
    Write-Info "==================================="
    Write-Info "Iniciando deployment"
    Write-Info "==================================="
    Write-Host ""
    
    try {
        Validate-Environment
        Load-EnvironmentVariables
        
        # Ejecutar pasos de deployment
        Run-Tests
        Build-Application
        Build-DockerImage
        Push-DockerImage
        Deploy-WithCompose
        
        Show-DeploymentInfo
    }
    catch {
        Write-Error "Deployment falló: $_"
        exit 1
    }
}

# Ejecutar main
Main
