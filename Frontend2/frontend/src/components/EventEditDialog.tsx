import React, { useState } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  Tabs,
  Tab,
  Box,
  IconButton,
  Typography,
} from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import SeatManagementTab from './SeatManagementTab';
import { useEvent } from '../api/events';

interface EventEditDialogProps {
  open: boolean;
  onClose: () => void;
  eventoId: string;
}

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

const TabPanel: React.FC<TabPanelProps> = ({ children, value, index }) => {
  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`event-edit-tabpanel-${index}`}
      aria-labelledby={`event-edit-tab-${index}`}
    >
      {value === index && <Box sx={{ p: 3 }}>{children}</Box>}
    </div>
  );
};

const EventEditDialog: React.FC<EventEditDialogProps> = ({ open, onClose, eventoId }) => {
  const [currentTab, setCurrentTab] = useState(0);
  const { data: event, isLoading } = useEvent(eventoId);

  const handleTabChange = (_event: React.SyntheticEvent, newValue: number) => {
    setCurrentTab(newValue);
  };

  // Get mapaId from localStorage (set during event creation)
  const mapaId = localStorage.getItem(`map_for_event_${eventoId}`) || '';
  
  // Debug: log mapaId
  React.useEffect(() => {
    console.log('[EventEditDialog] eventoId:', eventoId);
    console.log('[EventEditDialog] mapaId from localStorage:', mapaId);
  }, [eventoId, mapaId]);

  return (
    <Dialog
      open={open}
      onClose={onClose}
      maxWidth="xl"
      fullWidth
      fullScreen={false}
      aria-labelledby="event-edit-dialog-title"
      sx={{
        '& .MuiDialog-paper': {
          height: { xs: '100%', md: '90vh' },
          maxHeight: { xs: '100%', md: '90vh' },
        }
      }}
    >
      <DialogTitle id="event-edit-dialog-title">
        Editar Evento: {isLoading ? 'Cargando...' : event?.nombre}
        <IconButton
          aria-label="Cerrar diálogo"
          onClick={onClose}
          sx={{
            position: 'absolute',
            right: 8,
            top: 8,
            color: 'text.secondary',
          }}
        >
          <CloseIcon />
        </IconButton>
      </DialogTitle>
      <DialogContent dividers sx={{ p: { xs: 1, sm: 2 } }}>
        <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
          <Tabs
            value={currentTab}
            onChange={handleTabChange}
            aria-label="Pestañas de edición de evento"
            variant="scrollable"
            scrollButtons="auto"
            allowScrollButtonsMobile
          >
            <Tab 
              label="Detalles del Evento" 
              id="event-edit-tab-0"
              aria-controls="event-edit-tabpanel-0"
            />
            <Tab 
              label="Gestión de Asientos" 
              id="event-edit-tab-1"
              aria-controls="event-edit-tabpanel-1"
            />
          </Tabs>
        </Box>
        <TabPanel value={currentTab} index={0}>
          <Box sx={{ p: { xs: 1, sm: 2 } }}>
            <Typography variant="body1" color="text.secondary">
              Detalles del evento (por implementar)
            </Typography>
          </Box>
        </TabPanel>
        <TabPanel value={currentTab} index={1}>
          {mapaId ? (
            <SeatManagementTab eventoId={eventoId} mapaId={mapaId} />
          ) : (
            <Box 
              sx={{ 
                p: 3, 
                textAlign: 'center',
                backgroundColor: 'action.hover',
                borderRadius: 1
              }}
            >
              <Typography variant="body1" color="error">
                No se encontró el mapa de asientos para este evento.
              </Typography>
              <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
                Por favor, asegúrese de que el evento tenga un mapa de asientos asociado.
              </Typography>
            </Box>
          )}
        </TabPanel>
      </DialogContent>
    </Dialog>
  );
};

export default EventEditDialog;
