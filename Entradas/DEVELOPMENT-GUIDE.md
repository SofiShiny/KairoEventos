# Guía de Desarrollo - Microservicio Entradas.API

## Introducción

Esta guía proporciona información detallada para desarrolladores que trabajen en el microservicio Entradas.API. Cubre patrones de diseño, convenciones de código, y mejores prácticas específicas del proyecto.

## Arquitectura Hexagonal

### Principios Fundamentales

1. **Dominio Puro**: La capa de dominio no debe tener dependencias externas
2. **Inversión de Dependencias**: Las capas externas dependen de las internas
3. **Puertos y Adaptadores**: Interfaces definen contratos, implementaciones son intercambiables
4. **Separación de Responsabilidades**: Cada capa tiene un propósito específico

### Estructura de Capas

```
Entradas.Dominio (Centro)
├── Entidades/
│   ├── EntidadBase.cs
│   └── Entrada.cs
├── Enums/
│   └── EstadoEntrada.cs
├── Eventos/
│   ├── EntradaCreadaEvento.cs
│   └── PagoConfirmadoEvento.cs
├── Excepciones/
│   ├── DominioException.cs
│   ├── EntradaNoEncontradaException.cs
│   └── ...
└── Interfaces/
    ├── IRepositorioEntradas.cs
    ├── IVerificadorEventos.cs
    └── ...

Entradas.Aplicacion (Casos de Uso)
├── Comandos/
├── Queries/
├── Handlers/
├── DTOs/
├── Validadores/
└── Consumers/

Entradas.Infraestructura (Adaptadores)
├── Persistencia/
├── Repositorios/
├── ServiciosExternos/
├── Servicios/
└── Configuracion/

Entradas.API (Interfaz)
├── Controllers/
├── DTOs/
├── Middleware/
├── Configuration/
└── Validators/
```

## Patrones de Diseño

### CQRS (Command Query Responsibility Segregation)

**Commands** - Modifican estado:
```csharp
public record CrearEntradaCommand(
    Guid EventoId,
    Guid UsuarioId,
    Guid? AsientoId,
    decimal Monto
) : IRequest<EntradaCreadaDto>;
```

**Queries** - Solo lectura:
```csharp
public record ObtenerEntradaQuery(Guid Id) : IRequest<EntradaDto>;
```

**Handlers** - Procesan commands/queries:
```csharp
public class CrearEntradaCommandHandler : IRequestHandler<CrearEntradaCommand, EntradaCreadaDto>
{
    // Implementación
}
```

### Repository Pattern

**Interface en Dominio:**
```csharp
public interface IRepositorioEntradas
{
    Task<Entrada> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Entrada> GuardarAsync(Entrada entrada, CancellationToken cancellationToken);
}
```

**Implementación en Infraestructura:**
```csharp
public class RepositorioEntradas : IRepositorioEntradas
{
    private readonly EntradasDbContext _context;
    // Implementación con EF Core
}
```

### Domain Events

**Definición:**
```csharp
public record EntradaCreadaEvento(
    Guid EntradaId,
    Guid EventoId,
    Guid UsuarioId,
    decimal Monto,
    DateTime FechaCreacion
);
```

**Publicación:**
```csharp
await _publisher.Publish(new EntradaCreadaEvento(
    entrada.Id,
    entrada.EventoId,
    entrada.UsuarioId,
    entrada.Monto,
    entrada.FechaCompra
), cancellationToken);
```

## Convenciones de Código

### Nomenclatura

- **Clases**: PascalCase en español (`CrearEntradaCommand`)
- **Métodos**: PascalCase en español (`ObtenerPorIdAsync`)
- **Propiedades**: PascalCase en español (`EventoId`)
- **Variables locales**: camelCase en español (`entradaCreada`)
- **Constantes**: UPPER_CASE en español (`ESTADO_INICIAL`)

### Estructura de Archivos

