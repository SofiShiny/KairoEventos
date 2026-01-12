import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import SeatManagementTab from './SeatManagementTab';
import { useEventSeats, useEventCategories, useCreateCategory, useCreateSeat } from '../api/seats';
import * as fc from 'fast-check';
import React from 'react';

vi.mock('../api/seats', () => ({
  useEventSeats: vi.fn(),
  useEventCategories: vi.fn(),
  useCreateCategory: vi.fn(),
  useCreateSeat: vi.fn(),
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

describe('SeatManagementTab - Property-Based Tests', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    
    // Default mock implementations
    vi.mocked(useEventSeats).mockReturnValue({
      data: null,
      isLoading: false,
    } as any);
    
    vi.mocked(useEventCategories).mockReturnValue({
      data: [],
      isLoading: false,
    } as any);
    
    vi.mocked(useCreateCategory).mockReturnValue({
      mutate: vi.fn(),
      isPending: false,
    } as any);
    
    vi.mocked(useCreateSeat).mockReturnValue({
      mutate: vi.fn(),
      isPending: false,
    } as any);
  });

  /**
   * **Feature: seat-categories-management, Property 7: Event context preservation**
   * **Validates: Requirements 4.2, 4.3**
   * 
   * For any navigation between tabs or sections within the edit interface,
   * the eventoId should remain consistent and be passed to all child components
   */
  it('Property 7: Event context preservation', async () => {
    await fc.assert(
      fc.asyncProperty(
        fc.record({
          eventoId: fc.uuid(),
          mapaId: fc.uuid(),
        }),
        async ({ eventoId, mapaId }) => {
          // Track which eventoId is passed to each hook
          const eventSeatsCallArgs: string[] = [];
          const eventCategoriesCallArgs: string[] = [];

          vi.mocked(useEventSeats).mockImplementation((id: string) => {
            eventSeatsCallArgs.push(id);
            return {
              data: null,
              isLoading: false,
            } as any;
          });

          vi.mocked(useEventCategories).mockImplementation((id: string) => {
            eventCategoriesCallArgs.push(id);
            return {
              data: [],
              isLoading: false,
            } as any;
          });

          // Render the component
          render(
            <SeatManagementTab eventoId={eventoId} mapaId={mapaId} />,
            { wrapper: createWrapper() }
          );

          // Verify that eventoId is passed consistently to all child components
          // CategoryManagementSection should receive eventoId
          expect(screen.getByText(/gestión de categorías/i)).toBeInTheDocument();
          
          // SeatAssignmentSection should receive eventoId
          expect(screen.getByText(/asignación de asientos/i)).toBeInTheDocument();
          
          // SeatGrid should be rendered
          expect(screen.getByText(/mapa de asientos/i)).toBeInTheDocument();

          // Verify that useEventSeats was called with the correct eventoId
          expect(eventSeatsCallArgs).toContain(eventoId);
          
          // Verify that useEventCategories was called with the correct eventoId
          // (called by both CategoryManagementSection and SeatAssignmentSection)
          expect(eventCategoriesCallArgs.filter(id => id === eventoId).length).toBeGreaterThanOrEqual(2);

          // Verify all calls use the same eventoId (no inconsistency)
          eventSeatsCallArgs.forEach(id => {
            expect(id).toBe(eventoId);
          });
          
          eventCategoriesCallArgs.forEach(id => {
            expect(id).toBe(eventoId);
          });
        }
      ),
      { numRuns: 100 }
    );
  });
});

