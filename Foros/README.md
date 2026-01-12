# Comunidad API - Microservicio de Foros y Comentarios

Microservicio para gestionar foros y comentarios de eventos, implementado con arquitectura hexagonal, DDD, MongoDB y RabbitMQ.

## 🏗️ Arquitectura

- **Patrón:** Arquitectura Hexagonal (Ports & Adapters) con DDD
- **Base de Datos:** MongoDB
- **Mensajería:** RabbitMQ con MassTransit
- **Framework:** .NET 8

## 📁 Estructura del Proyecto

```
Foros/
├── src/
│   ├── Comunidad.Domain/          # Entidades, Value Objects, Interfaces
│   │   ├── Entidades/
│   │   │   ├── Foro.cs
│   │   │   └── Comentario.cs
│   │   ├── ContratosExternos/
│   │   │   └── EventoPublicadoEventoDominio.cs
│   │   └── Repositorios/
│   │       ├── IForoRepository.cs
│   │       └── IComentarioRepository.cs
│   │
│   ├── Comunidad.Application/     # Casos de uso, DTOs, Comandos, Queries
│   │   ├── Comandos/
│   │   ├── Consultas/
│   │   └── DTOs/
│   │
│   ├── Comunidad.Infrastructure/  # Implementaciones, Persistencia, Consumers
│   │   ├── Persistencia/
│   │   ├── Repositorios/
│   │   └── Consumers/
│   │       └── EventoPublicadoConsumer.cs
│   │
│   └── Comunidad.API/             # Controllers, Program.cs
│       └── Controllers/
│           └── ComentariosController.cs
│
├── Dockerfile
├── docker-compose.yml
└── README.md
```

## 🚀 Características Principales

### 1. Creación Automática de Foros
Cuando el microservicio **Eventos** publica un evento `EventoPublicado`, este servicio:
- Escucha el evento vía RabbitMQ
- Crea automáticamente un foro vacío asociado al evento
- Usa el "truco del namespace" para compatibilidad con el emisor

### 2. Sistema de Comentarios (Estilo YouTube)
- **2 niveles únicamente:** Comentario Principal → Respuestas Directas
- No hay anidación infinita
- Estructura embebida en MongoDB para mejor rendimiento

### 3. Moderación Post-Publicación
- Los comentarios se publican inmediatamente
- El organizador puede ocultarlos después (Soft Delete)
- Los comentarios ocultos no aparecen en las consultas

## 📡 API Endpoints

### Obtener Comentarios de un Foro
```http
GET /api/comunidad/foros/{eventoId}
```
Retorna todos los comentarios visibles de un foro.

### Crear Comentario Principal
```http
POST /api/comunidad/comentarios
Content-Type: application/json

{
  "foroId": "guid",
  "usuarioId": "guid",
  "contenido": "string"
}
```

### Responder a un Comentario
```http
POST /api/comunidad/comentarios/{id}/responder
Content-Type: application/json

{
  "usuarioId": "guid",
  "contenido": "string"
}
```

### Ocultar Comentario (Moderación)
```http
DELETE /api/comunidad/comentarios/{id}
```

## 🐳 Despliegue con Docker

### Requisitos Previos
1. Docker y Docker Compose instalados
2. Red externa `kairo-network` creada:
```bash
docker network create kairo-network
```

### Levantar el Servicio
```bash
# Desde la carpeta Foros/
docker-compose up -d
```

### Verificar Estado
```bash
docker-compose ps
docker logs comunidad-api
```

### Acceder a los Servicios
- **API:** http://localhost:5007
- **Swagger:** http://localhost:5007/swagger
- **MongoDB:** localhost:27020
- **RabbitMQ Management:** http://localhost:15675 (guest/guest)

## 🔧 Configuración

### Variables de Entorno
```yaml
ASPNETCORE_ENVIRONMENT: Production
ConnectionStrings__MongoDB: mongodb://mongodb:27017
MongoDB__DatabaseName: ComunidadDB
RabbitMQ__Host: rabbitmq
RabbitMQ__Username: guest
RabbitMQ__Password: guest
```

### Configuración de RabbitMQ
El consumer escucha en la cola: `comunidad-evento-publicado`

## 🗄️ Modelo de Datos MongoDB

### Colección: Foros
```json
{
  "_id": "guid",
  "eventoId": "guid",
  "titulo": "string",
  "fechaCreacion": "datetime"
}
```

