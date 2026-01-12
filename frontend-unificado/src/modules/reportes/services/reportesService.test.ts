/**
 * Tests for reportesService
 */

import { describe, it, expect, vi, beforeEach } from 'vitest';
import axiosClient from '@shared/api/axiosClient';
import {
  fetchMetricasEventos,
  fetchHistorialAsistencia,
  fetchConciliacionFinanciera,
  exportarReporte,
} from './reportesService';

// Mock axiosClient
vi.mock('@shared/api/axiosClient');

describe('reportesService', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('fetchMetricasEventos', () => {
    it('should fetch métricas for a specific evento when eventoId is provided', async () => {
      const mockMetricas = {
        eventoId: '123',
        tituloEvento: 'Test Evento',
        totalAsistentes: 100,
        estado: 'Publicado',
        ultimaActualizacion: '2024-01-01T00:00:00Z',
      };

      vi.mocked(axiosClient.get).mockResolvedValue({ data: mockMetricas });

      const result = await fetchMetricasEventos({ eventoId: '123' });

      expect(axiosClient.get).toHaveBeenCalledWith('/reportes/metricas-evento/123');
      expect(result).toEqual([mockMetricas]);
    });

    it('should fetch resumen de ventas when no eventoId is provided', async () => {
      const mockResumen = {
        ventasPorEvento: [
          {
            eventoId: '123',
            tituloEvento: 'Test Evento',
            cantidadReservas: 50,
            totalIngresos: 5000,
          },
        ],
      };

      vi.mocked(axiosClient.get).mockResolvedValue({ data: mockResumen });

      const result = await fetchMetricasEventos({
        fechaInicio: new Date('2024-01-01'),
        fechaFin: new Date('2024-01-31'),
      });

      expect(axiosClient.get).toHaveBeenCalled();
      expect(result).toHaveLength(1);
      expect(result[0].eventoId).toBe('123');
      expect(result[0].totalAsistentes).toBe(50);
    });
  });

  describe('fetchHistorialAsistencia', () => {
    it('should fetch historial de asistencia for an evento', async () => {
      const mockHistorial = {
        eventoId: '123',
        tituloEvento: 'Test Evento',
        totalAsistentes: 100,
        asientosReservados: 80,
        asientosDisponibles: 20,
        capacidadTotal: 100,
        porcentajeOcupacion: 80,
        ultimaActualizacion: '2024-01-01T00:00:00Z',
      };

      vi.mocked(axiosClient.get).mockResolvedValue({ data: mockHistorial });

      const result = await fetchHistorialAsistencia({ eventoId: '123' });

      expect(axiosClient.get).toHaveBeenCalledWith('/reportes/asistencia/123');
      expect(result).toEqual([mockHistorial]);
    });

    it('should throw error when eventoId is not provided', async () => {
      await expect(fetchHistorialAsistencia({})).rejects.toThrow(
        'eventoId es requerido para obtener historial de asistencia'
      );
    });
  });

  describe('fetchConciliacionFinanciera', () => {
    it('should fetch conciliación financiera with date filters', async () => {
      const mockConciliacion = {
        fechaInicio: '2024-01-01T00:00:00Z',
        fechaFin: '2024-01-31T00:00:00Z',
        totalIngresos: 10000,
        cantidadTransacciones: 100,
        desglosePorCategoria: { VIP: 5000, General: 5000 },
        transacciones: [],
      };

      vi.mocked(axiosClient.get).mockResolvedValue({ data: mockConciliacion });

      const result = await fetchConciliacionFinanciera({
        fechaInicio: new Date('2024-01-01'),
        fechaFin: new Date('2024-01-31'),
      });

      expect(axiosClient.get).toHaveBeenCalled();
      expect(result.totalIngresos).toBe(10000);
      expect(result.cantidadTransacciones).toBe(100);
    });

    it('should fetch conciliación without date filters', async () => {
      const mockConciliacion = {
        fechaInicio: '2024-01-01T00:00:00Z',
        fechaFin: '2024-01-31T00:00:00Z',
        totalIngresos: 10000,
        cantidadTransacciones: 100,
        desglosePorCategoria: {},
        transacciones: [],
      };

      vi.mocked(axiosClient.get).mockResolvedValue({ data: mockConciliacion });

      const result = await fetchConciliacionFinanciera({});

      expect(axiosClient.get).toHaveBeenCalledWith(
        '/reportes/conciliacion-financiera?'
      );
      expect(result).toEqual(mockConciliacion);
    });
  });

  describe('exportarReporte', () => {
    it('should export reporte as CSV', async () => {
      const mockData = [
        { eventoId: '123', tituloEvento: 'Test', totalAsistentes: 100 },
      ];

      vi.mocked(axiosClient.get).mockResolvedValue({ data: mockData });

      const result = await exportarReporte({
        tipo: 'metricas',
        formato: 'csv',
        filtros: { eventoId: '123' },
      });

      expect(result).toBeInstanceOf(Blob);
      expect(result.type).toBe('text/csv;charset=utf-8;');
    });

    it('should throw error for unsupported formats', async () => {
      await expect(
        exportarReporte({
          tipo: 'metricas',
          formato: 'pdf',
          filtros: {},
        })
      ).rejects.toThrow('Formato pdf no implementado aún');
    });

    it('should throw error for unsupported report types', async () => {
      await expect(
        exportarReporte({
          tipo: 'invalid' as any,
          formato: 'csv',
          filtros: {},
        })
      ).rejects.toThrow('Tipo de reporte no soportado');
    });
  });
});
