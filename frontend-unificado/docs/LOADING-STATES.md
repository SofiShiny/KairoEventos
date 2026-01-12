# Loading States & Feedback UX Guide

This guide covers all loading states and user feedback components available in the Frontend Unificado application.

## Overview

Good UX requires clear feedback for all user actions and system states. This application provides a comprehensive set of components for:

- Loading indicators (spinners, skeletons, progress bars)
- User feedback (toasts, success/error messages)
- Empty states
- Image loading placeholders
- Smooth page transitions

## Components

### 1. LoadingSpinner

A versatile loading spinner that can be used inline or as a full-screen overlay.

**Props:**
- `size?: 'small' | 'medium' | 'large'` - Size of the spinner (default: 'medium')
- `fullScreen?: boolean` - Show as full-screen overlay (default: false)

**Usage:**

```tsx
import { LoadingSpinner } from '@shared/components';

// Inline spinner
<LoadingSpinner size="medium" />

// Full-screen overlay
<LoadingSpinner fullScreen />
```

**When to use:**
- Short operations (< 3 seconds)
- Initial data fetching
- Form submissions

### 2. Button with Loading State

Enhanced MUI Button that automatically shows a spinner and disables when loading.

**Props:**
- All standard MUI Button props
- `loading?: boolean` - Show loading state

**Usage:**

```tsx
import { Button } from '@shared/components';

function MyComponent() {
  const [loading, setLoading] = useState(false);

  const handleSubmit = async () => {
    setLoading(true);
    try {
      await submitForm();
    } finally {
      setLoading(false);
    }
  };

  return (
    <Button
      variant="contained"
      loading={loading}
      onClick={handleSubmit}
    >
      Submit
    </Button>
  );
}
```

**When to use:**
- All async button actions
- Form submissions
- API calls triggered by buttons

### 3. SkeletonLoader

Displays skeleton placeholders while content is loading, showing the structure of the content.

**Props:**
- `variant?: 'list' | 'card' | 'table' | 'detail'` - Type of skeleton (default: 'list')
- `count?: number` - Number of skeleton items (default: 3)

**Usage:**

```tsx
import { SkeletonLoader } from '@shared/components';

function EventosList() {
  const { data: eventos, isLoading } = useEventos();

  if (isLoading) {
    return <SkeletonLoader variant="card" count={6} />;
  }

  return (
    <Grid container spacing={2}>
      {eventos.map(evento => (
        <EventoCard key={evento.id} evento={evento} />
      ))}
    </Grid>
  );
}
```

**Variants:**
- `list` - For list views with thumbnail + text
- `card` - For card grids
- `table` - For table rows
- `detail` - For detail pages with image + content

**When to use:**
- Initial page loads
- List/grid loading
- Detail page loading
- Better UX than blank screens or spinners

### 4. ProgressIndicator

Shows progress for long-running operations with determinate or indeterminate progress.

**Props:**
- `value?: number` - Progress value 0-100 (undefined = indeterminate)
- `variant?: 'linear' | 'circular'` - Display style (default: 'linear')
- `label?: string` - Optional label text
- `showPercentage?: boolean` - Show percentage (default: true)

**Usage:**

```tsx
import { ProgressIndicator } from '@shared/components';

// Indeterminate (unknown duration)
<ProgressIndicator
  variant="linear"
  label="Processing..."
  showPercentage={false}
/>

// Determinate (known progress)
<ProgressIndicator
  variant="linear"
  value={uploadProgress}
  label="Uploading file..."
  showPercentage
/>

// Circular variant
<ProgressIndicator
  variant="circular"
  value={75}
  label="Processing..."
/>
```

**When to use:**
- File uploads
- Long-running operations (> 3 seconds)
- Batch processing
- When progress can be tracked

### 5. ImagePlaceholder

Image component with loading placeholder and error fallback.

**Props:**
- `src?: string` - Image URL
- `alt: string` - Alt text (required for accessibility)
- `width?: string | number` - Width (default: '100%')
- `height?: string | number` - Height (default: 200)
- `borderRadius?: string | number` - Border radius (default: 1)
- `objectFit?: 'cover' | 'contain' | 'fill' | 'none' | 'scale-down'` - Object fit (default: 'cover')

**Usage:**

```tsx
import { ImagePlaceholder } from '@shared/components';

<ImagePlaceholder
  src={evento.imagenUrl}
  alt={evento.nombre}
  height={300}
  borderRadius={2}
  objectFit="cover"
/>
```

**Features:**
- Shows skeleton while loading
- Lazy loading (loading="lazy")
- Fallback icon if image fails
- Placeholder if no src provided

**When to use:**
- All images in the application
- Event images
- User avatars
- Any dynamic images

### 6. Toast Notifications

Context-based toast notification system for user feedback.

**Setup:**

```tsx
// In App.tsx
import { ToastProvider } from '@shared/components';

function App() {
  return (
    <ToastProvider>
      <YourApp />
    </ToastProvider>
  );
}
```

**Usage:**

```tsx
import { useToast } from '@shared/components';

function MyComponent() {
  const toast = useToast();

  const handleSuccess = () => {
    toast.showSuccess('Operation completed successfully!');
  };

  const handleError = () => {
    toast.showError('An error occurred. Please try again.');
  };

  const handleWarning = () => {
    toast.showWarning('This action cannot be undone.');
  };

  const handleInfo = () => {
    toast.showInfo('New features available!');
  };

  // Custom toast
  toast.showToast('Custom message', 'info', 3000);
}
```