```
Entidades/
├── Entrada.cs              # Una entidad por archivo
├── EntidadBase.cs          # Clase base común

Excepciones/
├── DominioException.cs     # Excepción base
├── EntradaNoEncontradaException.cs  # Excepciones específicas

Interfaces/
├── IRepositorioEntradas.cs # Una interface por archivo
```

### Documentación XML

```csharp
/// <summary>
/// Crea una nueva entrada para un evento específico.
/// </summary>
/// <param name="eventoId">Identificador único del evento</param>
/// <param name="usuarioId">Identificador único del usuario</param>
/// <param name="asientoId">Identificador del asiento (opcional para entradas generales)</param>
/// <param name="monto">Precio de la entrada</param>
/// <returns>Nueva instancia de Entrada en estado PendientePago</returns>
/// <exception cref="ArgumentException">Cuando los parámetros son inválidos</exception>
public static Entrada Crear(Guid eventoId, Guid usuarioId, Guid? asientoId, decimal monto)
```

## Testing

### Estructura de Tests

```
Entradas.Pruebas/
├── Dominio/
│   ├── Entidades/
│   │   └── EntradaTests.cs
│   └── Propiedades/
│       └── EntradaPropiedadesTests.cs
├── Aplicacion/
│   ├── Handlers/
│   │   └── CrearEntradaCommandHandlerTests.cs
│   └── Propiedades/
│       └── ComandosPropiedadesTests.cs
├── Infraestructura/
│   ├── Repositorios/
│   │   └── RepositorioEntradasTests.cs
│   └── ServiciosExternos/
│       └── VerificadorEventosHttpTests.cs
└── Integracion/
    ├── API/
    │   └── EntradasControllerIntegrationTests.cs
    └── BaseDatos/
        └── PostgreSqlIntegrationTests.cs
```

### Unit Tests

```csharp
[Fact]
public void Crear_ConParametrosValidos_DebeCrearEntradaEnEstadoPendientePago()
{
    // Arrange
    var eventoId = Guid.NewGuid();
    var usuarioId = Guid.NewGuid();
    var monto = 100m;
    var codigoQr = "TICKET-ABC123-4567";

    // Act
    var entrada = Entrada.Crear(eventoId, usuarioId, null, monto, codigoQr);

    // Assert
    entrada.Should().NotBeNull();
    entrada.EventoId.Should().Be(eventoId);
    entrada.UsuarioId.Should().Be(usuarioId);
    entrada.Estado.Should().Be(EstadoEntrada.PendientePago);
    entrada.Monto.Should().Be(monto);
    entrada.CodigoQr.Should().Be(codigoQr);
}
```

### Property-Based Tests

```csharp
[Property]
[Trait("Feature", "microservicio-entradas")]
[Trait("Property", "1: Estructura de Entidad Entrada")]
public void Entrada_DebeContenerTodasLasPropiedadesRequeridas(
    Guid eventoId, 
    Guid usuarioId, 
    PositiveInt montoInt)
{
    // Arrange
    var monto = (decimal)montoInt.Get;
    var codigoQr = "TICKET-TEST-1234";

    // Act
    var entrada = Entrada.Crear(eventoId, usuarioId, null, monto, codigoQr);

    // Assert
    entrada.Id.Should().NotBeEmpty();
    entrada.EventoId.Should().Be(eventoId);
    entrada.UsuarioId.Should().Be(usuarioId);
    entrada.Monto.Should().Be(monto);
    entrada.CodigoQr.Should().Be(codigoQr);
    entrada.Estado.Should().Be(EstadoEntrada.PendientePago);
    entrada.FechaCompra.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
}
```

### Integration Tests

```csharp
public class EntradasControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public EntradasControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CrearEntrada_ConDatosValidos_DebeRetornar201()
    {
        // Arrange
        var request = new CrearEntradaRequestDto
        {
            EventoId = Guid.NewGuid(),
            UsuarioId = Guid.NewGuid(),
            Monto = 100m
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/entradas", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
```

