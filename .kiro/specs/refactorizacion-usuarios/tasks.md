# Implementation Plan: Refactorización Microservicio Usuarios

## Overview

Este plan describe las tareas para refactorizar el microservicio de Usuarios desde su arquitectura actual (que no cumple estándares) hacia una Arquitectura Hexagonal profesional con CQRS, siguiendo los mismos patrones y calidad de los microservicios Eventos, Asientos y Reportes. La implementación sigue un enfoque incremental con validación en cada paso.

## Tasks

- [x] 1. Preparación y estructura inicial del proyecto
  - Crear estructura de carpetas para Arquitectura Hexagonal
  - Configurar proyectos .NET 8
  - Configurar dependencias entre capas
  - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5, 1.6, 1.7_

- [x] 1.1 Crear estructura de carpetas
  - Crear `Usuarios/src/Usuarios.Dominio/`
  - Crear `Usuarios/src/Usuarios.Aplicacion/`
  - Crear `Usuarios/src/Usuarios.Infraestructura/`
  - Crear `Usuarios/src/Usuarios.API/`
  - Crear `Usuarios/src/Usuarios.Pruebas/`
  - _Requirements: 1.1_

- [x] 1.2 Crear proyectos .NET
  - Crear `Usuarios.Dominio.csproj` (classlib, net8.0)
  - Crear `Usuarios.Aplicacion.csproj` (classlib, net8.0)
  - Crear `Usuarios.Infraestructura.csproj` (classlib, net8.0)
  - Crear `Usuarios.API.csproj` (web, net8.0)
  - Crear `Usuarios.Pruebas.csproj` (xunit, net8.0)
  - _Requirements: 1.1_

- [x] 1.3 Configurar referencias entre proyectos
  - Aplicacion referencia Dominio
  - Infraestructura referencia Dominio
  - API referencia Aplicacion e Infraestructura
  - Pruebas referencia todos los proyectos
  - _Requirements: 1.6, 1.7_

- [x] 1.4 Instalar paquetes NuGet en Dominio
  - No requiere paquetes externos (solo lógica de negocio)
  - _Requirements: 1.2_

- [x] 1.5 Instalar paquetes NuGet en Aplicacion
  - MediatR (12.2.0)
  - FluentValidation (11.9.0)
  - FluentValidation.DependencyInjectionExtensions (11.9.0)
  - Microsoft.Extensions.Logging.Abstractions
  - _Requirements: 2.7, 7.1_

- [x] 1.6 Instalar paquetes NuGet en Infraestructura
  - Microsoft.EntityFrameworkCore (8.0.0)
  - Microsoft.EntityFrameworkCore.Design (8.0.0)
  - Npgsql.EntityFrameworkCore.PostgreSQL (8.0.0)
  - Microsoft.Extensions.Http (10.0.1)
  - _Requirements: 10.2_

- [x] 1.7 Instalar paquetes NuGet en API
  - Serilog.AspNetCore (8.0.0)
  - Serilog.Sinks.Console (5.0.0)
  - Swashbuckle.AspNetCore (6.5.0)
  - _Requirements: 11.1_

- [x] 1.8 Instalar paquetes NuGet en Pruebas
  - xUnit (2.6.6)
  - xUnit.runner.visualstudio (2.5.6)
  - Moq (4.20.0)
  - FluentAssertions (6.12.0)
  - Microsoft.EntityFrameworkCore.InMemory (8.0.0)
  - Testcontainers.PostgreSql (3.6.0)
  - Microsoft.AspNetCore.Mvc.Testing (8.0.0)
  - Coverlet.collector (6.0.0)
  - _Requirements: 9.1, 9.2, 9.3, 9.4, 9.5, 9.7_

- [x] 2. Implementación de la capa de Dominio
  - Crear entidad Usuario (Aggregate Root)
  - Crear Value Objects (Correo, Telefono, Direccion)
  - Crear enum Rol
  - Crear interfaces de repositorio
  - Crear excepciones de dominio
  - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5, 3.6, 3.7, 4.1, 4.2_

