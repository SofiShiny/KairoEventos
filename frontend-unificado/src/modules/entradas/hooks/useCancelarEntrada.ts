/**
 * useCancelarEntrada Hook
 * Custom hook for canceling an entrada using React Query mutation
 */

import { useMutation, useQueryClient } from '@tanstack/react-query';
import { cancelarEntrada } from '../services';
import { useToast } from '@shared/components';

/**
 * Hook to cancel an entrada
 * Automatically invalidates relevant queries on success
 * 
 * @returns Mutation result with mutate function, loading state, and error
 * 
 * @example
 * const { mutate: cancelarEntradaMutation, isPending } = useCancelarEntrada();
 * 
 * const handleCancel = (entradaId: string) => {
 *   if (confirm('¿Está seguro de cancelar esta entrada?')) {
 *     cancelarEntradaMutation(entradaId);
 *   }
 * };
 */
export function useCancelarEntrada() {
  const queryClient = useQueryClient();
  const { showSuccess, showError } = useToast();

  return useMutation<void, Error, string>({
    mutationFn: cancelarEntrada,
    onSuccess: () => {
      // Invalidate queries to refetch fresh data
      queryClient.invalidateQueries({ queryKey: ['mis-entradas'] });
      queryClient.invalidateQueries({ queryKey: ['asientos-disponibles'] });
      queryClient.invalidateQueries({ queryKey: ['eventos'] });
      
      // Show success message
      showSuccess('Entrada cancelada exitosamente');
    },
    onError: (error) => {
      // Show error message
      showError(error.message || 'Error al cancelar entrada');
    },
  });
}
