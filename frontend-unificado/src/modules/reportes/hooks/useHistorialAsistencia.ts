/**
 * useHistorialAsistencia Hook
 * Custom hook for fetching asistencia de un evento using React Query
 */

import { useQuery } from '@tanstack/react-query';
import { fetchHistorialAsistencia } from '../services/reportesService';
import type { AsistenciaEvento, ReporteFiltros } from '../types';

/**
 * Hook to fetch asistencia de un evento
 * 
 * @param filtros - Filtros para la asistencia (debe incluir eventoId)
 * @returns Query result with asistencia data, loading state, and error
 * 
 * @example
 * const { data: asistencia, isLoading, error } = useHistorialAsistencia({
 *   eventoId: '123e4567-e89b-12d3-a456-426614174000',
 * });
 */
export function useHistorialAsistencia(filtros: ReporteFiltros) {
  return useQuery<AsistenciaEvento | null, Error>({
    queryKey: ['reportes', 'asistencia', filtros],
    queryFn: () => fetchHistorialAsistencia(filtros),
    staleTime: 2 * 60 * 1000, // 2 minutes
    retry: 2,
    enabled: !!filtros.eventoId, // Only fetch if eventoId is provided
  });
}
