/**
 * MetricasEventos Component
 * Displays event metrics with visual charts
 */

import {
  Box,
  Paper,
  Typography,
  Grid,
  Card,
  CardContent,
  CircularProgress,
} from '@mui/material';
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
  PieChart,
  Pie,
  Cell,
} from 'recharts';
import type { VentaPorEvento } from '../types';
import { ErrorMessage } from '../../../shared/components/ErrorMessage';
import { EmptyState } from '../../../shared/components/EmptyState';
import EventIcon from '@mui/icons-material/Event';

interface MetricasEventosProps {
  metricas: VentaPorEvento[];
  isLoading: boolean;
  error: Error | null;
}

const COLORS = ['#0088FE', '#00C49F', '#FFBB28', '#FF8042', '#8884D8'];

/**
 * MetricasEventos - Visual display of event metrics
 * Shows bar charts for sales and pie charts for revenue distribution
 */
export function MetricasEventos({
  metricas,
  isLoading,
  error,
}: MetricasEventosProps) {
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

  if (!metricas || metricas.length === 0) {
    return (
      <EmptyState
        icon={<EventIcon />}
        title="No hay métricas disponibles"
        description="No se encontraron métricas para los filtros seleccionados"
      />
    );
  }

  // Calculate totals
  const totalReservas = metricas.reduce((sum, m) => sum + m.cantidadReservas, 0);
  const totalIngresos = metricas.reduce((sum, m) => sum + m.totalIngresos, 0);

  // Prepare data for charts
  const ventasData = metricas.map((m) => ({
    nombre: m.tituloEvento.length > 20 
      ? m.tituloEvento.substring(0, 20) + '...' 
      : m.tituloEvento,
    reservas: m.cantidadReservas,
    ingresos: m.totalIngresos,
  }));

  const ingresosData = metricas.map((m, index) => ({
    name: m.tituloEvento.length > 15 
      ? m.tituloEvento.substring(0, 15) + '...' 
      : m.tituloEvento,
    value: m.totalIngresos,
    fill: COLORS[index % COLORS.length],
  }));

  return (
    <Box>
      {/* Summary Cards */}
      <Grid container spacing={3} sx={{ mb: 4 }}>
        <Grid size={{ xs: 12, sm: 6, md: 4 }}>
          <Card>
            <CardContent>
              <Typography color="text.secondary" gutterBottom variant="body2">
                Total Eventos
              </Typography>
              <Typography variant="h4">{metricas.length}</Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 4 }}>
          <Card>
            <CardContent>
              <Typography color="text.secondary" gutterBottom variant="body2">
                Total Reservas
              </Typography>
              <Typography variant="h4">{totalReservas}</Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 4 }}>
          <Card>
            <CardContent>
              <Typography color="text.secondary" gutterBottom variant="body2">
                Ingresos Totales
              </Typography>
              <Typography variant="h4">
                ${totalIngresos.toLocaleString()}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Charts */}
      <Grid container spacing={3}>
        {/* Bar Chart - Ventas por Evento */}
        <Grid size={{ xs: 12, lg: 8 }}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" gutterBottom>
              Reservas e Ingresos por Evento
            </Typography>
            <ResponsiveContainer width="100%" height={300}>
              <BarChart data={ventasData}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="nombre" />
                <YAxis />
                <Tooltip />
                <Legend />
                <Bar dataKey="reservas" fill="#0088FE" name="Reservas" />
                <Bar dataKey="ingresos" fill="#00C49F" name="Ingresos ($)" />
              </BarChart>
            </ResponsiveContainer>
          </Paper>
        </Grid>

        {/* Pie Chart - Distribución de Ingresos */}
        <Grid size={{ xs: 12, lg: 4 }}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" gutterBottom>
              Distribución de Ingresos
            </Typography>
            <ResponsiveContainer width="100%" height={300}>
              <PieChart>
                <Pie
                  data={ingresosData}
                  cx="50%"
                  cy="50%"
                  labelLine={false}
                  label={(entry) => `$${entry.value.toLocaleString()}`}
                  outerRadius={80}
                  fill="#8884d8"
                  dataKey="value"
                >
                  {ingresosData.map((entry, index) => (
                    <Cell key={`cell-${index}`} fill={entry.fill} />
                  ))}
                </Pie>
                <Tooltip />
              </PieChart>
            </ResponsiveContainer>
          </Paper>
        </Grid>

        {/* Detailed Table */}
        <Grid size={{ xs: 12 }}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" gutterBottom>
              Detalle por Evento
            </Typography>
            <Box sx={{ overflowX: 'auto' }}>
              <table style={{ width: '100%', borderCollapse: 'collapse' }}>
                <thead>
                  <tr style={{ borderBottom: '2px solid #e0e0e0' }}>
                    <th style={{ padding: '12px', textAlign: 'left' }}>
                      Evento
                    </th>
                    <th style={{ padding: '12px', textAlign: 'right' }}>
                      Reservas
                    </th>
                    <th style={{ padding: '12px', textAlign: 'right' }}>
                      Ingresos
                    </th>
                    <th style={{ padding: '12px', textAlign: 'right' }}>
                      Promedio/Reserva
                    </th>
                  </tr>
                </thead>
                <tbody>
                  {metricas.map((metrica, index) => (
                    <tr
                      key={metrica.eventoId}
                      style={{
                        borderBottom: '1px solid #e0e0e0',
                        backgroundColor:
                          index % 2 === 0 ? '#fafafa' : 'transparent',
                      }}
                    >
                      <td style={{ padding: '12px' }}>{metrica.tituloEvento}</td>
                      <td style={{ padding: '12px', textAlign: 'right' }}>
                        {metrica.cantidadReservas}
                      </td>
                      <td style={{ padding: '12px', textAlign: 'right' }}>
                        ${metrica.totalIngresos.toLocaleString()}
                      </td>
                      <td style={{ padding: '12px', textAlign: 'right' }}>
                        ${(metrica.totalIngresos / metrica.cantidadReservas).toFixed(2)}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </Box>
          </Paper>
        </Grid>
      </Grid>
    </Box>
  );
}
