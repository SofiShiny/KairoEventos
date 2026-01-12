# Plan de Cobertura para Risk Hotspots - Microservicio Entradas

## 🚨 Análisis de Risk Hotspots Identificados

### Hotspots Críticos por Crap Score

| Componente | Crap Score | Complejidad Ciclomática | Prioridad |
|------------|------------|-------------------------|-----------|
| Microsoft.AspNetCore.OpenApi.Generated.TransformAsync(...) | 855 | 692 | 🔴 CRÍTICA |
| Microsoft.AspNetCore.OpenApi.Generated.TransformAsync(...) | 1190 | 34 | 🔴 CRÍTICA |
| Microsoft.AspNetCore.OpenApi.Generated.GetTypeDocId(...) | 812 | 28 | 🟡 ALTA |
| Microsoft.AspNetCore.OpenApi.Generated.CreateDocumentationId(...) | 342 | 18 | 🟡 ALTA |
| RepositorioEntradas.GuardarAsync() | 110 | 10 | 🟠 MEDIA |
| VerificadorAsientosHttp.ObtenerInfoAsientoAsync() | 72 | 8 | 🟠 MEDIA |
| UnitOfWork.CommitTransactionAsync() | 42 | 6 | 🟠 MEDIA |

## 🎯 Estrategia de Mitigación por Categorías

### 1. Código Generado de OpenAPI (Crap Score: 855-1190)

**Problema**: El código generado automáticamente por ASP.NET Core OpenAPI tiene complejidad extremadamente alta y no está cubierto por pruebas.

**Estrategia de Mitigación**:

#### Opción A: Exclusión de Cobertura (RECOMENDADA)
```xml
<!-- En Entradas.API.csproj -->
<PropertyGroup>
  <ExcludeFromCodeCoverage>
    Microsoft.AspNetCore.OpenApi.Generated.*
  </ExcludeFromCodeCoverage>
</PropertyGroup>
```

#### Opción B: Pruebas de Integración de OpenAPI
```csharp
// Entradas.Pruebas/API/OpenApiIntegrationTests.cs
[Fact]
public async Task OpenApi_Schema_Should_Be_Valid()
{
    // Validar que el schema OpenAPI se genera correctamente
    var response = await _client.GetAsync("/swagger/v1/swagger.json");
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    
    var content = await response.Content.ReadAsStringAsync();
    var schema = JsonDocument.Parse(content);
    
    // Validaciones básicas del schema
    schema.RootElement.GetProperty("openapi").GetString().Should().StartWith("3.0");
    schema.RootElement.GetProperty("info").GetProperty("title").GetString().Should().Be("Entradas API");
}
```

**Justificación**: El código generado automáticamente no requiere pruebas unitarias detalladas, pero sí validación de integración.

### 2. Repositorio de Entradas (Crap Score: 110)

**Problema**: `RepositorioEntradas.GuardarAsync()` tiene alta complejidad debido a manejo de transacciones y validaciones.

**Solución**: Pruebas exhaustivas del método crítico

```csharp
// Entradas.Pruebas/Infraestructura/Repositorios/RepositorioEntradasCriticalTests.cs
public class RepositorioEntradasCriticalTests
{
    [Fact]
    public async Task GuardarAsync_ConEntradaValida_DebeGuardarCorrectamente()
    {
        // Arrange
        var entrada = EntradaTestBuilder.UnaEntradaValida().Build();
        
        // Act
        var resultado = await _repositorio.GuardarAsync(entrada);
        
        // Assert
        resultado.Should().NotBeNull();
        resultado.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GuardarAsync_ConErrorDeBaseDatos_DebeLanzarExcepcion()
    {
        // Arrange - Simular error de BD
        var entrada = EntradaTestBuilder.UnaEntradaInvalida().Build();
        
        // Act & Assert
        await FluentActions.Invoking(() => _repositorio.GuardarAsync(entrada))
            .Should().ThrowAsync<InfraestructuraException>();
    }

    [Fact]
    public async Task GuardarAsync_ConTransaccionFallida_DebeHacerRollback()
    {
        // Arrange
        var entrada = EntradaTestBuilder.UnaEntradaValida().Build();
        
        // Simular fallo en transacción
        _mockUnitOfWork.Setup(x => x.CommitAsync()).ThrowsAsync(new Exception("DB Error"));
        
        // Act & Assert
        await FluentActions.Invoking(() => _repositorio.GuardarAsync(entrada))
            .Should().ThrowAsync<Exception>();
            
        // Verificar que no se guardó nada
        var entradas = await _repositorio.ObtenerTodosAsync();
        entradas.Should().BeEmpty();
    }
}
```