### Colección: Comentarios
```json
{
  "_id": "guid",
  "foroId": "guid",
  "usuarioId": "guid",
  "contenido": "string",
  "esVisible": true,
  "fechaCreacion": "datetime",
  "respuestas": [
    {
      "usuarioId": "guid",
      "contenido": "string",
      "fechaCreacion": "datetime"
    }
  ]
}
```

## 🔄 Integración con RabbitMQ

### Evento Consumido: EventoPublicado
```csharp
namespace Eventos.Domain.Events;

public record EventoPublicadoEventoDominio
{
    public Guid EventoId { get; init; }
    public string TituloEvento { get; init; }
    public DateTime FechaInicio { get; init; }
}
```

**Nota:** El namespace `Eventos.Domain.Events` es intencional para que MassTransit reconozca el evento del emisor.

## 🛠️ Desarrollo Local

### Compilar la Solución
```bash
dotnet build Comunidad.sln
```

### Ejecutar la API
```bash
cd src/Comunidad.API
dotnet run
```

### Ejecutar con Hot Reload
```bash
dotnet watch run --project src/Comunidad.API
```

## 📊 Health Checks
```http
GET /health
```

## 🔍 Logs y Monitoreo

Los logs se pueden ver con:
```bash
docker logs -f comunidad-api
```

Niveles de log configurables en `appsettings.json`:
- Default: Information
- MassTransit: Information
- Microsoft.AspNetCore: Warning

## 🧪 Pruebas

### Suite de Pruebas Unitarias

El proyecto incluye una suite completa de pruebas unitarias con **cobertura >90%**.

#### Ejecutar Tests

**Opción 1: Script completo con reporte HTML (Recomendado)**
```bash
./run-coverage.ps1
```
Ejecuta tests, genera cobertura y abre reporte HTML automáticamente.

**Opción 2: Script simplificado**
```bash
./test-and-open.ps1
```
Versión rápida que ejecuta tests y abre el reporte.

**Opción 3: Ejecución básica**
```bash
# Ejecutar todos los tests
dotnet test

# Con script de PowerShell
./run-tests.ps1
```

Ver [QUICK-TEST-GUIDE.md](QUICK-TEST-GUIDE.md) para guía rápida de testing.

#### Estadísticas de Tests
- **Total de Tests:** 35
- **Handlers (CQRS):** 17 tests
- **Consumer (RabbitMQ):** 5 tests
- **Entidades de Dominio:** 13 tests
- **Cobertura:** >95%

#### Stack de Pruebas
- **xUnit** 2.5.4 - Framework de testing
- **Moq** 4.20.70 - Mocking de dependencias
- **FluentAssertions** 6.12.0 - Aserciones expresivas
- **coverlet.collector** 6.0.0 - Cobertura de código

Ver [TASK-2-COMPLETION-SUMMARY.md](TASK-2-COMPLETION-SUMMARY.md) para detalles completos.

### Probar Creación Automática de Foro
1. Publicar un evento desde el microservicio Eventos
2. Verificar logs del consumer
3. Consultar MongoDB para ver el foro creado

### Probar Comentarios
```bash
# Crear comentario
curl -X POST http://localhost:5007/api/comunidad/comentarios \
  -H "Content-Type: application/json" \
  -d '{
    "foroId": "guid-del-foro",
    "usuarioId": "guid-del-usuario",
    "contenido": "Mi primer comentario"
  }'

# Obtener comentarios
curl http://localhost:5007/api/comunidad/foros/{eventoId}
```

## 🔐 Seguridad

- CORS configurado para permitir todos los orígenes (ajustar en producción)
- Validación de entrada en controllers
- Manejo de excepciones centralizado

## 📝 Notas Técnicas

1. **Truco del Namespace:** El contrato `EventoPublicadoEventoDominio` usa el namespace original del emisor para compatibilidad con MassTransit.

2. **Estructura de 2 Niveles:** La decisión de limitar a 2 niveles simplifica la UI y mejora el rendimiento.

3. **Soft Delete:** Los comentarios ocultos mantienen `EsVisible = false` pero no se eliminan físicamente.

4. **MongoDB Embebido:** Las respuestas están embebidas en el documento del comentario para reducir consultas.

## 🚧 Próximas Mejoras

- [ ] Paginación de comentarios
- [ ] Búsqueda de comentarios
- [ ] Notificaciones de nuevas respuestas
- [ ] Rate limiting para prevenir spam
- [ ] Autenticación y autorización con Keycloak
- [ ] Métricas con Prometheus

## 📄 Licencia

Este proyecto es parte del sistema de gestión de eventos Kairo.
