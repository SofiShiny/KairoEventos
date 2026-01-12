// Tipos para respuestas de la API

// Respuesta estándar del Gateway
export interface ApiResponse<T> {
  data: T;
  success: boolean;
  message?: string;
}

// Respuesta de error
export interface ApiError {
  message: string;
  errors?: Record<string, string[]>; // Errores de validación
  statusCode: number;
}

// Respuesta paginada
export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

// Errores de validación para formularios
export interface ValidationErrors {
  validationErrors: Record<string, string[]>;
  message?: string;
}
