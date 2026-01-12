import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import SeatAssignmentSection from './SeatAssignmentSection';
import { useCreateSeat, useEventCategories } from '../api/seats';
import * as fc from 'fast-check';
import React from 'react';

vi.mock('../api/seats', () => ({
  useCreateSeat: vi.fn(),
  useEventCategories: vi.fn(),
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

describe('SeatAssignmentSection - Property-Based Tests', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    
    // Default mock implementations
    vi.mocked(useCreateSeat).mockReturnValue({
      mutate: vi.fn(),
      isPending: false,
    } as any);
    
    vi.mocked(useEventCategories).mockReturnValue({
      data: [
        { id: 'cat-1', nombre: 'VIP', precio: 100, eventoId: 'event-1' },
        { id: 'cat-2', nombre: 'General', precio: 50, eventoId: 'event-1' },
      ],
      isLoading: false,
    } as any);
  });

  /**
   * **Feature: seat-categories-management, Property 6: Category display completeness**
   * **Validates: Requirements 3.2**
   * 
   * For any category displayed in the selection list, both the nombre and precio fields
   * should be visible to the user
   */
  it('Property 6: Category display completeness', async () => {
    await fc.assert(
      fc.asyncProperty(
        fc.array(
          fc.record({
            id: fc.uuid(),
            nombre: fc.string({ minLength: 2, maxLength: 50 }),
            descripcion: fc.option(fc.string({ maxLength: 200 })),
            precio: fc.float({ min: 0.01, max: 10000, noNaN: true }),
            eventoId: fc.uuid(),
          }),
          { minLength: 1, maxLength: 10 }
        ),
        fc.uuid(),
        fc.uuid(),
        async (categories, eventoId, mapaId) => {
          // Mock the categories hook to return our generated categories
          vi.mocked(useEventCategories).mockReturnValue({
            data: categories,
            isLoading: false,
          } as any);

          render(
            <SeatAssignmentSection eventoId={eventoId} mapaId={mapaId} />,
            { wrapper: createWrapper() }
          );

          // Open the category dropdown
          const categorySelect = screen.getByLabelText(/categoría/i);
          await userEvent.setup().click(categorySelect);

          // Wait for options to appear
          await waitFor(() => {
            const options = screen.getAllByRole('option');
            expect(options.length).toBeGreaterThan(0);
          });

          // Verify each category displays both nombre and precio
          for (const category of categories) {
            const optionText = new RegExp(`${category.nombre}.*\\$${category.precio.toFixed(2)}`, 'i');
            const option = screen.getByRole('option', { name: optionText });
            expect(option).toBeInTheDocument();
            
            // Verify both nombre and precio are in the text content
            expect(option.textContent).toContain(category.nombre);
            expect(option.textContent).toContain(category.precio.toFixed(2));
          }
        }
      ),
      { numRuns: 100 }
    );
  });

  /**
   * **Feature: seat-categories-management, Property 10: Validation error highlighting**
   * **Validates: Requirements 5.4**
   * 
   * For any form field that fails validation, the field should be visually highlighted
   * with an error state
   */
  it('Property 10: Validation error highlighting', async () => {
    await fc.assert(
      fc.asyncProperty(
        fc.record({
          fila: fc.oneof(
            fc.constant(''), // empty fila
            fc.string().filter(s => !/^[A-Za-z0-9]+$/.test(s) && s.length > 0) // invalid characters
          ),
          numero: fc.oneof(
            fc.constant(''), // empty numero
            fc.constant('abc'), // non-numeric
            fc.integer({ min: -100, max: 0 }).map(String), // negative or zero
            fc.integer({ min: 1000, max: 9999 }).map(String) // too large
          ),
          eventoId: fc.uuid(),
          mapaId: fc.uuid(),
        }),
        async ({ fila, numero, eventoId, mapaId }) => {
          const mockMutate = vi.fn();
          vi.mocked(useCreateSeat).mockReturnValue({
            mutate: mockMutate,
            isPending: false,
          } as any);

          const { container } = render(
            <SeatAssignmentSection eventoId={eventoId} mapaId={mapaId} />,
            { wrapper: createWrapper() }
          );

          const user = userEvent.setup();
          const filaInput = screen.getByLabelText(/fila/i);
          const numeroInput = screen.getByLabelText(/número/i);
          const categorySelect = screen.getByLabelText(/categoría/i);
          const submitButton = screen.getByRole('button', { name: /crear asiento/i });

          // Fill form with invalid data
          if (fila) {
            await user.clear(filaInput);
            await user.type(filaInput, fila);
          }

          if (numero) {
            await user.clear(numeroInput);
            await user.type(numeroInput, numero);
          }

          // Select a category
          await user.click(categorySelect);
          const firstOption = screen.getAllByRole('option')[0];
          await user.click(firstOption);

          // Submit form to trigger validation
          await user.click(submitButton);

          // Wait for validation errors to appear
          await waitFor(() => {
            // Check that fields with errors have the error attribute
            const filaInvalid = !fila || fila.trim().length === 0 || !/^[A-Za-z0-9]+$/.test(fila.trim());
            const numeroInt = parseInt(numero, 10);
            const numeroInvalid = !numero || numero.trim().length === 0 || isNaN(numeroInt) || 
                                 numeroInt < 1 || numeroInt > 999 || !Number.isInteger(numeroInt);

            if (filaInvalid) {
              // Check that fila input has error styling
              const filaInputElement = filaInput.closest('.MuiFormControl-root');
              expect(filaInputElement?.classList.contains('Mui-error') || 
                     filaInput.getAttribute('aria-invalid') === 'true').toBe(true);
            }

            if (numeroInvalid) {
              // Check that numero input has error styling
              const numeroInputElement = numeroInput.closest('.MuiFormControl-root');
              expect(numeroInputElement?.classList.contains('Mui-error') || 
                     numeroInput.getAttribute('aria-invalid') === 'true').toBe(true);
            }
          });
        }
      ),
      { numRuns: 100 }
    );
  });

  /**
   * **Feature: seat-categories-management, Property 3: Seat validation correctness**
   * **Validates: Requirements 2.2**
   * 
   * For any seat form submission, if fila is empty or numero is not a positive integer,
   * the validation should reject the submission
   */
  it('Property 3: Seat validation correctness', async () => {
    const user = userEvent.setup();

    await fc.assert(
      fc.asyncProperty(
        fc.record({
          fila: fc.oneof(
            fc.constant(''), // empty fila
            fc.string().filter(s => !/^[A-Za-z0-9]+$/.test(s) && s.length > 0), // invalid characters
            fc.string({ minLength: 1, maxLength: 5 }).filter(s => /^[A-Za-z0-9]+$/.test(s)) // valid fila
          ),
          numero: fc.oneof(
            fc.constant(''), // empty numero
            fc.constant('abc'), // non-numeric
            fc.float().map(String), // decimal number
            fc.integer({ min: -100, max: 0 }).map(String), // negative or zero
            fc.integer({ min: 1000, max: 9999 }).map(String), // too large
            fc.integer({ min: 1, max: 999 }).map(String) // valid numero
          ),
          eventoId: fc.uuid(),
          mapaId: fc.uuid(),
        }),
        async ({ fila, numero, eventoId, mapaId }) => {
          // Determine if inputs are invalid
          const filaInvalid = !fila || fila.trim().length === 0 || !/^[A-Za-z0-9]+$/.test(fila.trim());
          const numeroInt = parseInt(numero, 10);
          const numeroInvalid = !numero || numero.trim().length === 0 || isNaN(numeroInt) || 
                               numeroInt < 1 || numeroInt > 999 || !Number.isInteger(numeroInt);

          // Only test invalid cases
          if (!filaInvalid && !numeroInvalid) {
            return;
          }

          const mockMutate = vi.fn();
          vi.mocked(useCreateSeat).mockReturnValue({
            mutate: mockMutate,
            isPending: false,
          } as any);

          const { container } = render(
            <SeatAssignmentSection eventoId={eventoId} mapaId={mapaId} />,
            { wrapper: createWrapper() }
          );

          const filaInput = screen.getByLabelText(/fila/i);
          const numeroInput = screen.getByLabelText(/número/i);
          const categorySelect = screen.getByLabelText(/categoría/i);
          const submitButton = screen.getByRole('button', { name: /crear asiento/i });

          // Fill form
          if (fila) {
            await user.clear(filaInput);
            await user.type(filaInput, fila);
          }

          if (numero) {
            await user.clear(numeroInput);
            await user.type(numeroInput, numero);
          }

          // Select a category
          await user.click(categorySelect);
          const firstOption = screen.getAllByRole('option')[0];
          await user.click(firstOption);

          // Submit form
          await user.click(submitButton);

          // Wait for validation
          await waitFor(() => {
            // Mutation should NOT be called when validation fails
            expect(mockMutate).not.toHaveBeenCalled();
          });

          // Check that error messages are displayed
          const errorText = container.textContent || '';
          if (filaInvalid) {
            expect(errorText).toMatch(/fila.*(requerida|válido)/i);
          }

          if (numeroInvalid) {
            expect(errorText).toMatch(/número.*(requerido|numérico|entre|entero)/i);
          }
        }
      ),
      { numRuns: 100 }
    );
  });
});

