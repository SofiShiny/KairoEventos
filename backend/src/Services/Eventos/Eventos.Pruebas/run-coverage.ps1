Write-Host "===================================" -ForegroundColor Cyan
Write-Host "Running Events Service Tests" -ForegroundColor Cyan
Write-Host "===================================" -ForegroundColor Cyan

# Restore dependencies
dotnet restore

# Run tests with coverage
dotnet test `
  --configuration Release `
  --no-restore `
  /p:CollectCoverage=true `
  /p:CoverletOutputFormat=cobertura `
  /p:CoverletOutput=./TestResults/coverage.cobertura.xml `
  /p:Exclude="[*.Tests]*" `
  --logger "console;verbosity=detailed"

# Generate HTML report
dotnet reportgenerator `
  -reports:./TestResults/coverage.cobertura.xml `
  -targetdir:./TestResults/coverage-report `
  -reporttypes:Html

Write-Host ""
Write-Host "===================================" -ForegroundColor Green
Write-Host "Coverage report generated at:" -ForegroundColor Green
Write-Host "$(Get-Location)/TestResults/coverage-report/index.html" -ForegroundColor Yellow
Write-Host "===================================" -ForegroundColor Green