- [x] 2.1 Crear enum Rol
  - Crear `Usuarios.Dominio/Enums/Rol.cs`
  - Definir valores: User = 1, Admin = 2, Organizator = 3
  - _Requirements: 5.3_

- [x] 2.2 Crear Value Object Correo
  - Crear `Usuarios.Dominio/ObjetosValor/Correo.cs`
  - Implementar como record inmutable
  - Validar formato de email en factory method Crear()
  - Normalizar a lowercase
  - _Requirements: 3.3, 7.2_

- [x] 2.3 Crear Value Object Telefono
  - Crear `Usuarios.Dominio/ObjetosValor/Telefono.cs`
  - Implementar como record inmutable
  - Validar longitud (7-15 dígitos) en factory method Crear()
  - Limpiar caracteres no numéricos
  - _Requirements: 3.4, 7.3_

- [x] 2.4 Crear Value Object Direccion
  - Crear `Usuarios.Dominio/ObjetosValor/Direccion.cs`
  - Implementar como record inmutable
  - Validar longitud mínima (5 caracteres) en factory method Crear()
  - _Requirements: 3.5_

- [x] 2.5 Crear entidad Usuario (Aggregate Root)
  - Crear `Usuarios.Dominio/Entidades/Usuario.cs`
  - Propiedades: Id, Username, Nombre, Correo, Telefono, Direccion, Rol, EstaActivo, FechaCreacion, FechaActualizacion
  - Factory method estático Crear() con validaciones
  - Métodos de negocio: Actualizar(), CambiarRol(), Desactivar(), Reactivar()
  - Constructor privado para EF Core
  - _Requirements: 3.1, 3.2, 3.6, 3.7, 5.5_

- [x] 2.6 Crear excepciones de dominio
  - Crear `Usuarios.Dominio/Excepciones/UsuarioNoEncontradoException.cs`
  - Crear `Usuarios.Dominio/Excepciones/CorreoDuplicadoException.cs`
  - Crear `Usuarios.Dominio/Excepciones/UsernameDuplicadoException.cs`
  - _Requirements: 8.1, 8.2_

- [x] 2.7 Crear interfaz IRepositorioUsuarios
  - Crear `Usuarios.Dominio/Repositorios/IRepositorioUsuarios.cs`
  - Métodos: ObtenerPorIdAsync, ObtenerPorUsernameAsync, ObtenerPorCorreoAsync
  - Métodos: ObtenerTodosAsync, ObtenerActivosAsync
  - Métodos: AgregarAsync, ActualizarAsync
  - Métodos: ExisteUsernameAsync, ExisteCorreoAsync
  - _Requirements: 4.1, 4.3_

- [x] 2.8 Crear interfaz IServicioKeycloak
  - Crear `Usuarios.Dominio/Servicios/IServicioKeycloak.cs`
  - Métodos: CrearUsuarioAsync, ActualizarUsuarioAsync, DesactivarUsuarioAsync, AsignarRolAsync
  - _Requirements: 6.1, 6.2, 6.3, 6.4, 6.6_

- [x] 3. Tests unitarios de la capa de Dominio
  - Tests de Value Objects
  - Tests de entidad Usuario
  - Tests de validaciones
  - _Requirements: 9.2_

- [x] 3.1 Tests de Value Object Correo
  - Test: Crear correo válido normaliza a lowercase
  - Test: Crear correo inválido lanza excepción
  - Test: Correo vacío lanza excepción
  - Test: Correos con mismo valor son iguales (record equality)
  - _Requirements: 3.3, 7.2, 9.2_

- [x] 3.2 Tests de Value Object Telefono
  - Test: Crear teléfono válido limpia caracteres especiales
  - Test: Teléfono con menos de 7 dígitos lanza excepción
  - Test: Teléfono con más de 15 dígitos lanza excepción
  - Test: Teléfono vacío lanza excepción
  - _Requirements: 3.4, 7.3, 9.2_

