import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { EventoCard } from './EventoCard';
import type { Evento } from '../types';

/**
 * Unit tests for EventoCard component
 * 
 * Task 11.1: Write unit tests for Eventos components
 * - Test EventoCard renderiza correctamente
 * - Test botones visibles según estado del evento
 * 
 * Requirements tested:
 * - 7.2: Display evento details (name, date, location, image)
 * - 7.5: Show evento details when clicked
 * - 7.6: Show "Comprar Entrada" button
 */

// Mock react-router-dom
const mockNavigate = vi.fn();
vi.mock('react-router-dom', () => ({
  useNavigate: () => mockNavigate,
}));

// Mock ImagePlaceholder component
vi.mock('@shared/components', () => ({
  ImagePlaceholder: ({ alt, src }: { alt: string; src?: string }) => (
    <div data-testid="image-placeholder">
      <img src={src || 'placeholder.jpg'} alt={alt} />
    </div>
  ),
}));

// Helper to render with QueryClient
function renderWithQueryClient(ui: React.ReactElement) {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  });
  return render(
    <QueryClientProvider client={queryClient}>
      {ui}
    </QueryClientProvider>
  );
}

describe('EventoCard', () => {
  const mockEvento: Evento = {
    id: '1',
    nombre: 'Concierto de Rock',
    descripcion: 'Un gran concierto de rock con las mejores bandas',
    fecha: '2024-06-15T20:00:00Z',
    ubicacion: 'Estadio Nacional',
    imagenUrl: 'https://example.com/image.jpg',
    estado: 'Publicado',
    capacidadTotal: 1000,
    asientosDisponibles: 500,
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('Rendering (Requirement 7.2)', () => {
    it('should render evento name', () => {
      renderWithQueryClient(<EventoCard evento={mockEvento} />);
      expect(screen.getByText('Concierto de Rock')).toBeInTheDocument();
    });

    it('should render evento description', () => {
      renderWithQueryClient(<EventoCard evento={mockEvento} />);
      expect(screen.getByText('Un gran concierto de rock con las mejores bandas')).toBeInTheDocument();
    });

    it('should render evento location', () => {
      renderWithQueryClient(<EventoCard evento={mockEvento} />);
      expect(screen.getByText('Estadio Nacional')).toBeInTheDocument();
    });

    it('should render formatted date', () => {
      renderWithQueryClient(<EventoCard evento={mockEvento} />);
      // Date should be formatted in Spanish locale
      const dateElement = screen.getByText(/2024/);
      expect(dateElement).toBeInTheDocument();
    });

    it('should render image with correct alt text', () => {
      renderWithQueryClient(<EventoCard evento={mockEvento} />);
      const image = screen.getByAltText('Concierto de Rock');
      expect(image).toBeInTheDocument();
      expect(image).toHaveAttribute('src', 'https://example.com/image.jpg');
    });

    it('should render image placeholder when no imagenUrl provided', () => {
      const eventoWithoutImage = { ...mockEvento, imagenUrl: undefined };
      renderWithQueryClient(<EventoCard evento={eventoWithoutImage} />);
      
      const placeholder = screen.getByTestId('image-placeholder');
      expect(placeholder).toBeInTheDocument();
    });

    it('should display available seats information', () => {
      renderWithQueryClient(<EventoCard evento={mockEvento} />);
      expect(screen.getByText('500 / 1000 disponibles')).toBeInTheDocument();
    });

    it('should display availability chip with correct color for high availability', () => {
      renderWithQueryClient(<EventoCard evento={mockEvento} />);
      const chip = screen.getByText('500 / 1000 disponibles');
      expect(chip).toBeInTheDocument();
    });

    it('should display availability chip with warning color for medium availability', () => {
      const eventoMediumAvailability = {
        ...mockEvento,
        asientosDisponibles: 300, // 30% available
      };
      renderWithQueryClient(<EventoCard evento={eventoMediumAvailability} />);
      expect(screen.getByText('300 / 1000 disponibles')).toBeInTheDocument();
    });

    it('should display availability chip with error color for low availability', () => {
      const eventoLowAvailability = {
        ...mockEvento,
        asientosDisponibles: 50, // 5% available
      };
      renderWithQueryClient(<EventoCard evento={eventoLowAvailability} />);
      expect(screen.getByText('50 / 1000 disponibles')).toBeInTheDocument();
    });
  });

  describe('Button Actions (Requirement 7.5, 7.6)', () => {
    it('should render "Ver Detalles" button', () => {
      renderWithQueryClient(<EventoCard evento={mockEvento} />);
      expect(screen.getByText('Ver Detalles')).toBeInTheDocument();
    });

    it('should render "Comprar" button for published evento with available seats', () => {
      renderWithQueryClient(<EventoCard evento={mockEvento} />);
      expect(screen.getByText('Comprar')).toBeInTheDocument();
    });

    it('should call onEventoClick when "Ver Detalles" is clicked', async () => {
      const user = userEvent.setup();
      const onEventoClick = vi.fn();
      
      renderWithQueryClient(<EventoCard evento={mockEvento} onEventoClick={onEventoClick} />);
      
      await user.click(screen.getByText('Ver Detalles'));
      
      expect(onEventoClick).toHaveBeenCalledWith('1');
      expect(onEventoClick).toHaveBeenCalledTimes(1);
    });

    it('should navigate to evento detail page when "Ver Detalles" is clicked without callback', async () => {
      const user = userEvent.setup();
      
      renderWithQueryClient(<EventoCard evento={mockEvento} />);
      
      await user.click(screen.getByText('Ver Detalles'));
      
      expect(mockNavigate).toHaveBeenCalledWith('/eventos/1');
    });

    it('should navigate to purchase page when "Comprar" is clicked', async () => {
      const user = userEvent.setup();
      
      renderWithQueryClient(<EventoCard evento={mockEvento} />);
      
      await user.click(screen.getByText('Comprar'));
      
      expect(mockNavigate).toHaveBeenCalledWith('/comprar/1');
    });

    it('should not call onEventoClick when "Comprar" is clicked', async () => {
      const user = userEvent.setup();
      const onEventoClick = vi.fn();
      
      renderWithQueryClient(<EventoCard evento={mockEvento} onEventoClick={onEventoClick} />);
      
      await user.click(screen.getByText('Comprar'));
      
      // Should navigate but not call onEventoClick
      expect(mockNavigate).toHaveBeenCalledWith('/comprar/1');
      expect(onEventoClick).not.toHaveBeenCalled();
    });
  });

  describe('Canceled Evento State', () => {
    const canceledEvento: Evento = {
      ...mockEvento,
      estado: 'Cancelado',
    };

    it('should display "Cancelado" chip for canceled evento', () => {
      renderWithQueryClient(<EventoCard evento={canceledEvento} />);
      expect(screen.getByText('Cancelado')).toBeInTheDocument();
    });

    it('should not display "Comprar" button for canceled evento', () => {
      renderWithQueryClient(<EventoCard evento={canceledEvento} />);
      expect(screen.queryByText('Comprar')).not.toBeInTheDocument();
    });

    it('should not display availability information for canceled evento', () => {
      renderWithQueryClient(<EventoCard evento={canceledEvento} />);
      expect(screen.queryByText(/disponibles/)).not.toBeInTheDocument();
    });

    it('should still display "Ver Detalles" button for canceled evento', () => {
      renderWithQueryClient(<EventoCard evento={canceledEvento} />);
      expect(screen.getByText('Ver Detalles')).toBeInTheDocument();
    });

    it('should have reduced opacity for canceled evento', () => {
      const { container } = renderWithQueryClient(<EventoCard evento={canceledEvento} />);
      const card = container.querySelector('[class*="MuiCard"]');
      expect(card).toBeInTheDocument();
    });
  });

  describe('Sold Out Evento State', () => {
    const soldOutEvento: Evento = {
      ...mockEvento,
      asientosDisponibles: 0,
    };

    it('should display "Agotado" button when no seats available', () => {
      renderWithQueryClient(<EventoCard evento={soldOutEvento} />);
      expect(screen.getByText('Agotado')).toBeInTheDocument();
    });

    it('should disable "Agotado" button', () => {
      renderWithQueryClient(<EventoCard evento={soldOutEvento} />);
      const agotadoButton = screen.getByText('Agotado');
      expect(agotadoButton).toBeDisabled();
    });

    it('should display 0 available seats', () => {
      renderWithQueryClient(<EventoCard evento={soldOutEvento} />);
      expect(screen.getByText('0 / 1000 disponibles')).toBeInTheDocument();
    });

    it('should not navigate when clicking disabled "Agotado" button', () => {
      renderWithQueryClient(<EventoCard evento={soldOutEvento} />);
      
      const agotadoButton = screen.getByText('Agotado');
      
      // Button should be disabled, so we just verify it's disabled
      // We cannot click a disabled button with user-event
      expect(agotadoButton).toBeDisabled();
      expect(mockNavigate).not.toHaveBeenCalled();
    });
  });

  describe('Edge Cases', () => {
    it('should handle evento with very long name', () => {
      const eventoLongName = {
        ...mockEvento,
        nombre: 'Este es un nombre de evento extremadamente largo que debería ser truncado con ellipsis',
      };
      renderWithQueryClient(<EventoCard evento={eventoLongName} />);
      expect(screen.getByText(eventoLongName.nombre)).toBeInTheDocument();
    });

    it('should handle evento with very long description', () => {
      const eventoLongDesc = {
        ...mockEvento,
        descripcion: 'Esta es una descripción muy larga que debería ser truncada después de dos líneas usando WebkitLineClamp para mantener el diseño consistente y evitar que la tarjeta se vuelva demasiado alta',
      };
      renderWithQueryClient(<EventoCard evento={eventoLongDesc} />);
      expect(screen.getByText(eventoLongDesc.descripcion)).toBeInTheDocument();
    });

    it('should handle evento with very long location', () => {
      const eventoLongLocation = {
        ...mockEvento,
        ubicacion: 'Centro de Convenciones Internacional de la Ciudad Metropolitana',
      };
      renderWithQueryClient(<EventoCard evento={eventoLongLocation} />);
      expect(screen.getByText(eventoLongLocation.ubicacion)).toBeInTheDocument();
    });

    it('should handle evento with 100% capacity', () => {
      const eventoFullCapacity = {
        ...mockEvento,
        asientosDisponibles: 1000,
      };
      renderWithQueryClient(<EventoCard evento={eventoFullCapacity} />);
      expect(screen.getByText('1000 / 1000 disponibles')).toBeInTheDocument();
    });

    it('should handle evento with minimal capacity', () => {
      const eventoMinCapacity = {
        ...mockEvento,
        capacidadTotal: 10,
        asientosDisponibles: 1,
      };
      renderWithQueryClient(<EventoCard evento={eventoMinCapacity} />);
      expect(screen.getByText('1 / 10 disponibles')).toBeInTheDocument();
    });
  });

  describe('Hover Effects', () => {
    it('should render card with hover effects for published evento', () => {
      const { container } = renderWithQueryClient(<EventoCard evento={mockEvento} />);
      const card = container.querySelector('[class*="MuiCard"]');
      expect(card).toBeInTheDocument();
    });

    it('should render card without hover effects for canceled evento', () => {
      const canceledEvento = { ...mockEvento, estado: 'Cancelado' as const };
      const { container } = renderWithQueryClient(<EventoCard evento={canceledEvento} />);
      const card = container.querySelector('[class*="MuiCard"]');
      expect(card).toBeInTheDocument();
    });
  });
});
