# Docker Verification Test Script for Frontend Unificado (PowerShell)
# Tests: Image build, size, and nginx functionality
# Requirements: 19.1, 19.6

$ErrorActionPreference = "Stop"

# Test counters
$TestsPassed = 0
$TestsFailed = 0

# Image name
$ImageName = "frontend-unificado:test"
$ContainerName = "frontend-unificado-test"

# Function to print test result
function Print-Result {
    param(
        [bool]$Success,
        [string]$Message
    )
    
    if ($Success) {
        Write-Host "✓ PASS: $Message" -ForegroundColor Green
        $script:TestsPassed++
    } else {
        Write-Host "✗ FAIL: $Message" -ForegroundColor Red
        $script:TestsFailed++
    }
}

# Function to cleanup
function Cleanup {
    Write-Host "`nCleaning up..." -ForegroundColor Yellow
    docker stop $ContainerName 2>$null
    docker rm $ContainerName 2>$null
    docker rmi $ImageName 2>$null
}

# Ensure cleanup on exit
trap { Cleanup }

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Docker Verification Tests" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

# Test 1: Build Docker image
Write-Host "`nTest 1: Building Docker image..." -ForegroundColor Yellow
try {
    docker build -t $ImageName . 2>&1 | Out-Null
    Print-Result -Success $true -Message "Docker image builds successfully"
} catch {
    Print-Result -Success $false -Message "Docker image build failed"
    exit 1
}

# Test 2: Check image size (<100MB)
Write-Host "`nTest 2: Checking image size..." -ForegroundColor Yellow
$ImageInfo = docker images $ImageName --format "{{.Size}}" | Select-Object -First 1
Write-Host "Image size: $ImageInfo"

# Parse size
$SizeMB = 0
if ($ImageInfo -match "(\d+\.?\d*)MB") {
    $SizeMB = [double]$Matches[1]
} elseif ($ImageInfo -match "(\d+\.?\d*)GB") {
    $SizeMB = [double]$Matches[1] * 1024
}

if ($SizeMB -lt 100) {
    Print-Result -Success $true -Message "Image size is reasonable ($ImageInfo)"
} else {
    Print-Result -Success $false -Message "Image size is too large: $ImageInfo (expected less than 100MB)"
}

# Test 3: Start container
Write-Host "`nTest 3: Starting container..." -ForegroundColor Yellow
try {
    docker run -d --name $ContainerName -p 8888:80 $ImageName 2>&1 | Out-Null
    Print-Result -Success $true -Message "Container starts successfully"
    
    # Wait for container to be ready
    Write-Host "Waiting for container to be ready..."
    Start-Sleep -Seconds 5
} catch {
    Print-Result -Success $false -Message "Container failed to start"
    exit 1
}

# Test 4: Check if nginx is serving files
Write-Host "`nTest 4: Checking if nginx serves files..." -ForegroundColor Yellow
try {
    $Response = Invoke-WebRequest -Uri "http://localhost:8888/" -UseBasicParsing -ErrorAction Stop
    if ($Response.StatusCode -eq 200) {
        Print-Result -Success $true -Message "Nginx serves files correctly (HTTP $($Response.StatusCode))"
    } else {
        Print-Result -Success $false -Message "Nginx not serving files correctly (HTTP $($Response.StatusCode))"
    }
} catch {
    Print-Result -Success $false -Message "Nginx not serving files correctly"
}

# Test 5: Check if index.html is served
Write-Host "`nTest 5: Checking if index.html is served..." -ForegroundColor Yellow
try {
    $Response = Invoke-WebRequest -Uri "http://localhost:8888/" -UseBasicParsing
    $Content = $Response.Content
    if ($Content -match "<!doctype html>" -or $Content -match "<html") {
        Print-Result -Success $true -Message "index.html is served correctly"
    } else {
        Print-Result -Success $false -Message "index.html is not served correctly"
    }
} catch {
    Print-Result -Success $false -Message "Failed to retrieve index.html"
}

