/**
 * useUsuario Hook
 * Custom hook for fetching a single usuario by ID using React Query
 * Admin only
 */

import { useQuery } from '@tanstack/react-query';
import { fetchUsuario } from '../services';
import type { Usuario } from '../types';

/**
 * Hook to fetch a single usuario by ID
 * 
 * @param id - Usuario ID
 * @returns Query result with usuario data, loading state, and error
 * 
 * @example
 * const { data: usuario, isLoading, error } = useUsuario('123');
 */
export function useUsuario(id: string) {
  return useQuery<Usuario, Error>({
    queryKey: ['usuario', id],
    queryFn: () => fetchUsuario(id),
    staleTime: 5 * 60 * 1000, // 5 minutes
    retry: 3,
    enabled: !!id, // Only run query if id is provided
  });
}
