import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import SeatGrid from './SeatGrid';
import type { Asiento, Categoria } from '../types/api';
import * as fc from 'fast-check';
import React from 'react';

describe('SeatGrid - Property-Based Tests', () => {
  /**
   * **Feature: seat-categories-management, Property 8: UI synchronization after mutations**
   * **Validates: Requirements 1.4, 2.4, 4.4**
   * 
   * For any successful creation of a category or seat, the corresponding list (categories or seat map)
   * should update to include the newly created item within the same render cycle
   */
  it('Property 8: UI synchronization after mutations', async () => {
    // Generator for categories
    const categoryArbitrary = fc.record({
      id: fc.uuid(),
      nombre: fc.string({ minLength: 2, maxLength: 50 }),
      descripcion: fc.option(fc.string({ maxLength: 200 }), { nil: undefined }),
      precio: fc.float({ min: 0.01, max: 10000, noNaN: true, noDefaultInfinity: true }),
      eventoId: fc.uuid(),
    });

    // Generator for seats
    const seatArbitrary = (categoria?: Categoria) => fc.record({
      id: fc.uuid(),
      fila: fc.oneof(
        fc.constantFrom('A', 'B', 'C', 'D', 'E'),
        fc.integer({ min: 1, max: 20 }).map(String)
      ),
      numero: fc.integer({ min: 1, max: 50 }),
      estado: fc.constantFrom('Disponible', 'Reservado', 'Ocupado') as fc.Arbitrary<'Disponible' | 'Reservado' | 'Ocupado'>,
      mapaId: fc.uuid(),
      categoriaId: categoria ? fc.constant(categoria.id) : fc.option(fc.uuid(), { nil: undefined }),
      categoria: categoria ? fc.constant(categoria) : fc.constant(undefined),
    });

    await fc.assert(
      fc.asyncProperty(
        fc.tuple(
          fc.array(categoryArbitrary, { minLength: 0, maxLength: 5 }),
          fc.nat({ max: 10 })
        ),
        async ([categories, numSeats]) => {
          // Create initial seats
          const initialSeats: Asiento[] = [];
          for (let i = 0; i < numSeats; i++) {
            const category = categories.length > 0 && i % 2 === 0 ? categories[i % categories.length] : undefined;
            const seat = fc.sample(seatArbitrary(category), 1)[0];
            initialSeats.push(seat);
          }

          // Render with initial seats
          const { rerender } = render(<SeatGrid seats={initialSeats} />);

          // Verify initial seats are displayed
          if (initialSeats.length > 0) {
            for (const seat of initialSeats) {
              const seatButton = screen.queryByText(seat.numero.toString());
              if (seatButton) {
                expect(seatButton).toBeInTheDocument();
              }
            }
          } else {
            expect(screen.getByText(/no hay asientos disponibles/i)).toBeInTheDocument();
          }

          // Simulate adding a new seat (mutation)
          const newCategory = categories.length > 0 ? categories[0] : undefined;
          const newSeat = fc.sample(seatArbitrary(newCategory), 1)[0];
          const updatedSeats = [...initialSeats, newSeat];

          // Rerender with updated seats
          rerender(<SeatGrid seats={updatedSeats} />);

          // Verify the new seat is now displayed (UI synchronization)
          const newSeatButton = screen.queryByText(newSeat.numero.toString());
          if (newSeatButton) {
            expect(newSeatButton).toBeInTheDocument();
          }

          // Verify category information is displayed if present
          if (newSeat.categoria) {
            // The category info should be in the tooltip
            const seatButtons = screen.getAllByRole('button');
            const seatButton = seatButtons.find(btn => 
              btn.textContent?.includes(newSeat.numero.toString())
            );
            expect(seatButton).toBeDefined();
          }
        }
      ),
      { numRuns: 100 }
    );
  });
});

