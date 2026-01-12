/**
 * useMetricasEventos Hook
 * Custom hook for fetching métricas de eventos using React Query
 */

import { useQuery } from '@tanstack/react-query';
import { fetchMetricasEventos } from '../services/reportesService';
import type { VentaPorEvento, ReporteFiltros } from '../types';

/**
 * Hook to fetch métricas de eventos con filtros
 * 
 * @param filtros - Filtros para las métricas (fechas, eventoId)
 * @returns Query result with métricas list, loading state, and error
 * 
 * @example
 * const { data: metricas, isLoading, error } = useMetricasEventos({
 *   fechaInicio: new Date('2024-01-01'),
 *   fechaFin: new Date('2024-01-31'),
 * });
 */
export function useMetricasEventos(filtros: ReporteFiltros) {
  return useQuery<VentaPorEvento[], Error>({
    queryKey: ['reportes', 'metricas', filtros],
    queryFn: () => fetchMetricasEventos(filtros),
    staleTime: 2 * 60 * 1000, // 2 minutes - reportes data changes less frequently
    retry: 2,
    enabled: true, // Always enabled, filtros can be empty
  });
}
