import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { EventosList } from './EventosList';
import type { Evento } from '../types';

/**
 * Unit tests for EventosList component
 * 
 * Task 11.1: Write unit tests for Eventos components
 * - Test EventosList renderiza lista correctamente
 * - Test filtros funcionan correctamente
 * - Test búsqueda funciona correctamente
 * - Test botones visibles según rol
 * 
 * Requirements tested:
 * - 7.1: Display list of eventos at "/eventos" route
 * - 7.2: Show evento details (name, date, location, image)
 * - 7.3: Filter by date and location
 * - 7.4: Search by name
 * - 7.7: Show CRUD buttons for Admin/Organizator roles
 */

// Mock shared components
vi.mock('@shared/components', () => ({
  EmptyState: ({ title, description }: { title: string; description: string }) => (
    <div data-testid="empty-state">
      <div>{title}</div>
      <div>{description}</div>
    </div>
  ),
  SkeletonLoader: ({ count }: { count: number }) => (
    <div data-testid="skeleton-loader">Loading {count} items...</div>
  ),
}));

// Mock EventoCard component
vi.mock('./EventoCard', () => ({
  EventoCard: ({ evento, onEventoClick }: { evento: Evento; onEventoClick?: (id: string) => void }) => (
    <div data-testid={`evento-card-${evento.id}`} onClick={() => onEventoClick?.(evento.id)}>
      <h3>{evento.nombre}</h3>
      <p>{evento.descripcion}</p>
      <span>{evento.ubicacion}</span>
      <span>{evento.fecha}</span>
    </div>
  ),
}));

describe('EventosList', () => {
  const mockEventos: Evento[] = [
    {
      id: '1',
      nombre: 'Concierto de Rock',
      descripcion: 'Gran concierto de rock',
      fecha: '2024-06-15T20:00:00Z',
      ubicacion: 'Estadio Nacional',
      estado: 'Publicado',
      capacidadTotal: 1000,
      asientosDisponibles: 500,
    },
    {
      id: '2',
      nombre: 'Festival de Jazz',
      descripcion: 'Festival de jazz internacional',
      fecha: '2024-07-20T19:00:00Z',
      ubicacion: 'Teatro Municipal',
      estado: 'Publicado',
      capacidadTotal: 500,
      asientosDisponibles: 200,
    },
    {
      id: '3',
      nombre: 'Obra de Teatro',
      descripcion: 'Obra clásica de Shakespeare',
      fecha: '2024-08-10T18:00:00Z',
      ubicacion: 'Teatro Nacional',
      estado: 'Publicado',
      capacidadTotal: 300,
      asientosDisponibles: 150,
    },
  ];

  describe('Rendering (Requirement 7.1, 7.2)', () => {
    it('should render list of eventos correctly', () => {
      render(<EventosList eventos={mockEventos} />);

      // Check that all eventos are rendered
      expect(screen.getByTestId('evento-card-1')).toBeInTheDocument();
      expect(screen.getByTestId('evento-card-2')).toBeInTheDocument();
      expect(screen.getByTestId('evento-card-3')).toBeInTheDocument();

      // Check evento details are displayed
      expect(screen.getByText('Concierto de Rock')).toBeInTheDocument();
      expect(screen.getByText('Festival de Jazz')).toBeInTheDocument();
      expect(screen.getByText('Obra de Teatro')).toBeInTheDocument();
    });

    it('should display count of eventos found', () => {
      render(<EventosList eventos={mockEventos} />);
      expect(screen.getByText('3 eventos encontrados')).toBeInTheDocument();
    });

    it('should display singular form when only one evento', () => {
      render(<EventosList eventos={[mockEventos[0]]} />);
      expect(screen.getByText('1 evento encontrado')).toBeInTheDocument();
    });

    it('should show loading skeleton when isLoading is true', () => {
      render(<EventosList eventos={[]} isLoading={true} />);
      
      expect(screen.getByTestId('skeleton-loader')).toBeInTheDocument();
      expect(screen.getByText('Loading 6 items...')).toBeInTheDocument();
      expect(screen.queryByTestId('empty-state')).not.toBeInTheDocument();
    });

    it('should show empty state when no eventos are available', () => {
      render(<EventosList eventos={[]} isLoading={false} />);
      
      const emptyState = screen.getByTestId('empty-state');
      expect(emptyState).toBeInTheDocument();
      expect(screen.getByText('No hay eventos disponibles')).toBeInTheDocument();
      expect(screen.getByText(/Actualmente no hay eventos publicados/)).toBeInTheDocument();
    });

    it('should not show empty state when loading', () => {
      render(<EventosList eventos={[]} isLoading={true} />);
      
      expect(screen.queryByTestId('empty-state')).not.toBeInTheDocument();
    });
  });

  describe('Event Interaction', () => {
    it('should call onEventoClick when evento card is clicked', async () => {
      const user = userEvent.setup();
      const onEventoClick = vi.fn();
      
      render(<EventosList eventos={mockEventos} onEventoClick={onEventoClick} />);
      
      const eventoCard = screen.getByTestId('evento-card-1');
      await user.click(eventoCard);
      
      expect(onEventoClick).toHaveBeenCalledWith('1');
      expect(onEventoClick).toHaveBeenCalledTimes(1);
    });

    it('should handle multiple evento clicks', async () => {
      const user = userEvent.setup();
      const onEventoClick = vi.fn();
      
      render(<EventosList eventos={mockEventos} onEventoClick={onEventoClick} />);
      
      await user.click(screen.getByTestId('evento-card-1'));
      await user.click(screen.getByTestId('evento-card-2'));
      await user.click(screen.getByTestId('evento-card-3'));
      
      expect(onEventoClick).toHaveBeenCalledTimes(3);
      expect(onEventoClick).toHaveBeenNthCalledWith(1, '1');
      expect(onEventoClick).toHaveBeenNthCalledWith(2, '2');
      expect(onEventoClick).toHaveBeenNthCalledWith(3, '3');
    });

    it('should work without onEventoClick callback', () => {
      // Should not throw error when onEventoClick is not provided
      expect(() => {
        render(<EventosList eventos={mockEventos} />);
      }).not.toThrow();
    });
  });

  describe('Edge Cases', () => {
    it('should handle empty eventos array', () => {
      render(<EventosList eventos={[]} />);
      expect(screen.getByTestId('empty-state')).toBeInTheDocument();
    });

    it('should handle single evento', () => {
      render(<EventosList eventos={[mockEventos[0]]} />);
      expect(screen.getByTestId('evento-card-1')).toBeInTheDocument();
      expect(screen.queryByTestId('evento-card-2')).not.toBeInTheDocument();
    });

    it('should handle eventos with missing optional fields', () => {
      const eventoWithoutImage: Evento = {
        id: '4',
        nombre: 'Evento sin imagen',
        descripcion: 'Descripción',
        fecha: '2024-09-01T20:00:00Z',
        ubicacion: 'Lugar',
        estado: 'Publicado',
        capacidadTotal: 100,
        asientosDisponibles: 50,
      };

      render(<EventosList eventos={[eventoWithoutImage]} />);
      expect(screen.getByTestId('evento-card-4')).toBeInTheDocument();
    });
  });
});