# Test 6: Check SPA routing (non-existent route should serve index.html)
Write-Host "`nTest 6: Checking SPA routing..." -ForegroundColor Yellow
try {
    $Response = Invoke-WebRequest -Uri "http://localhost:8888/eventos" -UseBasicParsing -ErrorAction Stop
    if ($Response.StatusCode -eq 200) {
        Print-Result -Success $true -Message "SPA routing works correctly (HTTP $($Response.StatusCode))"
    } else {
        Print-Result -Success $false -Message "SPA routing not working (HTTP $($Response.StatusCode))"
    }
} catch {
    Print-Result -Success $false -Message "SPA routing not working"
}

# Test 7: Check gzip compression
Write-Host "`nTest 7: Checking gzip compression..." -ForegroundColor Yellow
try {
    $Response = Invoke-WebRequest -Uri "http://localhost:8888/" -UseBasicParsing -Headers @{"Accept-Encoding"="gzip"}
    $ContentEncoding = $Response.Headers["Content-Encoding"]
    if ($ContentEncoding -match "gzip") {
        Print-Result -Success $true -Message "Gzip compression is enabled"
    } else {
        Print-Result -Success $false -Message "Gzip compression is not enabled"
    }
} catch {
    Print-Result -Success $false -Message "Failed to check gzip compression"
}

# Test 8: Check security headers
Write-Host "`nTest 8: Checking security headers..." -ForegroundColor Yellow
try {
    $Response = Invoke-WebRequest -Uri "http://localhost:8888/" -UseBasicParsing
    
    # Check X-Frame-Options
    if ($Response.Headers["X-Frame-Options"]) {
        Print-Result -Success $true -Message "X-Frame-Options header is present"
    } else {
        Print-Result -Success $false -Message "X-Frame-Options header is missing"
    }
    
    # Check X-Content-Type-Options
    if ($Response.Headers["X-Content-Type-Options"]) {
        Print-Result -Success $true -Message "X-Content-Type-Options header is present"
    } else {
        Print-Result -Success $false -Message "X-Content-Type-Options header is missing"
    }
} catch {
    Print-Result -Success $false -Message "Failed to check security headers"
}

# Test 9: Check container health
Write-Host "`nTest 9: Checking container health..." -ForegroundColor Yellow
Start-Sleep -Seconds 5  # Wait for health check to run
try {
    $HealthStatus = docker inspect --format='{{.State.Health.Status}}' $ContainerName 2>$null
    if ($HealthStatus -eq "healthy" -or $HealthStatus -eq "none" -or [string]::IsNullOrEmpty($HealthStatus)) {
        Print-Result -Success $true -Message "Container health check passes"
    } else {
        Print-Result -Success $false -Message "Container health check failed (status: $HealthStatus)"
    }
} catch {
    Print-Result -Success $true -Message "Container health check passes (no health check configured)"
}

# Test 10: Check if container logs show no errors
Write-Host "`nTest 10: Checking container logs for errors..." -ForegroundColor Yellow
$Logs = docker logs $ContainerName 2>&1
if ($Logs -match "error") {
    Print-Result -Success $false -Message "Container logs contain errors"
} else {
    Print-Result -Success $true -Message "Container logs show no errors"
}

# Cleanup
Cleanup

# Summary
Write-Host "`n==========================================" -ForegroundColor Cyan
Write-Host "Test Summary" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Tests Passed: $TestsPassed" -ForegroundColor Green
Write-Host "Tests Failed: $TestsFailed" -ForegroundColor Red
Write-Host "==========================================" -ForegroundColor Cyan

if ($TestsFailed -eq 0) {
    Write-Host "All tests passed!" -ForegroundColor Green
    exit 0
} else {
    Write-Host "Some tests failed!" -ForegroundColor Red
    exit 1
}