describe('SeatManagementTab - Integration Tests', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  /**
   * Test complete category creation flow
   * Requirements: 4.1, 4.2, 4.3, 4.4
   */
  it('completes category creation flow end-to-end', async () => {
    const user = await import('@testing-library/user-event').then(m => m.default.setup());
    const eventoId = '123e4567-e89b-12d3-a456-426614174000';
    const mapaId = '987e6543-e21b-12d3-a456-426614174000';

    const mockCreateCategory = vi.fn((data, callbacks) => {
      callbacks?.onSuccess?.();
    });

    const mockCategories: any[] = [];

    vi.mocked(useEventSeats).mockReturnValue({
      data: [],
      isLoading: false,
    } as any);

    vi.mocked(useEventCategories).mockReturnValue({
      data: mockCategories,
      isLoading: false,
    } as any);

    vi.mocked(useCreateCategory).mockReturnValue({
      mutate: mockCreateCategory,
      isPending: false,
    } as any);

    vi.mocked(useCreateSeat).mockReturnValue({
      mutate: vi.fn(),
      isPending: false,
    } as any);

    render(
      <SeatManagementTab eventoId={eventoId} mapaId={mapaId} />,
      { wrapper: createWrapper() }
    );

    // Verify all sections are rendered
    expect(screen.getByText(/gestión de categorías/i)).toBeInTheDocument();
    expect(screen.getByText(/asignación de asientos/i)).toBeInTheDocument();
    expect(screen.getByText(/mapa de asientos/i)).toBeInTheDocument();

    // Fill category form
    const nombreInput = screen.getByLabelText(/nombre/i);
    const precioInput = screen.getByLabelText(/precio/i);
    const submitButton = screen.getByRole('button', { name: /crear categoría/i });

    await user.type(nombreInput, 'VIP');
    await user.type(precioInput, '150.00');
    await user.click(submitButton);

    // Verify mutation was called with correct data
    await waitFor(() => {
      expect(mockCreateCategory).toHaveBeenCalledWith(
        expect.objectContaining({
          nombre: 'VIP',
          precio: 150.00,
          eventoId,
        }),
        expect.any(Object)
      );
    });

    // Verify success message appears
    await waitFor(() => {
      expect(screen.getByText(/creada exitosamente/i)).toBeInTheDocument();
    });
  });

  /**
   * Test complete seat creation flow
   * Requirements: 4.1, 4.2, 4.3, 4.4
   */
  it('completes seat creation flow end-to-end', async () => {
    const user = await import('@testing-library/user-event').then(m => m.default.setup());
    const eventoId = '123e4567-e89b-12d3-a456-426614174000';
    const mapaId = '987e6543-e21b-12d3-a456-426614174000';
    const categoryId = 'cat-123';

    const mockCreateSeat = vi.fn((data, callbacks) => {
      callbacks?.onSuccess?.();
    });

    const mockCategories = [
      {
        id: categoryId,
        nombre: 'VIP',
        precio: 150.00,
        eventoId,
      },
    ];

    vi.mocked(useEventSeats).mockReturnValue({
      data: [],
      isLoading: false,
    } as any);

    vi.mocked(useEventCategories).mockReturnValue({
      data: mockCategories,
      isLoading: false,
    } as any);

    vi.mocked(useCreateCategory).mockReturnValue({
      mutate: vi.fn(),
      isPending: false,
    } as any);

    vi.mocked(useCreateSeat).mockReturnValue({
      mutate: mockCreateSeat,
      isPending: false,
    } as any);

    render(
      <SeatManagementTab eventoId={eventoId} mapaId={mapaId} />,
      { wrapper: createWrapper() }
    );

    // Verify seat assignment section is rendered
    expect(screen.getByText(/asignación de asientos/i)).toBeInTheDocument();

    // Fill seat form
    const filaInput = screen.getByLabelText(/fila/i);
    const numeroInput = screen.getByLabelText(/número/i);
    const categorySelect = screen.getByLabelText(/categoría/i);
    const submitButton = screen.getByRole('button', { name: /crear asiento/i });

    await user.type(filaInput, 'A');
    await user.type(numeroInput, '1');
    await user.click(categorySelect);
    
    // Select the category from dropdown
    const categoryOption = await screen.findByText(/VIP - \$150\.00/i);
    await user.click(categoryOption);
    
    await user.click(submitButton);

    // Verify mutation was called with correct data
    await waitFor(() => {
      expect(mockCreateSeat).toHaveBeenCalledWith(
        expect.objectContaining({
          fila: 'A',
          numero: 1,
          categoriaId: categoryId,
          mapaId,
          estado: 'Disponible',
        }),
        expect.any(Object)
      );
    });

    // Verify success message appears
    await waitFor(() => {
      expect(screen.getByText(/asiento creado exitosamente/i)).toBeInTheDocument();
    });
  });

  /**
   * Test error handling flow
   * Requirements: 4.1, 4.2, 4.3, 4.4
   */
  it('handles errors appropriately', async () => {
    const user = await import('@testing-library/user-event').then(m => m.default.setup());
    const eventoId = '123e4567-e89b-12d3-a456-426614174000';
    const mapaId = '987e6543-e21b-12d3-a456-426614174000';

    const mockCreateCategory = vi.fn((data, callbacks) => {
      callbacks?.onError?.({
        response: {
          data: {
            message: 'Ya existe una categoría con este nombre',
          },
        },
      });
    });

    vi.mocked(useEventSeats).mockReturnValue({
      data: [],
      isLoading: false,
    } as any);

    vi.mocked(useEventCategories).mockReturnValue({
      data: [],
      isLoading: false,
    } as any);

    vi.mocked(useCreateCategory).mockReturnValue({
      mutate: mockCreateCategory,
      isPending: false,
    } as any);

    vi.mocked(useCreateSeat).mockReturnValue({
      mutate: vi.fn(),
      isPending: false,
    } as any);

    render(
      <SeatManagementTab eventoId={eventoId} mapaId={mapaId} />,
      { wrapper: createWrapper() }
    );

    // Fill category form with duplicate name
    const nombreInput = screen.getByLabelText(/nombre/i);
    const precioInput = screen.getByLabelText(/precio/i);
    const submitButton = screen.getByRole('button', { name: /crear categoría/i });

    await user.type(nombreInput, 'VIP');
    await user.type(precioInput, '150.00');
    await user.click(submitButton);

    // Verify error message appears
    await waitFor(() => {
      expect(screen.getByText(/ya existe una categoría con este nombre/i)).toBeInTheDocument();
    });

    // Verify form is still editable (not cleared)
    const nombreInputAfterError = screen.getByLabelText(/nombre/i) as HTMLInputElement;
    expect(nombreInputAfterError.value).toBe('VIP');
  });

  /**
   * Test that seat assignment shows message when no categories exist
   * Requirements: 3.3
   */
  it('shows message when no categories exist for seat assignment', () => {
    const eventoId = '123e4567-e89b-12d3-a456-426614174000';
    const mapaId = '987e6543-e21b-12d3-a456-426614174000';

    vi.mocked(useEventSeats).mockReturnValue({
      data: [],
      isLoading: false,
    } as any);

    vi.mocked(useEventCategories).mockReturnValue({
      data: [],
      isLoading: false,
    } as any);

    vi.mocked(useCreateCategory).mockReturnValue({
      mutate: vi.fn(),
      isPending: false,
    } as any);

    vi.mocked(useCreateSeat).mockReturnValue({
      mutate: vi.fn(),
      isPending: false,
    } as any);

    render(
      <SeatManagementTab eventoId={eventoId} mapaId={mapaId} />,
      { wrapper: createWrapper() }
    );

    // Verify message is shown
    expect(screen.getByText(/primero debe crear categorías/i)).toBeInTheDocument();
  });
});
