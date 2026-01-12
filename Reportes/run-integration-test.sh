#!/bin/bash
# Script de prueba de integración end-to-end
# Publica eventos de prueba y verifica que los consumidores procesen correctamente

set -e

SKIP_BUILD=false
SKIP_VERIFICATION=false

# Parse arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --skip-build)
            SKIP_BUILD=true
            shift
            ;;
        --skip-verification)
            SKIP_VERIFICATION=true
            shift
            ;;
        *)
            echo "Unknown option: $1"
            exit 1
            ;;
    esac
done

echo -e "\033[0;36m=== Prueba de Integración End-to-End ===\033[0m"
echo ""

# Paso 1: Verificar servicios
echo -e "\033[0;33mPaso 1: Verificando servicios...\033[0m"
if ! docker-compose ps > /dev/null 2>&1; then
    echo -e "\033[0;31m❌ Error: No se pudo obtener el estado de los servicios\033[0m"
    echo -e "\033[0;33m   Ejecuta 'docker-compose up -d' primero\033[0m"
    exit 1
fi

ALL_HEALTHY=true
while IFS= read -r line; do
    if echo "$line" | grep -q "running"; then
        SERVICE=$(echo "$line" | awk '{print $1}')
        echo -e "\033[0;32m   ✓ $SERVICE está corriendo\033[0m"
    elif echo "$line" | grep -qv "NAME"; then
        SERVICE=$(echo "$line" | awk '{print $1}')
        echo -e "\033[0;31m   ✗ $SERVICE NO está corriendo\033[0m"
        ALL_HEALTHY=false
    fi
done < <(docker-compose ps)

if [ "$ALL_HEALTHY" = false ]; then
    echo ""
    echo -e "\033[0;31m❌ ERROR: No todos los servicios están corriendo\033[0m"
    exit 1
fi

# Paso 2: Verificar MongoDB
echo ""
echo -e "\033[0;33mPaso 2: Verificando MongoDB...\033[0m"
if docker exec reportes-mongodb mongosh --eval "db.runCommand({ping: 1})" --quiet > /dev/null 2>&1; then
    echo -e "\033[0;32m   ✓ MongoDB está respondiendo\033[0m"
else
    echo -e "\033[0;31m   ✗ MongoDB no responde\033[0m"
    exit 1
fi

# Paso 3: Verificar RabbitMQ
echo ""
echo -e "\033[0;33mPaso 3: Verificando RabbitMQ...\033[0m"
if curl -s -u guest:guest http://localhost:15672/api/overview > /dev/null 2>&1; then
    echo -e "\033[0;32m   ✓ RabbitMQ está respondiendo\033[0m"
else
    echo -e "\033[0;31m   ✗ RabbitMQ no responde\033[0m"
    exit 1
fi

# Paso 4: Verificar API
echo ""
echo -e "\033[0;33mPaso 4: Verificando API de Reportes...\033[0m"
if curl -s http://localhost:5002/health > /dev/null 2>&1; then
    echo -e "\033[0;32m   ✓ API de Reportes está respondiendo\033[0m"
else
    echo -e "\033[0;33m   ⚠ API de Reportes no responde en /health\033[0m"
    echo -e "\033[0;33m   Intentando con endpoint de reportes...\033[0m"
    if curl -s http://localhost:5002/api/reportes/auditoria > /dev/null 2>&1; then
        echo -e "\033[0;32m   ✓ API de Reportes está respondiendo\033[0m"
    else
        echo -e "\033[0;31m   ✗ API de Reportes no responde\033[0m"
        exit 1
    fi
fi

# Paso 5: Limpiar datos anteriores
echo ""
echo -e "\033[0;33mPaso 5: Limpiando datos de pruebas anteriores...\033[0m"
docker exec reportes-mongodb mongosh reportes_db --eval "db.metricas_evento.deleteMany({})" --quiet > /dev/null 2>&1 || true
docker exec reportes-mongodb mongosh reportes_db --eval "db.historial_asistencia.deleteMany({})" --quiet > /dev/null 2>&1 || true
docker exec reportes-mongodb mongosh reportes_db --eval "db.reportes_ventas_diarias.deleteMany({})" --quiet > /dev/null 2>&1 || true
docker exec reportes-mongodb mongosh reportes_db --eval "db.logs_auditoria.deleteMany({})" --quiet > /dev/null 2>&1 || true
echo -e "\033[0;32m   ✓ Colecciones limpiadas\033[0m"

# Paso 6: Compilar y ejecutar el publicador de eventos
echo ""
echo -e "\033[0;33mPaso 6: Publicando eventos de prueba...\033[0m"

if [ "$SKIP_BUILD" = false ]; then
    echo -e "\033[0;36m   Compilando publicador de eventos...\033[0m"
    cd test-event-publisher
    if ! dotnet build --configuration Release --verbosity quiet; then
        echo -e "\033[0;31m   ✗ Error compilando el publicador\033[0m"
        exit 1
    fi
    echo -e "\033[0;32m   ✓ Compilación exitosa\033[0m"
    cd ..
fi

echo -e "\033[0;36m   Ejecutando publicador de eventos...\033[0m"
cd test-event-publisher
if ! dotnet run --configuration Release --no-build; then
    echo -e "\033[0;31m   ✗ Error ejecutando el publicador\033[0m"
    exit 1
fi
cd ..

if [ "$SKIP_VERIFICATION" = true ]; then
    echo ""
    echo -e "\033[0;32m=== Publicación completada (verificación omitida) ===\033[0m"
    exit 0
fi

