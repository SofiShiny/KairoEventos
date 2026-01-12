/**
 * Eventos Service
 * API calls for eventos management
 */

import axiosClient from '@shared/api/axiosClient';
import type { Evento, CreateEventoDto, UpdateEventoDto } from '../types';

/**
 * Fetch all eventos
 * GET /api/eventos
 */
export async function fetchEventos(): Promise<Evento[]> {
  const response = await axiosClient.get<{ data: Evento[] }>('/api/eventos');
  return response.data.data;
}

/**
 * Fetch a single evento by ID
 * GET /api/eventos/:id
 */
export async function fetchEvento(id: string): Promise<Evento> {
  const response = await axiosClient.get<{ data: Evento }>(`/api/eventos/${id}`);
  return response.data.data;
}

/**
 * Create a new evento (Admin/Organizator only)
 * POST /api/eventos
 */
export async function createEvento(data: CreateEventoDto): Promise<Evento> {
  const response = await axiosClient.post<{ data: Evento }>('/api/eventos', data);
  return response.data.data;
}

/**
 * Update an existing evento (Admin/Organizator only)
 * PUT /api/eventos/:id
 */
export async function updateEvento(id: string, data: UpdateEventoDto): Promise<Evento> {
  const response = await axiosClient.put<{ data: Evento }>(`/api/eventos/${id}`, data);
  return response.data.data;
}

/**
 * Cancel an evento (Admin/Organizator only)
 * DELETE /api/eventos/:id/cancelar
 */
export async function cancelEvento(id: string): Promise<void> {
  await axiosClient.delete(`/api/eventos/${id}/cancelar`);
}

