/**
 * UsuarioForm Component
 * Form for creating and editing usuarios (Admin only)
 * Includes validation for username, email, and phone
 */

import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Button,
  Stack,
  MenuItem,
  FormHelperText,
} from '@mui/material';
import {
  usuarioSchema,
  usuarioEditSchema,
  type UsuarioEditFormData,
} from '@/shared/validation';
import type { CreateUsuarioDto, Usuario, RolUsuario, UpdateUsuarioDto } from '../types';

interface UsuarioFormProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (data: CreateUsuarioDto | UpdateUsuarioDto) => void;
  usuario?: Usuario;
  isSubmitting?: boolean;
}

const roles: RolUsuario[] = ['Admin', 'Organizator', 'Asistente'];

export function UsuarioForm({
  open,
  onClose,
  onSubmit,
  usuario,
  isSubmitting = false,
}: UsuarioFormProps) {
  const isEditing = !!usuario;

  const {
    control,
    handleSubmit,
    formState: { errors, isValid },
    reset,
  } = useForm<UsuarioEditFormData>({
    resolver: zodResolver(isEditing ? usuarioEditSchema : usuarioSchema) as any,
    defaultValues: usuario
      ? {
          username: usuario.username,
          nombre: usuario.nombre,
          correo: usuario.correo,
          telefono: usuario.telefono,
          rol: usuario.rol,
          password: '',
        }
      : {
          username: '',
          nombre: '',
          correo: '',
          telefono: '',
          rol: 'Asistente' as RolUsuario,
          password: '',
        },
    mode: 'onChange',
  });

  const handleFormSubmit = (data: UsuarioEditFormData) => {
    if (isEditing) {
      // For editing, only send changed fields (UpdateUsuarioDto)
      const updateData: UpdateUsuarioDto = {
        nombre: data.nombre,
        correo: data.correo,
        telefono: data.telefono,
        rol: data.rol,
      };
      onSubmit(updateData);
    } else {
      // For creating, send all fields including password (CreateUsuarioDto)
      const createData: CreateUsuarioDto = {
        username: data.username!,
        nombre: data.nombre,
        correo: data.correo,
        telefono: data.telefono,
        rol: data.rol,
        password: data.password || '',
      };
      onSubmit(createData);
    }
  };

  const handleClose = () => {
    reset();
    onClose();
  };

  return (
    <Dialog
      open={open}
      onClose={handleClose}
      maxWidth="sm"
      fullWidth
      aria-labelledby="usuario-form-title"
    >
      <DialogTitle id="usuario-form-title">
        {isEditing ? 'Editar Usuario' : 'Crear Nuevo Usuario'}
      </DialogTitle>

      <form onSubmit={handleSubmit(handleFormSubmit)}>
        <DialogContent>
          <Stack spacing={2}>
            <Controller
              name="username"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  label="Username"
                  fullWidth
                  required
                  disabled={isEditing || isSubmitting}
                  error={!!errors.username}
                  helperText={
                    errors.username?.message ||
                    (isEditing ? 'El username no se puede modificar' : 'Único en el sistema')
                  }
                  inputProps={{
                    'aria-label': 'Username',
                  }}
                />
              )}
            />

            <Controller
              name="nombre"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  label="Nombre Completo"
                  fullWidth
                  required
                  error={!!errors.nombre}
                  helperText={errors.nombre?.message}
                  disabled={isSubmitting}
                  inputProps={{
                    'aria-label': 'Nombre completo',
                  }}
                />
              )}
            />

            <Controller
              name="correo"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  label="Correo Electrónico"
                  type="email"
                  fullWidth
                  required
                  error={!!errors.correo}
                  helperText={errors.correo?.message || 'Debe ser único en el sistema'}
                  disabled={isSubmitting}
                  inputProps={{
                    'aria-label': 'Correo electrónico',
                  }}
                />
              )}
            />

            <Controller
              name="telefono"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  label="Teléfono"
                  type="tel"
                  fullWidth
                  required
                  error={!!errors.telefono}
                  helperText={errors.telefono?.message || 'Formato: +1234567890 o 1234567890'}
                  disabled={isSubmitting}
                  inputProps={{
                    'aria-label': 'Teléfono',
                  }}
                />
              )}
            />

            <Controller
              name="rol"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  label="Rol"
                  select
                  fullWidth
                  required
                  error={!!errors.rol}
                  helperText={errors.rol?.message}
                  disabled={isSubmitting}
                  inputProps={{
                    'aria-label': 'Rol de usuario',
                  }}
                >
                  {roles.map((rol) => (
                    <MenuItem key={rol} value={rol}>
                      {rol}
                    </MenuItem>
                  ))}
                </TextField>
              )}
            />

            <Controller
              name="password"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  label={isEditing ? 'Nueva Contraseña (opcional)' : 'Contraseña'}
                  type="password"
                  fullWidth
                  required={!isEditing}
                  error={!!errors.password}
                  helperText={
                    errors.password?.message ||
                    (isEditing
                      ? 'Dejar en blanco para mantener la contraseña actual'
                      : 'Mínimo 8 caracteres, incluir mayúscula, minúscula y número')
                  }
                  disabled={isSubmitting}
                  inputProps={{
                    'aria-label': 'Contraseña',
                  }}
                />
              )}
            />

            {!isValid && Object.keys(errors).length > 0 && (
              <FormHelperText error>
                Por favor, corrija los errores antes de continuar
              </FormHelperText>
            )}
          </Stack>
        </DialogContent>

        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={handleClose} disabled={isSubmitting}>
            Cancelar
          </Button>
          <Button
            type="submit"
            variant="contained"
            disabled={!isValid || isSubmitting}
          >
            {isSubmitting ? 'Guardando...' : isEditing ? 'Actualizar' : 'Crear'}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
}
