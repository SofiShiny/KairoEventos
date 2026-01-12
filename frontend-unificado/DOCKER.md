# Docker Deployment Guide - Frontend Unificado

Este documento describe cómo construir y desplegar el Frontend Unificado usando Docker.

## Requisitos Previos

- Docker 20.10+
- Docker Compose 2.0+
- Red externa `kairo-network` creada

## Crear Red Externa

Si la red `kairo-network` no existe, créala primero:

```bash
docker network create kairo-network
```

## Construcción de Imagen

### Producción

Construir la imagen de producción:

```bash
docker build -t frontend-unificado:latest .
```

Verificar el tamaño de la imagen:

```bash
docker images frontend-unificado:latest
```

**Tamaño esperado**: ~50-80MB (nginx:alpine + archivos estáticos)

### Desarrollo

Construir la imagen de desarrollo:

```bash
docker build -f Dockerfile.dev -t frontend-unificado:dev .
```

## Ejecución con Docker Compose

### Producción

```bash
# Iniciar el servicio
docker-compose up -d

# Ver logs
docker-compose logs -f frontend-unificado

# Detener el servicio
docker-compose down
```

### Desarrollo (con hot reload)

```bash
# Iniciar el servidor de desarrollo
docker-compose -f docker-compose.dev.yml up -d

# Ver logs
docker-compose -f docker-compose.dev.yml logs -f

# Detener el servicio
docker-compose -f docker-compose.dev.yml down
```

## Ejecución Manual con Docker

### Producción

```bash
docker run -d \
  --name frontend-unificado \
  --network kairo-network \
  -p 3000:80 \
  frontend-unificado:latest
```

### Desarrollo

```bash
docker run -d \
  --name frontend-unificado-dev \
  --network kairo-network \
  -p 3000:3000 \
  -v $(pwd)/src:/app/src \
  -v $(pwd)/public:/app/public \
  -e VITE_GATEWAY_URL=http://gateway:8080 \
  -e VITE_KEYCLOAK_URL=http://localhost:8180 \
  -e VITE_KEYCLOAK_REALM=Kairo \
  -e VITE_KEYCLOAK_CLIENT_ID=kairo-web \
  frontend-unificado:dev
```

## Variables de Entorno

Las variables de entorno se configuran en **tiempo de build** (no runtime) porque Vite las embebe en el código durante la compilación.

### Variables Requeridas

