/**
 * ReporteFiltros Component
 * Filter controls for reports (date range and event selection)
 */

import {
  Box,
  Paper,
  TextField,
  MenuItem,
  Grid,
  Button,
} from '@mui/material';
import { useState, useEffect } from 'react';
import type { ReporteFiltros as ReporteFiltrosType } from '../types';
import FilterListIcon from '@mui/icons-material/FilterList';
import ClearIcon from '@mui/icons-material/Clear';

interface ReporteFiltrosProps {
  filtros: ReporteFiltrosType;
  onFiltrosChange: (filtros: ReporteFiltrosType) => void;
  eventos?: Array<{ id: string; nombre: string }>;
  showEventoFilter?: boolean;
}

/**
 * ReporteFiltros - Filter controls for reports
 * Allows filtering by date range and event
 */
export function ReporteFiltros({
  filtros,
  onFiltrosChange,
  eventos = [],
  showEventoFilter = true,
}: ReporteFiltrosProps) {
  const [localFiltros, setLocalFiltros] = useState<ReporteFiltrosType>(filtros);

  useEffect(() => {
    setLocalFiltros(filtros);
  }, [filtros]);

  const handleFechaInicioChange = (
    event: React.ChangeEvent<HTMLInputElement>
  ) => {
    const newFiltros = {
      ...localFiltros,
      fechaInicio: event.target.value ? new Date(event.target.value) : undefined,
    };
    setLocalFiltros(newFiltros);
  };

  const handleFechaFinChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const newFiltros = {
      ...localFiltros,
      fechaFin: event.target.value ? new Date(event.target.value) : undefined,
    };
    setLocalFiltros(newFiltros);
  };

  const handleEventoChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const newFiltros = {
      ...localFiltros,
      eventoId: event.target.value || undefined,
    };
    setLocalFiltros(newFiltros);
  };

  const handleAplicar = () => {
    onFiltrosChange(localFiltros);
  };

  const handleLimpiar = () => {
    const emptyFiltros: ReporteFiltrosType = {};
    setLocalFiltros(emptyFiltros);
    onFiltrosChange(emptyFiltros);
  };

  const formatDateForInput = (date?: Date) => {
    if (!date) return '';
    const d = new Date(date);
    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  };

  return (
    <Paper sx={{ p: 3, mb: 3 }}>
      <Grid container spacing={2} alignItems="center">
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <TextField
            fullWidth
            label="Fecha Inicio"
            type="date"
            value={formatDateForInput(localFiltros.fechaInicio)}
            onChange={handleFechaInicioChange}
            InputLabelProps={{
              shrink: true,
            }}
          />
        </Grid>

        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <TextField
            fullWidth
            label="Fecha Fin"
            type="date"
            value={formatDateForInput(localFiltros.fechaFin)}
            onChange={handleFechaFinChange}
            InputLabelProps={{
              shrink: true,
            }}
          />
        </Grid>

        {showEventoFilter && (
          <Grid size={{ xs: 12, sm: 6, md: 3 }}>
            <TextField
              fullWidth
              select
              label="Evento"
              value={localFiltros.eventoId || ''}
              onChange={handleEventoChange}
            >
              <MenuItem value="">Todos los eventos</MenuItem>
              {eventos.map((evento) => (
                <MenuItem key={evento.id} value={evento.id}>
                  {evento.nombre}
                </MenuItem>
              ))}
            </TextField>
          </Grid>
        )}

        <Grid size={{ xs: 12, sm: 6, md: showEventoFilter ? 3 : 6 }}>
          <Box display="flex" gap={1}>
            <Button
              fullWidth
              variant="contained"
              startIcon={<FilterListIcon />}
              onClick={handleAplicar}
            >
              Aplicar
            </Button>
            <Button
              fullWidth
              variant="outlined"
              startIcon={<ClearIcon />}
              onClick={handleLimpiar}
            >
              Limpiar
            </Button>
          </Box>
        </Grid>
      </Grid>
    </Paper>
  );
}
