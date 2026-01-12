# Documento de Requerimientos - Microservicio Entradas.API

## Introducción

El microservicio **Entradas.API** es responsable de la gestión completa del ciclo de vida de las entradas (tickets) para eventos, desde su creación hasta su uso final. Implementa Arquitectura Hexagonal Estricta, Domain-Driven Design (DDD) y utiliza nomenclatura 100% en español.

## Glosario

- **Sistema_Entradas**: El microservicio completo de gestión de entradas
- **Entrada**: Ticket digital que permite acceso a un evento específico
- **Verificador_Eventos**: Servicio externo que valida la existencia y disponibilidad de eventos
- **Verificador_Asientos**: Servicio externo que valida la disponibilidad de asientos
- **Generador_QR**: Componente que genera códigos QR únicos para las entradas
- **Procesador_Pagos**: Sistema externo que maneja la confirmación de pagos
- **Estado_Entrada**: Enumeración que define los posibles estados de una entrada
- **Orquestacion_Sincrona**: Patrón donde las validaciones externas se realizan de forma síncrona
- **Consumidor_Eventos**: Componente que escucha eventos asincrónicos de RabbitMQ

## Requerimientos

### Requerimiento 1: Gestión de Entidad Entrada

**User Story:** Como desarrollador del sistema, quiero definir la entidad Entrada con todas sus propiedades y comportamientos, para que represente correctamente un ticket digital en el dominio.

#### Acceptance Criteria

1. THE Sistema_Entradas SHALL define una entidad Entrada con propiedades Id (Guid), EventoId, UsuarioId, AsientoId (Nullable), Monto (decimal), CodigoQr (string), Estado (Enum), FechaCompra
2. THE Estado_Entrada SHALL support valores PendientePago, Pagada, Cancelada, Usada
3. WHEN una Entrada es creada, THE Sistema_Entradas SHALL assign estado inicial PendientePago
4. THE Sistema_Entradas SHALL ensure que todas las propiedades requeridas están presentes al crear una Entrada

### Requerimiento 2: Generación de Códigos QR

**User Story:** Como usuario del sistema, quiero que cada entrada tenga un código QR único, para que pueda ser identificada de manera inequívoca.

#### Acceptance Criteria

1. WHEN una Entrada es creada, THE Generador_QR SHALL generate un código único con formato "TICKET-{Guid}-{Random}"
2. THE Generador_QR SHALL ensure que cada código QR es único en el sistema
3. THE Sistema_Entradas SHALL store el código QR como string sin generar imagen
4. THE Generador_QR SHALL use componentes criptográficamente seguros para la generación aleatoria

### Requerimiento 3: Validación Externa Síncrona

**User Story:** Como sistema de entradas, quiero validar la existencia del evento y disponibilidad del asiento antes de crear una entrada, para garantizar la integridad de los datos.

#### Acceptance Criteria

1. WHEN se recibe CrearEntradaCommand, THE Sistema_Entradas SHALL communicate síncronamente con Verificador_Eventos via HTTP
2. WHEN se recibe CrearEntradaCommand, THE Sistema_Entradas SHALL communicate síncronamente con Verificador_Asientos via HTTP
3. IF Verificador_Eventos returns false, THEN THE Sistema_Entradas SHALL reject la creación y return error descriptivo
4. IF Verificador_Asientos returns false, THEN THE Sistema_Entradas SHALL reject la creación y return error descriptivo
5. THE Sistema_Entradas SHALL complete todas las validaciones antes de persistir la entrada

### Requerimiento 4: Creación de Entradas

**User Story:** Como usuario, quiero crear una entrada para un evento específico, para que pueda reservar mi participación.

#### Acceptance Criteria

1. WHEN todas las validaciones externas son exitosas, THE Sistema_Entradas SHALL create nueva Entrada en estado PendientePago
2. WHEN una Entrada es creada, THE Sistema_Entradas SHALL persist la entrada en base de datos PostgreSQL
3. WHEN una Entrada es persistida, THE Sistema_Entradas SHALL publish evento EntradaCreadaEvento via RabbitMQ
4. THE Sistema_Entradas SHALL return la Entrada creada con todos sus datos al cliente
5. IF cualquier paso falla, THEN THE Sistema_Entradas SHALL rollback la transacción completa

### Requerimiento 5: Confirmación de Pagos Asíncrona

**User Story:** Como sistema de entradas, quiero procesar confirmaciones de pago de manera asíncrona, para mantener el desacoplamiento con el sistema de pagos.

#### Acceptance Criteria

1. THE Sistema_Entradas SHALL implement PagoConfirmadoConsumer que escucha eventos de RabbitMQ
2. WHEN PagoConfirmadoConsumer receives evento de pago confirmado, THE Sistema_Entradas SHALL locate la entrada correspondiente
3. WHEN una entrada es localizada, THE Sistema_Entradas SHALL change estado de PendientePago a Pagada
4. WHEN el estado cambia a Pagada, THE Sistema_Entradas SHALL persist el cambio en base de datos
5. IF la entrada no existe, THEN THE Sistema_Entradas SHALL log error y continue procesando

### Requerimiento 6: Interfaces para Servicios Externos

**User Story:** Como desarrollador, quiero interfaces bien definidas para servicios externos, para facilitar testing y mantener bajo acoplamiento.

#### Acceptance Criteria

1. THE Sistema_Entradas SHALL define interface IVerificadorEventos en capa de dominio
2. THE Sistema_Entradas SHALL define interface IVerificadorAsientos en capa de dominio
3. THE Sistema_Entradas SHALL implement estas interfaces usando HttpClient en capa de infraestructura
4. THE Sistema_Entradas SHALL allow dependency injection de estas interfaces para testing
5. THE Sistema_Entradas SHALL handle timeouts y errores de red en las implementaciones HTTP

