/**
 * Tipos para el módulo de Usuarios
 */

// Roles de usuario
export type RolUsuario = 'Admin' | 'Organizator' | 'Asistente';

// Interfaz principal de Usuario
export interface Usuario {
  id: string;
  username: string;
  nombre: string;
  correo: string;
  telefono: string;
  rol: RolUsuario;
  activo: boolean;
  fechaCreacion?: string; // ISO 8601 date string
  fechaActualizacion?: string; // ISO 8601 date string
}

// DTO para crear un usuario
export interface CreateUsuarioDto {
  username: string;
  nombre: string;
  correo: string;
  telefono: string;
  rol: RolUsuario;
  password: string;
}

// DTO para actualizar un usuario
export interface UpdateUsuarioDto {
  nombre?: string;
  correo?: string;
  telefono?: string;
  rol?: RolUsuario;
}

// Respuesta de la API
export interface UsuariosResponse {
  data: Usuario[];
  success: boolean;
  message?: string;
}

export interface UsuarioResponse {
  data: Usuario;
  success: boolean;
  message?: string;
}
