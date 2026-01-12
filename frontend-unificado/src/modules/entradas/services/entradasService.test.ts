/**
 * Tests for entradasService
 * Basic tests to verify service functions are correctly structured
 */

import { describe, it, expect, vi, beforeEach } from 'vitest';
import axiosClient from '@shared/api/axiosClient';
import * as entradasService from './entradasService';
import type { Entrada, Asiento, CreateEntradaDto } from '../types';

// Mock axiosClient
vi.mock('@shared/api/axiosClient');

describe('entradasService', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('fetchMisEntradas', () => {
    it('should fetch user entradas', async () => {
      const mockEntradas: Entrada[] = [
        {
          id: '1',
          eventoId: 'evento-1',
          eventoNombre: 'Concierto Rock',
          asientoId: 'asiento-1',
          asientoInfo: 'Fila A - Asiento 12',
          estado: 'Reservada',
          precio: 50.0,
          fechaCompra: '2024-01-15T10:00:00Z',
          tiempoRestante: 14,
        },
        {
          id: '2',
          eventoId: 'evento-2',
          eventoNombre: 'Teatro Musical',
          asientoId: 'asiento-2',
          asientoInfo: 'Fila B - Asiento 5',
          estado: 'Pagada',
          precio: 75.0,
          fechaCompra: '2024-01-10T15:30:00Z',
        },
      ];

      vi.mocked(axiosClient.get).mockResolvedValue({
        data: { data: mockEntradas },
      });

      const result = await entradasService.fetchMisEntradas();

      expect(axiosClient.get).toHaveBeenCalledWith('/api/entradas/mis-entradas');
      expect(result).toEqual(mockEntradas);
    });
  });

  describe('fetchAsientosDisponibles', () => {
    it('should fetch available asientos for an evento', async () => {
      const mockAsientos: Asiento[] = [
        {
          id: 'asiento-1',
          fila: 'A',
          numero: 1,
          estado: 'Disponible',
          precio: 50.0,
        },
        {
          id: 'asiento-2',
          fila: 'A',
          numero: 2,
          estado: 'Reservado',
          precio: 50.0,
        },
        {
          id: 'asiento-3',
          fila: 'A',
          numero: 3,
          estado: 'Ocupado',
          precio: 50.0,
        },
      ];

      vi.mocked(axiosClient.get).mockResolvedValue({
        data: { data: mockAsientos },
      });

      const result = await entradasService.fetchAsientosDisponibles('evento-1');

      expect(axiosClient.get).toHaveBeenCalledWith('/api/entradas/asientos-disponibles/evento-1');
      expect(result).toEqual(mockAsientos);
    });
  });

  describe('createEntrada', () => {
    it('should create a new entrada', async () => {
      const createDto: CreateEntradaDto = {
        eventoId: 'evento-1',
        asientoId: 'asiento-1',
        usuarioId: 'user-1',
      };

      const mockCreatedEntrada: Entrada = {
        id: '3',
        eventoId: createDto.eventoId,
        eventoNombre: 'Nuevo Evento',
        asientoId: createDto.asientoId,
        asientoInfo: 'Fila C - Asiento 10',
        estado: 'Reservada',
        precio: 60.0,
        fechaCompra: '2024-01-20T12:00:00Z',
        tiempoRestante: 15,
      };

      vi.mocked(axiosClient.post).mockResolvedValue({
        data: { data: mockCreatedEntrada },
      });

      const result = await entradasService.createEntrada(createDto);

      expect(axiosClient.post).toHaveBeenCalledWith('/api/entradas', createDto);
      expect(result).toEqual(mockCreatedEntrada);
    });
  });

  describe('cancelarEntrada', () => {
    it('should cancel an entrada', async () => {
      vi.mocked(axiosClient.delete).mockResolvedValue({});

      await entradasService.cancelarEntrada('1');

      expect(axiosClient.delete).toHaveBeenCalledWith('/api/entradas/1');
    });
  });
});
