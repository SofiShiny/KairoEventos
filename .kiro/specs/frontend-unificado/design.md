# Design Document - Frontend Unificado

## Overview

El Frontend Unificado es una aplicación web moderna construida con React + Vite + TypeScript que reemplaza los dos frontends incompletos existentes (Frontend1 y Frontend2). Esta aplicación proporciona una interfaz de usuario profesional, consistente y escalable para el sistema de gestión de eventos Kairo.

### Objetivos Principales

1. **Unificación**: Consolidar funcionalidades dispersas en Frontend1 y Frontend2 en una única aplicación coherente
2. **Arquitectura Moderna**: Utilizar tecnologías actuales (React 18+, Vite, TypeScript) para desarrollo rápido y mantenible
3. **Integración Gateway**: Comunicación exclusiva con el Gateway implementado en FASE 2
4. **Autenticación Centralizada**: Integración con Keycloak usando OIDC para gestión de identidad
5. **Experiencia de Usuario**: Interfaz responsive, accesible y con feedback visual apropiado

### Decisiones de Diseño Clave

**Stack Tecnológico**:
- **React 18+**: Framework UI con hooks modernos y concurrent features
- **Vite**: Build tool rápido con HMR instantáneo, mejor que Create React App
- **TypeScript**: Type safety para reducir errores en tiempo de desarrollo
- **Material UI (MUI)**: Librería de componentes robusta con theming y accesibilidad integrada

**Rationale**: Este stack proporciona desarrollo rápido, excelente DX, y componentes profesionales out-of-the-box.

**Arquitectura de Comunicación**:
- Comunicación exclusiva con Gateway (puerto 8080)
- Sin comunicación directa con microservicios
- Axios con interceptors para manejo centralizado de autenticación y errores

**Rationale**: Mantiene la arquitectura de microservicios correctamente, simplifica configuración y proporciona punto único de control.

## Architecture

### Estructura de Directorios

```
frontend-unificado/
├── src/
│   ├── modules/              # Módulos por dominio
│   │   ├── eventos/
│   │   │   ├── components/
│   │   │   ├── hooks/
│   │   │   ├── services/
│   │   │   ├── types/
│   │   │   └── index.ts
│   │   ├── usuarios/
│   │   ├── entradas/
│   │   └── reportes/
│   ├── shared/               # Código compartido
│   │   ├── components/       # Componentes reutilizables
│   │   ├── hooks/            # Custom hooks compartidos
│   │   ├── utils/            # Utilidades
│   │   ├── types/            # Tipos TypeScript globales
│   │   └── api/              # Cliente API y configuración
│   ├── context/              # React Context providers
│   │   ├── AuthContext.tsx
│   │   └── ThemeContext.tsx
│   ├── layouts/              # Layouts de página
│   │   ├── MainLayout.tsx
│   │   └── AuthLayout.tsx
│   ├── routes/               # Configuración de rutas
│   │   ├── AppRoutes.tsx
│   │   ├── ProtectedRoute.tsx
│   │   └── RoleBasedRoute.tsx
│   ├── App.tsx
│   └── main.tsx
├── public/
├── .env.development
├── .env.production
├── vite.config.ts
├── tsconfig.json
└── package.json
```


**Rationale de Arquitectura Modular**:
- Cada módulo encapsula su dominio completo (eventos, usuarios, entradas, reportes)
- Barrel exports (index.ts) controlan qué se expone públicamente
- Separación clara entre lógica de negocio (hooks/services) y UI (components)
- Facilita testing, mantenimiento y escalabilidad

### Flujo de Datos

```
Usuario → Componente UI → Custom Hook → API Service → Axios Interceptor → Gateway → Microservicio
                                                              ↓
                                                         Auth Token
                                                         Error Handling
```

### Capas de la Aplicación

1. **Capa de Presentación**: Componentes React (UI)
2. **Capa de Lógica**: Custom hooks (business logic)
3. **Capa de Servicios**: API services (comunicación con Gateway)
4. **Capa de Estado**: React Context + React Query (state management)
5. **Capa de Infraestructura**: Axios interceptors, configuración

## Components and Interfaces

### Módulo de Autenticación

#### AuthContext

```typescript
interface AuthContextType {
  user: User | null;
  token: string | null;
  roles: string[];
  isAuthenticated: boolean;
  login: () => Promise<void>;
  logout: () => void;
  hasRole: (role: string) => boolean;
}

interface User {
  id: string;
  username: string;
  email: string;
  nombre: string;
  roles: string[];
}
```

**Implementación**: Usa `react-oidc-context` para integración con Keycloak.

#### ProtectedRoute Component

```typescript
interface ProtectedRouteProps {
  children: React.ReactNode;
  requiredRoles?: string[];
}

// Redirige a login si no autenticado
// Muestra 403 si no tiene roles requeridos
```

### Módulo de Eventos

#### EventosList Component

```typescript
interface EventosListProps {
  filters?: EventoFilters;
  onEventoClick: (eventoId: string) => void;
}

interface EventoFilters {
  fecha?: Date;
  ubicacion?: string;
  busqueda?: string;
}
```

#### EventoDetail Component

```typescript
interface EventoDetailProps {
  eventoId: string;
}

interface Evento {
  id: string;
  nombre: string;
  descripcion: string;
  fecha: Date;
  ubicacion: string;
  imagenUrl?: string;
  estado: 'Publicado' | 'Cancelado';
  capacidadTotal: number;
  asientosDisponibles: number;
}
```

#### useEventos Hook

```typescript
function useEventos() {
  return {
    eventos: Evento[];
    isLoading: boolean;
    error: Error | null;
    refetch: () => void;
  };
}

function useEvento(id: string) {
  return {
    evento: Evento | null;
    isLoading: boolean;
    error: Error | null;
  };
}

function useCreateEvento() {
  return {
    createEvento: (data: CreateEventoDto) => Promise<Evento>;
    isCreating: boolean;
    error: Error | null;
  };
}
```