### Requerimiento 7: Persistencia con Entity Framework Core

**User Story:** Como sistema, quiero persistir las entradas en PostgreSQL usando Entity Framework Core con Code First, para garantizar consistencia y facilitar migraciones.

#### Acceptance Criteria

1. THE Sistema_Entradas SHALL use PostgreSQL como base de datos principal
2. THE Sistema_Entradas SHALL implement Entity Framework Core con enfoque Code First
3. THE Sistema_Entradas SHALL define configuraciones de entidad apropiadas para Entrada
4. THE Sistema_Entradas SHALL support migraciones automáticas de base de datos
5. THE Sistema_Entradas SHALL implement transacciones para operaciones críticas

### Requerimiento 8: Arquitectura Hexagonal Estricta

**User Story:** Como arquitecto de software, quiero que el sistema siga Arquitectura Hexagonal estricta, para mantener separación clara de responsabilidades y facilitar testing.

#### Acceptance Criteria

1. THE Sistema_Entradas SHALL organize código en capas: Dominio, Aplicacion, Infraestructura, API, Pruebas
2. THE Sistema_Entradas SHALL ensure que capa de Dominio no depende de capas externas
3. THE Sistema_Entradas SHALL implement interfaces en Dominio e implementaciones en Infraestructura
4. THE Sistema_Entradas SHALL use dependency injection para invertir dependencias
5. THE Sistema_Entradas SHALL maintain clear boundaries entre capas

### Requerimiento 9: Manejo de Comandos y Queries

**User Story:** Como desarrollador, quiero implementar CQRS con handlers específicos, para separar operaciones de lectura y escritura.

#### Acceptance Criteria

1. THE Sistema_Entradas SHALL implement CrearEntradaCommand con su correspondiente handler
2. THE Sistema_Entradas SHALL implement queries para consultar entradas existentes
3. THE Sistema_Entradas SHALL use MediatR para dispatch de comandos y queries
4. THE Sistema_Entradas SHALL validate comandos usando FluentValidation
5. THE Sistema_Entradas SHALL return DTOs apropiados desde los handlers

### Requerimiento 10: Integración con RabbitMQ

**User Story:** Como sistema distribuido, quiero integrarme con RabbitMQ para comunicación asíncrona, para mantener desacoplamiento entre microservicios.

#### Acceptance Criteria

1. THE Sistema_Entradas SHALL use MassTransit para integración con RabbitMQ
2. THE Sistema_Entradas SHALL publish EntradaCreadaEvento cuando una entrada es creada
3. THE Sistema_Entradas SHALL consume PagoConfirmadoEvento para actualizar estados
4. THE Sistema_Entradas SHALL handle errores de conexión y reintento automático
5. THE Sistema_Entradas SHALL use configuración externa para endpoints de RabbitMQ

### Requerimiento 11: Testing Comprehensivo

**User Story:** Como desarrollador, quiero tests comprehensivos con >90% cobertura, para garantizar la calidad y correctness del código.

#### Acceptance Criteria

1. THE Sistema_Entradas SHALL achieve >90% code coverage en tests unitarios
2. THE Sistema_Entradas SHALL implement tests para CrearEntradaHandler con mocks de servicios externos
3. THE Sistema_Entradas SHALL test escenarios de éxito y fallo en validaciones externas
4. THE Sistema_Entradas SHALL use xUnit, Moq, y FluentAssertions para testing
5. THE Sistema_Entradas SHALL include tests de integración para base de datos y RabbitMQ

### Requerimiento 12: Configuración y Logging

**User Story:** Como operador del sistema, quiero configuración externa y logging comprehensivo, para facilitar deployment y troubleshooting.

#### Acceptance Criteria

1. THE Sistema_Entradas SHALL use configuración externa para connection strings y endpoints
2. THE Sistema_Entradas SHALL implement structured logging usando Serilog
3. THE Sistema_Entradas SHALL log todas las operaciones críticas y errores
4. THE Sistema_Entradas SHALL support diferentes niveles de logging por ambiente
5. THE Sistema_Entradas SHALL include correlation IDs para tracing distribuido

### Requerimiento 13: API RESTful

**User Story:** Como cliente del sistema, quiero una API RESTful bien documentada, para poder integrarme fácilmente con el microservicio.

#### Acceptance Criteria

1. THE Sistema_Entradas SHALL expose endpoints RESTful para operaciones CRUD de entradas
2. THE Sistema_Entradas SHALL implement proper HTTP status codes para diferentes escenarios
3. THE Sistema_Entradas SHALL use DTOs para request/response en lugar de entidades de dominio
4. THE Sistema_Entradas SHALL include Swagger/OpenAPI documentation
5. THE Sistema_Entradas SHALL implement proper error handling y response formatting

### Requerimiento 14: Validación y Manejo de Errores

**User Story:** Como sistema robusto, quiero validación comprehensiva y manejo de errores, para proporcionar feedback claro y mantener estabilidad.

#### Acceptance Criteria

1. THE Sistema_Entradas SHALL validate todos los inputs usando FluentValidation
2. THE Sistema_Entradas SHALL return mensajes de error descriptivos y localizados
3. THE Sistema_Entradas SHALL handle excepciones de manera centralizada
4. THE Sistema_Entradas SHALL log errores con suficiente contexto para debugging
5. THE Sistema_Entradas SHALL implement circuit breaker para servicios externos