import React from 'react';
import {
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  IconButton,
  Chip,
} from '@mui/material';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import PublishIcon from '@mui/icons-material/Publish';
import type { Evento } from '../types/api';

interface EventTableProps {
  events: Evento[];
  onEdit: (id: string) => void;
  onDelete: (id: string) => void;
  onPublish: (id: string) => void;
}

const EventTable: React.FC<EventTableProps> = ({ events, onEdit, onDelete, onPublish }) => {
  return (
    <TableContainer component={Paper}>
      <Table sx={{ minWidth: 650 }} aria-label="simple table">
        <TableHead>
          <TableRow>
            <TableCell>Título</TableCell>
            <TableCell>Fecha</TableCell>
            <TableCell>Ubicación</TableCell>
            <TableCell>Estado</TableCell>
            <TableCell align="right">Acciones</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {events.map((event) => (
            <TableRow
              key={event.id}
              sx={{ '&:last-child td, &:last-child th': { border: 0 } }}
            >
              <TableCell component="th" scope="row">
                {event.titulo}
              </TableCell>
              <TableCell>{new Date(event.fechaInicio).toLocaleDateString()}</TableCell>
              <TableCell>{event.ubicacion.nombreLugar}</TableCell>
              <TableCell>
                <Chip
                  label={event.publicado ? 'Publicado' : 'Borrador'}
                  color={event.publicado ? 'success' : 'default'}
                  size="small"
                />
              </TableCell>
              <TableCell align="right">
                {!event.publicado && (
                  <IconButton onClick={() => onPublish(event.id)} title="Publicar">
                    <PublishIcon />
                  </IconButton>
                )}
                <IconButton onClick={() => onEdit(event.id)} title="Editar">
                  <EditIcon />
                </IconButton>
                <IconButton onClick={() => onDelete(event.id)} color="error" title="Eliminar">
                  <DeleteIcon />
                </IconButton>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
};

export default EventTable;
