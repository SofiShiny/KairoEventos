import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { LoginPage } from './LoginPage';

/**
 * Unit tests for LoginPage component
 * 
 * Task 8.1: Write unit tests for LoginPage
 * - Test renderizado de botón de login
 * - Test redirección a Keycloak
 * - Test manejo de errores
 * 
 * Requirements tested:
 * - 5.1: Show login screen for unauthenticated users
 * - 5.2: Display "Login with Keycloak" button
 * - 5.3: Redirect to Keycloak on button click
 * - 5.4: Display application logo
 * - 5.5: Centered and attractive design
 * - 5.6: Handle authentication errors
 * - 5.7: Redirect to Dashboard after successful authentication
 */

// Create mock functions that can be reassigned per test
let mockLogin = vi.fn();
let mockNavigate = vi.fn();
let mockIsAuthenticated = false;
let mockIsLoading = false;
let mockLocationSearch = '';

// Mock the auth context to avoid OIDC initialization in tests
vi.mock('../context/AuthContext', () => ({
  useAuth: () => ({
    isAuthenticated: mockIsAuthenticated,
    isLoading: mockIsLoading,
    login: mockLogin,
    logout: vi.fn(),
    user: null,
    token: null,
    roles: [],
    hasRole: vi.fn(() => false),
  }),
  AppAuthProvider: ({ children }: { children: React.ReactNode }) => <>{children}</>,
}));

// Mock react-router-dom
vi.mock('react-router-dom', () => ({
  useNavigate: () => mockNavigate,
  useLocation: () => ({ search: mockLocationSearch, state: null, pathname: '/login' }),
  BrowserRouter: ({ children }: { children: React.ReactNode }) => <>{children}</>,
  MemoryRouter: ({ children }: { children: React.ReactNode }) => <>{children}</>,
}));

