/**
 * EntradaFilters Component
 * Filter controls for entradas list by status
 */

import { Box, ToggleButtonGroup, ToggleButton } from '@mui/material';
import type { EstadoEntrada } from '../types';

interface EntradaFiltersProps {
  selectedEstado: EstadoEntrada | 'Todas';
  onEstadoChange: (estado: EstadoEntrada | 'Todas') => void;
}

export function EntradaFilters({ selectedEstado, onEstadoChange }: EntradaFiltersProps) {
  const handleChange = (_event: React.MouseEvent<HTMLElement>, newEstado: EstadoEntrada | 'Todas' | null) => {
    if (newEstado !== null) {
      onEstadoChange(newEstado);
    }
  };

  return (
    <Box sx={{ mb: 3 }}>
      <ToggleButtonGroup
        value={selectedEstado}
        exclusive
        onChange={handleChange}
        aria-label="filtro de estado de entradas"
        size="small"
        fullWidth
        sx={{
          '& .MuiToggleButton-root': {
            textTransform: 'none',
          },
        }}
      >
        <ToggleButton value="Todas" aria-label="todas las entradas">
          Todas
        </ToggleButton>
        <ToggleButton value="Reservada" aria-label="entradas reservadas">
          Reservadas
        </ToggleButton>
        <ToggleButton value="Pagada" aria-label="entradas pagadas">
          Pagadas
        </ToggleButton>
        <ToggleButton value="Cancelada" aria-label="entradas canceladas">
          Canceladas
        </ToggleButton>
      </ToggleButtonGroup>
    </Box>
  );
}
