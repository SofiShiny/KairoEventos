#!/bin/bash
# Script de configuración para desarrollo - Entradas.API
# Bash script para Linux/macOS

set -e

# Colores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Parámetros
CLEAN=false
SEED=false
LOGS=false

# Procesar argumentos
while [[ $# -gt 0 ]]; do
    case $1 in
        --clean)
            CLEAN=true
            shift
            ;;
        --seed)
            SEED=true
            shift
            ;;
        --logs)
            LOGS=true
            shift
            ;;
        *)
            echo "Uso: $0 [--clean] [--seed] [--logs]"
            exit 1
            ;;
    esac
done

echo -e "${GREEN}=== Setup de Desarrollo - Entradas.API ===${NC}"

# Función para verificar si Docker está ejecutándose
check_docker() {
    if ! docker info >/dev/null 2>&1; then
        echo -e "${RED}Error: Docker no está ejecutándose${NC}"
        exit 1
    fi
}

# Función para limpiar contenedores y volúmenes
clean_environment() {
    echo -e "${YELLOW}Limpiando entorno...${NC}"
    
    docker-compose -f docker-compose.yml -f docker-compose.override.yml down -v
    docker system prune -f
    
    # Limpiar logs locales
    if [ -d "logs" ]; then
        rm -rf logs/*
        echo -e "${GREEN}Logs locales limpiados${NC}"
    fi
}

# Función para iniciar servicios
start_services() {
    echo -e "${YELLOW}Iniciando servicios...${NC}"
    
    # Crear directorios necesarios
    mkdir -p logs
    mkdir -p mocks/eventos
    mkdir -p mocks/asientos
    
    # Iniciar servicios de infraestructura primero
    echo -e "${CYAN}Iniciando PostgreSQL y RabbitMQ...${NC}"
    docker-compose up -d postgres rabbitmq
    
    # Esperar a que los servicios estén listos
    echo -e "${CYAN}Esperando a que los servicios estén listos...${NC}"
    sleep 30
    
    # Iniciar mocks
    echo -e "${CYAN}Iniciando servicios mock...${NC}"
    docker-compose up -d eventos-api-mock asientos-api-mock
    
    # Iniciar aplicación
    echo -e "${CYAN}Iniciando Entradas.API...${NC}"
    docker-compose up -d entradas-api
    
    # Iniciar herramientas de desarrollo
    echo -e "${CYAN}Iniciando herramientas de desarrollo...${NC}"
    docker-compose up -d pgadmin
}

# Función para verificar el estado de los servicios
check_services() {
    echo -e "${YELLOW}Verificando estado de servicios...${NC}"
    
    services=("postgres" "rabbitmq" "entradas-api")
    
    for service in "${services[@]}"; do
        if docker-compose ps -q "$service" >/dev/null 2>&1; then
            echo -e "${GREEN}✓ $service está ejecutándose${NC}"
        else
            echo -e "${RED}✗ $service no está ejecutándose${NC}"
        fi
    done
    
    # Verificar health checks
    echo -e "\n${YELLOW}Verificando health checks...${NC}"
    sleep 10
    
    if curl -f http://localhost:8080/health >/dev/null 2>&1; then
        echo -e "${GREEN}✓ API Health Check: OK${NC}"
    else
        echo -e "${RED}✗ API Health Check: FAILED${NC}"
    fi
}

# Función para insertar datos de prueba
add_seed_data() {
    echo -e "${YELLOW}Insertando datos de prueba...${NC}"
    
    # Esperar a que la aplicación esté lista
    sleep 20
    
    # Ejecutar función de seed data
    docker exec entradas-postgres psql -U entradas_user -d entradas_db -c "SELECT entradas.insert_development_data();"
    
    echo -e "${GREEN}Datos de prueba insertados${NC}"
}

# Función para mostrar logs
show_logs() {
    echo -e "${YELLOW}Mostrando logs de la aplicación...${NC}"
    docker-compose logs -f entradas-api
}

# Verificar Docker
check_docker

# Ejecutar acciones según parámetros
if [ "$CLEAN" = true ]; then
    clean_environment
fi

start_services

echo -e "\n${GREEN}=== Servicios iniciados ===${NC}"
echo -e "${CYAN}API: http://localhost:8080${NC}"
echo -e "${CYAN}Swagger: http://localhost:8080/swagger${NC}"
echo -e "${CYAN}RabbitMQ Management: http://localhost:15672${NC}"
echo -e "${CYAN}PgAdmin: http://localhost:8090${NC}"

check_services

if [ "$SEED" = true ]; then
    add_seed_data
fi

if [ "$LOGS" = true ]; then
    show_logs
else
    echo -e "\n${YELLOW}Use --logs para ver los logs en tiempo real${NC}"
    echo -e "${YELLOW}Use docker-compose logs -f [servicio] para logs específicos${NC}"
fi

echo -e "\n${GREEN}=== Setup completado ===${NC}"