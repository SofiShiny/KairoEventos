import React, { useState } from 'react';
import {
  Box,
  Paper,
  Typography,
  TextField,
  Button,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  CircularProgress,
  Alert,
  Snackbar,
  FormHelperText,
} from '@mui/material';
import { useCreateSeat, useEventCategories } from '../api/seats';
import type { SeatCreateDto } from '../types/api';

interface SeatAssignmentSectionProps {
  eventoId: string;
  mapaId: string;
}

interface FormErrors {
  fila?: string;
  numero?: string;
  categoriaId?: string;
}

const SeatAssignmentSection: React.FC<SeatAssignmentSectionProps> = ({ eventoId, mapaId }) => {
  const [fila, setFila] = useState('');
  const [numero, setNumero] = useState('');
  const [categoriaId, setCategoriaId] = useState('');
  const [errors, setErrors] = useState<FormErrors>({});
  const [showSuccess, setShowSuccess] = useState(false);
  const [showError, setShowError] = useState(false);
  const [errorMessage, setErrorMessage] = useState('');

  const { mutate: createSeat, isPending } = useCreateSeat();
  const { data: categories, isLoading: categoriesLoading } = useEventCategories(mapaId);

  // Debug: log categories
  React.useEffect(() => {
    console.log('[SeatAssignmentSection] Categories:', categories);
    console.log('[SeatAssignmentSection] MapaId:', mapaId);
    if (categories && categories.length > 0) {
      console.log('[SeatAssignmentSection] First category:', categories[0]);
    }
  }, [categories, mapaId]);

  const validateForm = (): boolean => {
    const newErrors: FormErrors = {};

    // Validate fila: must be a single letter A-Z or a number
    if (!fila || fila.trim().length === 0) {
      newErrors.fila = 'La fila es requerida';
    } else if (!/^[A-Za-z0-9]+$/.test(fila.trim())) {
      newErrors.fila = 'La fila debe ser una letra o número válido';
    }

    // Validate numero: must be a positive integer between 1-999
    const numeroInt = parseInt(numero, 10);
    if (!numero || numero.trim().length === 0) {
      newErrors.numero = 'El número es requerido';
    } else if (isNaN(numeroInt)) {
      newErrors.numero = 'El número debe ser un valor numérico';
    } else if (numeroInt < 1 || numeroInt > 999) {
      newErrors.numero = 'El número debe estar entre 1 y 999';
    } else if (!Number.isInteger(numeroInt)) {
      newErrors.numero = 'El número debe ser un entero';
    }

    // Validate categoriaId
    if (!categoriaId) {
      newErrors.categoriaId = 'Debe seleccionar una categoría';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    if (!validateForm()) {
      return;
    }

    // Find the category name from the selected ID
    const selectedCategory = categories?.find(cat => 
      (cat.categoriaId || cat.id) === categoriaId
    );
    
    const seatData: SeatCreateDto = {
      fila: fila.trim().toUpperCase(),
      numero: parseInt(numero, 10),
      categoriaId,
      categoriaNombre: selectedCategory?.nombre || '',
      mapaId,
      estado: 'Disponible',
    };

    createSeat(seatData, {
      onSuccess: () => {
        // Clear form
        setFila('');
        setNumero('');
        setCategoriaId('');
        setErrors({});
        setShowSuccess(true);
      },
      onError: (error: any) => {
        const message = error?.response?.data?.message || 'Error al crear el asiento';
        setErrorMessage(message);
        setShowError(true);
      },
    });
  };

  // Check if categories exist
  const hasCategories = categories && categories.length > 0;

  return (
    <Paper elevation={2} sx={{ p: { xs: 2, sm: 3 }, mb: 3 }}>
      <Typography variant="h6" gutterBottom>
        Asignación de Asientos
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
      ) : !hasCategories ? (
        <Alert 
          severity="info" 
          sx={{ mb: 2 }}
          role="alert"
        >
          Primero debe crear categorías antes de asignar asientos. Use el formulario de "Gestión de Categorías" arriba para crear una categoría.
        </Alert>
      ) : (
        <Box component="form" onSubmit={handleSubmit} noValidate>
          <TextField
            fullWidth
            label="Fila"
            value={fila}
            onChange={(e) => setFila(e.target.value)}
            error={!!errors.fila}
            helperText={errors.fila || 'Ingrese una letra (A-Z) o número para identificar la fila'}
            margin="normal"
            required
            disabled={isPending}
            placeholder="Ej: A, B, 1, 2"
            autoComplete="off"
            inputProps={{
              'aria-label': 'Fila del asiento',
              'aria-required': 'true',
              'aria-invalid': !!errors.fila,
            }}
          />

          <TextField
            fullWidth
            label="Número"
            value={numero}
            onChange={(e) => setNumero(e.target.value)}
            error={!!errors.numero}
            helperText={errors.numero || 'Ingrese el número del asiento (1-999)'}
            margin="normal"
            required
            type="number"
            inputProps={{ 
              min: '1', 
              max: '999', 
              step: '1',
              'aria-label': 'Número del asiento',
              'aria-required': 'true',
              'aria-invalid': !!errors.numero,
            }}
            disabled={isPending}
            autoComplete="off"
          />

          <FormControl
            fullWidth
            margin="normal"
            required
            error={!!errors.categoriaId}
            disabled={isPending}
          >
            <InputLabel id="category-select-label">Categoría</InputLabel>
            <Select
              labelId="category-select-label"
              id="category-select"
              value={categoriaId || ''}
              label="Categoría"
              onChange={(e) => setCategoriaId(e.target.value)}
              inputProps={{
                'aria-label': 'Categoría del asiento',
                'aria-required': 'true',
                'aria-invalid': !!errors.categoriaId,
              }}
            >
              {categories?.map((category) => (
                <MenuItem key={category.categoriaId || category.id} value={category.categoriaId || category.id}>
                  {category.nombre} - ${category.precioBase.toFixed(2)}
                </MenuItem>
              ))}
            </Select>
            {errors.categoriaId ? (
              <FormHelperText error>{errors.categoriaId}</FormHelperText>
            ) : (
              <FormHelperText>Seleccione la categoría para este asiento</FormHelperText>
            )}
          </FormControl>

          <Button
            type="submit"
            variant="contained"
            color="primary"
            fullWidth
            sx={{ mt: 2, minHeight: 42 }}
            disabled={isPending}
            aria-label="Crear nuevo asiento"
          >
            {isPending ? (
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                <CircularProgress size={20} color="inherit" />
                <span>Creando...</span>
              </Box>
            ) : (
              'Crear Asiento'
            )}
          </Button>
        </Box>
      )}

      <Snackbar
        open={showSuccess}
        autoHideDuration={3000}
        onClose={() => setShowSuccess(false)}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      >
        <Alert onClose={() => setShowSuccess(false)} severity="success" sx={{ width: '100%' }}>
          Asiento creado exitosamente
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

export default SeatAssignmentSection;
