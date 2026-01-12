/**
 * Usuarios Service
 * API calls for usuarios management (Admin only)
 */

import axiosClient from '@shared/api/axiosClient';
import type { Usuario, CreateUsuarioDto, UpdateUsuarioDto } from '../types';

/**
 * Fetch all usuarios
 * GET /api/usuarios
 * Admin only
 */
export async function fetchUsuarios(): Promise<Usuario[]> {
  const response = await axiosClient.get<{ data: Usuario[] }>('/api/usuarios');
  return response.data.data;
}

/**
 * Fetch a single usuario by ID
 * GET /api/usuarios/:id
 * Admin only
 */
export async function fetchUsuario(id: string): Promise<Usuario> {
  const response = await axiosClient.get<{ data: Usuario }>(`/api/usuarios/${id}`);
  return response.data.data;
}

/**
 * Create a new usuario
 * POST /api/usuarios
 * Admin only
 */
export async function createUsuario(data: CreateUsuarioDto): Promise<Usuario> {
  const response = await axiosClient.post<{ data: Usuario }>('/api/usuarios', data);
  return response.data.data;
}

/**
 * Update an existing usuario
 * PUT /api/usuarios/:id
 * Admin only
 */
export async function updateUsuario(id: string, data: UpdateUsuarioDto): Promise<Usuario> {
  const response = await axiosClient.put<{ data: Usuario }>(`/api/usuarios/${id}`, data);
  return response.data.data;
}

/**
 * Deactivate a usuario (soft delete)
 * DELETE /api/usuarios/:id
 * Admin only
 */
export async function deactivateUsuario(id: string): Promise<void> {
  await axiosClient.delete(`/api/usuarios/${id}`);
}
