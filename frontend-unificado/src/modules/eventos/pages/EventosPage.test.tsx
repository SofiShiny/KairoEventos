import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { EventosPage } from './EventosPage';
import type { Evento } from '../types';

/**
 * Unit tests for EventosPage component
 * 
 * Task 11.1: Write unit tests for Eventos components
 * - Test botones visibles según rol
 * - Test filtros funcionan correctamente
 * - Test búsqueda funciona correctamente
 * 
 * Requirements tested:
 * - 7.1: Display list of eventos at "/eventos" route
 * - 7.3: Filter by date and location
 * - 7.4: Search by name
 * - 7.7: Show CRUD buttons for Admin/Organizator roles
 */

// Mock data
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

// Mock hooks
let mockHasRole = vi.fn(() => false);
let mockEventosData = mockEventos;
let mockIsLoading = false;
let mockError: Error | null = null;
const mockRefetch = vi.fn();
const mockCreateEvento = vi.fn();
const mockUpdateEvento = vi.fn();
const mockNavigate = vi.fn();

vi.mock('../hooks', () => ({
  useEventos: () => ({
    data: mockEventosData,
    isLoading: mockIsLoading,
    error: mockError,
    refetch: mockRefetch,
  }),
  useCreateEvento: () => ({
    mutate: mockCreateEvento,
    isPending: false,
  }),
  useUpdateEvento: () => ({
    mutate: mockUpdateEvento,
    isPending: false,
  }),
}));

vi.mock('@/context/AuthContext', () => ({
  useAuth: () => ({
    hasRole: mockHasRole,
    isAuthenticated: true,
    user: { id: '1', username: 'testuser', email: 'test@test.com', nombre: 'Test User', roles: [] },
  }),
}));

vi.mock('react-router-dom', () => ({
  useNavigate: () => mockNavigate,
}));

// Mock components
vi.mock('../components', () => ({
  EventosList: ({ eventos, onEventoClick }: { eventos: Evento[]; onEventoClick: (id: string) => void }) => (
    <div data-testid="eventos-list">
      {eventos.map((evento) => (
        <div key={evento.id} data-testid={`evento-${evento.id}`} onClick={() => onEventoClick(evento.id)}>
          {evento.nombre}
        </div>
      ))}
    </div>
  ),
  EventoFilters: ({ onFiltersChange }: { onFiltersChange: (filters: any) => void }) => (
    <div data-testid="evento-filters">
      <input
        data-testid="search-input"
        placeholder="Buscar"
        onChange={(e) => onFiltersChange({ busqueda: e.target.value })}
      />
      <input
        data-testid="date-input"
        type="date"
        onChange={(e) => onFiltersChange({ fecha: e.target.value ? new Date(e.target.value) : undefined })}
      />
      <input
        data-testid="location-input"
        placeholder="Ubicación"
        onChange={(e) => onFiltersChange({ ubicacion: e.target.value })}
      />
    </div>
  ),
  EventoForm: ({ open, onClose }: { open: boolean; onClose: () => void }) =>
    open ? (
      <div data-testid="evento-form">
        <button onClick={onClose}>Close</button>
      </div>
    ) : null,
}));

vi.mock('@shared/components', () => ({
  ErrorMessage: ({ error, onRetry }: { error: Error; onRetry: () => void }) => (
    <div data-testid="error-message">
      <p>{error.message}</p>
      <button onClick={onRetry}>Retry</button>
    </div>
  ),
}));

