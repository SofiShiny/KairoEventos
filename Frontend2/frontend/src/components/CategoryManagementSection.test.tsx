import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import CategoryManagementSection from './CategoryManagementSection';
import { useCreateCategory, useEventCategories } from '../api/seats';
import * as fc from 'fast-check';
import React from 'react';

vi.mock('../api/seats', () => ({
  useCreateCategory: vi.fn(),
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

describe('CategoryManagementSection - Property-Based Tests', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    
    // Default mock implementations
    vi.mocked(useCreateCategory).mockReturnValue({
      mutate: vi.fn(),
      isPending: false,
    } as any);
    
    vi.mocked(useEventCategories).mockReturnValue({
      data: [],
      isLoading: false,
    } as any);
  });

  /**
   * **Feature: seat-categories-management, Property 9: Operation feedback consistency**
   * **Validates: Requirements 5.1, 5.2, 5.3**
   * 
   * For any async operation (create category), the UI should display a loading indicator
   * during execution, a success message on completion, or an error message on failure
   */
  it('Property 9: Operation feedback consistency', async () => {
    const user = userEvent.setup();

    await fc.assert(
      fc.asyncProperty(
        fc.record({
          nombre: fc.string({ minLength: 2, maxLength: 50 }),
          precio: fc.float({ min: 0.01, max: 10000, noNaN: true, noDefaultInfinity: true }),
          eventoId: fc.uuid(),
          shouldSucceed: fc.boolean(),
        }),
        async ({ nombre, precio, eventoId, shouldSucceed }) => {
          const mockMutate = vi.fn((data, callbacks) => {
            if (shouldSucceed) {
              callbacks?.onSuccess?.();
            } else {
              callbacks?.onError?.({ response: { data: { message: 'Test error' } } });
            }
          });

          vi.mocked(useCreateCategory).mockReturnValue({
            mutate: mockMutate,
            isPending: false,
          } as any);

          const { rerender } = render(
            <CategoryManagementSection eventoId={eventoId} />,
            { wrapper: createWrapper() }
          );

          const nombreInput = screen.getByLabelText(/nombre/i);
          const precioInput = screen.getByLabelText(/precio/i);
          const submitButton = screen.getByRole('button', { name: /crear categoría/i });

          // Fill form with valid data
          await user.type(nombreInput, nombre);
          await user.type(precioInput, precio.toString());

          // Simulate loading state
          vi.mocked(useCreateCategory).mockReturnValue({
            mutate: mockMutate,
            isPending: true,
          } as any);

          rerender(<CategoryManagementSection eventoId={eventoId} />);

          // Check loading indicator is shown
          await waitFor(() => {
            const button = screen.getByRole('button', { name: /crear categoría/i });
            expect(button).toBeDisabled();
          });

          // Reset to not pending
          vi.mocked(useCreateCategory).mockReturnValue({
            mutate: mockMutate,
            isPending: false,
          } as any);

          rerender(<CategoryManagementSection eventoId={eventoId} />);

          // Submit form
          await user.click(submitButton);

          // Wait for mutation to be called
          await waitFor(() => {
            expect(mockMutate).toHaveBeenCalled();
          });

          // Check appropriate feedback is shown
          if (shouldSucceed) {
            await waitFor(() => {
              expect(screen.getByText(/creada exitosamente/i)).toBeInTheDocument();
            });
          } else {
            await waitFor(() => {
              expect(screen.getByText(/error/i)).toBeInTheDocument();
            });
          }
        }
      ),
      { numRuns: 100 }
    );
  });

  /**
   * **Feature: seat-categories-management, Property 11: Form reset after success**
   * **Validates: Requirements 5.5**
   * 
   * For any successful form submission, all form fields should be cleared to their initial empty state
   */
  it('Property 11: Form reset after success', async () => {
    const user = userEvent.setup();

    await fc.assert(
      fc.asyncProperty(
        fc.record({
          nombre: fc.string({ minLength: 2, maxLength: 50 }),
          descripcion: fc.option(fc.string({ maxLength: 200 }), { nil: undefined }),
          precio: fc.float({ min: 0.01, max: 10000, noNaN: true, noDefaultInfinity: true }),
          eventoId: fc.uuid(),
        }),
        async ({ nombre, descripcion, precio, eventoId }) => {
          const mockMutate = vi.fn((data, callbacks) => {
            // Simulate successful mutation
            callbacks?.onSuccess?.();
          });

          vi.mocked(useCreateCategory).mockReturnValue({
            mutate: mockMutate,
            isPending: false,
          } as any);

          render(
            <CategoryManagementSection eventoId={eventoId} />,
            { wrapper: createWrapper() }
          );

          const nombreInput = screen.getByLabelText(/nombre/i) as HTMLInputElement;
          const descripcionInput = screen.getByLabelText(/descripción/i) as HTMLInputElement;
          const precioInput = screen.getByLabelText(/precio/i) as HTMLInputElement;
          const submitButton = screen.getByRole('button', { name: /crear categoría/i });

          // Fill form
          await user.type(nombreInput, nombre);
          if (descripcion) {
            await user.type(descripcionInput, descripcion);
          }
          await user.type(precioInput, precio.toString());

          // Verify fields are filled
          expect(nombreInput.value).toBe(nombre);
          if (descripcion) {
            expect(descripcionInput.value).toBe(descripcion);
          }
          expect(precioInput.value).toBe(precio.toString());

          // Submit form
          await user.click(submitButton);

          // Wait for mutation to complete
          await waitFor(() => {
            expect(mockMutate).toHaveBeenCalled();
          });

          // Verify all fields are cleared
          await waitFor(() => {
            expect(nombreInput.value).toBe('');
            expect(descripcionInput.value).toBe('');
            expect(precioInput.value).toBe('');
          });
        }
      ),
      { numRuns: 100 }
    );
  });

  /**
   * **Feature: seat-categories-management, Property 1: Form validation completeness**
   * **Validates: Requirements 1.2**
   * 
   * For any category form submission with missing required fields (nombre or precio),
   * the validation should reject the submission and display appropriate error messages
   */
  it('Property 1: Form validation completeness', async () => {
    const user = userEvent.setup();

    await fc.assert(
      fc.asyncProperty(
        fc.record({
          nombre: fc.option(fc.string(), { nil: '' }),
          precio: fc.option(fc.string(), { nil: '' }),
          eventoId: fc.uuid(),
        }),
        async ({ nombre, precio, eventoId }) => {
          // Only test cases where at least one required field is missing or invalid
          const nombreInvalid = !nombre || nombre.trim().length === 0;
          const precioInvalid = !precio || precio.trim().length === 0 || isNaN(parseFloat(precio)) || parseFloat(precio) <= 0;
          
          if (!nombreInvalid && !precioInvalid) {
            // Skip valid cases - we're testing validation rejection
            return;
          }

          const mockMutate = vi.fn();
          vi.mocked(useCreateCategory).mockReturnValue({
            mutate: mockMutate,
            isPending: false,
          } as any);

          const { container } = render(
            <CategoryManagementSection eventoId={eventoId} />,
            { wrapper: createWrapper() }
          );

          // Fill in the form
          const nombreInput = screen.getByLabelText(/nombre/i);
          const precioInput = screen.getByLabelText(/precio/i);
          const submitButton = screen.getByRole('button', { name: /crear categoría/i });

          if (nombre) {
            await user.clear(nombreInput);
            await user.type(nombreInput, nombre);
          }

          if (precio) {
            await user.clear(precioInput);
            await user.type(precioInput, precio);
          }

          // Submit the form
          await user.click(submitButton);

          // Wait for validation to complete
          await waitFor(() => {
            // The mutation should NOT be called when validation fails
            expect(mockMutate).not.toHaveBeenCalled();
          });

          // Check that error messages are displayed
          if (nombreInvalid) {
            const errorText = container.textContent || '';
            expect(errorText).toMatch(/nombre.*requerido/i);
          }

          if (precioInvalid) {
            const errorText = container.textContent || '';
            expect(errorText).toMatch(/precio.*(requerido|válido|mayor)/i);
          }
        }
      ),
      { numRuns: 100 }
    );
  });
});

