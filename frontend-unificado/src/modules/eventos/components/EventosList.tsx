/**
 * EventosList Component
 * Displays a grid of evento cards with filtering and search capabilities
 * 
 * Performance optimizations:
 * - Memoized with React.memo
 * - useCallback for event handlers
 */

import { memo, useCallback } from 'react';
import { Grid, Box, Typography } from '@mui/material';
import { EventoCard } from './EventoCard';
import { EmptyState, SkeletonLoader } from '@shared/components';
import EventIcon from '@mui/icons-material/Event';
import type { Evento } from '../types';

interface EventosListProps {
  eventos: Evento[];
  isLoading?: boolean;
  onEventoClick?: (eventoId: string) => void;
}

function EventosListComponent({ eventos, isLoading, onEventoClick }: EventosListProps) {
  // Memoize the click handler to maintain referential equality
  const handleEventoClick = useCallback(
    (eventoId: string) => {
      if (onEventoClick) {
        onEventoClick(eventoId);
      }
    },
    [onEventoClick]
  );

  if (isLoading) {
    return <SkeletonLoader variant="card" count={6} />;
  }

  if (eventos.length === 0) {
    return (
      <EmptyState
        icon={<EventIcon />}
        title="No hay eventos disponibles"
        description="Actualmente no hay eventos publicados. Vuelve más tarde o crea un nuevo evento."
      />
    );
  }

  return (
    <Box>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
        {eventos.length} {eventos.length === 1 ? 'evento encontrado' : 'eventos encontrados'}
      </Typography>
      
      <Grid container spacing={3}>
        {eventos.map((evento) => (
          <Grid size={{ xs: 12, sm: 6, md: 4 }} key={evento.id}>
            <EventoCard evento={evento} onEventoClick={handleEventoClick} />
          </Grid>
        ))}
      </Grid>
    </Box>
  );
}

// Memoize to prevent re-renders when parent re-renders
export const EventosList = memo(EventosListComponent);
