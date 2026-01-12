# Mejora de Cobertura de Tests - Microservicio Reportes

# Mejora de Cobertura de Tests - Microservicio Reportes

## Estado Actual (Actualizado: 1/1/2026)

### ✅ TASK 11 COMPLETADO - Consumer Tests Agregados y Cobertura Mejorada
- **Cobertura de líneas**: **87.6%** (1,454/1,659) ⚠️ Objetivo: >90% (falta 2.4%)
- **Cobertura de ramas**: **86.5%** (154/178) ✅ **OBJETIVO ALCANZADO** (>80%)
- **Tests totales**: **243** ✅ **TODOS PASANDO** (0 fallando)
- **CRAP score**: ✅ **TODOS los componentes críticos <15** (Risk Hotspots eliminados)
- **Duración**: ~17.4 segundos
- **Refactoring completado**: Program.cs y InyeccionDependencias.cs refactorizados con arquitectura modular

### 📊 Progreso Alcanzado vs Estado Inicial
| Métrica | Inicial | Task 11 Final | Mejora |
|---------|---------|---------------|--------|
| Cobertura líneas | 44.71% | 87.6% | +42.89% |
| Cobertura ramas | 38.55% | 86.5% | +47.95% |
| Tests pasando | 68/83 | 243/243 | +175 tests |
| Tests fallando | 15 | 0 | -15 |

### 🎯 Objetivos Restantes
- **Líneas faltantes**: ~40 líneas adicionales (2.4% brecha)
- **Meta alcanzada**: Cobertura de ramas >80% ✅
- **Meta pendiente**: Cobertura de líneas >90% (muy cerca)

### Risk Hotspots Identificados (CRAP Score)

1. **InyeccionDependencias.AgregarAplicacion** - CRAP 600, Complejidad 24
2. **Program.<Main>$()** - CRAP 506, Complejidad 22
3. **GlobalExceptionHandlerMiddleware.HandleExceptionAsync** - CRAP 72, Complejidad 8
4. **RepositorioReportesLecturaMongo.ContarLogsAuditoriaAsync** - CRAP 72, Complejidad 8
5. **MapaAsientosCreadoConsumer.Consume** - CRAP 42, Complejidad 6

### Cobertura por Ensamblado

- **Reportes.API**: 36.3% líneas, 46.3% ramas
- **Reportes.Aplicacion**: 45.7% líneas, 28.7% ramas
- **Reportes.Infraestructura**: 49% líneas, 37.5% ramas

### Áreas Sin Cobertura (0%)

#### Middleware
- `GlobalExceptionHandlerMiddleware` (15 líneas)
- `CorrelationIdMiddleware` (10 líneas)
- `HangfireAuthorizationFilter` (8 líneas)

#### Consumers
- `AsientoAgregadoConsumer` (25 líneas)
- `EventoCanceladoConsumer` (30 líneas)
- `MapaAsientosCreadoConsumer` (35 líneas)

#### Configuración
- `Program.cs` (15 líneas)
- `InyeccionDependencias.cs` (83 líneas)

#### Health Checks
- `MongoDbHealthCheck` (20 líneas)
- `RabbitMqHealthCheck` (20 líneas)

### Tests Fallando

#### RepositorioReportesLecturaTests (10 tests)
**Problema**: Intentan mockear `ReportesMongoDbContext` (clase concreta) con Moq
**Solución**: Convertir a tests de integración con Mongo2Go

#### MongoDbIntegrationTests (5 tests)
**Problema**: Insertan datos pero no los encuentran al consultar
**Solución**: Investigar configuración de colecciones y índices

## Tareas de Mejora

### TASK 1: Agregar Mongo2Go para Tests de Integración

**Objetivo**: Reemplazar mocks de MongoDB con instancia en memoria