describe('CategoryManagementSection - Unit Tests', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    
    vi.mocked(useCreateCategory).mockReturnValue({
      mutate: vi.fn(),
      isPending: false,
    } as any);
    
    vi.mocked(useEventCategories).mockReturnValue({
      data: [],
      isLoading: false,
    } as any);
  });

  /**
   * Test component renders form and list
   * Requirements: 1.1
   */
  it('renders form and category list', () => {
    const eventoId = '123e4567-e89b-12d3-a456-426614174000';
    
    render(
      <CategoryManagementSection eventoId={eventoId} />,
      { wrapper: createWrapper() }
    );

    // Check form elements are present
    expect(screen.getByLabelText(/nombre/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/descripción/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/precio/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /crear categoría/i })).toBeInTheDocument();

    // Check category list section
    expect(screen.getByText(/categorías existentes/i)).toBeInTheDocument();
  });

  /**
   * Test form validation rejects empty nombre
   * Requirements: 1.2
   */
  it('rejects empty nombre', async () => {
    const user = userEvent.setup();
    const eventoId = '123e4567-e89b-12d3-a456-426614174000';
    const mockMutate = vi.fn();
    
    vi.mocked(useCreateCategory).mockReturnValue({
      mutate: mockMutate,
      isPending: false,
    } as any);

    render(
      <CategoryManagementSection eventoId={eventoId} />,
      { wrapper: createWrapper() }
    );

    const precioInput = screen.getByLabelText(/precio/i);
    const submitButton = screen.getByRole('button', { name: /crear categoría/i });

    // Fill only precio, leave nombre empty
    await user.type(precioInput, '100');
    await user.click(submitButton);

    await waitFor(() => {
      expect(mockMutate).not.toHaveBeenCalled();
      expect(screen.getByText(/nombre.*requerido/i)).toBeInTheDocument();
    });
  });

  /**
   * Test form validation rejects negative precio
   * Requirements: 1.2
   */
  it('rejects negative precio', async () => {
    const user = userEvent.setup();
    const eventoId = '123e4567-e89b-12d3-a456-426614174000';
    const mockMutate = vi.fn();
    
    vi.mocked(useCreateCategory).mockReturnValue({
      mutate: mockMutate,
      isPending: false,
    } as any);

    render(
      <CategoryManagementSection eventoId={eventoId} />,
      { wrapper: createWrapper() }
    );

    const nombreInput = screen.getByLabelText(/nombre/i);
    const precioInput = screen.getByLabelText(/precio/i);
    const submitButton = screen.getByRole('button', { name: /crear categoría/i });

    await user.type(nombreInput, 'VIP');
    await user.type(precioInput, '-50');
    await user.click(submitButton);

    await waitFor(() => {
      expect(mockMutate).not.toHaveBeenCalled();
      expect(screen.getByText(/precio.*mayor/i)).toBeInTheDocument();
    });
  });

  /**
   * Test successful submission calls mutation
   * Requirements: 1.3
   */
  it('calls mutation on successful submission', async () => {
    const user = userEvent.setup();
    const eventoId = '123e4567-e89b-12d3-a456-426614174000';
    const mockMutate = vi.fn((data, callbacks) => {
      // Simulate successful mutation
      callbacks?.onSuccess?.();
    });
    
    vi.mocked(useCreateCategory).mockReturnValue({
      mutate: mockMutate,
      isPending: false,
    } as any);

    render(
      <CategoryManagementSection eventoId={eventoId} />,
      { wrapper: createWrapper() }
    );

    const nombreInput = screen.getByLabelText(/nombre/i);
    const descripcionInput = screen.getByLabelText(/descripción/i);
    const precioInput = screen.getByLabelText(/precio/i);
    const submitButton = screen.getByRole('button', { name: /crear categoría/i });

    await user.type(nombreInput, 'VIP');
    await user.type(descripcionInput, 'VIP seats');
    await user.type(precioInput, '100.50');
    await user.click(submitButton);

    await waitFor(() => {
      expect(mockMutate).toHaveBeenCalledWith(
        expect.objectContaining({
          nombre: 'VIP',
          descripcion: 'VIP seats',
          precio: 100.50,
          eventoId,
        }),
        expect.any(Object)
      );
    });
  });
});
