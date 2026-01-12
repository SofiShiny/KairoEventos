import { useQueryClient } from '@tanstack/react-query';

/**
 * Hook personalizado para invalidar queries de React Query
 * 
 * Proporciona una función para invalidar múltiples queries a la vez,
 * útil después de mutaciones que afectan múltiples recursos.
 * 
 * @example
 * const invalidate = useInvalidateQueries();
 * 
 * // Después de crear un evento
 * await createEvento(data);
 * invalidate(['eventos', 'dashboard-stats']);
 */
export function useInvalidateQueries() {
  const queryClient = useQueryClient();

  return (queryKeys: string[]) => {
    queryKeys.forEach((key) => {
      queryClient.invalidateQueries({ queryKey: [key] });
    });
  };
}
