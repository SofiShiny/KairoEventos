#!/bin/bash

# Build script for Frontend Unificado Docker image
# Usage: ./build-docker.sh [environment]
# Example: ./build-docker.sh production

set -e

ENVIRONMENT=${1:-production}
IMAGE_NAME="frontend-unificado"
TIMESTAMP=$(date +%Y%m%d-%H%M%S)

echo "=========================================="
echo "Building Frontend Unificado Docker Image"
echo "Environment: $ENVIRONMENT"
echo "Timestamp: $TIMESTAMP"
echo "=========================================="

# Load environment variables based on environment
if [ "$ENVIRONMENT" = "production" ]; then
    ENV_FILE=".env.production"
    TAG="latest"
elif [ "$ENVIRONMENT" = "development" ]; then
    ENV_FILE=".env.development"
    TAG="dev"
else
    echo "Error: Unknown environment '$ENVIRONMENT'"
    echo "Valid options: production, development"
    exit 1
fi

# Check if env file exists
if [ ! -f "$ENV_FILE" ]; then
    echo "Error: Environment file '$ENV_FILE' not found"
    exit 1
fi

echo "Using environment file: $ENV_FILE"

# Build the Docker image
echo "Building Docker image..."
docker build \
    -t ${IMAGE_NAME}:${TAG} \
    -t ${IMAGE_NAME}:${TAG}-${TIMESTAMP} \
    -f Dockerfile \
    .

echo ""
echo "=========================================="
echo "Build completed successfully!"
echo "=========================================="
echo "Image tags:"
echo "  - ${IMAGE_NAME}:${TAG}"
echo "  - ${IMAGE_NAME}:${TAG}-${TIMESTAMP}"
echo ""
echo "Image size:"
docker images ${IMAGE_NAME}:${TAG} --format "table {{.Repository}}\t{{.Tag}}\t{{.Size}}"
echo ""
echo "To run the container:"
echo "  docker-compose up -d"
echo ""
echo "To push to registry:"
echo "  docker push ${IMAGE_NAME}:${TAG}"
echo "=========================================="
