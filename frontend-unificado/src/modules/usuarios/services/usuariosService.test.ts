/**
 * Tests for usuariosService
 * Basic tests to verify service functions are correctly structured
 */

import { describe, it, expect, vi, beforeEach } from 'vitest';
import axiosClient from '@shared/api/axiosClient';
import * as usuariosService from './usuariosService';
import type { Usuario, CreateUsuarioDto, UpdateUsuarioDto } from '../types';

// Mock axiosClient
vi.mock('@shared/api/axiosClient');

describe('usuariosService', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('fetchUsuarios', () => {
    it('should fetch all usuarios', async () => {
      const mockUsuarios: Usuario[] = [
        {
          id: '1',
          username: 'admin1',
          nombre: 'Admin User',
          correo: 'admin@example.com',
          telefono: '+1234567890',
          rol: 'Admin',
          activo: true,
        },
        {
          id: '2',
          username: 'organizator1',
          nombre: 'Organizator User',
          correo: 'org@example.com',
          telefono: '+1234567891',
          rol: 'Organizator',
          activo: true,
        },
      ];

      vi.mocked(axiosClient.get).mockResolvedValue({
        data: { data: mockUsuarios },
      });

      const result = await usuariosService.fetchUsuarios();

      expect(axiosClient.get).toHaveBeenCalledWith('/api/usuarios');
      expect(result).toEqual(mockUsuarios);
    });
  });

  describe('fetchUsuario', () => {
    it('should fetch a single usuario by id', async () => {
      const mockUsuario: Usuario = {
        id: '1',
        username: 'admin1',
        nombre: 'Admin User',
        correo: 'admin@example.com',
        telefono: '+1234567890',
        rol: 'Admin',
        activo: true,
      };

      vi.mocked(axiosClient.get).mockResolvedValue({
        data: { data: mockUsuario },
      });

      const result = await usuariosService.fetchUsuario('1');

      expect(axiosClient.get).toHaveBeenCalledWith('/api/usuarios/1');
      expect(result).toEqual(mockUsuario);
    });
  });

  describe('createUsuario', () => {
    it('should create a new usuario', async () => {
      const createDto: CreateUsuarioDto = {
        username: 'newuser',
        nombre: 'New User',
        correo: 'newuser@example.com',
        telefono: '+1234567892',
        rol: 'Asistente',
        password: 'password123',
      };

      const mockCreatedUsuario: Usuario = {
        id: '3',
        username: createDto.username,
        nombre: createDto.nombre,
        correo: createDto.correo,
        telefono: createDto.telefono,
        rol: createDto.rol,
        activo: true,
      };

      vi.mocked(axiosClient.post).mockResolvedValue({
        data: { data: mockCreatedUsuario },
      });

      const result = await usuariosService.createUsuario(createDto);

      expect(axiosClient.post).toHaveBeenCalledWith('/api/usuarios', createDto);
      expect(result).toEqual(mockCreatedUsuario);
    });
  });

  describe('updateUsuario', () => {
    it('should update an existing usuario', async () => {
      const updateDto: UpdateUsuarioDto = {
        nombre: 'Updated Name',
        telefono: '+9999999999',
      };

      const mockUpdatedUsuario: Usuario = {
        id: '1',
        username: 'admin1',
        nombre: 'Updated Name',
        correo: 'admin@example.com',
        telefono: '+9999999999',
        rol: 'Admin',
        activo: true,
      };

      vi.mocked(axiosClient.put).mockResolvedValue({
        data: { data: mockUpdatedUsuario },
      });

      const result = await usuariosService.updateUsuario('1', updateDto);

      expect(axiosClient.put).toHaveBeenCalledWith('/api/usuarios/1', updateDto);
      expect(result).toEqual(mockUpdatedUsuario);
    });
  });

  describe('deactivateUsuario', () => {
    it('should deactivate a usuario', async () => {
      vi.mocked(axiosClient.delete).mockResolvedValue({});

      await usuariosService.deactivateUsuario('1');

      expect(axiosClient.delete).toHaveBeenCalledWith('/api/usuarios/1');
    });
  });
});
