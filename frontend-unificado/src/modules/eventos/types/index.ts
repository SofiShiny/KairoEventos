/**
 * Tipos para el módulo de Eventos
 */

// Estado de un evento
export type EstadoEvento = 'Publicado' | 'Cancelado';

// Interfaz principal de Evento
export interface Evento {
  id: string;
  nombre: string;
  descripcion: string;
  fecha: string; // ISO 8601 date string
  ubicacion: string;
  imagenUrl?: string;
  estado: EstadoEvento;
  capacidadTotal: number;
  asientosDisponibles: number;
}

// DTO para crear un evento
export interface CreateEventoDto {
  nombre: string;
  descripcion: string;
  fecha: string; // ISO 8601 date string
  ubicacion: string;
  imagenUrl?: string;
}

// DTO para actualizar un evento
export interface UpdateEventoDto {
  nombre?: string;
  descripcion?: string;
  fecha?: string; // ISO 8601 date string
  ubicacion?: string;
  imagenUrl?: string;
}

// Filtros para búsqueda de eventos
export interface EventoFiltersData {
  fecha?: Date;
  ubicacion?: string;
  busqueda?: string;
}

