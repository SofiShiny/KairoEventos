/**
 * useConciliacionFinanciera Hook
 * Custom hook for fetching conciliación financiera using React Query
 */

import { useQuery } from '@tanstack/react-query';
import { fetchConciliacionFinanciera } from '../services/reportesService';
import type { ConciliacionFinanciera, ReporteFiltros } from '../types';

/**
 * Hook to fetch conciliación financiera con filtros de fecha
 * 
 * @param filtros - Filtros para la conciliación (fechas)
 * @returns Query result with conciliación data, loading state, and error
 * 
 * @example
 * const { data: conciliacion, isLoading, error } = useConciliacionFinanciera({
 *   fechaInicio: new Date('2024-01-01'),
 *   fechaFin: new Date('2024-01-31'),
 * });
 */
export function useConciliacionFinanciera(filtros: ReporteFiltros) {
  return useQuery<ConciliacionFinanciera, Error>({
    queryKey: ['reportes', 'conciliacion', filtros],
    queryFn: () => fetchConciliacionFinanciera(filtros),
    staleTime: 2 * 60 * 1000, // 2 minutes
    retry: 2,
    enabled: true, // Always enabled, uses default dates if not provided
  });
}