## Configuración

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=entradas_db;Username=entradas_user;Password=entradas_password"
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Username": "guest",
    "Password": "guest",
    "VirtualHost": "/",
    "Port": 5672
  },
  "ServiciosExternos": {
    "Eventos": {
      "BaseUrl": "http://localhost:5001",
      "TimeoutSeconds": 30
    },
    "Asientos": {
      "BaseUrl": "http://localhost:5002",
      "TimeoutSeconds": 30
    }
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    }
  }
}
```

### Variables de Entorno

```bash
# Base de datos
ConnectionStrings__DefaultConnection="Host=postgres;Database=entradas_db;Username=entradas_user;Password=entradas_password"

# RabbitMQ
RabbitMQ__Host="rabbitmq"
RabbitMQ__Username="guest"
RabbitMQ__Password="guest"

# Servicios externos
ServiciosExternos__Eventos__BaseUrl="http://eventos-api:5000"
ServiciosExternos__Asientos__BaseUrl="http://asientos-api:5000"

# Logging
Serilog__MinimumLevel__Default="Information"
```

## Debugging

### Logs Útiles

```csharp
// En handlers
_logger.LogInformation("Creando entrada para evento {EventoId} y usuario {UsuarioId}", 
    command.EventoId, command.UsuarioId);

// En servicios externos
_logger.LogWarning("Servicio de eventos no disponible. Reintentando en {Delay}ms", delay);

// En excepciones
_logger.LogError(ex, "Error al procesar comando {CommandType} para usuario {UsuarioId}", 
    typeof(TCommand).Name, command.UsuarioId);
```

### Health Checks

```bash
# Verificar estado general
curl http://localhost:5000/health

# Verificar componentes específicos
curl http://localhost:5000/health/ready
curl http://localhost:5000/health/live
```

### Métricas

```bash
# Endpoint de métricas (si está habilitado)
curl http://localhost:5000/metrics
```

## Mejores Prácticas

### 1. Manejo de Errores

- Usar excepciones específicas del dominio
- Implementar middleware global para manejo de excepciones
- Loggear errores con contexto suficiente
- Retornar códigos HTTP apropiados

### 2. Validación

- Validar en múltiples capas (API, Aplicación, Dominio)
- Usar FluentValidation para reglas complejas
- Validar invariantes en el dominio
- Proporcionar mensajes de error descriptivos

### 3. Performance

- Usar async/await consistentemente
- Implementar paginación en queries
- Usar índices apropiados en base de datos
- Configurar timeouts para servicios externos

### 4. Seguridad

- Validar todos los inputs
- Usar parámetros en queries SQL
- Implementar rate limiting
- Loggear intentos de acceso sospechosos

### 5. Mantenibilidad

- Seguir principios SOLID
- Escribir tests antes de refactorizar
- Documentar decisiones arquitectónicas
- Mantener dependencias actualizadas

## Troubleshooting

### Problemas Comunes

1. **Error de conexión a PostgreSQL**
   - Verificar cadena de conexión
   - Confirmar que PostgreSQL está ejecutándose
   - Revisar permisos de usuario

2. **Error de conexión a RabbitMQ**
   - Verificar configuración de host y puerto
   - Confirmar credenciales
   - Revisar estado del servicio RabbitMQ

3. **Tests fallando**
   - Verificar que TestContainers puede acceder a Docker
   - Confirmar que puertos no están en uso
   - Revisar logs de contenedores de test

4. **Baja cobertura de tests**
   - Ejecutar análisis de cobertura
   - Identificar código no cubierto
   - Agregar tests para casos faltantes

### Comandos de Diagnóstico

```bash
# Verificar estado de servicios
docker-compose ps

# Ver logs de aplicación
docker-compose logs -f entradas-api

# Verificar conectividad a base de datos
dotnet ef database update --dry-run --project Entradas.Infraestructura --startup-project Entradas.API

# Ejecutar health checks
curl -f http://localhost:5000/health || echo "Health check failed"
```