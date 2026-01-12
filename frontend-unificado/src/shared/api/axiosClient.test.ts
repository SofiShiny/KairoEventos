import { describe, it, expect, beforeEach, vi } from 'vitest';
import axiosClient from './axiosClient';

describe('Axios Client Configuration', () => {
  beforeEach(() => {
    // Clear localStorage before each test
    localStorage.clear();
    vi.clearAllMocks();
  });

  it('should be configured with correct baseURL', () => {
    expect(axiosClient.defaults.baseURL).toBeDefined();
    expect(axiosClient.defaults.timeout).toBe(30000);
    expect(axiosClient.defaults.headers['Content-Type']).toBe('application/json');
  });

  it('should have request interceptor configured', () => {
    // @ts-expect-error - accessing internal property for testing
    expect(axiosClient.interceptors.request.handlers.length).toBeGreaterThan(0);
  });

  it('should have response interceptor configured', () => {
    // @ts-expect-error - accessing internal property for testing
    expect(axiosClient.interceptors.response.handlers.length).toBeGreaterThan(0);
  });

  describe('Request Interceptor', () => {
    it('should add Authorization header when token exists in localStorage', async () => {
      const token = 'test-jwt-token';
      localStorage.setItem('auth_token', token);

      // Get the request interceptor
      // @ts-expect-error - accessing internal property for testing
      const requestInterceptor = axiosClient.interceptors.request.handlers[0];
      
      const config = {
        headers: {},
      } as any;

      const result = await requestInterceptor.fulfilled(config);
      
      expect(result.headers.Authorization).toBe(`Bearer ${token}`);
    });

    it('should not add Authorization header when token does not exist', async () => {
      // Get the request interceptor
      // @ts-expect-error - accessing internal property for testing
      const requestInterceptor = axiosClient.interceptors.request.handlers[0];
      
      const config = {
        headers: {},
      } as any;

      const result = await requestInterceptor.fulfilled(config);
      
      expect(result.headers.Authorization).toBeUndefined();
    });
  });

  describe('Response Interceptor - Error Handling', () => {
    it('should handle network errors', async () => {
      // @ts-expect-error - accessing internal property for testing
      const responseInterceptor = axiosClient.interceptors.response.handlers[0];
      
      const networkError = {
        config: { _retryCount: 3 }, // Already retried 3 times
        message: 'Network Error',
      };

      try {
        await responseInterceptor.rejected(networkError);
        // Should throw
        expect.fail('Should have thrown an error');
      } catch (error: any) {
        expect(error.message).toContain('Error de conexión');
      }
    });

    it('should handle 401 errors by clearing localStorage', async () => {
      localStorage.setItem('auth_token', 'test-token');
      localStorage.setItem('auth_user', 'test-user');
      
      // @ts-expect-error - accessing internal property for testing
      const responseInterceptor = axiosClient.interceptors.response.handlers[0];
      
      const error401 = {
        config: {},
        response: {
          status: 401,
          data: {},
        },
      };

      // Mock window.location.href
      delete (window as any).location;
      (window as any).location = { href: '' };

      try {
        await responseInterceptor.rejected(error401);
      } catch (error) {
        // Error should be rejected
      }

      // Token should be cleared
      expect(localStorage.getItem('auth_token')).toBeNull();
      expect(localStorage.getItem('auth_user')).toBeNull();
    });

    it('should handle 403 errors', async () => {
      const consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      
      // @ts-expect-error - accessing internal property for testing
      const responseInterceptor = axiosClient.interceptors.response.handlers[0];
      
      const error403 = {
        config: {},
        response: {
          status: 403,
          data: {},
        },
      };

      try {
        await responseInterceptor.rejected(error403);
      } catch (error) {
        // Error should be rejected
      }

      expect(consoleErrorSpy).toHaveBeenCalledWith('No tiene permisos para realizar esta acción');
      consoleErrorSpy.mockRestore();
    });

    it('should handle 404 errors', async () => {
      const consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      
      // @ts-expect-error - accessing internal property for testing
      const responseInterceptor = axiosClient.interceptors.response.handlers[0];
      
      const error404 = {
        config: {},
        response: {
          status: 404,
          data: {},
        },
      };

      try {
        await responseInterceptor.rejected(error404);
      } catch (error) {
        // Error should be rejected
      }

      expect(consoleErrorSpy).toHaveBeenCalledWith('Recurso no encontrado');
      consoleErrorSpy.mockRestore();
    });

    it('should handle 400 errors with validation errors', async () => {
      // @ts-expect-error - accessing internal property for testing
      const responseInterceptor = axiosClient.interceptors.response.handlers[0];
      
      const validationErrors = {
        email: ['Email is required'],
        password: ['Password must be at least 8 characters'],
      };

      const error400 = {
        config: {},
        response: {
          status: 400,
          data: {
            message: 'Validation failed',
            errors: validationErrors,
          },
        },
      };

      try {
        await responseInterceptor.rejected(error400);
      } catch (error: any) {
        expect(error.validationErrors).toEqual(validationErrors);
        expect(error.message).toBe('Validation failed');
      }
    });

    it('should handle 500 errors', async () => {
      const consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      
      // @ts-expect-error - accessing internal property for testing
      const responseInterceptor = axiosClient.interceptors.response.handlers[0];
      
      const error500 = {
        config: {},
        response: {
          status: 500,
          data: {},
        },
      };

      try {
        await responseInterceptor.rejected(error500);
      } catch (error) {
        // Error should be rejected
      }

      expect(consoleErrorSpy).toHaveBeenCalledWith('Error del servidor. Intente más tarde.');
      consoleErrorSpy.mockRestore();
    });
  });

  describe('Exclusive Gateway Communication', () => {
    it('should only communicate with Gateway URL from environment', () => {
      const baseURL = axiosClient.defaults.baseURL;
      
      // Should be Gateway URL, not direct microservice URLs
      expect(baseURL).not.toContain('eventos');
      expect(baseURL).not.toContain('usuarios');
      expect(baseURL).not.toContain('entradas');
      expect(baseURL).not.toContain('reportes');
      
      // Should be Gateway URL (either from env or default)
      expect(baseURL).toMatch(/localhost:8080|gateway/i);
    });
  });
});
