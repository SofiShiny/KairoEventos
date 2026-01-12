/**
 * useCreateEvento Hook
 * Custom hook for creating a new evento (Admin/Organizator only)
 */

import { useMutationWithInvalidation } from '@shared/hooks';
import { createEvento } from '../services';
import type { CreateEventoDto, Evento } from '../types';

/**
 * Hook to create a new evento
 * Automatically invalidates 'eventos' and 'dashboard' queries on success
 * 
 * @returns Mutation result with mutate function, loading state, and error
 * 
 * @example
 * const { mutate: create, isPending, error } = useCreateEvento();
 * 
 * const handleSubmit = (data: CreateEventoDto) => {
 *   create(data, {
 *     onSuccess: () => {
 *       toast.success('Evento creado exitosamente');
 *       navigate('/eventos');
 *     },
 *     onError: (error) => {
 *       toast.error('Error al crear evento');
 *     }
 *   });
 * };
 */
export function useCreateEvento() {
  return useMutationWithInvalidation<Evento, CreateEventoDto>(
    createEvento,
    ['eventos', 'dashboard'] // Invalidate eventos list and dashboard stats
  );
}

