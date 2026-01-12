import { useMutation, useQueryClient } from '@tanstack/react-query';
import type { UseMutationOptions } from '@tanstack/react-query';

/**
 * Hook personalizado que combina useMutation con invalidación automática de caché
 * 
 * Este hook simplifica el patrón común de:
 * 1. Ejecutar una mutación (crear, actualizar, eliminar)
 * 2. Invalidar queries relacionadas para refrescar los datos
 * 
 * Performance optimizations:
 * - Automatic cache invalidation
 * - Support for optimistic updates
 * 
 * @param mutationFn - Función que ejecuta la mutación
 * @param queriesToInvalidate - Array de query keys a invalidar después de la mutación exitosa
 * @param options - Opciones adicionales de useMutation
 * 
 * @example
 * const createEvento = useMutationWithInvalidation(
 *   (data: CreateEventoDto) => eventosService.create(data),
 *   ['eventos', 'dashboard-stats'],
 *   {
 *     onError: (error) => {
 *       toast.error('Error al crear evento');
 *     }
 *   }
 * );
 */
export function useMutationWithInvalidation<TData, TVariables, TError = Error, TContext = unknown>(
  mutationFn: (variables: TVariables) => Promise<TData>,
  queriesToInvalidate: string[],
  options?: Omit<UseMutationOptions<TData, TError, TVariables, TContext>, 'mutationFn'>
) {
  const queryClient = useQueryClient();

  return useMutation<TData, TError, TVariables, TContext>({
    mutationFn,
    onSuccess: (data, variables, context) => {
      // Invalidar todas las queries especificadas
      queriesToInvalidate.forEach((key) => {
        queryClient.invalidateQueries({ queryKey: [key] });
      });

      // Llamar al onSuccess personalizado si existe
      if (options?.onSuccess) {
        // @ts-expect-error - Context type mismatch is expected here
        options.onSuccess(data, variables, context);
      }
    },
    ...options,
  });
}

/**
 * Hook personalizado con soporte para optimistic updates
 * 
 * Optimistic updates mejoran la UX al actualizar la UI inmediatamente
 * antes de que el servidor responda, y revertir si hay error
 * 
 * @param mutationFn - Función que ejecuta la mutación
 * @param queryKey - Query key a actualizar optimísticamente
 * @param optimisticUpdateFn - Función que actualiza los datos optimísticamente
 * @param options - Opciones adicionales de useMutation
 * 
 * @example
 * const updateEvento = useMutationWithOptimisticUpdate(
 *   (data: UpdateEventoDto) => eventosService.update(data.id, data),
 *   ['eventos'],
 *   (oldData: Evento[], newData: UpdateEventoDto) => {
 *     return oldData.map(e => e.id === newData.id ? { ...e, ...newData } : e);
 *   }
 * );
 */
export function useMutationWithOptimisticUpdate<TData, TVariables, TError = Error>(
  mutationFn: (variables: TVariables) => Promise<TData>,
  queryKey: unknown[],
  optimisticUpdateFn: (oldData: TData | undefined, variables: TVariables) => TData,
  options?: Omit<UseMutationOptions<TData, TError, TVariables, TData | undefined>, 'mutationFn' | 'onMutate' | 'onError' | 'onSettled'>
) {
  const queryClient = useQueryClient();

  return useMutation<TData, TError, TVariables, TData | undefined>({
    mutationFn,
    // Antes de la mutación, actualizar optimísticamente
    onMutate: async (variables) => {
      // Cancelar queries en progreso para evitar sobrescribir el update optimista
      await queryClient.cancelQueries({ queryKey });

      // Snapshot del valor anterior
      const previousData = queryClient.getQueryData<TData>(queryKey);

      // Actualizar optimísticamente
      if (previousData !== undefined) {
        queryClient.setQueryData<TData>(queryKey, optimisticUpdateFn(previousData, variables));
      }

      // Retornar contexto con datos anteriores para rollback
      return previousData;
    },
    // Si hay error, revertir al snapshot anterior
    onError: (_error, _variables, context) => {
      if (context !== undefined) {
        queryClient.setQueryData(queryKey, context);
      }
    },
    // Siempre refetch después de error o éxito
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey });
    },
    ...options,
  });
}
