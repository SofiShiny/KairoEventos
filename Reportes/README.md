# Microservicio de Reportes

Microservicio de reportes y analíticas para el sistema de gestión de eventos. Implementa un patrón CQRS como modelo de lectura optimizado, consumiendo eventos de dominio mediante MassTransit/RabbitMQ y persistiendo datos en MongoDB.

## 📋 Tabla de Contenidos

- [Arquitectura](#arquitectura)
- [Estructura del Proyecto](#estructura-del-proyecto)
- [Requisitos Previos](#requisitos-previos)
- [Instalación y Ejecución](#instalación-y-ejecución)
- [Endpoints Disponibles](#endpoints-disponibles)
- [Desarrollo](#desarrollo)
- [Testing](#testing)
- [Configuración](#configuración)
- [Eventos Consumidos](#eventos-consumidos)
- [Monitoreo y Logs](#monitoreo-y-logs)
- [Troubleshooting](#troubleshooting)

## Arquitectura

### Patrón Arquitectónico

- **Patrón Principal:** Arquitectura Hexagonal (Puertos y Adaptadores) con DDD
- **Patrón CQRS:** Modelo de lectura optimizado (Read Model)
- **Base de Datos:** MongoDB (NoSQL para consultas analíticas)
- **Mensajería:** RabbitMQ con MassTransit
- **Jobs Programados:** Hangfire para consolidación nocturna
- **Testing:** Property-Based Testing con FsCheck + Unit Tests + Integration Tests

### Diagrama de Arquitectura

```
┌─────────────────────────────────────────────────────────────┐
│                      CAPA API (Puerto)                       │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  ReportesController                                   │   │
│  │  - GET /api/reportes/resumen-ventas                  │   │
│  │  - GET /api/reportes/asistencia/{eventoId}          │   │
│  │  - GET /api/reportes/auditoria                       │   │
│  │  - GET /api/reportes/conciliacion-financiera        │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                   CAPA APLICACIÓN                            │
│  ┌──────────────────┐  ┌──────────────────┐                │
│  │   Consumers      │  │   Jobs           │                │
│  │  (MassTransit)   │  │  (Hangfire)      │                │
│  └──────────────────┘  └──────────────────┘                │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                     CAPA DOMINIO                             │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  Modelos de Lectura + Contratos Espejo               │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                 CAPA INFRAESTRUCTURA                         │
│  ┌──────────────────┐  ┌──────────────────┐                │
│  │  MongoDB         │  │  RabbitMQ        │                │
│  │  (Repositorios)  │  │  (MassTransit)   │                │
│  └──────────────────┘  └──────────────────┘                │
└─────────────────────────────────────────────────────────────┘
```

### Flujo de Datos

1. **Eventos Entrantes:** Microservicios de Eventos y Asientos publican eventos en RabbitMQ
2. **Consumidores:** MassTransit consume eventos y actualiza modelos de lectura en MongoDB
3. **Consolidación:** Hangfire ejecuta jobs nocturnos para agregar métricas diarias
4. **Consultas:** API REST lee datos optimizados de MongoDB
5. **Auditoría:** Todas las operaciones se registran en `LogAuditoria`

## Estructura del Proyecto

```
Reportes/
├── backend/
│   └── src/
│       └── Services/
│           └── Reportes/
│               ├── Reportes.API/              # Capa de presentación (REST API)
│               ├── Reportes.Aplicacion/       # Lógica de aplicación (Consumers, Jobs)
│               ├── Reportes.Dominio/          # Modelos de dominio y contratos
│               ├── Reportes.Infraestructura/  # Persistencia y servicios externos
│               └── Reportes.Pruebas/          # Tests (Property + Unit + Integration)
├── docker-compose.yml
├── Dockerfile
└── README.md
```

## Requisitos Previos

### Software Requerido

- **.NET 8 SDK** - [Descargar](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Docker** - [Descargar](https://www.docker.com/products/docker-desktop)
- **Docker Compose** - Incluido con Docker Desktop

### Verificar Instalación

```bash
# Verificar .NET
dotnet --version
# Debe mostrar: 8.0.x o superior

# Verificar Docker
docker --version
docker-compose --version
```

## Instalación y Ejecución

### Opción 1: Ejecución Completa con Docker (Recomendado)

Esta opción levanta todos los servicios (MongoDB, RabbitMQ y la API) en contenedores:

```bash
# 1. Navegar al directorio del proyecto
cd Reportes

# 2. Construir y levantar todos los servicios
docker-compose up --build

# 3. Verificar que los servicios estén corriendo
docker-compose ps
```

Los servicios estarán disponibles en:
- **API de Reportes:** http://localhost:5002
- **Swagger UI:** http://localhost:5002/swagger
- **Health Check:** http://localhost:5002/health
- **Hangfire Dashboard:** http://localhost:5002/hangfire
- **RabbitMQ Management:** http://localhost:15672 (usuario: guest, contraseña: guest)

### Opción 2: Desarrollo Local (API en local, infraestructura en Docker)

Esta opción es útil para desarrollo activo:

```bash
# 1. Levantar solo la infraestructura (MongoDB y RabbitMQ)
docker-compose up mongodb rabbitmq -d

# 2. Restaurar dependencias
dotnet restore

# 3. Ejecutar la API localmente
cd backend/src/Services/Reportes/Reportes.API
dotnet run

# La API estará disponible en http://localhost:5002
```

### Opción 3: Ejecución Manual (Sin Docker)

Si prefieres instalar MongoDB y RabbitMQ localmente:

```bash
# 1. Instalar MongoDB localmente
# Windows: https://www.mongodb.com/try/download/community
# macOS: brew install mongodb-community
# Linux: sudo apt-get install mongodb

# 2. Instalar RabbitMQ localmente
# Windows: https://www.rabbitmq.com/install-windows.html
# macOS: brew install rabbitmq
# Linux: sudo apt-get install rabbitmq-server

# 3. Iniciar servicios
# MongoDB: mongod
# RabbitMQ: rabbitmq-server

# 4. Configurar variables de entorno (opcional)
export MONGODB_CONNECTION_STRING="mongodb://localhost:27017"
export MONGODB_DATABASE="reportes_db"
export RABBITMQ_HOST="localhost"

# 5. Ejecutar la API
cd backend/src/Services/Reportes/Reportes.API
dotnet run
```

### Detener Servicios

```bash
# Detener servicios Docker
docker-compose down

# Detener y eliminar volúmenes (limpia datos)
docker-compose down -v
```

## Endpoints Disponibles

### Reportes

- `GET /api/reportes/resumen-ventas` - Métricas generales de ventas
- `GET /api/reportes/asistencia/{eventoId}` - Aforo en tiempo real
- `GET /api/reportes/auditoria` - Logs del sistema
- `GET /api/reportes/conciliacion-financiera` - Datos para contabilidad

### Health Checks

- `GET /health` - Estado general del servicio
- `GET /health/mongodb` - Estado de MongoDB
- `GET /health/rabbitmq` - Estado de RabbitMQ

## Desarrollo

### Estructura de Capas

El proyecto sigue una arquitectura hexagonal con las siguientes capas:

#### 1. Reportes.API (Capa de Presentación)
- Controladores REST
- DTOs de respuesta
- Middleware de manejo de errores
- Configuración de Swagger
- Health checks

#### 2. Reportes.Aplicacion (Capa de Aplicación)
- Consumidores de eventos (MassTransit)
- Jobs programados (Hangfire)
- Lógica de orquestación

#### 3. Reportes.Dominio (Capa de Dominio)
- Modelos de lectura (Read Models)
- Contratos espejo de eventos externos
- Interfaces de repositorios

#### 4. Reportes.Infraestructura (Capa de Infraestructura)
- Implementación de repositorios (MongoDB)
- Configuración de persistencia
- Servicios externos

#### 5. Reportes.Pruebas (Capa de Testing)
- Property-based tests
- Unit tests
- Integration tests

### Flujo de Trabajo de Desarrollo

1. **Crear una rama de feature**
   ```bash
   git checkout -b feature/nueva-funcionalidad
   ```

2. **Hacer cambios siguiendo la arquitectura**
   - Dominio primero (modelos, interfaces)
   - Infraestructura (implementaciones)
   - Aplicación (lógica de negocio)
   - API (endpoints)

3. **Escribir tests**
   - Property tests para propiedades universales
   - Unit tests para casos específicos
   - Integration tests para flujos completos

4. **Ejecutar tests localmente**
   ```bash
   dotnet test
   ```

5. **Verificar cobertura**
   ```bash
   dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
   ```

6. **Commit y push**
   ```bash
   git add .
   git commit -m "feat: descripción del cambio"
   git push origin feature/nueva-funcionalidad
   ```

### Ejecutar Tests

```bash
# Todos los tests
dotnet test

# Solo property tests
dotnet test --filter "Category=Property"

# Solo unit tests
dotnet test --filter "Category=Unit"

# Solo integration tests
dotnet test --filter "Category=Integration"

# Con cobertura de código
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura

# Generar reporte HTML de cobertura
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
reportgenerator -reports:coverage.opencover.xml -targetdir:coverage-report
```

### Ejecutar Localmente (sin Docker)

```bash
cd backend/src/Services/Reportes/Reportes.API
dotnet run
```

Asegúrate de tener MongoDB y RabbitMQ corriendo localmente o actualiza las variables de entorno en `appsettings.Development.json`.

### Hot Reload durante Desarrollo

```bash
cd backend/src/Services/Reportes/Reportes.API
dotnet watch run
```

Esto reiniciará automáticamente la aplicación cuando detecte cambios en el código.

### Debugging

#### Visual Studio Code
1. Abrir el proyecto en VS Code
2. Presionar F5 o ir a Run > Start Debugging
3. Seleccionar ".NET Core Launch (web)"

#### Visual Studio
1. Abrir `Reportes.sln`
2. Establecer `Reportes.API` como proyecto de inicio
3. Presionar F5

### Agregar Nuevos Endpoints

1. **Definir DTO en `Reportes.API/DTOs/`**
   ```csharp
   public class NuevoReporteDto
   {
       public string Campo { get; set; }
   }
   ```

2. **Agregar método al repositorio en `Reportes.Dominio/Repositorios/`**
   ```csharp
   Task<NuevoReporte> ObtenerNuevoReporteAsync();
   ```

3. **Implementar en `Reportes.Infraestructura/Repositorios/`**
   ```csharp
   public async Task<NuevoReporte> ObtenerNuevoReporteAsync()
   {
       // Implementación
   }
   ```

4. **Agregar endpoint en `Reportes.API/Controladores/ReportesController.cs`**
   ```csharp
   [HttpGet("nuevo-reporte")]
   public async Task<ActionResult<NuevoReporteDto>> ObtenerNuevoReporte()
   {
       // Implementación
   }
   ```

5. **Escribir tests en `Reportes.Pruebas/`**

### Agregar Nuevos Consumidores de Eventos

1. **Definir contrato espejo en `Reportes.Dominio/ContratosExternos/`**
   ```csharp
   namespace MicroservicioOrigen.Dominio.EventosDominio;
   
   public record NuevoEventoDominio
   {
       public Guid Id { get; init; }
   }
   ```

2. **Crear consumidor en `Reportes.Aplicacion/Consumers/`**
   ```csharp
   public class NuevoEventoConsumer : IConsumer<NuevoEventoDominio>
   {
       public async Task Consume(ConsumeContext<NuevoEventoDominio> context)
       {
           // Implementación
       }
   }
   ```

3. **Registrar en `Reportes.Aplicacion/InyeccionDependencias.cs`**
   ```csharp
   cfg.AddConsumer<NuevoEventoConsumer>();
   ```

4. **Escribir property tests**

## Configuración

### Variables de Entorno

| Variable | Descripción | Default |
|----------|-------------|---------|
| `MONGODB_CONNECTION_STRING` | Cadena de conexión a MongoDB | `mongodb://localhost:27017` |
| `MONGODB_DATABASE` | Nombre de la base de datos | `reportes_db` |
| `RABBITMQ_HOST` | Host de RabbitMQ | `localhost` |
| `RABBITMQ_PORT` | Puerto de RabbitMQ | `5672` |
| `RABBITMQ_USER` | Usuario de RabbitMQ | `guest` |
| `RABBITMQ_PASSWORD` | Contraseña de RabbitMQ | `guest` |
| `HANGFIRE_CRON_CONSOLIDACION` | Expresión cron para job de consolidación | `0 2 * * *` (2 AM diario) |

## Eventos Consumidos

El microservicio consume los siguientes eventos de dominio:

### Del Microservicio de Eventos
- `EventoPublicadoEventoDominio` - Crea métricas iniciales del evento
- `AsistenteRegistradoEventoDominio` - Incrementa contador de asistencia
- `EventoCanceladoEventoDominio` - Actualiza estado del evento

### Del Microservicio de Asientos
- `AsientoReservadoEventoDominio` - Registra venta y actualiza aforo
- `AsientoLiberadoEventoDominio` - Actualiza disponibilidad
- `MapaAsientosCreadoEventoDominio` - Inicializa capacidad del evento

## Colecciones MongoDB

- `reportes_ventas_diarias` - Ventas agregadas por día
- `historial_asistencia` - Aforo y asistentes por evento
- `metricas_evento` - Métricas generales de eventos
- `logs_auditoria` - Trazabilidad de operaciones
- `reportes_consolidados` - Métricas consolidadas (generadas por Hangfire)

## Jobs Programados

### Job de Consolidación Nocturna
- **Frecuencia:** Diaria a las 2 AM
- **Función:** Agrega métricas del día anterior
- **Colección destino:** `reportes_consolidados`

## Testing

El proyecto incluye una estrategia de testing exhaustiva:

- **21 Property-Based Tests** (100+ iteraciones cada uno)
- **~30 Unit Tests** para casos específicos
- **~10 Integration Tests** end-to-end
- **Objetivo de Cobertura:** >80%

### Ejemplo de Property Test

```csharp
[Property(MaxTest = 100)]
public Property Propiedad_InvarianteDisponibilidadAsientos()
{
    // Feature: microservicio-reportes, Property 3
    return Prop.ForAll<HistorialAsistencia>(historial =>
    {
        var suma = historial.AsientosDisponibles + historial.AsientosReservados;
        return (suma == historial.CapacidadTotal).ToProperty();
    });
}
```

## Monitoreo y Logs

### Logging Estructurado con Serilog

El servicio utiliza Serilog para logging estructurado con múltiples sinks:

- **Consola:** Logs en formato legible para desarrollo
- **MongoDB:** Logs persistidos en colección `logs` para análisis
- **Contexto de Correlación:** Cada request tiene un ID único para trazabilidad

### Niveles de Log

- **Verbose:** Información muy detallada (solo en desarrollo)
- **Debug:** Información de debugging
- **Information:** Eventos normales del sistema
- **Warning:** Situaciones anómalas pero manejables
- **Error:** Errores que requieren atención
- **Fatal:** Errores críticos que detienen el servicio

### Consultar Logs

#### Desde MongoDB
```javascript
// Conectar a MongoDB
mongo mongodb://localhost:27017/reportes_db

// Ver últimos 10 logs
db.logs.find().sort({Timestamp: -1}).limit(10)

// Filtrar por nivel
db.logs.find({Level: "Error"})

// Filtrar por correlation ID
db.logs.find({"Properties.CorrelationId": "abc-123"})
```

#### Desde la API
```bash
# Obtener logs de auditoría
curl http://localhost:5002/api/reportes/auditoria
```

### Health Checks

El servicio expone health checks detallados:

```bash
# Health check general
curl http://localhost:5002/health

# Respuesta ejemplo:
{
  "status": "Healthy",
  "timestamp": "2024-01-15T10:30:00Z",
  "checks": [
    {
      "name": "mongodb",
      "status": "Healthy",
      "duration": 45.2
    },
    {
      "name": "rabbitmq",
      "status": "Healthy",
      "duration": 23.1
    }
  ],
  "totalDuration": 68.3
}
```

### Métricas de Hangfire

Acceder al dashboard de Hangfire para monitorear jobs:

```
http://localhost:5002/hangfire
```

Información disponible:
- Jobs ejecutados exitosamente
- Jobs fallidos
- Jobs programados
- Tiempo de ejecución
- Historial de ejecuciones

## Troubleshooting

### Problema: MongoDB no se conecta

**Síntomas:**
```
Error verificando conexión a MongoDB: A timeout occurred after 30000ms
```

**Soluciones:**
1. Verificar que MongoDB esté corriendo:
   ```bash
   docker-compose ps mongodb
   ```

2. Verificar la cadena de conexión:
   ```bash
   echo $MONGODB_CONNECTION_STRING
   ```

3. Reiniciar MongoDB:
   ```bash
   docker-compose restart mongodb
   ```

### Problema: RabbitMQ no consume eventos

**Síntomas:**
- Los eventos se publican pero no se procesan
- No hay logs de consumidores

**Soluciones:**
1. Verificar que RabbitMQ esté corriendo:
   ```bash
   docker-compose ps rabbitmq
   ```

2. Verificar colas en RabbitMQ Management:
   - Ir a http://localhost:15672
   - Login: guest/guest
   - Verificar que existan las colas

3. Verificar logs del consumidor:
   ```bash
   docker-compose logs reportes-api | grep Consumer
   ```

4. Reiniciar el servicio:
   ```bash
   docker-compose restart reportes-api
   ```

### Problema: Tests fallan localmente

**Síntomas:**
```
Test failed: Connection refused
```

**Soluciones:**
1. Asegurarse de que MongoDB esté corriendo para integration tests:
   ```bash
   docker-compose up mongodb -d
   ```

2. Limpiar y reconstruir:
   ```bash
   dotnet clean
   dotnet build
   dotnet test
   ```

3. Verificar que no haya procesos usando los puertos:
   ```bash
   # Windows
   netstat -ano | findstr :27017
   
   # Linux/Mac
   lsof -i :27017
   ```

### Problema: Job de consolidación no se ejecuta

**Síntomas:**
- No se generan reportes consolidados
- No hay logs del job

**Soluciones:**
1. Verificar configuración de Hangfire:
   ```bash
   curl http://localhost:5002/hangfire
   ```

2. Verificar que el job esté programado:
   - Ir al dashboard de Hangfire
   - Buscar "generar-reportes-consolidados" en Recurring Jobs

3. Ejecutar manualmente desde el dashboard:
   - Click en "Trigger now"

4. Verificar logs:
   ```bash
   docker-compose logs reportes-api | grep JobGenerarReportesConsolidados
   ```

### Problema: Swagger no muestra documentación

**Síntomas:**
- Swagger UI carga pero no muestra descripciones

**Soluciones:**
1. Verificar que el archivo XML se genere:
   ```bash
   ls backend/src/Services/Reportes/Reportes.API/bin/Debug/net8.0/Reportes.API.xml
   ```

2. Reconstruir el proyecto:
   ```bash
   dotnet build
   ```

3. Verificar configuración en Program.cs:
   ```csharp
   options.IncludeXmlComments(xmlPath);
   ```

### Problema: Cobertura de tests baja

**Soluciones:**
1. Generar reporte de cobertura:
   ```bash
   dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
   ```

2. Ver reporte HTML:
   ```bash
   reportgenerator -reports:coverage.cobertura.xml -targetdir:coverage-report
   ```

3. Identificar áreas sin cobertura y agregar tests

### Logs Útiles

```bash
# Ver logs en tiempo real
docker-compose logs -f reportes-api

# Ver logs de MongoDB
docker-compose logs mongodb

# Ver logs de RabbitMQ
docker-compose logs rabbitmq

# Ver logs de un consumidor específico
docker-compose logs reportes-api | grep EventoPublicadoConsumer
```

## Contribución

### Proceso de Contribución

1. **Fork del repositorio**
2. **Crear rama de feature:** `git checkout -b feature/nueva-funcionalidad`
3. **Hacer cambios siguiendo las convenciones**
4. **Escribir tests (cobertura >80%)**
5. **Ejecutar tests:** `dotnet test`
6. **Commit:** `git commit -m "feat: descripción"`
7. **Push:** `git push origin feature/nueva-funcionalidad`
8. **Crear Pull Request**

### Convenciones de Código

- Seguir las guías de estilo de C# (.NET)
- Usar nombres descriptivos en español para el dominio
- Documentar métodos públicos con XML comments
- Mantener métodos pequeños y enfocados
- Escribir tests para toda nueva funcionalidad

### Convenciones de Commits

Seguir [Conventional Commits](https://www.conventionalcommits.org/):

- `feat:` Nueva funcionalidad
- `fix:` Corrección de bug
- `docs:` Cambios en documentación
- `test:` Agregar o modificar tests
- `refactor:` Refactorización de código
- `perf:` Mejoras de rendimiento
- `chore:` Tareas de mantenimiento

### Revisión de Código

Todos los PRs deben:
- Pasar todos los tests
- Mantener cobertura >80%
- Ser revisados por al menos un desarrollador
- Seguir las convenciones del proyecto

## Recursos Adicionales

- **Especificación Completa:** `.kiro/specs/microservicio-reportes/`
- **Documento de Requisitos:** `.kiro/specs/microservicio-reportes/requirements.md`
- **Documento de Diseño:** `.kiro/specs/microservicio-reportes/design.md`
- **Plan de Tareas:** `.kiro/specs/microservicio-reportes/tasks.md`
- **Documentación de Tests de Integración:** `INTEGRATION-TESTS-README.md`

## Licencia

[Especificar licencia]

## Contacto

Para preguntas o soporte:
- Email: dev@eventos.com
- Issues: [GitHub Issues](https://github.com/tu-org/reportes/issues)
