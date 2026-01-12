/**
 * EventoCard Component
 * Displays an individual evento card with image, details, and actions
 * 
 * Performance optimizations:
 * - Memoized with React.memo to prevent unnecessary re-renders
 * - useCallback for event handlers to maintain referential equality
 * - useMemo for computed values
 * - Prefetching on hover for better UX
 */

import { memo, useCallback, useMemo } from 'react';
import {
  Card,
  CardContent,
  CardActions,
  Typography,
  Button,
  Box,
  Chip,
  Stack,
} from '@mui/material';
import { useNavigate } from 'react-router-dom';
import EventIcon from '@mui/icons-material/Event';
import LocationOnIcon from '@mui/icons-material/LocationOn';
import EventSeatIcon from '@mui/icons-material/EventSeat';
import { ImagePlaceholder } from '@shared/components';
import { usePrefetchEvento } from '../hooks/useEventos';
import type { Evento } from '../types';

interface EventoCardProps {
  evento: Evento;
  onEventoClick?: (eventoId: string) => void;
}

function EventoCardComponent({ evento, onEventoClick }: EventoCardProps) {
  const navigate = useNavigate();
  const prefetchEvento = usePrefetchEvento();

  const handleVerDetalles = useCallback(() => {
    if (onEventoClick) {
      onEventoClick(evento.id);
    } else {
      navigate(`/eventos/${evento.id}`);
    }
  }, [evento.id, onEventoClick, navigate]);

  const handleComprar = useCallback((e: React.MouseEvent) => {
    e.stopPropagation();
    navigate(`/comprar/${evento.id}`);
  }, [evento.id, navigate]);

  // Prefetch evento details on hover for better UX
  const handleMouseEnter = useCallback(() => {
    prefetchEvento(evento.id);
  }, [evento.id, prefetchEvento]);

  // Memoize computed values to avoid recalculation on every render
  const disponibilidadPorcentaje = useMemo(
    () => Math.round((evento.asientosDisponibles / evento.capacidadTotal) * 100),
    [evento.asientosDisponibles, evento.capacidadTotal]
  );

  const disponibilidadColor = useMemo(() => {
    if (disponibilidadPorcentaje > 50) return 'success';
    if (disponibilidadPorcentaje > 20) return 'warning';
    return 'error';
  }, [disponibilidadPorcentaje]);

  const fechaFormateada = useMemo(() => {
    const date = new Date(evento.fecha);
    return date.toLocaleDateString('es-ES', {
      weekday: 'short',
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  }, [evento.fecha]);

  const isCancelado = evento.estado === 'Cancelado';
  const isAgotado = evento.asientosDisponibles === 0;

  return (
    <Card
      onMouseEnter={handleMouseEnter}
      sx={{
        height: '100%',
        display: 'flex',
        flexDirection: 'column',
        transition: 'transform 0.2s, box-shadow 0.2s',
        opacity: isCancelado ? 0.6 : 1,
        '&:hover': {
          transform: isCancelado ? 'none' : 'translateY(-4px)',
          boxShadow: isCancelado ? 2 : 6,
        },
      }}
    >
      <ImagePlaceholder
        src={evento.imagenUrl}
        alt={evento.nombre}
        height={180}
        objectFit="cover"
        loading="lazy"
      />
      
      {isCancelado && (
        <Box
          sx={{
            position: 'absolute',
            top: 8,
            right: 8,
          }}
        >
          <Chip label="Cancelado" color="error" size="small" />
        </Box>
      )}

      <CardContent sx={{ flexGrow: 1 }}>
        <Typography variant="h6" component="h3" gutterBottom noWrap>
          {evento.nombre}
        </Typography>
        
        <Typography
          variant="body2"
          color="text.secondary"
          sx={{
            mb: 2,
            overflow: 'hidden',
            textOverflow: 'ellipsis',
            display: '-webkit-box',
            WebkitLineClamp: 2,
            WebkitBoxOrient: 'vertical',
            minHeight: 40,
          }}
        >
          {evento.descripcion}
        </Typography>

        <Stack spacing={1}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <EventIcon fontSize="small" color="action" />
            <Typography variant="body2" color="text.secondary">
              {fechaFormateada}
            </Typography>
          </Box>

          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <LocationOnIcon fontSize="small" color="action" />
            <Typography variant="body2" color="text.secondary" noWrap>
              {evento.ubicacion}
            </Typography>
          </Box>

          {!isCancelado && (
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              <EventSeatIcon fontSize="small" color="action" />
              <Chip
                label={`${evento.asientosDisponibles} / ${evento.capacidadTotal} disponibles`}
                size="small"
                color={disponibilidadColor}
                variant="outlined"
              />
            </Box>
          )}
        </Stack>
      </CardContent>

      <CardActions sx={{ p: 2, pt: 0 }}>
        <Button size="small" onClick={handleVerDetalles}>
          Ver Detalles
        </Button>
        {!isCancelado && (
          <Button
            size="small"
            variant="contained"
            onClick={handleComprar}
            disabled={isAgotado}
          >
            {isAgotado ? 'Agotado' : 'Comprar'}
          </Button>
        )}
      </CardActions>
    </Card>
  );
}

// Memoize the component to prevent unnecessary re-renders
// Only re-render if evento or onEventoClick changes
export const EventoCard = memo(EventoCardComponent);
