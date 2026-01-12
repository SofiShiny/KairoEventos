import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import EventEditDialog from './EventEditDialog';
import { useEvent } from '../api/events';
import { useEventSeats, useEventCategories } from '../api/seats';
import React from 'react';

// Mock the API hooks
vi.mock('../api/events', () => ({
  useEvent: vi.fn(),
}));

vi.mock('../api/seats', () => ({
  useEventSeats: vi.fn(),
  useEventCategories: vi.fn(),
  useCreateCategory: vi.fn(),
  useCreateSeat: vi.fn(),
}));

// Mock SeatManagementTab to simplify testing
vi.mock('./SeatManagementTab', () => ({
  default: ({ eventoId, mapaId }: { eventoId: string; mapaId: string }) =>
    React.createElement(
      'div',
      { 'data-testid': 'seat-management-tab' },
      React.createElement('span', { 'data-testid': 'evento-id' }, eventoId),
      React.createElement('span', { 'data-testid': 'mapa-id' }, mapaId)
    ),
}));

describe('EventEditDialog', () => {
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

  beforeEach(() => {
    vi.clearAllMocks();
    localStorage.clear();

    // Default mock implementations
    vi.mocked(useEvent).mockReturnValue({
      data: { id: 'event-1', nombre: 'Test Event' },
      isLoading: false,
    } as any);

    vi.mocked(useEventSeats).mockReturnValue({
      data: [],
      isLoading: false,
    } as any);

    vi.mocked(useEventCategories).mockReturnValue({
      data: [],
      isLoading: false,
    } as any);
  });

  /**
   * Test: Seat management tab renders when dialog is opened
   * Validates: Requirements 4.1
   */
  it('should render seat management tab when dialog is opened', async () => {
    const eventoId = 'event-123';
    const mapaId = 'map-456';
    
    // Set up localStorage with mapaId
    localStorage.setItem(`map_for_event_${eventoId}`, mapaId);

    render(
      <EventEditDialog open={true} onClose={vi.fn()} eventoId={eventoId} />,
      { wrapper: createWrapper() }
    );

    // Check that dialog is rendered
    expect(screen.getByRole('dialog')).toBeInTheDocument();
    
    // Check that tabs are present
    expect(screen.getByText('Detalles del Evento')).toBeInTheDocument();
    expect(screen.getByText('Gestión de Asientos')).toBeInTheDocument();
  });

  /**
   * Test: eventoId is correctly passed to SeatManagementTab
   * Validates: Requirements 4.2, 4.3
   */
  it('should pass eventoId correctly to SeatManagementTab', async () => {
    const eventoId = 'event-789';
    const mapaId = 'map-101';
    
    localStorage.setItem(`map_for_event_${eventoId}`, mapaId);

    render(
      <EventEditDialog open={true} onClose={vi.fn()} eventoId={eventoId} />,
      { wrapper: createWrapper() }
    );

    // Click on the seat management tab
    const seatManagementTab = screen.getByText('Gestión de Asientos');
    await userEvent.click(seatManagementTab);

    // Wait for the tab panel to be visible
    await waitFor(() => {
      expect(screen.getByTestId('seat-management-tab')).toBeInTheDocument();
    });

    // Verify that eventoId is passed correctly
    expect(screen.getByTestId('evento-id')).toHaveTextContent(eventoId);
    expect(screen.getByTestId('mapa-id')).toHaveTextContent(mapaId);
  });

  /**
   * Test: Navigation between tabs preserves event context
   * Validates: Requirements 4.2, 4.3
   */
  it('should preserve event context when navigating between tabs', async () => {
    const eventoId = 'event-preserve-123';
    const mapaId = 'map-preserve-456';
    
    localStorage.setItem(`map_for_event_${eventoId}`, mapaId);

    render(
      <EventEditDialog open={true} onClose={vi.fn()} eventoId={eventoId} />,
      { wrapper: createWrapper() }
    );

    // Initially on first tab (Detalles del Evento)
    expect(screen.getByText('Detalles del evento (por implementar)')).toBeInTheDocument();

    // Navigate to seat management tab
    const seatManagementTab = screen.getByText('Gestión de Asientos');
    await userEvent.click(seatManagementTab);

    // Verify seat management tab is shown with correct context
    await waitFor(() => {
      expect(screen.getByTestId('seat-management-tab')).toBeInTheDocument();
      expect(screen.getByTestId('evento-id')).toHaveTextContent(eventoId);
    });

    // Navigate back to details tab
    const detailsTab = screen.getByText('Detalles del Evento');
    await userEvent.click(detailsTab);

    // Verify we're back on details tab
    await waitFor(() => {
      expect(screen.getByText('Detalles del evento (por implementar)')).toBeInTheDocument();
    });

    // Navigate to seat management tab again
    await userEvent.click(seatManagementTab);

    // Verify event context is still preserved
    await waitFor(() => {
      expect(screen.getByTestId('evento-id')).toHaveTextContent(eventoId);
      expect(screen.getByTestId('mapa-id')).toHaveTextContent(mapaId);
    });
  });

  /**
   * Test: Dialog displays event name from useEvent hook
   * Validates: Requirements 4.1
   */
  it('should display event name in dialog title', async () => {
    const eventoId = 'event-name-test';
    const eventName = 'Mi Evento de Prueba';
    
    vi.mocked(useEvent).mockReturnValue({
      data: { id: eventoId, nombre: eventName },
      isLoading: false,
    } as any);

    render(
      <EventEditDialog open={true} onClose={vi.fn()} eventoId={eventoId} />,
      { wrapper: createWrapper() }
    );

    expect(screen.getByText(`Editar Evento: ${eventName}`)).toBeInTheDocument();
  });

  /**
   * Test: Dialog shows message when mapaId is not found
   * Validates: Requirements 4.1
   */
  it('should show message when mapaId is not found in localStorage', async () => {
    const eventoId = 'event-no-map';
    
    // Don't set mapaId in localStorage

    render(
      <EventEditDialog open={true} onClose={vi.fn()} eventoId={eventoId} />,
      { wrapper: createWrapper() }
    );

    // Navigate to seat management tab
    const seatManagementTab = screen.getByText('Gestión de Asientos');
    await userEvent.click(seatManagementTab);

    // Should show error message
    await waitFor(() => {
      expect(screen.getByText('No se encontró el mapa de asientos para este evento.')).toBeInTheDocument();
    });
  });

  /**
   * Test: Close button calls onClose callback
   * Validates: Requirements 4.1
   */
  it('should call onClose when close button is clicked', async () => {
    const onClose = vi.fn();
    const eventoId = 'event-close-test';
    
    render(
      <EventEditDialog open={true} onClose={onClose} eventoId={eventoId} />,
      { wrapper: createWrapper() }
    );

    const closeButton = screen.getByLabelText('close');
    await userEvent.click(closeButton);

    expect(onClose).toHaveBeenCalledTimes(1);
  });
});
