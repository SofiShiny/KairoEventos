# Plan de Implementación: Microservicio Entradas.API

## Overview

Este plan implementa el microservicio Entradas.API siguiendo Arquitectura Hexagonal estricta con .NET 8, PostgreSQL, Entity Framework Core, y RabbitMQ. El enfoque es incremental, construyendo desde el dominio hacia afuera, con testing comprehensivo en cada paso.

## Tasks

- [x] 1. Configurar estructura de proyecto y solución
  - Crear solución `Entradas.sln` con 5 proyectos
  - Configurar referencias entre proyectos según arquitectura hexagonal
  - Instalar paquetes NuGet necesarios (EF Core, MediatR, MassTransit, xUnit, Moq, FluentAssertions, FsCheck)
  - _Requirements: 8.1, 8.4_

- [x] 2. Implementar capa de dominio
  - [x] 2.1 Crear entidad Entrada con todas las propiedades
    - Implementar entidad `Entrada` con propiedades: Id, EventoId, UsuarioId, AsientoId, Monto, CodigoQr, Estado, FechaCompra
    - Implementar enum `EstadoEntrada` con valores: PendientePago, Pagada, Cancelada, Usada
    - Agregar factory method `Crear` y métodos de negocio (`ConfirmarPago`, `Cancelar`, `MarcarComoUsada`)
    - _Requirements: 1.1, 1.2, 1.3_

  - [ ]* 2.2 Escribir property test para estructura de entidad
    - **Property 1: Estructura de Entidad Entrada**
    - **Validates: Requirements 1.1**

  - [ ]* 2.3 Escribir property test para estados válidos
    - **Property 2: Estados Válidos de Entrada**
    - **Property 3: Estado Inicial de Entrada**
    - **Validates: Requirements 1.2, 1.3**

  - [x] 2.4 Definir interfaces de servicios externos
    - Crear interfaces `IVerificadorEventos`, `IVerificadorAsientos`, `IGeneradorCodigoQr`
    - Definir interface `IRepositorioEntradas` con métodos CRUD
    - Crear excepciones de dominio específicas
    - _Requirements: 6.1, 6.2_

- [-] 3. Implementar generador de códigos QR
  - [x] 3.1 Crear implementación de IGeneradorCodigoQr
    - Implementar generación con formato "TICKET-{Guid}-{Random}"
    - Usar componentes criptográficamente seguros
    - _Requirements: 2.1, 2.4_

  - [ ]* 3.2 Escribir property tests para códigos QR
    - **Property 4: Formato de Código QR**
    - **Property 5: Unicidad de Códigos QR**
    - **Validates: Requirements 2.1, 2.2**

- [x] 4. Implementar capa de aplicación
  - [x] 4.1 Crear comando CrearEntradaCommand y su handler
    - Definir record `CrearEntradaCommand` con validaciones FluentValidation
    - Implementar `CrearEntradaCommandHandler` con orquestación síncrona
    - Integrar llamadas a verificadores externos
    - _Requirements: 3.1, 3.2, 4.1, 9.1_

  - [ ]* 4.2 Escribir property tests para validación externa
    - **Property 6: Validación Externa Obligatoria**
    - **Property 7: Rechazo por Validación Externa Fallida**
    - **Validates: Requirements 3.1, 3.3**

  - [ ]* 4.3 Escribir property test para creación exitosa
    - **Property 8: Creación Exitosa de Entrada**
    - **Validates: Requirements 4.1**

  - [x] 4.4 Crear DTOs y mappers
    - Implementar `EntradaCreadaDto`, `CrearEntradaDto`
    - Crear mappers entre entidades y DTOs
    - _Requirements: 9.5, 13.3_

  - [ ]* 4.5 Escribir unit tests para CrearEntradaHandler
    - Test con mocks de servicios externos (escenario exitoso)
    - Test con verificador de eventos fallando
    - Test con verificador de asientos fallando
    - _Requirements: 11.2_

- [x] 5. Checkpoint - Validar dominio y aplicación
  - Ensure all tests pass, ask the user if questions arise.

- [x] 6. Implementar capa de infraestructura
  - [x] 6.1 Configurar Entity Framework Core con PostgreSQL
    - Crear `EntradasDbContext` con configuración de entidades
    - Implementar `EntradaConfiguration` para mapeo ORM
    - Configurar migraciones Code First
    - _Requirements: 7.1, 7.2, 7.3_

  - [x] 6.2 Implementar repositorio de entradas
    - Crear `RepositorioEntradas` implementando `IRepositorioEntradas`
    - Implementar métodos CRUD con Entity Framework
    - Agregar manejo de transacciones
    - _Requirements: 7.5_

  - [ ]* 6.3 Escribir property test para atomicidad
    - **Property 10: Atomicidad de Transacciones**
    - **Validates: Requirements 4.5**

  - [x] 6.4 Implementar servicios HTTP externos
    - Crear `VerificadorEventosHttp` implementando `IVerificadorEventos`
    - Crear `VerificadorAsientosHttp` implementando `IVerificadorAsientos`
    - Configurar HttpClient con políticas de retry y circuit breaker
    - _Requirements: 6.3, 6.5_

