# API Client Documentation

## Overview

The frontend communicates exclusively with the Gateway API (port 8080) using a configured Axios client. This client handles authentication, error handling, and retry logic automatically.

## Configuration

### Environment Variables

The API client is configured via environment variables:

```bash
# .env.development
VITE_GATEWAY_URL=http://localhost:8080

# .env.production
VITE_GATEWAY_URL=https://api.kairo.com
```

### Features

1. **Automatic JWT Token Injection**: Adds Bearer token to all requests
2. **Centralized Error Handling**: Handles all HTTP error codes appropriately
3. **Retry Logic**: Automatic retry with exponential backoff for network and server errors
4. **401 Auto-Redirect**: Automatically redirects to login on unauthorized responses
5. **Validation Error Propagation**: Passes validation errors to forms

## Usage

### Basic Requests

```typescript
import axiosClient from '@shared/api';

// GET request
const response = await axiosClient.get('/api/eventos');
const eventos = response.data;

// POST request
const newEvento = await axiosClient.post('/api/eventos', {
  nombre: 'Evento Test',
  fecha: '2024-01-01',
  ubicacion: 'Teatro Principal',
});

// PUT request
await axiosClient.put(`/api/eventos/${id}`, {
  nombre: 'Evento Actualizado',
});

// DELETE request
await axiosClient.delete(`/api/eventos/${id}`);
```

### Creating Service Functions

Organize API calls in service files within each module:

```typescript
// src/modules/eventos/services/eventosService.ts
import axiosClient from '@shared/api';
import { Evento, CreateEventoDto, UpdateEventoDto } from '../types';

export const eventosService = {
  async fetchEventos(): Promise<Evento[]> {
    const response = await axiosClient.get('/api/eventos');
    return response.data;
  },

  async fetchEvento(id: string): Promise<Evento> {
    const response = await axiosClient.get(`/api/eventos/${id}`);
    return response.data;
  },

  async createEvento(data: CreateEventoDto): Promise<Evento> {
    const response = await axiosClient.post('/api/eventos', data);
    return response.data;
  },

  async updateEvento(id: string, data: UpdateEventoDto): Promise<Evento> {
    const response = await axiosClient.put(`/api/eventos/${id}`, data);
    return response.data;
  },

  async cancelEvento(id: string): Promise<void> {
    await axiosClient.delete(`/api/eventos/${id}`);
  },
};
```

### Using with React Query

```typescript
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { eventosService } from '../services/eventosService';

// Query hook
export function useEventos() {
  return useQuery({
    queryKey: ['eventos'],
    queryFn: eventosService.fetchEventos,
  });
}

// Mutation hook
export function useCreateEvento() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: eventosService.createEvento,
    onSuccess: () => {
      // Invalidate and refetch
      queryClient.invalidateQueries({ queryKey: ['eventos'] });
    },
  });
}
```

## Error Handling

### HTTP Status Codes

The client automatically handles the following error codes:

| Code | Behavior | User Message |
|------|----------|--------------|
| 401 | Clears auth, redirects to login | (Silent redirect) |
| 403 | Logs error | "No tiene permisos para realizar esta acción" |
| 404 | Logs error | "Recurso no encontrado" |
| 400 | Propagates validation errors | Specific field errors |
| 500/502/503 | Retries 3 times, then logs | "Error del servidor. Intente más tarde." |
| Network Error | Retries 3 times, then logs | "Error de conexión. Intente nuevamente." |

### Handling Validation Errors

For 400 Bad Request with validation errors:

```typescript
import { useForm } from 'react-hook-form';

const { setError } = useForm();

try {
  await axiosClient.post('/api/usuarios', formData);
} catch (error: any) {
  if (error.validationErrors) {
    // error.validationErrors is Record<string, string[]>
    Object.keys(error.validationErrors).forEach(field => {
      setError(field, { 
        message: error.validationErrors[field][0] 
      });
    });
  }
}
```

### Custom Error Handling

For specific error handling in components:

```typescript
try {
  await eventosService.createEvento(data);
  toast.success('Evento creado exitosamente');
} catch (error: any) {
  if (error.response?.status === 403) {
    toast.error('No tiene permisos para crear eventos');
  } else if (error.validationErrors) {
    // Handle validation errors
  } else {
    toast.error('Error al crear evento');
  }
}
```

## Retry Logic

### Network Errors

