import React, { useState } from 'react';
import {
  Box,
  Paper,
  Typography,
  TextField,
  Button,
  List,
  ListItem,
  ListItemText,
  CircularProgress,
  Alert,
  Snackbar,
  Checkbox,
  FormControlLabel,
} from '@mui/material';
import { useCreateCategory, useEventCategories } from '../api/seats';
import type { CategoryCreateDto } from '../types/api';

interface CategoryManagementSectionProps {
  eventoId: string;
  mapaId: string;
}

interface FormErrors {
  nombre?: string;
  precio?: string;
}

const CategoryManagementSection: React.FC<CategoryManagementSectionProps> = ({ eventoId, mapaId }) => {
  const [nombre, setNombre] = useState('');
  const [precioBase, setPrecioBase] = useState('');
  const [tienePrioridad, setTienePrioridad] = useState(false);
  const [errors, setErrors] = useState<FormErrors>({});
  const [showSuccess, setShowSuccess] = useState(false);
  const [showError, setShowError] = useState(false);
  const [errorMessage, setErrorMessage] = useState('');

  const { mutate: createCategory, isPending } = useCreateCategory();
  const { data: categories, isLoading: categoriesLoading } = useEventCategories(mapaId);

  const validateForm = (): boolean => {
    const newErrors: FormErrors = {};

    if (!nombre || nombre.trim().length === 0) {
      newErrors.nombre = 'El nombre es requerido';
    } else if (nombre.length < 2) {
      newErrors.nombre = 'El nombre debe tener al menos 2 caracteres';
    } else if (nombre.length > 50) {
      newErrors.nombre = 'El nombre no puede exceder 50 caracteres';
    }

    const precioNum = parseFloat(precioBase);
    if (!precioBase || precioBase.trim().length === 0) {
      newErrors.precio = 'El precio es requerido';
    } else if (isNaN(precioNum)) {
      newErrors.precio = 'El precio debe ser un número válido';
    } else if (precioNum <= 0) {
      newErrors.precio = 'El precio debe ser mayor a 0';
    } else if (precioNum > 10000) {
      newErrors.precio = 'El precio no puede exceder 10000';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    if (!validateForm()) {
      return;
    }

    const categoryData: CategoryCreateDto = {
      nombre: nombre.trim(),
      precioBase: parseFloat(precioBase),
      tienePrioridad,
      mapaId,
    };

    createCategory(categoryData, {
      onSuccess: () => {
        // Clear form
        setNombre('');
        setPrecioBase('');
        setTienePrioridad(false);
        setErrors({});
        setShowSuccess(true);
      },
      onError: (error: any) => {
        // Extract error message from backend response
        let message = 'Error al crear la categoría';
        
        if (error?.response?.data) {
          const errorData = error.response.data;
          
          // Check for exception message (C# backend format)
          if (typeof errorData === 'string') {
            // Parse exception message if it's a string
            const match = errorData.match(/System\.InvalidOperationException:\s*(.+?)(?:\s+at|$)/);
            if (match) {
              message = match[1];
            } else {
              message = errorData;
            }
          } else if (errorData.message) {
            message = errorData.message;
          } else if (errorData.title) {
            message = errorData.title;
          }
        }
        
        setErrorMessage(message);
        setShowError(true);
      },
    });
  };

  return (
    <Paper elevation={2} sx={{ p: { xs: 2, sm: 3 }, mb: 3 }}>
      <Typography variant="h6" gutterBottom>
        Gestión de Categorías
      </Typography>

      <Box component="form" onSubmit={handleSubmit} sx={{ mb: 3 }} noValidate>
        <TextField
          fullWidth
          label="Nombre"
          value={nombre}
          onChange={(e) => setNombre(e.target.value)}
          error={!!errors.nombre}
          helperText={errors.nombre || 'Ingrese el nombre de la categoría (2-50 caracteres)'}
          margin="normal"
          required
          disabled={isPending}
          autoComplete="off"
          inputProps={{
            'aria-label': 'Nombre de la categoría',
            'aria-required': 'true',
            'aria-invalid': !!errors.nombre,
          }}
        />

        <TextField
          fullWidth
          label="Precio Base"
          value={precioBase}
          onChange={(e) => setPrecioBase(e.target.value)}
          error={!!errors.precio}
          helperText={errors.precio || 'Ingrese el precio base en formato decimal (ej: 50.00)'}
          margin="normal"
          required
          type="number"
          inputProps={{ 
            step: '0.01', 
            min: '0',
            max: '10000',
            'aria-label': 'Precio base de la categoría',
            'aria-required': 'true',
            'aria-invalid': !!errors.precio,
          }}
          disabled={isPending}
          autoComplete="off"
        />

        <Box sx={{ display: 'flex', alignItems: 'center', mt: 2 }}>
          <FormControlLabel
            control={
              <Checkbox
                checked={tienePrioridad}
                onChange={(e) => setTienePrioridad(e.target.checked)}
                disabled={isPending}
              />
            }
            label="Tiene prioridad"
          />
        </Box>

        <Button
          type="submit"
          variant="contained"
          color="primary"
          fullWidth
          sx={{ mt: 2, minHeight: 42 }}
          disabled={isPending}
          aria-label="Crear nueva categoría"
        >
          {isPending ? (
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              <CircularProgress size={20} color="inherit" />
              <span>Creando...</span>
            </Box>
          ) : (
            'Crear Categoría'
          )}
        </Button>
      </Box>

      <Typography variant="subtitle1" gutterBottom sx={{ mt: 2 }}>
        Categorías Existentes
      </Typography>

      {categoriesLoading ? (
        <Box 
          sx={{ 
            display: 'flex', 
            flexDirection: 'column',
            alignItems: 'center',
            justifyContent: 'center', 
            p: 3,
            gap: 1
          }}
          role="status"
          aria-live="polite"
          aria-label="Cargando categorías"
        >
          <CircularProgress size={32} />
          <Typography variant="body2" color="text.secondary">
            Cargando categorías...
          </Typography>
        </Box>
      ) : categories && categories.length > 0 ? (
        <List sx={{ maxHeight: { xs: 300, sm: 400 }, overflow: 'auto' }}>
          {categories.map((category) => (
            <ListItem 
              key={category.categoriaId || category.id} 
              divider
              sx={{
                '&:hover': {
                  backgroundColor: 'action.hover',
                },
              }}
            >
              <ListItemText
                primary={
                  <Typography variant="body1" fontWeight="medium">
                    {category.nombre}
                  </Typography>
                }
                secondary={
                  <Box component="span" sx={{ display: 'block', mt: 0.5 }}>
                    <Typography variant="body2" component="span" sx={{ fontWeight: 'bold', color: 'primary.main' }}>
                      Precio Base: ${category.precioBase.toFixed(2)}
                    </Typography>
                    {category.tienePrioridad && (
                      <>
                        <br />
                        <Typography variant="body2" component="span" color="secondary">
                          ⭐ Prioridad
                        </Typography>
                      </>
                    )}
                  </Box>
                }
              />
            </ListItem>
          ))}
        </List>
      ) : (
        <Box 
          sx={{ 
            p: 3, 
            textAlign: 'center',
            backgroundColor: 'action.hover',
            borderRadius: 1
          }}
        >
          <Typography color="text.secondary">
            No hay categorías creadas aún. Cree la primera categoría usando el formulario anterior.
          </Typography>
        </Box>
      )}

      <Snackbar
        open={showSuccess}
        autoHideDuration={3000}
        onClose={() => setShowSuccess(false)}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      >
        <Alert onClose={() => setShowSuccess(false)} severity="success" sx={{ width: '100%' }}>
          Categoría creada exitosamente
        </Alert>
      </Snackbar>

      <Snackbar
        open={showError}
        autoHideDuration={5000}
        onClose={() => setShowError(false)}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      >
        <Alert onClose={() => setShowError(false)} severity="error" sx={{ width: '100%' }}>
          {errorMessage}
        </Alert>
      </Snackbar>
    </Paper>
  );
};

export default CategoryManagementSection;
