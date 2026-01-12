#!/bin/bash

###############################################################################
# Script de Deployment - Microservicio de Reportes
# 
# Este script automatiza el proceso de build y deployment del microservicio
# Soporta múltiples ambientes: development, staging, production
#
# Uso:
#   ./deploy.sh [ambiente] [opciones]
#
# Ejemplos:
#   ./deploy.sh development
#   ./deploy.sh production --skip-tests
#   ./deploy.sh staging --build-only
###############################################################################

set -e  # Salir si cualquier comando falla

# Colores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Variables por defecto
AMBIENTE=${1:-development}
SKIP_TESTS=false
BUILD_ONLY=false
DOCKER_REGISTRY=""
IMAGE_TAG="latest"

# Procesar argumentos
for arg in "$@"; do
    case $arg in
        --skip-tests)
            SKIP_TESTS=true
            shift
            ;;
        --build-only)
            BUILD_ONLY=true
            shift
            ;;
        --registry=*)
            DOCKER_REGISTRY="${arg#*=}"
            shift
            ;;
        --tag=*)
            IMAGE_TAG="${arg#*=}"
            shift
            ;;
    esac
done

# Funciones de utilidad
log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Validar ambiente
validate_environment() {
    log_info "Validando ambiente: $AMBIENTE"
    
    case $AMBIENTE in
        development|staging|production)
            log_success "Ambiente válido: $AMBIENTE"
            ;;
        *)
            log_error "Ambiente inválido: $AMBIENTE"
            log_error "Ambientes válidos: development, staging, production"
            exit 1
            ;;
    esac
}

# Cargar variables de entorno según ambiente
load_environment_variables() {
    log_info "Cargando variables de entorno para: $AMBIENTE"
    
    ENV_FILE=".env.$AMBIENTE"
    
    if [ -f "$ENV_FILE" ]; then
        export $(cat "$ENV_FILE" | grep -v '^#' | xargs)
        log_success "Variables de entorno cargadas desde $ENV_FILE"
    else
        log_warning "Archivo $ENV_FILE no encontrado, usando valores por defecto"
    fi
    
    # Variables por defecto si no están definidas
    export MONGODB_CONNECTION_STRING=${MONGODB_CONNECTION_STRING:-"mongodb://localhost:27017"}
    export MONGODB_DATABASE=${MONGODB_DATABASE:-"reportes_db"}
    export RABBITMQ_HOST=${RABBITMQ_HOST:-"localhost"}
    export RABBITMQ_PORT=${RABBITMQ_PORT:-"5672"}
    export RABBITMQ_USER=${RABBITMQ_USER:-"guest"}
    export RABBITMQ_PASSWORD=${RABBITMQ_PASSWORD:-"guest"}
    export ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT:-"Development"}
}

# Ejecutar tests
run_tests() {
    if [ "$SKIP_TESTS" = true ]; then
        log_warning "Saltando tests (--skip-tests especificado)"
        return 0
    fi
    
    log_info "Ejecutando tests..."
    
    cd backend/src/Services/Reportes
    
    # Restaurar dependencias
    dotnet restore
    
    # Ejecutar tests con cobertura
    dotnet test \
        --configuration Release \
        --no-restore \
        /p:CollectCoverage=true \
        /p:CoverletOutputFormat=cobertura \
        /p:Threshold=80 \
        /p:ThresholdType=line
    
    if [ $? -eq 0 ]; then
        log_success "Tests ejecutados exitosamente"
    else
        log_error "Tests fallaron"
        exit 1
    fi
    
    cd ../../../../
}

# Build de la aplicación
build_application() {
    log_info "Construyendo aplicación..."
    
    cd backend/src/Services/Reportes/Reportes.API
    
    # Limpiar builds anteriores
    dotnet clean --configuration Release
    
    # Build
    dotnet build \
        --configuration Release \
        --no-restore
    
    if [ $? -eq 0 ]; then
        log_success "Build completado exitosamente"
    else
        log_error "Build falló"
        exit 1
    fi
    
    cd ../../../../../
}

