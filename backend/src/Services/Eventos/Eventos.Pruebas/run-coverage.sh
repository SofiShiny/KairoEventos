#!/bin/bash

echo "==================================="
echo "Running Events Service Tests"
echo "==================================="

# Restore dependencies
dotnet restore

# Run tests with coverage
dotnet test \
  --configuration Release \
  --no-restore \
  /p:CollectCoverage=true \
  /p:CoverletOutputFormat=cobertura \
  /p:CoverletOutput=./TestResults/coverage.cobertura.xml \
  /p:Exclude="[*.Tests]*" \
  --logger "console;verbosity=detailed"

# Generate HTML report
dotnet reportgenerator \
  -reports:./TestResults/coverage.cobertura.xml \
  -targetdir:./TestResults/coverage-report \
  -reporttypes:Html

echo ""
echo "==================================="
echo "Coverage report generated at:"
echo "$(pwd)/TestResults/coverage-report/index.html"
echo "==================================="