### Módulo de Entradas

#### MapaAsientos Component

```typescript
interface MapaAsientosProps {
  eventoId: string;
  onAsientoSelect: (asientoId: string) => void;
}

interface Asiento {
  id: string;
  fila: string;
  numero: number;
  estado: 'Disponible' | 'Reservado' | 'Ocupado';
  precio: number;
}
```

#### MisEntradas Component

```typescript
interface MisEntradasProps {
  filtroEstado?: EstadoEntrada;
}

interface Entrada {
  id: string;
  eventoId: string;
  eventoNombre: string;
  asientoId: string;
  asientoInfo: string;
  estado: 'Reservada' | 'Pagada' | 'Cancelada';
  precio: number;
  fechaCompra: Date;
  tiempoRestante?: number; // minutos para pagar
}
```

#### useEntradas Hook

```typescript
function useMisEntradas(filtro?: EstadoEntrada) {
  return {
    entradas: Entrada[];
    isLoading: boolean;
    error: Error | null;
  };
}

function useCreateEntrada() {
  return {
    createEntrada: (data: CreateEntradaDto) => Promise<Entrada>;
    isCreating: boolean;
    error: Error | null;
  };
}

function useCancelarEntrada() {
  return {
    cancelarEntrada: (entradaId: string) => Promise<void>;
    isCanceling: boolean;
    error: Error | null;
  };
}
```

### Módulo de Usuarios

#### UsuariosList Component (Admin only)

```typescript
interface UsuariosListProps {
  onUsuarioClick: (usuarioId: string) => void;
}

interface Usuario {
  id: string;
  username: string;
  nombre: string;
  correo: string;
  telefono: string;
  rol: 'Admin' | 'Organizator' | 'Asistente';
  activo: boolean;
}
```

#### UsuarioForm Component

```typescript
interface UsuarioFormProps {
  usuario?: Usuario; // undefined para crear, definido para editar
  onSubmit: (data: UsuarioFormData) => Promise<void>;
  onCancel: () => void;
}

interface UsuarioFormData {
  username: string;
  nombre: string;
  correo: string;
  telefono: string;
  rol: string;
}
```

### Módulo de Reportes

#### ReportesMetricas Component

```typescript
interface ReportesMetricasProps {
  filtros: ReporteFiltros;
}

interface ReporteFiltros {
  fechaInicio: Date;
  fechaFin: Date;
  eventoId?: string;
}

interface MetricasEvento {
  eventoId: string;
  eventoNombre: string;
  totalAsientos: number;
  asientosVendidos: number;
  ingresoTotal: number;
  tasaOcupacion: number;
}
```

### Shared Components

#### Layout Components

```typescript
// MainLayout: navbar + sidebar + content
interface MainLayoutProps {
  children: React.ReactNode;
}

// Navbar: logo, user menu, logout
interface NavbarProps {
  user: User;
  onLogout: () => void;
}

// Sidebar: navigation menu
interface SidebarProps {
  items: MenuItem[];
}

interface MenuItem {
  label: string;
  path: string;
  icon: React.ReactNode;
  requiredRoles?: string[];
}
```

#### Form Components

```typescript
// TextField wrapper con validación
interface TextFieldProps {
  name: string;
  label: string;
  type?: 'text' | 'email' | 'tel' | 'password';
  required?: boolean;
  validation?: ValidationRule[];
}

// Button con loading state
interface ButtonProps {
  children: React.ReactNode;
  onClick?: () => void;
  loading?: boolean;
  disabled?: boolean;
  variant?: 'contained' | 'outlined' | 'text';
}
```

#### Feedback Components

```typescript
// Toast notifications
interface ToastProps {
  message: string;
  severity: 'success' | 'error' | 'warning' | 'info';
  duration?: number;
}

// Loading spinner
interface LoadingSpinnerProps {
  size?: 'small' | 'medium' | 'large';
  fullScreen?: boolean;
}

// Error message
interface ErrorMessageProps {
  error: Error | string;
  onRetry?: () => void;
}
```

## Data Models

### API Response Types

```typescript
// Respuesta estándar del Gateway
interface ApiResponse<T> {
  data: T;
  success: boolean;
  message?: string;
}

// Respuesta de error
interface ApiError {
  message: string;
  errors?: Record<string, string[]>; // Errores de validación
  statusCode: number;
}

// Respuesta paginada
interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}
```

### DTOs (Data Transfer Objects)

```typescript
// Crear Evento
interface CreateEventoDto {
  nombre: string;
  descripcion: string;
  fecha: string; // ISO 8601
  ubicacion: string;
  imagenUrl?: string;
}

// Crear Entrada
interface CreateEntradaDto {
  eventoId: string;
  asientoId: string;
  usuarioId: string;
}

// Crear Usuario
interface CreateUsuarioDto {
  username: string;
  nombre: string;
  correo: string;
  telefono: string;
  rol: string;
  password: string;
}

// Actualizar Usuario
interface UpdateUsuarioDto {
  nombre?: string;
  correo?: string;
  telefono?: string;
  rol?: string;
}
```

### Form Validation Schemas

```typescript
// Usando react-hook-form + zod
import { z } from 'zod';

const eventoSchema = z.object({
  nombre: z.string().min(3, 'Mínimo 3 caracteres').max(100),
  descripcion: z.string().min(10, 'Mínimo 10 caracteres'),
  fecha: z.date().min(new Date(), 'Fecha debe ser futura'),
  ubicacion: z.string().min(3, 'Ubicación requerida'),
});

const usuarioSchema = z.object({
  username: z.string().min(3).max(50),
  nombre: z.string().min(3).max(100),
  correo: z.string().email('Correo inválido'),
  telefono: z.string().regex(/^\+?[0-9]{10,15}$/, 'Teléfono inválido'),
  rol: z.enum(['Admin', 'Organizator', 'Asistente']),
});
```


