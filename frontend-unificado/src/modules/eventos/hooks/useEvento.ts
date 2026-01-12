/**
 * useEvento Hook
 * Custom hook for fetching a single evento by ID using React Query
 */

import { useQuery } from '@tanstack/react-query';
import { fetchEvento } from '../services';
import type { Evento } from '../types';

/**
 * Hook to fetch a single evento by ID
 * 
 * @param id - Evento ID
 * @returns Query result with evento data, loading state, and error
 * 
 * @example
 * const { data: evento, isLoading, error } = useEvento('123');
 */
export function useEvento(id: string) {
  return useQuery<Evento, Error>({
    queryKey: ['evento', id],
    queryFn: () => fetchEvento(id),
    staleTime: 5 * 60 * 1000, // 5 minutes
    retry: 3,
    enabled: !!id, // Only run query if id is provided
  });
}

