import { useEffect, useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { Typography, Button, Box, Alert, CircularProgress } from '@mui/material';
import { useAuth } from '../context/AuthContext';
import { AuthLayout } from '../layouts';
import LoginIcon from '@mui/icons-material/Login';
import EventIcon from '@mui/icons-material/Event';

/**
 * LoginPage - Authentication page
 * Redirects to Keycloak for authentication
 * 
 * Requirements:
 * - 5.1: Show login screen for unauthenticated users
 * - 5.2: Display "Login with Keycloak" button
 * - 5.3: Redirect to Keycloak on button click
 * - 5.4: Display application logo
 * - 5.5: Centered and attractive design
 * - 5.6: Handle authentication errors
 * - 5.7: Redirect to Dashboard after successful authentication
 * 
 * Accessibility Features:
 * - Proper heading hierarchy
 * - ARIA labels for interactive elements
 * - Alt text for images/icons
 * - Keyboard navigation support
 */
export function LoginPage() {
  const { isAuthenticated, login, isLoading } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [error, setError] = useState<string | null>(null);
  const [isLoggingIn, setIsLoggingIn] = useState(false);

  // Check for authentication errors in URL (from Keycloak redirect)
  useEffect(() => {
    const params = new URLSearchParams(location.search);
    const errorParam = params.get('error');
    const errorDescription = params.get('error_description');

    if (errorParam) {
      // Handle authentication errors from Keycloak
      let errorMessage = 'Error de autenticación. Por favor, intente nuevamente.';
      
      if (errorDescription) {
        errorMessage = errorDescription;
      } else if (errorParam === 'access_denied') {
        errorMessage = 'Acceso denegado. No tiene permisos para acceder a esta aplicación.';
      } else if (errorParam === 'invalid_request') {
        errorMessage = 'Solicitud inválida. Por favor, contacte al administrador.';
      } else if (errorParam === 'server_error') {
        errorMessage = 'Error del servidor de autenticación. Intente más tarde.';
      }

      setError(errorMessage);
      
      // Clean URL parameters
      window.history.replaceState({}, document.title, window.location.pathname);
    }
  }, [location.search]);

  // Redirect to dashboard if already authenticated (Requirement 5.7)
  useEffect(() => {
    if (isAuthenticated && !isLoading) {
      // Get the intended destination from location state, or default to dashboard
      const from = (location.state as any)?.from?.pathname || '/';
      navigate(from, { replace: true });
    }
  }, [isAuthenticated, isLoading, navigate, location.state]);

  // Handle login button click (Requirement 5.3)
  const handleLogin = async () => {
    try {
      setError(null);
      setIsLoggingIn(true);
      await login();
    } catch (err) {
      // Handle login errors (Requirement 5.6)
      setIsLoggingIn(false);
      setError('Error al iniciar sesión. Por favor, intente nuevamente.');
      console.error('Login error:', err);
    }
  };

  // Show loading state while checking authentication
  if (isLoading) {
    return (
      <AuthLayout>
        <CircularProgress size={60} aria-label="Loading authentication status" />
        <Typography variant="body1" sx={{ mt: 2 }}>
          Verificando autenticación...
        </Typography>
      </AuthLayout>
    );
  }

  return (
    <AuthLayout>
      {/* Application Logo (Requirement 5.4) */}
      <Box
        sx={{
          mb: 3,
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
        }}
        role="img"
        aria-label="Kairo application logo"
      >
        <EventIcon
          sx={{
            fontSize: 80,
            color: 'primary.main',
          }}
          aria-hidden="true"
        />
      </Box>

      {/* Application Title */}
      <Typography
        variant="h3"
        component="h1"
        gutterBottom
        color="primary"
        sx={{
          fontWeight: 700,
          textAlign: 'center',
        }}
      >
        Kairo
      </Typography>

      <Typography
        variant="h5"
        component="h2"
        gutterBottom
        sx={{
          mb: 1,
          textAlign: 'center',
          color: 'text.secondary',
        }}
      >
        Sistema de Gestión de Eventos
      </Typography>

      <Typography
        variant="body1"
        color="text.secondary"
        sx={{
          mb: 4,
          textAlign: 'center',
        }}
      >
        Inicie sesión para acceder a la aplicación
      </Typography>

      {/* Error Alert (Requirement 5.6) */}
      {error && (
        <Alert
          severity="error"
          sx={{ mb: 3, width: '100%' }}
          onClose={() => setError(null)}
          role="alert"
          aria-live="assertive"
        >
          {error}
        </Alert>
      )}

      {/* Login Button (Requirements 5.2, 5.3) */}
      <Button
        variant="contained"
        size="large"
        startIcon={isLoggingIn ? <CircularProgress size={20} color="inherit" aria-hidden="true" /> : <LoginIcon aria-hidden="true" />}
        onClick={handleLogin}
        disabled={isLoggingIn}
        fullWidth
        sx={{
          py: 1.5,
          fontSize: '1.1rem',
          fontWeight: 600,
          textTransform: 'none',
        }}
        aria-label={isLoggingIn ? 'Redirecting to Keycloak' : 'Login with Keycloak'}
      >
        {isLoggingIn ? 'Redirigiendo...' : 'Iniciar Sesión con Keycloak'}
      </Button>

      {/* Additional Info */}
      <Typography
        variant="caption"
        color="text.secondary"
        sx={{
          mt: 3,
          textAlign: 'center',
        }}
      >
        Será redirigido a Keycloak para autenticarse de forma segura
      </Typography>
    </AuthLayout>
  );
}
