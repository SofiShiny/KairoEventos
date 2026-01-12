/**
 * UsuariosList Component
 * Displays a table of usuarios with action buttons (Admin only)
 */

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
  Tooltip,
  Box,
  Typography,
} from '@mui/material';
import EditIcon from '@mui/icons-material/Edit';
import PersonOffIcon from '@mui/icons-material/PersonOff';
import { EmptyState, LoadingSpinner } from '@shared/components';
import PeopleIcon from '@mui/icons-material/People';
import type { Usuario } from '../types';

interface UsuariosListProps {
  usuarios: Usuario[];
  isLoading?: boolean;
  onEdit?: (usuario: Usuario) => void;
  onDeactivate?: (usuario: Usuario) => void;
}

export function UsuariosList({
  usuarios,
  isLoading,
  onEdit,
  onDeactivate,
}: UsuariosListProps) {
  if (isLoading) {
    return <LoadingSpinner size="large" />;
  }

  if (usuarios.length === 0) {
    return (
      <EmptyState
        icon={<PeopleIcon />}
        title="No hay usuarios registrados"
        description="Crea el primer usuario para comenzar."
      />
    );
  }

  const getRolColor = (rol: string) => {
    switch (rol) {
      case 'Admin':
        return 'error';
      case 'Organizator':
        return 'warning';
      case 'Asistente':
        return 'info';
      default:
        return 'default';
    }
  };

  return (
    <Box>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
        {usuarios.length} {usuarios.length === 1 ? 'usuario registrado' : 'usuarios registrados'}
      </Typography>

      <TableContainer component={Paper} elevation={2}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Username</TableCell>
              <TableCell>Nombre</TableCell>
              <TableCell>Correo</TableCell>
              <TableCell>Teléfono</TableCell>
              <TableCell>Rol</TableCell>
              <TableCell>Estado</TableCell>
              <TableCell align="right">Acciones</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {usuarios.map((usuario) => (
              <TableRow
                key={usuario.id}
                sx={{
                  '&:last-child td, &:last-child th': { border: 0 },
                  opacity: usuario.activo ? 1 : 0.6,
                }}
              >
                <TableCell>
                  <Typography variant="body2" fontWeight="medium">
                    {usuario.username}
                  </Typography>
                </TableCell>
                <TableCell>{usuario.nombre}</TableCell>
                <TableCell>{usuario.correo}</TableCell>
                <TableCell>{usuario.telefono}</TableCell>
                <TableCell>
                  <Chip
                    label={usuario.rol}
                    color={getRolColor(usuario.rol)}
                    size="small"
                  />
                </TableCell>
                <TableCell>
                  <Chip
                    label={usuario.activo ? 'Activo' : 'Inactivo'}
                    color={usuario.activo ? 'success' : 'default'}
                    size="small"
                    variant={usuario.activo ? 'filled' : 'outlined'}
                  />
                </TableCell>
                <TableCell align="right">
                  <Tooltip title="Editar usuario">
                    <IconButton
                      size="small"
                      onClick={() => onEdit?.(usuario)}
                      disabled={!usuario.activo}
                      aria-label={`Editar ${usuario.username}`}
                    >
                      <EditIcon fontSize="small" />
                    </IconButton>
                  </Tooltip>
                  <Tooltip title="Desactivar usuario">
                    <IconButton
                      size="small"
                      onClick={() => onDeactivate?.(usuario)}
                      disabled={!usuario.activo}
                      color="error"
                      aria-label={`Desactivar ${usuario.username}`}
                    >
                      <PersonOffIcon fontSize="small" />
                    </IconButton>
                  </Tooltip>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
    </Box>
  );
}
