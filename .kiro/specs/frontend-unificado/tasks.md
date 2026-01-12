# Implementation Plan: Frontend Unificado

## Overview

Este plan de implementación cubre la creación de un frontend unificado profesional usando React + Vite + TypeScript que reemplaza Frontend1 (React + JavaScript + Keycloak básico) y Frontend2 (React + TypeScript + MUI incompleto). El nuevo frontend se integrará exclusivamente con el Gateway y proporcionará una experiencia de usuario moderna, accesible y completa.

## Tasks

- [x] 1. Configurar proyecto base y estructura modular
  - Crear directorio `frontend-unificado/` en la raíz del proyecto
  - Inicializar proyecto Vite con template React + TypeScript
  - Configurar estructura de directorios modular: `/src/modules/{eventos,usuarios,entradas,reportes}`, `/src/shared`, `/src/context`, `/src/layouts`, `/src/routes`
  - Configurar alias de TypeScript para imports limpios (@/, @modules/, @shared/)
  - Configurar ESLint y Prettier para consistencia de código
  - Crear archivos `.env.development` y `.env.production` con variables requeridas
  - Implementar validación de variables de entorno en startup
  - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.6, 18.1, 18.2, 18.3, 18.4, 18.5, 18.6, 18.7_

- [x] 1.1 Escribir test de propiedad para validación de variables de entorno

  - **Property 16: Variables de Entorno Requeridas**
  - **Validates: Requirements 18.7**

- [x] 2. Implementar autenticación con Keycloak (OIDC)
  - Instalar dependencias: `react-oidc-context`, `oidc-client-ts`
  - Crear `AuthContext.tsx` con configuración OIDC para Keycloak
  - Configurar realm "Kairo" y cliente "kairo-web"
  - Implementar renovación automática de tokens
  - Implementar extracción de roles del JWT
  - Crear hook `useAuth()` para acceso al contexto de autenticación
  - Implementar limpieza de estado al cerrar sesión
  - _Requirements: 2.1, 2.3, 2.4, 2.5, 2.6, 2.7_

- [x] 2.1 Escribir test de propiedad para autenticación requerida en rutas protegidas
  - **Property 1: Autenticación Requerida para Rutas Protegidas**
  - **Validates: Requirements 2.2, 15.2**

- [x] 2.2 Escribir test de propiedad para token JWT en peticiones
  - **Property 2: Token JWT en Todas las Peticiones Autenticadas**
  - **Validates: Requirements 3.3**

- [x] 2.3 Escribir test de propiedad para renovación automática de token
  - **Property 3: Renovación Automática de Token**
  - **Validates: Requirements 2.5**

- [x] 2.4 Escribir test de propiedad para limpieza de estado al cerrar sesión
  - **Property 5: Limpieza de Estado al Cerrar Sesión**
  - **Validates: Requirements 2.4, 16.6**

- [x] 3. Configurar comunicación con Gateway
  - Crear cliente Axios configurado con baseURL del Gateway
  - Implementar request interceptor para agregar token JWT en header Authorization
  - Implementar response interceptor para manejo de errores HTTP (401, 403, 404, 400, 500)
  - Configurar redirección automática a login en 401
  - Configurar mensajes de error apropiados por código HTTP
  - Implementar retry logic con backoff exponencial
  - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5, 3.6, 3.7, 12.1, 12.2, 12.3, 12.4, 12.5, 12.6_

- [x] 3.1 Escribir test de propiedad para comunicación exclusiva con Gateway

  - **Property 15: Comunicación Exclusiva con Gateway**
  - **Validates: Requirements 3.1, 3.2**

- [x] 3.2 Escribir test de propiedad para manejo de 401 Unauthorized

  - **Property 6: Manejo de Respuestas 401 Unauthorized**
  - **Validates: Requirements 3.4**


- [x] 3.3 Escribir test de propiedad para manejo de 403 Forbidden

  - **Property 7: Manejo de Respuestas 403 Forbidden**
  - **Validates: Requirements 3.5**

- [x] 4. Configurar React Query y gestión de estado
  - Instalar `@tanstack/react-query`
  - Configurar QueryClient con opciones por defecto (staleTime, cacheTime, retry)
  - Crear QueryClientProvider en App
  - Implementar invalidación de caché al modificar datos
  - Configurar persistencia de autenticación en localStorage
  - Implementar limpieza de estado global al cerrar sesión
  - _Requirements: 16.1, 16.2, 16.3, 16.4, 16.5, 16.6, 16.7_

