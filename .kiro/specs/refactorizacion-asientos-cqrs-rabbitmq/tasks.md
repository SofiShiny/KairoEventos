# Implementation Plan: Refactorización Asientos CQRS + RabbitMQ

## Overview

Este plan describe las tareas para refactorizar el microservicio de Asientos aplicando correctamente CQRS, reorganizando eventos de dominio e integrando RabbitMQ con MassTransit. La implementación sigue un enfoque incremental con validación en cada paso.

## Tasks

- [x] 1. Auditoría y corrección de violaciones CQRS
  - Identificar y documentar violaciones actuales del patrón CQRS
  - Corregir Commands que retornan entidades completas
  - Crear Queries para operaciones de lectura
  - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5_

- [x] 1.1 Corregir CrearMapaAsientosComando para retornar solo Guid
  - Modificar `CrearMapaAsientosComando` de `IRequest<MapaAsientos>` a `IRequest<Guid>`
  - Actualizar `CrearMapaAsientosComandoHandler` para retornar `mapa.Id` en lugar de `mapa`
  - Actualizar `MapasAsientosController.Crear()` para manejar solo el Guid
  - _Requirements: 1.1, 1.3_

- [x] 1.2 Crear Query para obtener mapa de asientos
  - Crear `ObtenerMapaAsientosQuery` como record con `MapaId`
  - Crear DTOs inmutables: `MapaAsientosDto`, `CategoriaDto`, `AsientoDto`
  - Crear `ObtenerMapaAsientosQueryHandler` con lógica de transformación
  - _Requirements: 1.2, 4.1, 4.2, 4.5_

- [x] 1.3 Refactorizar MapasAsientosController para usar Query
  - Remover inyección de `IRepositorioMapaAsientos`
  - Actualizar método `Obtener()` para usar `ObtenerMapaAsientosQuery`
  - Verificar que el controlador solo ejecuta `_mediator.Send()`
  - _Requirements: 1.4, 4.3, 4.4_

- [x] 1.4 Limpiar controladores para ser "thin"
  - Remover construcción de objetos anónimos en `AsientosController.Crear()`
  - Remover construcción de objetos anónimos en `AsientosController.Reservar()`
  - Remover construcción de objetos anónimos en `AsientosController.Liberar()`
  - Asegurar que solo retornan IDs o Ok() vacío
  - _Requirements: 1.5, 8.1, 8.2, 8.3, 8.4_

- [x] 1.5 Escribir tests unitarios para validar CQRS
  - Test: Commands retornan solo Guid o Unit
  - Test: Queries retornan DTOs inmutables
  - Test: Controllers no tienen lógica de negocio
  - Tests existentes actualizados para inyectar IPublishEndpoint
  - _Requirements: 1.1, 1.2, 1.4_

- [x] 2. Reorganización de eventos de dominio
  - Separar cada evento en su propio archivo
  - Establecer namespace consistente
  - Eliminar archivo consolidado
  - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5_

- [x] 2.1 Crear archivo MapaAsientosCreadoEventoDominio.cs
  - Crear archivo en `Asientos.Dominio/EventosDominio/`
  - Definir clase con propiedades `MapaId` y `EventoId`
  - Usar namespace `Asientos.Dominio.EventosDominio`
  - Heredar de `BloquesConstruccion.Dominio.EventoDominio`
  - _Requirements: 2.1, 2.2, 2.3, 9.1_

- [x] 2.2 Crear archivo CategoriaAgregadaEventoDominio.cs
  - Crear archivo en `Asientos.Dominio/EventosDominio/`
  - Definir clase con propiedades `MapaId` y `NombreCategoria`
  - Usar namespace `Asientos.Dominio.EventosDominio`
  - Heredar de `BloquesConstruccion.Dominio.EventoDominio`
  - _Requirements: 2.1, 2.2, 2.3, 9.2_

- [x] 2.3 Crear archivo AsientoAgregadoEventoDominio.cs
  - Crear archivo en `Asientos.Dominio/EventosDominio/`
  - Definir clase con propiedades `MapaId`, `Fila`, `Numero`, `Categoria`
  - Usar namespace `Asientos.Dominio.EventosDominio`
  - Heredar de `BloquesConstruccion.Dominio.EventoDominio`
  - _Requirements: 2.1, 2.2, 2.3, 9.3_

