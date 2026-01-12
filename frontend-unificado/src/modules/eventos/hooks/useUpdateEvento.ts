/**
 * useUpdateEvento Hook
 * Custom hook for updating an existing evento (Admin/Organizator only)
 */

import { useMutation, useQueryClient } from '@tanstack/react-query';
import { updateEvento } from '../services';
import type { UpdateEventoDto, Evento } from '../types';

interface UpdateEventoVariables {
  id: string;
  data: UpdateEventoDto;
}

/**
 * Hook to update an existing evento
 * Automatically invalidates 'eventos' and specific 'evento' queries on success
 * 
 * @returns Mutation result with mutate function, loading state, and error
 * 
 * @example
 * const { mutate: update, isPending, error } = useUpdateEvento();
 * 
 * const handleSubmit = (data: UpdateEventoDto) => {
 *   update({ id: eventoId, data }, {
 *     onSuccess: () => {
 *       toast.success('Evento actualizado exitosamente');
 *       navigate(`/eventos/${eventoId}`);
 *     },
 *     onError: (error) => {
 *       toast.error('Error al actualizar evento');
 *     }
 *   });
 * };
 */
export function useUpdateEvento() {
  const queryClient = useQueryClient();

  return useMutation<Evento, Error, UpdateEventoVariables>({
    mutationFn: ({ id, data }) => updateEvento(id, data),
    onSuccess: (updatedEvento) => {
      // Invalidate eventos list
      queryClient.invalidateQueries({ queryKey: ['eventos'] });
      
      // Invalidate specific evento query
      queryClient.invalidateQueries({ queryKey: ['evento', updatedEvento.id] });
      
      // Invalidate dashboard stats
      queryClient.invalidateQueries({ queryKey: ['dashboard'] });
    },
  });
}

