# Routing and Navigation

This document describes the routing and navigation implementation for the Frontend Unificado application.

## Overview

The application uses `react-router-dom` v6 for client-side routing with the following features:

- **Protected Routes**: Routes that require authentication
- **Role-Based Access Control**: Routes restricted to specific user roles
- **Lazy Loading**: Non-critical routes are loaded on-demand for better performance
- **Nested Routes**: Organized route structure with layouts

## Route Structure

```
/
├── /login (public)
└── / (protected with MainLayout)
    ├── / (Dashboard)
    ├── /eventos
    │   ├── / (Events list)
    │   └── /:id (Event details)
    ├── /mis-entradas (My tickets)
    ├── /comprar/:eventoId (Purchase ticket)
    ├── /usuarios (Admin only)
    └── /reportes (Admin/Organizator only)
```

## Components

### AppRoutes

Main routing configuration component that defines all application routes.

**Location**: `src/routes/AppRoutes.tsx`

**Features**:
- Lazy loading for non-critical routes
- Suspense fallback for loading states
- Nested route structure with layouts

### ProtectedRoute

Component that requires authentication to access routes.

**Location**: `src/routes/ProtectedRoute.tsx`

**Behavior**:
- Shows loading spinner while checking authentication
- Redirects to `/login` if user is not authenticated
- Renders children or nested routes if authenticated

**Usage**:
```tsx
// As a wrapper
<ProtectedRoute>
  <DashboardPage />
</ProtectedRoute>

// As a layout route
<Route element={<ProtectedRoute />}>
  <Route path="/" element={<DashboardPage />} />
</Route>
```

### RoleBasedRoute

Component that requires specific roles to access routes.

**Location**: `src/routes/RoleBasedRoute.tsx`

**Behavior**:
- Shows loading spinner while checking authentication
- Redirects to `/login` if user is not authenticated
- Shows 403 Forbidden page if user doesn't have required roles
- Renders children or nested routes if user has required role

**Usage**:
```tsx
// As a wrapper
<RoleBasedRoute requiredRoles={['Admin']}>
  <UsuariosPage />
</RoleBasedRoute>

// As a layout route
<Route element={<RoleBasedRoute requiredRoles={['Admin', 'Organizator']} />}>
  <Route path="/reportes" element={<ReportesPage />} />
</Route>
```

### MainLayout

Main application layout with navbar, navigation menu, and footer.

**Location**: `src/layouts/MainLayout.tsx`

**Features**:
- Responsive navbar with user menu
- Desktop navigation menu
- Mobile drawer navigation
- Role-based menu items
- Footer with copyright

## Pages

### Public Pages

- **LoginPage** (`/login`): Authentication page with Keycloak login button

### Protected Pages

- **DashboardPage** (`/`): Main dashboard with user info and quick stats
- **EventosPage** (`/eventos`): List of events (placeholder)
- **EventoDetailPage** (`/eventos/:id`): Event details (placeholder)
- **MisEntradasPage** (`/mis-entradas`): User's tickets (placeholder)
- **ComprarEntradaPage** (`/comprar/:eventoId`): Purchase ticket (placeholder)

### Role-Restricted Pages

- **UsuariosPage** (`/usuarios`): User management - Admin only (placeholder)
- **ReportesPage** (`/reportes`): Reports and analytics - Admin/Organizator only (placeholder)

### Error Pages

- **NotFoundPage** (`*`): 404 error page

## Navigation

### Programmatic Navigation

Use the `useNavigate` hook from `react-router-dom`:

```tsx
import { useNavigate } from 'react-router-dom';

function MyComponent() {
  const navigate = useNavigate();
  
  const handleClick = () => {
    navigate('/eventos');
  };
  
  return <button onClick={handleClick}>Go to Events</button>;
}
```

### Link Navigation

Use the `Link` component from `react-router-dom`:

```tsx
import { Link } from 'react-router-dom';

function MyComponent() {
  return <Link to="/eventos">Events</Link>;
}
```

### Active Route Detection

Use the `useLocation` hook to detect the current route:

```tsx
import { useLocation } from 'react-router-dom';

function MyComponent() {
  const location = useLocation();
  const isActive = location.pathname === '/eventos';
  
  return <div className={isActive ? 'active' : ''}>Events</div>;
}
```

## Lazy Loading

Non-critical routes are lazy-loaded to improve initial load performance:

```tsx
const EventosPage = lazy(() =>
  import('../modules/eventos/pages').then((m) => ({ default: m.EventosPage }))
);
```

All lazy-loaded routes are wrapped in a `Suspense` component with a loading fallback.

## Access Control

### Authentication Check

The `ProtectedRoute` component checks if the user is authenticated using the `useAuth` hook:

```tsx
const { isAuthenticated, isLoading } = useAuth();
```

### Role Check

The `RoleBasedRoute` component checks if the user has required roles:

```tsx
const { hasRole } = useAuth();
const hasRequiredRole = requiredRoles.some((role) => hasRole(role));
```

## Future Enhancements

The following features will be implemented in upcoming tasks:

1. **Breadcrumbs**: Hierarchical navigation (Requirement 15.5)
2. **Active Menu Highlighting**: Visual indication of current route (Requirement 15.6)
3. **Route Transitions**: Smooth animations between pages (Requirement 13.4)
4. **Deep Linking**: Support for URL parameters and query strings
5. **Route Guards**: Additional validation before route changes

## Related Requirements

This implementation satisfies the following requirements:

- **15.1**: Uses react-router-dom for routing
- **15.2**: Protected routes require authentication
- **15.3**: Role-based access control
- **15.4**: URL state management
- **15.7**: Lazy loading for non-critical routes
- **2.2**: Redirect to login for unauthenticated users

## Testing

Route components can be tested using React Testing Library with MemoryRouter:

```tsx
import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { ProtectedRoute } from './ProtectedRoute';

test('redirects to login when not authenticated', () => {
  render(
    <MemoryRouter initialEntries={['/protected']}>
      <ProtectedRoute>
        <div>Protected Content</div>
      </ProtectedRoute>
    </MemoryRouter>
  );
  
  // Assert redirect behavior
});
```

## See Also

- [Authentication Documentation](./AUTHENTICATION.md)
- [React Router Documentation](https://reactrouter.com/)
