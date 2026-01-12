import React from 'react';
import { Typography, Box, Button } from '@mui/material';
import { useAdminEvents, useDeleteEvent, usePublishEvent } from '../api/events';
import EventTable from '../components/EventTable';
import { useNavigate } from 'react-router-dom';
import AddIcon from '@mui/icons-material/Add';

const AdminDashboard: React.FC = () => {
  const navigate = useNavigate();
  const { data: events, isLoading, error } = useAdminEvents();
  const deleteEventMutation = useDeleteEvent();
  const publishEventMutation = usePublishEvent();

  const handleEdit = (id: string) => {
    navigate(`/admin/editar/${id}`);
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
          Panel de Administración
        </Typography>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => navigate('/admin/crear')}
        >
          Crear Evento
        </Button>
      </Box>

      {isLoading ? (
        <Typography>Cargando eventos...</Typography>
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

export default AdminDashboard;
