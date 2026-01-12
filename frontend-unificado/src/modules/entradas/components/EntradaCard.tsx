/**
 * EntradaCard Component
 * Displays a single entrada (ticket) with event info, seat, status, date, and price
 */

import {
  Card,
  CardContent,
  CardActions,
  Typography,
  Chip,
  Button,
  Box,
  Divider,
} from '@mui/material';
import {
  Event as EventIcon,
  EventSeat as SeatIcon,
  CalendarToday as CalendarIcon,
  AttachMoney as MoneyIcon,
  AccessTime as TimeIcon,
} from '@mui/icons-material';
import type { Entrada, EstadoEntrada } from '../types';
import { format } from 'date-fns';
import { es } from 'date-fns/locale';

interface EntradaCardProps {
  entrada: Entrada;
  onPagar?: (entradaId: string) => void;
  onCancelar?: (entradaId: string) => void;
}

/**
 * Get color for entrada status chip
 */
function getStatusColor(estado: EstadoEntrada): 'warning' | 'success' | 'error' | 'default' {
  switch (estado) {
    case 'Reservada':
      return 'warning';
    case 'Pagada':
      return 'success';
    case 'Cancelada':
      return 'error';
    default:
      return 'default';
  }
}

/**
 * Format time remaining in minutes to readable string
 */
function formatTimeRemaining(minutes: number): string {
  if (minutes <= 0) return 'Expirado';
  if (minutes < 60) return `${minutes} minutos`;
  const hours = Math.floor(minutes / 60);
  const mins = minutes % 60;
  return `${hours}h ${mins}m`;
}

export function EntradaCard({ entrada, onPagar, onCancelar }: EntradaCardProps) {
  const showPayButton = entrada.estado === 'Reservada' && onPagar;
  const showCancelButton = 
    (entrada.estado === 'Reservada' || entrada.estado === 'Pagada') && onCancelar;
  const showTimeRemaining = entrada.estado === 'Reservada' && entrada.tiempoRestante !== undefined;

  return (
    <Card elevation={2} sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
      <CardContent sx={{ flexGrow: 1 }}>
        {/* Status Chip */}
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
          <Chip
            label={entrada.estado}
            color={getStatusColor(entrada.estado)}
            size="small"
          />
          {showTimeRemaining && entrada.tiempoRestante !== undefined && (
            <Chip
              icon={<TimeIcon />}
              label={formatTimeRemaining(entrada.tiempoRestante)}
              color={entrada.tiempoRestante < 5 ? 'error' : 'warning'}
              size="small"
              variant="outlined"
            />
          )}
        </Box>

        {/* Event Name */}
        <Box sx={{ display: 'flex', alignItems: 'flex-start', gap: 1, mb: 2 }}>
          <EventIcon color="primary" />
          <Box>
            <Typography variant="h6" component="h3" gutterBottom>
              {entrada.eventoNombre}
            </Typography>
          </Box>
        </Box>

        <Divider sx={{ my: 2 }} />

        {/* Seat Info */}
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1.5 }}>
          <SeatIcon fontSize="small" color="action" />
          <Typography variant="body2" color="text.secondary">
            Asiento: <strong>{entrada.asientoInfo}</strong>
          </Typography>
        </Box>

        {/* Purchase Date */}
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1.5 }}>
          <CalendarIcon fontSize="small" color="action" />
          <Typography variant="body2" color="text.secondary">
            Comprado: {format(new Date(entrada.fechaCompra), 'PPP', { locale: es })}
          </Typography>
        </Box>

        {/* Price */}
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <MoneyIcon fontSize="small" color="action" />
          <Typography variant="body2" color="text.secondary">
            Precio: <strong>${entrada.precio.toFixed(2)}</strong>
          </Typography>
        </Box>
      </CardContent>

      {/* Actions */}
      {(showPayButton || showCancelButton) && (
        <CardActions sx={{ p: 2, pt: 0 }}>
          <Box sx={{ display: 'flex', gap: 1, width: '100%' }}>
            {showPayButton && (
              <Button
                variant="contained"
                color="primary"
                fullWidth
                onClick={() => onPagar!(entrada.id)}
              >
                Pagar
              </Button>
            )}
            {showCancelButton && (
              <Button
                variant="outlined"
                color="error"
                fullWidth
                onClick={() => onCancelar!(entrada.id)}
              >
                Cancelar
              </Button>
            )}
          </Box>
        </CardActions>
      )}
    </Card>
  );
}