### 3. Verificador de Asientos HTTP (Crap Score: 72)

**Problema**: `VerificadorAsientosHttp.ObtenerInfoAsientoAsync()` tiene complejidad por manejo de políticas de resiliencia.

**Solución**: Pruebas de políticas de resiliencia y casos edge

```csharp
// Entradas.Pruebas/Infraestructura/ServiciosExternos/VerificadorAsientosHttpResilienceTests.cs
public class VerificadorAsientosHttpResilienceTests
{
    [Fact]
    public async Task ObtenerInfoAsientoAsync_ConTimeoutTransitorio_DebeReintentar()
    {
        // Arrange
        _mockHttpMessageHandler
            .SetupSequence(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException()) // Primer intento - timeout
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) // Segundo intento - éxito
            {
                Content = new StringContent(JsonSerializer.Serialize(new AsientoDto { Id = 1, Disponible = true }))
            });

        // Act
        var resultado = await _verificador.ObtenerInfoAsientoAsync(1, 1);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Disponible.Should().BeTrue();
        
        // Verificar que se hicieron 2 intentos
        _mockHttpMessageHandler.Verify(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()), 
            Times.Exactly(2));
    }

    [Fact]
    public async Task ObtenerInfoAsientoAsync_ConCircuitBreakerAbierto_DebeLanzarExcepcion()
    {
        // Arrange - Simular múltiples fallos para abrir circuit breaker
        for (int i = 0; i < 5; i++)
        {
            _mockHttpMessageHandler
                .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("Service unavailable"));
                
            try { await _verificador.ObtenerInfoAsientoAsync(1, 1); } catch { }
        }

        // Act & Assert - El circuit breaker debería estar abierto
        await FluentActions.Invoking(() => _verificador.ObtenerInfoAsientoAsync(1, 1))
            .Should().ThrowAsync<CircuitBreakerOpenException>();
    }
}
```

### 4. Unit of Work (Crap Score: 42)

**Problema**: `UnitOfWork.CommitTransactionAsync()` tiene complejidad por manejo de transacciones distribuidas.

**Solución**: Pruebas de transacciones y rollbacks

```csharp
// Entradas.Pruebas/Infraestructura/Persistencia/UnitOfWorkTransactionTests.cs
public class UnitOfWorkTransactionTests
{
    [Fact]
    public async Task CommitTransactionAsync_ConOperacionExitosa_DebeCommitear()
    {
        // Arrange
        using var unitOfWork = new UnitOfWork(_dbContext);
        await unitOfWork.BeginTransactionAsync();
        
        var entrada = EntradaTestBuilder.UnaEntradaValida().Build();
        _dbContext.Entradas.Add(entrada);

        // Act
        await unitOfWork.CommitTransactionAsync();

        // Assert
        var entradaGuardada = await _dbContext.Entradas.FirstOrDefaultAsync(e => e.Id == entrada.Id);
        entradaGuardada.Should().NotBeNull();
    }

    [Fact]
    public async Task CommitTransactionAsync_ConErrorEnCommit_DebeHacerRollback()
    {
        // Arrange
        using var unitOfWork = new UnitOfWork(_dbContext);
        await unitOfWork.BeginTransactionAsync();
        
        // Simular error en commit
        _mockDbContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateException("Constraint violation"));

        // Act & Assert
        await FluentActions.Invoking(() => unitOfWork.CommitTransactionAsync())
            .Should().ThrowAsync<DbUpdateException>();
            
        // Verificar que la transacción se hizo rollback
        _dbContext.Database.CurrentTransaction.Should().BeNull();
    }

    [Fact]
    public async Task CommitTransactionAsync_ConTransaccionDistribuida_DebeCoordinarCorrectamente()
    {
        // Arrange
        using var unitOfWork = new UnitOfWork(_dbContext);
        await unitOfWork.BeginTransactionAsync();
        
        // Simular operaciones en múltiples agregados
        var entrada = EntradaTestBuilder.UnaEntradaValida().Build();
        _dbContext.Entradas.Add(entrada);
        
        // Simular publicación de evento
        var evento = new EntradaCreadaEvento(entrada.Id, entrada.EventoId, entrada.UsuarioId);
        
        // Act
        await unitOfWork.CommitTransactionAsync();
        await _eventPublisher.PublishAsync(evento);

        // Assert
        // Verificar que tanto la persistencia como la publicación fueron exitosas
        var entradaGuardada = await _dbContext.Entradas.FirstOrDefaultAsync(e => e.Id == entrada.Id);
        entradaGuardada.Should().NotBeNull();
        
        _mockEventPublisher.Verify(x => x.PublishAsync(It.IsAny<EntradaCreadaEvento>()), Times.Once);
    }
}
```