- [x] 3.3 Tests de Value Object Direccion
  - Test: Crear dirección válida trimea espacios
  - Test: Dirección con menos de 5 caracteres lanza excepción
  - Test: Dirección vacía lanza excepción
  - _Requirements: 3.5, 9.2_

- [x] 3.4 Tests de entidad Usuario
  - Test: Crear usuario con datos válidos establece propiedades correctamente
  - Test: Crear usuario con username vacío lanza excepción
  - Test: Crear usuario con nombre vacío lanza excepción
  - Test: Actualizar usuario modifica propiedades y FechaActualizacion
  - Test: CambiarRol modifica rol y FechaActualizacion
  - Test: Desactivar establece EstaActivo en false
  - Test: Reactivar establece EstaActivo en true
  - _Requirements: 3.1, 3.2, 3.6, 3.7, 5.5, 9.2_

- [x] 4. Checkpoint - Compilación de Dominio
  - Compilar proyecto Usuarios.Dominio
  - Ejecutar tests unitarios de Dominio
  - Verificar que no hay errores
  - Preguntar al usuario si hay dudas antes de continuar

- [x] 5. Implementación de la capa de Aplicación
  - Crear Commands y Handlers
  - Crear Queries y Handlers
  - Crear DTOs
  - Crear Validators
  - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5, 2.6, 2.7, 2.8, 7.1, 7.2, 7.3, 7.4, 7.5, 7.6, 7.7_

- [x] 5.1 Crear DTOs
  - Crear `Usuarios.Aplicacion/DTOs/UsuarioDto.cs` (record)
  - Crear `Usuarios.Aplicacion/DTOs/CrearUsuarioDto.cs` (record)
  - Crear `Usuarios.Aplicacion/DTOs/ActualizarUsuarioDto.cs` (record)
  - Todas las propiedades con init setters
  - _Requirements: 2.1, 2.2_

- [x] 5.2 Crear AgregarUsuarioComando y Handler
  - Crear `Usuarios.Aplicacion/Comandos/AgregarUsuarioComando.cs` (record, IRequest<Guid>)
  - Crear `Usuarios.Aplicacion/Comandos/AgregarUsuarioComandoHandler.cs`
  - Validar unicidad de username y correo
  - Crear Value Objects
  - Crear usuario con factory method
  - Crear en Keycloak primero, luego persistir en BD
  - Logging de operación
  - _Requirements: 2.2, 5.1, 5.2, 6.1, 6.2, 11.2_

- [x] 5.3 Crear ActualizarUsuarioComando y Handler
  - Crear `Usuarios.Aplicacion/Comandos/ActualizarUsuarioComando.cs` (record, IRequest<Unit>)
  - Crear `Usuarios.Aplicacion/Comandos/ActualizarUsuarioComandoHandler.cs`
  - Obtener usuario existente
  - Actualizar con método de negocio
  - Actualizar en Keycloak
  - Persistir cambios
  - _Requirements: 2.3, 5.4, 6.3, 11.2_

- [x] 5.4 Crear EliminarUsuarioComando y Handler
  - Crear `Usuarios.Aplicacion/Comandos/EliminarUsuarioComando.cs` (record, IRequest<Unit>)
  - Crear `Usuarios.Aplicacion/Comandos/EliminarUsuarioComandoHandler.cs`
  - Obtener usuario existente
  - Desactivar con método de negocio (eliminación lógica)
  - Desactivar en Keycloak
  - Persistir cambios
  - _Requirements: 2.4, 5.5, 6.4, 11.2_

- [x] 5.5 Crear ConsultarUsuarioQuery y Handler
  - Crear `Usuarios.Aplicacion/Consultas/ConsultarUsuarioQuery.cs` (record, IRequest<UsuarioDto?>)
  - Crear `Usuarios.Aplicacion/Consultas/ConsultarUsuarioQueryHandler.cs`
  - Obtener usuario por ID
  - Verificar que está activo
  - Mapear a DTO
  - _Requirements: 2.5, 5.6_

