# Frontend Unificado - Kairo Events

Frontend moderno construido con React + Vite + TypeScript para el sistema de gestión de eventos Kairo.

## 🚀 Stack Tecnológico

- **React 18+** - Framework UI
- **Vite** - Build tool y dev server
- **TypeScript** - Type safety
- **Keycloak (OIDC)** - Autenticación y autorización
- **react-oidc-context** - Integración OIDC con React
- **React Query** - State management y caché de datos del servidor
- **Axios** - Cliente HTTP con interceptors
- **Material UI (MUI)** - Librería de componentes UI
- **Emotion** - CSS-in-JS para estilos
- **React Router** - Routing (próximamente)

## 📁 Estructura del Proyecto

```
src/
├── modules/              # Módulos por dominio
│   ├── eventos/         # Gestión de eventos
│   ├── usuarios/        # Gestión de usuarios
│   ├── entradas/        # Compra y gestión de entradas
│   └── reportes/        # Reportes y métricas
├── shared/              # Código compartido
│   ├── components/      # Componentes reutilizables
│   ├── hooks/           # Custom hooks
│   ├── utils/           # Utilidades
│   ├── types/           # Tipos TypeScript globales
│   └── api/             # Cliente API
├── context/             # React Context providers
├── layouts/             # Layouts de página
└── routes/              # Configuración de rutas
```

## 🛠️ Configuración

### Variables de Entorno

Copia `.env.example` a `.env.development` y configura las variables:

```bash
# Gateway Configuration
VITE_GATEWAY_URL=http://localhost:8080

# Keycloak Configuration
VITE_KEYCLOAK_URL=http://localhost:8180
VITE_KEYCLOAK_REALM=Kairo
VITE_KEYCLOAK_CLIENT_ID=kairo-web
```

### Instalación

```bash
npm install
```

### Desarrollo

```bash
npm run dev
```

La aplicación estará disponible en `http://localhost:5173`

### Build

```bash
npm run build
```

### Linting y Formateo

```bash
# Ejecutar ESLint
npm run lint

# Corregir problemas de ESLint automáticamente
npm run lint:fix

# Formatear código con Prettier
npm run format

# Verificar formato sin modificar
npm run format:check

# Verificar tipos de TypeScript
npm run type-check
```

## 🎯 Alias de TypeScript

El proyecto está configurado con los siguientes alias para imports limpios:

- `@/*` - Raíz de src
- `@modules/*` - Módulos de dominio
- `@shared/*` - Código compartido
- `@context/*` - Context providers
- `@layouts/*` - Layouts
- `@routes/*` - Configuración de rutas

Ejemplo:
```typescript
import { validateEnv } from '@shared/utils';
import { EventosList } from '@modules/eventos';
```

## 📝 Convenciones de Código

- **Componentes**: PascalCase (ej: `EventosList.tsx`)
- **Funciones/Variables**: camelCase (ej: `fetchEventos`)
- **Tipos/Interfaces**: PascalCase (ej: `EventoDto`)
- **Archivos de utilidades**: camelCase (ej: `validateEnv.ts`)

## 🔐 Autenticación

El frontend utiliza Keycloak con OpenID Connect (OIDC) para autenticación. Características:

- **Single Sign-On (SSO)**: Autenticación centralizada con Keycloak
- **Renovación Automática**: Los tokens se renuevan automáticamente antes de expirar
- **Control de Acceso por Roles**: Roles extraídos del JWT (Admin, Organizator, Asistente)
- **Logout Seguro**: Limpieza completa del estado de autenticación

### Uso Básico

```typescript
import { useAuth } from './context/AuthContext';

function MyComponent() {
  const { isAuthenticated, login, logout, user, hasRole } = useAuth();

  if (!isAuthenticated) {
    return <button onClick={login}>Login</button>;
  }

  return (
    <div>
      <p>Welcome, {user?.profile?.name}!</p>
      {hasRole('Admin') && <AdminPanel />}
      <button onClick={logout}>Logout</button>
    </div>
  );
}
```

Para más detalles, consulta [docs/AUTHENTICATION.md](./docs/AUTHENTICATION.md)

## 🌐 Cliente API y React Query

El frontend utiliza Axios para comunicación HTTP y React Query para gestión de estado del servidor.

### Cliente API

