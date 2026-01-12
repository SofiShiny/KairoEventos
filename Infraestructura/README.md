# Infraestructura Compartida - Kairo Microservices

Esta carpeta contiene la infraestructura base compartida por todos los microservicios del sistema Kairo, incluyendo bases de datos, message broker, sistema de autenticación (Keycloak) y API Gateway.

## 🏗️ Arquitectura

La infraestructura utiliza una **red Docker externa** llamada `kairo-network` que permite la comunicación entre todos los microservicios, independientemente de en qué repositorio se encuentren.

### Servicios Incluidos

#### 1. **Keycloak** - Identity and Access Management (IAM)
- **Puerto:** `8180` (HTTP)
- **Admin Console:** http://localhost:8180
- **Credenciales Admin:** `admin` / `admin`
- **Realm:** `Kairo` (importado automáticamente)
- **Base de datos:** PostgreSQL (keycloak)

#### 2. **API Gateway** - Punto de Entrada Único
- **Puerto:** `8080` (HTTP)
- **Tecnología:** YARP (Yet Another Reverse Proxy)
- **Funciones:** Enrutamiento, Autenticación JWT, Autorización, CORS
- **Health Check:** http://localhost:8080/health

#### 3. **PostgreSQL 16** - Base de Datos Relacional
- **Puerto:** `5432`
- **Bases de datos:** 
  - `kairo_eventos` - Servicio de Eventos
  - `kairo_asientos` - Servicio de Asientos
  - `kairo_reportes` - Servicio de Reportes (escritura)
  - `keycloak` - Keycloak IAM
- **Credenciales:** `postgres` / `postgres`

#### 4. **MongoDB 7** - Base de Datos NoSQL
- **Puerto:** `27017`
- **Base de datos:** `kairo_reportes` (lectura)
- **Uso:** Modelos de lectura para reportes (CQRS)

#### 5. **RabbitMQ 3 Management** - Message Broker
- **Puerto AMQP:** `5672`
- **Puerto Management UI:** `15672`
- **Management Console:** http://localhost:15672
- **Credenciales:** `guest` / `guest`
- **Uso:** Event-driven communication entre microservicios

## 🚀 Inicio Rápido

### 1. Crear la Red Externa (Solo la primera vez)

```bash
docker network create kairo-network
```

### 2. Levantar la Infraestructura

```bash
cd Infraestructura
docker-compose up -d
```

Esto levantará todos los servicios en el siguiente orden:
1. PostgreSQL (base de datos)
2. MongoDB (base de datos NoSQL)
3. RabbitMQ (message broker)
4. Keycloak (IAM) - espera a que PostgreSQL esté listo
5. Gateway (API Gateway) - espera a que Keycloak esté listo

### 3. Verificar que Todo Está Corriendo

```bash
docker-compose ps
```

Deberías ver 5 servicios en estado `healthy`:
- `kairo-postgres`
- `kairo-mongodb`
- `kairo-rabbitmq`
- `kairo-keycloak`
- `kairo-gateway`

### 4. Verificar Health Checks

```bash
# PostgreSQL
docker exec kairo-postgres pg_isready -U postgres

# MongoDB
docker exec kairo-mongodb mongosh --eval "db.adminCommand('ping')"

# RabbitMQ
docker exec kairo-rabbitmq rabbitmq-diagnostics ping

# Keycloak
curl http://localhost:8180/health/ready

# Gateway
curl http://localhost:8080/health
```

### 5. Acceder a las Interfaces de Administración

- **Keycloak Admin Console:** http://localhost:8180
  - Usuario: `admin`
  - Password: `admin`

- **RabbitMQ Management:** http://localhost:15672
  - Usuario: `guest`
  - Password: `guest`

- **Gateway Health Check:** http://localhost:8080/health

## 🔐 Keycloak - Configuración Automatizada

### Importación Automática del Realm

Keycloak se configura automáticamente al iniciar mediante el archivo `configs/keycloak/realm-export.json`. Este archivo contiene:

- ✅ Realm "Kairo" con configuración completa
- ✅ Clientes (kairo-web, kairo-api)
- ✅ Roles (User, Admin, Organizator)
- ✅ Usuarios por defecto con credenciales

**No se requiere configuración manual** - todo está automatizado.