## Correctness Properties

*A property is a characteristic or behavior that should hold true across all valid executions of a system—essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.*

### Property 1: Autenticación Requerida para Rutas Protegidas

*For any* ruta protegida y cualquier usuario no autenticado, cuando el usuario intenta acceder a la ruta, el sistema debe redirigir al login de Keycloak.

**Validates: Requirements 2.2, 15.2**

### Property 2: Token JWT en Todas las Peticiones Autenticadas

*For any* petición HTTP al Gateway desde un usuario autenticado, el header Authorization debe contener el token JWT en formato "Bearer {token}".

**Validates: Requirements 3.3**

### Property 3: Renovación Automática de Token

*For any* token JWT que esté próximo a expirar (dentro de 5 minutos), el sistema debe renovar automáticamente el token antes de que expire.

**Validates: Requirements 2.5**

### Property 4: Control de Acceso Basado en Roles

*For any* ruta con roles requeridos y cualquier usuario autenticado, el usuario solo puede acceder si tiene al menos uno de los roles requeridos.

**Validates: Requirements 15.3**

### Property 5: Limpieza de Estado al Cerrar Sesión

*For any* usuario autenticado, cuando cierra sesión, el sistema debe limpiar completamente el token, información de usuario y estado de autenticación del localStorage y memoria.

**Validates: Requirements 2.4, 16.6**

### Property 6: Manejo de Respuestas 401 Unauthorized

*For any* petición HTTP que retorne 401 Unauthorized, el sistema debe redirigir automáticamente al login y limpiar el estado de autenticación.

**Validates: Requirements 3.4**

### Property 7: Manejo de Respuestas 403 Forbidden

*For any* petición HTTP que retorne 403 Forbidden, el sistema debe mostrar un mensaje "No tiene permisos para realizar esta acción" sin redirigir.

**Validates: Requirements 3.5**

### Property 8: Validación de Campos Requeridos

*For any* formulario con campos marcados como requeridos, el botón de envío debe estar deshabilitado hasta que todos los campos requeridos sean válidos.

**Validates: Requirements 14.2, 14.7**

### Property 9: Validación de Formato de Correo

*For any* campo de correo electrónico en un formulario, el sistema debe validar que el valor ingresado tenga formato de email válido antes de permitir el envío.

**Validates: Requirements 14.3**

### Property 10: Mensajes de Error de Validación

*For any* campo de formulario inválido, el sistema debe mostrar un mensaje de error específico debajo del campo indicando qué está mal.

**Validates: Requirements 14.5**

### Property 11: Loading State Durante Operaciones

*For any* operación asíncrona (carga de datos, envío de formulario), el sistema debe mostrar un indicador visual de loading y deshabilitar controles interactivos hasta que la operación complete.

**Validates: Requirements 13.1, 13.2**

### Property 12: Feedback de Operaciones Exitosas

*For any* operación que complete exitosamente (crear, actualizar, eliminar), el sistema debe mostrar un mensaje de éxito mediante toast notification.

**Validates: Requirements 13.3**

### Property 13: Invalidación de Caché al Modificar Datos

*For any* operación de modificación de datos (crear, actualizar, eliminar), el sistema debe invalidar la caché de React Query para esos datos, forzando una recarga.

**Validates: Requirements 16.4**

### Property 14: Persistencia de Autenticación

*For any* usuario autenticado, si recarga la página o cierra y reabre el navegador, el estado de autenticación debe persistir desde localStorage.

**Validates: Requirements 16.5**

### Property 15: Comunicación Exclusiva con Gateway

*For any* petición HTTP realizada por el frontend, la URL base debe ser la del Gateway (configurada en VITE_GATEWAY_URL), nunca URLs directas de microservicios.

**Validates: Requirements 3.1, 3.2**

### Property 16: Variables de Entorno Requeridas

*For any* inicio de la aplicación, si alguna variable de entorno requerida (VITE_GATEWAY_URL, VITE_KEYCLOAK_URL, VITE_KEYCLOAK_REALM, VITE_KEYCLOAK_CLIENT_ID) no está presente, el sistema debe fallar con un error claro.

**Validates: Requirements 18.7**

### Property 17: Responsive Design

*For any* componente de UI, debe renderizarse correctamente y ser funcional en tamaños de pantalla desktop (>1024px), tablet (768-1024px) y móvil (<768px).

**Validates: Requirements 4.2**

### Property 18: Navegación con Teclado

*For any* elemento interactivo (botones, links, inputs), debe ser accesible y operable completamente usando solo el teclado (Tab, Enter, Escape).

**Validates: Requirements 20.4**

### Property 19: Atributos Alt en Imágenes

*For any* elemento `<img>` renderizado, debe tener un atributo `alt` con descripción apropiada.

**Validates: Requirements 20.2**

### Property 20: Labels Asociados a Inputs

*For any* input de formulario, debe tener un `<label>` asociado mediante atributo `htmlFor` o estar envuelto en un `<label>`.

**Validates: Requirements 20.3**

## Error Handling

### Estrategia de Manejo de Errores

El frontend implementa un sistema de manejo de errores en múltiples capas:

#### 1. Axios Interceptors (Capa de Red)

```typescript
// Interceptor de respuesta para errores HTTP
axios.interceptors.response.use(
  (response) => response,
  (error) => {
    const status = error.response?.status;
    
    switch (status) {
      case 401:
        // Limpiar autenticación y redirigir a login
        authContext.logout();
        navigate('/login');
        break;
      
      case 403:
        // Mostrar mensaje de permisos insuficientes
        toast.error('No tiene permisos para realizar esta acción');
        break;
      
      case 404:
        toast.error('Recurso no encontrado');
        break;
      
      case 400:
        // Errores de validación del servidor
        const validationErrors = error.response?.data?.errors;
        if (validationErrors) {
          // Propagar errores de validación al formulario
          return Promise.reject({ validationErrors });
        }
        toast.error(error.response?.data?.message || 'Solicitud inválida');
        break;
      
      case 500:
      case 502:
      case 503:
        toast.error('Error del servidor. Intente más tarde.');
        break;
      
      default:
        if (!error.response) {
          // Error de red
          toast.error('Error de conexión. Intente nuevamente.');
        }
    }
    
    return Promise.reject(error);
  }
);
```

