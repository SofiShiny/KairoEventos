# 🔧 Comandos de Referencia Rápida

## 🐳 Docker

### RabbitMQ

```powershell
# Iniciar RabbitMQ
docker run -d --name rabbitmq `
  -p 5672:5672 `
  -p 15672:15672 `
  rabbitmq:3-management

# Detener RabbitMQ
docker stop rabbitmq

# Iniciar RabbitMQ existente
docker start rabbitmq

# Ver logs
docker logs rabbitmq -f

# Eliminar contenedor
docker rm rabbitmq

# Eliminar contenedor y volúmenes
docker rm -v rabbitmq
```

### PostgreSQL

```powershell
# Iniciar PostgreSQL
docker run -d --name postgres `
  -e POSTGRES_DB=eventsdb `
  -e POSTGRES_USER=postgres `
  -e POSTGRES_PASSWORD=postgres `
  -p 5432:5432 `
  postgres:15

# Detener PostgreSQL
docker stop postgres

# Iniciar PostgreSQL existente
docker start postgres

# Ver logs
docker logs postgres -f

# Conectarse a PostgreSQL
docker exec -it postgres psql -U postgres -d eventsdb

# Ejecutar query
docker exec -it postgres psql -U postgres -d eventsdb -c "SELECT * FROM eventos;"

# Eliminar contenedor
docker rm postgres
```

### Comandos Generales

```powershell
# Ver contenedores corriendo
docker ps

# Ver todos los contenedores
docker ps -a

# Ver uso de recursos
docker stats

# Detener todos los contenedores
docker stop $(docker ps -q)

# Eliminar todos los contenedores detenidos
docker container prune -f

# Eliminar volúmenes no usados
docker volume prune -f

# Ver redes
docker network ls

# Limpiar todo (CUIDADO)
docker system prune -a --volumes
```

---

## 🚀 .NET

### Compilación y Ejecución

```powershell
# Navegar al proyecto
cd Eventos/backend/src/Services/Eventos/Eventos.API

# Restaurar dependencias
dotnet restore

# Compilar
dotnet build

# Ejecutar
dotnet run

# Ejecutar en modo watch (recarga automática)
dotnet watch run

# Compilar en Release
dotnet build -c Release

# Publicar
dotnet publish -c Release -o ./publish
```

### Pruebas

```powershell
# Navegar a pruebas
cd Eventos/backend/src/Services/Eventos/Eventos.Pruebas

# Ejecutar todas las pruebas
dotnet test

# Ejecutar con verbosidad
dotnet test -v detailed

# Ejecutar pruebas específicas
dotnet test --filter "FullyQualifiedName~PublicarEvento"

# Ejecutar con cobertura
dotnet test /p:CollectCoverage=true
```

### Gestión de Paquetes

```powershell
# Agregar paquete
dotnet add package MassTransit.RabbitMQ

# Listar paquetes
dotnet list package

# Actualizar paquete
dotnet add package MassTransit.RabbitMQ --version 8.1.3

# Eliminar paquete
dotnet remove package MassTransit.RabbitMQ
```

---

## 🌐 API Testing

### cURL (Windows)

```powershell
# Crear evento
curl -X POST http://localhost:5000/api/eventos `
  -H "Content-Type: application/json" `
  -d '{\"titulo\":\"Evento Test\",\"descripcion\":\"Test\",\"ubicacion\":{\"nombre\":\"Centro\",\"direccion\":\"Av 123\",\"ciudad\":\"Ciudad\",\"pais\":\"Pais\"},\"fechaInicio\":\"2025-02-01T10:00:00Z\",\"fechaFin\":\"2025-02-01T18:00:00Z\",\"maximoAsistentes\":100}'

# Obtener evento
curl http://localhost:5000/api/eventos/{id}

# Publicar evento
curl -X PATCH http://localhost:5000/api/eventos/{id}/publicar

# Registrar asistente
curl -X POST http://localhost:5000/api/eventos/{id}/asistentes `
  -H "Content-Type: application/json" `
  -d '{\"usuarioId\":\"user001\",\"nombre\":\"Juan\",\"correo\":\"juan@example.com\"}'

# Cancelar evento
curl -X PATCH http://localhost:5000/api/eventos/{id}/cancelar

# Health check
curl http://localhost:5000/health
```

### PowerShell (Invoke-RestMethod)

