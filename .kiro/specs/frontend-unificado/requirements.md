# Requirements Document - Frontend Unificado

## Introduction

Este documento define los requisitos para crear un frontend unificado profesional usando React + Vite + TypeScript que reemplace los dos frontends incompletos existentes (Frontend1 y Frontend2). El nuevo frontend se integrará con el Gateway implementado en FASE 2 y consumirá los microservicios refactorizados en FASE 1, proporcionando una experiencia de usuario moderna y consistente.

## Glossary

- **Frontend_Unificado**: Aplicación React única que reemplaza Frontend1 y Frontend2
- **Vite**: Build tool moderno y rápido para aplicaciones frontend
- **OIDC**: OpenID Connect - protocolo de autenticación sobre OAuth 2.0
- **Gateway**: API Gateway único punto de entrada para todos los microservicios
- **Keycloak**: Sistema de gestión de identidad y acceso (IAM)
- **Module**: Agrupación lógica de componentes relacionados (eventos, usuarios, entradas)
- **Protected_Route**: Ruta que requiere autenticación para acceder
- **Role_Based_Access**: Control de acceso basado en roles de usuario

## Requirements

### Requirement 1: Arquitectura Modular

**User Story:** Como desarrollador, quiero una arquitectura modular clara, para que el código sea mantenible y escalable.

#### Acceptance Criteria

1. THE Frontend SHALL organizar el código en módulos por dominio: /src/modules/eventos, /src/modules/usuarios, /src/modules/entradas, /src/modules/reportes
2. EACH Module SHALL contener sus propios componentes, hooks, services y types
3. THE Frontend SHALL tener una carpeta /src/shared para componentes y utilidades compartidas
4. THE Frontend SHALL usar barrel exports (index.ts) para exportar públicamente desde cada módulo
5. THE Frontend SHALL separar lógica de negocio de componentes UI usando custom hooks
6. THE Frontend SHALL usar TypeScript estricto para type safety
7. THE Frontend SHALL seguir convenciones de nombres consistentes (PascalCase para componentes, camelCase para funciones)

### Requirement 2: Autenticación con Keycloak (OIDC)

**User Story:** Como usuario, quiero autenticarme usando Keycloak, para acceder de forma segura a la aplicación.

#### Acceptance Criteria

1. THE Frontend SHALL usar react-oidc-context o oidc-client-ts para integración con Keycloak
2. WHEN un usuario no autenticado accede a una ruta protegida, THE Frontend SHALL redirigir al login de Keycloak
3. WHEN un usuario se autentica exitosamente, THE Frontend SHALL almacenar el token JWT
4. WHEN un usuario cierra sesión, THE Frontend SHALL limpiar el token y redirigir al login
5. THE Frontend SHALL renovar automáticamente el token antes de que expire
6. THE Frontend SHALL extraer roles del token JWT para control de acceso
7. THE Frontend SHALL configurar Keycloak con el realm "Kairo" y cliente "kairo-web"

### Requirement 3: Comunicación Exclusiva con Gateway

**User Story:** Como arquitecto de sistemas, quiero que el frontend solo se comunique con el Gateway, para mantener la arquitectura de microservicios correctamente.

#### Acceptance Criteria