#### 2. React Error Boundaries

```typescript
class ErrorBoundary extends React.Component {
  state = { hasError: false, error: null };
  
  static getDerivedStateFromError(error) {
    return { hasError: true, error };
  }
  
  componentDidCatch(error, errorInfo) {
    // Log error a servicio de monitoreo
    console.error('Error boundary caught:', error, errorInfo);
  }
  
  render() {
    if (this.state.hasError) {
      return <ErrorFallback error={this.state.error} />;
    }
    return this.props.children;
  }
}
```

#### 3. React Query Error Handling

```typescript
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 3,
      retryDelay: (attemptIndex) => Math.min(1000 * 2 ** attemptIndex, 30000),
      onError: (error) => {
        // Manejo global de errores de queries
        console.error('Query error:', error);
      },
    },
    mutations: {
      onError: (error) => {
        // Manejo global de errores de mutations
        toast.error('Operación fallida. Intente nuevamente.');
      },
    },
  },
});
```

#### 4. Form Validation Errors

```typescript
// Usando react-hook-form
const { handleSubmit, formState: { errors } } = useForm({
  resolver: zodResolver(schema),
});

// Errores de validación del cliente
{errors.email && <FormHelperText error>{errors.email.message}</FormHelperText>}

// Errores de validación del servidor
const onSubmit = async (data) => {
  try {
    await createUsuario(data);
  } catch (error) {
    if (error.validationErrors) {
      // Mapear errores del servidor al formulario
      Object.keys(error.validationErrors).forEach(field => {
        setError(field, { message: error.validationErrors[field][0] });
      });
    }
  }
};
```

### Mensajes de Error por Código HTTP

| Código | Mensaje | Acción |
|--------|---------|--------|
| 400 | Errores de validación específicos o "Solicitud inválida" | Mostrar errores en formulario |
| 401 | (Sin mensaje visible) | Redirigir a login automáticamente |
| 403 | "No tiene permisos para realizar esta acción" | Mostrar toast, mantener en página |
| 404 | "Recurso no encontrado" | Mostrar toast |
| 500/502/503 | "Error del servidor. Intente más tarde." | Mostrar toast |
| Network Error | "Error de conexión. Intente nuevamente." | Mostrar toast con botón retry |

### Estados Vacíos

Para mejorar UX, el sistema muestra estados vacíos informativos:

```typescript
// Ejemplo: Lista de eventos vacía
{eventos.length === 0 && !isLoading && (
  <EmptyState
    icon={<EventIcon />}
    title="No hay eventos disponibles"
    description="Actualmente no hay eventos publicados. Vuelve más tarde."
    action={hasRole('Organizator') && (
      <Button onClick={handleCreateEvento}>Crear Evento</Button>
    )}
  />
)}
```

## Testing Strategy

### Enfoque Dual de Testing

El frontend implementa tanto **unit tests** como **property-based tests** para cobertura completa:

- **Unit tests**: Verifican ejemplos específicos, casos edge y condiciones de error
- **Property tests**: Verifican propiedades universales a través de múltiples inputs generados

### Herramientas de Testing

- **Vitest**: Test runner rápido, compatible con Vite
- **React Testing Library**: Testing de componentes centrado en comportamiento de usuario
- **MSW (Mock Service Worker)**: Mocking de API requests
- **fast-check**: Property-based testing para JavaScript/TypeScript

### Configuración de Property Tests

Cada property test debe:
- Ejecutar mínimo 100 iteraciones
- Incluir comentario con referencia a la propiedad del diseño
- Formato: `// Feature: frontend-unificado, Property {number}: {property_text}`

### Estructura de Tests

```
src/
├── modules/
│   ├── eventos/
│   │   ├── components/
│   │   │   ├── EventosList.tsx
│   │   │   └── EventosList.test.tsx
│   │   ├── hooks/
│   │   │   ├── useEventos.ts
│   │   │   └── useEventos.test.ts
│   │   └── services/
│   │       ├── eventosService.ts
│   │       └── eventosService.test.ts
```

### Unit Tests - Componentes

```typescript
// EventosList.test.tsx
describe('EventosList', () => {
  it('should render list of eventos', () => {
    const eventos = [
      { id: '1', nombre: 'Evento 1', fecha: new Date() },
      { id: '2', nombre: 'Evento 2', fecha: new Date() },
    ];
    
    render(<EventosList eventos={eventos} />);
    
    expect(screen.getByText('Evento 1')).toBeInTheDocument();
    expect(screen.getByText('Evento 2')).toBeInTheDocument();
  });
  
  it('should show empty state when no eventos', () => {
    render(<EventosList eventos={[]} />);
    expect(screen.getByText('No hay eventos disponibles')).toBeInTheDocument();
  });
  
  it('should call onEventoClick when evento is clicked', () => {
    const onEventoClick = vi.fn();
    const eventos = [{ id: '1', nombre: 'Evento 1', fecha: new Date() }];
    
    render(<EventosList eventos={eventos} onEventoClick={onEventoClick} />);
    fireEvent.click(screen.getByText('Evento 1'));
    
    expect(onEventoClick).toHaveBeenCalledWith('1');
  });
});
```

### Unit Tests - Hooks

