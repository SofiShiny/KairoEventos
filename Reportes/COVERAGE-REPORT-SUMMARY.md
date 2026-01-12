# Reporte de Cobertura de Código - Microservicio Reportes

**Fecha**: 31 de diciembre de 2025  
**Proyecto**: Reportes.Pruebas  
**Framework**: .NET 8.0 con xUnit, Moq, FluentAssertions

## Resumen Ejecutivo

✅ **Error de compilación corregido**: Se solucionó el error en `ReportesController.cs` línea 155 donde se intentaba acceder a `FechaPublicacion` en lugar de `FechaCreacion`.

## Métricas de Cobertura

| Métrica | Valor | Estado |
|---------|-------|--------|
| **Cobertura de Líneas** | **44.71%** | ⚠️ Requiere mejora |
| **Cobertura de Ramas** | **38.55%** | ⚠️ Requiere mejora |
| **Líneas Cubiertas** | 681 / 1,523 | - |
| **Ramas Cubiertas** | 64 / 166 | - |

## Resultados de Tests

| Categoría | Cantidad |
|-----------|----------|
| ✅ Tests Pasando | 68 |
| ❌ Tests Fallando | 15 |
| **Total** | **83** |

## Tests Fallando

### 1. RepositorioReportesLecturaTests (10 tests)
**Problema**: Moq no puede crear un proxy de la clase `ReportesMongoDbContext` porque es una clase concreta, no una interfaz.

**Tests afectados**:
- `RegistrarLogAuditoriaAsync_DebeInsertarLogCorrectamente`
- `ActualizarAsistenciaAsync_CuandoMongoDBNoDisponible_DebeLanzarExcepcion`
- `ActualizarMetricasAsync_CuandoMongoDBNoDisponible_DebeLanzarExcepcion`
- `ActualizarVentasDiariasAsync_DebeActualizarReporteCorrectamente`
- `RegistrarLogAuditoriaAsync_DebeEstablecerTimestamp`
- `ActualizarAsistenciaAsync_DebeActualizarHistorialCorrectamente`
- `ActualizarMetricasAsync_DebeEstablecerUltimaActualizacion`
- `ActualizarMetricasAsync_DebeActualizarMetricasCorrectamente`
- `RegistrarLogAuditoriaAsync_CuandoMongoDBNoDisponible_DebeLanzarExcepcion`
- `ObtenerLogsAuditoriaAsync_CuandoMongoDBNoDisponible_DebeLanzarExcepcion`

**Solución recomendada**: Estos tests deberían ser tests de integración usando Mongo2Go en lugar de mocks.

### 2. MongoDbIntegrationTests (5 tests)
**Problema**: Los tests insertan datos en MongoDB pero luego no los encuentran al consultar. Posible problema con índices o con la configuración de Mongo2Go.

**Tests afectados**:
- `ActualizarMetricasAsync_DebeActualizarRegistroExistente`
- `ObtenerMetricasEventoAsync_DebeCompletarseEnMenosDe500ms`
- `ActualizarMetricasAsync_DebeInsertarYRecuperarCorrectamente`
- `ActualizarAsistenciaAsync_DebeInsertarYRecuperarCorrectamente`
- `ActualizarMetricasAsync_DebeSerOperacionAtomica`

**Solución recomendada**: Revisar la configuración de Mongo2Go y asegurar que los índices estén correctamente configurados.

## Estructura de Tests

```
Reportes.Pruebas/
├── API/
│   ├── ReportesControllerTests.cs
│   └── EndpointsPropiedadesTests.cs
├── Aplicacion/
│   ├── Consumers/
│   │   ├── ConsumidoresPropiedadesTests.cs
│   │   ├── DeserializacionPropiedadesTests.cs
│   │   ├── ManejoErroresPropiedadesTests.cs
│   │   └── ResilienciaTests.cs
│   └── Jobs/
│       ├── JobGenerarReportesConsolidadosTests.cs
│       └── ConsolidacionPropiedadesTests.cs
├── Dominio/
│   └── ModelosLectura/
│       └── HistorialAsistenciaPropiedadesTests.cs
└── Infraestructura/
    ├── MongoDbIntegrationTests.cs
    └── Repositorios/
        ├── RepositorioReportesLecturaTests.cs
        └── RepositorioPersistenciaPropiedadesTests.cs
```

## Recomendaciones

### Prioridad Alta
1. **Convertir tests unitarios a tests de integración**: Los tests en `RepositorioReportesLecturaTests.cs` deberían usar Mongo2Go en lugar de mocks, similar a como se hizo en el microservicio Foros.

2. **Corregir tests de integración**: Investigar por qué `ObtenerMetricasEventoAsync` retorna null después de insertar datos.

### Prioridad Media
3. **Aumentar cobertura**: Agregar más tests para alcanzar el objetivo de >90% de cobertura de líneas.

4. **Tests de propiedades**: Los tests basados en propiedades (Property-Based Testing) están funcionando correctamente y proporcionan buena cobertura.

## Comparación con Foros

| Métrica | Foros | Reportes | Diferencia |
|---------|-------|----------|------------|
| Cobertura de Líneas | 99.6% | 44.71% | -54.89% |
| Cobertura de Ramas | 100% | 38.55% | -61.45% |
| Tests Pasando | 49 | 68 | +19 |
| Tests Fallando | 0 | 15 | +15 |

## Próximos Pasos

1. Refactorizar `RepositorioReportesLecturaTests.cs` para usar Mongo2Go
2. Investigar y corregir los tests de integración que fallan
3. Agregar tests faltantes para aumentar la cobertura
4. Ejecutar el reporte de cobertura nuevamente una vez corregidos los tests

## Comando para Ejecutar Tests

```powershell
# Ejecutar todos los tests con cobertura
.\run-coverage.ps1

# O manualmente:
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
```

## Notas

- El proyecto compila correctamente después de corregir el error en `ReportesController.cs`
- 68 de 83 tests están pasando (81.9%)
- La infraestructura de tests está bien configurada con xUnit, Moq, FluentAssertions y Mongo2Go
- Se necesita trabajo adicional para alcanzar el objetivo de >90% de cobertura