# Build de imagen Docker
build_docker_image() {
    log_info "Construyendo imagen Docker..."
    
    IMAGE_NAME="reportes-api"
    FULL_IMAGE_NAME="$IMAGE_NAME:$IMAGE_TAG"
    
    if [ -n "$DOCKER_REGISTRY" ]; then
        FULL_IMAGE_NAME="$DOCKER_REGISTRY/$FULL_IMAGE_NAME"
    fi
    
    docker build \
        -t "$FULL_IMAGE_NAME" \
        -f Dockerfile \
        --build-arg ASPNETCORE_ENVIRONMENT="$ASPNETCORE_ENVIRONMENT" \
        .
    
    if [ $? -eq 0 ]; then
        log_success "Imagen Docker construida: $FULL_IMAGE_NAME"
    else
        log_error "Build de imagen Docker falló"
        exit 1
    fi
    
    # Tag adicional con timestamp para versionado
    TIMESTAMP=$(date +%Y%m%d-%H%M%S)
    VERSIONED_IMAGE="$IMAGE_NAME:$AMBIENTE-$TIMESTAMP"
    
    if [ -n "$DOCKER_REGISTRY" ]; then
        VERSIONED_IMAGE="$DOCKER_REGISTRY/$VERSIONED_IMAGE"
    fi
    
    docker tag "$FULL_IMAGE_NAME" "$VERSIONED_IMAGE"
    log_success "Imagen taggeada: $VERSIONED_IMAGE"
}

# Push de imagen a registry
push_docker_image() {
    if [ -z "$DOCKER_REGISTRY" ]; then
        log_warning "No se especificó registry, saltando push"
        return 0
    fi
    
    log_info "Pusheando imagen a registry..."
    
    IMAGE_NAME="reportes-api"
    FULL_IMAGE_NAME="$DOCKER_REGISTRY/$IMAGE_NAME:$IMAGE_TAG"
    
    docker push "$FULL_IMAGE_NAME"
    
    if [ $? -eq 0 ]; then
        log_success "Imagen pusheada exitosamente"
    else
        log_error "Push de imagen falló"
        exit 1
    fi
}

# Deploy con docker-compose
deploy_with_compose() {
    if [ "$BUILD_ONLY" = true ]; then
        log_warning "Modo build-only, saltando deployment"
        return 0
    fi
    
    log_info "Desplegando con docker-compose..."
    
    # Detener servicios existentes
    docker-compose down
    
    # Levantar servicios
    docker-compose up -d --build
    
    if [ $? -eq 0 ]; then
        log_success "Servicios desplegados exitosamente"
    else
        log_error "Deployment falló"
        exit 1
    fi
    
    # Esperar a que los servicios estén listos
    log_info "Esperando a que los servicios estén listos..."
    sleep 10
    
    # Verificar health checks
    check_health
}

# Verificar health checks
check_health() {
    log_info "Verificando health checks..."
    
    MAX_RETRIES=30
    RETRY_COUNT=0
    
    while [ $RETRY_COUNT -lt $MAX_RETRIES ]; do
        HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:5002/health)
        
        if [ "$HTTP_CODE" = "200" ]; then
            log_success "Health check OK"
            return 0
        fi
        
        RETRY_COUNT=$((RETRY_COUNT + 1))
        log_info "Esperando health check... ($RETRY_COUNT/$MAX_RETRIES)"
        sleep 2
    done
    
    log_error "Health check falló después de $MAX_RETRIES intentos"
    exit 1
}

# Mostrar información de deployment
show_deployment_info() {
    log_success "==================================="
    log_success "Deployment completado exitosamente"
    log_success "==================================="
    echo ""
    log_info "Ambiente: $AMBIENTE"
    log_info "Imagen: reportes-api:$IMAGE_TAG"
    echo ""
    log_info "Servicios disponibles:"
    log_info "  - API: http://localhost:5002"
    log_info "  - Swagger: http://localhost:5002/swagger"
    log_info "  - Health: http://localhost:5002/health"
    log_info "  - Hangfire: http://localhost:5002/hangfire"
    log_info "  - RabbitMQ Management: http://localhost:15672"
    echo ""
    log_info "Para ver logs:"
    log_info "  docker-compose logs -f reportes-api"
    echo ""
}

# Rollback
rollback() {
    log_warning "Ejecutando rollback..."
    
    docker-compose down
    
    # Aquí se podría implementar lógica para volver a una versión anterior
    # Por ejemplo, usando tags de imágenes anteriores
    
    log_success "Rollback completado"
}

# Main
main() {
    log_info "==================================="
    log_info "Iniciando deployment"
    log_info "==================================="
    echo ""
    
    validate_environment
    load_environment_variables
    
    # Ejecutar pasos de deployment
    run_tests
    build_application
    build_docker_image
    push_docker_image
    deploy_with_compose
    
    show_deployment_info
}

# Trap para manejar errores
trap 'log_error "Deployment falló en línea $LINENO"; exit 1' ERR

# Ejecutar main
main
