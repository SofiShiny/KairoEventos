/**
 * Validation Schemas Tests
 * Unit tests for Zod validation schemas
 */

import { describe, it, expect } from 'vitest';
import {
  eventoSchema,
  usuarioSchema,
  usuarioEditSchema,
  entradaSchema,
  reporteFiltrosSchema,
  searchSchema,
  getZodErrorMessage,
  hasFieldError,
  getFieldErrorMessage,
} from './schemas';
import { z } from 'zod';

describe('eventoSchema', () => {
  it('should validate valid evento data', () => {
    const validData = {
      nombre: 'Test Event',
      descripcion: 'This is a test event description with enough characters',
      fecha: '2025-12-31',
      ubicacion: 'Test Location',
      imagenUrl: 'https://example.com/image.jpg',
    };

    const result = eventoSchema.safeParse(validData);
    expect(result.success).toBe(true);
  });

  it('should reject nombre too short', () => {
    const invalidData = {
      nombre: 'AB',
      descripcion: 'This is a test event description',
      fecha: '2025-12-31',
      ubicacion: 'Test Location',
    };

    const result = eventoSchema.safeParse(invalidData);
    expect(result.success).toBe(false);
    if (!result.success) {
      expect(result.error.issues[0].message).toContain('al menos 3 caracteres');
    }
  });

  it('should reject descripcion too short', () => {
    const invalidData = {
      nombre: 'Test Event',
      descripcion: 'Short',
      fecha: '2025-12-31',
      ubicacion: 'Test Location',
    };

    const result = eventoSchema.safeParse(invalidData);
    expect(result.success).toBe(false);
    if (!result.success) {
      expect(result.error.issues[0].message).toContain('al menos 10 caracteres');
    }
  });

  it('should reject past date', () => {
    const invalidData = {
      nombre: 'Test Event',
      descripcion: 'This is a test event description',
      fecha: '2020-01-01',
      ubicacion: 'Test Location',
    };

    const result = eventoSchema.safeParse(invalidData);
    expect(result.success).toBe(false);
    if (!result.success) {
      expect(result.error.issues[0].message).toContain('hoy o en el futuro');
    }
  });

  it('should accept empty imagenUrl', () => {
    const validData = {
      nombre: 'Test Event',
      descripcion: 'This is a test event description',
      fecha: '2025-12-31',
      ubicacion: 'Test Location',
      imagenUrl: '',
    };

    const result = eventoSchema.safeParse(validData);
    expect(result.success).toBe(true);
  });

  it('should reject invalid URL', () => {
    const invalidData = {
      nombre: 'Test Event',
      descripcion: 'This is a test event description',
      fecha: '2025-12-31',
      ubicacion: 'Test Location',
      imagenUrl: 'not-a-url',
    };

    const result = eventoSchema.safeParse(invalidData);
    expect(result.success).toBe(false);
  });
});

describe('usuarioSchema', () => {
  it('should validate valid usuario data', () => {
    const validData = {
      username: 'testuser',
      nombre: 'Test User',
      correo: 'test@example.com',
      telefono: '+1234567890',
      rol: 'Asistente' as const,
      password: 'Password123',
    };

    const result = usuarioSchema.safeParse(validData);
    expect(result.success).toBe(true);
  });

  it('should reject invalid username characters', () => {
    const invalidData = {
      username: 'test user!',
      nombre: 'Test User',
      correo: 'test@example.com',
      telefono: '+1234567890',
      rol: 'Asistente' as const,
      password: 'Password123',
    };

    const result = usuarioSchema.safeParse(invalidData);
    expect(result.success).toBe(false);
    if (!result.success) {
      expect(result.error.issues[0].message).toContain('letras, números');
    }
  });

  it('should reject invalid email format', () => {
    const invalidData = {
      username: 'testuser',
      nombre: 'Test User',
      correo: 'invalid-email',
      telefono: '+1234567890',
      rol: 'Asistente' as const,
      password: 'Password123',
    };

    const result = usuarioSchema.safeParse(invalidData);
    expect(result.success).toBe(false);
    if (!result.success) {
      expect(result.error.issues[0].message).toContain('correo');
    }
  });

  it('should reject invalid phone format', () => {
    const invalidData = {
      username: 'testuser',
      nombre: 'Test User',
      correo: 'test@example.com',
      telefono: '123', // Too short
      rol: 'Asistente' as const,
      password: 'Password123',
    };

    const result = usuarioSchema.safeParse(invalidData);
    expect(result.success).toBe(false);
    if (!result.success) {
      expect(result.error.issues[0].message).toContain('teléfono');
    }
  });

  it('should reject weak password', () => {
    const invalidData = {
      username: 'testuser',
      nombre: 'Test User',
      correo: 'test@example.com',
      telefono: '+1234567890',
      rol: 'Asistente' as const,
      password: 'password', // No uppercase or number
    };

    const result = usuarioSchema.safeParse(invalidData);
    expect(result.success).toBe(false);
    if (!result.success) {
      expect(result.error.issues[0].message).toContain('mayúscula');
    }
  });

  it('should accept valid phone with +', () => {
    const validData = {
      username: 'testuser',
      nombre: 'Test User',
      correo: 'test@example.com',
      telefono: '+1234567890',
      rol: 'Asistente' as const,
      password: 'Password123',
    };

    const result = usuarioSchema.safeParse(validData);
    expect(result.success).toBe(true);
  });

  it('should accept valid phone without +', () => {
    const validData = {
      username: 'testuser',
      nombre: 'Test User',
      correo: 'test@example.com',
      telefono: '1234567890',
      rol: 'Asistente' as const,
      password: 'Password123',
    };

    const result = usuarioSchema.safeParse(validData);
    expect(result.success).toBe(true);
  });
});