- **Comunicación exclusiva con Gateway**: Todas las peticiones van a `http://localhost:8080`
- **Autenticación automática**: Token JWT agregado automáticamente en header Authorization
- **Manejo de errores**: Interceptors para manejo centralizado de errores HTTP
- **Retry logic**: Reintentos automáticos con backoff exponencial

### React Query

- **Caché inteligente**: Datos cacheados con staleTime de 5 minutos
- **Invalidación automática**: Caché invalidado al modificar datos
- **Limpieza en logout**: Caché limpiado completamente al cerrar sesión
- **Optimistic updates**: Soporte para actualizaciones optimistas

### Uso Básico

```typescript
import { useQuery } from '@tanstack/react-query';
import { useMutationWithInvalidation } from '@shared/hooks';

// Fetch data
const { data, isLoading } = useQuery({
  queryKey: ['eventos'],
  queryFn: eventosService.fetchAll,
});

// Mutate data with auto-invalidation
const createEvento = useMutationWithInvalidation(
  (data) => eventosService.create(data),
  ['eventos'], // Queries to invalidate
);
```

Para más detalles:
- [docs/API-CLIENT.md](./docs/API-CLIENT.md) - Cliente API y Axios
- [docs/REACT-QUERY.md](./docs/REACT-QUERY.md) - React Query completo
- [docs/QUICK-START-REACT-QUERY.md](./docs/QUICK-START-REACT-QUERY.md) - Guía rápida

## 🎨 Material UI y Tema

El frontend utiliza Material UI v6 con un tema personalizado que define colores, tipografía, espaciado y estilos de componentes.

### Características del Tema

- **Paleta de colores**: Primary (azul), Secondary (púrpura), colores semánticos (error, warning, info, success)
- **Tipografía**: Sistema de fuentes moderno con 6 niveles de headings
- **Espaciado**: Sistema base de 8px para consistencia
- **Breakpoints responsive**: xs (0px), sm (600px), md (960px), lg (1280px), xl (1920px)
- **Componentes personalizados**: Botones, Cards, TextFields, Papers con estilos consistentes

### Uso Básico

```typescript
import { Button, Typography, Box } from '@mui/material';
import { useTheme } from '@mui/material/styles';

function MyComponent() {
  const theme = useTheme();
  
  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4" color="primary">
        Title
      </Typography>
      <Button variant="contained" color="primary">
        Click Me
      </Button>
    </Box>
  );
}
```

### Diseño Responsive

```typescript
// Valores diferentes por breakpoint
<Box
  sx={{
    width: { xs: '100%', sm: '50%', md: '33%' },
    p: { xs: 2, sm: 3, md: 4 },
  }}
>
  Responsive Box
</Box>

// Detección de tamaño de pantalla
const isMobile = useMediaQuery(theme.breakpoints.down('sm'));
```

Para más detalles, consulta [docs/THEME.md](./docs/THEME.md)

## 🔒 Comunicación con Backend

El frontend se comunica **exclusivamente** con el Gateway (puerto 8080), nunca directamente con microservicios.

## 📦 Próximos Pasos

1. ✅ Configuración base del proyecto
2. ✅ Implementar autenticación con Keycloak
3. ✅ Configurar comunicación con Gateway
4. ✅ Configurar React Query y gestión de estado
5. ✅ Implementar UI library y tema (Material UI)
6. ✅ Implementar routing y navegación
7. ✅ Implementar módulos de dominio
8. ✅ Dockerización y despliegue
9. ⏳ Agregar tests completos

## 🐳 Docker

El frontend está completamente dockerizado con soporte para desarrollo y producción.

### Quick Start con Docker

```bash
# Crear red externa (solo una vez)
docker network create kairo-network

# Producción
docker-compose up -d

# Desarrollo (con hot reload)
docker-compose -f docker-compose.dev.yml up -d
```

### Build Manual

```bash
# Linux/Mac
./build-docker.sh production

# Windows
.\build-docker.ps1 production
```

### Características Docker

- ✅ Multi-stage build (builder + nginx)
- ✅ Imagen optimizada (~50-80MB)
- ✅ Nginx con SPA routing
- ✅ Compresión gzip habilitada
- ✅ Cache de assets estáticos (1 año)
- ✅ Security headers configurados
- ✅ Health checks incluidos
- ✅ Conexión a red kairo-network

Para más detalles, consulta [DOCKER.md](./DOCKER.md)

## 📄 Licencia

Privado - Sistema de Eventos Kairo