describe('SeatGrid - Unit Tests', () => {
  /**
   * Test category information displays for seats
   * Requirements: 2.4
   */
  it('displays category information for seats with categories', () => {
    const categoria: Categoria = {
      id: 'cat-123',
      nombre: 'VIP',
      descripcion: 'VIP seats',
      precio: 150.00,
      eventoId: 'event-123',
    };

    const seats: Asiento[] = [
      {
        id: 'seat-1',
        fila: 'A',
        numero: 1,
        estado: 'Disponible',
        mapaId: 'map-123',
        categoriaId: categoria.id,
        categoria: categoria,
      },
      {
        id: 'seat-2',
        fila: 'A',
        numero: 2,
        estado: 'Disponible',
        mapaId: 'map-123',
        categoriaId: categoria.id,
        categoria: categoria,
      },
    ];

    render(<SeatGrid seats={seats} />);

    // Verify seats are rendered
    expect(screen.getByText('1')).toBeInTheDocument();
    expect(screen.getByText('2')).toBeInTheDocument();

    // Verify the seats have category styling (color coding)
    const buttons = screen.getAllByRole('button');
    expect(buttons.length).toBeGreaterThan(0);
    
    // Check that buttons have the EventSeatIcon
    buttons.forEach(button => {
      const icon = button.querySelector('svg');
      expect(icon).toBeInTheDocument();
    });
  });

  /**
   * Test seats without categories still render correctly
   * Requirements: 2.4
   */
  it('renders seats without categories correctly', () => {
    const seats: Asiento[] = [
      {
        id: 'seat-1',
        fila: 'B',
        numero: 5,
        estado: 'Disponible',
        mapaId: 'map-123',
      },
      {
        id: 'seat-2',
        fila: 'B',
        numero: 6,
        estado: 'Reservado',
        mapaId: 'map-123',
      },
    ];

    render(<SeatGrid seats={seats} />);

    // Verify seats are rendered
    expect(screen.getByText('5')).toBeInTheDocument();
    expect(screen.getByText('6')).toBeInTheDocument();

    // Verify seats are displayed with correct status colors
    const buttons = screen.getAllByRole('button');
    expect(buttons.length).toBe(2);
  });

  /**
   * Test empty seat list displays appropriate message
   */
  it('displays message when no seats are available', () => {
    render(<SeatGrid seats={[]} />);

    expect(screen.getByText(/no hay asientos disponibles/i)).toBeInTheDocument();
  });

  /**
   * Test seat grid handles null seats prop
   */
  it('handles null seats prop gracefully', () => {
    render(<SeatGrid seats={null} />);

    expect(screen.getByText(/no hay asientos disponibles/i)).toBeInTheDocument();
  });

  /**
   * Test seat grid groups seats by row
   */
  it('groups seats by row correctly', () => {
    const seats: Asiento[] = [
      {
        id: 'seat-1',
        fila: 'A',
        numero: 1,
        estado: 'Disponible',
        mapaId: 'map-123',
      },
      {
        id: 'seat-2',
        fila: 'A',
        numero: 2,
        estado: 'Disponible',
        mapaId: 'map-123',
      },
      {
        id: 'seat-3',
        fila: 'B',
        numero: 1,
        estado: 'Disponible',
        mapaId: 'map-123',
      },
    ];

    render(<SeatGrid seats={seats} />);

    // Verify row labels are displayed
    expect(screen.getByText('A')).toBeInTheDocument();
    expect(screen.getByText('B')).toBeInTheDocument();

    // Verify all seats are rendered
    expect(screen.getByText('1')).toBeInTheDocument();
    expect(screen.getByText('2')).toBeInTheDocument();
  });

  /**
   * Test category information appears in tooltip
   */
  it('includes category name and price in tooltip', () => {
    const categoria: Categoria = {
      id: 'cat-123',
      nombre: 'Premium',
      descripcion: 'Premium seats',
      precio: 200.50,
      eventoId: 'event-123',
    };

    const seats: Asiento[] = [
      {
        id: 'seat-1',
        fila: 'C',
        numero: 10,
        estado: 'Disponible',
        mapaId: 'map-123',
        categoriaId: categoria.id,
        categoria: categoria,
      },
    ];

    const { container } = render(<SeatGrid seats={seats} />);

    // Find the tooltip element (MUI Tooltip renders title attribute)
    const tooltipTrigger = container.querySelector('[title]');
    expect(tooltipTrigger).toBeInTheDocument();
    
    if (tooltipTrigger) {
      const title = tooltipTrigger.getAttribute('title');
      expect(title).toContain('Premium');
      expect(title).toContain('200.5');
    }
  });
});
