/**
 * Tipos para el módulo de Entradas
 */

// Estado de una entrada
export type EstadoEntrada = 'Reservada' | 'Pagada' | 'Cancelada';

// Estado de un asiento
export type EstadoAsiento = 'Disponible' | 'Reservado' | 'Ocupado';

// Interfaz principal de Entrada
export interface Entrada {
  id: string;
  eventoId: string;
  eventoNombre: string;
  asientoId: string;
  asientoInfo: string; // e.g., "Fila A - Asiento 12"
  estado: EstadoEntrada;
  precio: number;
  fechaCompra: string; // ISO 8601 date string
  tiempoRestante?: number; // minutos restantes para pagar (solo para Reservada)
}

// Interfaz de Asiento
export interface Asiento {
  id: string;
  fila: string;
  numero: number;
  estado: EstadoAsiento;
  precio: number;
}

// DTO para crear una entrada
export interface CreateEntradaDto {
  eventoId: string;
  asientoId: string;
  usuarioId: string;
}

// Filtros para entradas
export type FiltroEstadoEntrada = 'Todas' | EstadoEntrada;
