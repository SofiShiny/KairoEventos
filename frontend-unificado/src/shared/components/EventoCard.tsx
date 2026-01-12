/**
 * EventoCard Component
 * Displays a featured event card with image, details, and action button
 */

import {
  Card,
  CardContent,
  CardMedia,
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
import type { EventoDestacado } from '../types/dashboard';

interface EventoCardProps {
  evento: EventoDestacado;
}

export function EventoCard({ evento }: EventoCardProps) {
  const navigate = useNavigate();

  const handleVerDetalles = () => {
    navigate(`/eventos/${evento.id}`);
  };

  const handleComprar = () => {
    navigate(`/comprar/${evento.id}`);
  };

  // Calcular porcentaje de disponibilidad
  const disponibilidadPorcentaje = Math.round(
    (evento.asientosDisponibles / evento.capacidadTotal) * 100
  );

  // Determinar color del chip según disponibilidad
  const getDisponibilidadColor = () => {
    if (disponibilidadPorcentaje > 50) return 'success';
    if (disponibilidadPorcentaje > 20) return 'warning';
    return 'error';
  };

  // Formatear fecha
  const formatFecha = (fecha: string) => {
    const date = new Date(fecha);
    return date.toLocaleDateString('es-ES', {
      weekday: 'long',
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    });
  };

  return (
    <Card
      sx={{
        height: '100%',
        display: 'flex',
        flexDirection: 'column',
        transition: 'transform 0.2s, box-shadow 0.2s',
        '&:hover': {
          transform: 'translateY(-4px)',
          boxShadow: 6,
        },
      }}
    >
      <CardMedia
        component="img"
        height="200"
        image={evento.imagenUrl || '/placeholder-event.jpg'}
        alt={evento.nombre}
        sx={{ objectFit: 'cover' }}
      />
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
          }}
        >
          {evento.descripcion}
        </Typography>

        <Stack spacing={1}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <EventIcon fontSize="small" color="action" />
            <Typography variant="body2" color="text.secondary">
              {formatFecha(evento.fecha)}
            </Typography>
          </Box>

          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <LocationOnIcon fontSize="small" color="action" />
            <Typography variant="body2" color="text.secondary" noWrap>
              {evento.ubicacion}
            </Typography>
          </Box>

          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <EventSeatIcon fontSize="small" color="action" />
            <Chip
              label={`${evento.asientosDisponibles} / ${evento.capacidadTotal} disponibles`}
              size="small"
              color={getDisponibilidadColor()}
              variant="outlined"
            />
          </Box>
        </Stack>
      </CardContent>

      <CardActions sx={{ p: 2, pt: 0 }}>
        <Button size="small" onClick={handleVerDetalles}>
          Ver Detalles
        </Button>
        <Button
          size="small"
          variant="contained"
          onClick={handleComprar}
          disabled={evento.asientosDisponibles === 0}
        >
          {evento.asientosDisponibles === 0 ? 'Agotado' : 'Comprar'}
        </Button>
      </CardActions>
    </Card>
  );
}
