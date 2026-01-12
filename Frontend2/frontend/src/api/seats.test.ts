import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useCreateCategory, useEventCategories, useCreateSeat } from './seats';
import { seatsApi } from './clients';
import * as fc from 'fast-check';
import type { CategoryCreateDto, Categoria, SeatCreateDto, Asiento } from '../types/api';
import React from 'react';

vi.mock('./clients', () => ({
  seatsApi: {
    post: vi.fn(),
    get: vi.fn(),
  },
}));

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  });
  return ({ children }: { children: React.ReactNode }) =>
    React.createElement(QueryClientProvider, { client: queryClient }, children);
};

describe('Category API Hooks - Property-Based Tests', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  /**
   * **Feature: seat-categories-management, Property 2: Category API invocation correctness**
   * **Validates: Requirements 1.3**
   * 
   * For any valid category data, submitting the form should invoke POST /api/asientos/categorias
   * with exactly the data entered by the user
   */
  it('Property 2: Category API invocation correctness', async () => {
    await fc.assert(
      fc.asyncProperty(
        fc.record({
          nombre: fc.string({ minLength: 2, maxLength: 50 }),
          descripcion: fc.option(fc.string({ maxLength: 200 }), { nil: undefined }),
          precio: fc.float({ min: 0.01, max: 10000, noNaN: true, noDefaultInfinity: true }),
          eventoId: fc.uuid(),
        }),
        async (categoryData: CategoryCreateDto) => {
          // Mock the API response
          const mockResponse: Categoria = {
            id: fc.sample(fc.uuid(), 1)[0],
            ...categoryData,
            descripcion: categoryData.descripcion ?? undefined,
          };

          vi.mocked(seatsApi.post).mockResolvedValueOnce({ data: mockResponse });

          const { result } = renderHook(() => useCreateCategory(), {
            wrapper: createWrapper(),
          });

          // Execute the mutation
          result.current.mutate(categoryData);

          // Wait for the mutation to complete
          await waitFor(() => expect(result.current.isSuccess).toBe(true));

          // Verify the API was called with the exact input data
          expect(seatsApi.post).toHaveBeenCalledWith(
            '/api/asientos/categorias',
            categoryData
          );
          expect(seatsApi.post).toHaveBeenCalledTimes(1);
        }
      ),
      { numRuns: 100 }
    );
  });

  /**
   * **Feature: seat-categories-management, Property 4: Seat API invocation correctness**
   * **Validates: Requirements 2.3**
   * 
   * For any valid seat data with a selected category, submitting the form should invoke
   * POST /api/asientos with the seat data including the categoriaId
   */
  it('Property 4: Seat API invocation correctness', async () => {
    await fc.assert(
      fc.asyncProperty(
        fc.record({
          fila: fc.oneof(
            fc.constantFrom('A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J'),
            fc.integer({ min: 1, max: 99 }).map(String)
          ),
          numero: fc.integer({ min: 1, max: 999 }),
          categoriaId: fc.uuid(),
          mapaId: fc.uuid(),
        }),
        async (seatData: SeatCreateDto) => {
          // Mock the API response
          const mockResponse: Asiento = {
            id: fc.sample(fc.uuid(), 1)[0],
            ...seatData,
            estado: 'Disponible',
          };

          vi.mocked(seatsApi.post).mockResolvedValueOnce({ data: mockResponse });

          const { result } = renderHook(() => useCreateSeat(), {
            wrapper: createWrapper(),
          });

          // Execute the mutation
          result.current.mutate(seatData);

          // Wait for the mutation to complete
          await waitFor(() => expect(result.current.isSuccess).toBe(true));

          // Verify the API was called with the seat data including categoriaId
          expect(seatsApi.post).toHaveBeenCalledWith(
            '/api/asientos',
            expect.objectContaining({
              fila: seatData.fila,
              numero: seatData.numero,
              categoriaId: seatData.categoriaId,
              mapaId: seatData.mapaId,
              estado: 'Disponible', // Should default to Disponible
            })
          );
          expect(seatsApi.post).toHaveBeenCalledTimes(1);
        }
      ),
      { numRuns: 100 }
    );
  });

  /**
   * **Feature: seat-categories-management, Property 5: Default seat state assignment**
   * **Validates: Requirements 2.5**
   * 
   * For any newly created seat, the estado field should automatically be set to "Disponible"
   * 
   * Note: This test validates the behavior when estado is not explicitly provided in the request.
   * The actual default assignment happens either in the API or in the hook implementation.
   */
  it('Property 5: Default seat state assignment', async () => {
    await fc.assert(
      fc.asyncProperty(
        fc.record({
          fila: fc.oneof(
            fc.constantFrom('A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J'),
            fc.integer({ min: 1, max: 99 }).map(String)
          ),
          numero: fc.integer({ min: 1, max: 999 }),
          categoriaId: fc.uuid(),
          mapaId: fc.uuid(),
        }),
        async (seatDataWithoutEstado: Omit<SeatCreateDto, 'estado'>) => {
          // Create seat data without estado field
          const seatData: SeatCreateDto = {
            ...seatDataWithoutEstado,
            // Explicitly omit estado to test default behavior
          };

          // Mock the API response with estado set to "Disponible"
          const mockResponse: Asiento = {
            id: fc.sample(fc.uuid(), 1)[0],
            ...seatDataWithoutEstado,
            estado: 'Disponible', // API should return this default
          };

          vi.mocked(seatsApi.post).mockResolvedValueOnce({ data: mockResponse });

          const { result } = renderHook(() => useCreateSeat(), {
            wrapper: createWrapper(),
          });

          // Execute the mutation
          result.current.mutate(seatData);

          // Wait for the mutation to complete
          await waitFor(() => expect(result.current.isSuccess).toBe(true));

          // Verify that when estado is not in the input, it defaults to "Disponible"
          expect(seatsApi.post).toHaveBeenCalledWith(
            '/api/asientos',
            expect.objectContaining({
              estado: 'Disponible',
            })
          );
          
          // Verify the returned seat has estado = "Disponible"
          expect(result.current.data?.estado).toBe('Disponible');
        }
      ),
      { numRuns: 100 }
    );
  });
});