```typescript
// useEventos.test.ts
describe('useEventos', () => {
  it('should fetch eventos on mount', async () => {
    const { result } = renderHook(() => useEventos());
    
    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });
    
    expect(result.current.eventos).toHaveLength(2);
  });
  
  it('should handle fetch error', async () => {
    server.use(
      rest.get('/api/eventos', (req, res, ctx) => {
        return res(ctx.status(500));
      })
    );
    
    const { result } = renderHook(() => useEventos());
    
    await waitFor(() => {
      expect(result.current.error).toBeTruthy();
    });
  });
});
```

### Property Tests - Validación

```typescript
// Feature: frontend-unificado, Property 8: Validación de Campos Requeridos
describe('Form Validation Properties', () => {
  it('should disable submit button when required fields are invalid', () => {
    fc.assert(
      fc.property(
        fc.record({
          nombre: fc.string(),
          correo: fc.string(),
          telefono: fc.string(),
        }),
        (formData) => {
          const { result } = renderHook(() => useForm({
            resolver: zodResolver(usuarioSchema),
            defaultValues: formData,
          }));
          
          const isValid = result.current.formState.isValid;
          const hasRequiredFields = formData.nombre && formData.correo && formData.telefono;
          
          // Si faltan campos requeridos, el formulario debe ser inválido
          if (!hasRequiredFields) {
            expect(isValid).toBe(false);
          }
        }
      ),
      { numRuns: 100 }
    );
  });
});

// Feature: frontend-unificado, Property 9: Validación de Formato de Correo
describe('Email Validation Property', () => {
  it('should validate email format correctly', () => {
    fc.assert(
      fc.property(
        fc.emailAddress(),
        fc.string(),
        (validEmail, invalidEmail) => {
          const validResult = usuarioSchema.shape.correo.safeParse(validEmail);
          const invalidResult = usuarioSchema.shape.correo.safeParse(invalidEmail);
          
          expect(validResult.success).toBe(true);
          
          // Si no es un email válido, debe fallar
          if (!invalidEmail.includes('@')) {
            expect(invalidResult.success).toBe(false);
          }
        }
      ),
      { numRuns: 100 }
    );
  });
});
```

### Property Tests - Autenticación

```typescript
// Feature: frontend-unificado, Property 1: Autenticación Requerida para Rutas Protegidas
describe('Protected Routes Property', () => {
  it('should redirect unauthenticated users to login', () => {
    fc.assert(
      fc.property(
        fc.constantFrom('/eventos', '/mis-entradas', '/usuarios', '/reportes'),
        (protectedRoute) => {
          const { result } = renderHook(() => useAuth(), {
            wrapper: ({ children }) => (
              <AuthProvider initialAuth={{ isAuthenticated: false }}>
                {children}
              </AuthProvider>
            ),
          });
          
          const navigate = vi.fn();
          render(
            <MemoryRouter initialEntries={[protectedRoute]}>
              <ProtectedRoute>
                <div>Protected Content</div>
              </ProtectedRoute>
            </MemoryRouter>
          );
          
          // Usuario no autenticado debe ser redirigido
          expect(screen.queryByText('Protected Content')).not.toBeInTheDocument();
        }
      ),
      { numRuns: 100 }
    );
  });
});

// Feature: frontend-unificado, Property 2: Token JWT en Todas las Peticiones Autenticadas
describe('JWT Token in Requests Property', () => {
  it('should include JWT token in all authenticated requests', () => {
    fc.assert(
      fc.property(
        fc.string({ minLength: 20 }), // JWT token
        fc.constantFrom('GET', 'POST', 'PUT', 'DELETE'),
        fc.oneof(fc.constant('/eventos'), fc.constant('/usuarios'), fc.constant('/entradas')),
        async (token, method, endpoint) => {
          const requestInterceptor = axios.interceptors.request.use((config) => {
            if (token) {
              expect(config.headers.Authorization).toBe(`Bearer ${token}`);
            }
            return config;
          });
          
          try {
            await axios({ method, url: endpoint });
          } catch (error) {
            // Ignorar errores de red en test
          }
          
          axios.interceptors.request.eject(requestInterceptor);
        }
      ),
      { numRuns: 100 }
    );
  });
});
```

### Integration Tests

```typescript
// E2E flow: Login → Ver Eventos → Comprar Entrada
describe('Purchase Flow Integration', () => {
  it('should complete full purchase flow', async () => {
    // 1. Login
    render(<App />);
    fireEvent.click(screen.getByText('Iniciar Sesión con Keycloak'));
    
    await waitFor(() => {
      expect(screen.getByText('Dashboard')).toBeInTheDocument();
    });
    
    // 2. Navigate to eventos
    fireEvent.click(screen.getByText('Eventos'));
    
    await waitFor(() => {
      expect(screen.getByText('Lista de Eventos')).toBeInTheDocument();
    });
    
    // 3. Select evento
    fireEvent.click(screen.getByText('Evento Test'));
    
    // 4. Select asiento
    fireEvent.click(screen.getByTestId('asiento-A1'));
    
    // 5. Confirm purchase
    fireEvent.click(screen.getByText('Comprar Entrada'));
    
    await waitFor(() => {
      expect(screen.getByText('Entrada creada exitosamente')).toBeInTheDocument();
    });
  });
});
```

### Cobertura de Código

Objetivo: >70% de cobertura

```json
// vitest.config.ts
export default defineConfig({
  test: {
    coverage: {
      provider: 'v8',
      reporter: ['text', 'json', 'html'],
      lines: 70,
      functions: 70,
      branches: 70,
      statements: 70,
    },
  },
});
```

### Mocking con MSW

```typescript
// mocks/handlers.ts
export const handlers = [
  rest.get('/api/eventos', (req, res, ctx) => {
    return res(
      ctx.json({
        data: [
          { id: '1', nombre: 'Evento 1', fecha: '2024-01-01' },
          { id: '2', nombre: 'Evento 2', fecha: '2024-02-01' },
        ],
      })
    );
  }),
  
  rest.post('/api/entradas', (req, res, ctx) => {
    return res(
      ctx.json({
        data: { id: '123', estado: 'Reservada' },
      })
    );
  }),
];

// setup.ts
const server = setupServer(...handlers);

beforeAll(() => server.listen());
afterEach(() => server.resetHandlers());
afterAll(() => server.close());
```