- [x] 2.4 Crear archivo AsientoReservadoEventoDominio.cs
  - Crear archivo en `Asientos.Dominio/EventosDominio/`
  - Definir clase con propiedades `MapaId`, `Fila`, `Numero`
  - Usar namespace `Asientos.Dominio.EventosDominio`
  - Heredar de `BloquesConstruccion.Dominio.EventoDominio`
  - _Requirements: 2.1, 2.2, 2.3, 9.4_

- [x] 2.5 Crear archivo AsientoLiberadoEventoDominio.cs
  - Crear archivo en `Asientos.Dominio/EventosDominio/`
  - Definir clase con propiedades `MapaId`, `Fila`, `Numero`
  - Usar namespace `Asientos.Dominio.EventosDominio`
  - Heredar de `BloquesConstruccion.Dominio.EventoDominio`
  - _Requirements: 2.1, 2.2, 2.3, 9.5_

- [x] 2.6 Eliminar archivo consolidado DomainEvents.cs
  - Verificar que todos los eventos están en archivos separados
  - Actualizar imports en handlers si es necesario
  - Eliminar `Asientos.Dominio/EventosDominio/DomainEvents.cs`
  - _Requirements: 2.5_

- [x] 2.7 Escribir tests para estructura de eventos
  - Test: Verificar que existen exactamente 5 archivos de eventos
  - Test: Verificar que no existe DomainEvents.cs
  - Test: Verificar que todos los eventos heredan de EventoDominio
  - _Requirements: 2.3, 2.4, 2.5_

- [x] 3. Checkpoint - Compilación y verificación de estructura
  - Compilar el proyecto y verificar que no hay errores
  - Verificar que todos los eventos están en archivos separados
  - Verificar que los controladores son "thin"
  - Preguntar al usuario si hay dudas antes de continuar

- [x] 4. Instalación y configuración de MassTransit
  - Instalar paquete MassTransit.RabbitMQ
  - Configurar MassTransit en Program.cs
  - Crear archivos de configuración
  - _Requirements: 3.1, 3.2, 7.1, 7.2, 7.3, 7.5_

- [x] 4.1 Instalar paquete MassTransit.RabbitMQ
  - Agregar `<PackageReference Include="MassTransit.RabbitMQ" Version="8.1.3" />` a `Asientos.Aplicacion.csproj`
  - Agregar `<PackageReference Include="MassTransit.RabbitMQ" Version="8.1.3" />` a `Asientos.API.csproj`
  - Ejecutar `dotnet restore`
  - _Requirements: 3.2_

- [x] 4.2 Configurar MassTransit en Program.cs
  - Agregar `using MassTransit;`
  - Leer configuración: `var rabbitMqHost = builder.Configuration["RabbitMq:Host"] ?? "localhost";`
  - Configurar MassTransit con RabbitMQ usando credenciales guest/guest
  - Usar `cfg.ConfigureEndpoints(context)` para auto-descubrimiento
  - _Requirements: 3.1, 7.2, 7.3, 7.4_

- [x] 4.3 Crear archivos de configuración
  - Crear `appsettings.json` con sección `RabbitMq: { Host: "localhost" }`
  - Crear `appsettings.Development.json` con logging de MassTransit en Debug
  - _Requirements: 7.1, 7.5_

- [x] 4.4 Actualizar Health Check para incluir RabbitMQ
  - Modificar endpoint `/health` para incluir `rabbitmq: rabbitMqHost`
  - Verificar que retorna `{ status, db, rabbitmq }`
  - _Requirements: 12.1, 12.2, 12.3, 12.4_

- [x] 4.5 Escribir tests para configuración de MassTransit
  - Test: Configuración con host presente usa ese valor
  - Test: Configuración ausente usa "localhost" como fallback
  - Test: Health check incluye información de RabbitMQ
  - _Requirements: 3.1, 7.2, 12.2, 12.3, 12.4_

- [x] 5. Integración de publicación de eventos en handlers
  - Actualizar todos los handlers para publicar eventos
  - Seguir patrón: Persistir → Publicar
  - Inyectar IPublishEndpoint
  - _Requirements: 3.3, 3.4, 3.5, 6.1, 6.2, 6.3, 6.4, 6.5, 6.6_