describe('API Hooks - Unit Tests', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  /**
   * Test useCreateCategory calls correct endpoint
   * Requirements: 1.3
   */
  it('useCreateCategory calls POST /api/asientos/categorias', async () => {
    const categoryData: CategoryCreateDto = {
      nombre: 'VIP',
      descripcion: 'VIP seats',
      precio: 100.50,
      eventoId: '123e4567-e89b-12d3-a456-426614174000',
    };

    const mockResponse: Categoria = {
      id: '123e4567-e89b-12d3-a456-426614174001',
      ...categoryData,
    };

    vi.mocked(seatsApi.post).mockResolvedValueOnce({ data: mockResponse });

    const { result } = renderHook(() => useCreateCategory(), {
      wrapper: createWrapper(),
    });

    result.current.mutate(categoryData);

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(seatsApi.post).toHaveBeenCalledWith('/api/asientos/categorias', categoryData);
    expect(seatsApi.post).toHaveBeenCalledTimes(1);
  });

  /**
   * Test useEventCategories fetches with correct eventoId
   * Requirements: 3.1
   */
  it('useEventCategories fetches with correct eventoId', async () => {
    const eventoId = '123e4567-e89b-12d3-a456-426614174000';
    const mockCategories: Categoria[] = [
      {
        id: '1',
        nombre: 'VIP',
        precio: 100,
        eventoId,
      },
      {
        id: '2',
        nombre: 'General',
        precio: 50,
        eventoId,
      },
    ];

    vi.mocked(seatsApi.get).mockResolvedValueOnce({ data: mockCategories });

    const { result } = renderHook(() => useEventCategories(eventoId), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(seatsApi.get).toHaveBeenCalledWith(`/api/asientos/categorias/evento/${eventoId}`);
    expect(result.current.data).toEqual(mockCategories);
  });

  /**
   * Test useCreateSeat includes all required fields
   * Requirements: 2.3
   */
  it('useCreateSeat includes all required fields including categoriaId', async () => {
    const seatData: SeatCreateDto = {
      fila: 'A',
      numero: 10,
      categoriaId: '123e4567-e89b-12d3-a456-426614174000',
      mapaId: '123e4567-e89b-12d3-a456-426614174001',
    };

    const mockResponse: Asiento = {
      id: '123e4567-e89b-12d3-a456-426614174002',
      ...seatData,
      estado: 'Disponible',
    };

    vi.mocked(seatsApi.post).mockResolvedValueOnce({ data: mockResponse });

    const { result } = renderHook(() => useCreateSeat(), {
      wrapper: createWrapper(),
    });

    result.current.mutate(seatData);

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(seatsApi.post).toHaveBeenCalledWith(
      '/api/asientos',
      expect.objectContaining({
        fila: 'A',
        numero: 10,
        categoriaId: '123e4567-e89b-12d3-a456-426614174000',
        mapaId: '123e4567-e89b-12d3-a456-426614174001',
        estado: 'Disponible',
      })
    );
  });
});