When a request fails due to network issues (no response), the client automatically retries up to 3 times with exponential backoff:

- 1st retry: 1 second delay
- 2nd retry: 2 seconds delay
- 3rd retry: 4 seconds delay

### Server Errors (5xx)

For 500, 502, and 503 errors, the same retry logic applies.

### Disabling Retry

To disable retry for a specific request:

```typescript
const response = await axiosClient.get('/api/eventos', {
  // Add a flag to skip retry
  _retry: true,
  _retryCount: 3,
});
```

## Authentication

### Token Management

The client automatically:
1. Reads JWT token from `localStorage.getItem('auth_token')`
2. Adds it to the `Authorization` header as `Bearer {token}`
3. Clears token on 401 responses

The token is managed by `AuthContext` and persisted automatically when the user logs in.

### Manual Token Refresh

Token refresh is handled automatically by Keycloak via `react-oidc-context` with `automaticSilentRenew: true`.

## Best Practices

### 1. Use Service Functions

Always wrap API calls in service functions:

```typescript
// ✅ Good
const eventos = await eventosService.fetchEventos();

// ❌ Bad
const eventos = await axiosClient.get('/api/eventos');
```

### 2. Use React Query

Leverage React Query for caching and state management:

```typescript
// ✅ Good
const { data, isLoading, error } = useEventos();

// ❌ Bad
const [eventos, setEventos] = useState([]);
useEffect(() => {
  axiosClient.get('/api/eventos').then(setEventos);
}, []);
```

### 3. Handle Errors Gracefully

Always provide user feedback:

```typescript
// ✅ Good
try {
  await createEvento(data);
  toast.success('Evento creado');
} catch (error) {
  toast.error('Error al crear evento');
}

// ❌ Bad
await createEvento(data); // Silent failure
```

### 4. Type Your Responses

Use TypeScript interfaces for type safety:

```typescript
// ✅ Good
interface Evento {
  id: string;
  nombre: string;
  fecha: Date;
}

const eventos: Evento[] = await eventosService.fetchEventos();

// ❌ Bad
const eventos = await eventosService.fetchEventos(); // any type
```

## Testing

### Mocking API Calls

Use MSW (Mock Service Worker) for testing:

```typescript
import { rest } from 'msw';
import { setupServer } from 'msw/node';

const server = setupServer(
  rest.get('/api/eventos', (req, res, ctx) => {
    return res(
      ctx.json({
        data: [
          { id: '1', nombre: 'Evento 1' },
          { id: '2', nombre: 'Evento 2' },
        ],
      })
    );
  })
);

beforeAll(() => server.listen());
afterEach(() => server.resetHandlers());
afterAll(() => server.close());
```

### Testing Error Handling

```typescript
it('should handle 404 errors', async () => {
  server.use(
    rest.get('/api/eventos/:id', (req, res, ctx) => {
      return res(ctx.status(404));
    })
  );

  await expect(eventosService.fetchEvento('999')).rejects.toThrow();
});
```

## Security

### HTTPS Only in Production

Always use HTTPS in production:

```bash
# .env.production
VITE_GATEWAY_URL=https://api.kairo.com
```

### Token Storage

Tokens are stored in `localStorage` for persistence across sessions. This is acceptable for:
- Short-lived tokens (< 1 hour)
- Tokens with automatic refresh
- Non-sensitive applications

For higher security requirements, consider:
- Using `httpOnly` cookies (requires backend changes)
- Implementing token rotation
- Using shorter token lifetimes

### CORS

The Gateway handles CORS configuration. The frontend should never bypass CORS.

## Troubleshooting

### "Network Error" on all requests

1. Check Gateway is running: `http://localhost:8080`
2. Verify `VITE_GATEWAY_URL` in `.env`
3. Check browser console for CORS errors

### "401 Unauthorized" loop

1. Clear localStorage: `localStorage.clear()`
2. Log out and log in again
3. Check Keycloak is running and configured

### Validation errors not showing

1. Ensure error has `validationErrors` property
2. Check Gateway returns errors in correct format:
   ```json
   {
     "message": "Validation failed",
     "errors": {
       "email": ["Email is required"],
       "password": ["Password too short"]
     }
   }
   ```

### Requests timing out

1. Increase timeout: `axiosClient.defaults.timeout = 60000`
2. Check Gateway performance
3. Verify network connectivity
