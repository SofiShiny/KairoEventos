/**
 * Tipos para el módulo de Reportes
 * Basados en la API del microservicio de Reportes
 */

// Filtros para reportes
export interface ReporteFiltros {
  fechaInicio?: Date;
  fechaFin?: Date;
  eventoId?: string;
}

// Venta por evento (para resumen de ventas)
export interface VentaPorEvento {
  eventoId: string;
  tituloEvento: string;
  cantidadReservas: number;
  totalIngresos: number;
}

// Resumen de ventas (métricas agregadas)
export interface ResumenVentas {
  fechaInicio: string;
  fechaFin: string;
  totalVentas: number;
  cantidadReservas: number;
  promedioEvento: number;
  ventasPorEvento: VentaPorEvento[];
}

// Asistencia de un evento específico
export interface AsistenciaEvento {
  eventoId: string;
  tituloEvento: string;
  totalAsistentes: number;
  asientosReservados: number;
  asientosDisponibles: number;
  capacidadTotal: number;
  porcentajeOcupacion: number;
  ultimaActualizacion: string;
}

// Transacción individual para conciliación
export interface Transaccion {
  eventoId: string;
  tituloEvento: string;
  fecha: string;
  cantidadReservas: number;
  monto: number;
}

// Conciliación financiera
export interface ConciliacionFinanciera {
  fechaInicio: string;
  fechaFin: string;
  totalIngresos: number;
  cantidadTransacciones: number;
  desglosePorCategoria: Record<string, number>;
  transacciones: Transaccion[];
}

// Formato de exportación de reporte
export type FormatoExportacion = 'pdf' | 'excel' | 'csv';

// Parámetros para exportar reporte
export interface ExportarReporteParams {
  tipo: 'metricas' | 'asistencia' | 'conciliacion';
  formato: FormatoExportacion;
  filtros: ReporteFiltros;
}
