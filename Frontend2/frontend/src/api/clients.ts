import axios from 'axios';

// Create axios instance for events API
export const eventsApi = axios.create({
  baseURL: import.meta.env.VITE_EVENTS_API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Create axios instance for seats API
export const seatsApi = axios.create({
  baseURL: import.meta.env.VITE_SEATS_API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor for authentication
const authInterceptor = (config: any) => {
  // Check for auth token in localStorage
  const token = localStorage.getItem('authToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
};

// Response interceptor for error handling
const errorInterceptor = (error: any) => {
  if (error.response) {
    // Server responded with error status
    const { status, data } = error.response;
    
    // Log error for debugging
    console.error(`API Error [${status}]:`, data);
    
    // Handle specific error cases
    switch (status) {
      case 401:
        // Unauthorized - could trigger logout or token refresh
        console.warn('Unauthorized request - authentication may be required');
        break;
      case 403:
        // Forbidden - user doesn't have permission
        console.warn('Forbidden - insufficient permissions');
        break;
      case 404:
        // Not found
        console.warn('Resource not found');
        break;
      case 409:
        // Conflict - e.g., duplicate resource
        console.warn('Conflict - resource may already exist');
        break;
      case 500:
      case 502:
      case 503:
        // Server errors
        console.error('Server error - please try again later');
        break;
    }
  } else if (error.request) {
    // Request was made but no response received
    console.error('Network error - no response from server');
  } else {
    // Something else happened
    console.error('Request error:', error.message);
  }
  
  return Promise.reject(error);
};

// Apply interceptors to both API clients
eventsApi.interceptors.request.use(authInterceptor, (error) => Promise.reject(error));
eventsApi.interceptors.response.use((response) => response, errorInterceptor);

seatsApi.interceptors.request.use(authInterceptor, (error) => Promise.reject(error));
seatsApi.interceptors.response.use((response) => response, errorInterceptor);
