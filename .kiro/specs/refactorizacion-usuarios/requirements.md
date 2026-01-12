# Requirements Document - Refactorización Microservicio Usuarios

## Introduction

Este documento define los requisitos para refactorizar el microservicio de Usuarios, transformándolo desde su arquitectura actual (que no cumple con los estándares del proyecto) hacia una Arquitectura Hexagonal profesional con CQRS, siguiendo los mismos patrones y calidad de los microservicios Eventos, Asientos y Reportes.

## Glossary

- **Usuario**: Entidad que representa a un usuario del sistema con información personal y rol
- **Arquitectura_Hexagonal**: Patrón arquitectónico que separa Domain, Application, Infrastructure y API
- **CQRS**: Command Query Responsibility Segregation - separación de comandos (escritura) y queries (lectura)
- **Repository_Pattern**: Patrón que abstrae el acceso a datos
- **Value_Object**: Objeto inmutable que representa un concepto del dominio (ej: Correo, Telefono)
- **Aggregate_Root**: Entidad raíz que garantiza la consistencia del agregado
- **Command**: Operación que modifica el estado del sistema
- **Query**: Operación que lee datos sin modificar el estado
- **Handler**: Clase que procesa un Command o Query específico

## Requirements

### Requirement 1: Arquitectura Hexagonal

**User Story:** Como arquitecto de software, quiero que el microservicio Usuarios siga Arquitectura Hexagonal, para que tenga la misma calidad y mantenibilidad que Eventos y Asientos.

#### Acceptance Criteria

1. THE System SHALL organizar el código en cuatro capas: Dominio, Aplicacion, Infraestructura y API
2. THE Dominio SHALL contener solo lógica de negocio sin dependencias externas
3. THE Aplicacion SHALL contener Commands, Queries y sus Handlers
4. THE Infraestructura SHALL contener implementaciones de repositorios y servicios externos
5. THE API SHALL contener solo controllers y configuración de servicios
6. WHEN una capa necesita funcionalidad de otra, THE System SHALL usar interfaces definidas en capas internas
7. THE System SHALL seguir el principio de Dependency Inversion (dependencias apuntan hacia el dominio)

### Requirement 2: Implementación CQRS

**User Story:** Como desarrollador, quiero que el microservicio implemente CQRS, para separar claramente las operaciones de lectura y escritura.

#### Acceptance Criteria

1. THE System SHALL separar todas las operaciones en Commands (escritura) y Queries (lectura)
2. WHEN se crea un usuario, THE System SHALL usar AgregarUsuarioComando
3. WHEN se actualiza un usuario, THE System SHALL usar ActualizarUsuarioComando
4. WHEN se elimina un usuario, THE System SHALL usar EliminarUsuarioComando
5. WHEN se consulta un usuario, THE System SHALL usar ConsultarUsuarioQuery
6. WHEN se consultan múltiples usuarios, THE System SHALL usar ConsultarUsuariosQuery
7. THE System SHALL usar MediatR para despachar Commands y Queries
8. THE System SHALL tener un Handler específico para cada Command y Query

### Requirement 3: Modelo de Dominio Rico

**User Story:** Como desarrollador, quiero que el dominio tenga entidades ricas con lógica de negocio, para que el modelo refleje correctamente las reglas del negocio.

#### Acceptance Criteria

1. THE Usuario SHALL ser un Aggregate Root con métodos de negocio
2. THE Usuario SHALL validar sus invariantes en el constructor y métodos
3. THE Correo SHALL ser un Value Object inmutable con validación de formato
4. THE Telefono SHALL ser un Value Object inmutable con validación de formato
5. THE Direccion SHALL ser un Value Object inmutable
6. THE Usuario SHALL tener métodos como Actualizar(), Desactivar(), CambiarRol()
7. WHEN se crea un Usuario con datos inválidos, THE System SHALL lanzar una excepción de dominio
8. THE Usuario SHALL encapsular su estado (propiedades privadas con setters privados)

### Requirement 4: Repository Pattern

**User Story:** Como desarrollador, quiero que el acceso a datos use el patrón Repository, para abstraer la persistencia y facilitar testing.

#### Acceptance Criteria

1. THE System SHALL definir IRepositorioUsuarios en la capa de Dominio
2. THE System SHALL implementar RepositorioUsuarios en la capa de Infraestructura
3. THE IRepositorioUsuarios SHALL definir métodos: Agregar, Actualizar, Eliminar, ObtenerPorId, ObtenerTodos
4. THE RepositorioUsuarios SHALL usar Entity Framework Core para persistencia
5. THE RepositorioUsuarios SHALL mapear entre entidades de dominio y entidades de persistencia
6. WHEN se guarda un Usuario, THE Repository SHALL persistir todos sus Value Objects correctamente
7. WHEN se consulta un Usuario, THE Repository SHALL reconstruir la entidad de dominio completa

### Requirement 5: Gestión de Usuarios

**User Story:** Como administrador, quiero gestionar usuarios del sistema, para controlar quién tiene acceso y con qué permisos.

#### Acceptance Criteria

1. WHEN se agrega un usuario, THE System SHALL validar que el correo sea único
2. WHEN se agrega un usuario, THE System SHALL validar que el username sea único
3. WHEN se agrega un usuario, THE System SHALL asignar un rol válido (User, Admin, Organizator)
4. WHEN se actualiza un usuario, THE System SHALL validar que el usuario existe
5. WHEN se elimina un usuario, THE System SHALL realizar eliminación lógica (IsActive = false)
6. WHEN se consulta un usuario por ID, THE System SHALL retornar el usuario si existe y está activo
7. WHEN se consultan usuarios, THE System SHALL retornar solo usuarios activos por defecto