```powershell
# Crear evento
$body = @{
    titulo = "Evento Test"
    descripcion = "Test"
    ubicacion = @{
        nombre = "Centro"
        direccion = "Av 123"
        ciudad = "Ciudad"
        pais = "Pais"
    }
    fechaInicio = "2025-02-01T10:00:00Z"
    fechaFin = "2025-02-01T18:00:00Z"
    maximoAsistentes = 100
} | ConvertTo-Json

$evento = Invoke-RestMethod -Uri "http://localhost:5000/api/eventos" `
    -Method Post `
    -Body $body `
    -ContentType "application/json"

$eventoId = $evento.id

# Obtener evento
$evento = Invoke-RestMethod -Uri "http://localhost:5000/api/eventos/$eventoId" `
    -Method Get

# Publicar evento
Invoke-RestMethod -Uri "http://localhost:5000/api/eventos/$eventoId/publicar" `
    -Method Patch

# Registrar asistente
$asistente = @{
    usuarioId = "user001"
    nombre = "Juan"
    correo = "juan@example.com"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/eventos/$eventoId/asistentes" `
    -Method Post `
    -Body $asistente `
    -ContentType "application/json"

# Cancelar evento
Invoke-RestMethod -Uri "http://localhost:5000/api/eventos/$eventoId/cancelar" `
    -Method Patch

# Health check
Invoke-RestMethod -Uri "http://localhost:5000/health" -Method Get
```

---

## 🐰 RabbitMQ Management

### CLI (rabbitmqctl)

```powershell
# Listar colas
docker exec rabbitmq rabbitmqctl list_queues

# Listar exchanges
docker exec rabbitmq rabbitmqctl list_exchanges

# Listar bindings
docker exec rabbitmq rabbitmqctl list_bindings

# Listar conexiones
docker exec rabbitmq rabbitmqctl list_connections

# Listar canales
docker exec rabbitmq rabbitmqctl list_channels

# Purgar cola
docker exec rabbitmq rabbitmqctl purge_queue {queue_name}

# Estado del cluster
docker exec rabbitmq rabbitmqctl cluster_status

# Reiniciar RabbitMQ
docker exec rabbitmq rabbitmqctl stop_app
docker exec rabbitmq rabbitmqctl start_app
```

### Management API

```powershell
# Obtener información general
Invoke-RestMethod -Uri "http://localhost:15672/api/overview" `
    -Method Get `
    -Credential (Get-Credential)

# Listar colas
Invoke-RestMethod -Uri "http://localhost:15672/api/queues" `
    -Method Get `
    -Credential (Get-Credential)

# Obtener mensajes de una cola (sin consumir)
Invoke-RestMethod -Uri "http://localhost:15672/api/queues/%2F/{queue_name}/get" `
    -Method Post `
    -Body '{"count":10,"ackmode":"ack_requeue_true","encoding":"auto"}' `
    -ContentType "application/json" `
    -Credential (Get-Credential)
```

---

## 🗄️ PostgreSQL

### Comandos SQL Útiles

```sql
-- Ver todos los eventos
SELECT * FROM eventos;

-- Ver eventos publicados
SELECT * FROM eventos WHERE estado = 'Publicado';

-- Ver eventos cancelados
SELECT * FROM eventos WHERE estado = 'Cancelado';

-- Ver asistentes de un evento
SELECT * FROM asistentes WHERE evento_id = '{guid}';

-- Contar eventos por estado
SELECT estado, COUNT(*) FROM eventos GROUP BY estado;

-- Ver últimos eventos creados
SELECT * FROM eventos ORDER BY fecha_inicio DESC LIMIT 10;

-- Limpiar todos los eventos (CUIDADO)
DELETE FROM asistentes;
DELETE FROM eventos;
```

### Comandos desde PowerShell

```powershell
# Ejecutar query
docker exec -it postgres psql -U postgres -d eventsdb -c "SELECT * FROM eventos;"

# Exportar datos
docker exec -it postgres pg_dump -U postgres eventsdb > backup.sql

# Importar datos
Get-Content backup.sql | docker exec -i postgres psql -U postgres -d eventsdb

# Ver tamaño de la base de datos
docker exec -it postgres psql -U postgres -d eventsdb -c "SELECT pg_size_pretty(pg_database_size('eventsdb'));"
```

---

## ⚙️ Variables de Entorno

### Configurar Variables

```powershell
# RabbitMQ
$env:RabbitMq:Host="localhost"

# PostgreSQL
$env:POSTGRES_HOST="localhost"
$env:POSTGRES_DB="eventsdb"
$env:POSTGRES_USER="postgres"
$env:POSTGRES_PASSWORD="postgres"
$env:POSTGRES_PORT="5432"

# API
$env:ASPNETCORE_URLS="http://0.0.0.0:5000"
$env:ASPNETCORE_ENVIRONMENT="Development"
```

### Ver Variables

```powershell
# Ver variable específica
echo $env:RabbitMq:Host

# Ver todas las variables de entorno
Get-ChildItem Env:

# Ver variables que contienen "RABBIT"
Get-ChildItem Env: | Where-Object { $_.Name -like "*RABBIT*" }
```

### Limpiar Variables

```powershell
# Eliminar variable
Remove-Item Env:RabbitMq:Host

# Limpiar todas las variables personalizadas
Remove-Item Env:POSTGRES_HOST
Remove-Item Env:POSTGRES_DB
Remove-Item Env:POSTGRES_USER
Remove-Item Env:POSTGRES_PASSWORD
Remove-Item Env:POSTGRES_PORT
```

---

## 🧪 Scripts de Prueba

### Ejecutar Script de Integración

```powershell
# Ejecutar con configuración por defecto
.\test-integracion.ps1

# Ejecutar con URL personalizada
.\test-integracion.ps1 -ApiUrl "http://localhost:5001"

# Ejecutar con salida verbose
.\test-integracion.ps1 -Verbose

# Ejecutar y guardar salida
.\test-integracion.ps1 | Tee-Object -FilePath test-results.txt
```

---

## 📊 Monitoreo

### Ver Logs en Tiempo Real

```powershell
# API (si se ejecuta con dotnet run)
# Los logs aparecen en la consola

# RabbitMQ
docker logs rabbitmq -f

# PostgreSQL
docker logs postgres -f

# Todos los contenedores
docker-compose logs -f
```

### Ver Uso de Recursos

```powershell
# Docker stats
docker stats

# Docker stats de un contenedor específico
docker stats rabbitmq

# Uso de puertos
netstat -ano | findstr :5000
netstat -ano | findstr :5672
netstat -ano | findstr :15672
netstat -ano | findstr :5432
```

---

## 🔍 Troubleshooting

### Verificar Conectividad

```powershell
# Verificar que el puerto está abierto
Test-NetConnection -ComputerName localhost -Port 5000
Test-NetConnection -ComputerName localhost -Port 5672
Test-NetConnection -ComputerName localhost -Port 15672
Test-NetConnection -ComputerName localhost -Port 5432

# Ping a contenedor Docker
docker exec rabbitmq ping -c 3 postgres
```

### Limpiar y Reiniciar

```powershell
# Detener todos los servicios
docker stop rabbitmq postgres

# Eliminar contenedores
docker rm rabbitmq postgres

# Limpiar volúmenes
docker volume prune -f

# Reiniciar Docker Desktop
Restart-Service docker

# Volver a empezar
# (ejecutar comandos de inicio de RabbitMQ y PostgreSQL)
```

---

## 📚 Accesos Rápidos

### URLs Importantes

```
API Swagger:        http://localhost:5000/swagger
API Health:         http://localhost:5000/health
RabbitMQ UI:        http://localhost:15672
RabbitMQ API:       http://localhost:15672/api
```

### Credenciales

```
RabbitMQ:
  Usuario: guest
  Password: guest

PostgreSQL:
  Usuario: postgres
  Password: postgres
  Database: eventsdb
```

---

## 🎯 Comandos Más Usados

```powershell
# Setup completo
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
docker run -d --name postgres -e POSTGRES_DB=eventsdb -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgres -p 5432:5432 postgres:15
$env:RabbitMq:Host="localhost"
cd Eventos/backend/src/Services/Eventos/Eventos.API
dotnet run

# Verificación rápida
docker ps
curl http://localhost:5000/health
curl http://localhost:15672

# Prueba completa
.\test-integracion.ps1

# Limpieza
docker stop rabbitmq postgres
docker rm rabbitmq postgres
```

---

**Tip:** Guarda este archivo como referencia rápida para comandos comunes.