- [x] 5.6 Crear ConsultarUsuariosQuery y Handler
  - Crear `Usuarios.Aplicacion/Consultas/ConsultarUsuariosQuery.cs` (record, IRequest<IEnumerable<UsuarioDto>>)
  - Crear `Usuarios.Aplicacion/Consultas/ConsultarUsuariosQueryHandler.cs`
  - Obtener usuarios activos
  - Mapear a DTOs
  - _Requirements: 2.6, 5.7_

- [x] 5.7 Crear Validators con FluentValidation
  - Crear `Usuarios.Aplicacion/Validadores/CrearUsuarioDtoValidator.cs`
  - Validar Username (requerido, 3-50 caracteres)
  - Validar Nombre (requerido, max 100 caracteres)
  - Validar Correo (requerido, formato email)
  - Validar Telefono (requerido)
  - Validar Direccion (requerida, min 5 caracteres)
  - Validar Rol (enum válido)
  - Validar Password (requerido, min 8 caracteres)
  - _Requirements: 7.1, 7.2, 7.3, 7.4, 7.5, 7.6_

- [x] 5.8 Crear ActualizarUsuarioDtoValidator
  - Crear `Usuarios.Aplicacion/Validadores/ActualizarUsuarioDtoValidator.cs`
  - Validar Nombre (requerido, max 100 caracteres)
  - Validar Telefono (requerido)
  - Validar Direccion (requerida, min 5 caracteres)
  - _Requirements: 7.1_

- [x] 5.9 Crear InyeccionDependencias.cs
  - Crear `Usuarios.Aplicacion/InyeccionDependencias.cs`
  - Registrar MediatR con handlers
  - Registrar FluentValidation con validators
  - Registrar PipelineBehavior para validación automática
  - _Requirements: 2.7, 7.1_

- [x] 6. Tests unitarios de la capa de Aplicación
  - Tests de Handlers con mocks
  - Tests de Validators
  - _Requirements: 9.1_

- [x] 6.1 Tests de AgregarUsuarioComandoHandler
  - Test: Agregar usuario válido retorna Guid
  - Test: Username duplicado lanza UsernameDuplicadoException
  - Test: Correo duplicado lanza CorreoDuplicadoException
  - Test: Llama a ServicioKeycloak.CrearUsuarioAsync
  - Test: Llama a Repositorio.AgregarAsync
  - Test: Registra log de operación
  - _Requirements: 2.2, 5.1, 5.2, 6.1, 9.1_

- [x] 6.2 Tests de ActualizarUsuarioComandoHandler
  - Test: Actualizar usuario existente retorna Unit
  - Test: Usuario no encontrado lanza UsuarioNoEncontradoException
  - Test: Llama a Usuario.Actualizar()
  - Test: Llama a ServicioKeycloak.ActualizarUsuarioAsync
  - Test: Llama a Repositorio.ActualizarAsync
  - _Requirements: 2.3, 5.4, 6.3, 9.1_

- [x] 6.3 Tests de EliminarUsuarioComandoHandler
  - Test: Eliminar usuario existente retorna Unit
  - Test: Usuario no encontrado lanza UsuarioNoEncontradoException
  - Test: Llama a Usuario.Desactivar()
  - Test: Llama a ServicioKeycloak.DesactivarUsuarioAsync
  - Test: Llama a Repositorio.ActualizarAsync
  - _Requirements: 2.4, 5.5, 6.4, 9.1_

- [x] 6.4 Tests de ConsultarUsuarioQueryHandler
  - Test: Consultar usuario existente y activo retorna UsuarioDto
  - Test: Consultar usuario inexistente retorna null
  - Test: Consultar usuario inactivo retorna null
  - Test: DTO contiene todos los datos del usuario
  - _Requirements: 2.5, 5.6, 9.1_

