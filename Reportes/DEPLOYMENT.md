# Guía de Deployment - Microservicio de Reportes

Esta guía describe el proceso completo de deployment del microservicio de Reportes en diferentes ambientes.

## 📋 Tabla de Contenidos

- [Requisitos Previos](#requisitos-previos)
- [Ambientes](#ambientes)
- [Scripts de Deployment](#scripts-de-deployment)
- [Proceso de Deployment](#proceso-de-deployment)
- [Configuración de Variables de Entorno](#configuración-de-variables-de-entorno)
- [Deployment en Kubernetes](#deployment-en-kubernetes)
- [Rollback](#rollback)
- [Monitoreo Post-Deployment](#monitoreo-post-deployment)
- [Troubleshooting](#troubleshooting)

## Requisitos Previos

### Software Necesario

- **Docker** 20.10 o superior
- **Docker Compose** 2.0 o superior
- **.NET 8 SDK** (para builds locales)
- **Git** para control de versiones

### Accesos Requeridos

- Acceso al repositorio de código
- Credenciales para Docker Registry (si aplica)
- Acceso a servidores de deployment
- Credenciales de MongoDB y RabbitMQ para cada ambiente

## Ambientes

El microservicio soporta tres ambientes:

### Development
- **Propósito:** Desarrollo local y pruebas
- **Base de datos:** MongoDB local
- **Mensajería:** RabbitMQ local
- **Logging:** Nivel Debug
- **Swagger:** Habilitado

### Staging
- **Propósito:** Pruebas de integración y QA
- **Base de datos:** MongoDB compartido (staging)
- **Mensajería:** RabbitMQ compartido (staging)
- **Logging:** Nivel Information
- **Swagger:** Habilitado

### Production
- **Propósito:** Ambiente productivo
- **Base de datos:** MongoDB cluster (producción)
- **Mensajería:** RabbitMQ cluster (producción)
- **Logging:** Nivel Warning
- **Swagger:** Deshabilitado

## Scripts de Deployment

### Linux/Mac: deploy.sh

```bash
# Deployment básico
./deploy.sh development

# Deployment a producción con registry
./deploy.sh production --registry=myregistry.azurecr.io --tag=v1.0.0

# Build sin deployment
./deploy.sh staging --build-only

# Deployment sin tests (no recomendado)
./deploy.sh production --skip-tests
```

### Windows: deploy.ps1

```powershell
# Deployment básico
.\deploy.ps1 -Ambiente development

# Deployment a producción con registry
.\deploy.ps1 -Ambiente production -DockerRegistry myregistry.azurecr.io -ImageTag v1.0.0

# Build sin deployment
.\deploy.ps1 -Ambiente staging -BuildOnly

# Deployment sin tests (no recomendado)
.\deploy.ps1 -Ambiente production -SkipTests
```

## Proceso de Deployment

### 1. Pre-Deployment

#### Verificar Estado del Código

```bash
# Asegurarse de estar en la rama correcta
git checkout main
git pull origin main

# Verificar que no haya cambios sin commitear
git status
```

#### Ejecutar Tests Localmente

```bash
cd backend/src/Services/Reportes
dotnet test --configuration Release
```

#### Revisar Variables de Entorno

```bash
# Editar archivo de ambiente correspondiente
nano .env.production

# Verificar que todas las variables estén configuradas
cat .env.production
```

### 2. Deployment

#### Opción A: Usando Script Automatizado (Recomendado)

```bash
# Linux/Mac
./deploy.sh production --registry=myregistry.azurecr.io --tag=v1.0.0

# Windows
.\deploy.ps1 -Ambiente production -DockerRegistry myregistry.azurecr.io -ImageTag v1.0.0
```

El script ejecutará automáticamente:
1. Validación de ambiente
2. Carga de variables de entorno
3. Ejecución de tests
4. Build de la aplicación
5. Build de imagen Docker
6. Push a registry (si se especifica)
7. Deployment con docker-compose
8. Verificación de health checks

#### Opción B: Deployment Manual

```bash
# 1. Cargar variables de entorno
source .env.production

# 2. Ejecutar tests
cd backend/src/Services/Reportes
dotnet test --configuration Release

# 3. Build de imagen Docker
cd ../../../../
docker build -t reportes-api:v1.0.0 -f Dockerfile .

# 4. Tag para registry
docker tag reportes-api:v1.0.0 myregistry.azurecr.io/reportes-api:v1.0.0

# 5. Push a registry
docker push myregistry.azurecr.io/reportes-api:v1.0.0

# 6. Deploy con docker-compose
docker-compose down
docker-compose up -d --build

# 7. Verificar health check
curl http://localhost:5002/health
```

### 3. Post-Deployment

#### Verificar Servicios

```bash
# Verificar que los contenedores estén corriendo
docker-compose ps

# Verificar logs
docker-compose logs -f reportes-api

# Verificar health check
curl http://localhost:5002/health
```

#### Smoke Tests

```bash
# Test de API
curl http://localhost:5002/api/reportes/resumen-ventas

# Test de Swagger (solo en dev/staging)
curl http://localhost:5002/swagger/index.html

# Test de Hangfire
curl http://localhost:5002/hangfire
```

#### Verificar Consumidores de Eventos

```bash
# Acceder a RabbitMQ Management
# http://localhost:15672

# Verificar que las colas estén creadas
# Verificar que los consumidores estén conectados
```

## Configuración de Variables de Entorno

### Variables Requeridas

| Variable | Descripción | Ejemplo |
|----------|-------------|---------|
| `MONGODB_CONNECTION_STRING` | Cadena de conexión a MongoDB | `mongodb://user:pass@host:27017` |
| `MONGODB_DATABASE` | Nombre de la base de datos | `reportes_db_prod` |
| `RABBITMQ_HOST` | Host de RabbitMQ | `rabbitmq-prod` |
| `RABBITMQ_PORT` | Puerto de RabbitMQ | `5672` |
| `RABBITMQ_USER` | Usuario de RabbitMQ | `reportes_user` |
| `RABBITMQ_PASSWORD` | Contraseña de RabbitMQ | `secure_password` |
| `ASPNETCORE_ENVIRONMENT` | Ambiente de ASP.NET Core | `Production` |
| `ASPNETCORE_URLS` | URLs donde escucha la API | `http://0.0.0.0:5002` |

### Variables Opcionales

| Variable | Descripción | Default |
|----------|-------------|---------|
| `HANGFIRE_CRON_CONSOLIDACION` | Expresión cron para job | `0 2 * * *` |
| `SERILOG_MINIMUM_LEVEL` | Nivel mínimo de logging | `Information` |
| `CORS_ALLOWED_ORIGINS` | Orígenes permitidos para CORS | `*` |
| `ENABLE_SWAGGER` | Habilitar Swagger UI | `true` |

### Gestión de Secretos

#### Opción 1: Variables de Entorno del Sistema

```bash
# Linux/Mac
export MONGODB_CONNECTION_STRING="mongodb://..."
export RABBITMQ_PASSWORD="..."

# Windows
$env:MONGODB_CONNECTION_STRING="mongodb://..."
$env:RABBITMQ_PASSWORD="..."
```

#### Opción 2: Archivos .env (No commitear)

```bash
# Crear archivo .env.production.local (gitignored)
cp .env.production .env.production.local

# Editar con credenciales reales
nano .env.production.local
```

#### Opción 3: Azure Key Vault / AWS Secrets Manager

```bash
# Ejemplo con Azure CLI
az keyvault secret set \
  --vault-name my-keyvault \
  --name mongodb-connection-string \
  --value "mongodb://..."

# Recuperar en runtime
az keyvault secret show \
  --vault-name my-keyvault \
  --name mongodb-connection-string \
  --query value -o tsv
```

## Deployment en Kubernetes

### Crear Manifiestos

```yaml
# deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: reportes-api
  namespace: eventos
spec:
  replicas: 3
  selector:
    matchLabels:
      app: reportes-api
  template:
    metadata:
      labels:
        app: reportes-api
    spec:
      containers:
      - name: reportes-api
        image: myregistry.azurecr.io/reportes-api:v1.0.0
        ports:
        - containerPort: 5002
        env:
        - name: MONGODB_CONNECTION_STRING
          valueFrom:
            secretKeyRef:
              name: reportes-secrets
              key: mongodb-connection-string
        - name: RABBITMQ_PASSWORD
          valueFrom:
            secretKeyRef:
              name: reportes-secrets
              key: rabbitmq-password
        livenessProbe:
          httpGet:
            path: /health
            port: 5002
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health
            port: 5002
          initialDelaySeconds: 10
          periodSeconds: 5
---
apiVersion: v1
kind: Service
metadata:
  name: reportes-api-service
  namespace: eventos
spec:
  selector:
    app: reportes-api
  ports:
  - protocol: TCP
    port: 80
    targetPort: 5002
  type: LoadBalancer
```

### Aplicar Manifiestos

```bash
# Crear namespace
kubectl create namespace eventos

# Crear secrets
kubectl create secret generic reportes-secrets \
  --from-literal=mongodb-connection-string="mongodb://..." \
  --from-literal=rabbitmq-password="..." \
  -n eventos

# Aplicar deployment
kubectl apply -f deployment.yaml

# Verificar deployment
kubectl get pods -n eventos
kubectl logs -f deployment/reportes-api -n eventos
```

## Rollback

### Rollback con Docker Compose

```bash
# Opción 1: Volver a imagen anterior
docker-compose down
docker-compose pull reportes-api:previous-tag
docker-compose up -d

# Opción 2: Usar backup de volúmenes
docker-compose down
docker volume rm reportes_mongodb_data
docker volume create reportes_mongodb_data
# Restaurar backup de MongoDB
docker-compose up -d
```

### Rollback en Kubernetes

```bash
# Ver historial de deployments
kubectl rollout history deployment/reportes-api -n eventos

# Rollback a versión anterior
kubectl rollout undo deployment/reportes-api -n eventos

# Rollback a versión específica
kubectl rollout undo deployment/reportes-api --to-revision=2 -n eventos

# Verificar rollback
kubectl rollout status deployment/reportes-api -n eventos
```

## Monitoreo Post-Deployment

### Métricas a Monitorear

1. **Health Checks**
   ```bash
   watch -n 5 'curl -s http://localhost:5002/health | jq'
   ```

2. **Logs de Aplicación**
   ```bash
   docker-compose logs -f --tail=100 reportes-api
   ```

3. **Consumo de Recursos**
   ```bash
   docker stats reportes-api
   ```

4. **Colas de RabbitMQ**
   - Acceder a http://localhost:15672
   - Verificar que no haya mensajes acumulados
   - Verificar tasa de consumo

5. **MongoDB**
   ```bash
   # Conectar a MongoDB
   mongo mongodb://localhost:27017/reportes_db_prod
   
   # Verificar colecciones
   show collections
   
   # Verificar últimos registros
   db.logs_auditoria.find().sort({Timestamp: -1}).limit(10)
   ```

### Alertas Recomendadas

- Health check fallando por más de 2 minutos
- Tasa de errores > 5%
- Latencia de API > 1 segundo
- Colas de RabbitMQ con más de 1000 mensajes
- Uso de CPU > 80%
- Uso de memoria > 90%
- Disco de MongoDB > 85% lleno

## Troubleshooting

### Problema: Deployment Falla en Tests

```bash
# Ver logs detallados de tests
dotnet test --logger "console;verbosity=detailed"

# Ejecutar solo tests que fallaron
dotnet test --filter "FullyQualifiedName~FailingTest"
```

### Problema: Imagen Docker No Se Construye

```bash
# Limpiar cache de Docker
docker builder prune -a

# Build con logs detallados
docker build --no-cache --progress=plain -t reportes-api:debug .
```

### Problema: Health Check Falla

```bash
# Verificar logs de la aplicación
docker-compose logs reportes-api

# Verificar conectividad a MongoDB
docker-compose exec reportes-api ping mongodb

# Verificar conectividad a RabbitMQ
docker-compose exec reportes-api ping rabbitmq

# Entrar al contenedor para debugging
docker-compose exec reportes-api /bin/bash
```

### Problema: Consumidores No Procesan Eventos

```bash
# Verificar que RabbitMQ esté corriendo
docker-compose ps rabbitmq

# Verificar colas en RabbitMQ Management
# http://localhost:15672

# Verificar logs de consumidores
docker-compose logs reportes-api | grep Consumer

# Reiniciar servicio
docker-compose restart reportes-api
```

## Checklist de Deployment

### Pre-Deployment
- [ ] Código en rama correcta (main/release)
- [ ] Tests pasando localmente
- [ ] Variables de entorno configuradas
- [ ] Credenciales verificadas
- [ ] Backup de base de datos realizado
- [ ] Notificación a equipo de deployment

### Durante Deployment
- [ ] Script de deployment ejecutado sin errores
- [ ] Imagen Docker construida correctamente
- [ ] Imagen pusheada a registry
- [ ] Servicios levantados con docker-compose
- [ ] Health checks pasando

### Post-Deployment
- [ ] Smoke tests ejecutados
- [ ] Logs verificados (sin errores críticos)
- [ ] Consumidores conectados a RabbitMQ
- [ ] Job de Hangfire programado
- [ ] Métricas de monitoreo activas
- [ ] Documentación actualizada
- [ ] Notificación a equipo de deployment completado

## Contacto y Soporte

Para problemas durante el deployment:
- **Email:** devops@eventos.com
- **Slack:** #deployments
- **On-call:** +1-555-0100

## Referencias

- [Documentación de Docker](https://docs.docker.com/)
- [Documentación de Kubernetes](https://kubernetes.io/docs/)
- [Documentación de .NET](https://docs.microsoft.com/dotnet/)
- [README del Proyecto](README.md)