**Pasos**:
1. Agregar paquete `Mongo2Go` al proyecto de pruebas
2. Crear clase base `MongoIntegrationTestBase` con setup/teardown
3. Refactorizar `RepositorioReportesLecturaTests` para usar Mongo2Go
4. Corregir `MongoDbIntegrationTests` para usar Mongo2Go correctamente

**Criterios de Aceptación**:
- ✅ Todos los tests de repositorio usan MongoDB real en memoria
- ✅ Los 15 tests fallando ahora pasan
- ✅ Tests se ejecutan en menos de 30 segundos

**Archivos a Modificar**:
- `Reportes.Pruebas.csproj` (agregar Mongo2Go)
- `Infraestructura/Repositorios/RepositorioReportesLecturaTests.cs`
- `Infraestructura/MongoDbIntegrationTests.cs`

**Archivos a Crear**:
- `Infraestructura/MongoIntegrationTestBase.cs`

---

### TASK 2: Tests para GlobalExceptionHandlerMiddleware

**Objetivo**: Cubrir manejo de excepciones centralizado (CRAP 72 → <30)

**Escenarios a Probar**:
1. `ArgumentException` → 400 Bad Request
2. `KeyNotFoundException` → 404 Not Found
3. `MongoException` → 503 Service Unavailable
4. `TimeoutException` → 408 Request Timeout
5. `Exception` genérica → 500 Internal Server Error
6. Verificar formato de respuesta JSON
7. Verificar logging de errores

**Criterios de Aceptación**:
- ✅ 7 tests nuevos pasando
- ✅ Cobertura del middleware >90%
- ✅ CRAP score <30

**Archivos a Crear**:
- `API/Middleware/GlobalExceptionHandlerMiddlewareTests.cs`

---

### TASK 3: Tests para Consumers de Eventos

**Objetivo**: Cubrir procesamiento de eventos (CRAP 42 → <30)

#### 3.1 MapaAsientosCreadoConsumer
**Escenarios**:
1. Procesamiento exitoso de evento
2. Creación de historial de asistencia nuevo
3. Actualización de historial existente
4. Registro en auditoría exitoso
5. Manejo de error y registro en auditoría
6. Verificar logging de información

**Archivos a Crear**:
- `Aplicacion/Consumers/MapaAsientosCreadoConsumerTests.cs`

#### 3.2 AsientoAgregadoConsumer
**Escenarios**:
1. Incremento de capacidad total
2. Incremento de asientos disponibles
3. Actualización de timestamp
4. Manejo de historial no existente
5. Manejo de error sin bloquear procesamiento

**Archivos a Crear**:
- `Aplicacion/Consumers/AsientoAgregadoConsumerTests.cs`

#### 3.3 EventoCanceladoConsumer
**Escenarios**:
1. Actualización de estado a "Cancelado"
2. Actualización de métricas existentes
3. Creación de métricas si no existen
4. Registro en auditoría
5. Manejo de error y reintento

**Archivos a Crear**:
- `Aplicacion/Consumers/EventoCanceladoConsumerTests.cs`

**Criterios de Aceptación**:
- ✅ 15+ tests nuevos pasando
- ✅ Cobertura de consumers >85%
- ✅ CRAP score <30 para todos los consumers

---

### TASK 4: Tests para Middleware Adicional

**Objetivo**: Cubrir middleware sin cobertura

#### 4.1 CorrelationIdMiddleware
**Escenarios**:
1. Generar correlation ID si no existe
2. Usar correlation ID existente del header
3. Agregar correlation ID a response headers
4. Propagar correlation ID al contexto de logging

**Archivos a Crear**:
- `API/Middleware/CorrelationIdMiddlewareTests.cs`

#### 4.2 HangfireAuthorizationFilter
**Escenarios**:
1. Permitir acceso en desarrollo
2. Denegar acceso en producción sin autenticación
3. Permitir acceso con credenciales válidas

**Archivos a Crear**:
- `API/Hangfire/HangfireAuthorizationFilterTests.cs`

**Criterios de Aceptación**:
- ✅ 7+ tests nuevos pasando
- ✅ Cobertura de middleware >80%