describe('EventosPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockHasRole = vi.fn(() => false);
    mockEventosData = mockEventos;
    mockIsLoading = false;
    mockError = null;
  });

  describe('Rendering', () => {
    it('should render page title and description', () => {
      render(<EventosPage />);
      
      expect(screen.getByText('Eventos')).toBeInTheDocument();
      expect(screen.getByText('Explora y descubre eventos disponibles')).toBeInTheDocument();
    });

    it('should render EventoFilters component', () => {
      render(<EventosPage />);
      expect(screen.getByTestId('evento-filters')).toBeInTheDocument();
    });

    it('should render EventosList component', () => {
      render(<EventosPage />);
      expect(screen.getByTestId('eventos-list')).toBeInTheDocument();
    });

    it('should display all eventos initially', () => {
      render(<EventosPage />);
      
      expect(screen.getByTestId('evento-1')).toBeInTheDocument();
      expect(screen.getByTestId('evento-2')).toBeInTheDocument();
      expect(screen.getByTestId('evento-3')).toBeInTheDocument();
    });
  });

  describe('Role-Based Button Visibility (Requirement 7.7)', () => {
    it('should NOT show "Crear Evento" button for regular users (Asistente)', () => {
      mockHasRole = vi.fn(() => false);
      render(<EventosPage />);
      
      expect(screen.queryByText('Crear Evento')).not.toBeInTheDocument();
    });

    it('should show "Crear Evento" button for Admin users', () => {
      mockHasRole = vi.fn((role: string) => role === 'Admin');
      render(<EventosPage />);
      
      expect(screen.getByText('Crear Evento')).toBeInTheDocument();
    });

    it('should show "Crear Evento" button for Organizator users', () => {
      mockHasRole = vi.fn((role: string) => role === 'Organizator');
      render(<EventosPage />);
      
      expect(screen.getByText('Crear Evento')).toBeInTheDocument();
    });

    it('should open form dialog when "Crear Evento" is clicked', async () => {
      const user = userEvent.setup();
      mockHasRole = vi.fn((role: string) => role === 'Admin');
      
      render(<EventosPage />);
      
      const createButton = screen.getByText('Crear Evento');
      await user.click(createButton);
      
      expect(screen.getByTestId('evento-form')).toBeInTheDocument();
    });
  });

  describe('Search Functionality (Requirement 7.4)', () => {
    it('should filter eventos by search term in name', async () => {
      const user = userEvent.setup();
      render(<EventosPage />);
      
      const searchInput = screen.getByTestId('search-input');
      await user.type(searchInput, 'Rock');
      
      await waitFor(() => {
        expect(screen.getByTestId('evento-1')).toBeInTheDocument();
        expect(screen.queryByTestId('evento-2')).not.toBeInTheDocument();
        expect(screen.queryByTestId('evento-3')).not.toBeInTheDocument();
      });
    });

    it('should filter eventos by search term in description', async () => {
      const user = userEvent.setup();
      render(<EventosPage />);
      
      const searchInput = screen.getByTestId('search-input');
      await user.type(searchInput, 'jazz');
      
      await waitFor(() => {
        expect(screen.queryByTestId('evento-1')).not.toBeInTheDocument();
        expect(screen.getByTestId('evento-2')).toBeInTheDocument();
        expect(screen.queryByTestId('evento-3')).not.toBeInTheDocument();
      });
    });

    it('should be case-insensitive when searching', async () => {
      const user = userEvent.setup();
      render(<EventosPage />);
      
      const searchInput = screen.getByTestId('search-input');
      await user.type(searchInput, 'ROCK');
      
      await waitFor(() => {
        expect(screen.getByTestId('evento-1')).toBeInTheDocument();
      });
    });

    it('should show all eventos when search is cleared', async () => {
      const user = userEvent.setup();
      render(<EventosPage />);
      
      const searchInput = screen.getByTestId('search-input');
      await user.type(searchInput, 'Rock');
      await user.clear(searchInput);
      
      await waitFor(() => {
        expect(screen.getByTestId('evento-1')).toBeInTheDocument();
        expect(screen.getByTestId('evento-2')).toBeInTheDocument();
        expect(screen.getByTestId('evento-3')).toBeInTheDocument();
      });
    });
  });

  describe('Date Filter (Requirement 7.3)', () => {
    it('should filter eventos by date', async () => {
      const user = userEvent.setup();
      render(<EventosPage />);
      
      const dateInput = screen.getByTestId('date-input');
      await user.type(dateInput, '2024-06-15');
      
      await waitFor(() => {
        expect(screen.getByTestId('evento-1')).toBeInTheDocument();
        expect(screen.queryByTestId('evento-2')).not.toBeInTheDocument();
        expect(screen.queryByTestId('evento-3')).not.toBeInTheDocument();
      });
    });

    it('should show all eventos when date filter is cleared', async () => {
      const user = userEvent.setup();
      render(<EventosPage />);
      
      const dateInput = screen.getByTestId('date-input');
      await user.type(dateInput, '2024-06-15');
      await user.clear(dateInput);
      
      await waitFor(() => {
        expect(screen.getByTestId('evento-1')).toBeInTheDocument();
        expect(screen.getByTestId('evento-2')).toBeInTheDocument();
        expect(screen.getByTestId('evento-3')).toBeInTheDocument();
      });
    });
  });

  describe('Location Filter (Requirement 7.3)', () => {
    it('should filter eventos by location', async () => {
      const user = userEvent.setup();
      render(<EventosPage />);
      
      const locationInput = screen.getByTestId('location-input');
      await user.type(locationInput, 'Teatro');
      
      await waitFor(() => {
        expect(screen.queryByTestId('evento-1')).not.toBeInTheDocument();
        expect(screen.getByTestId('evento-2')).toBeInTheDocument();
        expect(screen.getByTestId('evento-3')).toBeInTheDocument();
      });
    });

    it('should be case-insensitive when filtering by location', async () => {
      const user = userEvent.setup();
      render(<EventosPage />);
      
      const locationInput = screen.getByTestId('location-input');
      await user.type(locationInput, 'TEATRO');
      
      await waitFor(() => {
        expect(screen.getByTestId('evento-2')).toBeInTheDocument();
        expect(screen.getByTestId('evento-3')).toBeInTheDocument();
      });
    });
  });

  describe('Combined Filters', () => {
    it('should apply multiple filters simultaneously', async () => {
      const user = userEvent.setup();
      render(<EventosPage />);
      
      // Search for "Teatro" and filter by location "Teatro Municipal"
      const searchInput = screen.getByTestId('search-input');
      await user.type(searchInput, 'Festival');
      
      const locationInput = screen.getByTestId('location-input');
      await user.type(locationInput, 'Municipal');
      
      await waitFor(() => {
        expect(screen.queryByTestId('evento-1')).not.toBeInTheDocument();
        expect(screen.getByTestId('evento-2')).toBeInTheDocument();
        expect(screen.queryByTestId('evento-3')).not.toBeInTheDocument();
      });
    });
  });

  describe('Sorting', () => {
    it('should sort eventos by date (upcoming first)', () => {
      render(<EventosPage />);
      
      const eventosList = screen.getByTestId('eventos-list');
      const eventos = eventosList.querySelectorAll('[data-testid^="evento-"]');
      
      // Should be in order: evento-1, evento-2, evento-3 (by date)
      expect(eventos[0]).toHaveAttribute('data-testid', 'evento-1');
      expect(eventos[1]).toHaveAttribute('data-testid', 'evento-2');
      expect(eventos[2]).toHaveAttribute('data-testid', 'evento-3');
    });
  });

  describe('Navigation', () => {
    it('should navigate to evento detail page when evento is clicked', async () => {
      const user = userEvent.setup();
      render(<EventosPage />);
      
      const evento = screen.getByTestId('evento-1');
      await user.click(evento);
      
      expect(mockNavigate).toHaveBeenCalledWith('/eventos/1');
    });
  });

  describe('Error Handling', () => {
    it('should display error message when eventos fail to load', () => {
      mockError = new Error('Failed to load eventos');
      render(<EventosPage />);
      
      expect(screen.getByTestId('error-message')).toBeInTheDocument();
      expect(screen.getByText('Failed to load eventos')).toBeInTheDocument();
    });

    it('should call refetch when retry button is clicked', async () => {
      const user = userEvent.setup();
      mockError = new Error('Failed to load eventos');
      render(<EventosPage />);
      
      const retryButton = screen.getByText('Retry');
      await user.click(retryButton);
      
      expect(mockRefetch).toHaveBeenCalledTimes(1);
    });
  });

  describe('Loading State', () => {
    it('should pass loading state to EventosList', () => {
      mockIsLoading = true;
      mockEventosData = [];
      render(<EventosPage />);
      
      // EventosList should receive isLoading prop
      expect(screen.getByTestId('eventos-list')).toBeInTheDocument();
    });
  });

  describe('Empty State', () => {
    it('should show empty list when no eventos match filters', async () => {
      const user = userEvent.setup();
      render(<EventosPage />);
      
      const searchInput = screen.getByTestId('search-input');
      await user.type(searchInput, 'NonexistentEvento');
      
      await waitFor(() => {
        const eventosList = screen.getByTestId('eventos-list');
        expect(eventosList.children.length).toBe(0);
      });
    });
  });
});
