/**
 * useEventos Hook
 * Custom hook for fetching list of eventos using React Query
 * 
 * Performance optimizations:
 * - Prefetching support for better UX
 * - Optimized cache configuration
 */

import { useQuery, useQueryClient } from '@tanstack/react-query';
import { fetchEventos, fetchEvento } from '../services';
import type { Evento } from '../types';

/**
 * Hook to fetch all eventos
 * 
 * @returns Query result with eventos list, loading state, and error
 * 
 * @example
 * const { data: eventos, isLoading, error, refetch } = useEventos();
 */
export function useEventos() {
  return useQuery<Evento[], Error>({
    queryKey: ['eventos'],
    queryFn: fetchEventos,
    staleTime: 5 * 60 * 1000, // 5 minutes
    retry: 3,
  });
}

/**
 * Hook to prefetch evento details
 * Useful for prefetching data when hovering over evento cards
 * 
 * @example
 * const prefetchEvento = usePrefetchEvento();
 * <EventoCard onMouseEnter={() => prefetchEvento(evento.id)} />
 */
export function usePrefetchEvento() {
  const queryClient = useQueryClient();

  return (eventoId: string) => {
    queryClient.prefetchQuery({
      queryKey: ['evento', eventoId],
      queryFn: () => fetchEvento(eventoId),
      staleTime: 5 * 60 * 1000, // 5 minutes
    });
  };
}
