/**
 * Entradas Service
 * API calls for entradas (tickets) management
 */

import axiosClient from '@shared/api/axiosClient';
import type { Entrada, Asiento, CreateEntradaDto } from '../types';

/**
 * Normaliza una entrada del backend al formato que espera el frontend
 */
function normalizarEntrada(raw: any): Entrada {
  // Mapeo de estados de número a string
  const mapeoEstados: Record<number | string, string> = {
    0: 'Reservada',
    'Reservada': 'Reservada',
    1: 'Reservada', // PendientePago -> Reservada
    'PendientePago': 'Reservada',
    2: 'Pagada',
    'Pagada': 'Pagada',
    3: 'Cancelada',
    'Cancelada': 'Cancelada',
    4: 'Pagada', // Usada -> Pagada (para visualización)
    'Usada': 'Pagada'
  };

  const estadoNorm = mapeoEstados[raw.estado] || (typeof raw.estado === 'string' ? raw.estado : 'Pagada');

  return {
    id: raw.id,
    eventoId: raw.eventoId,
    // Soportar múltiples nombres de propiedad para el título
    eventoNombre: raw.eventoNombre || raw.tituloEvento || raw.titulo || 'Evento',
    asientoId: raw.asientoId,
    // Construir info del asiento si no viene formateada
    asientoInfo: raw.asientoInfo || (raw.fila ? `Fila ${raw.fila}, Asiento ${raw.numero || raw.numeroAsiento}` : 'General'),
    estado: estadoNorm as any,
    // Soportar múltiples nombres para el precio
    precio: raw.precio || raw.monto || raw.montoFinal || 0,
    fechaCompra: raw.fechaCompra || raw.fechaCreacion || new Date().toISOString(),
    tiempoRestante: raw.tiempoRestante
  };
}

/**
 * Fetch user's entradas (tickets)
 */
export async function fetchMisEntradas(usuarioId?: string): Promise<Entrada[]> {
  try {
    // Intentar primero con el endpoint genérico 'mis-entradas'
    const response = await axiosClient.get<{ data: any[] }>('/api/entradas/mis-entradas');
    const data = Array.isArray(response.data) ? response.data : (response.data.data || []);
    return data.map(normalizarEntrada);
  } catch (error) {
    // Si falla y tenemos el ID del usuario, intentar con el endpoint específico
    if (usuarioId) {
      const response = await axiosClient.get<{ data: any[] }>(`/api/entradas/usuario/${usuarioId}`);
      const data = Array.isArray(response.data) ? response.data : (response.data.data || []);
      return data.map(normalizarEntrada);
    }
    throw error;
  }
}

/**
 * Fetch available asientos (seats) for an evento
 * GET /api/entradas/asientos-disponibles/:eventoId
 */
export async function fetchAsientosDisponibles(eventoId: string): Promise<Asiento[]> {
  const response = await axiosClient.get<{ data: Asiento[] }>(
    `/api/entradas/asientos-disponibles/${eventoId}`
  );
  return response.data.data;
}

/**
 * Create a new entrada (reserve a seat)
 * POST /api/entradas
 */
export async function createEntrada(data: CreateEntradaDto): Promise<Entrada> {
  const response = await axiosClient.post<{ data: Entrada }>('/api/entradas', data);
  return response.data.data;
}

/**
 * Cancel an entrada
 * DELETE /api/entradas/:id
 */
export async function cancelarEntrada(id: string): Promise<void> {
  await axiosClient.delete(`/api/entradas/${id}`);
}
