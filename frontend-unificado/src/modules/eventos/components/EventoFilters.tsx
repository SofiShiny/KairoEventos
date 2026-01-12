/**
 * EventoFilters Component
 * Provides filtering and search controls for eventos
 */

import { useState } from 'react';
import {
  TextField,
  InputAdornment,
  Stack,
  Paper,
} from '@mui/material';
import SearchIcon from '@mui/icons-material/Search';
import type { EventoFiltersData } from '../types';

interface EventoFiltersProps {
  onFiltersChange: (filters: EventoFiltersData) => void;
}

export function EventoFilters({ onFiltersChange }: EventoFiltersProps) {
  const [busqueda, setBusqueda] = useState('');
  const [fecha, setFecha] = useState('');
  const [ubicacion, setUbicacion] = useState('');

  const handleBusquedaChange = (value: string) => {
    setBusqueda(value);
    onFiltersChange({
      busqueda: value || undefined,
      fecha: fecha ? new Date(fecha) : undefined,
      ubicacion: ubicacion || undefined,
    });
  };

  const handleFechaChange = (value: string) => {
    setFecha(value);
    onFiltersChange({
      busqueda: busqueda || undefined,
      fecha: value ? new Date(value) : undefined,
      ubicacion: ubicacion || undefined,
    });
  };

  const handleUbicacionChange = (value: string) => {
    setUbicacion(value);
    onFiltersChange({
      busqueda: busqueda || undefined,
      fecha: fecha ? new Date(fecha) : undefined,
      ubicacion: value || undefined,
    });
  };

  return (
    <Paper elevation={0} sx={{ p: 2, mb: 3, bgcolor: 'background.default' }}>
      <Stack
        direction={{ xs: 'column', sm: 'row' }}
        spacing={2}
        alignItems="stretch"
      >
        <TextField
          fullWidth
          placeholder="Buscar eventos por nombre..."
          value={busqueda}
          onChange={(e) => handleBusquedaChange(e.target.value)}
          InputProps={{
            startAdornment: (
              <InputAdornment position="start">
                <SearchIcon />
              </InputAdornment>
            ),
          }}
          size="small"
        />

        <TextField
          label="Fecha"
          type="date"
          value={fecha}
          onChange={(e) => handleFechaChange(e.target.value)}
          InputLabelProps={{
            shrink: true,
          }}
          size="small"
          sx={{ minWidth: { sm: 200 } }}
        />

        <TextField
          label="Ubicación"
          placeholder="Filtrar por ubicación"
          value={ubicacion}
          onChange={(e) => handleUbicacionChange(e.target.value)}
          size="small"
          sx={{ minWidth: { sm: 200 } }}
        />
      </Stack>
    </Paper>
  );
}
