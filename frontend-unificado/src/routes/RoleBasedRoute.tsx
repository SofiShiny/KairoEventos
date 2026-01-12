import { Navigate, Outlet } from 'react-router-dom';
import { Box, CircularProgress, Typography, Paper, Button } from '@mui/material';
import { useAuth } from '../context/AuthContext';
import { useNavigate } from 'react-router-dom';

interface RoleBasedRouteProps {
  children?: React.ReactNode;
  requiredRoles: string[];
}

/**
 * RoleBasedRoute component that requires specific roles
 * Shows 403 Forbidden if user doesn't have required roles
 * 
 * Can be used in two ways:
 * 1. As a wrapper: <RoleBasedRoute requiredRoles={['Admin']}><Component /></RoleBasedRoute>
 * 2. As a layout route: <Route element={<RoleBasedRoute requiredRoles={['Admin']} />}> with nested routes
 */
export function RoleBasedRoute({ children, requiredRoles }: RoleBasedRouteProps) {
  const { isAuthenticated, isLoading, hasRole } = useAuth();
  const navigate = useNavigate();

  // Show loading state while checking authentication
  if (isLoading) {
    return (
      <Box
        display="flex"
        justifyContent="center"
        alignItems="center"
        minHeight="100vh"
        flexDirection="column"
        gap={2}
      >
        <CircularProgress size={60} />
        <Typography variant="h6" color="text.secondary">
          Checking permissions...
        </Typography>
      </Box>
    );
  }

  // Redirect to login if not authenticated
  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  // Check if user has at least one of the required roles
  const hasRequiredRole = requiredRoles.some((role) => hasRole(role));

  if (!hasRequiredRole) {
    return (
      <Box
        display="flex"
        justifyContent="center"
        alignItems="center"
        minHeight="100vh"
        p={3}
      >
        <Paper elevation={3} sx={{ p: 4, maxWidth: 600, textAlign: 'center' }}>
          <Typography variant="h3" color="error" gutterBottom>
            403
          </Typography>
          <Typography variant="h5" gutterBottom>
            Access Forbidden
          </Typography>
          <Typography variant="body1" color="text.secondary" sx={{ mb: 3 }}>
            You don't have permission to access this page.
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
            Required roles: <strong>{requiredRoles.join(', ')}</strong>
          </Typography>
          <Button variant="contained" onClick={() => navigate('/')}>
            Go to Dashboard
          </Button>
        </Paper>
      </Box>
    );
  }

  // Render children if provided, otherwise render Outlet for nested routes
  return children ? <>{children}</> : <Outlet />;
}
