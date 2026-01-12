import { http, HttpResponse } from 'msw';

const GATEWAY_URL = import.meta.env.VITE_GATEWAY_URL || 'http://localhost:8080';

/**
 * Error handlers for testing error scenarios
 * Use these handlers to override default handlers in specific tests
 */

// 401 Unauthorized
export const unauthorized401Handler = http.get(`${GATEWAY_URL}/api/*`, () => {
  return HttpResponse.json(
    {
      message: 'Unauthorized',
      statusCode: 401,
    },
    { status: 401 }
  );
});

// 403 Forbidden
export const forbidden403Handler = http.get(`${GATEWAY_URL}/api/*`, () => {
  return HttpResponse.json(
    {
      message: 'No tiene permisos para realizar esta acción',
      statusCode: 403,
    },
    { status: 403 }
  );
});

// 404 Not Found
export const notFound404Handler = http.get(`${GATEWAY_URL}/api/*`, () => {
  return HttpResponse.json(
    {
      message: 'Recurso no encontrado',
      statusCode: 404,
    },
    { status: 404 }
  );
});

// 400 Bad Request with validation errors
export const badRequest400Handler = http.post(`${GATEWAY_URL}/api/*`, () => {
  return HttpResponse.json(
    {
      message: 'Solicitud inválida',
      errors: {
        nombre: ['El nombre es requerido'],
        correo: ['El correo no es válido'],
      },
      statusCode: 400,
    },
    { status: 400 }
  );
});

// 500 Internal Server Error
export const serverError500Handler = http.get(`${GATEWAY_URL}/api/*`, () => {
  return HttpResponse.json(
    {
      message: 'Error del servidor. Intente más tarde.',
      statusCode: 500,
    },
    { status: 500 }
  );
});

// Network Error
export const networkErrorHandler = http.get(`${GATEWAY_URL}/api/*`, () => {
  return HttpResponse.error();
});

/**
 * Helper function to override handlers in tests
 * 
 * Example usage:
 * ```typescript
 * import { server } from '@/test/mocks/server';
 * import { unauthorized401Handler } from '@/test/mocks/errorHandlers';
 * 
 * test('handles 401 error', () => {
 *   server.use(unauthorized401Handler);
 *   // ... test code
 * });
 * ```
 */