### Usuarios por Defecto

El realm incluye tres usuarios preconfigurados:

| Usuario | Password | Roles | Email | Descripción |
|---------|----------|-------|-------|-------------|
| `admin` | `admin123` | Admin, User | admin@kairo.com | Administrador con acceso completo |
| `organizador` | `org123` | Organizator, User | organizador@kairo.com | Organizador de eventos |
| `usuario` | `user123` | User | usuario@kairo.com | Usuario regular |

### Clientes Configurados

#### kairo-web (Frontend)
- **Tipo:** Cliente público
- **Flujo:** Authorization Code + PKCE
- **Redirect URIs:** 
  - `http://localhost:5173/*`
  - `http://localhost:3000/*`
- **Web Origins:** 
  - `http://localhost:5173`
  - `http://localhost:3000`

#### kairo-api (Backend)
- **Tipo:** Bearer-only
- **Uso:** Validación de tokens JWT en el Gateway

### Acceder a Keycloak Admin Console

1. Abrir http://localhost:8180
2. Click en "Administration Console"
3. Login con `admin` / `admin`
4. Seleccionar realm "Kairo" en el dropdown superior izquierdo

### Obtener un Token JWT

```bash
curl -X POST http://localhost:8180/realms/Kairo/protocol/openid-connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "client_id=kairo-web" \
  -d "username=admin" \
  -d "password=admin123" \
  -d "grant_type=password"
```

### Proceso de Importación del Realm

El realm se importa automáticamente mediante:

1. El archivo `realm-export.json` se monta en `/opt/keycloak/data/import/`
2. Keycloak inicia con el flag `--import-realm`
3. Si el realm ya existe, la importación se omite (idempotente)
4. Los usuarios, roles y clientes se crean automáticamente

**Ubicación del archivo:** `Infraestructura/configs/keycloak/realm-export.json`

## 🌐 API Gateway

### Funcionalidades

El Gateway proporciona:

1. **Enrutamiento Inteligente**
   - `/api/eventos/*` → Servicio de Eventos
   - `/api/asientos/*` → Servicio de Asientos
   - `/api/usuarios/*` → Servicio de Usuarios
   - `/api/entradas/*` → Servicio de Entradas
   - `/api/reportes/*` → Servicio de Reportes

2. **Autenticación JWT**
   - Validación de tokens con Keycloak
   - Extracción de claims (roles, username, email)

3. **Autorización Basada en Roles**
   - User: Acceso básico
   - Admin: Acceso completo
   - Organizator: Gestión de eventos

4. **CORS**
   - Permite peticiones desde frontends configurados
   - Soporta credenciales (cookies, headers de autenticación)

5. **Health Checks**
   - `/health` - Estado general
   - `/health/ready` - Verifica Keycloak
   - `/health/live` - Liveness probe

### Usar el Gateway

```bash
# 1. Obtener token
TOKEN=$(curl -X POST http://localhost:8180/realms/Kairo/protocol/openid-connect/token \
  -d "client_id=kairo-web" \
  -d "username=admin" \
  -d "password=admin123" \
  -d "grant_type=password" | jq -r '.access_token')

# 2. Hacer petición a través del Gateway
curl -H "Authorization: Bearer $TOKEN" \
  http://localhost:8080/api/eventos
```

## 🔌 Conexión desde Microservicios

### Desde Docker (Otros Contenedores)

Los microservicios que corren en Docker deben usar los nombres de servicio como hosts:

```yaml
# En tu docker-compose.yml del microservicio
services:
  mi-api:
    environment:
      POSTGRES_HOST: postgres
      MONGODB_HOST: mongodb
      RABBITMQ_HOST: rabbitmq
      KEYCLOAK_AUTHORITY: http://keycloak:8080/realms/Kairo
    networks:
      - kairo-network

networks:
  kairo-network:
    external: true
```

### Desde Local (Desarrollo)

Cuando ejecutas un microservicio localmente (fuera de Docker), usa `localhost`:

```bash
# Variables de entorno para desarrollo local
POSTGRES_HOST=localhost
MONGODB_HOST=localhost
RABBITMQ_HOST=localhost
KEYCLOAK_AUTHORITY=http://localhost:8180/realms/Kairo
```

## 📊 Acceso a Interfaces de Administración

