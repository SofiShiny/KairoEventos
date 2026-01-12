import { type FC, useState, useEffect } from 'react';
import {
  TextField,
  Button,
  Box,
  Typography,
  Alert,
  Paper,
  Grid,
  CircularProgress
} from '@mui/material';
import { DateTimePicker } from '@mui/x-date-pickers/DateTimePicker';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import dayjs, { type Dayjs } from 'dayjs';
import { useCreateEvent, useUpdateEvent } from '../api/events';
import type { EventoCreateDto, Evento } from '../types/api';
import { useNavigate } from 'react-router-dom';
import SeatManagementTab from './SeatManagementTab';
import { createMapForEvent } from '../api/seats';

interface EventFormProps {
  initialData?: Evento;
  mode: 'create' | 'edit';
  redirectUrl: string; // URL to redirect after success
}

const EventForm: FC<EventFormProps> = ({ initialData, mode, redirectUrl }) => {
  const navigate = useNavigate();

  const createEventMutation = useCreateEvent();
  const updateEventMutation = useUpdateEvent();

  const [formData, setFormData] = useState<{
    titulo: string;
    descripcion: string;
    ubicacion: {
      nombreLugar: string;
      direccion: string;
      ciudad: string;
      pais: string;
    };
    maximoAsistentes: number;
  }>({
    titulo: '',
    descripcion: '',
    ubicacion: {
      nombreLugar: '',
      direccion: '',
      ciudad: '',
      pais: ''
    },
    maximoAsistentes: 100
  });

  const [dates, setDates] = useState<{ start: Dayjs | null; end: Dayjs | null }>({
    start: dayjs().add(1, 'day'),
    end: dayjs().add(1, 'day').add(2, 'hour')
  });

  useEffect(() => {
    if (initialData) {
      setFormData({
        titulo: initialData.titulo,
        descripcion: initialData.descripcion,
        ubicacion: {
          nombreLugar: initialData.ubicacion.nombreLugar,
          direccion: initialData.ubicacion.direccion,
          ciudad: initialData.ubicacion.ciudad || '',
          pais: initialData.ubicacion.pais || ''
        },
        maximoAsistentes: initialData.maximoAsistentes
      });
      setDates({
        start: dayjs(initialData.fechaInicio),
        end: dayjs(initialData.fechaFin)
      });
    }
  }, [initialData]);

  const [message, setMessage] = useState<{ type: 'success' | 'error' | 'warning' | 'info'; text: string } | null>(null);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    if (!dates.start || !dates.end) {
      setMessage({ type: 'error', text: 'Por favor selecciona las fechas.' });
      return;
    }

    const payload: EventoCreateDto = {
      ...formData,
      fechaInicio: dates.start.toISOString(),
      fechaFin: dates.end.toISOString()
    };

    if (mode === 'create') {
      createEventMutation.mutate(payload, {
        onSuccess: () => {
          setMessage({ type: 'success', text: 'Evento creado correctamente.' });
          setTimeout(() => {
            navigate(redirectUrl);
          }, 1500);
        },
        onError: (error) => {
          // eslint-disable-next-line @typescript-eslint/no-explicit-any
          const errAny = error as any;
          console.error('Error creating event:', errAny);
          setMessage({ type: 'error', text: errAny.message || 'Error al crear evento' });
        }
      });
    } else {
      if (!initialData?.id) return;
      updateEventMutation.mutate(
        { id: initialData.id, event: payload },
        {
          onSuccess: () => {
            setMessage({ type: 'success', text: 'Evento actualizado correctamente.' });
            setTimeout(() => {
              navigate(redirectUrl);
            }, 1500);
          },
          onError: (error) => {
            // eslint-disable-next-line @typescript-eslint/no-explicit-any
            const errAny = error as any;
            console.error('Error updating event:', errAny);
            setMessage({ type: 'error', text: errAny.message || 'Error al actualizar evento' });
          }
        }
      );
    }
  };

  const isPending = createEventMutation.isPending || updateEventMutation.isPending;

  return (
    <LocalizationProvider dateAdapter={AdapterDayjs}>
      <Paper elevation={3} sx={{ p: 4, maxWidth: 800, mx: 'auto', mt: 4 }}>
        <Typography variant="h5" component="h2" gutterBottom>
          {mode === 'create' ? 'Crear Nuevo Evento' : 'Editar Evento'}
        </Typography>

        {message && (
          <Alert severity={message.type} sx={{ mb: 2 }}>
            {message.text}
          </Alert>
        )}

        <Box component="form" onSubmit={handleSubmit} noValidate>
          <Grid container spacing={2}>
            <Grid item xs={12}>
              <TextField
                required
                fullWidth
                label="Título"
                value={formData.titulo}
                onChange={(e) => setFormData({ ...formData, titulo: e.target.value })}
              />
            </Grid>

            <Grid item xs={12}>
              <TextField
                required
                fullWidth
                multiline
                rows={3}
                label="Descripción"
                value={formData.descripcion}
                onChange={(e) => setFormData({ ...formData, descripcion: e.target.value })}
              />
            </Grid>

            <Grid item xs={12} sm={6}>
              <TextField
                required
                fullWidth
                label="Lugar"
                value={formData.ubicacion.nombreLugar}
                onChange={(e) => setFormData({
                  ...formData,
                  ubicacion: { ...formData.ubicacion, nombreLugar: e.target.value }
                })}
              />
            </Grid>

            <Grid item xs={12} sm={6}>
              <TextField
                required
                fullWidth
                label="Dirección"
                value={formData.ubicacion.direccion}
                onChange={(e) => setFormData({
                  ...formData,
                  ubicacion: { ...formData.ubicacion, direccion: e.target.value }
                })}
              />
            </Grid>

            <Grid item xs={12} sm={6}>
              <TextField
                required
                fullWidth
                label="Ciudad"
                value={formData.ubicacion.ciudad}
                onChange={(e) => setFormData({
                  ...formData,
                  ubicacion: { ...formData.ubicacion, ciudad: e.target.value }
                })}
              />
            </Grid>

            <Grid item xs={12} sm={6}>
              <TextField
                required
                fullWidth
                label="País"
                value={formData.ubicacion.pais}
                onChange={(e) => setFormData({
                  ...formData,
                  ubicacion: { ...formData.ubicacion, pais: e.target.value }
                })}
              />
            </Grid>

            <Grid item xs={12} sm={6}>
              <DateTimePicker
                label="Fecha Inicio"
                value={dates.start}
                onChange={(newValue) => setDates({ ...dates, start: newValue })}
                slotProps={{ textField: { fullWidth: true, required: true } }}
              />
            </Grid>

            <Grid item xs={12} sm={6}>
              <DateTimePicker
                label="Fecha Fin"
                value={dates.end}
                onChange={(newValue) => setDates({ ...dates, end: newValue })}
                slotProps={{ textField: { fullWidth: true, required: true } }}
              />
            </Grid>

            <Grid item xs={12}>
              <TextField
                required
                fullWidth
                type="number"
                label="Máximo Asistentes"
                value={formData.maximoAsistentes}
                onChange={(e) => setFormData({ ...formData, maximoAsistentes: parseInt(e.target.value) || 0 })}
              />
            </Grid>

            <Grid item xs={12}>
              <Button
                type="submit"
                variant="contained"
                fullWidth
                size="large"
                disabled={isPending}
              >
                {isPending ? 'Guardando...' : (mode === 'create' ? 'Crear Evento' : 'Actualizar Evento')}
              </Button>
            </Grid>
          </Grid>
        </Box>

        {/* Seat Configuration Section - Full Management */}
        {mode === 'edit' && initialData?.id && (
          <SeatConfigurationWrapper eventoId={initialData.id} />
        )}
      </Paper>
    </LocalizationProvider>
  );
};