- [x] 5.1 Actualizar CrearMapaAsientosComandoHandler
  - Inyectar `IPublishEndpoint` en el constructor
  - Después de `_repo.AgregarAsync()`, publicar `MapaAsientosCreadoEventoDominio`
  - Pasar `cancellationToken` al método `Publish()`
  - _Requirements: 3.3, 3.4, 6.1, 6.6_

- [x] 5.2 Actualizar AgregarAsientoComandoHandler
  - Inyectar `IPublishEndpoint` en el constructor
  - Después de `_repo.AgregarAsientoAsync()`, publicar `AsientoAgregadoEventoDominio`
  - Pasar `cancellationToken` al método `Publish()`
  - _Requirements: 3.3, 3.4, 6.2, 6.6_

- [x] 5.3 Actualizar AgregarCategoriaComandoHandler
  - Inyectar `IPublishEndpoint` en el constructor
  - Después de `_repo.ActualizarAsync()`, publicar `CategoriaAgregadaEventoDominio`
  - Pasar `cancellationToken` al método `Publish()`
  - _Requirements: 3.3, 3.4, 6.3, 6.6_

- [x] 5.4 Actualizar ReservarAsientoComandoHandler
  - Inyectar `IPublishEndpoint` en el constructor
  - Después de `_repo.ActualizarAsync()`, publicar `AsientoReservadoEventoDominio`
  - Pasar `cancellationToken` al método `Publish()`
  - _Requirements: 3.3, 3.4, 6.4, 6.6_

- [x] 5.5 Actualizar LiberarAsientoComandoHandler
  - Inyectar `IPublishEndpoint` en el constructor
  - Después de `_repo.ActualizarAsync()`, publicar `AsientoLiberadoEventoDominio`
  - Pasar `cancellationToken` al método `Publish()`
  - _Requirements: 3.3, 3.4, 6.5, 6.6_

- [x] 5.6 Escribir tests unitarios para publicación de eventos
  - Test: CrearMapaAsientosComandoHandler publica MapaAsientosCreadoEventoDominio
  - Test: AgregarAsientoComandoHandler publica AsientoAgregadoEventoDominio
  - Test: AgregarCategoriaComandoHandler publica CategoriaAgregadaEventoDominio
  - Test: ReservarAsientoComandoHandler publica AsientoReservadoEventoDominio
  - Test: LiberarAsientoComandoHandler publica AsientoLiberadoEventoDominio
  - Tests existentes actualizados con mocks de IPublishEndpoint
  - _Requirements: 6.1, 6.2, 6.3, 6.4, 6.5_

- [x] 5.7 Escribir property test para orden de operaciones
  - **Property 5: Handlers publican después de persistir**
  - **Validates: Requirements 3.3, 3.4**
  - Usar mocks para verificar que Save se llama antes que Publish
  - _Requirements: 3.3, 3.4_

- [x] 6. Checkpoint - Verificación de integración RabbitMQ
  - Compilar el proyecto y verificar que no hay errores
  - Levantar RabbitMQ con Docker
  - Ejecutar la API y verificar conexión a RabbitMQ
  - Ejecutar operación y verificar evento en RabbitMQ Management
  - Preguntar al usuario si hay dudas antes de continuar

- [x] 7. Verificación de inmutabilidad
  - Verificar que Commands, Queries y DTOs son records
  - Verificar propiedades con init setters
  - _Requirements: 5.1, 5.2, 5.3, 5.4_

- [x] 7.1 Escribir property tests para inmutabilidad
  - **Property 6: Commands son records inmutables**
  - **Validates: Requirements 5.1, 5.4**
  - **Property 7: Queries son records inmutables**
  - **Validates: Requirements 5.2, 5.4**
  - **Property 8: DTOs son records inmutables**
  - **Validates: Requirements 5.3, 5.4**
  - Usar reflection para verificar que son records con init setters
  - _Requirements: 5.1, 5.2, 5.3, 5.4_

