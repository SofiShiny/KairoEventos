# MSW (Mock Service Worker) Configuration

This directory contains the MSW configuration for mocking Gateway API endpoints during tests.

## Overview

MSW intercepts HTTP requests at the network level, allowing us to test components that make API calls without hitting the actual Gateway. This provides:

- **Isolated tests**: Tests don't depend on external services
- **Faster tests**: No network latency
- **Predictable responses**: Controlled test data
- **Error scenario testing**: Easy to simulate errors

## Files

### `handlers.ts`

Contains all the default mock handlers for Gateway endpoints:

- **Eventos**: List, get, create, update, cancel
- **Entradas**: List user's entradas, get available seats, create, cancel
- **Usuarios**: List, get, create, update, deactivate (Admin only)
- **Reportes**: Metrics, attendance history, financial reconciliation, export
- **Dashboard**: Statistics

### `server.ts`

Sets up the MSW server for Node environment (used in tests).

### `errorHandlers.ts`

Contains handlers for testing error scenarios:

- `unauthorized401Handler`: Returns 401 Unauthorized
- `forbidden403Handler`: Returns 403 Forbidden
- `notFound404Handler`: Returns 404 Not Found
- `badRequest400Handler`: Returns 400 with validation errors
- `serverError500Handler`: Returns 500 Internal Server Error
- `networkErrorHandler`: Simulates network error

## Usage

### Basic Usage

MSW is automatically configured in `src/test/setup.ts` and will intercept all requests during tests.

```typescript
import { render, screen, waitFor } from '@testing-library/react';
import { EventosList } from './EventosList';

test('renders list of eventos', async () => {
  render(<EventosList />);
  
  // MSW will automatically return mock data
  await waitFor(() => {
    expect(screen.getByText('Evento Test 1')).toBeInTheDocument();
  });
});
```

### Testing Error Scenarios

Override default handlers for specific tests:

```typescript
import { server } from '@/test/mocks/server';
import { serverError500Handler } from '@/test/mocks/errorHandlers';

test('handles 500 error', async () => {
  // Override handler for this test
  server.use(serverError500Handler);
  
  render(<EventosList />);
  
  await waitFor(() => {
    expect(screen.getByText('Error del servidor')).toBeInTheDocument();
  });
});
```

### Custom Handlers for Specific Tests

Create custom handlers inline:

```typescript
import { http, HttpResponse } from 'msw';
import { server } from '@/test/mocks/server';

test('handles empty eventos list', async () => {
  server.use(
    http.get('http://localhost:8080/api/eventos', () => {
      return HttpResponse.json({
        data: [],
        success: true,
      });
    })
  );
  
  render(<EventosList />);
  
  await waitFor(() => {
    expect(screen.getByText('No hay eventos disponibles')).toBeInTheDocument();
  });
});
```

### Testing with Query Parameters

```typescript
import { http, HttpResponse } from 'msw';
import { server } from '@/test/mocks/server';

test('filters entradas by estado', async () => {
  server.use(
    http.get('http://localhost:8080/api/entradas/mis-entradas', ({ request }) => {
      const url = new URL(request.url);
      const estado = url.searchParams.get('estado');
      
      return HttpResponse.json({
        data: estado === 'Pagada' ? [/* pagadas */] : [/* all */],
        success: true,
      });
    })
  );
  
  // ... test code
});
```

## Best Practices

1. **Use default handlers**: Most tests should work with default handlers
2. **Override sparingly**: Only override handlers when testing specific scenarios
3. **Reset after tests**: Handlers are automatically reset after each test
4. **Test isolation**: Each test should be independent
5. **Realistic data**: Mock data should match actual API responses

## Environment Variables

MSW uses the `VITE_GATEWAY_URL` environment variable to construct endpoint URLs. Make sure this is set in your `.env.test` file:

```env
VITE_GATEWAY_URL=http://localhost:8080
```

## Debugging

To see which requests MSW is intercepting, check the console output. MSW will warn about unhandled requests.

To disable MSW warnings for specific tests:

```typescript
beforeAll(() => {
  server.listen({ onUnhandledRequest: 'bypass' });
});
```

## Adding New Endpoints

When adding new Gateway endpoints:

1. Add the handler to `handlers.ts`
2. Follow the existing pattern
3. Return realistic mock data
4. Include success/error responses

Example:

```typescript
// GET /api/new-endpoint
http.get(`${GATEWAY_URL}/api/new-endpoint`, () => {
  return HttpResponse.json({
    data: { /* mock data */ },
    success: true,
  });
}),
```
