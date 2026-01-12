/**
 * MisEntradasPage - User's tickets page
 * Displays list of user's entradas with filtering and actions
 */

import { useState } from 'react';
import { Container, Typography, Box, Alert, Button } from '@mui/material';
import { useMisEntradas, useCancelarEntrada } from '../hooks';
import { EntradaCard, EntradaFilters } from '../components';
import type { EstadoEntrada } from '../types';
import { LoadingSpinner, EmptyState, ErrorMessage } from '@shared/components';
import ConfirmationNumberIcon from '@mui/icons-material/ConfirmationNumber';

export function MisEntradasPage() {
  const [filtroEstado, setFiltroEstado] = useState<EstadoEntrada | 'Todas'>('Todas');
  
  const { data: entradas, isLoading, error, refetch } = useMisEntradas(
    filtroEstado === 'Todas' ? undefined : filtroEstado
  );
  
  const { mutate: cancelarEntrada, isPending: isCanceling } = useCancelarEntrada();

  const handlePagar = (entradaId: string) => {
    // TODO: Implement payment flow in future task
    console.log('Pagar entrada:', entradaId);
    alert('Funcionalidad de pago será implementada próximamente');
  };

  const handleCancelar = (entradaId: string) => {
    if (window.confirm('¿Está seguro de que desea cancelar esta entrada?')) {
      cancelarEntrada(entradaId);
    }
  };

  if (isLoading) {
    return <LoadingSpinner fullScreen />;
  }

  if (error) {
    return (
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <ErrorMessage
          error={error}
          onRetry={refetch}
        />
      </Container>
    );
  }

  const hasReservedTickets = entradas?.some((e) => e.estado === 'Reservada') || false;

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Typography variant="h3" component="h1" gutterBottom>
        Mis Entradas
      </Typography>

      <Typography variant="body1" color="text.secondary" paragraph>
        Gestiona tus entradas compradas y reservadas
      </Typography>

      {/* Warning for reserved tickets */}
      {hasReservedTickets && (
        <Alert severity="warning" sx={{ mb: 3 }}>
          Tienes entradas reservadas. Recuerda pagarlas antes de que expire el tiempo límite (15 minutos).
        </Alert>
      )}

      {/* Filters */}
      <EntradaFilters
        selectedEstado={filtroEstado}
        onEstadoChange={setFiltroEstado}
      />

      {/* Entradas List */}
      {!entradas || entradas.length === 0 ? (
        <EmptyState
          icon={<ConfirmationNumberIcon sx={{ fontSize: 64 }} />}
          title="No tienes entradas"
          description={
            filtroEstado === 'Todas'
              ? 'Aún no has comprado ninguna entrada. Explora los eventos disponibles.'
              : `No tienes entradas en estado "${filtroEstado}".`
          }
          action={
            <Button variant="contained" href="/eventos">
              Ver Eventos
            </Button>
          }
        />
      ) : (
        <Box
          sx={{
            display: 'grid',
            gridTemplateColumns: {
              xs: '1fr',
              sm: 'repeat(2, 1fr)',
              md: 'repeat(3, 1fr)',
            },
            gap: 3,
          }}
        >
          {entradas.map((entrada) => (
            <EntradaCard
              key={entrada.id}
              entrada={entrada}
              onPagar={handlePagar}
              onCancelar={handleCancelar}
            />
          ))}
        </Box>
      )}

      {/* Loading overlay when canceling */}
      {isCanceling && (
        <Box
          sx={{
            position: 'fixed',
            top: 0,
            left: 0,
            right: 0,
            bottom: 0,
            bgcolor: 'rgba(0, 0, 0, 0.5)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            zIndex: 9999,
          }}
        >
          <LoadingSpinner />
        </Box>
      )}
    </Container>
  );
}