- [x] 6.5 Tests de ConsultarUsuariosQueryHandler
  - Test: Consultar usuarios retorna lista de UsuarioDtos
  - Test: Solo retorna usuarios activos
  - Test: Lista vacía si no hay usuarios activos
  - _Requirements: 2.6, 5.7, 9.1_

- [x] 6.6 Tests de CrearUsuarioDtoValidator
  - Test: DTO válido pasa validación
  - Test: Username vacío falla validación
  - Test: Username muy corto falla validación
  - Test: Correo inválido falla validación
  - Test: Password muy corto falla validación
  - Test: Rol inválido falla validación
  - _Requirements: 7.1, 7.2, 7.3, 7.5, 7.6, 9.1_

- [x] 7. Checkpoint - Compilación de Aplicación
  - Compilar proyecto Usuarios.Aplicacion
  - Ejecutar tests unitarios de Aplicación
  - Verificar que no hay errores
  - Preguntar al usuario si hay dudas antes de continuar

- [x] 8. Implementación de la capa de Infraestructura
  - Crear DbContext y configuraciones
  - Implementar RepositorioUsuarios
  - Implementar ServicioKeycloak
  - Configurar migraciones
  - _Requirements: 4.4, 4.5, 4.6, 4.7, 6.1, 6.2, 6.3, 6.4, 10.1, 10.2, 10.3, 10.4, 10.5, 10.6, 10.7_

- [x] 8.1 Crear UsuariosDbContext
  - Crear `Usuarios.Infraestructura/Persistencia/UsuariosDbContext.cs`
  - DbSet<Usuario> Usuarios
  - Aplicar configuraciones en OnModelCreating
  - _Requirements: 10.2_

- [x] 8.2 Crear UsuarioEntityConfiguration
  - Crear `Usuarios.Infraestructura/Persistencia/Configuraciones/UsuarioEntityConfiguration.cs`
  - Configurar tabla "Usuarios"
  - Configurar propiedades con tipos y longitudes
  - Configurar índices únicos en Username y Correo
  - Configurar Value Objects con OwnsOne (Correo, Telefono, Direccion)
  - Configurar conversión de enum Rol a int
  - _Requirements: 10.5, 10.7_

- [x] 8.3 Implementar RepositorioUsuarios
  - Crear `Usuarios.Infraestructura/Repositorios/RepositorioUsuarios.cs`
  - Implementar todos los métodos de IRepositorioUsuarios
  - Usar async/await con EF Core
  - Logging de operaciones
  - _Requirements: 4.4, 4.5, 4.6, 4.7_

- [x] 8.4 Implementar ServicioKeycloak
  - Crear `Usuarios.Infraestructura/Servicios/ServicioKeycloak.cs`
  - Implementar CrearUsuarioAsync con HttpClient
  - Implementar ActualizarUsuarioAsync
  - Implementar DesactivarUsuarioAsync
  - Implementar AsignarRolAsync
  - Leer configuración de Keycloak desde IConfiguration
  - Manejo de errores con logging
  - _Requirements: 6.1, 6.2, 6.3, 6.4, 6.7_

- [x] 8.5 Crear InyeccionDependencias.cs
  - Crear `Usuarios.Infraestructura/InyeccionDependencias.cs`
  - Registrar DbContext con PostgreSQL
  - Registrar IRepositorioUsuarios con RepositorioUsuarios
  - Registrar HttpClient para ServicioKeycloak
  - Registrar IServicioKeycloak con ServicioKeycloak
  - _Requirements: 10.2_

- [x] 8.6 Crear migración inicial
  - Ejecutar `dotnet ef migrations add InitialCreate`
  - Verificar que la migración crea tabla Usuarios correctamente
  - Verificar índices y constraints
  - _Requirements: 10.3_

- [x] 9. Tests de la capa de Infraestructura
  - Tests de Repositorio con InMemory database
  - Tests de integración con PostgreSQL (Testcontainers)
  - Tests de ServicioKeycloak con mock de HttpClient
  - _Requirements: 9.4_