- [x] 7. Implementar integración con RabbitMQ
  - [x] 7.1 Configurar MassTransit
    - Configurar MassTransit con RabbitMQ
    - Definir contratos de eventos (`EntradaCreadaEvento`, `PagoConfirmadoEvento`)
    - _Requirements: 10.1, 10.5_

  - [x] 7.2 Implementar publicación de eventos
    - Integrar publicación de `EntradaCreadaEvento` en CrearEntradaHandler
    - _Requirements: 4.3, 10.2_

  - [ ]* 7.3 Escribir property test para publicación de eventos
    - **Property 9: Publicación de Eventos**
    - **Validates: Requirements 4.3**

  - [x] 7.4 Crear consumer para pagos confirmados
    - Implementar `PagoConfirmadoConsumer`
    - Manejar localización y actualización de estado de entrada
    - _Requirements: 5.1, 5.2, 5.3, 10.3_

  - [ ]* 7.5 Escribir property tests para consumer
    - **Property 11: Localización en Consumer**
    - **Property 12: Transición de Estado por Pago**
    - **Validates: Requirements 5.2, 5.3**

- [x] 8. Implementar capa de API
  - [x] 8.1 Crear controlador de entradas
    - Implementar `EntradasController` con endpoints RESTful
    - Configurar routing y model binding
    - _Requirements: 13.1, 13.3_

  - [x] 8.2 Configurar validación y manejo de errores
    - Implementar middleware de manejo global de excepciones
    - Configurar FluentValidation para DTOs
    - _Requirements: 14.1, 14.2, 14.3_

  - [ ]* 8.3 Escribir property tests para validación
    - **Property 13: Validación de Comandos**
    - **Property 14: Códigos de Estado HTTP Apropiados**
    - **Validates: Requirements 9.4, 13.2, 14.1**

  - [x] 8.4 Configurar Swagger/OpenAPI
    - Agregar documentación automática de API
    - Configurar ejemplos y descripciones
    - _Requirements: 13.4_

- [x] 9. Configurar logging y observabilidad
  - [x] 9.1 Implementar structured logging
    - Configurar Serilog con structured logging
    - Agregar correlation IDs para tracing distribuido
    - _Requirements: 12.2, 12.3, 12.5_

  - [x] 9.2 Configurar métricas y health checks
    - Implementar health checks para PostgreSQL y RabbitMQ
    - Configurar métricas de performance
    - _Requirements: 12.4_

- [ ] 10. Implementar tests de integración
  - [ ] 10.1 Crear tests de integración de base de datos
    - Usar TestContainers para PostgreSQL
    - Test de persistencia y consultas
    - _Requirements: 11.5_

  - [ ] 10.2 Crear tests de integración de RabbitMQ
    - Usar TestContainers para RabbitMQ
    - Test de publicación y consumo de eventos
    - _Requirements: 11.5_

  - [ ] 10.3 Crear tests de integración de API
    - Test end-to-end de creación de entradas
    - Test de manejo de errores y validaciones
    - _Requirements: 11.5_

- [ ] 11. Checkpoint - Validar integración completa
  - Ensure all tests pass, ask the user if questions arise.

- [-] 12. Configurar deployment y containerización
  - [x] 12.1 Crear Dockerfile
    - Dockerfile multi-stage para optimización
    - Configurar variables de entorno
    - _Requirements: 12.1_

  - [x] 12.2 Crear docker-compose para desarrollo
    - Incluir PostgreSQL, RabbitMQ, y aplicación
    - Configurar networking y volúmenes
    - _Requirements: 12.1_

  - [-] 12.3 Crear scripts de inicialización
    - Scripts para setup de base de datos
    - Scripts para configuración inicial
    - _Requirements: 12.1_

- [x] 13. Validación final y documentación
  - [x] 13.1 Ejecutar suite completa de tests
    - Verificar >90% cobertura de pruebas
    - _Requirements: 11.1_

  - [x] 13.2 Generar documentación técnica
    - README con instrucciones de setup
    - Documentación de API
    - Guía de desarrollo
    - _Requirements: 13.4_

  - [x] 13.3 Verificar cumplimiento de requerimientos
    - Checklist de todos los requerimientos implementados
    - Validación de arquitectura hexagonal
    - _Requirements: 8.1, 8.2, 8.3_

