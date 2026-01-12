# Form Validation

This module provides centralized form validation using `react-hook-form` and `zod`.

## Overview

All form validation schemas are defined in `schemas.ts` and exported through `index.ts`. This ensures:

- **Consistency**: Same validation rules across the application
- **Reusability**: Schemas can be imported and used in any form
- **Type Safety**: TypeScript types are automatically inferred from schemas
- **Maintainability**: Single source of truth for validation rules

## Requirements Validation

This implementation satisfies the following requirements:

- ✅ **14.1**: Uses `react-hook-form` and `zod` for form validation
- ✅ **14.2**: Validates required fields with specific error messages
- ✅ **14.3**: Validates email format with proper regex
- ✅ **14.4**: Validates min/max length for all text fields
- ✅ **14.5**: Provides specific error messages for each field
- ✅ **14.6**: Shows visual indicators (error/success states)
- ✅ **14.7**: Disables submit button when form is invalid

## Available Schemas

### 1. eventoSchema

Validates evento (event) creation and editing forms.

**Fields:**
- `nombre`: 3-100 characters, required
- `descripcion`: 10-500 characters, required
- `fecha`: Date string, must be today or future, required
- `ubicacion`: 3-200 characters, required
- `imagenUrl`: Valid URL, optional

**Usage:**
```typescript
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { eventoSchema, type EventoFormData } from '@/shared/validation';

const { control, handleSubmit, formState: { errors, isValid } } = useForm<EventoFormData>({
  resolver: zodResolver(eventoSchema),
  mode: 'onChange',
});
```

### 2. usuarioSchema

Validates usuario (user) creation forms.

**Fields:**
- `username`: 3-50 characters, alphanumeric + underscore/hyphen, required
- `nombre`: 3-100 characters, required
- `correo`: Valid email format, max 100 characters, required
- `telefono`: 10-15 digits, can include +, required
- `rol`: Must be 'Admin', 'Organizator', or 'Asistente', required
- `password`: Min 8 characters, must include uppercase, lowercase, and number, required

**Usage:**
```typescript
import { usuarioSchema, type UsuarioFormData } from '@/shared/validation';

const { control, handleSubmit } = useForm<UsuarioFormData>({
  resolver: zodResolver(usuarioSchema),
  mode: 'onChange',
});
```

### 3. usuarioEditSchema

Extends `usuarioSchema` but makes password optional (for editing existing users).

**Usage:**
```typescript
import { usuarioEditSchema, type UsuarioEditFormData } from '@/shared/validation';

const { control, handleSubmit } = useForm<UsuarioEditFormData>({
  resolver: zodResolver(usuarioEditSchema),
  mode: 'onChange',
});
```

### 4. entradaSchema

Validates entrada (ticket) creation forms.

**Fields:**
- `eventoId`: Valid UUID, required
- `asientoId`: Valid UUID, required
- `usuarioId`: Valid UUID, required

**Usage:**
```typescript
import { entradaSchema, type EntradaFormData } from '@/shared/validation';

const { control, handleSubmit } = useForm<EntradaFormData>({
  resolver: zodResolver(entradaSchema),
  mode: 'onChange',
});
```

### 5. reporteFiltrosSchema

Validates report filter forms.

**Fields:**
- `fechaInicio`: Date string, required
- `fechaFin`: Date string, must be >= fechaInicio, required
- `eventoId`: Valid UUID, optional

**Usage:**
```typescript
import { reporteFiltrosSchema, type ReporteFiltrosFormData } from '@/shared/validation';

const { control, handleSubmit } = useForm<ReporteFiltrosFormData>({
  resolver: zodResolver(reporteFiltrosSchema),
  mode: 'onChange',
});
```

### 6. searchSchema

Generic schema for search and filter forms.

**Fields:**
- `query`: Max 200 characters, optional
- `fecha`: Date string, optional
- `ubicacion`: Max 200 characters, optional

## Form Implementation Pattern

### Basic Form Structure

```typescript
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { TextField, Button } from '@mui/material';
import { eventoSchema, type EventoFormData } from '@/shared/validation';

function MyForm() {
  const {
    control,
    handleSubmit,
    formState: { errors, isValid },
  } = useForm<EventoFormData>({
    resolver: zodResolver(eventoSchema),
    defaultValues: {
      nombre: '',
      descripcion: '',
      fecha: '',
      ubicacion: '',
      imagenUrl: '',
    },
    mode: 'onChange', // Validate on change for real-time feedback
  });

  const onSubmit = (data: EventoFormData) => {
    console.log('Valid data:', data);
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <Controller
        name="nombre"
        control={control}
        render={({ field }) => (
          <TextField
            {...field}
            label="Nombre"
            fullWidth
            required
            error={!!errors.nombre}
            helperText={errors.nombre?.message}
            inputProps={{
              'aria-label': 'Nombre del evento',
            }}
          />
        )}
      />
      
      {/* More fields... */}
      
      <Button
        type="submit"
        variant="contained"
        disabled={!isValid}
      >
        Enviar
      </Button>
    </form>
  );
}
```

