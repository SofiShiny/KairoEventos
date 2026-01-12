# Arquitectura del Frontend Unificado

## Visión General

El Frontend Unificado sigue una arquitectura modular basada en dominios, donde cada módulo encapsula toda la funcionalidad relacionada con un área específica del negocio.

## Principios de Diseño

### 1. Separación por Dominio

Cada módulo de dominio (`eventos`, `usuarios`, `entradas`, `reportes`) contiene:

- **components/**: Componentes React específicos del dominio
- **hooks/**: Custom hooks para lógica de negocio
- **services/**: Funciones para comunicación con API
- **types/**: Tipos TypeScript del dominio

### 2. Barrel Exports

Cada módulo y subcarpeta tiene un `index.ts` que controla qué se exporta públicamente:

```typescript
// src/modules/eventos/index.ts
export * from './components';
export * from './hooks';
export * from './services';
export * from './types';
```

Esto permite imports limpios:

```typescript
import { EventosList, useEventos } from '@modules/eventos';
```

### 3. Código Compartido

La carpeta `shared/` contiene código reutilizable entre módulos:

- **components/**: Componentes UI genéricos (Button, TextField, LoadingSpinner)
- **hooks/**: Hooks compartidos (useAuth, useToast)
- **utils/**: Utilidades (validateEnv, formatDate)
- **types/**: Tipos globales (ApiResponse, PaginatedResponse)
- **api/**: Cliente HTTP configurado (Axios con interceptors)

### 4. Separación de Responsabilidades

#### Componentes (UI)
- Solo renderizado y manejo de eventos de UI
- No contienen lógica de negocio
- Reciben datos y callbacks como props

#### Hooks (Lógica)
- Encapsulan lógica de negocio
- Manejan estado local y efectos
- Llaman a servicios para obtener datos

#### Services (API)
- Funciones puras para comunicación con backend
- Retornan Promises
- No manejan estado

Ejemplo:

```typescript
// Service
export async function fetchEventos(): Promise<Evento[]> {
  const response = await apiClient.get('/eventos');
  return response.data;
}

// Hook
export function useEventos() {
  return useQuery({
    queryKey: ['eventos'],
    queryFn: fetchEventos,
  });
}

// Component
export function EventosList() {
  const { data: eventos, isLoading } = useEventos();
  
  if (isLoading) return <LoadingSpinner />;
  
  return (
    <div>
      {eventos?.map(evento => (
        <EventoCard key={evento.id} evento={evento} />
      ))}
    </div>
  );
}
```

## Flujo de Datos

```
Usuario → Componente → Hook → Service → API Client → Gateway → Microservicio
                                              ↓
                                         Interceptors
                                         (Auth, Errors)
```

## Alias de TypeScript

Los alias configurados facilitan imports limpios:

| Alias | Ruta | Uso |
|-------|------|-----|
| `@/*` | `src/*` | Cualquier archivo en src |
| `@modules/*` | `src/modules/*` | Módulos de dominio |
| `@shared/*` | `src/shared/*` | Código compartido |
| `@context/*` | `src/context/*` | Context providers |
| `@layouts/*` | `src/layouts/*` | Layouts |
| `@routes/*` | `src/routes/*` | Configuración de rutas |

## Gestión de Estado

### Estado Local
- `useState` para estado de componente
- `useReducer` para estado complejo

### Estado Global
- React Context para autenticación y tema
- React Query para caché de datos del servidor

### Estado del Servidor
- React Query maneja:
  - Caché automático
  - Revalidación
  - Loading/Error states
  - Optimistic updates

## Comunicación con Backend

### Regla de Oro
**El frontend SOLO se comunica con el Gateway (puerto 8080)**

Nunca comunicación directa con microservicios.

### Cliente HTTP

```typescript
// src/shared/api/client.ts
import axios from 'axios';
import { env } from '@shared/utils';

export const apiClient = axios.create({
  baseURL: env.gatewayUrl,
});

// Request interceptor: agregar token
apiClient.interceptors.request.use((config) => {
  const token = getToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Response interceptor: manejo de errores
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Redirigir a login
    }
    return Promise.reject(error);
  }
);
```

## Variables de Entorno

### Validación en Startup

El archivo `validateEnv.ts` verifica que todas las variables requeridas estén presentes:

```typescript
validateEnv(); // Lanza error si falta alguna variable
```

### Acceso Type-Safe

```typescript
import { env } from '@shared/utils';

console.log(env.gatewayUrl); // Type-safe
console.log(env.keycloak.realm); // Type-safe
```

## Testing

### Estructura de Tests

Los tests se colocan junto al código que prueban:

```
src/modules/eventos/
├── components/
│   ├── EventosList.tsx
│   └── EventosList.test.tsx
├── hooks/
│   ├── useEventos.ts
│   └── useEventos.test.ts
```

### Tipos de Tests

1. **Unit Tests**: Componentes, hooks, utilidades
2. **Property Tests**: Validación, lógica de negocio
3. **Integration Tests**: Flujos completos

## Convenciones

### Nombres de Archivos
- Componentes: `PascalCase.tsx` (ej: `EventosList.tsx`)
- Hooks: `camelCase.ts` (ej: `useEventos.ts`)
- Utilidades: `camelCase.ts` (ej: `validateEnv.ts`)
- Tipos: `PascalCase.ts` (ej: `EventoDto.ts`)

### Nombres de Variables
- Componentes: `PascalCase`
- Funciones: `camelCase`
- Constantes: `UPPER_SNAKE_CASE`
- Tipos/Interfaces: `PascalCase`

### Imports
Ordenar imports en este orden:
1. React y librerías externas
2. Alias internos (@modules, @shared)
3. Imports relativos
4. Estilos

```typescript
import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';

import { useAuth } from '@shared/hooks';
import { EventosList } from '@modules/eventos';

import { formatDate } from './utils';

import './styles.css';
```

## Próximos Pasos

1. Implementar autenticación con Keycloak
2. Configurar React Query
3. Crear componentes compartidos base
4. Implementar módulos de dominio
5. Agregar tests
