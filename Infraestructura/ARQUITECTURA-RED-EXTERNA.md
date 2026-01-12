# Arquitectura de Red Externa - Kairo Microservices

## 📋 Resumen

Este documento describe la arquitectura de despliegue basada en **red Docker externa** para el ecosistema de microservicios Kairo.

## 🎯 Objetivo

Permitir que cada microservicio se despliegue de forma **independiente** en su propio repositorio, mientras mantienen la capacidad de comunicarse entre sí a través de una red compartida.

## 🏗️ Arquitectura

```
┌─────────────────────────────────────────────────────────────────┐
│                      kairo-network (bridge)                      │
│                                                                   │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐          │
│  │  PostgreSQL  │  │   MongoDB    │  │   RabbitMQ   │          │
│  │   :5432      │  │   :27017     │  │ :5672/:15672 │          │
│  └──────────────┘  └──────────────┘  └──────────────┘          │
│         ▲                 ▲                  ▲                    │
│         │                 │                  │                    │
│  ┌──────┴─────┐    ┌─────┴──────┐    ┌─────┴──────┐           │
│  │  Eventos   │    │  Reportes  │    │  Asientos  │           │
│  │  API:5000  │    │  API:5002  │    │  API:5100  │           │
│  └────────────┘    └────────────┘    └────────────┘           │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
```

## 🔑 Conceptos Clave

### 1. Red Externa (`kairo-network`)

- **Tipo**: Bridge network
- **Nombre**: `kairo-network`
- **Propósito**: Permitir comunicación entre contenedores de diferentes docker-compose
- **Creación**: Manual, una sola vez

```bash
docker network create kairo-network
```

### 2. Infraestructura Compartida

Servicios transversales que todos los microservicios necesitan:

- **PostgreSQL 16**: Base de datos relacional
- **MongoDB 7**: Base de datos NoSQL
- **RabbitMQ 3**: Message broker

**Ubicación**: Carpeta `infraestructura/` (puede estar en su propio repositorio)

### 3. Microservicios Independientes

Cada microservicio:
- Tiene su propio repositorio Git
- Tiene su propio `docker-compose.yml`
- Se conecta a `kairo-network` como red externa
- Puede desplegarse/actualizarse independientemente

## 📁 Estructura de Repositorios

```
kairo-infraestructura/          # Repositorio de infraestructura
├── docker-compose.yml
├── configs/
│   └── postgres/init.sql
└── README.md

kairo-eventos/                   # Repositorio de Eventos
├── docker-compose.yml          # Solo API de Eventos
├── Dockerfile
└── backend/

kairo-reportes/                  # Repositorio de Reportes
├── docker-compose.yml          # Solo API de Reportes
├── Dockerfile
└── backend/

kairo-asientos/                  # Repositorio de Asientos
├── docker-compose.yml          # Solo API de Asientos
├── Dockerfile
└── backend/
```

## 🔌 Configuración de Conexión

### Infraestructura (`infraestructura/docker-compose.yml`)

```yaml
networks:
  kairo-network:
    driver: bridge
    name: kairo-network

services:
  postgres:
    networks:
      - kairo-network
  mongodb:
    networks:
      - kairo-network
  rabbitmq:
    networks:
      - kairo-network
```

### Microservicio (`Eventos/docker-compose.yml`)

```yaml
networks:
  kairo-network:
    external: true  # ← Clave: marca la red como externa

services:
  eventos-api:
    environment:
      POSTGRES_HOST: postgres      # ← Usa nombre del servicio
      RABBITMQ_HOST: rabbitmq      # ← No localhost
    networks:
      - kairo-network
```

## 🚀 Flujo de Despliegue

### Paso 1: Crear Red (Una sola vez)

```bash
docker network create kairo-network
```

### Paso 2: Levantar Infraestructura

```bash
cd infraestructura
docker-compose up -d
```

### Paso 3: Levantar Microservicios (En cualquier orden)

