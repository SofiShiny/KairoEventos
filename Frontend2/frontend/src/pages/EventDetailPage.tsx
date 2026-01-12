import React, { useState } from 'react';
import { useParams } from 'react-router-dom';
import { Typography, Box, Paper, Snackbar, Alert, Grid, Divider } from '@mui/material';
import { useEvent } from '../api/events';
import { useEventSeats, useReserveSeat } from '../api/seats';
import SeatGrid from '../components/SeatGrid';
import LocationOnIcon from '@mui/icons-material/LocationOn';
import CalendarTodayIcon from '@mui/icons-material/CalendarToday';

const EventDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const { data: event, isLoading: isLoadingEvent, error: errorEvent } = useEvent(id!);
  const { data: seats, isLoading: isLoadingSeats, error: errorSeats } = useEventSeats(id!);
  const reserveSeatMutation = useReserveSeat();

  const [notification, setNotification] = useState<{ open: boolean; message: string; severity: 'success' | 'error' }>({
    open: false,
    message: '',
    severity: 'success',
  });

  const handleSeatClick = (seatId: string) => {
    if (!id || !seats) return;
    
    // Find the seat by ID to get fila and numero
    const seatsArray = Array.isArray(seats) ? seats : seats.asientos || [];
    const seat = seatsArray.find((s: any) => s.id === seatId);
    
    if (!seat) {
      setNotification({ open: true, message: 'Asiento no encontrado.', severity: 'error' });
      return;
    }
    
    // Get mapaId from seats object
    const mapaId = seats.mapaId || '';
    
    reserveSeatMutation.mutate(
      { mapaId, fila: seat.fila, numero: seat.numero },
      {
        onSuccess: () => {
          setNotification({ open: true, message: 'Asiento reservado con éxito!', severity: 'success' });
        },
        onError: () => {
          setNotification({ open: true, message: 'Error al reservar el asiento.', severity: 'error' });
        },
      }
    );
  };

  if (isLoadingEvent) return <Typography>Cargando detalles del evento...</Typography>;
  if (errorEvent || !event) return <Typography color="error">Error al cargar el evento.</Typography>;

  return (
    <Box>
      <Paper elevation={3} sx={{ p: 4, mb: 4 }}>
        <Typography variant="h3" gutterBottom>
          {event.titulo}
        </Typography>
        <Grid container spacing={2} sx={{ mb: 2 }}>
          <Grid item xs={12} md={6}>
            <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
              <CalendarTodayIcon sx={{ mr: 1, color: 'primary.main' }} />
              <Typography variant="h6">
                {new Date(event.fechaInicio).toLocaleDateString()} - {new Date(event.fechaInicio).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
              </Typography>
            </Box>
            <Box sx={{ display: 'flex', alignItems: 'center' }}>
              <LocationOnIcon sx={{ mr: 1, color: 'primary.main' }} />
              <Typography variant="h6">
                {event.ubicacion.nombreLugar}, {event.ubicacion.direccion}, {event.ubicacion.ciudad}
              </Typography>
            </Box>
          </Grid>
        </Grid>
        <Divider sx={{ my: 2 }} />
        <Typography variant="body1" paragraph>
          {event.descripcion}
        </Typography>
      </Paper>

      <Box>
        <Typography variant="h4" gutterBottom>
          Reserva tu Asiento
        </Typography>
        {isLoadingSeats ? (
          <Typography>Cargando mapa de asientos...</Typography>
        ) : errorSeats ? (
          <Typography color="error">Error al cargar el mapa de asientos.</Typography>
        ) : seats ? (
          <SeatGrid seats={seats} onSeatClick={handleSeatClick} isInteractive={true} />
        ) : (
          <Typography>No hay información de asientos disponible.</Typography>
        )}
      </Box>

      <Snackbar
        open={notification.open}
        autoHideDuration={6000}
        onClose={() => setNotification({ ...notification, open: false })}
      >
        <Alert onClose={() => setNotification({ ...notification, open: false })} severity={notification.severity}>
          {notification.message}
        </Alert>
      </Snackbar>
    </Box>
  );
};

export default EventDetailPage;