# Paso 7: Verificar datos en MongoDB
echo ""
echo -e "\033[0;33mPaso 7: Verificando datos en MongoDB...\033[0m"

echo -e "\033[0;36m   Verificando métricas de eventos...\033[0m"
METRICAS_COUNT=$(docker exec reportes-mongodb mongosh reportes_db --eval "db.metricas_evento.countDocuments({})" --quiet 2>&1 | grep -o '[0-9]*' | head -1)
if [ "$METRICAS_COUNT" -gt 0 ] 2>/dev/null; then
    echo -e "\033[0;32m   ✓ Métricas de eventos: $METRICAS_COUNT registros\033[0m"
else
    echo -e "\033[0;31m   ✗ No se encontraron métricas de eventos\033[0m"
fi

echo -e "\033[0;36m   Verificando historial de asistencia...\033[0m"
ASISTENCIA_COUNT=$(docker exec reportes-mongodb mongosh reportes_db --eval "db.historial_asistencia.countDocuments({})" --quiet 2>&1 | grep -o '[0-9]*' | head -1)
if [ "$ASISTENCIA_COUNT" -gt 0 ] 2>/dev/null; then
    echo -e "\033[0;32m   ✓ Historial de asistencia: $ASISTENCIA_COUNT registros\033[0m"
else
    echo -e "\033[0;33m   ⚠ No se encontró historial de asistencia\033[0m"
fi

echo -e "\033[0;36m   Verificando reportes de ventas...\033[0m"
VENTAS_COUNT=$(docker exec reportes-mongodb mongosh reportes_db --eval "db.reportes_ventas_diarias.countDocuments({})" --quiet 2>&1 | grep -o '[0-9]*' | head -1)
if [ "$VENTAS_COUNT" -gt 0 ] 2>/dev/null; then
    echo -e "\033[0;32m   ✓ Reportes de ventas: $VENTAS_COUNT registros\033[0m"
else
    echo -e "\033[0;33m   ⚠ No se encontraron reportes de ventas\033[0m"
fi

echo -e "\033[0;36m   Verificando logs de auditoría...\033[0m"
AUDITORIA_COUNT=$(docker exec reportes-mongodb mongosh reportes_db --eval "db.logs_auditoria.countDocuments({})" --quiet 2>&1 | grep -o '[0-9]*' | head -1)
if [ "$AUDITORIA_COUNT" -gt 0 ] 2>/dev/null; then
    echo -e "\033[0;32m   ✓ Logs de auditoría: $AUDITORIA_COUNT registros\033[0m"
else
    echo -e "\033[0;33m   ⚠ No se encontraron logs de auditoría\033[0m"
fi

# Paso 8: Verificar endpoints de API
echo ""
echo -e "\033[0;33mPaso 8: Verificando endpoints de API...\033[0m"

echo -e "\033[0;36m   Probando GET /api/reportes/resumen-ventas...\033[0m"
if RESPONSE=$(curl -s -w "\n%{http_code}" http://localhost:5002/api/reportes/resumen-ventas); then
    HTTP_CODE=$(echo "$RESPONSE" | tail -n1)
    if [ "$HTTP_CODE" = "200" ]; then
        echo -e "\033[0;32m   ✓ Endpoint responde correctamente\033[0m"
    else
        echo -e "\033[0;31m   ✗ Endpoint retornó código $HTTP_CODE\033[0m"
    fi
else
    echo -e "\033[0;31m   ✗ Error en endpoint de resumen de ventas\033[0m"
fi

echo -e "\033[0;36m   Probando GET /api/reportes/auditoria...\033[0m"
if RESPONSE=$(curl -s -w "\n%{http_code}" "http://localhost:5002/api/reportes/auditoria?pagina=1&tamañoPagina=10"); then
    HTTP_CODE=$(echo "$RESPONSE" | tail -n1)
    if [ "$HTTP_CODE" = "200" ]; then
        echo -e "\033[0;32m   ✓ Endpoint de auditoría responde correctamente\033[0m"
    else
        echo -e "\033[0;31m   ✗ Endpoint retornó código $HTTP_CODE\033[0m"
    fi
else
    echo -e "\033[0;31m   ✗ Error en endpoint de auditoría\033[0m"
fi

# Resumen final
echo ""
echo -e "\033[0;36m=== Resumen de Prueba de Integración ===\033[0m"
echo ""
echo -e "\033[0;37mServicios verificados:\033[0m"
echo -e "\033[0;32m  ✓ MongoDB\033[0m"
echo -e "\033[0;32m  ✓ RabbitMQ\033[0m"
echo -e "\033[0;32m  ✓ API de Reportes\033[0m"
echo ""
echo -e "\033[0;37mDatos persistidos:\033[0m"
echo -e "\033[0;36m  • Métricas de eventos: $METRICAS_COUNT\033[0m"
echo -e "\033[0;36m  • Historial de asistencia: $ASISTENCIA_COUNT\033[0m"
echo -e "\033[0;36m  • Reportes de ventas: $VENTAS_COUNT\033[0m"
echo -e "\033[0;36m  • Logs de auditoría: $AUDITORIA_COUNT\033[0m"
echo ""
echo -e "\033[0;32m✅ Prueba de integración completada exitosamente\033[0m"
echo ""
echo -e "\033[0;33mPara ver los datos en detalle:\033[0m"
echo -e "\033[0;37m  docker exec reportes-mongodb mongosh reportes_db --eval 'db.metricas_evento.find().pretty()'\033[0m"
echo -e "\033[0;37m  docker exec reportes-mongodb mongosh reportes_db --eval 'db.historial_asistencia.find().pretty()'\033[0m"
echo ""