---

### TASK 5: Tests para Health Checks

**Objetivo**: Cubrir verificación de salud de servicios

#### 5.1 MongoDbHealthCheck
**Escenarios**:
1. MongoDB disponible → Healthy
2. MongoDB no disponible → Unhealthy
3. Timeout de conexión → Degraded
4. Verificar mensaje descriptivo

**Archivos a Crear**:
- `API/HealthChecks/MongoDbHealthCheckTests.cs`

#### 5.2 RabbitMqHealthCheck
**Escenarios**:
1. RabbitMQ disponible → Healthy
2. RabbitMQ no disponible → Unhealthy
3. Conexión lenta → Degraded
4. Verificar mensaje descriptivo

**Archivos a Crear**:
- `API/HealthChecks/RabbitMqHealthCheckTests.cs`

**Criterios de Aceptación**:
- ✅ 8+ tests nuevos pasando
- ✅ Cobertura de health checks >90%

---

### TASK 6: Tests de Integración para Program.cs

**Objetivo**: Cubrir configuración de startup (CRAP 506 → <30)

**Enfoque**: Tests de integración usando `WebApplicationFactory`

**Escenarios**:
1. Aplicación inicia correctamente
2. Servicios se registran correctamente (DI)
3. Middleware se configura en orden correcto
4. Health checks responden correctamente
5. Swagger se configura en desarrollo
6. CORS se configura correctamente
7. Hangfire se inicializa correctamente

**Archivos a Crear**:
- `API/ProgramIntegrationTests.cs`
- `API/TestWebApplicationFactory.cs`

**Criterios de Aceptación**:
- ✅ 7+ tests de integración pasando
- ✅ Cobertura de Program.cs >70%

---

### TASK 7: Tests de Integración para InyeccionDependencias

**Objetivo**: Cubrir configuración de servicios (CRAP 600 → <30)

**Escenarios**:
1. MassTransit se configura correctamente
2. Consumers se registran correctamente
3. RabbitMQ host se configura con variables de entorno
4. Política de reintentos se aplica
5. Hangfire se configura con MongoDB
6. Jobs se registran correctamente

**Archivos a Crear**:
- `Aplicacion/InyeccionDependenciasTests.cs`

**Criterios de Aceptación**:
- ✅ 6+ tests pasando
- ✅ Cobertura de InyeccionDependencias >60%

---

### TASK 8: Tests de Propiedades (Property-Based Testing)

**Objetivo**: Validar invariantes con FsCheck

**Escenarios**:
1. **Deserialización de eventos**: Cualquier evento válido se deserializa correctamente
2. **Persistencia de modelos**: Cualquier modelo válido se persiste y recupera sin pérdida de datos
3. **Cálculo de porcentajes**: Porcentaje de ocupación siempre está entre 0-100
4. **Timestamps**: Timestamps siempre son UTC y no futuros
5. **Paginación**: Paginación siempre retorna cantidad correcta de elementos

**Archivos a Crear**:
- `Aplicacion/Consumers/DeserializacionPropiedadesTests.cs`
- `Infraestructura/Repositorios/PersistenciaPropiedadesTests.cs`
- `Dominio/ModelosLectura/CalculosPropiedadesTests.cs`

**Criterios de Aceptación**:
- ✅ 15+ tests de propiedades pasando
- ✅ Invariantes críticos validados

---

### TASK 9: Ejecutar Cobertura y Verificar Objetivo

**Objetivo**: Validar que se alcanzó >90% de cobertura

**Pasos**:
1. Ejecutar `run-coverage-coverlet.ps1`
2. Revisar reporte HTML
3. Identificar áreas restantes sin cobertura
4. Agregar tests adicionales si es necesario
5. Documentar resultados finales

**Criterios de Aceptación**:
- ✅ Cobertura de líneas >90%
- ✅ Cobertura de ramas >80%
- ✅ Todos los tests pasando (0 fallando)
- ✅ CRAP score <30 en todos los componentes críticos
- ✅ Reporte de cobertura actualizado

