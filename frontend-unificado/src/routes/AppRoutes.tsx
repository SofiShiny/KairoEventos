import { Routes, Route } from 'react-router-dom';
import { lazy, Suspense } from 'react';
import { Box, CircularProgress, Typography } from '@mui/material';
import { ProtectedRoute } from './ProtectedRoute';
import { RoleBasedRoute } from './RoleBasedRoute';
import { MainLayout } from '../layouts';
import { LoginPage, DashboardPage, NotFoundPage } from '../pages';

// Lazy load non-critical routes for better performance
const EventosPage = lazy(() =>
  import('../modules/eventos/pages').then((m) => ({ default: m.EventosPage }))
);
const EventoDetailPage = lazy(() =>
  import('../modules/eventos/pages').then((m) => ({ default: m.EventoDetailPage }))
);
const MisEntradasPage = lazy(() =>
  import('../modules/entradas/pages').then((m) => ({ default: m.MisEntradasPage }))
);
const ComprarEntradaPage = lazy(() =>
  import('../modules/entradas/pages').then((m) => ({ default: m.ComprarEntradaPage }))
);
const UsuariosPage = lazy(() =>
  import('../modules/usuarios/pages').then((m) => ({ default: m.UsuariosPage }))
);
const ReportesPage = lazy(() =>
  import('../modules/reportes/pages').then((m) => ({ default: m.ReportesPage }))
);

/**
 * Loading fallback component for lazy-loaded routes
 */
function LoadingFallback() {
  return (
    <Box
      display="flex"
      justifyContent="center"
      alignItems="center"
      minHeight="50vh"
      flexDirection="column"
      gap={2}
    >
      <CircularProgress size={60} />
      <Typography variant="h6" color="text.secondary">
        Loading...
      </Typography>
    </Box>
  );
}

/**
 * AppRoutes - Main routing configuration
 * 
 * Route structure:
 * - /login - Public login page
 * - / - Protected routes with MainLayout
 *   - / - Dashboard (all authenticated users)
 *   - /eventos - Events list (all authenticated users)
 *   - /eventos/:id - Event details (all authenticated users)
 *   - /mis-entradas - My tickets (all authenticated users)
 *   - /comprar/:eventoId - Purchase ticket (all authenticated users)
 *   - /usuarios - User management (Admin only)
 *   - /reportes - Reports (Admin and Organizator only)
 * - * - 404 Not Found
 */
export function AppRoutes() {
  return (
    <Suspense fallback={<LoadingFallback />}>
      <Routes>
        {/* Public routes */}
        <Route path="/login" element={<LoginPage />} />

        {/* Protected routes with MainLayout */}
        <Route element={<ProtectedRoute />}>
          <Route element={<MainLayout />}>
            {/* Dashboard */}
            <Route index element={<DashboardPage />} />

            {/* Events module - accessible to all authenticated users */}
            <Route path="eventos">
              <Route index element={<EventosPage />} />
              <Route path=":id" element={<EventoDetailPage />} />
            </Route>

            {/* Tickets module - accessible to all authenticated users */}
            <Route path="mis-entradas" element={<MisEntradasPage />} />
            <Route path="comprar/:eventoId" element={<ComprarEntradaPage />} />

            {/* Users module - Admin only */}
            <Route element={<RoleBasedRoute requiredRoles={['Admin']} />}>
              <Route path="usuarios" element={<UsuariosPage />} />
            </Route>

            {/* Reports module - Admin and Organizator only */}
            <Route element={<RoleBasedRoute requiredRoles={['Admin', 'Organizator']} />}>
              <Route path="reportes" element={<ReportesPage />} />
            </Route>
          </Route>
        </Route>

        {/* 404 Not Found */}
        <Route path="*" element={<NotFoundPage />} />
      </Routes>
    </Suspense>
  );
}
