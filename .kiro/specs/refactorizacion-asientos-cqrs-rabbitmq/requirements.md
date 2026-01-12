# Requirements Document

## Introduction

Este documento especifica los requerimientos para la refactorización del microservicio de Asientos, aplicando correctamente el patrón CQRS, reorganizando eventos de dominio e integrando RabbitMQ con MassTransit para comunicación asíncrona entre microservicios.

## Glossary

- **System**: Microservicio de Asientos
- **CQRS**: Command Query Responsibility Segregation - patrón que separa operaciones de lectura y escritura
- **Command**: Operación de escritura que modifica el estado del sistema
- **Query**: Operación de lectura que no modifica el estado del sistema
- **Handler**: Componente que procesa Commands o Queries usando MediatR
- **Event**: Evento de dominio que representa algo que ha ocurrido en el sistema
- **RabbitMQ**: Message broker para comunicación asíncrona
- **MassTransit**: Framework de abstracción sobre RabbitMQ
- **Controller**: Componente de API que recibe requests HTTP
- **DTO**: Data Transfer Object - objeto inmutable para transferir datos
- **Repository**: Componente que abstrae el acceso a datos
- **Aggregate**: Entidad raíz de dominio que agrupa entidades relacionadas

## Requirements

### Requirement 1: Corrección de Violaciones CQRS

**User Story:** Como arquitecto de software, quiero que el sistema implemente correctamente el patrón CQRS, para que haya separación estricta entre operaciones de lectura y escritura.

#### Acceptance Criteria

1. WHEN THE System ejecuta un Command THEN THE System SHALL retornar solo un Guid o Unit
2. WHEN THE System ejecuta una Query THEN THE System SHALL retornar un DTO inmutable
3. THE System SHALL NOT retornar entidades de dominio completas desde Commands
4. WHEN un Controller recibe un request THEN THE Controller SHALL delegar toda la lógica a MediatR
5. THE Controller SHALL NOT contener lógica de negocio ni construcción manual de ViewModels

### Requirement 2: Reorganización de Eventos de Dominio

**User Story:** Como desarrollador, quiero que cada evento de dominio esté en su propio archivo, para que el código sea más mantenible y organizado.

#### Acceptance Criteria

1. WHEN THE System define un evento de dominio THEN THE System SHALL crear un archivo separado para ese evento
2. THE System SHALL usar el namespace consistente "Asientos.Dominio.EventosDominio"
3. WHEN un evento es creado THEN THE Event SHALL heredar de EventoDominio base
4. THE System SHALL tener exactamente 5 archivos de eventos: MapaAsientosCreadoEventoDominio, CategoriaAgregadaEventoDominio, AsientoAgregadoEventoDominio, AsientoReservadoEventoDominio, AsientoLiberadoEventoDominio
5. WHEN todos los eventos están separados THEN THE System SHALL eliminar el archivo consolidado DomainEvents.cs

### Requirement 3: Integración con RabbitMQ

**User Story:** Como arquitecto de sistemas, quiero que el microservicio publique eventos a RabbitMQ, para que otros microservicios puedan reaccionar a cambios de estado.

#### Acceptance Criteria

1. WHEN THE System configura MassTransit THEN THE System SHALL leer el host de configuración con fallback a "localhost"
2. THE System SHALL instalar el paquete MassTransit.RabbitMQ versión 8.1.3
3. WHEN un Handler persiste cambios THEN THE Handler SHALL publicar el evento correspondiente a RabbitMQ
4. THE System SHALL seguir el patrón: primero persistir, luego publicar evento
5. WHEN THE System publica un evento THEN THE System SHALL usar IPublishEndpoint de MassTransit

### Requirement 4: Separación de Queries

**User Story:** Como desarrollador, quiero que las operaciones de lectura usen Queries de MediatR, para que no haya acceso directo a repositorios desde controladores.

#### Acceptance Criteria

1. WHEN THE System necesita leer datos THEN THE System SHALL crear una Query con su Handler correspondiente
2. THE Query SHALL retornar DTOs inmutables definidos como records
3. WHEN un Controller necesita datos THEN THE Controller SHALL ejecutar una Query via MediatR
4. THE Controller SHALL NOT inyectar repositorios directamente
5. THE Query Handler SHALL encapsular toda la lógica de transformación de entidades a DTOs

### Requirement 5: Inmutabilidad de Commands y Queries

**User Story:** Como desarrollador, quiero que Commands y Queries sean inmutables, para prevenir modificaciones accidentales y mejorar la seguridad del código.

#### Acceptance Criteria

1. THE System SHALL definir todos los Commands como records
2. THE System SHALL definir todas las Queries como records
3. THE System SHALL definir todos los DTOs como records
4. WHEN un Command o Query es creado THEN THE System SHALL usar propiedades con init setters
5. THE System SHALL NOT permitir modificación de propiedades después de la construcción

### Requirement 6: Publicación de Eventos en Handlers

**User Story:** Como desarrollador, quiero que cada Handler publique eventos después de persistir, para mantener consistencia entre la base de datos y el message broker.

#### Acceptance Criteria

