# Requirements Document - Gateway y Keycloak Automatizado

## Introduction

Este documento define los requisitos para implementar un API Gateway profesional usando YARP (Yet Another Reverse Proxy) con autenticación y autorización centralizada mediante Keycloak, incluyendo la automatización completa de la configuración de Keycloak para eliminar la configuración manual.

## Glossary

- **Gateway**: API Gateway que actúa como punto de entrada único para todos los microservicios
- **YARP**: Yet Another Reverse Proxy - biblioteca de Microsoft para crear reverse proxies
- **Keycloak**: Sistema de gestión de identidad y acceso (IAM) open source
- **Realm**: Espacio de trabajo aislado en Keycloak que contiene usuarios, roles y clientes
- **Client**: Aplicación registrada en Keycloak que puede autenticar usuarios
- **Role**: Rol de usuario que define permisos (User, Admin, Organizator)
- **Realm_Export**: Archivo JSON que contiene la configuración completa de un realm de Keycloak

## Requirements

### Requirement 1: API Gateway con YARP

**User Story:** Como arquitecto de sistema, quiero un API Gateway centralizado usando YARP, para que todas las peticiones HTTP pasen por un único punto de entrada con enrutamiento inteligente.

#### Acceptance Criteria

1. WHEN el Gateway recibe una petición a `/api/eventos/*`, THE Gateway SHALL enrutar la petición al microservicio de Eventos
2. WHEN el Gateway recibe una petición a `/api/asientos/*`, THE Gateway SHALL enrutar la petición al microservicio de Asientos
3. WHEN el Gateway recibe una petición a `/api/usuarios/*`, THE Gateway SHALL enrutar la petición al microservicio de Usuarios
4. WHEN el Gateway recibe una petición a `/api/entradas/*`, THE Gateway SHALL enrutar la petición al microservicio de Entradas
5. WHEN el Gateway recibe una petición a `/api/reportes/*`, THE Gateway SHALL enrutar la petición al microservicio de Reportes
6. WHEN un microservicio no está disponible, THE Gateway SHALL retornar un error HTTP 503 Service Unavailable
7. WHEN el Gateway se inicia, THE Gateway SHALL cargar la configuración de rutas desde appsettings.json

### Requirement 2: Autenticación JWT con Keycloak

**User Story:** Como desarrollador, quiero que el Gateway valide tokens JWT emitidos por Keycloak, para que solo usuarios autenticados puedan acceder a los microservicios.

#### Acceptance Criteria

1. WHEN una petición incluye un token JWT válido en el header Authorization, THE Gateway SHALL validar el token contra Keycloak
2. WHEN una petición incluye un token JWT expirado, THE Gateway SHALL retornar HTTP 401 Unauthorized
3. WHEN una petición incluye un token JWT inválido, THE Gateway SHALL retornar HTTP 401 Unauthorized
4. WHEN una petición no incluye token JWT en endpoints protegidos, THE Gateway SHALL retornar HTTP 401 Unauthorized
5. WHEN el token JWT es válido, THE Gateway SHALL extraer los claims del usuario (roles, username, email)
6. WHEN el Gateway valida un token, THE Gateway SHALL usar el endpoint de metadata de Keycloak para obtener las claves públicas

### Requirement 3: Autorización Basada en Roles

**User Story:** Como administrador de sistema, quiero que el Gateway aplique políticas de autorización basadas en roles, para que los usuarios solo accedan a recursos permitidos según su rol.

#### Acceptance Criteria

1. WHEN un usuario con rol "User" intenta acceder a un endpoint de usuario, THE Gateway SHALL permitir el acceso
2. WHEN un usuario con rol "Admin" intenta acceder a cualquier endpoint, THE Gateway SHALL permitir el acceso
3. WHEN un usuario con rol "Organizator" intenta acceder a endpoints de eventos y asientos, THE Gateway SHALL permitir el acceso
4. WHEN un usuario sin el rol requerido intenta acceder a un endpoint protegido, THE Gateway SHALL retornar HTTP 403 Forbidden
5. WHEN se define una política de autorización, THE Gateway SHALL leer los roles desde el claim "roles" del token JWT

### Requirement 4: Configuración Automatizada de Keycloak

**User Story:** Como DevOps engineer, quiero que Keycloak se configure automáticamente al iniciar Docker, para que no tenga que configurar manualmente realms, clientes, roles y usuarios cada vez.

#### Acceptance Criteria

1. WHEN Docker Compose inicia Keycloak por primera vez, THE System SHALL importar automáticamente el archivo realm-export.json
2. WHEN se importa el realm, THE System SHALL crear el realm "Kairo" con toda su configuración
3. WHEN se importa el realm, THE System SHALL crear el cliente "kairo-web" para el frontend
4. WHEN se importa el realm, THE System SHALL crear el cliente "kairo-api" para el backend
5. WHEN se importa el realm, THE System SHALL crear los roles "User", "Admin" y "Organizator"
6. WHEN se importa el realm, THE System SHALL crear un usuario administrador por defecto con credenciales conocidas
7. WHEN Keycloak ya tiene el realm importado, THE System SHALL omitir la importación para evitar duplicados