```bash
# Terminal 1
cd kairo-eventos
docker-compose up -d

# Terminal 2
cd kairo-reportes
docker-compose up -d

# Terminal 3
cd kairo-asientos
docker-compose up -d
```

### Paso 4: Verificar Conectividad

```bash
# Ver todos los contenedores en la red
docker network inspect kairo-network

# Probar conectividad desde un contenedor
docker exec eventos-api ping postgres
docker exec reportes-api ping rabbitmq
```

## 🔄 Desarrollo Local vs Docker

### Desarrollo Local (Sin Docker)

Cuando ejecutas el código directamente en tu máquina:

```bash
# Variables de entorno
POSTGRES_HOST=localhost
MONGODB_HOST=localhost
RABBITMQ_HOST=localhost
```

### Desarrollo en Docker

Cuando ejecutas en contenedores:

```bash
# Variables de entorno
POSTGRES_HOST=postgres
MONGODB_HOST=mongodb
RABBITMQ_HOST=rabbitmq
```

### Detección Automática

Puedes usar variables de entorno para detectar el contexto:

```csharp
// En Program.cs
var postgresHost = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";
var rabbitHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
```

## ✅ Ventajas de Esta Arquitectura

1. **Desacoplamiento**: Cada microservicio en su propio repositorio
2. **Despliegue Independiente**: Actualiza un servicio sin afectar otros
3. **Escalabilidad**: Fácil agregar nuevos microservicios
4. **Desarrollo Flexible**: Trabaja en local o Docker según necesites
5. **CI/CD Simplificado**: Cada repo tiene su propio pipeline
6. **Versionado Independiente**: Cada servicio tiene su propio versionado

## ⚠️ Consideraciones

### 1. Orden de Inicio

La infraestructura debe estar corriendo antes que los microservicios:

```bash
# ✅ Correcto
docker-compose up -d  # En infraestructura
docker-compose up -d  # En eventos

# ❌ Incorrecto
docker-compose up -d  # En eventos (falla si infra no está)
```

### 2. Health Checks

Los servicios de infraestructura tienen health checks para asegurar que estén listos:

```yaml
healthcheck:
  test: ["CMD", "pg_isready", "-U", "postgres"]
  interval: 10s
  timeout: 5s
  retries: 5
```

### 3. Nombres de Servicio

Los nombres de servicio en la red deben ser consistentes:

- `postgres` (no `kairo-postgres` ni `eventos-postgres`)
- `mongodb` (no `kairo-mongodb`)
- `rabbitmq` (no `kairo-rabbitmq`)

### 4. Puertos

Evita conflictos de puertos entre microservicios:

- Eventos: 5000
- Reportes: 5002
- Asientos: 5100

## 🔧 Troubleshooting

### Error: "network kairo-network not found"

```bash
docker network create kairo-network
```

### Error: "could not resolve host: postgres"

El microservicio no está conectado a la red:

```yaml
# Verifica en docker-compose.yml
networks:
  kairo-network:
    external: true  # ← Debe estar presente
```

### Ver qué contenedores están en la red

```bash
docker network inspect kairo-network
```

### Probar conectividad entre servicios

```bash
# Desde eventos-api hacia postgres
docker exec eventos-api ping postgres

# Desde reportes-api hacia rabbitmq
docker exec reportes-api ping rabbitmq
```

## 📚 Referencias

- [Docker Networks Documentation](https://docs.docker.com/network/)
- [Docker Compose Networking](https://docs.docker.com/compose/networking/)
- [Microservices Communication Patterns](https://microservices.io/patterns/communication-style/messaging.html)

## 🎓 Próximos Pasos

1. ✅ Infraestructura compartida creada
2. ⏳ Actualizar docker-compose de Eventos
3. ⏳ Actualizar docker-compose de Reportes
4. ⏳ Actualizar docker-compose de Asientos
5. ⏳ Probar despliegue completo
6. ⏳ Documentar en READMEs individuales
