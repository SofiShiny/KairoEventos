/**
 * Reportes Service
 * API calls for reportes management
 */

import axiosClient from '@shared/api/axiosClient';
import type {
  ResumenVentas,
  VentaPorEvento,
  AsistenciaEvento,
  ConciliacionFinanciera,
  ReporteFiltros,
  ExportarReporteParams,
} from '../types';

/**
 * Fetch resumen de ventas (métricas agregadas por evento)
 * GET /api/reportes/resumen-ventas
 */
export async function fetchResumenVentas(
  filtros: ReporteFiltros
): Promise<ResumenVentas> {
  const params = new URLSearchParams();
  if (filtros.fechaInicio) {
    params.append('fechaInicio', filtros.fechaInicio.toISOString());
  }
  if (filtros.fechaFin) {
    params.append('fechaFin', filtros.fechaFin.toISOString());
  }

  const response = await axiosClient.get<ResumenVentas>(
    `/api/reportes/resumen-ventas?${params.toString()}`
  );

  return response.data;
}

/**
 * Fetch métricas de eventos (alias para resumen de ventas)
 * Retorna solo las ventas por evento
 */
export async function fetchMetricasEventos(
  filtros: ReporteFiltros
): Promise<VentaPorEvento[]> {
  const resumen = await fetchResumenVentas(filtros);
  return resumen.ventasPorEvento;
}

/**
 * Fetch historial de asistencia de un evento
 * GET /api/reportes/asistencia/:eventoId
 */
export async function fetchHistorialAsistencia(
  filtros: ReporteFiltros
): Promise<AsistenciaEvento | null> {
  if (!filtros.eventoId) {
    return null;
  }

  const response = await axiosClient.get<AsistenciaEvento>(
    `/api/reportes/asistencia/${filtros.eventoId}`
  );

  return response.data;
}

/**
 * Fetch conciliación financiera
 * GET /api/reportes/conciliacion-financiera
 */
export async function fetchConciliacionFinanciera(
  filtros: ReporteFiltros
): Promise<ConciliacionFinanciera> {
  const params = new URLSearchParams();
  
  if (filtros.fechaInicio) {
    params.append('fechaInicio', filtros.fechaInicio.toISOString());
  }
  if (filtros.fechaFin) {
    params.append('fechaFin', filtros.fechaFin.toISOString());
  }

  const response = await axiosClient.get<ConciliacionFinanciera>(
    `/api/reportes/conciliacion-financiera?${params.toString()}`
  );

  return response.data;
}

/**
 * Exportar reporte en formato específico
 * Esta función prepara los datos para exportación
 * La exportación real se maneja en el cliente (navegador)
 */
export async function exportarReporte(
  params: ExportarReporteParams
): Promise<Blob> {
  // Validar formato antes de obtener datos
  if (params.formato === 'pdf' || params.formato === 'excel') {
    throw new Error(
      `Formato ${params.formato} no implementado aún. Use 'csv' por ahora.`
    );
  }

  // Obtener los datos según el tipo de reporte
  let data: unknown;
  
  switch (params.tipo) {
    case 'metricas':
      data = await fetchMetricasEventos(params.filtros);
      break;
    case 'asistencia':
      data = await fetchHistorialAsistencia(params.filtros);
      break;
    case 'conciliacion':
      data = await fetchConciliacionFinanciera(params.filtros);
      break;
    default:
      throw new Error(`Tipo de reporte no soportado: ${params.tipo}`);
  }

  // Convertir datos a formato de exportación
  const jsonData = JSON.stringify(data, null, 2);

  // Convertir a CSV si se solicita
  if (params.formato === 'csv') {
    const csv = convertToCSV(data);
    return new Blob([csv], { type: 'text/csv;charset=utf-8;' });
  }

  // Por defecto, retornar como JSON
  return new Blob([jsonData], { type: 'application/json' });
}

/**
 * Helper function to convert data to CSV format
 */
function convertToCSV(data: unknown): string {
  if (!data || (Array.isArray(data) && data.length === 0)) {
    return '';
  }

  const items = Array.isArray(data) ? data : [data];
  const headers = Object.keys(items[0]);
  
  const csvRows = [
    headers.join(','), // Header row
    ...items.map((item) =>
      headers
        .map((header) => {
          const value = (item as Record<string, unknown>)[header];
          // Escape commas and quotes in values
          if (typeof value === 'string' && (value.includes(',') || value.includes('"'))) {
            return `"${value.replace(/"/g, '""')}"`;
          }
          return value;
        })
        .join(',')
    ),
  ];

  return csvRows.join('\n');
}
