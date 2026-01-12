# Plan de Mejora de Cobertura de Pruebas - Microservicio Entradas

## 🎯 Objetivo
**Aumentar la cobertura de pruebas del 12.7% al 90%** mediante la implementación sistemática de pruebas unitarias, de integración y end-to-end.

## 📊 Estado Actual
- **Cobertura de líneas**: 12.7% (349/2735 líneas)
- **Cobertura de branches**: 9.6% (58/600 branches)
- **Pruebas existentes**: 69 pruebas pasando
- **Tiempo de ejecución**: ~17 segundos

## 🚀 Estrategia de Implementación

### Fase 1: Capa API (+25% cobertura) 🔥 ALTA PRIORIDAD
**Impacto**: Mayor ganancia de cobertura con menor esfuerzo
- Pruebas de `EntradasController` (endpoints REST)
- Pruebas de middleware (`GlobalExceptionHandler`, `Metrics`, `CorrelationId`)
- Pruebas de health checks y validadores
- Pruebas de configuración de aplicación

### Fase 2: Capa Aplicación (+30% cobertura) 🔥 ALTA PRIORIDAD  
**Impacto**: Lógica de negocio crítica
- Expansión de pruebas de handlers existentes
- Pruebas completas del consumer de RabbitMQ
- Pruebas de validadores y mappers

### Fase 3: Capa Infraestructura (+25% cobertura) 🟡 MEDIA PRIORIDAD
**Impacto**: Integración con servicios externos
- Pruebas completas de repositorios
- Pruebas de servicios HTTP con políticas de resiliencia
- Pruebas de persistencia y configuración EF Core

### Fase 4: Pruebas de Integración (+10% cobertura) 🟡 MEDIA PRIORIDAD
**Impacto**: Validación end-to-end
- Integración con PostgreSQL (TestContainers)
- Integración con RabbitMQ (TestContainers)
- Pruebas end-to-end con WireMock

## 📈 Métricas de Progreso

| Fase | Cobertura Objetivo | Componentes Clave |
|------|-------------------|-------------------|
| Inicial | 12.7% | Estado actual |
| Fase 1 | 37% | API + Middleware |
| Fase 2 | 67% | + Handlers + Consumer |
| Fase 3 | 92% | + Repositorios + EF Core |
| Fase 4 | 95% | + Integración completa |

## 🛠️ Herramientas Necesarias

### Paquetes NuGet Adicionales
```xml
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.0" />
<PackageReference Include="Testcontainers.PostgreSql" Version="3.6.0" />
<PackageReference Include="Testcontainers.RabbitMq" Version="3.6.0" />
<PackageReference Include="WireMock.Net" Version="1.5.45" />
<PackageReference Include="Bogus" Version="35.0.1" />
```

### Scripts de Automatización
```bash
# Ejecutar pruebas con cobertura
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults

# Generar reporte HTML
reportgenerator -reports:"TestResults/**/coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:Html
```

## 📋 Próximos Pasos Inmediatos

### 1. Configurar Herramientas (30 min)
```bash
# Instalar paquetes adicionales
dotnet add package Microsoft.AspNetCore.Mvc.Testing
dotnet add package Testcontainers.PostgreSql
dotnet add package WireMock.Net
dotnet add package Bogus
```

### 2. Implementar Fase 1 - Pruebas de API (2-3 días)
- **Prioridad 1**: `EntradasControllerTests.cs`
- **Prioridad 2**: `GlobalExceptionHandlerMiddlewareTests.cs`
- **Prioridad 3**: `MetricsMiddlewareTests.cs`

### 3. Verificar Progreso
```bash
# Ejecutar y verificar cobertura después de cada fase
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:"TestResults/**/coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:Html
```

## 🎯 Criterios de Éxito

### Objetivos Técnicos
- ✅ Cobertura de líneas >90%
- ✅ Cobertura de branches >85%
- ✅ Tiempo de ejecución <60 segundos
- ✅ Flakiness <1%

### Objetivos de Calidad
- ✅ Todas las capas arquitectónicas cubiertas
- ✅ Casos edge y manejo de errores probados
- ✅ Integración con servicios externos validada
- ✅ Flujos end-to-end funcionando

## 🚨 Riesgos y Mitigaciones

| Riesgo | Probabilidad | Impacto | Mitigación |
|--------|-------------|---------|------------|
| Complejidad de mocking | Media | Alto | Usar WireMock para HTTP, TestContainers para servicios |
| Tiempo de ejecución lento | Alta | Medio | Paralelización y optimización de containers |
| Pruebas inestables | Media | Alto | Polling patterns y timeouts apropiados |
| Resistencia del equipo | Baja | Alto | Mostrar valor incremental por fase |

## 📞 Soporte y Recursos

- **Documentación detallada**: `.kiro/specs/microservicio-entradas/test-coverage-improvement.md`
- **Tareas específicas**: `.kiro/specs/microservicio-entradas/tasks.md` (Task 14)
- **Ejemplos de implementación**: Revisar microservicio Reportes (90%+ cobertura)

---

**¿Listo para empezar?** 🚀 
Comienza con la **Fase 1** implementando `EntradasControllerTests.cs` para obtener el mayor impacto inmediato en cobertura.