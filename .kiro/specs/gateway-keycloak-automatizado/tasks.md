# Implementation Plan: Gateway y Keycloak Automatizado

## Overview

Este plan implementa un API Gateway profesional con YARP y configuración automatizada de Keycloak. Las tareas están organizadas para construir incrementalmente: primero la estructura base del Gateway, luego la autenticación, autorización, y finalmente la integración con Docker y Keycloak.

## Tasks

- [x] 1. Configurar proyecto Gateway con YARP
  - Crear estructura de proyecto .NET 8
  - Instalar paquetes NuGet necesarios (Yarp.ReverseProxy, Microsoft.AspNetCore.Authentication.JwtBearer)
  - Configurar Program.cs con servicios básicos
  - _Requirements: 1.7_

- [x] 2. Implementar configuración de rutas YARP
  - [x] 2.1 Crear appsettings.json con configuración de rutas
    - Definir rutas para /api/eventos/*, /api/asientos/*, /api/usuarios/*, /api/entradas/*, /api/reportes/*
    - Definir clusters para cada microservicio
    - Configurar transformaciones de path
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5_

  - [x] 2.2 Escribir tests unitarios para configuración de rutas
    - Verificar que todas las rutas están definidas correctamente
    - Verificar que todos los clusters tienen destinos válidos
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5_

- [x] 3. Implementar autenticación JWT con Keycloak
  - [x] 3.1 Crear AuthenticationConfiguration.cs
    - Implementar método de extensión AddKeycloakAuthentication
    - Configurar JwtBearerOptions con Authority y Audience
    - Configurar TokenValidationParameters
    - Agregar eventos de logging para autenticación
    - _Requirements: 2.1, 2.5, 2.6_

  - [x] 3.2 Configurar appsettings.json con parámetros de Keycloak
    - Agregar sección Keycloak con Authority, Audience, MetadataAddress
    - _Requirements: 2.1, 2.6_

  - [x] 3.3 Escribir tests unitarios para autenticación
    - Verificar que la configuración de autenticación se aplica correctamente
    - Verificar que TokenValidationParameters están configurados
    - _Requirements: 2.1, 2.5_

- [x] 4. Implementar autorización basada en roles
  - [x] 4.1 Crear AuthorizationConfiguration.cs
    - Implementar método de extensión AddRoleBasedAuthorization
    - Definir políticas: Authenticated, UserAccess, AdminAccess, OrganizatorAccess, EventManagement
    - Configurar RoleClaimType como "roles"
    - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5_

  - [x] 4.2 Escribir tests unitarios para autorización
    - Verificar que todas las políticas están registradas
    - Verificar que las políticas requieren los roles correctos
    - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5_

- [x] 5. Implementar configuración CORS
  - [x] 5.1 Crear CorsConfiguration.cs
    - Implementar método de extensión AddCorsPolicy
    - Leer orígenes permitidos desde configuración
    - Configurar política AllowFrontends con AllowCredentials
    - _Requirements: 6.1, 6.2, 6.3, 6.4, 6.5_

  - [x] 5.2 Agregar configuración CORS a appsettings.json
    - Definir array de orígenes permitidos
    - _Requirements: 6.1, 6.2_

  - [x] 5.3 Escribir tests unitarios para CORS
    - Verificar que la política CORS está configurada
    - Verificar que los orígenes permitidos están registrados
    - _Requirements: 6.1, 6.2, 6.3, 6.4, 6.5_

- [x] 6. Implementar middleware de logging
  - [x] 6.1 Crear RequestLoggingMiddleware.cs
    - Implementar logging de todas las peticiones HTTP
    - Registrar método, path, timestamp, duración, status code
    - Manejar excepciones y registrarlas
    - _Requirements: 8.1, 8.5_

  - [x] 6.2 Escribir tests unitarios para RequestLoggingMiddleware
    - Verificar que se registran todas las peticiones
    - Verificar que se incluyen todos los campos requeridos
    - _Requirements: 8.1, 8.5_

- [x] 7. Implementar middleware de manejo de excepciones
  - [x] 7.1 Crear ExceptionHandlingMiddleware.cs
    - Implementar manejo de SecurityTokenExpiredException (401)
    - Implementar manejo de SecurityTokenInvalidSignatureException (401)
    - Implementar manejo de HttpRequestException (503)
    - Implementar manejo de excepciones genéricas (500)
    - Retornar respuestas JSON estructuradas
    - _Requirements: 2.2, 2.3, 2.4, 1.6_

  - [x] 7.2 Escribir tests unitarios para ExceptionHandlingMiddleware
    - Verificar manejo de cada tipo de excepción
    - Verificar formato de respuestas de error
    - _Requirements: 2.2, 2.3, 2.4, 1.6_

- [x] 8. Implementar health checks
  - [x] 8.1 Crear KeycloakHealthCheck.cs
    - Implementar IHealthCheck
    - Verificar conectividad con Keycloak usando MetadataAddress
    - Retornar Healthy/Unhealthy según disponibilidad
    - _Requirements: 7.2, 7.3_

  - [x] 8.2 Configurar endpoints de health checks en Program.cs
    - Agregar /health endpoint (liveness)
    - Agregar /health/ready endpoint con KeycloakHealthCheck (readiness)
    - Agregar /health/live endpoint (liveness simple)
    - _Requirements: 7.1, 7.2, 7.4_

  - [x] 8.3 Escribir tests unitarios para health checks
    - Verificar KeycloakHealthCheck cuando Keycloak está disponible
    - Verificar KeycloakHealthCheck cuando Keycloak no está disponible
    - _Requirements: 7.2, 7.3_

- [x] 9. Implementar configuración de variables de entorno
  - [x] 9.1 Crear clase de configuración para leer variables de entorno
    - Leer Keycloak__Authority, Keycloak__Audience, Keycloak__MetadataAddress
    - Leer URLs de microservicios
    - Leer orígenes CORS
    - Proporcionar valores por defecto para desarrollo
    - _Requirements: 10.1, 10.2, 10.3, 10.4_

  - [x] 9.2 Implementar validación de configuración al inicio
    - Validar que variables requeridas están presentes
    - Lanzar excepción con mensaje claro si faltan variables
    - _Requirements: 10.5_

  - [x] 9.3 Escribir tests unitarios para configuración
    - Verificar lectura de variables de entorno
    - Verificar aplicación de valores por defecto
    - Verificar validación de configuración requerida
    - _Requirements: 10.1, 10.2, 10.3, 10.4, 10.5_

- [x] 10. Checkpoint - Verificar Gateway funciona localmente
  - Compilar el proyecto sin errores
  - Ejecutar el Gateway localmente
  - Verificar que los endpoints de health check responden
  - Asegurar que todos los tests pasan

- [x] 11. Crear archivo realm-export.json de Keycloak
  - [x] 11.1 Crear estructura base del realm
    - Definir realm "Kairo" con configuración básica
    - Configurar tiempos de vida de tokens (access: 300s, refresh: 1800s)
    - Configurar opciones de seguridad (bruteForceProtected, etc.)
    - _Requirements: 5.1, 5.6_

  - [x] 11.2 Definir roles del realm
    - Crear rol "User" con descripción
    - Crear rol "Admin" con descripción
    - Crear rol "Organizator" con descripción
    - _Requirements: 5.4, 4.5_

  - [x] 11.3 Definir cliente kairo-web
    - Configurar como cliente público (publicClient: true)
    - Habilitar Authorization Code flow con PKCE
    - Configurar redirectUris para localhost:5173 y localhost:3000
    - Configurar webOrigins para CORS
    - _Requirements: 5.2, 5.7, 4.3_

  - [x] 11.4 Definir cliente kairo-api
    - Configurar como bearer-only client
    - Deshabilitar flujos no necesarios
    - _Requirements: 5.3, 4.4_

  - [x] 11.5 Definir usuarios por defecto
    - Crear usuario "admin" con password "admin123" y rol Admin
    - Crear usuario "organizador" con password "org123" y rol Organizator
    - Crear usuario "usuario" con password "user123" y rol User
    - _Requirements: 5.5, 4.6_

  - [x] 11.6 Escribir test de validación del realm-export.json
    - Verificar que el JSON es válido
    - Verificar que contiene todos los elementos requeridos
    - Verificar estructura de clientes, roles y usuarios
    - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5, 5.6, 5.7_

- [x] 12. Crear Dockerfile para el Gateway
  - Usar multi-stage build (base, build, publish, final)
  - Instalar curl para health checks
  - Exponer puerto 8080
  - Configurar ENTRYPOINT
  - _Requirements: 9.4_

- [x] 13. Actualizar docker-compose.yml de infraestructura
  - [x] 13.1 Agregar servicio Keycloak
    - Usar imagen quay.io/keycloak/keycloak:23.0
    - Configurar variables de entorno (admin, database)
    - Montar realm-export.json en /opt/keycloak/data/import/
    - Agregar comando --import-realm
    - Configurar health check
    - Exponer puerto 8180
    - _Requirements: 4.1, 4.2, 9.1, 9.5_

  - [x] 13.2 Agregar servicio Gateway
    - Usar build context de Gateway
    - Configurar variables de entorno para Keycloak y microservicios
    - Configurar depends_on con Keycloak
    - Configurar health check
    - Exponer puerto 8080
    - Conectar a kairo-network
    - _Requirements: 9.2, 9.3, 9.4, 9.5_

  - [x] 13.3 Actualizar init.sql de PostgreSQL
    - Agregar creación de base de datos "keycloak"
    - _Requirements: 4.1_

- [x] 14. Crear archivo .env.example
  - Documentar todas las variables de entorno necesarias
  - Incluir valores de ejemplo para desarrollo
  - _Requirements: 10.1, 10.2, 10.3, 10.4_

- [x] 15. Checkpoint - Verificar integración Docker
  - Ejecutar docker-compose up en Infraestructura
  - Verificar que Keycloak inicia y importa el realm
  - Verificar que Gateway inicia y se conecta a Keycloak
  - Verificar health checks de ambos servicios
  - Verificar que se puede acceder a Keycloak Admin Console

- [x] 16. Escribir tests de integración end-to-end
  - [x] 16.1 Test de enrutamiento a microservicios
    - **Property 1: Route Matching Consistency**
    - **Validates: Requirements 1.1, 1.2, 1.3, 1.4, 1.5**

  - [x] 16.2 Test de manejo de servicios no disponibles
    - **Property 2: Service Unavailability Handling**
    - **Validates: Requirements 1.6**

  - [x] 16.3 Test de autenticación con tokens válidos
    - **Property 3: Valid Token Authentication**
    - **Validates: Requirements 2.1, 2.5**

  - [x] 16.4 Test de autorización basada en roles
    - **Property 4: Role-Based Authorization**
    - **Validates: Requirements 3.1, 3.2, 3.3, 3.4**

  - [x] 16.5 Test de extracción de roles del token
    - **Property 5: Role Claim Extraction**
    - **Validates: Requirements 3.5**

  - [x] 16.6 Test de headers CORS
    - **Property 6: CORS Header Presence**
    - **Validates: Requirements 6.1, 6.2, 6.3, 6.4, 6.5**

  - [x] 16.7 Test de logging de peticiones
    - **Property 7: Request Logging Completeness**
    - **Validates: Requirements 8.1, 8.5**

  - [x] 16.8 Test de logging de autenticación
    - **Property 8: Authentication Logging**
    - **Validates: Requirements 8.2, 8.3**

  - [x] 16.9 Test de logging de autorización
    - **Property 9: Authorization Logging**
    - **Validates: Requirements 8.4**

  - [x] 16.10 Test de configuración por variables de entorno
    - **Property 10: Environment Variable Configuration**
    - **Validates: Requirements 10.1, 10.2, 10.3, 10.4**

  - [x] 16.11 Test de validación de configuración al inicio
    - **Property 11: Startup Configuration Validation**
    - **Validates: Requirements 10.5**

- [-] 17. Crear documentación de uso
  - [x] 17.1 Actualizar README.md del Gateway
    - Documentar arquitectura y componentes
    - Documentar cómo ejecutar localmente
    - Documentar cómo ejecutar con Docker
    - Documentar endpoints disponibles
    - Documentar configuración de variables de entorno

  - [x] 17.2 Actualizar README.md de Infraestructura
    - Documentar nuevo servicio Keycloak
    - Documentar nuevo servicio Gateway
    - Documentar cómo acceder a Keycloak Admin Console
    - Documentar usuarios por defecto
    - Documentar proceso de importación del realm

  - [x] 17.3 Crear guía de troubleshooting
    - Documentar problemas comunes y soluciones
    - Documentar cómo verificar logs de Keycloak
    - Documentar cómo verificar logs del Gateway
    - Documentar cómo regenerar el realm si es necesario

- [x] 18. Checkpoint Final - Verificación completa del sistema
  - Ejecutar todos los tests (unit, integration, property-based)
  - Verificar cobertura de tests >90%
  - Levantar todo el sistema con docker-compose
  - Verificar que Keycloak está configurado correctamente
  - Verificar que Gateway enruta correctamente a todos los microservicios
  - Verificar autenticación y autorización funcionan
  - Verificar CORS funciona desde frontend
  - Verificar health checks funcionan
  - Verificar logs se generan correctamente

## Notes

- Todas las tareas son requeridas para una implementación completa y profesional
- Cada tarea referencia los requisitos específicos que implementa
- Los checkpoints aseguran validación incremental
- Los tests de propiedades validan correctness universal
- La documentación es esencial para el mantenimiento futuro