## 📊 Plan de Implementación Priorizado

### Fase 1: Mitigación Inmediata (1-2 días)
1. **Excluir código generado OpenAPI** de métricas de cobertura
2. **Implementar pruebas críticas** para `RepositorioEntradas.GuardarAsync()`
3. **Crear pruebas de resiliencia** para `VerificadorAsientosHttp`

### Fase 2: Cobertura Completa (3-5 días)
1. **Completar pruebas de UnitOfWork** con todos los escenarios de transacciones
2. **Agregar pruebas de integración** para validar comportamiento real
3. **Implementar property-based tests** para validar invariantes

### Fase 3: Validación y Optimización (1-2 días)
1. **Ejecutar análisis de cobertura** y verificar reducción de risk hotspots
2. **Optimizar pruebas lentas** y eliminar flakiness
3. **Documentar casos no cubiertos** con justificación técnica

## 🎯 Métricas de Éxito

### Objetivos de Reducción de Risk Hotspots
- **OpenAPI Generated Code**: Excluido de métricas (Crap Score: N/A)
- **RepositorioEntradas.GuardarAsync()**: Crap Score < 10
- **VerificadorAsientosHttp.ObtenerInfoAsientoAsync()**: Crap Score < 15
- **UnitOfWork.CommitTransactionAsync()**: Crap Score < 8

### Métricas de Cobertura Objetivo
- **Cobertura de líneas**: >90% (excluyendo código generado)
- **Cobertura de branches**: >85%
- **Risk Hotspots**: Reducir de 10 a máximo 2 componentes con Crap Score >30

## 🛠️ Herramientas y Configuración

### Configuración de Exclusiones
```xml
<!-- Directory.Build.props -->
<PropertyGroup>
  <CodeCoverageExcludeByFile>**/Microsoft.AspNetCore.OpenApi.Generated.*</CodeCoverageExcludeByFile>
  <CodeCoverageExcludeByAttribute>GeneratedCodeAttribute</CodeCoverageExcludeByAttribute>
</PropertyGroup>
```

### Script de Análisis de Risk Hotspots
```powershell
# analyze-risk-hotspots.ps1
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
reportgenerator -reports:"TestResults/**/coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:Html;JsonSummary

# Analizar métricas de complejidad
dotnet tool install --global dotnet-complexity
dotnet complexity --output complexity-report.json
```

## 📋 Checklist de Implementación

### ✅ Tareas Completadas
- [x] Análisis de risk hotspots identificados
- [x] Estrategia de mitigación definida
- [x] Plan de implementación priorizado

### 🔄 Tareas en Progreso
- [ ] Implementar exclusiones de código generado
- [ ] Crear pruebas críticas para RepositorioEntradas
- [ ] Implementar pruebas de resiliencia para VerificadorAsientos
- [ ] Completar pruebas de UnitOfWork

### ⏳ Tareas Pendientes
- [ ] Ejecutar análisis de cobertura post-implementación
- [ ] Validar reducción de risk hotspots
- [ ] Documentar resultados y lecciones aprendidas

---

**🚀 Próximo Paso**: Comenzar con la **Fase 1** implementando las exclusiones de código generado y las pruebas críticas del repositorio para obtener el mayor impacto inmediato en la reducción de risk hotspots.