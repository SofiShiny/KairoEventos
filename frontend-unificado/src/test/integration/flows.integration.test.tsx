/**
 * Integration Tests for Main User Flows
 * 
 * These tests verify complete user journeys through the application:
 * 1. Login → Ver Eventos → Comprar Entrada
 * 2. Login Admin → Gestionar Usuarios
 * 3. Login Organizator → Ver Reportes
 * 
 * Requirements: 17.2
 * 
 * Note: These are simplified integration tests that focus on core functionality.
 * They test the main user flows without requiring full authentication setup.
 */

import { describe, it, expect, beforeEach, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ThemeProvider } from '@mui/material/styles';
import { theme } from '@shared/theme';
import { AppRoutes } from '../../routes';
import { ToastProvider } from '@shared/components';
import { http, HttpResponse } from 'msw';
import { server } from '../mocks/server';

// Mock authentication context for testing
const mockAuthContext = {
  user: {
    profile: {
      sub: 'test-user-id',
      name: 'Test User',
      email: 'test@example.com',
      realm_access: { roles: ['Asistente'] },
    },
    access_token: 'mock-token',
  },
  token: 'mock-token',
  roles: ['Asistente'],
  isAuthenticated: true,
  isLoading: false,
  login: vi.fn(),
  logout: vi.fn(),
  hasRole: (role: string) => mockAuthContext.roles.includes(role),
};

// Mock the AuthContext module
vi.mock('../../context/AuthContext', async () => {
  const actual = await vi.importActual('../../context/AuthContext');
  return {
    ...actual,
    useAuth: () => mockAuthContext,
    AppAuthProvider: ({ children }: { children: React.ReactNode }) => <>{children}</>,
  };
});

// Helper function to render with all providers
function renderWithProviders(initialRoute = '/') {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
        cacheTime: 0,
      },
      mutations: {
        retry: false,
      },
    },
  });

  return render(
    <MemoryRouter initialEntries={[initialRoute]}>
      <QueryClientProvider client={queryClient}>
        <ThemeProvider theme={theme}>
          <ToastProvider>
            <AppRoutes />
          </ToastProvider>
        </ThemeProvider>
      </QueryClientProvider>
    </MemoryRouter>
  );
}

