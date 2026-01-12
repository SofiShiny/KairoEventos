/**
 * useCreateEntrada Hook
 * Custom hook for creating a new entrada using React Query mutation
 */

import { useMutation, useQueryClient } from '@tanstack/react-query';
import { createEntrada } from '../services';
import type { CreateEntradaDto, Entrada } from '../types';
import { useToast } from '@shared/components';

/**
 * Hook to create a new entrada (reserve a seat)
 * Automatically invalidates relevant queries on success
 * 
 * @returns Mutation result with mutate function, loading state, and error
 * 
 * @example
 * const { mutate: createEntradaMutation, isPending } = useCreateEntrada();
 * 
 * const handleReserve = () => {
 *   createEntradaMutation({
 *     eventoId: 'evento-123',
 *     asientoId: 'asiento-456',
 *     usuarioId: 'user-789'
 *   });
 * };
 */
export function useCreateEntrada() {
  const queryClient = useQueryClient();
  const { showSuccess, showError } = useToast();

  return useMutation<Entrada, Error, CreateEntradaDto>({
    mutationFn: createEntrada,
    onSuccess: (data) => {
      // Invalidate queries to refetch fresh data
      queryClient.invalidateQueries({ queryKey: ['mis-entradas'] });
      queryClient.invalidateQueries({ queryKey: ['asientos-disponibles', data.eventoId] });
      queryClient.invalidateQueries({ queryKey: ['eventos'] });
      queryClient.invalidateQueries({ queryKey: ['evento', data.eventoId] });
      
      // Show success message
      showSuccess('Entrada reservada exitosamente');
    },
    onError: (error) => {
      // Show error message
      showError(error.message || 'Error al reservar entrada');
    },
  });
}
