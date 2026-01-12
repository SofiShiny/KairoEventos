/**
 * useAsientosDisponibles Hook
 * Custom hook for fetching available asientos for an evento using React Query
 */

import { useQuery } from '@tanstack/react-query';
import { fetchAsientosDisponibles } from '../services';
import type { Asiento } from '../types';

/**
 * Hook to fetch available asientos for a specific evento
 * 
 * @param eventoId - ID of the evento
 * @returns Query result with asientos list, loading state, and error
 * 
 * @example
 * const { data: asientos, isLoading, error } = useAsientosDisponibles('evento-123');
 */
export function useAsientosDisponibles(eventoId: string) {
  return useQuery<Asiento[], Error>({
    queryKey: ['asientos-disponibles', eventoId],
    queryFn: () => fetchAsientosDisponibles(eventoId),
    staleTime: 1 * 60 * 1000, // 1 minute (short stale time since availability changes frequently)
    retry: 3,
    enabled: !!eventoId, // Only run query if eventoId is provided
  });
}