### Requirement 5: Realm Export de Keycloak

**User Story:** Como arquitecto de sistema, quiero un archivo realm-export.json completo, para que defina toda la configuración de seguridad del sistema de forma declarativa.

#### Acceptance Criteria

1. THE Realm_Export SHALL definir el realm "Kairo" con configuración de tokens JWT
2. THE Realm_Export SHALL definir el cliente "kairo-web" con flujo de autenticación Authorization Code + PKCE
3. THE Realm_Export SHALL definir el cliente "kairo-api" con flujo de autenticación Client Credentials
4. THE Realm_Export SHALL definir los roles "User", "Admin" y "Organizator" como realm roles
5. THE Realm_Export SHALL definir un usuario "admin" con password "admin123" y rol "Admin"
6. THE Realm_Export SHALL configurar el tiempo de vida de tokens (access token: 5 minutos, refresh token: 30 minutos)
7. THE Realm_Export SHALL configurar las URLs de redirección permitidas para el cliente web

### Requirement 6: CORS y Configuración de Red

**User Story:** Como desarrollador frontend, quiero que el Gateway permita peticiones CORS desde el frontend, para que la aplicación web pueda comunicarse con el backend.

#### Acceptance Criteria

1. WHEN el frontend en http://localhost:5173 envía una petición, THE Gateway SHALL permitir la petición CORS
2. WHEN el frontend en http://localhost:3000 envía una petición, THE Gateway SHALL permitir la petición CORS
3. WHEN una petición CORS incluye credenciales, THE Gateway SHALL permitir el envío de cookies y headers de autenticación
4. WHEN se recibe una petición OPTIONS (preflight), THE Gateway SHALL responder con los headers CORS apropiados
5. THE Gateway SHALL permitir los headers Authorization, Content-Type y Accept en peticiones CORS

### Requirement 7: Health Checks y Monitoreo

**User Story:** Como SRE, quiero que el Gateway exponga endpoints de health check, para que pueda monitorear el estado del sistema.

#### Acceptance Criteria

1. WHEN se accede a `/health`, THE Gateway SHALL retornar HTTP 200 OK si está funcionando
2. WHEN se accede a `/health/ready`, THE Gateway SHALL verificar la conectividad con Keycloak
3. WHEN Keycloak no está disponible, THE Gateway SHALL retornar HTTP 503 en `/health/ready`
4. WHEN se accede a `/health/live`, THE Gateway SHALL retornar HTTP 200 si el proceso está vivo
5. THE Gateway SHALL incluir información de versión en la respuesta de health check

### Requirement 8: Logging y Observabilidad

**User Story:** Como desarrollador, quiero que el Gateway registre todas las peticiones y errores, para que pueda diagnosticar problemas en producción.

#### Acceptance Criteria

1. WHEN el Gateway recibe una petición, THE Gateway SHALL registrar la ruta, método HTTP y timestamp
2. WHEN el Gateway valida un token JWT, THE Gateway SHALL registrar el resultado de la validación
3. WHEN ocurre un error de autenticación, THE Gateway SHALL registrar el error con nivel Warning
4. WHEN ocurre un error de autorización, THE Gateway SHALL registrar el usuario y el recurso denegado
5. WHEN un microservicio retorna un error, THE Gateway SHALL registrar el error con el código de estado HTTP

### Requirement 9: Docker Compose Integrado

**User Story:** Como DevOps engineer, quiero que el Gateway se integre con el docker-compose de infraestructura, para que todo el sistema se levante con un solo comando.

#### Acceptance Criteria

1. WHEN se ejecuta docker-compose up, THE System SHALL iniciar Keycloak con la configuración automatizada
2. WHEN se ejecuta docker-compose up, THE System SHALL iniciar el Gateway después de que Keycloak esté listo
3. WHEN el Gateway se inicia, THE Gateway SHALL conectarse a la red kairo-network
4. WHEN el Gateway se inicia, THE Gateway SHALL exponer el puerto 8080 para peticiones HTTP
5. THE Docker_Compose SHALL definir health checks para Keycloak y el Gateway

### Requirement 10: Variables de Entorno y Configuración

**User Story:** Como desarrollador, quiero que toda la configuración sensible use variables de entorno, para que pueda cambiar la configuración sin modificar código.

#### Acceptance Criteria

1. THE Gateway SHALL leer la URL de Keycloak desde una variable de entorno
2. THE Gateway SHALL leer las URLs de los microservicios desde variables de entorno
3. THE Gateway SHALL leer la configuración de CORS desde variables de entorno
4. THE Gateway SHALL proporcionar valores por defecto para desarrollo local
5. THE Gateway SHALL validar que todas las variables de entorno requeridas estén presentes al iniciar
