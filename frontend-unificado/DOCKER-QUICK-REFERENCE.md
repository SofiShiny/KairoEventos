# Docker Quick Reference - Frontend Unificado

Comandos rápidos para trabajar con el Frontend Unificado dockerizado.

## 🚀 Quick Start

```bash
# 1. Crear red (solo una vez)
docker network create kairo-network

# 2. Iniciar aplicación
docker-compose up -d

# 3. Ver logs
docker-compose logs -f

# 4. Detener aplicación
docker-compose down
```

## 🔨 Build Commands

### Producción

```bash
# Build manual
docker build -t frontend-unificado:latest .

# Build con script (Linux/Mac)
./build-docker.sh production

# Build con script (Windows)
.\build-docker.ps1 production

# Build sin cache
docker build --no-cache -t frontend-unificado:latest .
```

### Desarrollo

```bash
# Build imagen de desarrollo
docker build -f Dockerfile.dev -t frontend-unificado:dev .

# Build con script
./build-docker.sh development
```

## 🏃 Run Commands

### Con Docker Compose (Recomendado)

```bash
# Producción
docker-compose up -d

# Desarrollo (hot reload)
docker-compose -f docker-compose.dev.yml up -d

# Ver logs en tiempo real
docker-compose logs -f

# Detener
docker-compose down

# Detener y eliminar volúmenes
docker-compose down -v
```

### Manual con Docker Run

```bash
# Producción
docker run -d \
  --name frontend-unificado \
  --network kairo-network \
  -p 3000:80 \
  frontend-unificado:latest

# Desarrollo
docker run -d \
  --name frontend-unificado-dev \
  --network kairo-network \
  -p 3000:3000 \
  -v $(pwd)/src:/app/src \
  -e VITE_GATEWAY_URL=http://gateway:8080 \
  frontend-unificado:dev
```

## 🔍 Inspection Commands

```bash
# Ver imágenes
docker images frontend-unificado

# Ver contenedores corriendo
docker ps | grep frontend

# Ver todos los contenedores
docker ps -a | grep frontend

# Inspeccionar contenedor
docker inspect frontend-unificado

# Ver logs
docker logs frontend-unificado

# Ver logs en tiempo real
docker logs -f frontend-unificado

# Ver últimas 100 líneas
docker logs --tail 100 frontend-unificado

# Ver estadísticas de recursos
docker stats frontend-unificado

# Ver health status
docker inspect --format='{{.State.Health.Status}}' frontend-unificado

# Ver health logs
docker inspect --format='{{range .State.Health.Log}}{{.Output}}{{end}}' frontend-unificado
```

## 🐛 Debug Commands

```bash
# Entrar al contenedor (producción - nginx)
docker exec -it frontend-unificado sh

# Entrar al contenedor (desarrollo - node)
docker exec -it frontend-unificado-dev sh

# Ver archivos servidos por nginx
docker exec frontend-unificado ls -la /usr/share/nginx/html

# Ver configuración de nginx
docker exec frontend-unificado cat /etc/nginx/conf.d/default.conf

# Probar nginx config
docker exec frontend-unificado nginx -t

# Recargar nginx
docker exec frontend-unificado nginx -s reload

# Ver procesos en contenedor
docker exec frontend-unificado ps aux
```

## 🧹 Cleanup Commands

```bash
# Detener contenedor
docker stop frontend-unificado

# Eliminar contenedor
docker rm frontend-unificado

# Detener y eliminar
docker rm -f frontend-unificado

# Eliminar imagen
docker rmi frontend-unificado:latest

# Eliminar imágenes sin usar
docker image prune

# Eliminar todo (contenedores, imágenes, volúmenes)
docker system prune -a --volumes
```

## 🌐 Network Commands

```bash
# Crear red
docker network create kairo-network

# Ver redes
docker network ls

# Inspeccionar red
docker network inspect kairo-network

# Ver contenedores en red
docker network inspect kairo-network --format='{{range .Containers}}{{.Name}} {{end}}'

# Conectar contenedor a red
docker network connect kairo-network frontend-unificado

# Desconectar contenedor de red
docker network disconnect kairo-network frontend-unificado
```

## 📊 Monitoring Commands

```bash
# Ver uso de recursos en tiempo real
docker stats frontend-unificado

# Ver eventos del contenedor
docker events --filter container=frontend-unificado

# Ver historial de imagen
docker history frontend-unificado:latest

# Ver capas de imagen
docker history --no-trunc frontend-unificado:latest
```

