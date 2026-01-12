/**
 * UsuariosPage - User management (Admin only)
 * Displays list of usuarios with CRUD operations
 */

import { useState } from 'react';
import {
  Container,
  Box,
  Typography,
  Button,
  Stack,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogContentText,
  DialogActions,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import { useUsuarios, useCreateUsuario, useUpdateUsuario, useDeactivateUsuario } from '../hooks';
import { UsuariosList, UsuarioForm } from '../components';
import { ErrorMessage } from '@shared/components';
import type { CreateUsuarioDto, Usuario, UpdateUsuarioDto } from '../types';

export function UsuariosPage() {
  const { data: usuarios = [], isLoading, error, refetch } = useUsuarios();
  const { mutate: createUsuario, isPending: isCreating } = useCreateUsuario();
  const { mutate: updateUsuario, isPending: isUpdating } = useUpdateUsuario();
  const { mutate: deactivateUsuario, isPending: isDeactivating } = useDeactivateUsuario();

  const [formOpen, setFormOpen] = useState(false);
  const [editingUsuario, setEditingUsuario] = useState<Usuario | undefined>();
  const [confirmDialogOpen, setConfirmDialogOpen] = useState(false);
  const [usuarioToDeactivate, setUsuarioToDeactivate] = useState<Usuario | undefined>();

  const handleCreateClick = () => {
    setEditingUsuario(undefined);
    setFormOpen(true);
  };

  const handleEditClick = (usuario: Usuario) => {
    setEditingUsuario(usuario);
    setFormOpen(true);
  };

  const handleFormClose = () => {
    setFormOpen(false);
    setEditingUsuario(undefined);
  };

  const handleFormSubmit = (data: CreateUsuarioDto | UpdateUsuarioDto) => {
    if (editingUsuario) {
      // Update existing usuario
      updateUsuario(
        { id: editingUsuario.id, data: data as UpdateUsuarioDto },
        {
          onSuccess: () => {
            handleFormClose();
            // Toast notification will be shown by mutation hook
          },
        }
      );
    } else {
      // Create new usuario
      createUsuario(data as CreateUsuarioDto, {
        onSuccess: () => {
          handleFormClose();
          // Toast notification will be shown by mutation hook
        },
      });
    }
  };

  const handleDeactivateClick = (usuario: Usuario) => {
    setUsuarioToDeactivate(usuario);
    setConfirmDialogOpen(true);
  };

  const handleConfirmDeactivate = () => {
    if (usuarioToDeactivate) {
      deactivateUsuario(usuarioToDeactivate.id, {
        onSuccess: () => {
          setConfirmDialogOpen(false);
          setUsuarioToDeactivate(undefined);
          // Toast notification will be shown by mutation hook
        },
      });
    }
  };

  const handleCancelDeactivate = () => {
    setConfirmDialogOpen(false);
    setUsuarioToDeactivate(undefined);
  };

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
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
            Gestión de Usuarios
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Administra usuarios del sistema y sus permisos
          </Typography>
        </Box>

        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={handleCreateClick}
          sx={{ minWidth: { sm: 150 } }}
        >
          Crear Usuario
        </Button>
      </Stack>

      {/* Error Message */}
      {error && (
        <Box sx={{ mb: 3 }}>
          <ErrorMessage error={error} onRetry={refetch} />
        </Box>
      )}

      {/* Usuarios List */}
      <UsuariosList
        usuarios={usuarios}
        isLoading={isLoading}
        onEdit={handleEditClick}
        onDeactivate={handleDeactivateClick}
      />

      {/* Create/Edit Form Dialog */}
      <UsuarioForm
        open={formOpen}
        onClose={handleFormClose}
        onSubmit={handleFormSubmit}
        usuario={editingUsuario}
        isSubmitting={isCreating || isUpdating}
      />

      {/* Deactivate Confirmation Dialog */}
      <Dialog
        open={confirmDialogOpen}
        onClose={handleCancelDeactivate}
        aria-labelledby="confirm-dialog-title"
        aria-describedby="confirm-dialog-description"
      >
        <DialogTitle id="confirm-dialog-title">Confirmar Desactivación</DialogTitle>
        <DialogContent>
          <DialogContentText id="confirm-dialog-description">
            ¿Está seguro que desea desactivar al usuario{' '}
            <strong>{usuarioToDeactivate?.username}</strong>?
            <br />
            <br />
            El usuario no podrá acceder al sistema hasta que sea reactivado.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCancelDeactivate} disabled={isDeactivating}>
            Cancelar
          </Button>
          <Button
            onClick={handleConfirmDeactivate}
            color="error"
            variant="contained"
            disabled={isDeactivating}
            autoFocus
          >
            {isDeactivating ? 'Desactivando...' : 'Desactivar'}
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
}

