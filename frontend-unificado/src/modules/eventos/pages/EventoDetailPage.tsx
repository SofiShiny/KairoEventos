/**
 * EventoDetailPage
 * Displays detailed information about a specific evento
 * Includes actions for purchasing tickets and managing the evento (Admin/Organizator)
 */

import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Box,
  Typography,
  Button,
  Stack,
  Paper,
  Chip,
  Grid,
  Divider,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  DialogContentText,
} from '@mui/material';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import EditIcon from '@mui/icons-material/Edit';
import CancelIcon from '@mui/icons-material/Cancel';
import ShoppingCartIcon from '@mui/icons-material/ShoppingCart';
import EventIcon from '@mui/icons-material/Event';
import LocationOnIcon from '@mui/icons-material/LocationOn';
import EventSeatIcon from '@mui/icons-material/EventSeat';
import { useEvento, useCancelarEvento, useUpdateEvento } from '../hooks';
import { EventoForm } from '../components';
import { LoadingSpinner, ErrorMessage } from '@shared/components';
import { useAuth } from '@/context/AuthContext';
import type { CreateEventoDto } from '../types';

export function EventoDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { hasRole } = useAuth();
  const { data: evento, isLoading, error, refetch } = useEvento(id!);
  const { mutate: cancelarEvento, isPending: isCanceling } = useCancelarEvento();
  const { mutate: updateEvento, isPending: isUpdating } = useUpdateEvento();

  const [editFormOpen, setEditFormOpen] = useState(false);
  const [cancelDialogOpen, setCancelDialogOpen] = useState(false);

  // Check if user can manage eventos (Admin or Organizator)
  const canManageEventos = hasRole('Admin') || hasRole('Organizator');

  if (isLoading) {
    return <LoadingSpinner size="large" />;
  }

  if (error) {
    return (
      <Box>
        <Button
          startIcon={<ArrowBackIcon />}
          onClick={() => navigate('/eventos')}
          sx={{ mb: 2 }}
        >
          Volver a Eventos
        </Button>
        <ErrorMessage error={error} onRetry={refetch} variant="centered" />
      </Box>
    );
  }

  if (!evento) {
    return (
      <Box>
        <Button
          startIcon={<ArrowBackIcon />}
          onClick={() => navigate('/eventos')}
          sx={{ mb: 2 }}
        >
          Volver a Eventos
        </Button>
        <Typography variant="h6">Evento no encontrado</Typography>
      </Box>
    );
  }

  const isCancelado = evento.estado === 'Cancelado';
  const isAgotado = evento.asientosDisponibles === 0;

  // Format date
  const formatFecha = (fecha: string) => {
    const date = new Date(fecha);
    return date.toLocaleDateString('es-ES', {
      weekday: 'long',
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const handleComprar = () => {
    navigate(`/comprar/${evento.id}`);
  };

  const handleEdit = () => {
    setEditFormOpen(true);
  };

  const handleCancelClick = () => {
    setCancelDialogOpen(true);
  };

  const handleCancelConfirm = () => {
    cancelarEvento(evento.id, {
      onSuccess: () => {
        setCancelDialogOpen(false);
        navigate('/eventos');
      },
    });
  };

  const handleEditSubmit = (data: CreateEventoDto) => {
    updateEvento(
      { id: evento.id, data },
      {
        onSuccess: () => {
          setEditFormOpen(false);
        },
      }
    );
  };

  // Calculate availability percentage
  const disponibilidadPorcentaje = Math.round(
    (evento.asientosDisponibles / evento.capacidadTotal) * 100
  );

  return (
    <Box>
      {/* Back Button */}
      <Button
        startIcon={<ArrowBackIcon />}
        onClick={() => navigate('/eventos')}
        sx={{ mb: 3 }}
      >
        Volver a Eventos
      </Button>

      <Grid container spacing={3}>
        {/* Main Content */}
        <Grid size={{ xs: 12, md: 8 }}>
          <Paper sx={{ p: 3 }}>
            {/* Image */}
            {evento.imagenUrl && (
              <Box
                component="img"
                src={evento.imagenUrl}
                alt={evento.nombre}
                sx={{
                  width: '100%',
                  height: 400,
                  objectFit: 'cover',
                  borderRadius: 1,
                  mb: 3,
                }}
              />
            )}

            {/* Title and Status */}
            <Stack direction="row" spacing={2} alignItems="center" sx={{ mb: 2 }}>
              <Typography variant="h4" component="h1" sx={{ flexGrow: 1 }}>
                {evento.nombre}
              </Typography>
              {isCancelado && <Chip label="Cancelado" color="error" />}
              {isAgotado && !isCancelado && <Chip label="Agotado" color="warning" />}
            </Stack>

            <Divider sx={{ my: 2 }} />

            {/* Description */}
            <Typography variant="h6" gutterBottom>
              Descripción
            </Typography>
            <Typography variant="body1" color="text.secondary" paragraph>
              {evento.descripcion}
            </Typography>

            <Divider sx={{ my: 2 }} />

            {/* Details */}
            <Typography variant="h6" gutterBottom>
              Detalles del Evento
            </Typography>
            <Stack spacing={2}>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                <EventIcon color="action" />
                <Box>
                  <Typography variant="body2" color="text.secondary">
                    Fecha y Hora
                  </Typography>
                  <Typography variant="body1">{formatFecha(evento.fecha)}</Typography>
                </Box>
              </Box>

              <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                <LocationOnIcon color="action" />
                <Box>
                  <Typography variant="body2" color="text.secondary">
                    Ubicación
                  </Typography>
                  <Typography variant="body1">{evento.ubicacion}</Typography>
                </Box>
              </Box>

              <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                <EventSeatIcon color="action" />
                <Box>
                  <Typography variant="body2" color="text.secondary">
                    Disponibilidad
                  </Typography>
                  <Typography variant="body1">
                    {evento.asientosDisponibles} de {evento.capacidadTotal} asientos disponibles
                    ({disponibilidadPorcentaje}%)
                  </Typography>
                </Box>
              </Box>
            </Stack>
          </Paper>
        </Grid>

        {/* Sidebar - Actions */}
        <Grid size={{ xs: 12, md: 4 }}>
          <Paper sx={{ p: 3, position: 'sticky', top: 16 }}>
            <Typography variant="h6" gutterBottom>
              Acciones
            </Typography>

            <Stack spacing={2}>
              {/* Purchase Button - For all users */}
              {!isCancelado && (
                <Button
                  variant="contained"
                  size="large"
                  fullWidth
                  startIcon={<ShoppingCartIcon />}
                  onClick={handleComprar}
                  disabled={isAgotado}
                >
                  {isAgotado ? 'Entradas Agotadas' : 'Comprar Entrada'}
                </Button>
              )}

              {/* Management Buttons - Admin/Organizator only */}
              {canManageEventos && !isCancelado && (
                <>
                  <Divider sx={{ my: 1 }} />
                  <Typography variant="body2" color="text.secondary" gutterBottom>
                    Gestión del Evento
                  </Typography>

                  <Button
                    variant="outlined"
                    fullWidth
                    startIcon={<EditIcon />}
                    onClick={handleEdit}
                  >
                    Editar Evento
                  </Button>

                  <Button
                    variant="outlined"
                    color="error"
                    fullWidth
                    startIcon={<CancelIcon />}
                    onClick={handleCancelClick}
                    disabled={isCanceling}
                  >
                    {isCanceling ? 'Cancelando...' : 'Cancelar Evento'}
                  </Button>
                </>
              )}
            </Stack>
          </Paper>
        </Grid>
      </Grid>

      {/* Edit Form Dialog */}
      <EventoForm
        open={editFormOpen}
        onClose={() => setEditFormOpen(false)}
        onSubmit={handleEditSubmit}
        evento={evento}
        isSubmitting={isUpdating}
      />

      {/* Cancel Confirmation Dialog */}
      <Dialog
        open={cancelDialogOpen}
        onClose={() => setCancelDialogOpen(false)}
        aria-labelledby="cancel-dialog-title"
      >
        <DialogTitle id="cancel-dialog-title">Cancelar Evento</DialogTitle>
        <DialogContent>
          <DialogContentText>
            ¿Está seguro de que desea cancelar el evento "{evento.nombre}"?
            Esta acción no se puede deshacer y todos los asistentes serán notificados.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setCancelDialogOpen(false)} disabled={isCanceling}>
            No, mantener evento
          </Button>
          <Button
            onClick={handleCancelConfirm}
            color="error"
            variant="contained"
            disabled={isCanceling}
          >
            {isCanceling ? 'Cancelando...' : 'Sí, cancelar evento'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}