**Methods:**
- `showSuccess(message: string)` - Green success toast
- `showError(message: string)` - Red error toast
- `showWarning(message: string)` - Orange warning toast
- `showInfo(message: string)` - Blue info toast
- `showToast(message: string, severity: AlertColor, duration?: number)` - Custom toast

**When to use:**
- After successful operations (create, update, delete)
- Error messages
- Warnings before destructive actions
- Informational messages

### 7. EmptyState

Displays informative message when lists or content are empty.

**Props:**
- `icon?: ReactNode` - Icon to display
- `title: string` - Main title
- `description?: string` - Description text
- `action?: ReactNode` - Optional action button

**Usage:**

```tsx
import { EmptyState } from '@shared/components';
import EventIcon from '@mui/icons-material/Event';

function EventosList() {
  const { data: eventos } = useEventos();

  if (eventos.length === 0) {
    return (
      <EmptyState
        icon={<EventIcon sx={{ fontSize: 64 }} />}
        title="No hay eventos disponibles"
        description="Actualmente no hay eventos publicados. Vuelve más tarde."
        action={
          hasRole('Organizator') && (
            <Button onClick={handleCreateEvento}>
              Crear Evento
            </Button>
          )
        }
      />
    );
  }

  return <EventosGrid eventos={eventos} />;
}
```

**When to use:**
- Empty lists
- No search results
- Filtered results with no matches
- First-time user experiences

### 8. PageTransition

Adds smooth transitions when navigating between pages.

**Props:**
- `children: ReactNode` - Page content
- `variant?: 'fade' | 'slide' | 'grow' | 'zoom'` - Animation type (default: 'fade')
- `direction?: 'up' | 'down' | 'left' | 'right'` - Slide direction (for slide variant)
- `timeout?: number` - Animation duration in ms (default: 300)

**Usage:**

```tsx
import { PageTransition } from '@shared/components';

function EventosPage() {
  return (
    <PageTransition variant="fade">
      <Container>
        <Typography variant="h4">Eventos</Typography>
        {/* Page content */}
      </Container>
    </PageTransition>
  );
}
```

**Variants:**
- `fade` - Fade in/out (recommended for most pages)
- `slide` - Slide from direction
- `grow` - Grow from center
- `zoom` - Zoom in/out

**When to use:**
- Wrap all page components
- Provides smooth navigation experience
- Use 'fade' for most pages

## Best Practices

### Loading States

1. **Choose the right indicator:**
   - Spinner: Short operations (< 3 seconds)
   - Skeleton: Initial page loads
   - Progress: Long operations (> 3 seconds) with trackable progress

2. **Always disable interactive elements during loading:**
   ```tsx
   <Button loading={isSubmitting} disabled={isSubmitting}>
     Submit
   </Button>
   ```

3. **Provide context:**
   ```tsx
   <ProgressIndicator
     value={progress}
     label="Uploading file..." // Tell users what's happening
   />
   ```

### User Feedback

1. **Always provide feedback for user actions:**
   ```tsx
   const handleDelete = async () => {
     try {
       await deleteEvento(id);
       toast.showSuccess('Evento eliminado exitosamente');
     } catch (error) {
       toast.showError('Error al eliminar evento');
     }
   };
   ```

2. **Use appropriate severity:**
   - Success: Completed actions
   - Error: Failed operations
   - Warning: Potentially destructive actions
   - Info: General information

3. **Keep messages concise and actionable:**
   - ✅ "Evento creado exitosamente"
   - ❌ "The event has been successfully created and saved to the database"

### Empty States

1. **Always provide helpful guidance:**
   ```tsx
   <EmptyState
     title="No hay eventos"
     description="Crea tu primer evento para comenzar"
     action={<Button>Crear Evento</Button>}
   />
   ```

2. **Match the context:**
   - No results: Suggest adjusting filters
   - First time: Guide user to create content
   - Error state: Provide retry action

### Images

1. **Always use ImagePlaceholder for dynamic images:**
   ```tsx
   <ImagePlaceholder
     src={evento.imagenUrl}
     alt={evento.nombre} // Required for accessibility
     loading="lazy" // Automatic
   />
   ```

2. **Provide meaningful alt text:**
   - ✅ `alt="Concierto de Rock en el Estadio Nacional"`
   - ❌ `alt="image"`

## Integration with React Query

Loading states work seamlessly with React Query:

```tsx
function EventosList() {
  const { data: eventos, isLoading, error } = useEventos();

  // Loading state
  if (isLoading) {
    return <SkeletonLoader variant="card" count={6} />;
  }

  // Error state
  if (error) {
    return (
      <ErrorMessage
        error={error}
        onRetry={() => queryClient.invalidateQueries(['eventos'])}
      />
    );
  }

  // Empty state
  if (eventos.length === 0) {
    return (
      <EmptyState
        title="No hay eventos"
        description="No se encontraron eventos"
      />
    );
  }

  // Success state
  return (
    <Grid container spacing={2}>
      {eventos.map(evento => (
        <EventoCard key={evento.id} evento={evento} />
      ))}
    </Grid>
  );
}
```

## Accessibility

All components follow accessibility best practices:

- **LoadingSpinner**: Includes `role="status"` and `aria-label`
- **ProgressIndicator**: Includes progress value for screen readers
- **ImagePlaceholder**: Requires `alt` text
- **Toast**: Uses MUI Alert with proper ARIA attributes
- **EmptyState**: Uses semantic HTML

## Examples

See `src/shared/examples/LoadingStatesShowcase.tsx` for a comprehensive showcase of all components.

## Related Documentation

- [Components Guide](./COMPONENTS.md)
- [React Query Guide](./REACT-QUERY.md)
- [Theme Guide](./THEME.md)
