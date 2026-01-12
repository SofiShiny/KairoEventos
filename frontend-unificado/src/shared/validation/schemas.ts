/**
 * Centralized Validation Schemas
 * Zod schemas for form validation across the application
 * 
 * Requirements:
 * - 14.1: Use react-hook-form and zod for validation
 * - 14.2: Validate required fields
 * - 14.3: Validate email format
 * - 14.4: Validate min/max length
 * - 14.5: Specific error messages per field
 */

import { z } from 'zod';

/**
 * Evento Validation Schema
 * Used for creating and editing eventos
 */
export const eventoSchema = z.object({
  nombre: z
    .string()
    .min(1, 'El nombre es requerido')
    .min(3, 'El nombre debe tener al menos 3 caracteres')
    .max(100, 'El nombre no puede exceder 100 caracteres'),
  
  descripcion: z
    .string()
    .min(1, 'La descripción es requerida')
    .min(10, 'La descripción debe tener al menos 10 caracteres')
    .max(500, 'La descripción no puede exceder 500 caracteres'),
  
  fecha: z
    .string()
    .min(1, 'La fecha es requerida')
    .refine((date) => {
      const selectedDate = new Date(date + 'T00:00:00');
      const today = new Date();
      today.setHours(0, 0, 0, 0);
      return selectedDate >= today;
    }, 'La fecha debe ser hoy o en el futuro'),
  
  ubicacion: z
    .string()
    .min(1, 'La ubicación es requerida')
    .min(3, 'La ubicación debe tener al menos 3 caracteres')
    .max(200, 'La ubicación no puede exceder 200 caracteres'),
  
  imagenUrl: z
    .union([
      z.string().url('La URL de la imagen no es válida'),
      z.literal(''),
    ])
    .optional(),
});

export type EventoFormData = z.infer<typeof eventoSchema>;

/**
 * Usuario Validation Schema
 * Used for creating and editing usuarios
 */
export const usuarioSchema = z.object({
  username: z
    .string()
    .min(1, 'El username es requerido')
    .min(3, 'El username debe tener al menos 3 caracteres')
    .max(50, 'El username no puede exceder 50 caracteres')
    .regex(
      /^[a-zA-Z0-9_-]+$/,
      'El username solo puede contener letras, números, guiones y guiones bajos'
    ),
  
  nombre: z
    .string()
    .min(1, 'El nombre es requerido')
    .min(3, 'El nombre debe tener al menos 3 caracteres')
    .max(100, 'El nombre no puede exceder 100 caracteres'),
  
  correo: z
    .string()
    .min(1, 'El correo electrónico es requerido')
    .email('El formato del correo electrónico no es válido')
    .max(100, 'El correo electrónico no puede exceder 100 caracteres'),
  
  telefono: z
    .string()
    .min(1, 'El teléfono es requerido')
    .regex(
      /^\+?[0-9]{10,15}$/,
      'El teléfono debe tener entre 10 y 15 dígitos (puede incluir + al inicio)'
    ),
  
  rol: z.enum(['Admin', 'Organizator', 'Asistente'], {
    message: 'Debe seleccionar un rol válido',
  }),
  
  password: z
    .string()
    .min(8, 'La contraseña debe tener al menos 8 caracteres')
    .regex(
      /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)/,
      'La contraseña debe contener al menos una mayúscula, una minúscula y un número'
    ),
});

/**
 * Usuario Edit Schema
 * Password is optional when editing
 */
export const usuarioEditSchema = usuarioSchema.extend({
  password: z
    .string()
    .min(8, 'La contraseña debe tener al menos 8 caracteres')
    .regex(
      /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)/,
      'La contraseña debe contener al menos una mayúscula, una minúscula y un número'
    )
    .optional()
    .or(z.literal('')),
});

export type UsuarioEditFormData = z.infer<typeof usuarioEditSchema>;

// Union type for form data that handles both create and edit scenarios
export type UsuarioFormData = z.infer<typeof usuarioSchema> | z.infer<typeof usuarioEditSchema>;

/**
 * Entrada Validation Schema
 * Used for creating entradas (ticket purchases)
 */
export const entradaSchema = z.object({
  eventoId: z
    .string()
    .min(1, 'El evento es requerido')
    .uuid('El ID del evento no es válido'),
  
  asientoId: z
    .string()
    .min(1, 'El asiento es requerido')
    .uuid('El ID del asiento no es válido'),
  
  usuarioId: z
    .string()
    .min(1, 'El usuario es requerido')
    .uuid('El ID del usuario no es válido'),
});

export type EntradaFormData = z.infer<typeof entradaSchema>;

/**
 * Login Validation Schema
 * Basic validation for login forms (if needed)
 */
export const loginSchema = z.object({
  username: z
    .string()
    .min(1, 'El username es requerido'),
  
  password: z
    .string()
    .min(1, 'La contraseña es requerida'),
});

export type LoginFormData = z.infer<typeof loginSchema>;

/**
 * Reporte Filtros Validation Schema
 * Used for filtering reports
 */
export const reporteFiltrosSchema = z.object({
  fechaInicio: z
    .string()
    .min(1, 'La fecha de inicio es requerida'),
  
  fechaFin: z
    .string()
    .min(1, 'La fecha de fin es requerida'),
  
  eventoId: z
    .union([
      z.string().uuid('El ID del evento no es válido'),
      z.literal(''),
    ])
    .optional(),
}).refine((data) => {
  if (!data.fechaInicio || !data.fechaFin) return true;
  return new Date(data.fechaFin) >= new Date(data.fechaInicio);
}, {
  message: 'La fecha de fin debe ser posterior o igual a la fecha de inicio',
  path: ['fechaFin'],
});

export type ReporteFiltrosFormData = z.infer<typeof reporteFiltrosSchema>;

/**
 * Search/Filter Validation Schema
 * Generic schema for search and filter forms
 */
export const searchSchema = z.object({
  query: z
    .string()
    .max(200, 'La búsqueda no puede exceder 200 caracteres')
    .optional()
    .default(''),
  
  fecha: z
    .string()
    .optional()
    .default(''),
  
  ubicacion: z
    .string()
    .max(200, 'La ubicación no puede exceder 200 caracteres')
    .optional()
    .default(''),
});

export type SearchFormData = z.infer<typeof searchSchema>;

/**
 * Helper function to get error message from Zod error
 */
export function getZodErrorMessage(error: unknown): string {
  if (error instanceof z.ZodError) {
    const issues = error.issues || [];
    return issues[0]?.message || 'Error de validación';
  }
  return 'Error de validación';
}

/**
 * Helper function to check if a field has an error
 */
export function hasFieldError(
  errors: Record<string, unknown>,
  fieldName: string
): boolean {
  return !!errors[fieldName];
}

/**
 * Helper function to get field error message
 */
export function getFieldErrorMessage(
  errors: Record<string, { message?: string }>,
  fieldName: string
): string | undefined {
  return errors[fieldName]?.message;
}