- [ ] 14. MEJORA CRÍTICA DE COBERTURA DE PRUEBAS (12% → 90%)
  - [ ] 14.1 Fase 1: Pruebas de Capa API (+25% cobertura)
    - [x] 14.1.1 Crear pruebas completas del controlador EntradasController
      - Test POST /api/entradas (creación exitosa y validación fallida)
      - Test GET /api/entradas/{id} (existente y no encontrada)
      - Test GET /api/entradas/usuario/{usuarioId}
      - Manejo de excepciones del controlador
      - _Requirements: 11.1, 13.2_
    
    - [x] 14.1.2 Crear pruebas de middleware críticos
      - GlobalExceptionHandlerMiddlewareTests (manejo de excepciones)
      - MetricsMiddlewareTests (recopilación de métricas)
      - CorrelationIdMiddlewareTests (correlation IDs)
      - _Requirements: 11.1, 12.5, 14.3_
    
    - [x] 14.1.3 Crear pruebas de health checks y validadores
      - EntradaServiceHealthCheckTests (health checks)
      - CrearEntradaRequestDtoValidatorTests (validación DTOs)
      - _Requirements: 11.1, 14.1_
    
    - [x] 14.1.4 Crear pruebas de configuración de aplicación
      - ProgramIntegrationTests (configuración de servicios y pipeline)
      - _Requirements: 11.1, 8.4_

  - [x] 14.2 Fase 2: Pruebas de Capa Aplicación (+30% cobertura)
    - [x] 14.2.1 Expandir pruebas de handlers existentes
      - Completar CrearEntradaCommandHandlerTests con todos los casos edge
      - Crear ObtenerEntradaQueryHandlerTests completo
      - Crear ObtenerEntradasPorUsuarioQueryHandlerTests completo
      - _Requirements: 11.2, 11.3_
    
    - [x] 14.2.2 Crear pruebas del consumer de RabbitMQ
      - PagoConfirmadoConsumerTests (procesamiento exitoso y errores)
      - Manejo de estados inválidos y entradas no encontradas
      - _Requirements: 11.1, 10.3_
    
    - [x] 14.2.3 Crear pruebas de validadores y mappers
      - CrearEntradaCommandValidatorTests (validaciones de negocio)
      - EntradaMapperTests (mapeo bidireccional)
      - _Requirements: 11.1, 9.4_

  - [ ] 14.3 Fase 3: Pruebas de Capa Infraestructura (+25% cobertura)
    - [x] 14.3.1 Expandir pruebas de repositorio y servicios HTTP
      - Completar RepositorioEntradasTests con CRUD completo
      - Expandir VerificadorEventosHttpTests con políticas de resiliencia
      - Expandir VerificadorAsientosHttpTests con circuit breaker
      - _Requirements: 11.1, 6.5_
    
    - [x] 14.3.2 Crear pruebas de persistencia y configuración
      - EntradasDbContextTests (configuración y migraciones)
      - EntradaConfigurationTests (mapeo de entidades)
      - UnitOfWorkTests (transacciones y rollback)
      - EntradasDbContextFactoryTests (factory pattern)
      - _Requirements: 11.1, 7.5_
    
    - [x] 14.3.3 Crear pruebas de métricas y servicios
      - EntradasMetricsTests (contadores y mediciones)
      - EntradasDbContextFactoryTests (factory pattern)
      - _Requirements: 11.1, 12.4_

  - [x] 14.4 Fase 4: Pruebas de Integración (+10% cobertura)
    - [x] 14.4.1 Crear pruebas de integración de base de datos
      - DatabaseIntegrationTests con TestContainers PostgreSQL
      - Operaciones CRUD reales y transacciones distribuidas
      - _Requirements: 11.5_
    
    - [x] 14.4.2 Crear pruebas de integración de RabbitMQ
      - RabbitMqIntegrationTests con TestContainers
      - Publicación y consumo de eventos reales
      - _Requirements: 11.5_
    
    - [x] 14.4.3 Crear pruebas end-to-end
      - EndToEndTests con flujo completo de creación y confirmación
      - Integración con servicios externos mockeados con WireMock
      - _Requirements: 11.5_

  - [ ] 14.5 Verificación de cobertura objetivo
    - Ejecutar suite completa con reporte de cobertura
    - Verificar >90% cobertura de líneas y >85% cobertura de branches
    - Generar reporte HTML de cobertura
    - _Requirements: 11.1_

- [ ] 15. Validación final y documentación actualizada
  - [ ] 15.1 Generar documentación técnica actualizada
    - README con instrucciones de setup y testing
    - Documentación de API actualizada
    - Guía de desarrollo y testing comprehensiva
    - _Requirements: 13.4_

  - [ ] 15.2 Verificar cumplimiento completo de requerimientos
    - Checklist de todos los requerimientos implementados
    - Validación de arquitectura hexagonal
    - Validación de cobertura de pruebas >90%
    - _Requirements: 8.1, 8.2, 8.3, 11.1_

## Notes

- Tasks marked with `*` are optional and can be skipped for faster MVP
- **PRIORIDAD CRÍTICA**: Task 14 (Mejora de Cobertura) es esencial para alcanzar el objetivo de 90% cobertura
- Each task references specific requirements for traceability  
- Checkpoints ensure incremental validation
- Property tests validate universal correctness properties
- Unit tests validate specific examples and edge cases
- Integration tests validate real component interactions
- All code must use Spanish nomenclature as specified
- **Target >90% code coverage** - actualmente en 12.7%, necesita incremento significativo
- **Estrategia de implementación**: Priorizar Fase 1 (API) y Fase 2 (Aplicación) para máximo impacto en cobertura
- **Herramientas adicionales necesarias**: TestContainers, WireMock, Bogus para generación de datos de prueba
- **Métricas de éxito por fase**: 37% (Fase 1), 67% (Fase 2), 92% (Fase 3), 95% (Fase 4)