- [x] 4.1 Escribir test de propiedad para invalidación de caché

  - **Property 13: Invalidación de Caché al Modificar Datos**
  - **Validates: Requirements 16.4**

- [x] 4.2 Escribir test de propiedad para persistencia de autenticación

  - **Property 14: Persistencia de Autenticación**
  - **Validates: Requirements 16.5**

- [x] 5. Implementar UI library y tema
  - Instalar Material UI: `@mui/material`, `@mui/icons-material`, `@emotion/react`, `@emotion/styled`
  - Crear tema personalizado con paleta de colores, tipografía y espaciado
  - Configurar ThemeProvider en App
  - Implementar diseño responsive con breakpoints
  - Configurar componentes MUI con estilos personalizados
  - _Requirements: 4.1, 4.2, 4.3, 4.7_

- [ ]* 5.1 Escribir test de propiedad para diseño responsive
  - **Property 17: Responsive Design**
  - **Validates: Requirements 4.2**

- [x] 6. Implementar routing y navegación
  - Instalar `react-router-dom`
  - Crear componente `ProtectedRoute` para rutas que requieren autenticación
  - Crear componente `RoleBasedRoute` para control de acceso basado en roles
  - Configurar rutas principales: /, /login, /eventos, /mis-entradas, /usuarios, /reportes
  - Implementar lazy loading para rutas no críticas
  - Implementar redirección a login para usuarios no autenticados
  - _Requirements: 15.1, 15.2, 15.3, 15.4, 15.7, 2.2_

- [x] 6.1 Escribir test de propiedad para control de acceso basado en roles

  - **Property 4: Control de Acceso Basado en Roles**
  - **Validates: Requirements 15.3**

- [x] 7. Implementar layouts y componentes compartidos
  - Crear `MainLayout` con navbar, sidebar opcional y área de contenido
  - Crear `AuthLayout` para pantallas de autenticación
  - Crear componente `Navbar` con logo, nombre de usuario, rol y botón logout
  - Crear componente `Sidebar` con menú de navegación basado en roles
  - Crear componentes compartidos: `LoadingSpinner`, `ErrorMessage`, `EmptyState`, `Button`, `TextField`
  - Implementar componente `Toast` para notificaciones usando MUI Snackbar
  - _Requirements: 4.4, 4.5, 4.6, 12.7, 13.1, 13.2, 13.3_

- [ ]* 7.1 Escribir tests unitarios para componentes compartidos
  - Test LoadingSpinner con diferentes tamaños
  - Test ErrorMessage con botón retry
  - Test EmptyState con acción opcional
  - _Requirements: 13.1, 13.7_

- [x] 8. Implementar pantalla de login
  - Crear `LoginPage` con diseño centrado y atractivo
  - Implementar botón "Iniciar Sesión con Keycloak"
  - Implementar redirección a Keycloak al hacer clic
  - Mostrar logo de la aplicación
  - Implementar manejo de errores de autenticación
  - Implementar redirección a Dashboard tras autenticación exitosa
  - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5, 5.6, 5.7_

- [x] 8.1 Escribir tests unitarios para LoginPage

  - Test renderizado de botón de login
  - Test redirección a Keycloak
  - Test manejo de errores
  - _Requirements: 5.1, 5.6_

- [x] 9. Implementar Dashboard principal
  - Crear `Dashboard` con tarjetas de estadísticas: Total de Eventos, Mis Entradas, Próximos Eventos
  - Implementar lista de eventos destacados
  - Implementar navegación rápida a módulos principales
  - Implementar personalización según rol de usuario (Admin vs Organizator vs Asistente)
  - Crear hook `useDashboardStats()` para obtener estadísticas
  - _Requirements: 6.1, 6.2, 6.3, 6.4, 6.5, 6.6, 6.7_

- [ ]* 9.1 Escribir tests unitarios para Dashboard
  - Test renderizado de tarjetas de estadísticas
  - Test personalización por rol
  - Test navegación rápida
  - _Requirements: 6.1, 6.5_

- [x] 10. Implementar módulo de Eventos - Servicios y Hooks
  - Crear `eventosService.ts` con funciones: `fetchEventos()`, `fetchEvento(id)`, `createEvento()`, `updateEvento()`, `cancelEvento()`
  - Crear hook `useEventos()` para listar eventos con React Query
  - Crear hook `useEvento(id)` para obtener detalle de evento
  - Crear hook `useCreateEvento()` para crear evento (Admin/Organizator)
  - Crear hook `useUpdateEvento()` para actualizar evento
  - Crear hook `useCancelarEvento()` para cancelar evento
  - _Requirements: 7.1, 7.7_