- [x] 9.1 Tests de RepositorioUsuarios con InMemory
  - Test: AgregarAsync persiste usuario correctamente
  - Test: ObtenerPorIdAsync retorna usuario existente
  - Test: ObtenerPorUsernameAsync retorna usuario por username
  - Test: ObtenerPorCorreoAsync retorna usuario por correo
  - Test: ObtenerActivosAsync solo retorna usuarios activos
  - Test: ExisteUsernameAsync retorna true si existe
  - Test: ExisteCorreoAsync retorna true si existe
  - Test: ActualizarAsync persiste cambios
  - _Requirements: 4.4, 4.5, 4.6, 4.7, 9.4_

- [x] 9.2 Tests de integración con PostgreSQL (Testcontainers)
  - Test: Crear usuario y recuperarlo de PostgreSQL real
  - Test: Value Objects se persisten y recuperan correctamente
  - Test: Índices únicos funcionan (username y correo)
  - Test: Eliminación lógica funciona correctamente
  - _Requirements: 10.1, 10.5, 10.7, 9.4_

- [x] 9.3 Tests de ServicioKeycloak
  - Test: CrearUsuarioAsync envía request correcto a Keycloak
  - Test: ActualizarUsuarioAsync envía request correcto
  - Test: DesactivarUsuarioAsync envía request correcto
  - Test: AsignarRolAsync envía request correcto
  - Test: Error de Keycloak se maneja apropiadamente
  - Usar mock de HttpClient con respuestas simuladas
  - _Requirements: 6.1, 6.2, 6.3, 6.4, 6.7, 9.4_

- [x] 10. Checkpoint - Compilación de Infraestructura
  - Compilar proyecto Usuarios.Infraestructura
  - Ejecutar tests de Infraestructura
  - Verificar que no hay errores
  - Preguntar al usuario si hay dudas antes de continuar

- [x] 11. Implementación de la capa de API
  - Crear UsuariosController
  - Crear ExceptionHandlingMiddleware
  - Configurar Program.cs
  - Configurar appsettings.json
  - _Requirements: 8.1, 8.2, 8.3, 8.4, 8.5, 8.6, 8.7, 11.1, 11.2, 11.3, 11.4, 11.5, 11.6, 11.7_

- [x] 11.1 Crear UsuariosController
  - Crear `Usuarios.API/Controllers/UsuariosController.cs`
  - Endpoint POST /api/usuarios (Crear)
  - Endpoint GET /api/usuarios/{id} (ObtenerPorId)
  - Endpoint GET /api/usuarios (ObtenerTodos)
  - Endpoint PUT /api/usuarios/{id} (Actualizar)
  - Endpoint DELETE /api/usuarios/{id} (Eliminar)
  - Controller solo ejecuta _mediator.Send()
  - Documentación con atributos ProducesResponseType
  - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5, 5.6, 5.7_

- [x] 11.2 Crear ExceptionHandlingMiddleware
  - Crear `Usuarios.API/Middleware/ExceptionHandlingMiddleware.cs`
  - Manejar UsuarioNoEncontradoException → 404
  - Manejar CorreoDuplicadoException → 400
  - Manejar UsernameDuplicadoException → 400
  - Manejar ValidationException → 400 con detalles
  - Manejar HttpRequestException (Keycloak) → 502
  - Manejar Exception genérica → 500
  - Logging de todas las excepciones
  - _Requirements: 8.1, 8.2, 8.3, 8.4, 8.5, 8.6, 8.7_

- [x] 11.3 Configurar Program.cs
  - Configurar Serilog para logging estructurado
  - Registrar servicios de Aplicacion (MediatR, FluentValidation)
  - Registrar servicios de Infraestructura (DbContext, Repositorios, Keycloak)
  - Configurar Swagger/OpenAPI
  - Configurar ExceptionHandlingMiddleware
  - Configurar health checks
  - Aplicar migraciones automáticamente al iniciar
  - _Requirements: 10.3, 11.1, 11.7_