- `VITE_GATEWAY_URL`: URL del Gateway API (default: http://localhost:8080)
- `VITE_KEYCLOAK_URL`: URL de Keycloak (default: http://localhost:8180)
- `VITE_KEYCLOAK_REALM`: Realm de Keycloak (default: Kairo)
- `VITE_KEYCLOAK_CLIENT_ID`: Client ID de Keycloak (default: kairo-web)

### Configurar Variables en Build

Para producción con variables personalizadas:

```bash
docker build \
  --build-arg VITE_GATEWAY_URL=https://api.kairo.com \
  --build-arg VITE_KEYCLOAK_URL=https://auth.kairo.com \
  --build-arg VITE_KEYCLOAK_REALM=Kairo \
  --build-arg VITE_KEYCLOAK_CLIENT_ID=kairo-web \
  -t frontend-unificado:prod .
```

**Nota**: Para esto, necesitas modificar el Dockerfile para aceptar build args.

## Verificación de Salud

El contenedor incluye un health check que verifica cada 30 segundos:

```bash
# Ver estado de salud
docker inspect --format='{{.State.Health.Status}}' frontend-unificado

# Ver logs de health check
docker inspect --format='{{range .State.Health.Log}}{{.Output}}{{end}}' frontend-unificado
```

## Optimizaciones de Imagen

### Tamaño de Imagen

La imagen final está optimizada para ser pequeña:

1. **Multi-stage build**: Solo archivos necesarios en imagen final
2. **Alpine Linux**: Base image mínima (~5MB)
3. **Nginx Alpine**: Servidor web ligero (~25MB)
4. **Assets compilados**: Solo archivos estáticos (~20-50MB)

### Compresión Gzip

Nginx está configurado para comprimir automáticamente:
- HTML, CSS, JS
- JSON, XML
- Fuentes (TTF, WOFF, etc.)
- SVG

### Cache de Assets

Los assets estáticos tienen cache de 1 año:
- JavaScript bundles
- CSS stylesheets
- Imágenes
- Fuentes

El `index.html` NO tiene cache para asegurar que los usuarios obtengan la última versión.

## Nginx Configuration

La configuración de nginx incluye:

### SPA Routing

Todas las rutas sirven `index.html` para que React Router funcione:

```nginx
location / {
    try_files $uri $uri/ /index.html;
}
```

### Security Headers

Headers de seguridad configurados:
- `X-Frame-Options: SAMEORIGIN`
- `X-Content-Type-Options: nosniff`
- `X-XSS-Protection: 1; mode=block`
- `Referrer-Policy: strict-origin-when-cross-origin`

### Gzip Compression

Compresión habilitada para todos los tipos de contenido relevantes.

## Troubleshooting

### Problema: La aplicación no carga

**Solución**: Verificar que el Gateway esté accesible:

```bash
# Desde dentro del contenedor
docker exec -it frontend-unificado wget -O- http://gateway:8080/health

# Desde el host
curl http://localhost:8080/health
```

### Problema: Variables de entorno incorrectas

**Solución**: Las variables de Vite se configuran en build time. Reconstruir la imagen:

```bash
docker-compose down
docker-compose build --no-cache
docker-compose up -d
```

### Problema: Rutas de React Router no funcionan

**Solución**: Verificar que nginx.conf esté correctamente copiado:

```bash
docker exec -it frontend-unificado cat /etc/nginx/conf.d/default.conf
```

### Problema: Imagen muy grande

**Solución**: Verificar que .dockerignore esté presente y excluya node_modules:

```bash
# Verificar tamaño de cada capa
docker history frontend-unificado:latest
```

## Integración con CI/CD

### GitHub Actions Example

```yaml
name: Build and Push Docker Image

on:
  push:
    branches: [main]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Build Docker image
        run: docker build -t frontend-unificado:${{ github.sha }} .
      
      - name: Tag as latest
        run: docker tag frontend-unificado:${{ github.sha }} frontend-unificado:latest
      
      - name: Push to registry
        run: |
          docker push frontend-unificado:${{ github.sha }}
          docker push frontend-unificado:latest
```

## Monitoreo

### Logs

Ver logs en tiempo real:

```bash
docker-compose logs -f frontend-unificado
```

### Métricas

Nginx expone métricas básicas. Para métricas avanzadas, considerar:
- Prometheus + nginx-exporter
- Datadog
- New Relic

## Backup y Restore

No hay datos persistentes en el frontend. Solo reconstruir la imagen si es necesario.

## Actualización

Para actualizar a una nueva versión:

```bash
# Pull latest code
git pull origin main

# Rebuild and restart
docker-compose down
docker-compose build --no-cache
docker-compose up -d
```

## Seguridad

### Escaneo de Vulnerabilidades

Escanear la imagen con Trivy:

```bash
docker run --rm -v /var/run/docker.sock:/var/run/docker.sock \
  aquasec/trivy image frontend-unificado:latest
```

### Actualizaciones de Base Image

Mantener actualizada la base image:

```bash
docker pull node:18-alpine
docker pull nginx:alpine
docker-compose build --no-cache
```

## Performance

### Métricas Esperadas

- **Tamaño de imagen**: 50-80MB
- **Tiempo de build**: 2-5 minutos
- **Tiempo de inicio**: <5 segundos
- **Memoria en runtime**: ~10-20MB (nginx)

### Optimizaciones Aplicadas

1. Multi-stage build
2. Gzip compression
3. Asset caching (1 year)
4. Code splitting (Vite)
5. Tree shaking (Vite)
6. Minification (Vite)

## Referencias

- [Vite Build Guide](https://vitejs.dev/guide/build.html)
- [Nginx Docker Official Image](https://hub.docker.com/_/nginx)
- [Docker Multi-stage Builds](https://docs.docker.com/build/building/multi-stage/)
