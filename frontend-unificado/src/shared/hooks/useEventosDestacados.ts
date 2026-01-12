/**
 * useEventosDestacados Hook
 * Custom hook for fetching featured events using React Query
 */

import { useQuery } from '@tanstack/react-query';
import { fetchEventosDestacados } from '../services/dashboardService';
import type { EventoDestacado } from '../types/dashboard';

export function useEventosDestacados() {
  return useQuery<EventoDestacado[], Error>({
    queryKey: ['dashboard', 'eventos-destacados'],
    queryFn: fetchEventosDestacados,
    staleTime: 5 * 60 * 1000, // 5 minutes
    retry: 3,
  });
}