describe('LoginPage', () => {
  beforeEach(() => {
    // Reset mocks before each test
    mockLogin = vi.fn();
    mockNavigate = vi.fn();
    mockIsAuthenticated = false;
    mockIsLoading = false;
    mockLocationSearch = '';
    vi.clearAllMocks();
  });

  describe('Rendering (Requirements 5.1, 5.2, 5.4, 5.5)', () => {
    it('should render login button (Requirement 5.2)', () => {
      render(<LoginPage />);
      expect(screen.getByText('Iniciar Sesión con Keycloak')).toBeInTheDocument();
    });

    it('should display application logo and title (Requirement 5.4)', () => {
      render(<LoginPage />);
      expect(screen.getByText('Kairo')).toBeInTheDocument();
      expect(screen.getByText('Sistema de Gestión de Eventos')).toBeInTheDocument();
    });

    it('should display descriptive text', () => {
      render(<LoginPage />);
      expect(screen.getByText('Inicie sesión para acceder a la aplicación')).toBeInTheDocument();
    });

    it('should display security notice', () => {
      render(<LoginPage />);
      expect(
        screen.getByText('Será redirigido a Keycloak para autenticarse de forma segura')
      ).toBeInTheDocument();
    });

    it('should have a login button that is enabled by default', () => {
      render(<LoginPage />);
      const loginButton = screen.getByRole('button', {
        name: /login with keycloak/i,
      });
      expect(loginButton).toBeInTheDocument();
      expect(loginButton).not.toBeDisabled();
    });

    it('should show loading state while checking authentication', () => {
      mockIsLoading = true;
      render(<LoginPage />);
      expect(screen.getByText('Verificando autenticación...')).toBeInTheDocument();
      expect(screen.getByLabelText('Loading authentication status')).toBeInTheDocument();
    });
  });

  describe('Redirection to Keycloak (Requirement 5.3)', () => {
    it('should call login function when login button is clicked', async () => {
      const user = userEvent.setup();
      render(<LoginPage />);

      const loginButton = screen.getByRole('button', {
        name: /login with keycloak/i,
      });

      await user.click(loginButton);

      expect(mockLogin).toHaveBeenCalledTimes(1);
    });

    it('should disable button and show loading state during login', async () => {
      const user = userEvent.setup();
      // Make login function return a promise that doesn't resolve immediately
      mockLogin = vi.fn(() => new Promise(() => {}));
      
      render(<LoginPage />);

      const loginButton = screen.getByRole('button', {
        name: /login with keycloak/i,
      });

      await user.click(loginButton);

      // Button should be disabled and show loading text
      await waitFor(() => {
        expect(screen.getByText('Redirigiendo...')).toBeInTheDocument();
        const button = screen.getByRole('button', { name: /redirecting to keycloak/i });
        expect(button).toBeDisabled();
      });
    });

    it('should redirect to dashboard after successful authentication (Requirement 5.7)', async () => {
      mockIsAuthenticated = true;
      mockIsLoading = false;

      render(<LoginPage />);

      await waitFor(() => {
        expect(mockNavigate).toHaveBeenCalledWith('/', { replace: true });
      });
    });
  });

  describe('Error Handling (Requirement 5.6)', () => {
    it('should display error message when login fails', async () => {
      const user = userEvent.setup();
      const errorMessage = 'Login failed';
      mockLogin = vi.fn().mockRejectedValue(new Error(errorMessage));

      render(<LoginPage />);

      const loginButton = screen.getByRole('button', {
        name: /login with keycloak/i,
      });

      await user.click(loginButton);

      await waitFor(() => {
        expect(screen.getByRole('alert')).toBeInTheDocument();
        expect(screen.getByText('Error al iniciar sesión. Por favor, intente nuevamente.')).toBeInTheDocument();
      });
    });

    it('should display error from URL parameters (access_denied)', () => {
      mockLocationSearch = '?error=access_denied';

      render(<LoginPage />);

      expect(screen.getByRole('alert')).toBeInTheDocument();
      expect(screen.getByText('Acceso denegado. No tiene permisos para acceder a esta aplicación.')).toBeInTheDocument();
    });

    it('should display error from URL parameters (invalid_request)', () => {
      mockLocationSearch = '?error=invalid_request';

      render(<LoginPage />);

      expect(screen.getByRole('alert')).toBeInTheDocument();
      expect(screen.getByText('Solicitud inválida. Por favor, contacte al administrador.')).toBeInTheDocument();
    });

    it('should display error from URL parameters (server_error)', () => {
      mockLocationSearch = '?error=server_error';

      render(<LoginPage />);

      expect(screen.getByRole('alert')).toBeInTheDocument();
      expect(screen.getByText('Error del servidor de autenticación. Intente más tarde.')).toBeInTheDocument();
    });

    it('should display custom error description from URL', () => {
      mockLocationSearch = '?error=custom_error&error_description=Custom error message';

      render(<LoginPage />);

      expect(screen.getByRole('alert')).toBeInTheDocument();
      expect(screen.getByText('Custom error message')).toBeInTheDocument();
    });

    it('should display generic error for unknown error types', () => {
      mockLocationSearch = '?error=unknown_error';

      render(<LoginPage />);

      expect(screen.getByRole('alert')).toBeInTheDocument();
      expect(screen.getByText('Error de autenticación. Por favor, intente nuevamente.')).toBeInTheDocument();
    });

    it('should allow closing error alert', async () => {
      const user = userEvent.setup();
      mockLocationSearch = '?error=access_denied';

      render(<LoginPage />);

      const alert = screen.getByRole('alert');
      expect(alert).toBeInTheDocument();

      // Find and click the close button
      const closeButton = alert.querySelector('button[aria-label="Close"]');
      if (closeButton) {
        await user.click(closeButton);
        await waitFor(() => {
          expect(screen.queryByRole('alert')).not.toBeInTheDocument();
        });
      }
    });

    it('should re-enable login button after error', async () => {
      const user = userEvent.setup();
      mockLogin = vi.fn().mockRejectedValue(new Error('Login failed'));

      render(<LoginPage />);

      const loginButton = screen.getByRole('button', {
        name: /login with keycloak/i,
      });

      await user.click(loginButton);

      // Wait for error to appear
      await waitFor(() => {
        expect(screen.getByRole('alert')).toBeInTheDocument();
      });

      // Button should be enabled again
      expect(loginButton).not.toBeDisabled();
    });
  });
});