## Implementation Details

### Configuración de Vite

```typescript
// vite.config.ts
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import path from 'path';

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
      '@modules': path.resolve(__dirname, './src/modules'),
      '@shared': path.resolve(__dirname, './src/shared'),
    },
  },
  server: {
    port: 3000,
    proxy: {
      '/api': {
        target: process.env.VITE_GATEWAY_URL || 'http://localhost:8080',
        changeOrigin: true,
      },
    },
  },
  build: {
    outDir: 'dist',
    sourcemap: true,
    rollupOptions: {
      output: {
        manualChunks: {
          'react-vendor': ['react', 'react-dom', 'react-router-dom'],
          'mui-vendor': ['@mui/material', '@mui/icons-material'],
          'form-vendor': ['react-hook-form', 'zod'],
        },
      },
    },
  },
});
```

### Configuración de TypeScript

```json
// tsconfig.json
{
  "compilerOptions": {
    "target": "ES2020",
    "useDefineForClassFields": true,
    "lib": ["ES2020", "DOM", "DOM.Iterable"],
    "module": "ESNext",
    "skipLibCheck": true,
    "moduleResolution": "bundler",
    "allowImportingTsExtensions": true,
    "resolveJsonModule": true,
    "isolatedModules": true,
    "noEmit": true,
    "jsx": "react-jsx",
    "strict": true,
    "noUnusedLocals": true,
    "noUnusedParameters": true,
    "noFallthroughCasesInSwitch": true,
    "baseUrl": ".",
    "paths": {
      "@/*": ["./src/*"],
      "@modules/*": ["./src/modules/*"],
      "@shared/*": ["./src/shared/*"]
    }
  },
  "include": ["src"],
  "references": [{ "path": "./tsconfig.node.json" }]
}
```

### Configuración de Axios

```typescript
// src/shared/api/axiosConfig.ts
import axios from 'axios';

const apiClient = axios.create({
  baseURL: import.meta.env.VITE_GATEWAY_URL || 'http://localhost:8080',
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor: agregar token
apiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('auth_token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor: manejo de errores
apiClient.interceptors.response.use(
  (response) => response.data,
  (error) => {
    // Manejo de errores implementado según sección Error Handling
    return Promise.reject(error);
  }
);

export default apiClient;
```

### Configuración de React Query

```typescript
// src/shared/api/queryClient.ts
import { QueryClient } from '@tanstack/react-query';

export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 5 * 60 * 1000, // 5 minutos
      cacheTime: 10 * 60 * 1000, // 10 minutos
      refetchOnWindowFocus: false,
      retry: 3,
      retryDelay: (attemptIndex) => Math.min(1000 * 2 ** attemptIndex, 30000),
    },
  },
});
```

### Configuración de Keycloak (OIDC)

```typescript
// src/context/AuthContext.tsx
import { AuthProvider } from 'react-oidc-context';

const oidcConfig = {
  authority: import.meta.env.VITE_KEYCLOAK_URL + '/realms/' + import.meta.env.VITE_KEYCLOAK_REALM,
  client_id: import.meta.env.VITE_KEYCLOAK_CLIENT_ID,
  redirect_uri: window.location.origin,
  post_logout_redirect_uri: window.location.origin,
  response_type: 'code',
  scope: 'openid profile email',
  automaticSilentRenew: true,
  loadUserInfo: true,
};

export function AppAuthProvider({ children }: { children: React.ReactNode }) {
  return (
    <AuthProvider {...oidcConfig}>
      {children}
    </AuthProvider>
  );
}
```

### Tema de Material UI

```typescript
// src/shared/theme/theme.ts
import { createTheme } from '@mui/material/styles';

export const theme = createTheme({
  palette: {
    primary: {
      main: '#1976d2',
      light: '#42a5f5',
      dark: '#1565c0',
    },
    secondary: {
      main: '#dc004e',
      light: '#e33371',
      dark: '#9a0036',
    },
    error: {
      main: '#f44336',
    },
    warning: {
      main: '#ff9800',
    },
    success: {
      main: '#4caf50',
    },
    background: {
      default: '#f5f5f5',
      paper: '#ffffff',
    },
  },
  typography: {
    fontFamily: '"Roboto", "Helvetica", "Arial", sans-serif',
    h1: {
      fontSize: '2.5rem',
      fontWeight: 500,
    },
    h2: {
      fontSize: '2rem',
      fontWeight: 500,
    },
    h3: {
      fontSize: '1.75rem',
      fontWeight: 500,
    },
    button: {
      textTransform: 'none',
    },
  },
  spacing: 8,
  shape: {
    borderRadius: 8,
  },
  components: {
    MuiButton: {
      styleOverrides: {
        root: {
          borderRadius: 8,
          padding: '8px 16px',
        },
      },
    },
    MuiCard: {
      styleOverrides: {
        root: {
          boxShadow: '0 2px 8px rgba(0,0,0,0.1)',
        },
      },
    },
  },
});
```

### Routing Configuration

