/**
 * HistorialAsistencia Component
 * Displays attendance summary for an event
 */

import {
  Box,
  Paper,
  Typography,
  CircularProgress,
  Grid,
  Card,
  CardContent,
  LinearProgress,
} from '@mui/material';
import {
  PieChart,
  Pie,
  Cell,
  ResponsiveContainer,
  Legend,
  Tooltip,
} from 'recharts';
import type { AsistenciaEvento } from '../types';
import { ErrorMessage } from '../../../shared/components/ErrorMessage';
import { EmptyState } from '../../../shared/components/EmptyState';
import PeopleIcon from '@mui/icons-material/People';
import EventSeatIcon from '@mui/icons-material/EventSeat';
import TrendingUpIcon from '@mui/icons-material/TrendingUp';

interface HistorialAsistenciaProps {
  asistencia: AsistenciaEvento | null;
  isLoading: boolean;
  error: Error | null;
}

const COLORS = ['#0088FE', '#00C49F', '#FFBB28'];

/**
 * HistorialAsistencia - Display attendance summary
 * Shows aggregate attendance data for an event
 */
export function HistorialAsistencia({
  asistencia,
  isLoading,
  error,
}: HistorialAsistenciaProps) {
  if (isLoading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" py={8}>
        <CircularProgress />
      </Box>
    );
  }

  if (error) {
    return <ErrorMessage error={error} />;
  }

  if (!asistencia) {
    return (
      <EmptyState
        icon={<PeopleIcon />}
        title="No hay datos de asistencia"
        description="Seleccione un evento para ver su información de asistencia"
      />
    );
  }

  // Prepare data for pie chart
  const chartData = [
    { name: 'Reservados', value: asistencia.asientosReservados },
    { name: 'Disponibles', value: asistencia.asientosDisponibles },
  ];

  return (
    <Box>
      {/* Summary Cards */}
      <Grid container spacing={3} mb={4}>
        <Grid size={{ xs: 12, md: 4 }}>
          <Card>
            <CardContent>
              <Box display="flex" alignItems="center" mb={1}>
                <PeopleIcon color="primary" sx={{ mr: 1 }} />
                <Typography variant="body2" color="text.secondary">
                  Total Asistentes
                </Typography>
              </Box>
              <Typography variant="h4">
                {asistencia.totalAsistentes}
              </Typography>
              <Typography variant="caption" color="text.secondary">
                Registrados en el evento
              </Typography>
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, md: 4 }}>
          <Card>
            <CardContent>
              <Box display="flex" alignItems="center" mb={1}>
                <EventSeatIcon color="success" sx={{ mr: 1 }} />
                <Typography variant="body2" color="text.secondary">
                  Asientos Reservados
                </Typography>
              </Box>
              <Typography variant="h4">
                {asistencia.asientosReservados}
              </Typography>
              <Typography variant="caption" color="text.secondary">
                De {asistencia.capacidadTotal} totales
              </Typography>
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, md: 4 }}>
          <Card>
            <CardContent>
              <Box display="flex" alignItems="center" mb={1}>
                <TrendingUpIcon color="warning" sx={{ mr: 1 }} />
                <Typography variant="body2" color="text.secondary">
                  Ocupación
                </Typography>
              </Box>
              <Typography variant="h4">
                {asistencia.porcentajeOcupacion.toFixed(1)}%
              </Typography>
              <Typography variant="caption" color="text.secondary">
                Porcentaje de aforo
              </Typography>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Occupancy Progress */}
      <Paper sx={{ p: 3, mb: 3 }}>
        <Typography variant="h6" gutterBottom>
          Estado de Ocupación
        </Typography>
        <Box sx={{ mt: 2 }}>
          <Box display="flex" justifyContent="space-between" mb={1}>
            <Typography variant="body2" color="text.secondary">
              Capacidad utilizada
            </Typography>
            <Typography variant="body2" fontWeight="bold">
              {asistencia.asientosReservados} / {asistencia.capacidadTotal}
            </Typography>
          </Box>
          <LinearProgress
            variant="determinate"
            value={asistencia.porcentajeOcupacion}
            sx={{ height: 10, borderRadius: 5 }}
          />
          <Box display="flex" justifyContent="space-between" mt={1}>
            <Typography variant="caption" color="text.secondary">
              {asistencia.asientosDisponibles} asientos disponibles
            </Typography>
            <Typography variant="caption" color="text.secondary">
              Actualizado: {new Date(asistencia.ultimaActualizacion).toLocaleString('es-ES')}
            </Typography>
          </Box>
        </Box>
      </Paper>

      {/* Distribution Chart */}
      <Paper sx={{ p: 3 }}>
        <Typography variant="h6" gutterBottom>
          Distribución de Asientos
        </Typography>
        <ResponsiveContainer width="100%" height={300}>
          <PieChart>
            <Pie
              data={chartData}
              cx="50%"
              cy="50%"
              labelLine={false}
              label={({ name, value, percent = 0 }) =>
                `${name}: ${value} (${(percent * 100).toFixed(0)}%)`
              }
              outerRadius={100}
              fill="#8884d8"
              dataKey="value"
            >
              {chartData.map((_entry, index) => (
                <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
              ))}
            </Pie>
            <Tooltip />
            <Legend />
          </PieChart>
        </ResponsiveContainer>

        {/* Event Info */}
        <Box sx={{ mt: 3, pt: 2, borderTop: 1, borderColor: 'divider' }}>
          <Typography variant="subtitle2" gutterBottom>
            {asistencia.tituloEvento}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            ID del Evento: {asistencia.eventoId}
          </Typography>
        </Box>
      </Paper>
    </Box>
  );
}
