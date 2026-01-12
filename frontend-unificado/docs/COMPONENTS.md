# Shared Components Documentation

This document describes all shared components available in the frontend-unificado application.

## Table of Contents

- [Layouts](#layouts)
- [UI Components](#ui-components)
- [Toast Notifications](#toast-notifications)
- [Usage Examples](#usage-examples)

## Layouts

### MainLayout

Main application layout with navbar, sidebar, and content area. Used for all authenticated routes.

**Features:**
- Responsive navbar with logo and user menu
- Collapsible sidebar with role-based navigation
- Mobile-friendly drawer navigation
- Footer with copyright information
- Automatic role-based menu filtering

**Usage:**
```tsx
import { MainLayout } from '@/layouts';

// Used automatically by ProtectedRoute
<Route element={<ProtectedRoute />}>
  <Route path="/" element={<MainLayout />}>
    <Route index element={<Dashboard />} />
  </Route>
</Route>
```

### AuthLayout

Layout for authentication screens (login, register, etc.). Provides a centered, card-based layout.

**Features:**
- Centered card design
- Gradient background
- Responsive sizing
- Minimal chrome for focus on authentication

**Usage:**
```tsx
import { AuthLayout } from '@/layouts';

export function LoginPage() {
  return (
    <AuthLayout>
      <Typography variant="h4">Login</Typography>
      {/* Login form content */}
    </AuthLayout>
  );
}
```

## UI Components

### LoadingSpinner

Displays a loading indicator with configurable size and display mode.

**Props:**
- `size?: 'small' | 'medium' | 'large'` - Size of the spinner (default: 'medium')
- `fullScreen?: boolean` - Display as full-screen overlay (default: false)

**Usage:**
```tsx
import { LoadingSpinner } from '@shared/components';

// Inline spinner
<LoadingSpinner size="small" />

// Full-screen overlay
<LoadingSpinner fullScreen />

// In a loading state
{isLoading && <LoadingSpinner />}
```

### ErrorMessage

Displays error messages with optional retry button.

**Props:**
- `error: Error | string` - Error object or message string
- `onRetry?: () => void` - Optional retry callback
- `variant?: 'inline' | 'centered'` - Display mode (default: 'inline')

**Usage:**
```tsx
import { ErrorMessage } from '@shared/components';

// Inline error
<ErrorMessage error="Something went wrong" />

// With retry button
<ErrorMessage 
  error={error} 
  onRetry={() => refetch()} 
/>

// Centered display
<ErrorMessage 
  error="Failed to load data" 
  variant="centered"
  onRetry={handleRetry}
/>
```

### EmptyState

Displays an informative empty state for lists or collections.

**Props:**
- `icon?: ReactNode` - Optional icon to display
- `title: string` - Main title text
- `description?: string` - Optional description text
- `action?: ReactNode` - Optional action button or element

**Usage:**
```tsx
import { EmptyState } from '@shared/components';
import EventIcon from '@mui/icons-material/Event';

<EmptyState
  icon={<EventIcon />}
  title="No events available"
  description="There are currently no events published. Check back later."
  action={
    <Button variant="contained" onClick={handleCreate}>
      Create Event
    </Button>
  }
/>
```

### Button

Enhanced MUI Button with loading state support.

**Props:**
- All standard MUI Button props
- `loading?: boolean` - Show loading spinner and disable button

**Usage:**
```tsx
import { Button } from '@shared/components';

// Standard button
<Button variant="contained">Click Me</Button>

// With loading state
<Button 
  variant="contained" 
  loading={isSubmitting}
  onClick={handleSubmit}
>
  Submit
</Button>

// Disabled when loading
<Button loading={true}>Processing...</Button>
```

### TextField

Enhanced MUI TextField with consistent styling.

**Props:**
- All standard MUI TextField props
- `variant?: 'outlined' | 'filled' | 'standard'` - Input variant (default: 'outlined')
- `fullWidth?: boolean` - Full width display (default: true)

**Usage:**
```tsx
import { TextField } from '@shared/components';

// Basic text field
<TextField 
  label="Name" 
  value={name}
  onChange={(e) => setName(e.target.value)}
/>

// With validation
<TextField
  label="Email"
  type="email"
  required
  error={!!errors.email}
  helperText={errors.email?.message}
/>

// Multiline
<TextField
  label="Description"
  multiline
  rows={4}
/>
```

## Toast Notifications

### ToastProvider

Context provider for toast notifications. Must wrap your app to enable toast functionality.

**Setup:**
```tsx
import { ToastProvider } from '@shared/components';

function App() {
  return (
    <ToastProvider>
      <YourApp />
    </ToastProvider>
  );
}
```

### useToast Hook

Hook to access toast notification functions.

**API:**
- `showToast(message, severity?, duration?)` - Show custom toast
- `showSuccess(message)` - Show success toast
- `showError(message)` - Show error toast
- `showWarning(message)` - Show warning toast
- `showInfo(message)` - Show info toast

**Usage:**
```tsx
import { useToast } from '@shared/components';

function MyComponent() {
  const toast = useToast();

  const handleSuccess = () => {
    toast.showSuccess('Operation completed successfully!');
  };

  const handleError = () => {
    toast.showError('Something went wrong');
  };

  const handleCustom = () => {
    toast.showToast('Custom message', 'warning', 3000);
  };

  return (
    <Button onClick={handleSuccess}>Show Success</Button>
  );
}
```

## Usage Examples

### Loading State Pattern

```tsx
function EventsList() {
  const { data: eventos, isLoading, error, refetch } = useEventos();

  if (isLoading) {
    return <LoadingSpinner />;
  }

  if (error) {
    return <ErrorMessage error={error} onRetry={refetch} />;
  }

  if (eventos.length === 0) {
    return (
      <EmptyState
        icon={<EventIcon />}
        title="No events found"
        description="Start by creating your first event"
        action={<Button onClick={handleCreate}>Create Event</Button>}
      />
    );
  }

  return (
    <div>
      {eventos.map(evento => (
        <EventCard key={evento.id} evento={evento} />
      ))}
    </div>
  );
}
```

### Form with Loading and Toast

```tsx
function CreateEventForm() {
  const toast = useToast();
  const { mutate: createEvento, isLoading } = useCreateEvento();

  const handleSubmit = (data) => {
    createEvento(data, {
      onSuccess: () => {
        toast.showSuccess('Event created successfully!');
        navigate('/eventos');
      },
      onError: (error) => {
        toast.showError(error.message);
      },
    });
  };

  return (
    <form onSubmit={handleSubmit}>
      <TextField
        label="Event Name"
        name="nombre"
        required
      />
      <TextField
        label="Description"
        name="descripcion"
        multiline
        rows={4}
      />
      <Button 
        type="submit" 
        variant="contained"
        loading={isLoading}
      >
        Create Event
      </Button>
    </form>
  );
}
```

### Error Boundary with ErrorMessage

```tsx
function EventDetail({ id }) {
  const { data: evento, isLoading, error } = useEvento(id);

  if (isLoading) {
    return <LoadingSpinner fullScreen />;
  }

  if (error) {
    return (
      <Container>
        <ErrorMessage 
          error={error}
          variant="centered"
          onRetry={() => window.location.reload()}
        />
      </Container>
    );
  }

  return <EventDetailView evento={evento} />;
}
```

## Accessibility

All components follow accessibility best practices:

- **LoadingSpinner**: Includes `aria-label` and `role="status"`
- **ErrorMessage**: Proper semantic HTML and ARIA attributes
- **EmptyState**: Uses `role="status"` and `aria-live="polite"`
- **Button**: Supports keyboard navigation and focus states
- **TextField**: Proper label association with `htmlFor`
- **Toast**: Announces messages to screen readers

## Responsive Design

All components are responsive and work across:
- Desktop (>1024px)
- Tablet (768-1024px)
- Mobile (<768px)

The MainLayout automatically switches between sidebar and drawer navigation based on screen size.

## Theming

All components use the application theme and support:
- Light/dark mode (if implemented)
- Custom color palettes
- Typography customization
- Spacing and border radius

See [THEME.md](./THEME.md) for theme customization details.

## Testing

Components can be tested using React Testing Library:

```tsx
import { render, screen } from '@testing-library/react';
import { LoadingSpinner } from '@shared/components';

test('renders loading spinner', () => {
  render(<LoadingSpinner />);
  expect(screen.getByRole('status')).toBeInTheDocument();
});
```

See individual component test files for more examples.
