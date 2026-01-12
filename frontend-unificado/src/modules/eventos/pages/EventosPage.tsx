/**
 * EventosPage
 * Main page for browsing and managing eventos
 * Displays list of eventos with filtering, search, and CRUD operations
 */

import { useState, useMemo } from 'react';
import { Box, Typography, Button, Stack } from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import { useEventos, useCreateEvento, useUpdateEvento } from '../hooks';
import { EventosList, EventoFilters, EventoForm } from '../components';
import { ErrorMessage } from '@shared/components';
import { useAuth } from '@/context/AuthContext';
import { useNavigate } from 'react-router-dom';
import type { EventoFiltersData, CreateEventoDto, Evento } from '../types';

export function EventosPage() {
  const navigate = useNavigate();
  const { hasRole } = useAuth();
  const { data: eventos = [], isLoading, error, refetch } = useEventos();
  const { mutate: createEvento, isPending: isCreating } = useCreateEvento();
  const { mutate: updateEvento, isPending: isUpdating } = useUpdateEvento();

  const [filters, setFilters] = useState<EventoFiltersData>({});
  const [formOpen, setFormOpen] = useState(false);
  const [editingEvento, setEditingEvento] = useState<Evento | undefined>();

  // Check if user can manage eventos (Admin or Organizator)
  const canManageEventos = hasRole('Admin') || hasRole('Organizator');

  // Filter eventos based on search and filters
  const filteredEventos = useMemo(() => {
    let result = [...eventos];

    // Filter by search term
    if (filters.busqueda) {
      const searchLower = filters.busqueda.toLowerCase();
      result = result.filter((evento) =>
        evento.nombre.toLowerCase().includes(searchLower) ||
        evento.descripcion.toLowerCase().includes(searchLower)
      );
    }

    // Filter by date
    if (filters.fecha) {
      const filterDate = filters.fecha.toISOString().split('T')[0];
      result = result.filter((evento) => {
        const eventoDate = new Date(evento.fecha).toISOString().split('T')[0];
        return eventoDate === filterDate;
      });
    }

    // Filter by location
    if (filters.ubicacion) {
      const ubicacionLower = filters.ubicacion.toLowerCase();
      result = result.filter((evento) =>
        evento.ubicacion.toLowerCase().includes(ubicacionLower)
      );
    }

    // Sort by date (upcoming first)
    result.sort((a, b) => new Date(a.fecha).getTime() - new Date(b.fecha).getTime());

    return result;
  }, [eventos, filters]);

  const handleCreateClick = () => {
    setEditingEvento(undefined);
    setFormOpen(true);
  };

  const handleFormClose = () => {
    setFormOpen(false);
    setEditingEvento(undefined);
  };

  const handleFormSubmit = (data: CreateEventoDto) => {
    if (editingEvento) {
      // Update existing evento
      updateEvento(
        { id: editingEvento.id, data },
        {
          onSuccess: () => {
            handleFormClose();
            // Toast notification will be shown by mutation hook
          },
        }
      );
    } else {
      // Create new evento
      createEvento(data, {
        onSuccess: () => {
          handleFormClose();
          // Toast notification will be shown by mutation hook
        },
      });
    }
  };

  const handleEventoClick = (eventoId: string) => {
    navigate(`/eventos/${eventoId}`);
  };

  return (
    <Box>
      {/* Header */}
      <Stack
        direction={{ xs: 'column', sm: 'row' }}
        justifyContent="space-between"
        alignItems={{ xs: 'stretch', sm: 'center' }}
        spacing={2}
        sx={{ mb: 3 }}
      >
        <Box>
          <Typography variant="h4" component="h1" gutterBottom>
            Eventos
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Explora y descubre eventos disponibles
          </Typography>
        </Box>

        {canManageEventos && (
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={handleCreateClick}
            sx={{ minWidth: { sm: 150 } }}
          >
            Crear Evento
          </Button>
        )}
      </Stack>

      {/* Error Message */}
      {error && (
        <Box sx={{ mb: 3 }}>
          <ErrorMessage error={error} onRetry={refetch} />
        </Box>
      )}

      {/* Filters */}
      <EventoFilters onFiltersChange={setFilters} />

      {/* Eventos List */}
      <EventosList
        eventos={filteredEventos}
        isLoading={isLoading}
        onEventoClick={handleEventoClick}
      />

      {/* Create/Edit Form Dialog */}
      <EventoForm
        open={formOpen}
        onClose={handleFormClose}
        onSubmit={handleFormSubmit}
        evento={editingEvento}
        isSubmitting={isCreating || isUpdating}
      />
    </Box>
  );
}