### Keycloak Admin Console
- **URL:** http://localhost:8180
- **Usuario:** `admin`
- **Password:** `admin`
- **Funciones:**
  - Gestionar usuarios y roles
  - Configurar clientes
  - Ver logs de autenticación
  - Configurar políticas de seguridad

### RabbitMQ Management UI
- **URL:** http://localhost:15672
- **Usuario:** `guest`
- **Password:** `guest`
- **Funciones:**
  - Ver colas y exchanges
  - Monitorear mensajes
  - Ver conexiones activas
  - Gestionar virtual hosts

### PostgreSQL (usando cliente)
```bash
# Conectar a base de datos de eventos
psql -h localhost -U postgres -d kairo_eventos
# Password: postgres

# Listar bases de datos
\l

# Conectar a otra base de datos
\c kairo_asientos
```

### MongoDB (usando mongosh)
```bash
# Conectar a MongoDB
mongosh mongodb://localhost:27017/kairo_reportes

# Listar colecciones
show collections

# Ver documentos
db.metricas_evento.find().pretty()
```

## 🛠️ Comandos Útiles

### Ver Logs

```bash
# Todos los servicios
docker-compose logs -f

# Un servicio específico
docker-compose logs -f keycloak
docker-compose logs -f gateway
docker-compose logs -f postgres
docker-compose logs -f mongodb
docker-compose logs -f rabbitmq
```

### Reiniciar Servicios

```bash
# Reiniciar todos
docker-compose restart

# Reiniciar uno específico
docker-compose restart keycloak
docker-compose restart gateway
```

### Detener Servicios

```bash
# Detener sin eliminar
docker-compose stop

# Detener y eliminar contenedores (mantiene volúmenes)
docker-compose down

# Eliminar todo incluidos volúmenes (⚠️ CUIDADO - borra datos)
docker-compose down -v
```

### Verificar Estado

```bash
# Ver contenedores corriendo
docker-compose ps

# Ver uso de recursos
docker stats

# Ver volúmenes
docker volume ls | grep kairo
```

### Ejecutar Comandos en Contenedores

```bash
# Shell en PostgreSQL
docker exec -it kairo-postgres psql -U postgres

# Shell en MongoDB
docker exec -it kairo-mongodb mongosh

# Shell en Keycloak
docker exec -it kairo-keycloak bash

# Ver logs de Gateway
docker exec kairo-gateway cat /app/logs/gateway-*.log
```

## 🔧 Troubleshooting

### Error: "network kairo-network not found"

```bash
docker network create kairo-network
```

### Error: "port is already allocated"

Verifica que no tengas otros servicios usando los puertos:

```bash
# Windows
netstat -ano | findstr "8080"
netstat -ano | findstr "8180"
netstat -ano | findstr "5432"
netstat -ano | findstr "27017"
netstat -ano | findstr "5672"

# Linux/Mac
lsof -i :8080
lsof -i :8180
lsof -i :5432
lsof -i :27017
lsof -i :5672
```

### Keycloak no Inicia

```bash
# Ver logs de Keycloak
docker-compose logs keycloak

# Verificar que PostgreSQL está listo
docker exec kairo-postgres pg_isready -U postgres

# Reiniciar Keycloak
docker-compose restart keycloak
```

### Gateway no se Conecta a Keycloak

```bash
# Verificar que Keycloak está disponible
curl http://localhost:8180/health/ready

# Ver logs del Gateway
docker-compose logs gateway

# Verificar conectividad desde el Gateway
docker exec kairo-gateway curl http://keycloak:8080/health/ready
```

### Realm no se Importa

```bash
# Verificar que el archivo existe
ls -la configs/keycloak/realm-export.json

# Ver logs de importación
docker-compose logs keycloak | grep import

# Forzar reimportación (elimina datos existentes)
docker-compose down -v
docker-compose up -d
```

### Resetear Base de Datos PostgreSQL

```bash
docker-compose down
docker volume rm kairo_postgres_data
docker-compose up -d
```

### Resetear MongoDB

```bash
docker-compose down
docker volume rm kairo_mongodb_data
docker-compose up -d
```

### Resetear RabbitMQ

```bash
docker-compose down
docker volume rm kairo_rabbitmq_data
docker-compose up -d
```