1. WHEN CrearMapaAsientosComandoHandler persiste un mapa THEN THE Handler SHALL publicar MapaAsientosCreadoEventoDominio
2. WHEN AgregarAsientoComandoHandler persiste un asiento THEN THE Handler SHALL publicar AsientoAgregadoEventoDominio
3. WHEN AgregarCategoriaComandoHandler persiste una categoría THEN THE Handler SHALL publicar CategoriaAgregadaEventoDominio
4. WHEN ReservarAsientoComandoHandler reserva un asiento THEN THE Handler SHALL publicar AsientoReservadoEventoDominio
5. WHEN LiberarAsientoComandoHandler libera un asiento THEN THE Handler SHALL publicar AsientoLiberadoEventoDominio
6. WHEN un Handler publica un evento THEN THE Handler SHALL pasar el CancellationToken al método Publish

### Requirement 7: Configuración de MassTransit

**User Story:** Como DevOps, quiero que la configuración de RabbitMQ sea flexible, para poder cambiar el host según el ambiente sin recompilar.

#### Acceptance Criteria

1. THE System SHALL leer la configuración de RabbitMQ desde appsettings.json
2. WHEN la configuración no existe THEN THE System SHALL usar "localhost" como valor por defecto
3. THE System SHALL configurar credenciales guest/guest para desarrollo
4. THE System SHALL usar ConfigureEndpoints para auto-descubrimiento de consumers
5. THE System SHALL crear archivos appsettings.json y appsettings.Development.json con la sección RabbitMq

### Requirement 8: Controladores Thin

**User Story:** Como arquitecto de software, quiero que los controladores sean delgados, para que toda la lógica esté en la capa de aplicación.

#### Acceptance Criteria

1. WHEN un Controller recibe un request THEN THE Controller SHALL solo ejecutar _mediator.Send()
2. THE Controller SHALL NOT construir objetos anónimos con datos de negocio
3. WHEN un Command retorna un Guid THEN THE Controller SHALL retornar solo ese Guid
4. WHEN un Command retorna Unit THEN THE Controller SHALL retornar Ok() sin datos adicionales
5. THE Controller SHALL NOT contener validaciones de negocio manuales

### Requirement 9: Estructura de Eventos de Dominio

**User Story:** Como desarrollador, quiero que los eventos de dominio tengan una estructura consistente, para facilitar su consumo por otros microservicios.

#### Acceptance Criteria

1. WHEN MapaAsientosCreadoEventoDominio es creado THEN THE Event SHALL contener MapaId y EventoId
2. WHEN CategoriaAgregadaEventoDominio es creado THEN THE Event SHALL contener MapaId y NombreCategoria
3. WHEN AsientoAgregadoEventoDominio es creado THEN THE Event SHALL contener MapaId, Fila, Numero y Categoria
4. WHEN AsientoReservadoEventoDominio es creado THEN THE Event SHALL contener MapaId, Fila y Numero
5. WHEN AsientoLiberadoEventoDominio es creado THEN THE Event SHALL contener MapaId, Fila y Numero
6. WHEN un evento es creado THEN THE Event SHALL establecer IdAgregado con el MapaId

### Requirement 10: Compilación y Verificación

**User Story:** Como desarrollador, quiero que el código compile sin errores después de la refactorización, para asegurar que todos los cambios son correctos.

#### Acceptance Criteria

1. WHEN THE System es compilado THEN THE System SHALL compilar sin errores
2. WHEN THE System es compilado THEN THE System SHALL generar Asientos.Dominio.dll
3. WHEN THE System es compilado THEN THE System SHALL generar Asientos.Aplicacion.dll
4. WHEN THE System es compilado THEN THE System SHALL generar Asientos.Infraestructura.dll
5. WHEN THE System es compilado THEN THE System SHALL generar Asientos.API.dll
6. THE System SHALL completar la compilación en menos de 10 segundos

### Requirement 11: Documentación

**User Story:** Como desarrollador nuevo en el proyecto, quiero documentación completa de la refactorización, para entender los cambios realizados y cómo usar el sistema.

#### Acceptance Criteria

1. THE System SHALL incluir un documento técnico completo de refactorización
2. THE System SHALL incluir un resumen ejecutivo de cambios
3. THE System SHALL incluir un README actualizado con instrucciones de uso
4. WHEN la documentación es creada THEN THE Documentation SHALL incluir ejemplos de código
5. WHEN la documentación es creada THEN THE Documentation SHALL incluir diagramas de arquitectura
6. THE Documentation SHALL explicar el flujo de eventos desde Controller hasta RabbitMQ

### Requirement 12: Health Check

**User Story:** Como operador de sistemas, quiero un endpoint de health check que incluya el estado de RabbitMQ, para monitorear la salud del servicio.

#### Acceptance Criteria

1. THE System SHALL exponer un endpoint /health
2. WHEN /health es llamado THEN THE System SHALL retornar el estado del servicio
3. WHEN /health es llamado THEN THE System SHALL incluir el tipo de base de datos (postgres o in-memory)
4. WHEN /health es llamado THEN THE System SHALL incluir el host de RabbitMQ configurado
5. THE Health Check SHALL retornar HTTP 200 cuando el servicio está saludable
