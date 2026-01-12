/**
 * Validation Module
 * Exports all validation schemas and utilities
 */

export {
  eventoSchema,
  usuarioSchema,
  usuarioEditSchema,
  entradaSchema,
  loginSchema,
  reporteFiltrosSchema,
  searchSchema,
  getZodErrorMessage,
  hasFieldError,
  getFieldErrorMessage,
  type EventoFormData,
  type UsuarioFormData,
  type UsuarioEditFormData,
  type EntradaFormData,
  type LoginFormData,
  type ReporteFiltrosFormData,
  type SearchFormData,
} from './schemas';
