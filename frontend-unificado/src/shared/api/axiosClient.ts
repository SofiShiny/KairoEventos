import axios, { AxiosError } from 'axios';
import type { InternalAxiosRequestConfig } from 'axios';

// Tipos para respuestas de error del Gateway
interface ApiErrorResponse {
  message: string;
  errors?: Record<string, string[]>;
  statusCode: number;
}

// Crear instancia de Axios configurada con baseURL del Gateway
const axiosClient = axios.create({
  baseURL: import.meta.env.VITE_GATEWAY_URL || 'http://localhost:8080',
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Variable para rastrear si ya estamos redirigiendo al login
let isRedirectingToLogin = false;

// Request Interceptor: Agregar token JWT en header Authorization
axiosClient.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    // Obtener token del localStorage
    const token = localStorage.getItem('auth_token');
    
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response Interceptor: Manejo de errores HTTP con retry logic
axiosClient.interceptors.response.use(
  (response) => {
    // Respuesta exitosa, retornar data directamente
    return response;
  },
  async (error: AxiosError<ApiErrorResponse>) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean; _retryCount?: number };
    
    // Si no hay respuesta, es un error de red
    if (!error.response) {
      // Implementar retry logic con backoff exponencial para errores de red
      if (!originalRequest._retry && (!originalRequest._retryCount || originalRequest._retryCount < 3)) {
        originalRequest._retry = true;
        originalRequest._retryCount = (originalRequest._retryCount || 0) + 1;
        
        // Backoff exponencial: 1s, 2s, 4s
        const delay = Math.min(1000 * Math.pow(2, originalRequest._retryCount - 1), 30000);
        
        await new Promise(resolve => setTimeout(resolve, delay));
        
        return axiosClient(originalRequest);
      }
      
      // Si ya reintentamos 3 veces, mostrar error de conexión
      console.error('Error de conexión. Intente nuevamente.');
      return Promise.reject(new Error('Error de conexión. Intente nuevamente.'));
    }
    
    const status = error.response.status;
    
    // Manejo de errores según código HTTP
    switch (status) {
      case 401:
        // 401 Unauthorized: Redirigir al login automáticamente
        if (!isRedirectingToLogin) {
          isRedirectingToLogin = true;
          
          // Limpiar autenticación
          localStorage.removeItem('auth_token');
          localStorage.removeItem('auth_user');
          sessionStorage.clear();
          
          // Redirigir al login
          window.location.href = '/login';
        }
        break;
      
      case 403:
        // 403 Forbidden: No tiene permisos
        console.error('No tiene permisos para realizar esta acción');
        break;
      
      case 404:
        // 404 Not Found: Recurso no encontrado
        console.error('Recurso no encontrado');
        break;
      
      case 400:
        // 400 Bad Request: Errores de validación
        const validationErrors = error.response.data?.errors;
        if (validationErrors) {
          // Los errores de validación se propagan para ser manejados por los formularios
          return Promise.reject({ validationErrors, message: error.response.data?.message });
        }
        console.error(error.response.data?.message || 'Solicitud inválida');
        break;
      
      case 500:
      case 502:
      case 503:
        // 500/502/503: Errores del servidor
        console.error('Error del servidor. Intente más tarde.');
        
        // Implementar retry logic con backoff exponencial para errores 5xx
        if (!originalRequest._retry && (!originalRequest._retryCount || originalRequest._retryCount < 3)) {
          originalRequest._retry = true;
          originalRequest._retryCount = (originalRequest._retryCount || 0) + 1;
          
          // Backoff exponencial: 1s, 2s, 4s
          const delay = Math.min(1000 * Math.pow(2, originalRequest._retryCount - 1), 30000);
          
          await new Promise(resolve => setTimeout(resolve, delay));
          
          return axiosClient(originalRequest);
        }
        break;
      
      default:
        console.error('Error inesperado:', error.response.data?.message || error.message);
    }
    
    return Promise.reject(error);
  }
);

export default axiosClient;
