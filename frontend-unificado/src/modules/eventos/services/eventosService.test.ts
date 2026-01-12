/**
 * Tests for eventosService
 * Basic tests to verify service functions are correctly structured
 */

import { describe, it, expect, vi, beforeEach } from 'vitest';
import axiosClient from '@shared/api/axiosClient';
import * as eventosService from './eventosService';
import type { Evento, CreateEventoDto, UpdateEventoDto } from '../types';

// Mock axiosClient
vi.mock('@shared/api/axiosClient');

describe('eventosService', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('fetchEventos', () => {
    it('should fetch all eventos', async () => {
      const mockEventos: Evento[] = [
        {
          id: '1',
          nombre: 'Evento Test 1',
          descripcion: 'Descripción test',
          fecha: '2024-12-31T20:00:00Z',
          ubicacion: 'Test Location',
          estado: 'Publicado',
          capacidadTotal: 100,
          asientosDisponibles: 50,
        },
      ];

      vi.mocked(axiosClient.get).mockResolvedValue({
        data: { data: mockEventos },
      });

      const result = await eventosService.fetchEventos();

      expect(axiosClient.get).toHaveBeenCalledWith('/api/eventos');
      expect(result).toEqual(mockEventos);
    });
  });

  describe('fetchEvento', () => {
    it('should fetch a single evento by id', async () => {
      const mockEvento: Evento = {
        id: '1',
        nombre: 'Evento Test',
        descripcion: 'Descripción test',
        fecha: '2024-12-31T20:00:00Z',
        ubicacion: 'Test Location',
        estado: 'Publicado',
        capacidadTotal: 100,
        asientosDisponibles: 50,
      };

      vi.mocked(axiosClient.get).mockResolvedValue({
        data: { data: mockEvento },
      });

      const result = await eventosService.fetchEvento('1');

      expect(axiosClient.get).toHaveBeenCalledWith('/api/eventos/1');
      expect(result).toEqual(mockEvento);
    });
  });

  describe('createEvento', () => {
    it('should create a new evento', async () => {
      const createDto: CreateEventoDto = {
        nombre: 'Nuevo Evento',
        descripcion: 'Descripción del nuevo evento',
        fecha: '2024-12-31T20:00:00Z',
        ubicacion: 'Nueva Ubicación',
      };

      const mockCreatedEvento: Evento = {
        id: '2',
        ...createDto,
        estado: 'Publicado',
        capacidadTotal: 0,
        asientosDisponibles: 0,
      };

      vi.mocked(axiosClient.post).mockResolvedValue({
        data: { data: mockCreatedEvento },
      });

      const result = await eventosService.createEvento(createDto);

      expect(axiosClient.post).toHaveBeenCalledWith('/api/eventos', createDto);
      expect(result).toEqual(mockCreatedEvento);
    });
  });

  describe('updateEvento', () => {
    it('should update an existing evento', async () => {
      const updateDto: UpdateEventoDto = {
        nombre: 'Evento Actualizado',
        descripcion: 'Descripción actualizada',
      };

      const mockUpdatedEvento: Evento = {
        id: '1',
        nombre: 'Evento Actualizado',
        descripcion: 'Descripción actualizada',
        fecha: '2024-12-31T20:00:00Z',
        ubicacion: 'Test Location',
        estado: 'Publicado',
        capacidadTotal: 100,
        asientosDisponibles: 50,
      };

      vi.mocked(axiosClient.put).mockResolvedValue({
        data: { data: mockUpdatedEvento },
      });

      const result = await eventosService.updateEvento('1', updateDto);

      expect(axiosClient.put).toHaveBeenCalledWith('/api/eventos/1', updateDto);
      expect(result).toEqual(mockUpdatedEvento);
    });
  });

  describe('cancelEvento', () => {
    it('should cancel an evento', async () => {
      vi.mocked(axiosClient.delete).mockResolvedValue({});

      await eventosService.cancelEvento('1');

      expect(axiosClient.delete).toHaveBeenCalledWith('/api/eventos/1/cancelar');
    });
  });
});

