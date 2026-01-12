# API Client Configuration

This directory contains the configured Axios client for communicating with the Gateway.

## Features

- **Automatic JWT Token Injection**: Adds Bearer token to all authenticated requests
- **Error Handling**: Centralized error handling for all HTTP status codes
- **Retry Logic**: Automatic retry with exponential backoff for network errors and 5xx errors
- **401 Redirect**: Automatic redirect to login on unauthorized responses
- **Validation Error Propagation**: Passes validation errors to forms for display

## Usage

### Basic Usage

```typescript
import axiosClient from '@shared/api';

// GET request
const response = await axiosClient.get('/api/eventos');
const eventos = response.data;

// POST request
const newEvento = await axiosClient.post('/api/eventos', {
  nombre: 'Evento Test',
  fecha: '2024-01-01',
});

// PUT request
await axiosClient.put(`/api/eventos/${id}`, updatedData);

// DELETE request
await axiosClient.delete(`/api/eventos/${id}`);
```

### Error Handling

The client automatically handles common HTTP errors:

- **401 Unauthorized**: Redirects to login and clears authentication
- **403 Forbidden**: Logs "No tiene permisos para realizar esta acción"
- **404 Not Found**: Logs "Recurso no encontrado"
- **400 Bad Request**: Propagates validation errors for forms
- **500/502/503**: Logs "Error del servidor. Intente más tarde." and retries
- **Network Errors**: Retries with exponential backoff

### Handling Validation Errors

```typescript
try {
  await axiosClient.post('/api/usuarios', formData);
} catch (error: any) {
  if (error.validationErrors) {
    // error.validationErrors is a Record<string, string[]>
    // Map to your form library (react-hook-form, formik, etc.)
    Object.keys(error.validationErrors).forEach(field => {
      setError(field, { message: error.validationErrors[field][0] });
    });
  }
}
```

### Creating Service Functions

```typescript
// src/modules/eventos/services/eventosService.ts
import axiosClient from '@shared/api';
import { Evento } from '../types';

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

  async deleteEvento(id: string): Promise<void> {
    await axiosClient.delete(`/api/eventos/${id}`);
  },
};
```

## Configuration

The client is configured via environment variables:

- `VITE_GATEWAY_URL`: Base URL of the Gateway (default: http://localhost:8080)

## Retry Logic

The client implements exponential backoff for:
- Network errors (no response)
- Server errors (500, 502, 503)

Retry schedule:
- 1st retry: 1 second delay
- 2nd retry: 2 seconds delay
- 3rd retry: 4 seconds delay
- After 3 retries: Error is thrown

## Token Management

The client automatically:
1. Reads the JWT token from `localStorage.getItem('auth_token')`
2. Adds it to the `Authorization` header as `Bearer {token}`
3. Clears the token on 401 responses

The token is managed by the `AuthContext` and persisted automatically.