- [x] 11. Implementar módulo de Eventos - Componentes UI
  - Crear `EventosPage` con lista de eventos
  - Crear componente `EventosList` con tarjetas de eventos
  - Implementar filtros por fecha y ubicación
  - Implementar búsqueda por nombre
  - Crear `EventoDetailPage` con información completa del evento
  - Implementar botones "Crear Evento", "Editar Evento", "Cancelar Evento" para Admin/Organizator
  - Implementar botón "Comprar Entrada" para todos los usuarios
  - _Requirements: 7.1, 7.2, 7.3, 7.4, 7.5, 7.6, 7.7_

- [x] 11.1 Escribir tests unitarios para componentes de Eventos

  - Test EventosList renderiza lista correctamente
  - Test filtros funcionan correctamente
  - Test búsqueda funciona correctamente
  - Test botones visibles según rol
  - _Requirements: 7.1, 7.2, 7.3, 7.4, 7.7_

- [x] 12. Implementar módulo de Entradas - Servicios y Hooks
  - Crear `entradasService.ts` con funciones: `fetchMisEntradas()`, `fetchAsientosDisponibles(eventoId)`, `createEntrada()`, `cancelarEntrada(id)`
  - Crear hook `useMisEntradas(filtro)` para listar entradas del usuario
  - Crear hook `useAsientosDisponibles(eventoId)` para obtener mapa de asientos
  - Crear hook `useCreateEntrada()` para crear entrada
  - Crear hook `useCancelarEntrada()` para cancelar entrada
  - _Requirements: 8.1, 8.2, 9.1_

- [x] 13. Implementar módulo de Entradas - Componentes UI
  - Crear `MisEntradasPage` con lista de entradas del usuario
  - Crear componente `EntradaCard` mostrando: evento, asiento, estado, fecha, precio
  - Implementar filtros por estado: Todas, Reservadas, Pagadas, Canceladas
  - Implementar botones "Pagar" y "Cancelar" según estado
  - Crear `ComprarEntradaPage` con mapa de asientos
  - Crear componente `MapaAsientos` mostrando asientos disponibles/reservados/ocupados con colores
  - Implementar selección de asiento y confirmación de compra
  - Implementar contador de tiempo restante para pagar (15 minutos)
  - _Requirements: 8.1, 8.2, 8.3, 8.4, 8.5, 8.6, 8.7, 9.1, 9.2, 9.3, 9.4, 9.5, 9.6, 9.7_

- [ ]* 13.1 Escribir tests unitarios para componentes de Entradas
  - Test MisEntradasPage renderiza lista correctamente
  - Test filtros por estado funcionan
  - Test botones visibles según estado
  - Test MapaAsientos muestra estados correctamente
  - _Requirements: 9.1, 9.2, 9.3, 9.4_

- [x] 14. Implementar módulo de Usuarios (Admin) - Servicios y Hooks
  - Crear `usuariosService.ts` con funciones: `fetchUsuarios()`, `fetchUsuario(id)`, `createUsuario()`, `updateUsuario()`, `deactivateUsuario()`
  - Crear hook `useUsuarios()` para listar usuarios
  - Crear hook `useUsuario(id)` para obtener detalle de usuario
  - Crear hook `useCreateUsuario()` para crear usuario
  - Crear hook `useUpdateUsuario()` para actualizar usuario
  - Crear hook `useDeactivateUsuario()` para desactivar usuario
  - _Requirements: 10.1, 10.2, 10.6_

- [x] 15. Implementar módulo de Usuarios (Admin) - Componentes UI
  - Crear `UsuariosPage` accesible solo para Admin
  - Crear componente `UsuariosList` con tabla de usuarios
  - Implementar botones "Crear Usuario", "Editar Usuario", "Desactivar Usuario"
  - Crear componente `UsuarioForm` con validación
  - Implementar validación: username único, correo válido, teléfono válido
  - Mostrar opción "Usuarios" en menú solo para Admin
  - _Requirements: 10.1, 10.2, 10.3, 10.4, 10.5, 10.6, 10.7_

- [ ]* 15.1 Escribir tests unitarios para componentes de Usuarios
  - Test UsuariosPage solo accesible para Admin
  - Test UsuariosList renderiza tabla correctamente
  - Test UsuarioForm valida campos correctamente
  - _Requirements: 10.2, 10.4, 10.5_

