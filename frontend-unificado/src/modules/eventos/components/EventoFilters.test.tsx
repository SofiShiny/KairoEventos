import { describe, it, expect, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { EventoFilters } from './EventoFilters';

/**
 * Unit tests for EventoFilters component
 * 
 * Task 11.1: Write unit tests for Eventos components
 * - Test filtros funcionan correctamente
 * - Test búsqueda funciona correctamente
 * 
 * Requirements tested:
 * - 7.3: Filter eventos by date and location
 * - 7.4: Search eventos by name
 */

describe('EventoFilters', () => {
  describe('Search Functionality (Requirement 7.4)', () => {
    it('should render search input with placeholder', () => {
      const onFiltersChange = vi.fn();
      render(<EventoFilters onFiltersChange={onFiltersChange} />);
      
      const searchInput = screen.getByPlaceholderText('Buscar eventos por nombre...');
      expect(searchInput).toBeInTheDocument();
    });

    it('should call onFiltersChange when search text is entered', async () => {
      const user = userEvent.setup();
      const onFiltersChange = vi.fn();
      
      render(<EventoFilters onFiltersChange={onFiltersChange} />);
      
      const searchInput = screen.getByPlaceholderText('Buscar eventos por nombre...');
      await user.type(searchInput, 'Concierto');
      
      await waitFor(() => {
        expect(onFiltersChange).toHaveBeenCalled();
      });
      
      // Check that the last call includes the search term
      const lastCall = onFiltersChange.mock.calls[onFiltersChange.mock.calls.length - 1][0];
      expect(lastCall.busqueda).toBe('Concierto');
    });

    it('should update search value as user types', async () => {
      const user = userEvent.setup();
      const onFiltersChange = vi.fn();
      
      render(<EventoFilters onFiltersChange={onFiltersChange} />);
      
      const searchInput = screen.getByPlaceholderText('Buscar eventos por nombre...') as HTMLInputElement;
      await user.type(searchInput, 'Rock');
      
      expect(searchInput.value).toBe('Rock');
    });

    it('should clear search when input is emptied', async () => {
      const user = userEvent.setup();
      const onFiltersChange = vi.fn();
      
      render(<EventoFilters onFiltersChange={onFiltersChange} />);
      
      const searchInput = screen.getByPlaceholderText('Buscar eventos por nombre...');
      await user.type(searchInput, 'Test');
      await user.clear(searchInput);
      
      await waitFor(() => {
        const lastCall = onFiltersChange.mock.calls[onFiltersChange.mock.calls.length - 1][0];
        expect(lastCall.busqueda).toBeUndefined();
      });
    });

    it('should display search icon', () => {
      const onFiltersChange = vi.fn();
      render(<EventoFilters onFiltersChange={onFiltersChange} />);
      
      // MUI SearchIcon should be rendered
      const searchInput = screen.getByPlaceholderText('Buscar eventos por nombre...');
      expect(searchInput.parentElement?.querySelector('svg')).toBeInTheDocument();
    });
  });

  describe('Date Filter (Requirement 7.3)', () => {
    it('should render date input', () => {
      const onFiltersChange = vi.fn();
      render(<EventoFilters onFiltersChange={onFiltersChange} />);
      
      const dateInput = screen.getByLabelText('Fecha');
      expect(dateInput).toBeInTheDocument();
      expect(dateInput).toHaveAttribute('type', 'date');
    });

    it('should call onFiltersChange when date is selected', async () => {
      const user = userEvent.setup();
      const onFiltersChange = vi.fn();
      
      render(<EventoFilters onFiltersChange={onFiltersChange} />);
      
      const dateInput = screen.getByLabelText('Fecha');
      await user.type(dateInput, '2024-06-15');
      
      await waitFor(() => {
        expect(onFiltersChange).toHaveBeenCalled();
      });
      
      const lastCall = onFiltersChange.mock.calls[onFiltersChange.mock.calls.length - 1][0];
      expect(lastCall.fecha).toBeInstanceOf(Date);
    });

    it('should clear date filter when date input is cleared', async () => {
      const user = userEvent.setup();
      const onFiltersChange = vi.fn();
      
      render(<EventoFilters onFiltersChange={onFiltersChange} />);
      
      const dateInput = screen.getByLabelText('Fecha');
      await user.type(dateInput, '2024-06-15');
      await user.clear(dateInput);
      
      await waitFor(() => {
        const lastCall = onFiltersChange.mock.calls[onFiltersChange.mock.calls.length - 1][0];
        expect(lastCall.fecha).toBeUndefined();
      });
    });
  });

  describe('Location Filter (Requirement 7.3)', () => {
    it('should render location input', () => {
      const onFiltersChange = vi.fn();
      render(<EventoFilters onFiltersChange={onFiltersChange} />);
      
      const locationInput = screen.getByLabelText('Ubicación');
      expect(locationInput).toBeInTheDocument();
    });

    it('should call onFiltersChange when location is entered', async () => {
      const user = userEvent.setup();
      const onFiltersChange = vi.fn();
      
      render(<EventoFilters onFiltersChange={onFiltersChange} />);
      
      const locationInput = screen.getByLabelText('Ubicación');
      await user.type(locationInput, 'Estadio Nacional');
      
      await waitFor(() => {
        expect(onFiltersChange).toHaveBeenCalled();
      });
      
      const lastCall = onFiltersChange.mock.calls[onFiltersChange.mock.calls.length - 1][0];
      expect(lastCall.ubicacion).toBe('Estadio Nacional');
    });

    it('should update location value as user types', async () => {
      const user = userEvent.setup();
      const onFiltersChange = vi.fn();
      
      render(<EventoFilters onFiltersChange={onFiltersChange} />);
      
      const locationInput = screen.getByLabelText('Ubicación') as HTMLInputElement;
      await user.type(locationInput, 'Teatro');
      
      expect(locationInput.value).toBe('Teatro');
    });

    it('should clear location filter when input is emptied', async () => {
      const user = userEvent.setup();
      const onFiltersChange = vi.fn();
      
      render(<EventoFilters onFiltersChange={onFiltersChange} />);
      
      const locationInput = screen.getByLabelText('Ubicación');
      await user.type(locationInput, 'Test');
      await user.clear(locationInput);
      
      await waitFor(() => {
        const lastCall = onFiltersChange.mock.calls[onFiltersChange.mock.calls.length - 1][0];
        expect(lastCall.ubicacion).toBeUndefined();
      });
    });
  });

  describe('Combined Filters', () => {
    it('should handle multiple filters simultaneously', async () => {
      const user = userEvent.setup();
      const onFiltersChange = vi.fn();
      
      render(<EventoFilters onFiltersChange={onFiltersChange} />);
      
      // Set search
      const searchInput = screen.getByPlaceholderText('Buscar eventos por nombre...');
      await user.type(searchInput, 'Concierto');
      
      // Set date
      const dateInput = screen.getByLabelText('Fecha');
      await user.type(dateInput, '2024-06-15');
      
      // Set location
      const locationInput = screen.getByLabelText('Ubicación');
      await user.type(locationInput, 'Estadio');
      
      await waitFor(() => {
        const lastCall = onFiltersChange.mock.calls[onFiltersChange.mock.calls.length - 1][0];
        expect(lastCall.busqueda).toBe('Concierto'); // Search value
        expect(lastCall.fecha).toBeInstanceOf(Date);
        expect(lastCall.ubicacion).toBe('Estadio');
      });
    });

    it('should preserve other filters when one filter changes', async () => {
      const user = userEvent.setup();
      const onFiltersChange = vi.fn();
      
      render(<EventoFilters onFiltersChange={onFiltersChange} />);
      
      // Set initial filters
      await user.type(screen.getByPlaceholderText('Buscar eventos por nombre...'), 'Test');
      await user.type(screen.getByLabelText('Fecha'), '2024-06-15');
      
      // Change only location
      await user.type(screen.getByLabelText('Ubicación'), 'Teatro');
      
      await waitFor(() => {
        const lastCall = onFiltersChange.mock.calls[onFiltersChange.mock.calls.length - 1][0];
        expect(lastCall.busqueda).toBe('Test');
        expect(lastCall.fecha).toBeInstanceOf(Date);
        expect(lastCall.ubicacion).toBe('Teatro');
      });
    });
  });

  describe('Responsive Layout', () => {
    it('should render all filter inputs in a stack layout', () => {
      const onFiltersChange = vi.fn();
      render(<EventoFilters onFiltersChange={onFiltersChange} />);
      
      expect(screen.getByPlaceholderText('Buscar eventos por nombre...')).toBeInTheDocument();
      expect(screen.getByLabelText('Fecha')).toBeInTheDocument();
      expect(screen.getByLabelText('Ubicación')).toBeInTheDocument();
    });
  });
});
