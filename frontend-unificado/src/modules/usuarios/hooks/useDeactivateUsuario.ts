/**
 * useDeactivateUsuario Hook
 * Custom hook for deactivating a usuario (Admin only)
 */

import { useMutation, useQueryClient } from '@tanstack/react-query';
import { deactivateUsuario } from '../services';

/**
 * Hook to deactivate a usuario (soft delete)
 * Automatically invalidates 'usuarios' query on success
 * 
 * @returns Mutation result with mutate function, loading state, and error
 * 
 * @example
 * const { mutate: deactivate, isPending, error } = useDeactivateUsuario();
 * 
 * const handleDeactivate = (usuarioId: string) => {
 *   if (confirm('¿Está seguro de desactivar este usuario?')) {
 *     deactivate(usuarioId, {
 *       onSuccess: () => {
 *         toast.success('Usuario desactivado exitosamente');
 *       },
 *       onError: (error) => {
 *         toast.error('Error al desactivar usuario');
 *       }
 *     });
 *   }
 * };
 */
export function useDeactivateUsuario() {
  const queryClient = useQueryClient();

  return useMutation<void, Error, string>({
    mutationFn: (id: string) => deactivateUsuario(id),
    onSuccess: () => {
      // Invalidate usuarios list to refresh the data
      queryClient.invalidateQueries({ queryKey: ['usuarios'] });
    },
  });
}