```typescript
// src/routes/AppRoutes.tsx
import { Routes, Route, Navigate } from 'react-router-dom';
import { ProtectedRoute } from './ProtectedRoute';
import { RoleBasedRoute } from './RoleBasedRoute';

export function AppRoutes() {
  return (
    <Routes>
      {/* Public routes */}
      <Route path="/login" element={<LoginPage />} />
      
      {/* Protected routes */}
      <Route element={<ProtectedRoute />}>
        <Route path="/" element={<MainLayout />}>
          <Route index element={<Dashboard />} />
          
          {/* Eventos */}
          <Route path="eventos" element={<EventosPage />} />
          <Route path="eventos/:id" element={<EventoDetailPage />} />
          
          {/* Entradas */}
          <Route path="mis-entradas" element={<MisEntradasPage />} />
          <Route path="comprar/:eventoId" element={<ComprarEntradaPage />} />
          
          {/* Usuarios - Admin only */}
          <Route element={<RoleBasedRoute requiredRoles={['Admin']} />}>
            <Route path="usuarios" element={<UsuariosPage />} />
          </Route>
          
          {/* Reportes - Admin & Organizator */}
          <Route element={<RoleBasedRoute requiredRoles={['Admin', 'Organizator']} />}>
            <Route path="reportes" element={<ReportesPage />} />
          </Route>
        </Route>
      </Route>
      
      {/* 404 */}
      <Route path="*" element={<NotFoundPage />} />
    </Routes>
  );
}
```

### Variables de Entorno

```bash
# .env.development
VITE_GATEWAY_URL=http://localhost:8080
VITE_KEYCLOAK_URL=http://localhost:8180
VITE_KEYCLOAK_REALM=Kairo
VITE_KEYCLOAK_CLIENT_ID=kairo-web

# .env.production
VITE_GATEWAY_URL=https://api.kairo.com
VITE_KEYCLOAK_URL=https://auth.kairo.com
VITE_KEYCLOAK_REALM=Kairo
VITE_KEYCLOAK_CLIENT_ID=kairo-web
```

### Validación de Variables de Entorno

```typescript
// src/config/validateEnv.ts
const requiredEnvVars = [
  'VITE_GATEWAY_URL',
  'VITE_KEYCLOAK_URL',
  'VITE_KEYCLOAK_REALM',
  'VITE_KEYCLOAK_CLIENT_ID',
];

export function validateEnvironment() {
  const missing = requiredEnvVars.filter(
    (varName) => !import.meta.env[varName]
  );
  
  if (missing.length > 0) {
    throw new Error(
      `Missing required environment variables: ${missing.join(', ')}\n` +
      'Please check your .env file.'
    );
  }
}

// Llamar en main.tsx antes de renderizar
validateEnvironment();
```

## Deployment

### Dockerfile (Multi-stage Build)

```dockerfile
# Stage 1: Build
FROM node:18-alpine AS builder

WORKDIR /app

# Copy package files
COPY package*.json ./

# Install dependencies
RUN npm ci

# Copy source code
COPY . .

# Build application
RUN npm run build

# Stage 2: Production
FROM nginx:alpine

# Copy built files
COPY --from=builder /app/dist /usr/share/nginx/html

# Copy nginx configuration for SPA routing
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Expose port
EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
```

### Nginx Configuration

```nginx
# nginx.conf
server {
    listen 80;
    server_name localhost;
    root /usr/share/nginx/html;
    index index.html;

    # Gzip compression
    gzip on;
    gzip_types text/plain text/css application/json application/javascript text/xml application/xml application/xml+rss text/javascript;

    # SPA routing: todas las rutas sirven index.html
    location / {
        try_files $uri $uri/ /index.html;
    }

    # Cache static assets
    location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg|woff|woff2|ttf|eot)$ {
        expires 1y;
        add_header Cache-Control "public, immutable";
    }

    # Security headers
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-XSS-Protection "1; mode=block" always;
}
```

### Docker Compose (Desarrollo Local)

```yaml
# docker-compose.yml
version: '3.8'

services:
  frontend:
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "3000:80"
    environment:
      - VITE_GATEWAY_URL=http://localhost:8080
      - VITE_KEYCLOAK_URL=http://localhost:8180
      - VITE_KEYCLOAK_REALM=Kairo
      - VITE_KEYCLOAK_CLIENT_ID=kairo-web
    networks:
      - kairo-network
    depends_on:
      - gateway

networks:
  kairo-network:
    external: true
```

### Build Scripts

```json
// package.json
{
  "scripts": {
    "dev": "vite",
    "build": "tsc && vite build",
    "preview": "vite preview",
    "test": "vitest",
    "test:ui": "vitest --ui",
    "test:coverage": "vitest --coverage",
    "lint": "eslint . --ext ts,tsx --report-unused-disable-directives --max-warnings 0",
    "format": "prettier --write \"src/**/*.{ts,tsx,json,css,md}\"",
    "type-check": "tsc --noEmit"
  }
}
```

## Performance Optimizations

### Code Splitting

```typescript
// Lazy loading de rutas no críticas
const EventosPage = lazy(() => import('@modules/eventos/pages/EventosPage'));
const UsuariosPage = lazy(() => import('@modules/usuarios/pages/UsuariosPage'));
const ReportesPage = lazy(() => import('@modules/reportes/pages/ReportesPage'));

// Suspense wrapper
<Suspense fallback={<LoadingSpinner fullScreen />}>
  <Routes>
    <Route path="/eventos" element={<EventosPage />} />
  </Routes>
</Suspense>
```

### Image Optimization

```typescript
// Lazy loading de imágenes
<img
  src={evento.imagenUrl}
  alt={evento.nombre}
  loading="lazy"
  onError={(e) => {
    e.currentTarget.src = '/placeholder.png';
  }}
/>
```

### React Query Optimizations

```typescript
// Prefetching de datos
const queryClient = useQueryClient();

const handleEventoHover = (eventoId: string) => {
  queryClient.prefetchQuery({
    queryKey: ['evento', eventoId],
    queryFn: () => fetchEvento(eventoId),
  });
};

// Optimistic updates
const { mutate } = useMutation({
  mutationFn: createEntrada,
  onMutate: async (newEntrada) => {
    await queryClient.cancelQueries({ queryKey: ['entradas'] });
    const previousEntradas = queryClient.getQueryData(['entradas']);
    
    queryClient.setQueryData(['entradas'], (old) => [...old, newEntrada]);
    
    return { previousEntradas };
  },
  onError: (err, newEntrada, context) => {
    queryClient.setQueryData(['entradas'], context.previousEntradas);
  },
  onSettled: () => {
    queryClient.invalidateQueries({ queryKey: ['entradas'] });
  },
});
```