describe('SeatAssignmentSection - Unit Tests', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    
    vi.mocked(useCreateSeat).mockReturnValue({
      mutate: vi.fn(),
      isPending: false,
    } as any);
    
    vi.mocked(useEventCategories).mockReturnValue({
      data: [
        { id: 'cat-1', nombre: 'VIP', precio: 100, eventoId: 'event-1' },
        { id: 'cat-2', nombre: 'General', precio: 50, eventoId: 'event-1' },
      ],
      isLoading: false,
    } as any);
  });

  /**
   * Test component renders form with category dropdown
   * Requirements: 2.1
   */
  it('renders form with category dropdown', () => {
    const eventoId = '123e4567-e89b-12d3-a456-426614174000';
    const mapaId = '123e4567-e89b-12d3-a456-426614174001';
    
    render(
      <SeatAssignmentSection eventoId={eventoId} mapaId={mapaId} />,
      { wrapper: createWrapper() }
    );

    // Check form elements are present
    expect(screen.getByLabelText(/fila/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/número/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/categoría/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /crear asiento/i })).toBeInTheDocument();
  });

  /**
   * Test form validation rejects invalid fila
   * Requirements: 2.2
   */
  it('rejects invalid fila', async () => {
    const user = userEvent.setup();
    const eventoId = '123e4567-e89b-12d3-a456-426614174000';
    const mapaId = '123e4567-e89b-12d3-a456-426614174001';
    const mockMutate = vi.fn();
    
    vi.mocked(useCreateSeat).mockReturnValue({
      mutate: mockMutate,
      isPending: false,
    } as any);

    render(
      <SeatAssignmentSection eventoId={eventoId} mapaId={mapaId} />,
      { wrapper: createWrapper() }
    );

    const filaInput = screen.getByLabelText(/fila/i);
    const numeroInput = screen.getByLabelText(/número/i);
    const submitButton = screen.getByRole('button', { name: /crear asiento/i });

    // Try with invalid fila (special characters)
    await user.type(filaInput, '@#$');
    await user.type(numeroInput, '10');
    await user.click(submitButton);

    await waitFor(() => {
      expect(mockMutate).not.toHaveBeenCalled();
      expect(screen.getByText(/fila.*válido/i)).toBeInTheDocument();
    });
  });

  /**
   * Test form validation rejects non-numeric numero
   * Requirements: 2.2
   */
  it('rejects non-numeric numero', async () => {
    const user = userEvent.setup();
    const eventoId = '123e4567-e89b-12d3-a456-426614174000';
    const mapaId = '123e4567-e89b-12d3-a456-426614174001';
    const mockMutate = vi.fn();
    
    vi.mocked(useCreateSeat).mockReturnValue({
      mutate: mockMutate,
      isPending: false,
    } as any);

    render(
      <SeatAssignmentSection eventoId={eventoId} mapaId={mapaId} />,
      { wrapper: createWrapper() }
    );

    const filaInput = screen.getByLabelText(/fila/i);
    const numeroInput = screen.getByLabelText(/número/i);
    const submitButton = screen.getByRole('button', { name: /crear asiento/i });

    await user.type(filaInput, 'A');
    await user.type(numeroInput, 'abc');
    await user.click(submitButton);

    await waitFor(() => {
      expect(mockMutate).not.toHaveBeenCalled();
      expect(screen.getByText(/número.*numérico/i)).toBeInTheDocument();
    });
  });

  /**
   * Test successful submission calls mutation with categoriaId
   * Requirements: 2.3
   */
  it('calls mutation with categoriaId on successful submission', async () => {
    const user = userEvent.setup();
    const eventoId = '123e4567-e89b-12d3-a456-426614174000';
    const mapaId = '123e4567-e89b-12d3-a456-426614174001';
    const mockMutate = vi.fn((data, callbacks) => {
      callbacks?.onSuccess?.();
    });
    
    vi.mocked(useCreateSeat).mockReturnValue({
      mutate: mockMutate,
      isPending: false,
    } as any);

    render(
      <SeatAssignmentSection eventoId={eventoId} mapaId={mapaId} />,
      { wrapper: createWrapper() }
    );

    const filaInput = screen.getByLabelText(/fila/i);
    const numeroInput = screen.getByLabelText(/número/i);
    const categorySelect = screen.getByLabelText(/categoría/i);
    const submitButton = screen.getByRole('button', { name: /crear asiento/i });

    await user.type(filaInput, 'A');
    await user.type(numeroInput, '10');
    
    // Select category
    await user.click(categorySelect);
    const vipOption = screen.getByRole('option', { name: /VIP.*100/i });
    await user.click(vipOption);

    await user.click(submitButton);

    await waitFor(() => {
      expect(mockMutate).toHaveBeenCalledWith(
        expect.objectContaining({
          fila: 'A',
          numero: 10,
          categoriaId: 'cat-1',
          mapaId,
          estado: 'Disponible',
        }),
        expect.any(Object)
      );
    });
  });

  /**
   * Test message displays when no categories exist
   * Requirements: 3.3
   */
  it('displays message when no categories exist', () => {
    const eventoId = '123e4567-e89b-12d3-a456-426614174000';
    const mapaId = '123e4567-e89b-12d3-a456-426614174001';
    
    vi.mocked(useEventCategories).mockReturnValue({
      data: [],
      isLoading: false,
    } as any);

    render(
      <SeatAssignmentSection eventoId={eventoId} mapaId={mapaId} />,
      { wrapper: createWrapper() }
    );

    expect(screen.getByText(/primero debe crear categorías/i)).toBeInTheDocument();
    expect(screen.queryByLabelText(/fila/i)).not.toBeInTheDocument();
  });
});
