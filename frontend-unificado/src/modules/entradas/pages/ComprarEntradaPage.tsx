/**
 * ComprarEntradaPage - Purchase ticket for an event
 * Displays seat map, allows seat selection, and handles ticket purchase
 */

import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Container,
  Typography,
  Paper,
  Button,
  Box,
  Alert,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  CircularProgress,
} from '@mui/material';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import ShoppingCartIcon from '@mui/icons-material/ShoppingCart';
import { useAsientosDisponibles, useCreateEntrada } from '../hooks';
import { MapaAsientos } from '../components';
import { LoadingSpinner, ErrorMessage } from '@shared/components';
import { useAuth } from '@/context/AuthContext';

const RESERVATION_TIME_MINUTES = 15;

export function ComprarEntradaPage() {
  const { eventoId } = useParams<{ eventoId: string }>();
  const navigate = useNavigate();
  const { user } = useAuth();
  
  const [selectedAsientoId, setSelectedAsientoId] = useState<string | null>(null);
  const [showConfirmDialog, setShowConfirmDialog] = useState(false);
  const [timeRemaining, setTimeRemaining] = useState(RESERVATION_TIME_MINUTES * 60); // in seconds

  const {
    data: asientos,
    isLoading,
    error,
    refetch,
  } = useAsientosDisponibles(eventoId!);

  const {
    mutate: createEntrada,
    isPending: isCreating,
    isSuccess,
  } = useCreateEntrada();

  // Countdown timer effect
  useEffect(() => {
    if (!selectedAsientoId) {
      setTimeRemaining(RESERVATION_TIME_MINUTES * 60);
      return;
    }

    const interval = setInterval(() => {
      setTimeRemaining((prev) => {
        if (prev <= 1) {
          clearInterval(interval);
          setSelectedAsientoId(null);
          alert('El tiempo de reserva ha expirado. Por favor, selecciona otro asiento.');
          return RESERVATION_TIME_MINUTES * 60;
        }
        return prev - 1;
      });
    }, 1000);

    return () => clearInterval(interval);
  }, [selectedAsientoId]);

  // Redirect after successful purchase
  useEffect(() => {
    if (isSuccess) {
      setTimeout(() => {
        navigate('/mis-entradas');
      }, 2000);
    }
  }, [isSuccess, navigate]);

  const handleAsientoSelect = (asientoId: string) => {
    setSelectedAsientoId(asientoId);
    setTimeRemaining(RESERVATION_TIME_MINUTES * 60); // Reset timer
  };

  const handleConfirmPurchase = () => {
    setShowConfirmDialog(true);
  };

  const handlePurchase = () => {
    if (!selectedAsientoId || !user) return;

    createEntrada({
      eventoId: eventoId!,
      asientoId: selectedAsientoId,
      usuarioId: user.profile.sub || '', // Use sub (subject) from OIDC profile
    });

    setShowConfirmDialog(false);
  };

  const formatTime = (seconds: number): string => {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  };

  const selectedAsiento = asientos?.find((a) => a.id === selectedAsientoId);

  if (!eventoId) {
    return (
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <Alert severity="error">ID de evento no válido</Alert>
      </Container>
    );
  }

  if (isLoading) {
    return <LoadingSpinner fullScreen />;
  }

  if (error) {
    return (
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <Button
          startIcon={<ArrowBackIcon />}
          onClick={() => navigate(`/eventos/${eventoId}`)}
          sx={{ mb: 2 }}
        >
          Volver al Evento
        </Button>
        <ErrorMessage error={error} onRetry={refetch} />
      </Container>
    );
  }

  if (isSuccess) {
    return (
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <Paper elevation={2} sx={{ p: 4, textAlign: 'center' }}>
          <Typography variant="h4" color="success.main" gutterBottom>
            ¡Entrada Reservada Exitosamente!
          </Typography>
          <Typography variant="body1" paragraph>
            Tu entrada ha sido reservada. Tienes 15 minutos para completar el pago.
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Redirigiendo a Mis Entradas...
          </Typography>
          <CircularProgress sx={{ mt: 2 }} />
        </Paper>
      </Container>
    );
  }

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Button
        startIcon={<ArrowBackIcon />}
        onClick={() => navigate(`/eventos/${eventoId}`)}
        sx={{ mb: 2 }}
      >
        Volver al Evento
      </Button>

      <Typography variant="h3" component="h1" gutterBottom>
        Comprar Entrada
      </Typography>

      <Typography variant="body1" color="text.secondary" paragraph>
        Selecciona un asiento disponible para continuar con la compra
      </Typography>

      {/* Timer Warning */}
      {selectedAsientoId && (
        <Alert
          severity={timeRemaining < 300 ? 'error' : 'info'}
          sx={{ mb: 3 }}
        >
          Tiempo restante para completar la reserva: <strong>{formatTime(timeRemaining)}</strong>
        </Alert>
      )}

      {/* Seat Map */}
      {asientos && asientos.length > 0 ? (
        <MapaAsientos
          asientos={asientos}
          onAsientoSelect={handleAsientoSelect}
          selectedAsientoId={selectedAsientoId || undefined}
        />
      ) : (
        <Alert severity="warning" sx={{ mt: 3 }}>
          No hay asientos disponibles para este evento.
        </Alert>
      )}

      {/* Purchase Button */}
      {selectedAsiento && (
        <Box sx={{ mt: 4, display: 'flex', justifyContent: 'center' }}>
          <Button
            variant="contained"
            size="large"
            startIcon={<ShoppingCartIcon />}
            onClick={handleConfirmPurchase}
            disabled={isCreating}
          >
            Confirmar Compra - ${selectedAsiento.precio.toFixed(2)}
          </Button>
        </Box>
      )}

      {/* Confirmation Dialog */}
      <Dialog open={showConfirmDialog} onClose={() => setShowConfirmDialog(false)}>
        <DialogTitle>Confirmar Compra</DialogTitle>
        <DialogContent>
          {selectedAsiento && (
            <Box>
              <Typography variant="body1" gutterBottom>
                ¿Confirmas la compra de esta entrada?
              </Typography>
              <Box sx={{ mt: 2, p: 2, bgcolor: 'grey.100', borderRadius: 1 }}>
                <Typography variant="body2">
                  <strong>Asiento:</strong> Fila {selectedAsiento.fila}, Número {selectedAsiento.numero}
                </Typography>
                <Typography variant="body2">
                  <strong>Precio:</strong> ${selectedAsiento.precio.toFixed(2)}
                </Typography>
              </Box>
              <Alert severity="info" sx={{ mt: 2 }}>
                La entrada será reservada por 15 minutos. Deberás completar el pago antes de que expire.
              </Alert>
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setShowConfirmDialog(false)} disabled={isCreating}>
            Cancelar
          </Button>
          <Button
            onClick={handlePurchase}
            variant="contained"
            disabled={isCreating}
            startIcon={isCreating ? <CircularProgress size={20} /> : <ShoppingCartIcon />}
          >
            {isCreating ? 'Procesando...' : 'Confirmar'}
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
}