- [x] 16. Implementar módulo de Reportes - Servicios y Hooks
  - Crear `reportesService.ts` con funciones: `fetchMetricasEventos()`, `fetchHistorialAsistencia()`, `fetchConciliacionFinanciera()`, `exportarReporte()`
  - Crear hook `useMetricasEventos(filtros)` para obtener métricas
  - Crear hook `useHistorialAsistencia(filtros)` para obtener historial
  - Crear hook `useConciliacionFinanciera(filtros)` para obtener conciliación
  - _Requirements: 11.1, 11.2, 11.3_

- [x] 17. Implementar módulo de Reportes - Componentes UI
  - Instalar librería de gráficos: `recharts` o `chart.js`
  - Crear `ReportesPage` accesible solo para Admin y Organizator
  - Crear componente `MetricasEventos` con gráficos visuales
  - Crear componente `HistorialAsistencia` con tabla y gráficos
  - Crear componente `ConciliacionFinanciera` con resumen financiero
  - Implementar filtros por fecha y evento
  - Implementar botón "Exportar a PDF" o "Exportar a Excel"
  - Implementar loading state durante carga de reportes
  - Mostrar opción "Reportes" en menú solo para Admin y Organizator
  - _Requirements: 11.1, 11.2, 11.3, 11.4, 11.5, 11.6, 11.7_

- [ ]* 17.1 Escribir tests unitarios para componentes de Reportes
  - Test ReportesPage solo accesible para Admin y Organizator
  - Test filtros funcionan correctamente
  - Test loading state se muestra correctamente
  - _Requirements: 11.2, 11.4, 11.7_

- [x] 18. Implementar validación de formularios
  - Instalar `react-hook-form` y `zod`
  - Crear schemas de validación con Zod: `eventoSchema`, `usuarioSchema`, `entradaSchema`
  - Implementar validación de campos requeridos
  - Implementar validación de formato de correo electrónico
  - Implementar validación de longitud mínima y máxima
  - Implementar mensajes de error específicos por campo
  - Implementar indicadores visuales de validación (error/éxito)
  - Deshabilitar botón de envío si formulario es inválido
  - _Requirements: 14.1, 14.2, 14.3, 14.4, 14.5, 14.6, 14.7_

- [ ]* 18.1 Escribir test de propiedad para validación de campos requeridos
  - **Property 8: Validación de Campos Requeridos**
  - **Validates: Requirements 14.2, 14.7**

- [ ]* 18.2 Escribir test de propiedad para validación de formato de correo
  - **Property 9: Validación de Formato de Correo**
  - **Validates: Requirements 14.3**

- [ ]* 18.3 Escribir test de propiedad para mensajes de error de validación
  - **Property 10: Mensajes de Error de Validación**
  - **Validates: Requirements 14.5**

- [x] 19. Implementar loading states y feedback UX
  - Implementar skeleton loaders para listas usando MUI Skeleton
  - Implementar loading state en botones de formularios
  - Implementar mensajes de éxito con toast notifications
  - Implementar transiciones suaves entre pantallas
  - Implementar progress indicators para operaciones largas
  - Implementar placeholders para imágenes
  - Implementar estados vacíos informativos para listas vacías
  - _Requirements: 13.1, 13.2, 13.3, 13.4, 13.5, 13.6, 13.7_

- [ ]* 19.1 Escribir test de propiedad para loading state durante operaciones
  - **Property 11: Loading State Durante Operaciones**
  - **Validates: Requirements 13.1, 13.2**

- [ ]* 19.2 Escribir test de propiedad para feedback de operaciones exitosas
  - **Property 12: Feedback de Operaciones Exitosas**
  - **Validates: Requirements 13.3**

- [x] 20. Implementar accesibilidad (A11y)
  - Usar etiquetas HTML semánticas: `<header>`, `<nav>`, `<main>`, `<footer>`, `<article>`, `<section>`
  - Agregar atributos `alt` a todas las imágenes
  - Asociar `<label>` a todos los inputs mediante `htmlFor`
  - Implementar navegación completa con teclado (Tab, Enter, Escape)
  - Verificar contraste de colores WCAG AA (4.5:1 mínimo)
  - Agregar `aria-label` a elementos interactivos sin texto
  - Implementar focus trap en modals
  - Agregar skip link para saltar al contenido principal
  - _Requirements: 20.1, 20.2, 20.3, 20.4, 20.5, 20.6_

- [ ]* 20.1 Escribir test de propiedad para navegación con teclado
  - **Property 18: Navegación con Teclado**
  - **Validates: Requirements 20.4**

