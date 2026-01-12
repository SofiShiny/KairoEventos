#!/bin/bash
# Script para levantar el entorno local de desarrollo
# Incluye RabbitMQ y PostgreSQL

echo "========================================"
echo "Iniciando Entorno de Desarrollo"
echo "========================================"
echo ""

# Verificar si Docker está corriendo
echo "Verificando Docker..."
if ! docker info > /dev/null 2>&1; then
    echo "ERROR: Docker no está corriendo. Por favor inicia Docker."
    exit 1
fi
echo "✓ Docker está corriendo"
echo ""

# Detener contenedores existentes si los hay
echo "Deteniendo contenedores existentes..."
docker-compose down > /dev/null 2>&1
echo "✓ Contenedores detenidos"
echo ""

# Levantar solo RabbitMQ y PostgreSQL (sin la API)
echo "Levantando RabbitMQ y PostgreSQL..."
docker-compose up -d rabbitmq postgres

# Esperar a que los servicios estén listos
echo ""
echo "Esperando a que los servicios estén listos..."
sleep 5

# Verificar RabbitMQ
echo ""
echo "Verificando RabbitMQ..."
max_retries=30
retry_count=0
rabbitmq_ready=false

while [ $retry_count -lt $max_retries ] && [ "$rabbitmq_ready" = false ]; do
    if curl -s http://localhost:15672 > /dev/null 2>&1; then
        rabbitmq_ready=true
    else
        retry_count=$((retry_count + 1))
        echo "  Intento $retry_count/$max_retries..."
        sleep 2
    fi
done

if [ "$rabbitmq_ready" = true ]; then
    echo "✓ RabbitMQ está listo"
else
    echo "⚠ RabbitMQ tardó más de lo esperado, pero puede estar iniciando"
fi

# Verificar PostgreSQL
echo ""
echo "Verificando PostgreSQL..."
if docker exec eventos-postgres pg_isready -U postgres > /dev/null 2>&1; then
    echo "✓ PostgreSQL está listo"
else
    echo "⚠ PostgreSQL puede estar iniciando todavía"
fi

# Mostrar información de acceso
echo ""
echo "========================================"
echo "Entorno Listo"
echo "========================================"
echo ""
echo "RabbitMQ Management UI:"
echo "  URL: http://localhost:15672"
echo "  Usuario: guest"
echo "  Contraseña: guest"
echo ""
echo "PostgreSQL:"
echo "  Host: localhost"
echo "  Puerto: 5434"
echo "  Base de datos: eventsdb"
echo "  Usuario: postgres"
echo "  Contraseña: postgres"
echo ""
echo "Para ver los logs:"
echo "  docker-compose logs -f"
echo ""
echo "Para detener el entorno:"
echo "  docker-compose down"
echo ""
echo "Para ejecutar la API localmente:"
echo "  cd backend/src/Services/Eventos/Eventos.API"
echo "  dotnet run"
echo ""
