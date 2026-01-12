/**
 * useUpdateUsuario Hook
 * Custom hook for updating an existing usuario (Admin only)
 */

import { useMutation, useQueryClient } from '@tanstack/react-query';
import { updateUsuario } from '../services';
import type { UpdateUsuarioDto, Usuario } from '../types';

interface UpdateUsuarioParams {
  id: string;
  data: UpdateUsuarioDto;
}

/**
 * Hook to update an existing usuario
 * Automatically invalidates 'usuarios' and specific 'usuario' queries on success
 * 
 * @returns Mutation result with mutate function, loading state, and error
 * 
 * @example
 * const { mutate: update, isPending, error } = useUpdateUsuario();
 * 
 * const handleSubmit = (data: UpdateUsuarioDto) => {
 *   update({ id: '123', data }, {
 *     onSuccess: () => {
 *       toast.success('Usuario actualizado exitosamente');
 *       navigate('/usuarios');
 *     },
 *     onError: (error) => {
 *       toast.error('Error al actualizar usuario');
 *     }
 *   });
 * };
 */
export function useUpdateUsuario() {
  const queryClient = useQueryClient();

  return useMutation<Usuario, Error, UpdateUsuarioParams>({
    mutationFn: ({ id, data }) => updateUsuario(id, data),
    onSuccess: (_, variables) => {
      // Invalidate usuarios list
      queryClient.invalidateQueries({ queryKey: ['usuarios'] });
      // Invalidate specific usuario
      queryClient.invalidateQueries({ queryKey: ['usuario', variables.id] });
    },
  });
}