- [ ]* 20.2 Escribir test de propiedad para atributos alt en imágenes
  - **Property 19: Atributos Alt en Imágenes**
  - **Validates: Requirements 20.2**

- [ ]* 20.3 Escribir test de propiedad para labels asociados a inputs
  - **Property 20: Labels Asociados a Inputs**
  - **Validates: Requirements 20.3**

- [x] 21. Checkpoint - Verificar funcionalidad completa
  - Verificar que todos los módulos funcionan correctamente
  - Verificar que la autenticación con Keycloak funciona
  - Verificar que todas las rutas protegidas requieren autenticación
  - Verificar que el control de acceso basado en roles funciona
  - Verificar que la comunicación con Gateway funciona
  - Verificar que los formularios validan correctamente
  - Verificar que los loading states se muestran apropiadamente
  - Verificar que los mensajes de error son claros
  - Preguntar al usuario si hay problemas o ajustes necesarios

- [x] 22. Implementar dockerización
  - Crear `Dockerfile` con multi-stage build (builder + nginx)
  - Configurar nginx para servir archivos estáticos
  - Configurar nginx para SPA routing (todas las rutas sirven index.html)
  - Configurar compresión gzip en nginx
  - Configurar cache de assets estáticos
  - Configurar security headers en nginx
  - Exponer puerto 80
  - Crear `docker-compose.yml` para desarrollo local
  - Configurar conexión a red `kairo-network`
  - Minimizar tamaño de imagen final
  - _Requirements: 19.1, 19.2, 19.3, 19.4, 19.5, 19.6, 19.7_

- [x] 22.1 Verificar que la imagen Docker se construye correctamente

  - Test construcción de imagen
  - Test tamaño de imagen es razonable (<100MB)
  - Test nginx sirve archivos correctamente
  - _Requirements: 19.1, 19.6_

- [x] 23. Configurar testing framework
  - Configurar Vitest con coverage
  - Configurar React Testing Library
  - Configurar MSW (Mock Service Worker) para mocking de API
  - Instalar `fast-check` para property-based testing
  - Configurar threshold de cobertura >70%
  - Crear mocks de handlers para Gateway endpoints
  - Configurar scripts de test en package.json
  - _Requirements: 17.1, 17.2, 17.3, 17.4, 17.5_


- [x]* 23.1 Escribir tests de integración para flujos principales
  - Test flujo completo: Login → Ver Eventos → Comprar Entrada
  - Test flujo: Login Admin → Gestionar Usuarios
  - Test flujo: Login Organizator → Ver Reportes
  - _Requirements: 17.2_

- [x] 24. Implementar optimizaciones de performance
  - Implementar code splitting con lazy loading de rutas
  - Implementar lazy loading de imágenes con atributo `loading="lazy"`
  - Implementar prefetching de datos con React Query
  - Implementar optimistic updates en mutations
  - Implementar memoization con `memo`, `useMemo`, `useCallback` en componentes pesados
  - Configurar manual chunks en Vite para vendor splitting
  - _Requirements: 15.7_

- [ ]* 24.1 Verificar métricas de performance
  - Medir Web Vitals (CLS, FID, FCP, LCP, TTFB)
  - Verificar que bundle size es razonable
  - Verificar que lazy loading funciona
  - _Requirements: 15.7_

- [x] 25. Checkpoint final - Verificación completa
  - Ejecutar todos los tests y verificar que pasan
  - Verificar cobertura de código >70%
  - Verificar que la aplicación se construye sin errores
  - Verificar que la imagen Docker funciona correctamente
  - Verificar que todas las variables de entorno están documentadas
  - Verificar que el README está actualizado con instrucciones
  - Realizar pruebas manuales de todos los flujos principales
  - Preguntar al usuario si está listo para despliegue

## Notes

- Las tareas marcadas con `*` son opcionales y pueden omitirse para un MVP más rápido
- Cada tarea referencia requisitos específicos para trazabilidad
- Los checkpoints aseguran validación incremental
- Los property tests validan propiedades de correctness universales
- Los unit tests validan ejemplos específicos y casos edge
- La implementación debe ser incremental: cada tarea construye sobre las anteriores
- El frontend debe comunicarse ÚNICAMENTE con el Gateway (puerto 8080), nunca directamente con microservicios
- Usar TypeScript estricto para type safety
- Seguir convenciones de nombres: PascalCase para componentes, camelCase para funciones
- Priorizar accesibilidad y UX en todos los componentes
