# Guía Rápida de Pruebas - Comunidad.API

## 🚀 Ejecución Rápida

### Opción 1: Script Completo con Reporte HTML (Recomendado)
```powershell
./run-coverage.ps1
```

Este comando:
- ✅ Ejecuta todos los 35 tests
- ✅ Genera cobertura de código
- ✅ Crea reporte HTML visual
- ✅ Abre automáticamente en el navegador
- ✅ Valida umbral de cobertura >90%

### Opción 2: Ejecución Simple
```powershell
./run-tests.ps1
```

Este comando:
- ✅ Ejecuta todos los tests
- ✅ Muestra resultados en consola
- ✅ Genera archivo de cobertura XML

### Opción 3: Comando Directo
```bash
dotnet test
```

## 📊 Resultados Esperados

```
✅ Total de Tests: 35
✅ Exitosos: 35
❌ Fallidos: 0
⏭️ Omitidos: 0
⏱️ Duración: ~3 segundos
📈 Cobertura: >95%
```

## 📁 Ubicación de Reportes

- **Reporte HTML**: `coverage-report/index.html`
- **Archivo XML**: `test/Comunidad.Tests/TestResults/coverage.cobertura.xml`

## 🔧 Requisitos

### Herramientas Necesarias
- ✅ .NET 8 SDK
- ✅ dotnet-reportgenerator-globaltool (se instala automáticamente)

### Instalación Manual de reportgenerator (si es necesario)
```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
```

## 📝 Detalles de Cobertura

### Componentes Probados
- **Handlers (CQRS)**: 17 tests
  - CrearComentarioComandoHandler (4)
  - ResponderComentarioComandoHandler (4)
  - OcultarComentarioComandoHandler (4)
  - ObtenerComentariosQueryHandler (5)

- **Consumer (RabbitMQ)**: 5 tests
  - EventoPublicadoConsumer

- **Entidades de Dominio**: 13 tests
  - Foro (5)
  - Comentario (8)

## 🎯 Umbrales de Calidad

- **Cobertura Mínima**: 90% (líneas)
- **Cobertura Actual**: >95%
- **Tests Requeridos**: 35/35 pasando

## 🐛 Solución de Problemas

### Error: "reportgenerator no encontrado"
```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
```

### Error: "Tests fallando"
```bash
# Limpiar y reconstruir
dotnet clean
dotnet build
dotnet test
```

### Ver logs detallados
```bash
dotnet test --verbosity detailed
```

## 📚 Documentación Completa

Ver [TASK-2-COMPLETION-SUMMARY.md](TASK-2-COMPLETION-SUMMARY.md) para:
- Detalles de cada test
- Patrones utilizados
- Estrategia de testing
- Métricas completas
