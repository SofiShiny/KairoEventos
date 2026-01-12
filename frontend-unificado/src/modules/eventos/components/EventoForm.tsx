/**
 * EventoForm Component
 * Form for creating and editing eventos (Admin/Organizator only)
 */

import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Button,
  Stack,
  FormHelperText,
} from '@mui/material';
import { eventoSchema, type EventoFormData } from '@/shared/validation';
import type { CreateEventoDto, Evento } from '../types';

interface EventoFormProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (data: CreateEventoDto) => void;
  evento?: Evento;
  isSubmitting?: boolean;
}

export function EventoForm({
  open,
  onClose,
  onSubmit,
  evento,
  isSubmitting = false,
}: EventoFormProps) {
  const isEditing = !!evento;

  const {
    control,
    handleSubmit,
    formState: { errors, isValid },
    reset,
  } = useForm<EventoFormData>({
    resolver: zodResolver(eventoSchema),
    defaultValues: evento
      ? {
          nombre: evento.nombre,
          descripcion: evento.descripcion,
          fecha: evento.fecha.split('T')[0], // Extract date part
          ubicacion: evento.ubicacion,
          imagenUrl: evento.imagenUrl || '',
        }
      : {
          nombre: '',
          descripcion: '',
          fecha: '',
          ubicacion: '',
          imagenUrl: '',
        },
    mode: 'onChange',
  });

  const handleFormSubmit = (data: EventoFormData) => {
    // Convert date to ISO 8601 format
    const isoDate = new Date(data.fecha).toISOString();
    
    onSubmit({
      nombre: data.nombre,
      descripcion: data.descripcion,
      fecha: isoDate,
      ubicacion: data.ubicacion,
      imagenUrl: data.imagenUrl || undefined,
    });
  };

  const handleClose = () => {
    reset();
    onClose();
  };

  return (
    <Dialog
      open={open}
      onClose={handleClose}
      maxWidth="sm"
      fullWidth
      aria-labelledby="evento-form-title"
    >
      <DialogTitle id="evento-form-title">
        {isEditing ? 'Editar Evento' : 'Crear Nuevo Evento'}
      </DialogTitle>

      <form onSubmit={handleSubmit(handleFormSubmit)}>
        <DialogContent>
          <Stack spacing={2}>
            <Controller
              name="nombre"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  label="Nombre del Evento"
                  fullWidth
                  required
                  error={!!errors.nombre}
                  helperText={errors.nombre?.message}
                  disabled={isSubmitting}
                  inputProps={{
                    'aria-label': 'Nombre del evento',
                  }}
                />
              )}
            />

            <Controller
              name="descripcion"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  label="Descripción"
                  fullWidth
                  required
                  multiline
                  rows={4}
                  error={!!errors.descripcion}
                  helperText={errors.descripcion?.message}
                  disabled={isSubmitting}
                  inputProps={{
                    'aria-label': 'Descripción del evento',
                  }}
                />
              )}
            />

            <Controller
              name="fecha"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  label="Fecha del Evento"
                  type="date"
                  fullWidth
                  required
                  InputLabelProps={{
                    shrink: true,
                  }}
                  inputProps={{
                    min: new Date().toISOString().split('T')[0], // Minimum today
                    'aria-label': 'Fecha del evento',
                  }}
                  error={!!errors.fecha}
                  helperText={errors.fecha?.message}
                  disabled={isSubmitting}
                />
              )}
            />

            <Controller
              name="ubicacion"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  label="Ubicación"
                  fullWidth
                  required
                  error={!!errors.ubicacion}
                  helperText={errors.ubicacion?.message}
                  disabled={isSubmitting}
                  inputProps={{
                    'aria-label': 'Ubicación del evento',
                  }}
                />
              )}
            />

            <Controller
              name="imagenUrl"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  label="URL de Imagen (opcional)"
                  fullWidth
                  error={!!errors.imagenUrl}
                  helperText={errors.imagenUrl?.message || 'URL completa de la imagen del evento'}
                  disabled={isSubmitting}
                  inputProps={{
                    'aria-label': 'URL de imagen del evento',
                  }}
                />
              )}
            />

            {!isValid && Object.keys(errors).length > 0 && (
              <FormHelperText error>
                Por favor, corrija los errores antes de continuar
              </FormHelperText>
            )}
          </Stack>
        </DialogContent>

        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={handleClose} disabled={isSubmitting}>
            Cancelar
          </Button>
          <Button
            type="submit"
            variant="contained"
            disabled={!isValid || isSubmitting}
          >
            {isSubmitting ? 'Guardando...' : isEditing ? 'Actualizar' : 'Crear'}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
}
