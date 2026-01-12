import React from 'react';
import { Typography, Box, Button } from '@mui/material';
import { useOrganizerEvents, useDeleteEvent, usePublishEvent } from '../api/events';
import EventTable from '../components/EventTable';
import { useNavigate } from 'react-router-dom';
import AddIcon from '@mui/icons-material/Add';

const OrganizerDashboard: React.FC = () => {
  const navigate = useNavigate();
  // Hardcoded ID for MVP as per instructions
  const organizerId = '123-temporal-organizer-id'; 
  const { data: events, isLoading, error } = useOrganizerEvents(organizerId);
  const deleteEventMutation = useDeleteEvent();
  const publishEventMutation = usePublishEvent();

  const handleEdit = (id: string) => {
    navigate(`/organizador/editar/${id}`);
  };

  const handleDelete = (id: string) => {
    if (window.confirm('¿Estás seguro de eliminar este evento?')) {
      deleteEventMutation.mutate(id);
    }
  };

  const handlePublish = (id: string) => {
    if (window.confirm('¿Estás seguro de publicar este evento?')) {
      publishEventMutation.mutate(id);
    }
  };

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 4 }}>
        <Typography variant="h4" component="h1">
          Panel de Organizador
        </Typography>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => navigate('/organizador/crear')}
        >
          Crear Evento
        </Button>
      </Box>

      {isLoading ? (
        <Typography>Cargando tus eventos...</Typography>
      ) : error ? (
        <Typography color="error">Error al cargar eventos.</Typography>
      ) : (
        <EventTable
          events={events || []}
          onEdit={handleEdit}
          onDelete={handleDelete}
          onPublish={handlePublish}
        />
      )}
    </Box>
  );
};

export default OrganizerDashboard;