### Memoization

```typescript
// Memoizar componentes pesados
const EventoCard = memo(({ evento }: { evento: Evento }) => {
  return (
    <Card>
      <CardContent>
        <Typography variant="h5">{evento.nombre}</Typography>
        <Typography>{evento.descripcion}</Typography>
      </CardContent>
    </Card>
  );
});

// Memoizar cálculos costosos
const sortedEventos = useMemo(() => {
  return eventos.sort((a, b) => a.fecha.getTime() - b.fecha.getTime());
}, [eventos]);

// Memoizar callbacks
const handleEventoClick = useCallback((eventoId: string) => {
  navigate(`/eventos/${eventoId}`);
}, [navigate]);
```

## Accessibility (A11y)

### Semantic HTML

```typescript
// Usar elementos semánticos
<header>
  <nav>
    <ul>
      <li><Link to="/eventos">Eventos</Link></li>
    </ul>
  </nav>
</header>

<main>
  <article>
    <h1>Título del Evento</h1>
    <section>
      <h2>Descripción</h2>
      <p>...</p>
    </section>
  </article>
</main>

<footer>
  <p>&copy; 2024 Kairo</p>
</footer>
```

### ARIA Attributes

```typescript
// Botones con aria-label
<IconButton
  aria-label="Cerrar sesión"
  onClick={handleLogout}
>
  <LogoutIcon />
</IconButton>

// Modals con aria-labelledby
<Dialog
  open={open}
  aria-labelledby="dialog-title"
  aria-describedby="dialog-description"
>
  <DialogTitle id="dialog-title">Confirmar Acción</DialogTitle>
  <DialogContent id="dialog-description">
    ¿Está seguro?
  </DialogContent>
</Dialog>

// Loading states
<div role="status" aria-live="polite">
  {isLoading && <CircularProgress aria-label="Cargando..." />}
</div>
```

### Keyboard Navigation

```typescript
// Trap focus en modals
import { FocusTrap } from '@mui/material';

<FocusTrap open={open}>
  <Dialog open={open}>
    <DialogContent>...</DialogContent>
  </Dialog>
</FocusTrap>

// Skip to main content
<a href="#main-content" className="skip-link">
  Saltar al contenido principal
</a>

<main id="main-content">
  {/* Contenido */}
</main>
```

### Color Contrast

```typescript
// Asegurar contraste WCAG AA (4.5:1 para texto normal)
const theme = createTheme({
  palette: {
    primary: {
      main: '#1976d2', // Contraste 4.6:1 con blanco
    },
    text: {
      primary: '#212121', // Contraste 16:1 con blanco
      secondary: '#757575', // Contraste 4.6:1 con blanco
    },
  },
});
```

## Security Considerations

### XSS Prevention

```typescript
// React escapa automáticamente contenido
<div>{evento.nombre}</div> // Safe

// Evitar dangerouslySetInnerHTML
// Si es necesario, sanitizar con DOMPurify
import DOMPurify from 'dompurify';

<div dangerouslySetInnerHTML={{
  __html: DOMPurify.sanitize(evento.descripcionHtml)
}} />
```

### CSRF Protection

```typescript
// El Gateway maneja CSRF tokens
// Frontend solo necesita incluir token en requests
apiClient.interceptors.request.use((config) => {
  const csrfToken = document.querySelector('meta[name="csrf-token"]')?.getAttribute('content');
  if (csrfToken) {
    config.headers['X-CSRF-Token'] = csrfToken;
  }
  return config;
});
```

### Secure Storage

```typescript
// NO almacenar datos sensibles en localStorage
// Solo almacenar tokens (que expiran)
localStorage.setItem('auth_token', token);

// Para datos sensibles, usar sessionStorage o memoria
const sensitiveData = useRef(null);
```

### Content Security Policy

```html
<!-- index.html -->
<meta http-equiv="Content-Security-Policy" 
      content="default-src 'self'; 
               script-src 'self' 'unsafe-inline'; 
               style-src 'self' 'unsafe-inline'; 
               img-src 'self' data: https:; 
               connect-src 'self' http://localhost:8080 http://localhost:8180;">
```

## Monitoring and Logging

### Error Tracking

```typescript
// Integración con Sentry (opcional)
import * as Sentry from '@sentry/react';

Sentry.init({
  dsn: import.meta.env.VITE_SENTRY_DSN,
  environment: import.meta.env.MODE,
  tracesSampleRate: 1.0,
});

// Capturar errores manualmente
try {
  await createEvento(data);
} catch (error) {
  Sentry.captureException(error);
  throw error;
}
```

### Analytics

```typescript
// Google Analytics o similar
import ReactGA from 'react-ga4';

ReactGA.initialize(import.meta.env.VITE_GA_TRACKING_ID);

// Track page views
useEffect(() => {
  ReactGA.send({ hitType: 'pageview', page: location.pathname });
}, [location]);

// Track events
const handleEventoClick = (eventoId: string) => {
  ReactGA.event({
    category: 'Eventos',
    action: 'Click',
    label: eventoId,
  });
  navigate(`/eventos/${eventoId}`);
};
```

### Performance Monitoring

```typescript
// Web Vitals
import { getCLS, getFID, getFCP, getLCP, getTTFB } from 'web-vitals';

function sendToAnalytics(metric) {
  console.log(metric);
  // Enviar a servicio de analytics
}

getCLS(sendToAnalytics);
getFID(sendToAnalytics);
getFCP(sendToAnalytics);
getLCP(sendToAnalytics);
getTTFB(sendToAnalytics);
```

