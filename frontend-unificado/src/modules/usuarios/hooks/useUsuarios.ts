/**
 * useUsuarios Hook
 * Custom hook for fetching list of usuarios using React Query
 * Admin only
 */

import { useQuery } from '@tanstack/react-query';
import { fetchUsuarios } from '../services';
import type { Usuario } from '../types';

/**
 * Hook to fetch all usuarios
 * 
 * @returns Query result with usuarios list, loading state, and error
 * 
 * @example
 * const { data: usuarios, isLoading, error, refetch } = useUsuarios();
 */
export function useUsuarios() {
  return useQuery<Usuario[], Error>({
    queryKey: ['usuarios'],
    queryFn: fetchUsuarios,
    staleTime: 5 * 60 * 1000, // 5 minutes
    retry: 3,
  });
}