### Resetear Keycloak (Reimportar Realm)

```bash
docker-compose down
docker volume rm kairo_postgres_data  # Keycloak usa PostgreSQL
docker-compose up -d
# El realm se importará automáticamente
```

## 📁 Estructura de Archivos

```
Infraestructura/
├── docker-compose.yml          # Definición de todos los servicios
├── configs/
│   ├── keycloak/
│   │   └── realm-export.json  # Configuración del realm Kairo
│   └── postgres/
│       └── init.sql           # Script de inicialización de BD
├── .env.example               # Variables de entorno de ejemplo
├── start.ps1                  # Script de inicio (Windows)
├── stop.ps1                   # Script de detención (Windows)
└── README.md                  # Esta documentación
```

## 🔐 Seguridad

⚠️ **IMPORTANTE**: Las credenciales por defecto son solo para desarrollo local.

### Para Producción

1. **Cambiar todas las contraseñas:**
   - Keycloak admin
   - PostgreSQL
   - RabbitMQ
   - Usuarios del realm

2. **Usar variables de entorno:**
   - No hardcodear credenciales
   - Usar secrets management (Azure Key Vault, AWS Secrets Manager)

3. **Configurar SSL/TLS:**
   - HTTPS para Keycloak
   - TLS para PostgreSQL
   - TLS para RabbitMQ

4. **Restringir acceso:**
   - Firewall rules
   - Network policies
   - No exponer puertos innecesarios

5. **Configurar Keycloak:**
   - Habilitar `sslRequired: "external"`
   - Configurar password policies
   - Habilitar brute force protection
   - Configurar session timeouts

## 🌐 Red Docker

La red `kairo-network` permite:

- ✅ Comunicación entre microservicios
- ✅ Aislamiento de otros proyectos
- ✅ Resolución de nombres por servicio
- ✅ Despliegue independiente de cada microservicio
- ✅ Escalabilidad horizontal

### Verificar la Red

```bash
# Ver información de la red
docker network inspect kairo-network

# Ver contenedores conectados
docker network inspect kairo-network | jq '.[0].Containers'
```

## 📊 Volúmenes de Datos

Los datos se persisten en volúmenes Docker:

| Volumen | Servicio | Datos |
|---------|----------|-------|
| `kairo_postgres_data` | PostgreSQL | Bases de datos relacionales + Keycloak |
| `kairo_mongodb_data` | MongoDB | Modelos de lectura de reportes |
| `kairo_rabbitmq_data` | RabbitMQ | Colas y mensajes persistentes |

### Backup de Datos

```bash
# Backup PostgreSQL
docker exec kairo-postgres pg_dumpall -U postgres > backup_postgres.sql

# Backup MongoDB
docker exec kairo-mongodb mongodump --out /backup
docker cp kairo-mongodb:/backup ./backup_mongodb

# Backup RabbitMQ (definiciones)
curl -u guest:guest http://localhost:15672/api/definitions > backup_rabbitmq.json
```

### Restaurar Datos

```bash
# Restaurar PostgreSQL
docker exec -i kairo-postgres psql -U postgres < backup_postgres.sql

# Restaurar MongoDB
docker cp ./backup_mongodb kairo-mongodb:/backup
docker exec kairo-mongodb mongorestore /backup

# Restaurar RabbitMQ
curl -u guest:guest -X POST -H "Content-Type: application/json" \
  -d @backup_rabbitmq.json http://localhost:15672/api/definitions
```

## 📝 Notas

- Los volúmenes persisten los datos entre reinicios
- Los health checks aseguran que los servicios estén listos antes de que otros servicios se conecten
- Cada microservicio debe tener su propio `docker-compose.yml` que se conecte a `kairo-network`
- El Gateway actúa como único punto de entrada - los microservicios no deben exponerse directamente
- Keycloak se configura automáticamente - no se requiere configuración manual

## 🔗 Referencias

- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [Keycloak Documentation](https://www.keycloak.org/documentation)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [MongoDB Documentation](https://www.mongodb.com/docs/)
- [RabbitMQ Documentation](https://www.rabbitmq.com/documentation.html)
- [Gateway README](../Gateway/README.md)

## 📄 Licencia

Este proyecto es parte del sistema Kairo Microservices.