### Key Features

1. **Real-time Validation**: Use `mode: 'onChange'` for immediate feedback
2. **Error Display**: Show errors with `error={!!errors.fieldName}` and `helperText={errors.fieldName?.message}`
3. **Submit Control**: Disable submit button with `disabled={!isValid}`
4. **Accessibility**: Include `aria-label` for screen readers
5. **Visual Indicators**: MUI TextField automatically shows error state with red border

## Validation Rules

### Required Fields

All required fields use `.min(1, 'Field is required')` to ensure they're not empty.

### Email Validation

Email fields use `.email('Invalid email format')` which validates against RFC 5322 standard.

### Length Validation

Text fields specify both minimum and maximum lengths:
```typescript
z.string()
  .min(3, 'Minimum 3 characters')
  .max(100, 'Maximum 100 characters')
```

### Pattern Validation

Complex patterns use `.regex()`:
```typescript
// Username: alphanumeric + underscore/hyphen
z.string().regex(/^[a-zA-Z0-9_-]+$/, 'Only letters, numbers, hyphens, and underscores')

// Phone: 10-15 digits, optional +
z.string().regex(/^\+?[0-9]{10,15}$/, 'Phone must be 10-15 digits')

// Password: uppercase, lowercase, number
z.string().regex(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)/, 'Must include uppercase, lowercase, and number')
```

### Custom Validation

Use `.refine()` for custom logic:
```typescript
z.string().refine((date) => {
  const selectedDate = new Date(date);
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  return selectedDate >= today;
}, 'Date must be today or in the future')
```

## Helper Functions

### getZodErrorMessage

Extracts the first error message from a Zod error.

```typescript
import { getZodErrorMessage } from '@/shared/validation';

try {
  eventoSchema.parse(data);
} catch (error) {
  const message = getZodErrorMessage(error);
  console.error(message);
}
```

### hasFieldError

Checks if a specific field has an error.

```typescript
import { hasFieldError } from '@/shared/validation';

const hasError = hasFieldError(errors, 'nombre');
```

### getFieldErrorMessage

Gets the error message for a specific field.

```typescript
import { getFieldErrorMessage } from '@/shared/validation';

const errorMessage = getFieldErrorMessage(errors, 'nombre');
```

## Testing

Validation schemas can be tested independently:

```typescript
import { eventoSchema } from '@/shared/validation';

describe('eventoSchema', () => {
  it('should validate valid evento data', () => {
    const validData = {
      nombre: 'Test Event',
      descripcion: 'This is a test event description',
      fecha: '2024-12-31',
      ubicacion: 'Test Location',
      imagenUrl: 'https://example.com/image.jpg',
    };
    
    const result = eventoSchema.safeParse(validData);
    expect(result.success).toBe(true);
  });
  
  it('should reject invalid email', () => {
    const invalidData = {
      nombre: 'AB', // Too short
      descripcion: 'Short', // Too short
      fecha: '2020-01-01', // Past date
      ubicacion: 'OK',
      imagenUrl: 'not-a-url',
    };
    
    const result = eventoSchema.safeParse(invalidData);
    expect(result.success).toBe(false);
  });
});
```

## Best Practices

1. **Always use `mode: 'onChange'`** for real-time validation feedback
2. **Show specific error messages** using `helperText={errors.fieldName?.message}`
3. **Disable submit button** when form is invalid: `disabled={!isValid}`
4. **Include aria-labels** for accessibility
5. **Use Controller** from react-hook-form for MUI components
6. **Reset form** after successful submission or cancel
7. **Handle loading states** by disabling inputs during submission
8. **Show general form error** if multiple fields are invalid

## Migration Guide

If you have existing forms without validation:

1. Import the appropriate schema from `@/shared/validation`
2. Add `resolver: zodResolver(schema)` to `useForm()`
3. Add `mode: 'onChange'` for real-time validation
4. Add error display to each field: `error={!!errors.fieldName}` and `helperText={errors.fieldName?.message}`
5. Disable submit button: `disabled={!isValid}`
6. Add aria-labels for accessibility

## References

- [react-hook-form Documentation](https://react-hook-form.com/)
- [Zod Documentation](https://zod.dev/)
- [MUI TextField API](https://mui.com/material-ui/api/text-field/)