// Wrapper component to handle map creation if needed
const SeatConfigurationWrapper: FC<{ eventoId: string }> = ({ eventoId }) => {
  const [mapaId, setMapaId] = useState<string>('');
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const initializeMap = async () => {
      try {
        // Check if we have a stored mapaId
        const storedMapId = localStorage.getItem(`map_for_event_${eventoId}`);
        
        if (storedMapId) {
          setMapaId(storedMapId);
          setIsLoading(false);
          return;
        }

        // No stored map, try to create one
        console.log('[SeatConfigurationWrapper] No stored map found, creating new map for event:', eventoId);
        const result = await createMapForEvent(eventoId);
        
        if (result.mapId) {
          setMapaId(result.mapId);
        } else {
          setError('No se pudo obtener el ID del mapa');
        }
      } catch (err: any) {
        console.error('[SeatConfigurationWrapper] Error initializing map:', err);
        setError(err.message || 'Error al inicializar el mapa de asientos');
      } finally {
        setIsLoading(false);
      }
    };

    initializeMap();
  }, [eventoId]);

  if (isLoading) {
    return (
      <Box sx={{ mt: 4, display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 2 }}>
        <CircularProgress />
        <Typography>Inicializando configuración de asientos...</Typography>
      </Box>
    );
  }

  if (error) {
    return (
      <Box sx={{ mt: 4 }}>
        <Alert severity="error">{error}</Alert>
      </Box>
    );
  }

  if (!mapaId) {
    return (
      <Box sx={{ mt: 4 }}>
        <Alert severity="warning">No se pudo cargar la configuración de asientos</Alert>
      </Box>
    );
  }

  return (
    <Box sx={{ mt: 4 }}>
      <Typography variant="h6" gutterBottom>
        Configuración de Asientos
      </Typography>
      <SeatManagementTab eventoId={eventoId} mapaId={mapaId} />
    </Box>
  );
};

export default EventForm;