- [x] 11.4 Crear appsettings.json
  - Configurar ConnectionStrings:PostgresConnection
  - Configurar Keycloak:Authority, Keycloak:AdminUrl, Keycloak:ClientId, Keycloak:ClientSecret
  - Configurar Serilog con niveles de log
  - _Requirements: 10.4, 11.1_

- [x] 11.5 Crear appsettings.Development.json
  - Configurar logging en Debug para desarrollo
  - Configurar ConnectionString para localhost
  - Configurar Keycloak para localhost
  - _Requirements: 11.1_

- [x] 12. Tests de la capa de API
  - Tests de UsuariosController
  - Tests de integración end-to-end
  - _Requirements: 9.5_

- [x] 12.1 Tests de UsuariosController
  - Test: POST /api/usuarios retorna 201 Created con Guid
  - Test: GET /api/usuarios/{id} retorna 200 OK con UsuarioDto
  - Test: GET /api/usuarios/{id} inexistente retorna 404
  - Test: GET /api/usuarios retorna 200 OK con lista
  - Test: PUT /api/usuarios/{id} retorna 204 No Content
  - Test: DELETE /api/usuarios/{id} retorna 204 No Content
  - Usar mocks de IMediator
  - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5, 5.6, 5.7, 9.5_

- [x] 12.2 Tests de integración end-to-end
  - Usar WebApplicationFactory
  - Test: Crear usuario, obtenerlo, actualizarlo, eliminarlo (flujo completo)
  - Test: Crear usuario con correo duplicado retorna 400
  - Test: Crear usuario con username duplicado retorna 400
  - Test: Obtener usuario inexistente retorna 404
  - Usar InMemory database para tests
  - _Requirements: 9.5_

- [-] 13. Verificación de Correctness Properties
  - Escribir property-based tests para validar propiedades del sistema
  - _Requirements: Todas las Correctness Properties_

- [x] 13.1 Property Test: Unicidad de Username
  - **Property 1: Unicidad de Username**
  - **Validates: Requirements 5.2**
  - Generar múltiples usuarios con diferentes usernames
  - Verificar que no se pueden crear dos usuarios con mismo username
  - _Requirements: 5.2_

- [x] 13.2 Property Test: Unicidad de Correo
  - **Property 2: Unicidad de Correo**
  - **Validates: Requirements 5.1**
  - Generar múltiples usuarios con diferentes correos
  - Verificar que no se pueden crear dos usuarios con mismo correo
  - _Requirements: 5.1_

- [x] 13.3 Property Test: Validación de Correo
  - **Property 3: Validación de Correo**
  - **Validates: Requirements 3.3, 7.2**
  - Generar correos aleatorios válidos e inválidos
  - Verificar que solo correos válidos se aceptan
  - _Requirements: 3.3, 7.2_

- [x] 13.4 Property Test: Validación de Teléfono
  - **Property 4: Validación de Teléfono**
  - **Validates: Requirements 3.4, 7.3**
  - Generar teléfonos aleatorios con diferentes longitudes
  - Verificar que solo teléfonos de 7-15 dígitos se aceptan
  - _Requirements: 3.4, 7.3_

- [x] 13.5 Property Test: Inmutabilidad de Value Objects
  - **Property 5: Inmutabilidad de Value Objects**
  - **Validates: Requirements 3.3, 3.4, 3.5**
  - Verificar que Correo, Telefono, Direccion son records
  - Verificar que no tienen setters públicos
  - _Requirements: 3.3, 3.4, 3.5_

- [x] 13.6 Property Test: Eliminación Lógica
  - **Property 6: Eliminación Lógica**
  - **Validates: Requirements 5.5**
  - Crear usuario, eliminarlo, verificar que EstaActivo = false
  - Verificar que no aparece en ObtenerActivosAsync
  - _Requirements: 5.5_

