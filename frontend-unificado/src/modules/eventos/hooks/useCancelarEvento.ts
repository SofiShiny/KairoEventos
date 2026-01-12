/**
 * useCancelarEvento Hook
 * Custom hook for canceling an evento (Admin/Organizator only)
 */

import { useMutation, useQueryClient } from '@tanstack/react-query';
import { cancelEvento } from '../services';

/**
 * Hook to cancel an evento
 * Automatically invalidates 'eventos' and specific 'evento' queries on success
 * 
 * @returns Mutation result with mutate function, loading state, and error
 * 
 * @example
 * const { mutate: cancelar, isPending, error } = useCancelarEvento();
 * 
 * const handleCancel = () => {
 *   if (confirm('¿Está seguro de cancelar este evento?')) {
 *     cancelar(eventoId, {
 *       onSuccess: () => {
 *         toast.success('Evento cancelado exitosamente');
 *         navigate('/eventos');
 *       },
 *       onError: (error) => {
 *         toast.error('Error al cancelar evento');
 *       }
 *     });
 *   }
 * };
 */
export function useCancelarEvento() {
  const queryClient = useQueryClient();

  return useMutation<void, Error, string>({
    mutationFn: (id: string) => cancelEvento(id),
    onSuccess: (_, eventoId) => {
      // Invalidate eventos list
      queryClient.invalidateQueries({ queryKey: ['eventos'] });
      
      // Invalidate specific evento query
      queryClient.invalidateQueries({ queryKey: ['evento', eventoId] });
      
      // Invalidate dashboard stats
      queryClient.invalidateQueries({ queryKey: ['dashboard'] });
    },
  });
}

