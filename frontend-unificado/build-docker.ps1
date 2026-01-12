# Build script for Frontend Unificado Docker image
# Usage: .\build-docker.ps1 [environment]
# Example: .\build-docker.ps1 production

param(
    [string]$Environment = "production"
)

$ErrorActionPreference = "Stop"

$ImageName = "frontend-unificado"
$Timestamp = Get-Date -Format "yyyyMMdd-HHmmss"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Building Frontend Unificado Docker Image" -ForegroundColor Cyan
Write-Host "Environment: $Environment" -ForegroundColor Cyan
Write-Host "Timestamp: $Timestamp" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Determine environment file and tag
if ($Environment -eq "production") {
    $EnvFile = ".env.production"
    $Tag = "latest"
} elseif ($Environment -eq "development") {
    $EnvFile = ".env.development"
    $Tag = "dev"
} else {
    Write-Host "Error: Unknown environment '$Environment'" -ForegroundColor Red
    Write-Host "Valid options: production, development" -ForegroundColor Yellow
    exit 1
}

# Check if env file exists
if (-not (Test-Path $EnvFile)) {
    Write-Host "Error: Environment file '$EnvFile' not found" -ForegroundColor Red
    exit 1
}

Write-Host "Using environment file: $EnvFile" -ForegroundColor Green
Write-Host ""

# Build the Docker image
Write-Host "Building Docker image..." -ForegroundColor Yellow
docker build `
    -t "${ImageName}:${Tag}" `
    -t "${ImageName}:${Tag}-${Timestamp}" `
    -f Dockerfile `
    .

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Docker build failed" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "==========================================" -ForegroundColor Green
Write-Host "Build completed successfully!" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green
Write-Host "Image tags:" -ForegroundColor Cyan
Write-Host "  - ${ImageName}:${Tag}" -ForegroundColor White
Write-Host "  - ${ImageName}:${Tag}-${Timestamp}" -ForegroundColor White
Write-Host ""
Write-Host "Image size:" -ForegroundColor Cyan
docker images ${ImageName}:${Tag} --format "table {{.Repository}}`t{{.Tag}}`t{{.Size}}"
Write-Host ""
Write-Host "To run the container:" -ForegroundColor Cyan
Write-Host "  docker-compose up -d" -ForegroundColor White
Write-Host ""
Write-Host "To push to registry:" -ForegroundColor Cyan
Write-Host "  docker push ${ImageName}:${Tag}" -ForegroundColor White
Write-Host "==========================================" -ForegroundColor Green