- [x] 13.7 Property Test: Logging de Operaciones
  - **Property 9: Logging de Operaciones**
  - **Validates: Requirements 11.2, 11.3**
  - Ejecutar operaciones y verificar que se registran logs
  - Verificar que logs contienen timestamp y resultado
  - _Requirements: 11.2, 11.3_

- [x] 14. Dockerización
  - Crear Dockerfile
  - Crear docker-compose.yml
  - Configurar variables de entorno
  - _Requirements: 12.1, 12.2, 12.3, 12.4, 12.5, 12.6, 12.7_

- [x] 14.1 Crear Dockerfile
  - Multi-stage build (base, build, publish, final)
  - Usar mcr.microsoft.com/dotnet/aspnet:8.0 como base
  - Usar mcr.microsoft.com/dotnet/sdk:8.0 para build
  - Exponer puerto 8080
  - Optimizar capas para cache
  - _Requirements: 12.1, 12.2, 12.6_

- [x] 14.2 Crear docker-compose.yml
  - Servicio usuarios-api
  - Configurar puerto 8083:8080
  - Configurar variables de entorno (ConnectionString, Keycloak)
  - Conectar a red kairo-network
  - Dependencias: postgres, keycloak
  - _Requirements: 12.3, 12.4, 12.5_

- [x] 14.3 Crear .dockerignore
  - Ignorar bin/, obj/, .vs/, .git/
  - Ignorar archivos de tests
  - _Requirements: 12.7_

- [x] 14.4 Configurar health checks en Docker
  - Agregar HEALTHCHECK en Dockerfile
  - Verificar endpoint /health
  - Configurar intervalo y timeout
  - _Requirements: 11.7, 12.5_

- [x] 15. Documentación
  - Crear README.md del microservicio
  - Documentar arquitectura
  - Documentar endpoints
  - Documentar configuración
  - _Requirements: Todas_

- [x] 15.1 Crear README.md
  - Sección: Descripción del microservicio
  - Sección: Arquitectura (Hexagonal + CQRS)
  - Sección: Tecnologías utilizadas
  - Sección: Estructura de carpetas
  - Sección: Endpoints de API con ejemplos
  - Sección: Configuración (variables de entorno)
  - Sección: Ejecución local (Docker y sin Docker)
  - Sección: Tests (cómo ejecutarlos)
  - Sección: Integración con Keycloak
  - _Requirements: Todas_

- [x] 16. Checkpoint final - Verificación completa
  - Compilar todo el proyecto sin errores
  - Ejecutar todos los tests (unitarios, integración, property-based)
  - Verificar cobertura de tests >90%
  - Levantar con Docker y verificar funcionamiento
  - Verificar integración con PostgreSQL
  - Verificar integración con Keycloak
  - Revisar checklist de requirements completados

- [x] 16.1 Ejecutar tests y verificar cobertura
  - Ejecutar `dotnet test --collect:"XPlat Code Coverage"`
  - Verificar que cobertura es >90%
  - Generar reporte de cobertura
  - _Requirements: 9.6_

- [x] 16.2 Verificar compilación
  - Ejecutar `dotnet build` en modo Release
  - Verificar que no hay warnings críticos
  - Verificar tiempo de compilación
  - _Requirements: Todas_

- [x] 16.3 Prueba end-to-end con Docker
  - Levantar infraestructura (PostgreSQL, Keycloak)
  - Levantar microservicio Usuarios
  - Crear usuario vía API
  - Verificar usuario en PostgreSQL
  - Verificar usuario en Keycloak
  - Actualizar y eliminar usuario
  - _Requirements: Todas_

## Notes

- Todas las tareas son requeridas para una refactorización completa con testing comprehensivo
- Cada tarea referencia los requirements específicos que valida
- Los checkpoints permiten validación incremental y feedback del usuario
- Los property tests validan correctness properties del sistema
- La cobertura de tests debe ser >90%
- El microservicio debe seguir exactamente los mismos estándares que Eventos, Asientos y Reportes
