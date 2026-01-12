/**
 * MapaAsientos Component
 * Displays a seat map showing available, reserved, and occupied seats
 * Allows seat selection for ticket purchase
 */

import {
  Box,
  Paper,
  Typography,
  Button,
  Chip,
  Stack,
  useTheme,
  useMediaQuery,
} from '@mui/material';
import {
  EventSeat as SeatIcon,
  CheckCircle as CheckIcon,
} from '@mui/icons-material';
import { useState } from 'react';
import type { Asiento } from '../types';

interface MapaAsientosProps {
  asientos: Asiento[];
  onAsientoSelect: (asientoId: string) => void;
  selectedAsientoId?: string;
}

/**
 * Get color for seat based on status
 */
function getSeatColor(estado: Asiento['estado'], isSelected: boolean): string {
  if (isSelected) return '#1976d2'; // Primary blue
  switch (estado) {
    case 'Disponible':
      return '#4caf50'; // Green
    case 'Reservado':
      return '#ff9800'; // Orange
    case 'Ocupado':
      return '#f44336'; // Red
    default:
      return '#9e9e9e'; // Grey
  }
}

/**
 * Group seats by row
 */
function groupSeatsByRow(asientos: Asiento[]): Map<string, Asiento[]> {
  const grouped = new Map<string, Asiento[]>();
  
  asientos.forEach((asiento) => {
    if (!grouped.has(asiento.fila)) {
      grouped.set(asiento.fila, []);
    }
    grouped.get(asiento.fila)!.push(asiento);
  });

  // Sort seats within each row by number
  grouped.forEach((seats) => {
    seats.sort((a, b) => a.numero - b.numero);
  });

  return grouped;
}

export function MapaAsientos({
  asientos,
  onAsientoSelect,
  selectedAsientoId,
}: MapaAsientosProps) {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));
  const [hoveredSeatId, setHoveredSeatId] = useState<string | null>(null);

  const seatsByRow = groupSeatsByRow(asientos);
  const sortedRows = Array.from(seatsByRow.keys()).sort();

  const handleSeatClick = (asiento: Asiento) => {
    if (asiento.estado === 'Disponible') {
      onAsientoSelect(asiento.id);
    }
  };

  const selectedSeat = asientos.find((a) => a.id === selectedAsientoId);

  return (
    <Box>
      {/* Legend */}
      <Paper elevation={1} sx={{ p: 2, mb: 3 }}>
        <Typography variant="subtitle2" gutterBottom>
          Leyenda:
        </Typography>
        <Stack direction="row" spacing={2} flexWrap="wrap" useFlexGap>
          <Chip
            icon={<SeatIcon />}
            label="Disponible"
            size="small"
            sx={{ bgcolor: '#4caf50', color: 'white' }}
          />
          <Chip
            icon={<SeatIcon />}
            label="Reservado"
            size="small"
            sx={{ bgcolor: '#ff9800', color: 'white' }}
          />
          <Chip
            icon={<SeatIcon />}
            label="Ocupado"
            size="small"
            sx={{ bgcolor: '#f44336', color: 'white' }}
          />
          <Chip
            icon={<CheckIcon />}
            label="Seleccionado"
            size="small"
            sx={{ bgcolor: '#1976d2', color: 'white' }}
          />
        </Stack>
      </Paper>

      {/* Selected Seat Info */}
      {selectedSeat && (
        <Paper elevation={2} sx={{ p: 2, mb: 3, bgcolor: 'primary.light', color: 'white' }}>
          <Typography variant="h6" gutterBottom>
            Asiento Seleccionado
          </Typography>
          <Typography variant="body1">
            Fila {selectedSeat.fila}, Asiento {selectedSeat.numero}
          </Typography>
          <Typography variant="h5" sx={{ mt: 1 }}>
            ${selectedSeat.precio.toFixed(2)}
          </Typography>
        </Paper>
      )}

      {/* Seat Map */}
      <Paper elevation={2} sx={{ p: 3, overflow: 'auto' }}>
        {/* Stage */}
        <Box
          sx={{
            bgcolor: 'grey.300',
            p: 2,
            mb: 4,
            textAlign: 'center',
            borderRadius: 1,
          }}
        >
          <Typography variant="h6" color="text.secondary">
            ESCENARIO
          </Typography>
        </Box>

        {/* Rows */}
        <Stack spacing={2}>
          {sortedRows.map((fila) => {
            const seats = seatsByRow.get(fila)!;
            return (
              <Box key={fila}>
                <Box sx={{ display: 'flex', gap: 1, alignItems: 'center' }}>
                  {/* Row Label */}
                  <Box sx={{ minWidth: 40 }}>
                    <Typography
                      variant="subtitle2"
                      fontWeight="bold"
                      textAlign="center"
                    >
                      {fila}
                    </Typography>
                  </Box>

                  {/* Seats */}
                  <Box
                    sx={{
                      display: 'flex',
                      gap: isMobile ? 0.5 : 1,
                      flexWrap: 'wrap',
                      justifyContent: 'center',
                      flex: 1,
                    }}
                  >
                    {seats.map((asiento) => {
                      const isSelected = asiento.id === selectedAsientoId;
                      const isHovered = asiento.id === hoveredSeatId;
                      const isClickable = asiento.estado === 'Disponible';

                      return (
                        <Button
                          key={asiento.id}
                          variant="contained"
                          size={isMobile ? 'small' : 'medium'}
                          disabled={!isClickable}
                          onClick={() => handleSeatClick(asiento)}
                          onMouseEnter={() => setHoveredSeatId(asiento.id)}
                          onMouseLeave={() => setHoveredSeatId(null)}
                          sx={{
                            minWidth: isMobile ? 32 : 48,
                            minHeight: isMobile ? 32 : 48,
                            p: isMobile ? 0.5 : 1,
                            bgcolor: getSeatColor(asiento.estado, isSelected),
                            '&:hover': {
                              bgcolor: isClickable
                                ? '#2e7d32'
                                : getSeatColor(asiento.estado, isSelected),
                              transform: isClickable ? 'scale(1.1)' : 'none',
                            },
                            '&.Mui-disabled': {
                              bgcolor: getSeatColor(asiento.estado, isSelected),
                              color: 'white',
                              opacity: 0.7,
                            },
                            transition: 'all 0.2s',
                            transform: isHovered && isClickable ? 'scale(1.1)' : 'scale(1)',
                            boxShadow: isSelected ? 4 : 1,
                          }}
                        >
                          {asiento.numero}
                        </Button>
                      );
                    })}
                  </Box>
                </Box>
              </Box>
            );
          })}
        </Stack>

        {/* Summary */}
        <Box sx={{ mt: 4, pt: 3, borderTop: 1, borderColor: 'divider' }}>
          <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
            <Box sx={{ flex: 1, minWidth: 150 }}>
              <Typography variant="body2" color="text.secondary">
                Disponibles: {asientos.filter((a) => a.estado === 'Disponible').length}
              </Typography>
            </Box>
            <Box sx={{ flex: 1, minWidth: 150 }}>
              <Typography variant="body2" color="text.secondary">
                Reservados: {asientos.filter((a) => a.estado === 'Reservado').length}
              </Typography>
            </Box>
            <Box sx={{ flex: 1, minWidth: 150 }}>
              <Typography variant="body2" color="text.secondary">
                Ocupados: {asientos.filter((a) => a.estado === 'Ocupado').length}
              </Typography>
            </Box>
          </Box>
        </Box>
      </Paper>
    </Box>
  );
}