1. THE Frontend SHALL comunicarse ÚNICAMENTE con el Gateway (http://localhost:8080)
2. THE Frontend SHALL NUNCA comunicarse directamente con microservicios individuales
3. WHEN el Frontend hace una petición, THE Frontend SHALL incluir el token JWT en el header Authorization
4. WHEN el Gateway retorna 401 Unauthorized, THE Frontend SHALL redirigir al login
5. WHEN el Gateway retorna 403 Forbidden, THE Frontend SHALL mostrar mensaje de permisos insuficientes
6. THE Frontend SHALL usar axios o fetch con interceptors para manejo centralizado de errores
7. THE Frontend SHALL configurar la URL del Gateway mediante variables de entorno

### Requirement 4: UI Library y Diseño

**User Story:** Como usuario, quiero una interfaz moderna y profesional, para tener una buena experiencia de uso.

#### Acceptance Criteria

1. THE Frontend SHALL usar Material UI (MUI) o Tailwind CSS como librería de componentes
2. THE Frontend SHALL tener un diseño responsive que funcione en desktop, tablet y móvil
3. THE Frontend SHALL usar un tema consistente con colores, tipografía y espaciado definidos
4. THE Frontend SHALL tener un layout principal con navbar, sidebar (opcional) y contenido
5. THE Frontend SHALL mostrar el nombre de usuario y rol en el navbar
6. THE Frontend SHALL tener un botón de logout visible en el navbar
7. THE Frontend SHALL usar iconos consistentes (Material Icons o Heroicons)

### Requirement 5: Pantalla de Login

**User Story:** Como usuario, quiero una pantalla de login clara, para autenticarme en el sistema.

#### Acceptance Criteria

1. WHEN un usuario no autenticado accede a la aplicación, THE Frontend SHALL mostrar la pantalla de login
2. THE Login Screen SHALL tener un botón "Iniciar Sesión con Keycloak"
3. WHEN el usuario hace clic en "Iniciar Sesión", THE Frontend SHALL redirigir a Keycloak
4. THE Login Screen SHALL mostrar el logo de la aplicación
5. THE Login Screen SHALL tener un diseño centrado y atractivo
6. WHEN la autenticación falla, THE Frontend SHALL mostrar un mensaje de error
7. WHEN la autenticación es exitosa, THE Frontend SHALL redirigir al Dashboard

### Requirement 6: Dashboard Principal

**User Story:** Como usuario autenticado, quiero ver un dashboard con información relevante, para tener una visión general del sistema.

#### Acceptance Criteria

1. WHEN un usuario autenticado accede a "/", THE Frontend SHALL mostrar el Dashboard
2. THE Dashboard SHALL mostrar tarjetas con estadísticas: Total de Eventos, Mis Entradas, Próximos Eventos
3. THE Dashboard SHALL mostrar una lista de eventos destacados
4. THE Dashboard SHALL tener navegación rápida a módulos principales (Eventos, Mis Entradas)
5. THE Dashboard SHALL mostrar información personalizada según el rol del usuario
6. WHEN el usuario es Admin, THE Dashboard SHALL mostrar estadísticas administrativas
7. WHEN el usuario es Organizator, THE Dashboard SHALL mostrar sus eventos creados

### Requirement 7: Módulo de Eventos

**User Story:** Como usuario, quiero ver y gestionar eventos, para explorar y organizar eventos.

#### Acceptance Criteria

1. THE Frontend SHALL tener una ruta "/eventos" que muestre lista de eventos
2. THE Lista de Eventos SHALL mostrar: nombre, fecha, ubicación, imagen (opcional)
3. THE Lista de Eventos SHALL tener filtros por fecha y ubicación
4. THE Lista de Eventos SHALL tener búsqueda por nombre
5. WHEN un usuario hace clic en un evento, THE Frontend SHALL mostrar detalles del evento
6. THE Detalle de Evento SHALL mostrar: información completa, mapa de asientos, botón "Comprar Entrada"
7. WHEN el usuario es Organizator o Admin, THE Frontend SHALL mostrar botones "Crear Evento", "Editar Evento", "Cancelar Evento"

### Requirement 8: Módulo de Compra de Entradas

**User Story:** Como usuario, quiero comprar entradas para eventos, para asistir a eventos de mi interés.

#### Acceptance Criteria

1. WHEN un usuario hace clic en "Comprar Entrada", THE Frontend SHALL mostrar el mapa de asientos
2. THE Mapa de Asientos SHALL mostrar asientos disponibles, reservados y ocupados con colores diferentes
3. WHEN un usuario selecciona un asiento, THE Frontend SHALL resaltar el asiento seleccionado
4. WHEN un usuario confirma la selección, THE Frontend SHALL crear una entrada en estado Reservada
5. THE Frontend SHALL mostrar un resumen de la compra: evento, asiento, precio
6. THE Frontend SHALL tener un botón "Proceder al Pago"
7. WHEN la entrada se crea exitosamente, THE Frontend SHALL mostrar mensaje de confirmación

### Requirement 9: Módulo de Mis Entradas

**User Story:** Como usuario, quiero ver mis entradas compradas, para gestionar mis asistencias a eventos.

#### Acceptance Criteria

1. THE Frontend SHALL tener una ruta "/mis-entradas" que muestre las entradas del usuario
2. THE Lista de Entradas SHALL mostrar: evento, asiento, estado, fecha de compra, precio
3. THE Lista de Entradas SHALL filtrar por estado: Todas, Reservadas, Pagadas, Canceladas
4. WHEN una entrada está en estado Reservada, THE Frontend SHALL mostrar botón "Pagar"
5. WHEN una entrada está en estado Reservada o Pagada, THE Frontend SHALL mostrar botón "Cancelar"
6. WHEN el usuario cancela una entrada, THE Frontend SHALL solicitar confirmación
7. THE Frontend SHALL mostrar el tiempo restante para pagar entradas reservadas (15 minutos)

### Requirement 10: Módulo de Usuarios (Admin)

**User Story:** Como administrador, quiero gestionar usuarios del sistema, para controlar accesos y permisos.

#### Acceptance Criteria

1. WHEN el usuario tiene rol Admin, THE Frontend SHALL mostrar opción "Usuarios" en el menú
2. THE Frontend SHALL tener una ruta "/usuarios" accesible solo para Admin
3. THE Lista de Usuarios SHALL mostrar: username, nombre, correo, rol, estado
4. THE Lista de Usuarios SHALL tener botones "Crear Usuario", "Editar Usuario", "Desactivar Usuario"
5. THE Formulario de Usuario SHALL validar: username único, correo válido, teléfono válido
6. WHEN se crea un usuario, THE Frontend SHALL enviar la petición al Gateway
7. WHEN la operación es exitosa, THE Frontend SHALL actualizar la lista de usuarios

### Requirement 11: Módulo de Reportes (Admin/Organizator)

**User Story:** Como administrador u organizador, quiero ver reportes del sistema, para analizar métricas y tomar decisiones.

#### Acceptance Criteria

1. WHEN el usuario tiene rol Admin o Organizator, THE Frontend SHALL mostrar opción "Reportes" en el menú
2. THE Frontend SHALL tener una ruta "/reportes" accesible solo para Admin y Organizator
3. THE Reportes SHALL mostrar: Métricas de Eventos, Historial de Asistencia, Conciliación Financiera
4. THE Reportes SHALL tener filtros por fecha y evento
5. THE Reportes SHALL mostrar gráficos visuales (charts) usando recharts o chart.js
6. THE Reportes SHALL tener botón "Exportar a PDF" o "Exportar a Excel"
7. WHEN se solicita un reporte, THE Frontend SHALL mostrar loading state

### Requirement 12: Manejo de Errores

**User Story:** Como usuario, quiero ver mensajes de error claros, para entender qué salió mal y cómo solucionarlo.

#### Acceptance Criteria

1. WHEN ocurre un error de red, THE Frontend SHALL mostrar mensaje "Error de conexión. Intente nuevamente."
2. WHEN el Gateway retorna 400, THE Frontend SHALL mostrar los errores de validación específicos
3. WHEN el Gateway retorna 401, THE Frontend SHALL redirigir al login
4. WHEN el Gateway retorna 403, THE Frontend SHALL mostrar "No tiene permisos para realizar esta acción"
5. WHEN el Gateway retorna 404, THE Frontend SHALL mostrar "Recurso no encontrado"
6. WHEN el Gateway retorna 500, THE Frontend SHALL mostrar "Error del servidor. Intente más tarde."
7. THE Frontend SHALL usar toast notifications o snackbars para mostrar errores

### Requirement 13: Loading States y UX

**User Story:** Como usuario, quiero feedback visual durante operaciones, para saber que el sistema está procesando mi petición.

#### Acceptance Criteria

1. WHEN se carga una lista, THE Frontend SHALL mostrar skeleton loaders o spinners
2. WHEN se envía un formulario, THE Frontend SHALL deshabilitar el botón y mostrar loading
3. WHEN se completa una operación exitosa, THE Frontend SHALL mostrar mensaje de éxito
4. THE Frontend SHALL usar transiciones suaves entre pantallas
5. THE Frontend SHALL mostrar progress indicators para operaciones largas
6. WHEN se carga una imagen, THE Frontend SHALL mostrar placeholder
7. THE Frontend SHALL tener estados vacíos informativos ("No hay eventos disponibles")

### Requirement 14: Validación de Formularios

**User Story:** Como usuario, quiero validación en tiempo real en formularios, para corregir errores antes de enviar.

#### Acceptance Criteria

1. THE Frontend SHALL usar react-hook-form o formik para manejo de formularios
2. THE Frontend SHALL validar campos requeridos antes de enviar
3. THE Frontend SHALL validar formato de correo electrónico
4. THE Frontend SHALL validar longitud mínima y máxima de campos
5. WHEN un campo es inválido, THE Frontend SHALL mostrar mensaje de error debajo del campo
6. WHEN un campo es válido, THE Frontend SHALL mostrar indicador visual de éxito
7. THE Frontend SHALL deshabilitar el botón de envío si el formulario es inválido

### Requirement 15: Routing y Navegación

**User Story:** Como usuario, quiero navegación intuitiva, para moverme fácilmente entre secciones.

#### Acceptance Criteria

1. THE Frontend SHALL usar react-router-dom para routing
2. THE Frontend SHALL tener rutas protegidas que requieren autenticación
3. THE Frontend SHALL tener rutas con control de acceso basado en roles
4. THE Frontend SHALL mantener el estado de navegación en la URL
5. THE Frontend SHALL tener breadcrumbs para navegación jerárquica
6. THE Frontend SHALL resaltar la opción activa en el menú de navegación
7. THE Frontend SHALL usar lazy loading para rutas no críticas

### Requirement 16: State Management

**User Story:** Como desarrollador, quiero gestión de estado predecible, para mantener consistencia en la aplicación.

#### Acceptance Criteria

1. THE Frontend SHALL usar React Context API o Zustand para estado global
2. THE Frontend SHALL mantener estado de autenticación global (usuario, token, roles)
3. THE Frontend SHALL usar React Query o SWR para caché de datos del servidor
4. THE Frontend SHALL invalidar caché cuando se modifican datos
5. THE Frontend SHALL persistir estado de autenticación en localStorage
6. THE Frontend SHALL limpiar estado al cerrar sesión
7. THE Frontend SHALL usar estado local para UI state (modals, drawers)

### Requirement 17: Testing

**User Story:** Como desarrollador, quiero tests automatizados, para garantizar la calidad del código.

#### Acceptance Criteria

1. THE Frontend SHALL tener tests unitarios para componentes críticos usando Vitest
2. THE Frontend SHALL tener tests de integración para flujos principales
3. THE Frontend SHALL usar React Testing Library para tests de componentes
4. THE Frontend SHALL mockear llamadas al Gateway en tests
5. THE Frontend SHALL alcanzar >70% de cobertura de código
6. THE Frontend SHALL tener tests para custom hooks
7. THE Frontend SHALL ejecutar tests en CI/CD pipeline

### Requirement 18: Configuración y Variables de Entorno

**User Story:** Como DevOps engineer, quiero configuración mediante variables de entorno, para facilitar despliegues en diferentes ambientes.

#### Acceptance Criteria

1. THE Frontend SHALL leer configuración desde variables de entorno (.env)
2. THE Frontend SHALL configurar VITE_GATEWAY_URL para URL del Gateway
3. THE Frontend SHALL configurar VITE_KEYCLOAK_URL para URL de Keycloak
4. THE Frontend SHALL configurar VITE_KEYCLOAK_REALM para realm de Keycloak
5. THE Frontend SHALL configurar VITE_KEYCLOAK_CLIENT_ID para cliente de Keycloak
6. THE Frontend SHALL tener archivos .env.development y .env.production
7. THE Frontend SHALL validar que variables requeridas estén presentes al iniciar

### Requirement 19: Dockerización

**User Story:** Como DevOps engineer, quiero que el frontend esté dockerizado, para facilitar el despliegue.

#### Acceptance Criteria

1. THE Frontend SHALL tener un Dockerfile optimizado con multi-stage build
2. THE Frontend SHALL usar nginx para servir archivos estáticos en producción
3. THE Frontend SHALL exponer el puerto 80
4. THE Frontend SHALL conectarse a la red kairo-network
5. THE Frontend SHALL tener configuración de nginx para SPA routing
6. THE Frontend SHALL minimizar el tamaño de la imagen final
7. THE Frontend SHALL tener docker-compose.yml para desarrollo local

### Requirement 20: Accesibilidad y SEO

**User Story:** Como usuario con discapacidad, quiero que la aplicación sea accesible, para poder usarla sin barreras.

#### Acceptance Criteria

1. THE Frontend SHALL usar etiquetas HTML semánticas (header, nav, main, footer)
2. THE Frontend SHALL tener atributos alt en todas las imágenes
3. THE Frontend SHALL tener labels asociados a inputs de formularios
4. THE Frontend SHALL ser navegable completamente con teclado
5. THE Frontend SHALL tener contraste de colores accesible (WCAG AA)
6. THE Frontend SHALL usar aria-labels para elementos interactivos
7. THE Frontend SHALL tener meta tags apropiados para SEO

