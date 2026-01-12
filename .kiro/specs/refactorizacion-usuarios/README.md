# Especificación: Refactorización Microservicio Usuarios

## Estado: ✅ ESPECIFICACIÓN COMPLETA

Esta especificación define la refactorización completa del microservicio Usuarios desde su arquitectura actual (que no cumple estándares) hacia una Arquitectura Hexagonal profesional con CQRS, siguiendo los mismos patrones y calidad de los microservicios Eventos, Asientos y Reportes.

## Archivos de la Especificación

### ✅ requirements.md
**Estado:** Completo y aprobado por el usuario

Define 12 requirements con 84 acceptance criteria que cubren:
- Arquitectura Hexagonal (4 capas)
- CQRS con MediatR
- Modelo de Dominio Rico con Value Objects
- Repository Pattern
- Gestión de Usuarios (CRUD)
- Integración con Keycloak
- Validación con FluentValidation
- Manejo de Errores
- Testing Comprehensivo (>90% cobertura)
- Persistencia con PostgreSQL
- Logging y Observabilidad
- Dockerización

### ✅ design.md
**Estado:** Completo y aprobado por el usuario

Incluye:
- Diagramas de arquitectura de alto nivel
- Estructura de componentes detallada
- Diseño de 4 capas (Dominio, Aplicación, Infraestructura, API)
- Entidad Usuario (Aggregate Root) con métodos de negocio
- Value Objects: Correo, Telefono, Direccion
- Commands y Queries con sus Handlers
- DTOs y Validators
- Repositorio y DbContext con EF Core
- Servicio de integración con Keycloak
- Schema de base de datos PostgreSQL
- 10 Correctness Properties
- Estrategia de manejo de errores
- Estrategia de testing
- Configuración de Docker
- Path de migración en 7 fases

### ✅ tasks.md
**Estado:** Completo - LISTO PARA IMPLEMENTACIÓN

Plan de implementación con 16 tareas principales y 70+ subtareas que cubren:

1. **Preparación** (8 subtareas): Estructura de carpetas, proyectos .NET, dependencias NuGet
2. **Dominio** (8 subtareas): Entidad Usuario, Value Objects, interfaces, excepciones
3. **Tests de Dominio** (4 subtareas): Tests unitarios de entidades y Value Objects
4. **Checkpoint 1**: Compilación de Dominio
5. **Aplicación** (9 subtareas): Commands, Queries, Handlers, DTOs, Validators
6. **Tests de Aplicación** (6 subtareas): Tests de Handlers y Validators
7. **Checkpoint 2**: Compilación de Aplicación
8. **Infraestructura** (6 subtareas): DbContext, Repositorio, ServicioKeycloak, migraciones
9. **Tests de Infraestructura** (3 subtareas): Tests de Repositorio y Keycloak
10. **Checkpoint 3**: Compilación de Infraestructura
11. **API** (5 subtareas): Controller, Middleware, Program.cs, appsettings
12. **Tests de API** (2 subtareas): Tests de Controller y end-to-end
13. **Property Tests** (7 subtareas): Validación de Correctness Properties
14. **Dockerización** (4 subtareas): Dockerfile, docker-compose, health checks
15. **Documentación** (1 subtarea): README completo
16. **Checkpoint Final** (3 subtareas): Verificación completa, cobertura, prueba end-to-end

## Características Clave

### Arquitectura
- **Hexagonal Architecture** con 4 capas bien separadas
- **CQRS** con separación clara de Commands y Queries
- **Domain-Driven Design** con Aggregate Root y Value Objects
- **Repository Pattern** para abstracción de persistencia

### Tecnologías
- **.NET 8** con C# 12
- **Entity Framework Core 8.0** con PostgreSQL
- **MediatR** para CQRS
- **FluentValidation** para validación
- **Serilog** para logging estructurado
- **xUnit, Moq, FluentAssertions** para testing
- **Testcontainers** para tests de integración
- **Docker** para containerización

### Calidad
- **>90% cobertura de tests** (unitarios, integración, property-based)
- **10 Correctness Properties** verificadas con property-based testing
- **Manejo de errores robusto** con middleware global
- **Logging estructurado** con Serilog
- **Health checks** configurados

### Integración
- **PostgreSQL** para persistencia (base de datos kairo_usuarios)
- **Keycloak** para autenticación y autorización
- **Docker network** kairo-network para comunicación entre servicios

## Próximos Pasos

1. **Revisar la especificación completa** (requirements.md, design.md, tasks.md)
2. **Comenzar implementación** siguiendo tasks.md secuencialmente
3. **Validar en cada checkpoint** antes de continuar
4. **Ejecutar tests continuamente** para mantener >90% cobertura
5. **Documentar cambios** en README.md del microservicio

## Comparación con Código Actual

### Problemas del Código Actual
❌ Mezcla de responsabilidades entre capas (Core, Application, Infrastructure)  
❌ No hay separación clara entre Commands y Queries  
❌ Entidades anémicas sin lógica de negocio  
❌ Repositorios genéricos que no expresan el dominio  
❌ Validaciones dispersas sin un patrón claro  
❌ Tests insuficientes y sin cobertura adecuada  
❌ Integración con Keycloak acoplada a la infraestructura  

### Solución Propuesta
✅ Arquitectura Hexagonal con 4 capas bien definidas  
✅ CQRS con MediatR para separar lecturas y escrituras  
✅ Modelo de dominio rico con Value Objects  
✅ Repository Pattern específico del dominio  
✅ Validación centralizada con FluentValidation  
✅ Testing comprehensivo (>90% cobertura)  
✅ Integración con Keycloak mediante servicios de dominio  

## Referencias

Esta especificación sigue los mismos estándares de calidad que:
- **Eventos**: Microservicio de referencia con Hexagonal + CQRS + RabbitMQ
- **Asientos**: Refactorización exitosa documentada en `.kiro/specs/refactorizacion-asientos-cqrs-rabbitmq/`
- **Reportes**: Microservicio con MongoDB y consumers de RabbitMQ

---

**Fecha de creación:** 30 de diciembre de 2025  
**Estado:** Especificación completa - Lista para implementación  
**Próximo paso:** Comenzar con Task 1 (Preparación y estructura inicial)
