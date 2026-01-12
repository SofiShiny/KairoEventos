/**
 * useCreateUsuario Hook
 * Custom hook for creating a new usuario (Admin only)
 */

import { useMutationWithInvalidation } from '@shared/hooks';
import { createUsuario } from '../services';
import type { CreateUsuarioDto, Usuario } from '../types';

/**
 * Hook to create a new usuario
 * Automatically invalidates 'usuarios' query on success
 * 
 * @returns Mutation result with mutate function, loading state, and error
 * 
 * @example
 * const { mutate: create, isPending, error } = useCreateUsuario();
 * 
 * const handleSubmit = (data: CreateUsuarioDto) => {
 *   create(data, {
 *     onSuccess: () => {
 *       toast.success('Usuario creado exitosamente');
 *       navigate('/usuarios');
 *     },
 *     onError: (error) => {
 *       toast.error('Error al crear usuario');
 *     }
 *   });
 * };
 */
export function useCreateUsuario() {
  return useMutationWithInvalidation<Usuario, CreateUsuarioDto>(
    createUsuario,
    ['usuarios'] // Invalidate usuarios list
  );
}