## 🔐 Security Commands

```bash
# Escanear vulnerabilidades con Trivy
docker run --rm -v /var/run/docker.sock:/var/run/docker.sock \
  aquasec/trivy image frontend-unificado:latest

# Ver información de seguridad
docker inspect --format='{{.Config.User}}' frontend-unificado

# Verificar que no corre como root
docker exec frontend-unificado whoami
```

## 📦 Registry Commands

```bash
# Tag para registry
docker tag frontend-unificado:latest registry.example.com/frontend-unificado:latest

# Push a registry
docker push registry.example.com/frontend-unificado:latest

# Pull desde registry
docker pull registry.example.com/frontend-unificado:latest

# Login a registry
docker login registry.example.com
```

## 🧪 Testing Commands

```bash
# Probar que la aplicación responde
curl http://localhost:3000

# Probar con headers
curl -I http://localhost:3000

# Verificar gzip
curl -H "Accept-Encoding: gzip" -I http://localhost:3000/assets/index.js

# Verificar security headers
curl -I http://localhost:3000 | grep -E "X-Frame-Options|X-Content-Type-Options"

# Probar health endpoint
curl http://localhost:3000/

# Probar SPA routing
curl http://localhost:3000/eventos
curl http://localhost:3000/usuarios
```

## 🔄 Update Commands

```bash
# Pull latest code
git pull origin main

# Rebuild y restart
docker-compose down
docker-compose build --no-cache
docker-compose up -d

# O con script
./build-docker.sh production
docker-compose up -d
```

## 💾 Backup Commands

```bash
# Exportar imagen
docker save frontend-unificado:latest | gzip > frontend-unificado-latest.tar.gz

# Importar imagen
gunzip -c frontend-unificado-latest.tar.gz | docker load

# Exportar contenedor
docker export frontend-unificado > frontend-unificado-container.tar

# Importar contenedor
docker import frontend-unificado-container.tar frontend-unificado:imported
```

## 🎯 Common Workflows

### Desarrollo Local

```bash
# 1. Iniciar desarrollo
docker-compose -f docker-compose.dev.yml up -d

# 2. Ver logs
docker-compose -f docker-compose.dev.yml logs -f

# 3. Hacer cambios en src/
# (Hot reload automático)

# 4. Detener
docker-compose -f docker-compose.dev.yml down
```

### Deploy a Producción

```bash
# 1. Build
./build-docker.sh production

# 2. Test localmente
docker run -d --name frontend-test -p 3000:80 frontend-unificado:latest
curl http://localhost:3000
docker rm -f frontend-test

# 3. Tag para registry
docker tag frontend-unificado:latest registry.example.com/frontend-unificado:v1.0.0

# 4. Push
docker push registry.example.com/frontend-unificado:v1.0.0

# 5. Deploy en servidor
# (SSH al servidor y pull + run)
```

### Troubleshooting

```bash
# 1. Ver logs
docker logs --tail 100 frontend-unificado

# 2. Verificar health
docker inspect --format='{{.State.Health.Status}}' frontend-unificado

# 3. Entrar al contenedor
docker exec -it frontend-unificado sh

# 4. Verificar nginx
docker exec frontend-unificado nginx -t

# 5. Ver archivos
docker exec frontend-unificado ls -la /usr/share/nginx/html

# 6. Rebuild sin cache
docker-compose down
docker-compose build --no-cache
docker-compose up -d
```

## 📚 Recursos Adicionales

- [DOCKER.md](./DOCKER.md) - Documentación completa
- [Dockerfile](./Dockerfile) - Dockerfile de producción
- [docker-compose.yml](./docker-compose.yml) - Configuración Docker Compose
- [nginx.conf](./nginx.conf) - Configuración Nginx

## 💡 Tips

1. **Siempre usar docker-compose** para gestión más fácil
2. **Ver logs regularmente** para detectar problemas temprano
3. **Usar --no-cache** si hay problemas de build
4. **Verificar health status** antes de considerar el contenedor listo
5. **Limpiar imágenes viejas** regularmente con `docker image prune`
6. **Usar tags con versión** en producción, no solo `latest`
7. **Escanear vulnerabilidades** antes de deploy a producción
8. **Monitorear recursos** con `docker stats` en producción

## 🆘 Ayuda

```bash
# Ver ayuda de Docker
docker --help

# Ver ayuda de comando específico
docker build --help
docker run --help
docker-compose --help

# Ver versión
docker --version
docker-compose --version
```