describe('usuarioEditSchema', () => {
  it('should allow empty password', () => {
    const validData = {
      username: 'testuser',
      nombre: 'Test User',
      correo: 'test@example.com',
      telefono: '+1234567890',
      rol: 'Asistente' as const,
      password: '',
    };

    const result = usuarioEditSchema.safeParse(validData);
    expect(result.success).toBe(true);
  });

  it('should validate password if provided', () => {
    const invalidData = {
      username: 'testuser',
      nombre: 'Test User',
      correo: 'test@example.com',
      telefono: '+1234567890',
      rol: 'Asistente' as const,
      password: 'weak',
    };

    const result = usuarioEditSchema.safeParse(invalidData);
    expect(result.success).toBe(false);
  });
});

describe('entradaSchema', () => {
  it('should validate valid entrada data', () => {
    const validData = {
      eventoId: '123e4567-e89b-12d3-a456-426614174000',
      asientoId: '123e4567-e89b-12d3-a456-426614174001',
      usuarioId: '123e4567-e89b-12d3-a456-426614174002',
    };

    const result = entradaSchema.safeParse(validData);
    expect(result.success).toBe(true);
  });

  it('should reject invalid UUID', () => {
    const invalidData = {
      eventoId: 'not-a-uuid',
      asientoId: '123e4567-e89b-12d3-a456-426614174001',
      usuarioId: '123e4567-e89b-12d3-a456-426614174002',
    };

    const result = entradaSchema.safeParse(invalidData);
    expect(result.success).toBe(false);
  });

  it('should reject empty fields', () => {
    const invalidData = {
      eventoId: '',
      asientoId: '123e4567-e89b-12d3-a456-426614174001',
      usuarioId: '123e4567-e89b-12d3-a456-426614174002',
    };

    const result = entradaSchema.safeParse(invalidData);
    expect(result.success).toBe(false);
  });
});

describe('reporteFiltrosSchema', () => {
  it('should validate valid filtros data', () => {
    const validData = {
      fechaInicio: '2025-01-01',
      fechaFin: '2025-12-31',
      eventoId: '123e4567-e89b-12d3-a456-426614174000',
    };

    const result = reporteFiltrosSchema.safeParse(validData);
    expect(result.success).toBe(true);
  });

  it('should accept empty eventoId', () => {
    const validData = {
      fechaInicio: '2025-01-01',
      fechaFin: '2025-12-31',
      eventoId: '',
    };

    const result = reporteFiltrosSchema.safeParse(validData);
    expect(result.success).toBe(true);
  });

  it('should reject fechaFin before fechaInicio', () => {
    const invalidData = {
      fechaInicio: '2025-12-31',
      fechaFin: '2025-01-01',
      eventoId: '',
    };

    const result = reporteFiltrosSchema.safeParse(invalidData);
    expect(result.success).toBe(false);
    if (!result.success) {
      expect(result.error.issues[0].message).toContain('posterior');
    }
  });
});

describe('searchSchema', () => {
  it('should validate valid search data', () => {
    const validData = {
      query: 'test search',
      fecha: '2025-12-31',
      ubicacion: 'Test Location',
    };

    const result = searchSchema.safeParse(validData);
    expect(result.success).toBe(true);
  });

  it('should accept empty fields', () => {
    const validData = {
      query: '',
      fecha: '',
      ubicacion: '',
    };

    const result = searchSchema.safeParse(validData);
    expect(result.success).toBe(true);
  });

  it('should reject query too long', () => {
    const invalidData = {
      query: 'a'.repeat(201),
      fecha: '',
      ubicacion: '',
    };

    const result = searchSchema.safeParse(invalidData);
    expect(result.success).toBe(false);
  });
});

describe('Helper Functions', () => {
  describe('getZodErrorMessage', () => {
    it('should extract error message from ZodError', () => {
      try {
        eventoSchema.parse({ nombre: 'AB' });
      } catch (error) {
        const message = getZodErrorMessage(error);
        expect(message).toContain('caracteres');
      }
    });

    it('should return default message for non-ZodError', () => {
      const message = getZodErrorMessage(new Error('Regular error'));
      expect(message).toBe('Error de validación');
    });
  });

  describe('hasFieldError', () => {
    it('should return true if field has error', () => {
      const errors = { nombre: { message: 'Error' } };
      expect(hasFieldError(errors, 'nombre')).toBe(true);
    });

    it('should return false if field has no error', () => {
      const errors = { nombre: { message: 'Error' } };
      expect(hasFieldError(errors, 'descripcion')).toBe(false);
    });
  });

  describe('getFieldErrorMessage', () => {
    it('should return error message if exists', () => {
      const errors = { nombre: { message: 'Error message' } };
      expect(getFieldErrorMessage(errors, 'nombre')).toBe('Error message');
    });

    it('should return undefined if no error', () => {
      const errors = { nombre: { message: 'Error message' } };
      expect(getFieldErrorMessage(errors, 'descripcion')).toBeUndefined();
    });
  });
});
