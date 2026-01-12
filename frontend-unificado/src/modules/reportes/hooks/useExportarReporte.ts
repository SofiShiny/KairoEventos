/**
 * useExportarReporte Hook
 * Custom hook for exporting reportes using React Query mutation
 */

import { useMutation } from '@tanstack/react-query';
import { exportarReporte } from '../services/reportesService';
import type { ExportarReporteParams } from '../types';

/**
 * Hook to export reportes in different formats
 * 
 * @returns Mutation result with export function, loading state, and error
 * 
 * @example
 * const { mutate: exportar, isPending } = useExportarReporte();
 * 
 * const handleExport = () => {
 *   exportar({
 *     tipo: 'metricas',
 *     formato: 'csv',
 *     filtros: { fechaInicio: new Date(), fechaFin: new Date() }
 *   }, {
 *     onSuccess: (blob) => {
 *       // Download the file
 *       const url = window.URL.createObjectURL(blob);
 *       const a = document.createElement('a');
 *       a.href = url;
 *       a.download = `reporte-${Date.now()}.csv`;
 *       a.click();
 *     }
 *   });
 * };
 */
export function useExportarReporte() {
  return useMutation<Blob, Error, ExportarReporteParams>({
    mutationFn: exportarReporte,
    onSuccess: (blob, variables) => {
      // Automatically trigger download
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      
      // Generate filename based on report type and format
      const timestamp = new Date().toISOString().split('T')[0];
      const filename = `reporte-${variables.tipo}-${timestamp}.${variables.formato}`;
      a.download = filename;
      
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      
      // Clean up the URL object
      window.URL.revokeObjectURL(url);
    },
  });
}
