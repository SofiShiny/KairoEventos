/**
 * ConciliacionFinanciera Component
 * Displays financial reconciliation summary
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
  PieChart,
  Pie,
  Cell,
  ResponsiveContainer,
  Legend,
  Tooltip,
} from 'recharts';
import type { ConciliacionFinanciera as ConciliacionFinancieraType } from '../types';
import { ErrorMessage } from '../../../shared/components/ErrorMessage';
import { EmptyState } from '../../../shared/components/EmptyState';
import AccountBalanceIcon from '@mui/icons-material/AccountBalance';
import AttachMoneyIcon from '@mui/icons-material/AttachMoney';
import ReceiptIcon from '@mui/icons-material/Receipt';

interface ConciliacionFinancieraProps {
  conciliacion: ConciliacionFinancieraType | null;
  isLoading: boolean;
  error: Error | null;
}

const COLORS = ['#0088FE', '#00C49F', '#FFBB28', '#FF8042', '#8884D8'];

/**
 * ConciliacionFinanciera - Financial reconciliation summary
 * Shows revenue and transaction breakdown
 */
export function ConciliacionFinanciera({
  conciliacion,
  isLoading,
  error,
}: ConciliacionFinancieraProps) {
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

  if (!conciliacion) {
    return (
      <EmptyState
        icon={<AccountBalanceIcon />}
        title="No hay datos financieros"
        description="No se encontraron datos de conciliación para el período seleccionado"
      />
    );
  }

  // Prepare data for category breakdown chart
  const categoryData = Object.entries(conciliacion.desglosePorCategoria).map(
    ([name, value]) => ({
      name,
      value: Number(value),
    })
  );

  const promedioTransaccion =
    conciliacion.cantidadTransacciones > 0
      ? conciliacion.totalIngresos / conciliacion.cantidadTransacciones
      : 0;

  return (
    <Box>
      {/* Summary Cards */}
      <Grid container spacing={3} mb={4}>
        <Grid size={{ xs: 12, md: 4 }}>
          <Card sx={{ bgcolor: 'success.light', color: 'success.contrastText' }}>
            <CardContent>
              <Box display="flex" alignItems="center" mb={1}>
                <AttachMoneyIcon sx={{ mr: 1 }} />
                <Typography variant="body2">Total Ingresos</Typography>
              </Box>
              <Typography variant="h4">
                ${conciliacion.totalIngresos.toLocaleString()}
              </Typography>
              <Typography variant="caption">
                Período: {new Date(conciliacion.fechaInicio).toLocaleDateString()} -{' '}
                {new Date(conciliacion.fechaFin).toLocaleDateString()}
              </Typography>
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, md: 4 }}>
          <Card sx={{ bgcolor: 'primary.main', color: 'primary.contrastText' }}>
            <CardContent>
              <Box display="flex" alignItems="center" mb={1}>
                <ReceiptIcon sx={{ mr: 1 }} />
                <Typography variant="body2">Total Transacciones</Typography>
              </Box>
              <Typography variant="h4">
                {conciliacion.cantidadTransacciones}
              </Typography>
              <Typography variant="caption">
                Reservas procesadas
              </Typography>
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, md: 4 }}>
          <Card sx={{ bgcolor: 'info.main', color: 'info.contrastText' }}>
            <CardContent>
              <Box display="flex" alignItems="center" mb={1}>
                <AccountBalanceIcon sx={{ mr: 1 }} />
                <Typography variant="body2">Promedio por Transacción</Typography>
              </Box>
              <Typography variant="h4">
                ${promedioTransaccion.toFixed(2)}
              </Typography>
              <Typography variant="caption">
                Ingreso promedio
              </Typography>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Category Breakdown */}
      {categoryData.length > 0 && (
        <Paper sx={{ p: 3, mb: 3 }}>
          <Typography variant="h6" gutterBottom>
            Desglose por Categoría
          </Typography>
          <Grid container spacing={3}>
            <Grid size={{ xs: 12, md: 6 }}>
              <ResponsiveContainer width="100%" height={300}>
                <PieChart>
                  <Pie
                    data={categoryData}
                    cx="50%"
                    cy="50%"
                    labelLine={false}
                    label={({ name, percent = 0 }) =>
                      `${name}: ${(percent * 100).toFixed(0)}%`
                    }
                    outerRadius={100}
                    fill="#8884d8"
                    dataKey="value"
                  >
                    {categoryData.map((_entry, index) => (
                      <Cell
                        key={`cell-${index}`}
                        fill={COLORS[index % COLORS.length]}
                      />
                    ))}
                  </Pie>
                  <Tooltip formatter={(value) => `$${Number(value).toLocaleString()}`} />
                  <Legend />
                </PieChart>
              </ResponsiveContainer>
            </Grid>
            <Grid size={{ xs: 12, md: 6 }}>
              <Box sx={{ mt: 2 }}>
                {categoryData.map((cat, index) => (
                  <Box
                    key={cat.name}
                    sx={{
                      display: 'flex',
                      justifyContent: 'space-between',
                      py: 1.5,
                      px: 2,
                      mb: 1,
                      bgcolor: 'grey.50',
                      borderRadius: 1,
                      borderLeft: 4,
                      borderColor: COLORS[index % COLORS.length],
                    }}
                  >
                    <Typography variant="body2">{cat.name}</Typography>
                    <Typography variant="body2" fontWeight="bold">
                      ${cat.value.toLocaleString()}
                    </Typography>
                  </Box>
                ))}
              </Box>
            </Grid>
          </Grid>
        </Paper>
      )}

      {/* Transactions Table */}
      <Paper sx={{ p: 3 }}>
        <Typography variant="h6" gutterBottom>
          Detalle de Transacciones
        </Typography>
        <Box sx={{ overflowX: 'auto' }}>
          <table style={{ width: '100%', borderCollapse: 'collapse' }}>
            <thead>
              <tr style={{ borderBottom: '2px solid #e0e0e0' }}>
                <th style={{ padding: '12px', textAlign: 'left' }}>Evento</th>
                <th style={{ padding: '12px', textAlign: 'left' }}>Fecha</th>
                <th style={{ padding: '12px', textAlign: 'center' }}>
                  Reservas
                </th>
                <th style={{ padding: '12px', textAlign: 'right' }}>Monto</th>
              </tr>
            </thead>
            <tbody>
              {conciliacion.transacciones.map((transaccion, index) => (
                <tr
                  key={`${transaccion.eventoId}-${index}`}
                  style={{
                    borderBottom: '1px solid #e0e0e0',
                    backgroundColor:
                      index % 2 === 0 ? '#fafafa' : 'transparent',
                  }}
                >
                  <td style={{ padding: '12px' }}>
                    <Typography variant="body2">
                      {transaccion.tituloEvento}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      ID: {transaccion.eventoId}
                    </Typography>
                  </td>
                  <td style={{ padding: '12px' }}>
                    {new Date(transaccion.fecha).toLocaleDateString('es-ES', {
                      year: 'numeric',
                      month: 'short',
                      day: 'numeric',
                    })}
                  </td>
                  <td style={{ padding: '12px', textAlign: 'center' }}>
                    {transaccion.cantidadReservas}
                  </td>
                  <td style={{ padding: '12px', textAlign: 'right' }}>
                    <Typography variant="body2" fontWeight="bold">
                      ${transaccion.monto.toLocaleString()}
                    </Typography>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </Box>

        {/* Summary */}
        <Box sx={{ mt: 3, pt: 2, borderTop: 1, borderColor: 'divider' }}>
          <Grid container spacing={2}>
            <Grid size={{ xs: 12, sm: 6 }}>
              <Typography variant="body2" color="text.secondary">
                Total de eventos: <strong>{conciliacion.transacciones.length}</strong>
              </Typography>
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <Typography variant="body2" color="text.secondary" textAlign="right">
                Total ingresos:{' '}
                <strong>${conciliacion.totalIngresos.toLocaleString()}</strong>
              </Typography>
            </Grid>
          </Grid>
        </Box>
      </Paper>
    </Box>
  );
}