- [x] 8. Verificación de propiedades de eventos
  - Verificar que eventos contienen propiedades requeridas
  - Verificar que IdAgregado se establece correctamente
  - _Requirements: 9.1, 9.2, 9.3, 9.4, 9.5, 9.6_

- [x] 8.1 Escribir property tests para estructura de eventos
  - **Property 12: Eventos contienen propiedades requeridas**
  - **Validates: Requirements 9.1, 9.2, 9.3, 9.4, 9.5**
  - **Property 13: IdAgregado igual a MapaId**
  - **Validates: Requirements 9.6**
  - Generar eventos aleatorios y verificar propiedades
  - _Requirements: 9.1, 9.2, 9.3, 9.4, 9.5, 9.6_

- [ ] 9. Documentación completa
  - Crear documento técnico de refactorización
  - Crear resumen ejecutivo
  - Actualizar README del microservicio
  - _Requirements: 11.1, 11.2, 11.3, 11.4, 11.5, 11.6_

- [x] 9.1 Crear documento técnico REFACTORIZACION-CQRS-RABBITMQ.md
  - Documentar errores CQRS encontrados y corregidos
  - Documentar estructura de eventos reorganizada
  - Documentar integración con RabbitMQ
  - Incluir ejemplos de código y diagramas
  - _Requirements: 11.1, 11.4, 11.5_

- [x] 9.2 Crear resumen ejecutivo RESUMEN-EJECUTIVO-REFACTORIZACION.md
  - Resumir cambios principales
  - Incluir métricas de refactorización
  - Documentar estado final del sistema
  - _Requirements: 11.2_

- [x] 9.3 Actualizar README.md del microservicio
  - Documentar arquitectura CQRS
  - Documentar eventos publicados
  - Incluir instrucciones de configuración de RabbitMQ
  - Documentar endpoints de API
  - Explicar flujo de eventos
  - _Requirements: 11.3, 11.6_

- [x] 10. Compilación final y verificación
  - Compilar todo el proyecto
  - Verificar tiempo de compilación
  - Verificar que se generan todos los DLLs
  - _Requirements: 10.1, 10.2, 10.3, 10.4, 10.5, 10.6_

- [x] 10.1 Escribir test de compilación
  - Test: Sistema compila sin errores
  - Test: Compilación genera Asientos.Dominio.dll
  - Test: Compilación genera Asientos.Aplicacion.dll
  - Test: Compilación genera Asientos.Infraestructura.dll
  - Test: Compilación genera Asientos.API.dll
  - Test: Compilación completa en menos de 10 segundos
  - _Requirements: 10.1, 10.2, 10.3, 10.4, 10.5, 10.6_

- [x] 11. Tests de integración con RabbitMQ
  - Configurar Testcontainers para RabbitMQ
  - Escribir tests end-to-end
  - Verificar publicación y consumo de eventos
  - _Requirements: 3.3, 6.1, 6.2, 6.3, 6.4, 6.5_

- [x] 11.1 Configurar Testcontainers para RabbitMQ
  - Instalar paquete Testcontainers.RabbitMQ
  - Crear fixture para levantar RabbitMQ en tests
  - _Requirements: 3.3_

- [x] 11.2 Escribir tests de integración para publicación de eventos
  - Test: Crear mapa publica evento a RabbitMQ
  - Test: Agregar asiento publica evento a RabbitMQ
  - Test: Reservar asiento publica evento a RabbitMQ
  - Test: Liberar asiento publica evento a RabbitMQ
  - Verificar que eventos llegan a RabbitMQ con datos correctos
  - _Requirements: 3.3, 6.1, 6.2, 6.4, 6.5_

- [x] 12. Checkpoint final - Verificación completa
  - Ejecutar todos los tests (unitarios, property-based, integración)
  - Verificar que la documentación está completa
  - Verificar que el sistema compila sin errores
  - Verificar que RabbitMQ recibe eventos correctamente
  - Revisar checklist de requirements completados

## Notes

- Todas las tareas son requeridas para un enfoque comprehensivo con testing completo
- Cada tarea referencia los requirements específicos que valida
- Los checkpoints permiten validación incremental y feedback del usuario
- Los property tests usan FsCheck con mínimo 100 iteraciones
- Los tests de integración usan Testcontainers para RabbitMQ real