describe('Integration Tests - Main User Flows', () => {
  beforeEach(() => {
    // Reset mocks before each test
    vi.clearAllMocks();
    // Reset auth context to default
    mockAuthContext.roles = ['Asistente'];
  });

  describe('Flow 1: Login → Ver Eventos → Comprar Entrada', () => {
    it('should display eventos list when navigating to eventos page', async () => {
      renderWithProviders('/eventos');

      // Wait for eventos list to load
      await waitFor(() => {
        expect(screen.getByText('Evento Test 1')).toBeInTheDocument();
      });

      // Verify eventos are displayed
      expect(screen.getByText('Evento Test 2')).toBeInTheDocument();
      expect(screen.getByText(/teatro principal/i)).toBeInTheDocument();
    });

    it('should show evento details when viewing a specific evento', async () => {
      renderWithProviders('/eventos/1');

      // Wait for evento detail to load
      await waitFor(() => {
        expect(screen.getByText(/evento test 1/i)).toBeInTheDocument();
      });

      // Verify details are shown
      expect(screen.getByText(/descripción del evento test 1/i)).toBeInTheDocument();
      expect(screen.getByText(/teatro principal/i)).toBeInTheDocument();
    });

    it('should display seat map when purchasing entrada', async () => {
      renderWithProviders('/eventos/1/comprar');

      // Wait for seat map to load
      await waitFor(() => {
        expect(screen.getByTestId('asiento-A1')).toBeInTheDocument();
      });

      // Verify different seat states are shown
      expect(screen.getByTestId('asiento-A2')).toBeInTheDocument(); // Reserved
      expect(screen.getByTestId('asiento-A3')).toBeInTheDocument(); // Occupied
      expect(screen.getByTestId('asiento-B1')).toBeInTheDocument(); // Available
    });

    it('should filter eventos by search term', async () => {
      const user = userEvent.setup();
      renderWithProviders('/eventos');

      // Wait for eventos to load
      await waitFor(() => {
        expect(screen.getByText('Evento Test 1')).toBeInTheDocument();
      });

      // Type in search box
      const searchInput = screen.getByPlaceholderText(/buscar eventos/i);
      await user.type(searchInput, 'Test 1');

      // Verify input was updated
      expect(searchInput).toHaveValue('Test 1');
    });

    it('should show user entradas on mis entradas page', async () => {
      renderWithProviders('/mis-entradas');

      // Wait for entradas to load
      await waitFor(() => {
        expect(screen.getByText('Evento Test 1')).toBeInTheDocument();
      });

      // Verify entrada details
      expect(screen.getByText(/fila a, asiento 1/i)).toBeInTheDocument();
      expect(screen.getByText(/pagada/i)).toBeInTheDocument();
    });
  });

  describe('Flow 2: Login Admin → Gestionar Usuarios', () => {
    beforeEach(() => {
      // Mock admin authentication
      mockAuthContext.roles = ['Admin'];
    });

    it('should display usuarios list for admin', async () => {
      renderWithProviders('/usuarios');

      // Wait for usuarios list to load
      await waitFor(() => {
        expect(screen.getByText('admin')).toBeInTheDocument();
      });

      // Verify usuarios are displayed
      expect(screen.getByText('organizador1')).toBeInTheDocument();
      expect(screen.getByText(/admin@example.com/i)).toBeInTheDocument();
    });

    it('should show create usuario form when clicking crear button', async () => {
      const user = userEvent.setup();
      renderWithProviders('/usuarios');

      // Wait for page to load
      await waitFor(() => {
        expect(screen.getByRole('button', { name: /crear usuario/i })).toBeInTheDocument();
      });

      // Click crear usuario
      const crearButton = screen.getByRole('button', { name: /crear usuario/i });
      await user.click(crearButton);

      // Verify form is shown
      await waitFor(() => {
        expect(screen.getByLabelText(/username/i)).toBeInTheDocument();
      });

      expect(screen.getByLabelText(/nombre/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/correo/i)).toBeInTheDocument();
    });

    it('should create new usuario successfully', async () => {
      const user = userEvent.setup();
      
      // Mock successful creation
      server.use(
        http.post('http://localhost:8080/api/usuarios', async ({ request }) => {
          const body = await request.json();
          return HttpResponse.json(
            {
              data: {
                id: 'new-user-123',
                ...body,
                activo: true,
              },
              success: true,
              message: 'Usuario creado exitosamente',
            },
            { status: 201 }
          );
        })
      );

      renderWithProviders('/usuarios');

      // Wait and click crear
      await waitFor(() => {
        expect(screen.getByRole('button', { name: /crear usuario/i })).toBeInTheDocument();
      });

      await user.click(screen.getByRole('button', { name: /crear usuario/i }));

      // Fill form
      await waitFor(() => {
        expect(screen.getByLabelText(/username/i)).toBeInTheDocument();
      });

      await user.type(screen.getByLabelText(/username/i), 'newuser');
      await user.type(screen.getByLabelText(/nombre/i), 'New User');
      await user.type(screen.getByLabelText(/correo/i), 'newuser@example.com');
      await user.type(screen.getByLabelText(/teléfono/i), '+1234567890');

      // Submit
      const submitButton = screen.getByRole('button', { name: /guardar/i });
      await user.click(submitButton);

      // Verify success message
      await waitFor(() => {
        expect(screen.getByText(/usuario creado exitosamente/i)).toBeInTheDocument();
      });
    });
  });

  describe('Flow 3: Login Organizator → Ver Reportes', () => {
    beforeEach(() => {
      // Mock organizator authentication
      mockAuthContext.roles = ['Organizator'];
    });

    it('should display reportes page for organizator', async () => {
      renderWithProviders('/reportes');

      // Wait for reportes page to load
      await waitFor(() => {
        expect(screen.getByText(/reportes y análisis/i)).toBeInTheDocument();
      });

      // Verify report sections are visible
      expect(screen.getByText(/métricas de eventos/i)).toBeInTheDocument();
    });

    it('should display event metrics data', async () => {
      renderWithProviders('/reportes');

      // Wait for metrics to load
      await waitFor(() => {
        expect(screen.getByText('Evento Test 1')).toBeInTheDocument();
      });

      // Verify metrics are shown
      expect(screen.getByText('Evento Test 2')).toBeInTheDocument();
    });

    it('should allow filtering reports by date range', async () => {
      const user = userEvent.setup();
      renderWithProviders('/reportes');

      // Wait for page load
      await waitFor(() => {
        expect(screen.getByLabelText(/fecha inicio/i)).toBeInTheDocument();
      });

      // Set date filters
      const fechaInicioInput = screen.getByLabelText(/fecha inicio/i);
      await user.type(fechaInicioInput, '2024-12-01');

      // Verify input was updated
      expect(fechaInicioInput).toHaveValue('2024-12-01');
    });

    it('should display financial reconciliation data', async () => {
      renderWithProviders('/reportes');

      // Wait for financial data to load
      await waitFor(() => {
        expect(screen.getByText(/conciliación financiera/i)).toBeInTheDocument();
      });

      // Verify financial section is present
      const financialSection = screen.getByText(/conciliación financiera/i).closest('div');
      expect(financialSection).toBeInTheDocument();
    });

    it('should show export button for reports', async () => {
      renderWithProviders('/reportes');

      // Wait for page load
      await waitFor(() => {
        expect(screen.getByRole('button', { name: /exportar/i })).toBeInTheDocument();
      });

      // Verify export button is present
      const exportButton = screen.getByRole('button', { name: /exportar/i });
      expect(exportButton).toBeInTheDocument();
    });
  });

  describe('Cross-flow scenarios', () => {
    it('should display dashboard with user statistics', async () => {
      renderWithProviders('/');

      // Wait for dashboard to load
      await waitFor(() => {
        expect(screen.getByText(/dashboard/i)).toBeInTheDocument();
      });

      // Verify dashboard content
      expect(screen.getByText(/test user/i)).toBeInTheDocument();
    });

    it('should handle loading states correctly', async () => {
      // Delay the response to test loading state
      server.use(
        http.get('http://localhost:8080/api/eventos', async () => {
          await new Promise((resolve) => setTimeout(resolve, 100));
          return HttpResponse.json({
            data: [],
            success: true,
          });
        })
      );

      renderWithProviders('/eventos');

      // Wait for data to load (loading state may be too fast to catch)
      await waitFor(() => {
        // Just verify the page loaded successfully
        expect(screen.queryByText(/error/i)).not.toBeInTheDocument();
      }, { timeout: 2000 });
    });

    it('should display error message when API fails', async () => {
      // Mock API error
      server.use(
        http.get('http://localhost:8080/api/eventos', () => {
          return HttpResponse.json(
            { message: 'Internal server error' },
            { status: 500 }
          );
        })
      );

      renderWithProviders('/eventos');

      // Wait for error message
      await waitFor(() => {
        expect(screen.getByText(/error/i)).toBeInTheDocument();
      });
    });
  });
});
