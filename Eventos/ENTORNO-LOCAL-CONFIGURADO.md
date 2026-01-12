# Entorno Local Configurado - Task 2.1

## Resumen

El entorno local para la integración de RabbitMQ ha sido configurado exitosamente. Todos los servicios necesarios están corriendo y accesibles.

## Estado de los Servicios

### ✅ RabbitMQ
- **Estado**: Running
- **Versión**: 3.13.7
- **Contenedor**: reportes-rabbitmq (compartido con microservicio de Reportes)
- **Puerto AMQP**: 5672
- **Puerto Management UI**: 15672
- **URL Management**: http://localhost:15672
- **Credenciales**: 
  - Usuario: `guest`
  - Contraseña: `guest`

### ✅ PostgreSQL
- **Estado**: Running
- **Versión**: PostgreSQL 16 (Alpine)
- **Contenedor**: eventos-postgres
- **Host**: localhost
- **Puerto**: 5434 (mapeado desde 5432 interno)
- **Base de datos**: eventsdb
- **Credenciales**:
  - Usuario: `postgres`
  - Contraseña: `postgres`

## Archivos de Configuración

### 1. docker-compose.yml
Actualizado para incluir:
- Servicio RabbitMQ con Management UI
- Servicio PostgreSQL con health checks
- Variables de entorno para la API de Eventos
- Volúmenes persistentes para ambos servicios
- Dependencias y health checks configurados

### 2. appsettings.json
Agregada configuración de RabbitMQ:
```json
{
  "RabbitMq": {
    "Host": "localhost",
    "Username": "guest",
    "Password": "guest"
  }
}
```

### 3. appsettings.Development.json
Agregada configuración de RabbitMQ con logging detallado:
```json
{
  "Logging": {
    "LogLevel": {
      "MassTransit": "Debug"
    }
  },
  "RabbitMq": {
    "Host": "localhost",
    "Username": "guest",
    "Password": "guest"
  }
}
```

## Scripts Creados

### 1. start-environment.ps1 (PowerShell)
Script para levantar el entorno completo:
- Verifica que Docker esté corriendo
- Levanta RabbitMQ y PostgreSQL
- Espera a que los servicios estén listos
- Muestra información de acceso

**Uso**:
```powershell
./start-environment.ps1
```

### 2. start-environment.sh (Bash)
Versión para Linux/Mac del script anterior.

**Uso**:
```bash
chmod +x start-environment.sh
./start-environment.sh
```

### 3. check-environment.ps1
Script simple para verificar el estado del entorno:
- Verifica RabbitMQ
- Verifica PostgreSQL
- Verifica archivos de configuración

**Uso**:
```powershell
./check-environment.ps1
```

## Verificación Realizada

### ✅ Docker
- Docker Desktop está corriendo
- Contenedores accesibles

### ✅ RabbitMQ Management UI
- Accesible en http://localhost:15672
- Login funcional con credenciales guest/guest
- API REST respondiendo correctamente
- Versión: 3.13.7

### ✅ PostgreSQL
- Contenedor eventos-postgres corriendo
- Aceptando conexiones en puerto 5434
- Base de datos eventsdb disponible
- Health check pasando

### ✅ Configuración
- appsettings.json contiene configuración RabbitMQ
- appsettings.Development.json contiene configuración RabbitMQ
- docker-compose.yml actualizado con RabbitMQ
- Variables de entorno configuradas para eventos-api

## Próximos Pasos

### 1. Acceder a RabbitMQ Management UI
```
URL: http://localhost:15672
Usuario: guest
Contraseña: guest
```

En la UI podrás:
- Ver exchanges creados
- Ver queues creadas
- Monitorear mensajes publicados
- Ver conexiones activas

### 2. Ejecutar la API de Eventos
```bash
cd backend/src/Services/Eventos/Eventos.API
dotnet run
```

La API estará disponible en:
- HTTP: http://localhost:5000
- Swagger UI: http://localhost:5000/swagger

### 3. Verificar Health Endpoint
```bash
curl http://localhost:5000/health
```

## Comandos Útiles

### Ver logs de RabbitMQ
```bash
docker logs -f reportes-rabbitmq
```

### Ver logs de PostgreSQL
```bash
docker logs -f eventos-postgres
```

### Reiniciar servicios
```bash
docker-compose restart rabbitmq postgres
```

### Detener servicios
```bash
docker-compose down
```

### Ver estado de contenedores
```bash
docker ps
```

## Notas Importantes

1. **RabbitMQ Compartido**: El contenedor de RabbitMQ (`reportes-rabbitmq`) es compartido con el microservicio de Reportes. Esto es intencional y permite la comunicación entre microservicios.

2. **Puerto PostgreSQL**: El puerto 5434 se usa externamente para evitar conflictos con otras instancias de PostgreSQL que puedan estar corriendo en el puerto estándar 5432.

3. **Persistencia**: Los datos de RabbitMQ y PostgreSQL se persisten en volúmenes Docker (`rabbitmq_data` y `pgdata_eventos`), por lo que sobreviven a reinicios de contenedores.

4. **Health Checks**: Ambos servicios tienen health checks configurados que aseguran que estén completamente listos antes de que la API intente conectarse.

## Troubleshooting

### RabbitMQ no accesible
```bash
# Verificar que el contenedor esté corriendo
docker ps | grep rabbitmq

# Ver logs
docker logs reportes-rabbitmq

# Reiniciar
docker restart reportes-rabbitmq
```

### PostgreSQL no accesible
```bash
# Verificar que el contenedor esté corriendo
docker ps | grep eventos-postgres

# Verificar conexión
docker exec eventos-postgres pg_isready -U postgres

# Ver logs
docker logs eventos-postgres
```

### Puerto ya en uso
Si ves errores de "port already allocated":
```bash
# Ver qué está usando el puerto
netstat -ano | findstr :5672
netstat -ano | findstr :15672
netstat -ano | findstr :5434

# Detener contenedores conflictivos
docker ps
docker stop <container_id>
```

## Requisitos Cumplidos

- ✅ Levantar RabbitMQ en Docker
- ✅ Levantar PostgreSQL en Docker
- ✅ Configurar variables de entorno
- ✅ Verificar acceso a RabbitMQ Management UI

**Task 2.1 Status**: ✅ COMPLETADO

---

**Fecha de Configuración**: 2025-12-29
**Verificado por**: Kiro AI Assistant
