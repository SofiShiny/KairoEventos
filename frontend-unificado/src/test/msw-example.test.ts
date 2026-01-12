import { describe, test, expect } from 'vitest';
import axios from 'axios';
import { server } from './mocks/server';
import { serverError500Handler, notFound404Handler } from './mocks/errorHandlers';

/**
 * Example tests demonstrating MSW usage
 * These tests show how to use MSW to mock API requests
 */

const GATEWAY_URL = 'http://localhost:8080';

describe('MSW Configuration', () => {
  test('intercepts GET requests with default handlers', async () => {
    const response = await axios.get(`${GATEWAY_URL}/api/eventos`);
    
    expect(response.status).toBe(200);
    expect(response.data.success).toBe(true);
    expect(response.data.data).toHaveLength(2);
    expect(response.data.data[0].nombre).toBe('Evento Test 1');
  });

  test('intercepts POST requests', async () => {
    const newEvento = {
      nombre: 'Nuevo Evento',
      descripcion: 'Descripción del nuevo evento',
      fecha: '2024-12-31T20:00:00Z',
      ubicacion: 'Teatro Test',
    };

    const response = await axios.post(`${GATEWAY_URL}/api/eventos`, newEvento);
    
    expect(response.status).toBe(201);
    expect(response.data.success).toBe(true);
    expect(response.data.data.nombre).toBe('Nuevo Evento');
  });

  test('handles query parameters', async () => {
    const response = await axios.get(`${GATEWAY_URL}/api/entradas/mis-entradas`, {
      params: { estado: 'Pagada' },
    });
    
    expect(response.status).toBe(200);
    expect(response.data.success).toBe(true);
    // Handler filters by estado
    expect(response.data.data.every((e: any) => e.estado === 'Pagada')).toBe(true);
  });

  test('handles path parameters', async () => {
    const eventoId = '123';
    const response = await axios.get(`${GATEWAY_URL}/api/eventos/${eventoId}`);
    
    expect(response.status).toBe(200);
    expect(response.data.data.id).toBe(eventoId);
  });
});

describe('MSW Error Handling', () => {
  test('can override handlers for 500 error', async () => {
    server.use(serverError500Handler);
    
    try {
      await axios.get(`${GATEWAY_URL}/api/eventos`);
      expect.fail('Should have thrown error');
    } catch (error: any) {
      expect(error.response.status).toBe(500);
      expect(error.response.data.message).toContain('Error del servidor');
    }
  });

  test('can override handlers for 404 error', async () => {
    server.use(notFound404Handler);
    
    try {
      await axios.get(`${GATEWAY_URL}/api/eventos/999`);
      expect.fail('Should have thrown error');
    } catch (error: any) {
      expect(error.response.status).toBe(404);
      expect(error.response.data.message).toContain('no encontrado');
    }
  });

  test('handlers are reset after each test', async () => {
    // This test runs after the 404 test above
    // If handlers weren't reset, this would still return 404
    const response = await axios.get(`${GATEWAY_URL}/api/eventos`);
    
    expect(response.status).toBe(200);
    expect(response.data.success).toBe(true);
  });
});
