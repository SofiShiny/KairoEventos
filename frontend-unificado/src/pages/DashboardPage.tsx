/**
 * DashboardPage - Main dashboard with statistics, featured events, and quick navigation
 * 
 * Features:
 * - Statistics cards showing key metrics
 * - Featured events list
 * - Quick navigation to main modules
 * - Role-based personalization (Admin, Organizator, Asistente)
 */

import {
  Container,
  Typography,
  Box,
  Grid,
  Paper,
  Skeleton,
  Alert,
} from '@mui/material';
import { useAuth } from '../context/AuthContext';
import EventIcon from '@mui/icons-material/Event';
import ConfirmationNumberIcon from '@mui/icons-material/ConfirmationNumber';
import PeopleIcon from '@mui/icons-material/People';
import CalendarTodayIcon from '@mui/icons-material/CalendarToday';
import {
  StatCard,
  EventoCard,
  QuickActionCard,
  EmptyState,
  ErrorMessage,
} from '../shared/components';
import {
  useDashboardStats,
  useEventosDestacados,
} from '../shared/hooks';
import type { QuickAction } from '../shared/types/dashboard';

/**
 * DashboardPage Component
 */
export function DashboardPage() {
  const { user, hasRole } = useAuth();
  
  // Fetch dashboard data
  const {
    data: stats,
    isLoading: isLoadingStats,
    error: statsError,
    refetch: refetchStats,
  } = useDashboardStats();

  const {
    data: eventosDestacados,
    isLoading: isLoadingEventos,
    error: eventosError,
    refetch: refetchEventos,
  } = useEventosDestacados();

  // Define quick actions based on user role
  const getQuickActions = (): QuickAction[] => {
    const actions: QuickAction[] = [
      {
        label: 'Explorar Eventos',
        path: '/eventos',
        icon: 'event',
        description: 'Ver todos los eventos disponibles',
      },
      {
        label: 'Mis Entradas',
        path: '/mis-entradas',
        icon: 'ticket',
        description: 'Gestionar mis entradas compradas',
      },
    ];

    if (hasRole('Admin')) {
      actions.push({
        label: 'Gestionar Usuarios',
        path: '/usuarios',
        icon: 'people',
        description: 'Administrar usuarios del sistema',
        requiredRoles: ['Admin'],
      });
      actions.push({
        label: 'Ver Reportes',
        path: '/reportes',
        icon: 'assessment',
        description: 'Analizar métricas y reportes',
        requiredRoles: ['Admin', 'Organizator'],
      });
    } else if (hasRole('Organizator')) {
      actions.push({
        label: 'Ver Reportes',
        path: '/reportes',
        icon: 'assessment',
        description: 'Analizar métricas de mis eventos',
        requiredRoles: ['Admin', 'Organizator'],
      });
    }

    return actions;
  };

  const quickActions = getQuickActions();

  // Get personalized greeting
  const getGreeting = () => {
    const hour = new Date().getHours();
    if (hour < 12) return 'Buenos días';
    if (hour < 18) return 'Buenas tardes';
    return 'Buenas noches';
  };

  // Get role display name
  const getRoleDisplay = () => {
    if (hasRole('Admin')) return 'Administrador';
    if (hasRole('Organizator')) return 'Organizador';
    return 'Asistente';
  };

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      {/* Header */}
      <Box sx={{ mb: 4 }}>
        <Typography variant="h3" component="h1" gutterBottom>
          {getGreeting()}, {user?.profile?.name || user?.profile?.preferred_username}
        </Typography>
        <Typography variant="body1" color="text.secondary">
          Rol: {getRoleDisplay()} • Bienvenido a tu panel de control
        </Typography>
      </Box>

      {/* Statistics Cards */}
      <Box sx={{ mb: 4 }}>
        <Typography variant="h5" component="h2" gutterBottom sx={{ mb: 2 }}>
          Estadísticas
        </Typography>
        
        {statsError && (
          <Alert severity="error" sx={{ mb: 2 }}>
            <ErrorMessage
              error={statsError}
              onRetry={refetchStats}
            />
          </Alert>
        )}

        <Grid container spacing={3}>
          <Grid size={{ xs: 12, sm: 6, md: hasRole('Admin') ? 3 : 4 }}>
            <StatCard
              icon={EventIcon}
              value={stats?.totalEventos ?? '-'}
              label="Total de Eventos"
              color="primary"
              isLoading={isLoadingStats}
            />
          </Grid>
          
          <Grid size={{ xs: 12, sm: 6, md: hasRole('Admin') ? 3 : 4 }}>
            <StatCard
              icon={ConfirmationNumberIcon}
              value={stats?.misEntradas ?? '-'}
              label="Mis Entradas"
              color="secondary"
              isLoading={isLoadingStats}
            />
          </Grid>
          
          <Grid size={{ xs: 12, sm: 6, md: hasRole('Admin') ? 3 : 4 }}>
            <StatCard
              icon={CalendarTodayIcon}
              value={stats?.proximosEventos ?? '-'}
              label="Próximos Eventos"
              color="info"
              isLoading={isLoadingStats}
            />
          </Grid>
          
          {hasRole('Admin') && (
            <Grid size={{ xs: 12, sm: 6, md: 3 }}>
              <StatCard
                icon={PeopleIcon}
                value={stats?.totalUsuarios ?? '-'}
                label="Total de Usuarios"
                color="success"
                isLoading={isLoadingStats}
              />
            </Grid>
          )}
          
          {hasRole('Organizator') && !hasRole('Admin') && (
            <Grid size={{ xs: 12, sm: 6, md: 4 }}>
              <StatCard
                icon={EventIcon}
                value={stats?.eventosCreados ?? '-'}
                label="Eventos Creados"
                color="warning"
                isLoading={isLoadingStats}
              />
            </Grid>
          )}
        </Grid>
      </Box>

      {/* Featured Events */}
      <Box sx={{ mb: 4 }}>
        <Typography variant="h5" component="h2" gutterBottom sx={{ mb: 2 }}>
          Eventos Destacados
        </Typography>

        {eventosError && (
          <Alert severity="error" sx={{ mb: 2 }}>
            <ErrorMessage
              error={eventosError}
              onRetry={refetchEventos}
            />
          </Alert>
        )}

        {isLoadingEventos ? (
          <Grid container spacing={3}>
            {[1, 2, 3].map((i) => (
              <Grid size={{ xs: 12, sm: 6, md: 4 }} key={i}>
                <Paper sx={{ p: 2 }}>
                  <Skeleton variant="rectangular" height={200} sx={{ mb: 2 }} />
                  <Skeleton variant="text" height={32} sx={{ mb: 1 }} />
                  <Skeleton variant="text" height={20} sx={{ mb: 1 }} />
                  <Skeleton variant="text" height={20} />
                </Paper>
              </Grid>
            ))}
          </Grid>
        ) : eventosDestacados && eventosDestacados.length > 0 ? (
          <Grid container spacing={3}>
            {eventosDestacados.map((evento) => (
              <Grid size={{ xs: 12, sm: 6, md: 4 }} key={evento.id}>
                <EventoCard evento={evento} />
              </Grid>
            ))}
          </Grid>
        ) : (
          <EmptyState
            icon={<EventIcon sx={{ fontSize: 64 }} />}
            title="No hay eventos destacados"
            description="Actualmente no hay eventos destacados disponibles. Vuelve más tarde para ver nuevos eventos."
          />
        )}
      </Box>

      {/* Quick Actions */}
      <Box>
        <Typography variant="h5" component="h2" gutterBottom sx={{ mb: 2 }}>
          Acceso Rápido
        </Typography>
        <Grid container spacing={3}>
          {quickActions.map((action) => (
            <Grid size={{ xs: 12, sm: 6, md: quickActions.length === 4 ? 3 : 4 }} key={action.path}>
              <QuickActionCard
                label={action.label}
                path={action.path}
                icon={action.icon}
                description={action.description}
              />
            </Grid>
          ))}
        </Grid>
      </Box>
    </Container>
  );
}