### Requirement 6: Integración con Keycloak

**User Story:** Como administrador, quiero que los usuarios se sincronicen con Keycloak, para que puedan autenticarse en el sistema.

#### Acceptance Criteria

1. WHEN se crea un usuario en el sistema, THE System SHALL crear el usuario en Keycloak
2. WHEN se crea un usuario en Keycloak, THE System SHALL asignar el rol correspondiente
3. WHEN se actualiza un usuario, THE System SHALL actualizar el usuario en Keycloak
4. WHEN se elimina un usuario, THE System SHALL desactivar el usuario en Keycloak
5. WHEN falla la creación en Keycloak, THE System SHALL revertir la creación en la base de datos
6. THE System SHALL usar un servicio de dominio para la integración con Keycloak
7. THE System SHALL manejar errores de Keycloak de forma apropiada

### Requirement 7: Validación de Datos

**User Story:** Como desarrollador, quiero que el sistema valide todos los datos de entrada, para garantizar la integridad de la información.

#### Acceptance Criteria

1. WHEN se recibe un comando, THE System SHALL validar el DTO usando FluentValidation
2. WHEN el correo es inválido, THE System SHALL retornar error de validación
3. WHEN el teléfono es inválido, THE System SHALL retornar error de validación
4. WHEN el username está vacío, THE System SHALL retornar error de validación
5. WHEN el nombre está vacío, THE System SHALL retornar error de validación
6. WHEN el rol es inválido, THE System SHALL retornar error de validación
7. THE System SHALL retornar mensajes de error descriptivos para cada validación

### Requirement 8: Manejo de Errores

**User Story:** Como desarrollador, quiero un manejo de errores consistente, para facilitar el debugging y proporcionar mensajes claros a los usuarios.

#### Acceptance Criteria

1. WHEN ocurre un error de dominio, THE System SHALL retornar HTTP 400 Bad Request
2. WHEN un usuario no existe, THE System SHALL retornar HTTP 404 Not Found
3. WHEN ocurre un error de validación, THE System SHALL retornar HTTP 400 con detalles de validación
4. WHEN ocurre un error de Keycloak, THE System SHALL retornar HTTP 502 Bad Gateway
5. WHEN ocurre un error inesperado, THE System SHALL retornar HTTP 500 Internal Server Error
6. THE System SHALL registrar todos los errores con Serilog
7. THE System SHALL usar un middleware global para manejo de excepciones

### Requirement 9: Testing Comprehensivo

**User Story:** Como desarrollador, quiero tests comprehensivos, para garantizar la calidad y correctness del código.

#### Acceptance Criteria

1. THE System SHALL tener tests unitarios para todos los Handlers
2. THE System SHALL tener tests unitarios para todas las entidades de dominio
3. THE System SHALL tener tests unitarios para todos los Value Objects
4. THE System SHALL tener tests de integración para los repositorios
5. THE System SHALL tener tests de integración para los endpoints de API
6. THE System SHALL alcanzar >90% de cobertura de código
7. THE System SHALL usar xUnit, Moq y FluentAssertions para testing

### Requirement 10: Persistencia con PostgreSQL

**User Story:** Como arquitecto de datos, quiero que el sistema use PostgreSQL, para mantener consistencia con los demás microservicios.

#### Acceptance Criteria

1. THE System SHALL usar PostgreSQL como base de datos
2. THE System SHALL usar Entity Framework Core para ORM
3. THE System SHALL aplicar migraciones automáticamente al iniciar
4. THE System SHALL usar la base de datos kairo_usuarios
5. THE System SHALL definir índices en campos de búsqueda frecuente (Username, Correo)
6. THE System SHALL usar transacciones para operaciones que modifican múltiples entidades
7. THE System SHALL mapear Value Objects a columnas de la tabla usando conversiones de EF Core

### Requirement 11: Logging y Observabilidad

**User Story:** Como SRE, quiero logging estructurado y observabilidad, para monitorear y diagnosticar problemas en producción.

#### Acceptance Criteria

1. THE System SHALL usar Serilog para logging estructurado
2. WHEN se ejecuta un comando, THE System SHALL registrar el comando y su resultado
3. WHEN se ejecuta una query, THE System SHALL registrar la query y el tiempo de ejecución
4. WHEN ocurre un error, THE System SHALL registrar el stack trace completo
5. THE System SHALL incluir correlation IDs en los logs
6. THE System SHALL registrar métricas de performance
7. THE System SHALL exponer endpoints de health check

### Requirement 12: Dockerización

**User Story:** Como DevOps engineer, quiero que el microservicio esté dockerizado, para facilitar el despliegue y la escalabilidad.

#### Acceptance Criteria

1. THE System SHALL tener un Dockerfile optimizado con multi-stage build
2. THE System SHALL exponer el puerto 8080
3. THE System SHALL conectarse a la red kairo-network
4. THE System SHALL leer configuración desde variables de entorno
5. THE System SHALL tener health checks configurados en Docker
6. THE System SHALL usar imágenes base oficiales de Microsoft
7. THE System SHALL minimizar el tamaño de la imagen final
