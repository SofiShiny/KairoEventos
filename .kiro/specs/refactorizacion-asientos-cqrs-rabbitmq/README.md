# Spec: Refactorización Microservicio Asientos - CQRS + RabbitMQ

## Descripción

Esta spec documenta la refactorización completa del microservicio de Asientos para implementar correctamente el patrón CQRS, reorganizar eventos de dominio e integrar RabbitMQ con MassTransit para comunicación asíncrona entre microservicios.

## Estado

✅ **COMPLETADO** - La refactorización ha sido implementada exitosamente.

## Archivos de la Spec

- **requirements.md**: Documento de requerimientos con 12 requirements en formato EARS
- **design.md**: Documento de diseño con arquitectura, componentes y 14 correctness properties
- **tasks.md**: Plan de implementación con 12 tareas principales y múltiples sub-tareas

## Resumen de Cambios

### 1. Corrección de Violaciones CQRS

**Errores Encontrados:**
- ❌ Commands retornaban entidades completas
- ❌ Controladores con lógica de presentación
- ❌ Controladores inyectaban repositorios directamente

**Soluciones Implementadas:**
- ✅ Commands retornan solo `Guid` o `Unit`
- ✅ Queries retornan DTOs inmutables
- ✅ Controladores "thin" - solo orquestación con MediatR

### 2. Reorganización de Eventos de Dominio

**Antes:**
- Todos los eventos en un archivo consolidado `DomainEvents.cs`

**Después:**
- 5 archivos separados, uno por evento
- Namespace consistente: `Asientos.Dominio.EventosDominio`
- Cada evento hereda de `EventoDominio`

**Eventos:**
1. `MapaAsientosCreadoEventoDominio.cs`
2. `CategoriaAgregadaEventoDominio.cs`
3. `AsientoAgregadoEventoDominio.cs`
4. `AsientoReservadoEventoDominio.cs`
5. `AsientoLiberadoEventoDominio.cs`

### 3. Integración con RabbitMQ

**Implementación:**
- ✅ Instalado `MassTransit.RabbitMQ` v8.1.3
- ✅ Configurado MassTransit en `Program.cs`
- ✅ Todos los handlers publican eventos después de persistir
- ✅ Patrón: `Persistir → Publicar`

**Handlers Actualizados:**
- `CrearMapaAsientosComandoHandler`
- `AgregarAsientoComandoHandler`
- `AgregarCategoriaComandoHandler`
- `ReservarAsientoComandoHandler`
- `LiberarAsientoComandoHandler`

## Métricas

| Métrica | Valor |
|---------|-------|
| Requirements | 12 |
| Correctness Properties | 14 |
| Tareas Principales | 12 |
| Sub-tareas | 50+ |
| Archivos Creados | 9 |
| Archivos Modificados | 11 |
| Archivos Eliminados | 1 |
| Errores CQRS Corregidos | 3 |
| Eventos Reorganizados | 5 |
| Handlers con RabbitMQ | 5 |

## Arquitectura Resultante

### Separación CQRS

```
Commands (Escritura)              Queries (Lectura)
├── CrearMapaAsientosComando      ├── ObtenerMapaAsientosQuery
├── AgregarAsientoComando         └── Retorna MapaAsientosDto
├── AgregarCategoriaComando
├── ReservarAsientoComando
└── LiberarAsientoComando
     ↓
  Retornan Guid o Unit
```

### Flujo de Eventos

```
Controller → MediatR → Handler → Domain → Repository → DB
                                                      ↓
                                                MassTransit → RabbitMQ
```

## Testing

### Estrategia Dual

**Unit Tests:**
- Ejemplos específicos
- Casos edge
- Mocks para handlers

**Property-Based Tests (FsCheck):**
- Propiedades universales
- 100+ iteraciones por test
- Generación aleatoria de inputs

### Tests de Integración

- Testcontainers para RabbitMQ
- Verificación end-to-end
- Validación de eventos publicados

## Documentación Generada

1. **REFACTORIZACION-CQRS-RABBITMQ.md** - Documento técnico completo (4,500+ palabras)
2. **RESUMEN-EJECUTIVO-REFACTORIZACION.md** - Resumen ejecutivo
3. **README.md** - Documentación actualizada del microservicio

## Verificación

### Compilación

```bash
cd Asientos/backend/src/Services/Asientos
dotnet build Asientos.API/Asientos.API.csproj
```

**Resultado:** ✅ Exitosa (5.3s)

### Health Check

```bash
curl http://localhost:5000/health
```

**Respuesta:**
```json
{
  "status": "healthy",
  "db": "postgres",
  "rabbitmq": "localhost"
}
```

### RabbitMQ Management

- URL: http://localhost:15672
- Usuario: `guest` / Password: `guest`
- Verificar exchanges creados por MassTransit

## Próximos Pasos

1. ✅ Refactorización completada
2. ⏭️ Crear consumers en microservicio Reportes
3. ⏭️ Implementar retry policies en MassTransit
4. ⏭️ Agregar logging estructurado
5. ⏭️ Implementar circuit breaker

## Referencias

- [EARS Pattern](https://alistairmavin.com/ears/) - Easy Approach to Requirements Syntax
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html) - Martin Fowler
- [MassTransit Documentation](https://masstransit-project.com/) - Official docs
- [Property-Based Testing](https://fsharpforfunandprofit.com/posts/property-based-testing/) - Introduction

## Autor

**Arquitecto de Software Senior en .NET 8**  
**Fecha:** 29 de Diciembre de 2024  
**Estado:** ✅ Completado y Verificado