**Archivos a Actualizar**:
- `COVERAGE-REPORT-SUMMARY.md`

---

## Patrones y Mejores Prácticas

### Patrón AAA (Arrange-Act-Assert)
```csharp
[Fact]
public async Task Consume_EventoValido_ActualizaMetricas()
{
    // Arrange
    var evento = new EventoPublicadoEventoDominio { ... };
    var context = Mock.Of<ConsumeContext<EventoPublicadoEventoDominio>>();
    
    // Act
    await _consumer.Consume(context);
    
    // Assert
    resultado.Should().NotBeNull();
    resultado.Estado.Should().Be("Publicado");
}
```

### Tests de Integración con Mongo2Go
```csharp
public class MongoIntegrationTestBase : IDisposable
{
    protected MongoDbRunner MongoRunner { get; }
    protected ReportesMongoDbContext Context { get; }
    
    public MongoIntegrationTestBase()
    {
        MongoRunner = MongoDbRunner.Start();
        Context = new ReportesMongoDbContext(MongoRunner.ConnectionString);
    }
    
    public void Dispose()
    {
        MongoRunner?.Dispose();
    }
}
```

### Tests de Middleware
```csharp
[Fact]
public async Task InvokeAsync_ArgumentException_Returns400()
{
    // Arrange
    var context = new DefaultHttpContext();
    context.Response.Body = new MemoryStream();
    
    RequestDelegate next = (ctx) => throw new ArgumentException("Invalid");
    var middleware = new GlobalExceptionHandlerMiddleware(next, _logger);
    
    // Act
    await middleware.InvokeAsync(context);
    
    // Assert
    context.Response.StatusCode.Should().Be(400);
}
```

### Property-Based Testing
```csharp
[Property]
public Property PorcentajeOcupacion_SiempreEntre0Y100(
    PositiveInt capacidad, 
    NonNegativeInt reservados)
{
    return (reservados.Get <= capacidad.Get).When(() =>
    {
        var historial = new HistorialAsistencia
        {
            CapacidadTotal = capacidad.Get,
            AsientosReservados = reservados.Get
        };
        
        return (historial.PorcentajeOcupacion >= 0 &&
                historial.PorcentajeOcupacion <= 100).ToProperty();
    });
}
```

## Orden de Ejecución Recomendado

1. **TASK 1** (Mongo2Go) - Fundamental para el resto
2. **TASK 2** (GlobalExceptionHandlerMiddleware) - Alto CRAP score
3. **TASK 3** (Consumers) - Alto CRAP score y 0% cobertura
4. **TASK 5** (Health Checks) - Relativamente simple
5. **TASK 4** (Middleware adicional) - Relativamente simple
6. **TASK 6** (Program.cs) - Requiere WebApplicationFactory
7. **TASK 7** (InyeccionDependencias) - Complejo pero importante
8. **TASK 8** (Property-Based Testing) - Opcional pero valioso
9. **TASK 9** (Verificación final) - Validación de objetivo

## Estimación de Esfuerzo

- **TASK 1**: 2-3 horas
- **TASK 2**: 1-2 horas
- **TASK 3**: 3-4 horas
- **TASK 4**: 2-3 horas
- **TASK 5**: 2-3 horas
- **TASK 6**: 2-3 horas
- **TASK 7**: 2-3 horas
- **TASK 8**: 3-4 horas (opcional)
- **TASK 9**: 1 hora

**Total estimado**: 18-26 horas (sin TASK 8: 15-22 horas)

## Métricas de Éxito

### Antes
- Cobertura: 44.71% líneas
- Tests: 68/83 pasando (15 fallando)
- CRAP score máximo: 600

### Después (Objetivo)
- Cobertura: >90% líneas
- Tests: 100% pasando (0 fallando)
- CRAP score máximo: <30
- Tests totales: ~150+
