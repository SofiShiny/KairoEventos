# Authentication Examples

This document provides practical examples of using the authentication system in the Frontend Unificado application.

## Basic Authentication

### Simple Login/Logout Component

```typescript
import { useAuth } from './context/AuthContext';

function LoginButton() {
  const { isAuthenticated, isLoading, login, logout, user } = useAuth();

  if (isLoading) {
    return <button disabled>Loading...</button>;
  }

  if (!isAuthenticated) {
    return <button onClick={login}>Login with Keycloak</button>;
  }

  return (
    <div>
      <span>Welcome, {user?.profile?.name}!</span>
      <button onClick={logout}>Logout</button>
    </div>
  );
}
```

## Role-Based Access Control

### Conditional Rendering Based on Roles

```typescript
import { useAuth } from './context/AuthContext';

function Dashboard() {
  const { hasRole } = useAuth();

  return (
    <div>
      <h1>Dashboard</h1>
      
      {/* Show admin panel only to admins */}
      {hasRole('Admin') && (
        <section>
          <h2>Admin Panel</h2>
          <AdminControls />
        </section>
      )}
      
      {/* Show organizer tools to admins and organizers */}
      {(hasRole('Admin') || hasRole('Organizator')) && (
        <section>
          <h2>Event Management</h2>
          <EventManagementTools />
        </section>
      )}
      
      {/* Show to all authenticated users */}
      <section>
        <h2>My Events</h2>
        <MyEventsList />
      </section>
    </div>
  );
}
```

### Role-Based Navigation Menu

```typescript
import { useAuth } from './context/AuthContext';

function NavigationMenu() {
  const { hasRole } = useAuth();

  return (
    <nav>
      <ul>
        <li><a href="/">Home</a></li>
        <li><a href="/eventos">Events</a></li>
        <li><a href="/mis-entradas">My Tickets</a></li>
        
        {/* Admin-only menu items */}
        {hasRole('Admin') && (
          <>
            <li><a href="/usuarios">Users</a></li>
            <li><a href="/reportes">Reports</a></li>
          </>
        )}
        
        {/* Organizer menu items */}
        {hasRole('Organizator') && (
          <li><a href="/reportes">Reports</a></li>
        )}
      </ul>
    </nav>
  );
}
```

## Protected Routes

### Basic Protected Route

```typescript
import { ProtectedRoute } from './routes';

function App() {
  return (
    <Routes>
      {/* Public route */}
      <Route path="/login" element={<LoginPage />} />
      
      {/* Protected routes */}
      <Route
        path="/dashboard"
        element={
          <ProtectedRoute>
            <Dashboard />
          </ProtectedRoute>
        }
      />
      
      <Route
        path="/eventos"
        element={
          <ProtectedRoute>
            <EventosPage />
          </ProtectedRoute>
        }
      />
    </Routes>
  );
}
```

### Role-Based Protected Route

```typescript
import { ProtectedRoute, RoleBasedRoute } from './routes';

function App() {
  return (
    <Routes>
      {/* Admin-only route */}
      <Route
        path="/usuarios"
        element={
          <ProtectedRoute>
            <RoleBasedRoute requiredRoles={['Admin']}>
              <UsuariosPage />
            </RoleBasedRoute>
          </ProtectedRoute>
        }
      />
      
      {/* Admin or Organizer route */}
      <Route
        path="/reportes"
        element={
          <ProtectedRoute>
            <RoleBasedRoute requiredRoles={['Admin', 'Organizator']}>
              <ReportesPage />
            </RoleBasedRoute>
          </ProtectedRoute>
        }
      />
    </Routes>
  );
}
```

## Token Management

### Accessing the Token

```typescript
import { useAuth } from './context/AuthContext';

function MyComponent() {
  const { token } = useAuth();

  const fetchData = async () => {
    const response = await fetch('/api/data', {
      headers: {
        'Authorization': `Bearer ${token}`,
      },
    });
    return response.json();
  };

  // ...
}
```

### Token Expiration Display

```typescript
import { useAuth } from './context/AuthContext';
import { useState, useEffect } from 'react';

function TokenExpirationDisplay() {
  const { user } = useAuth();
  const [timeRemaining, setTimeRemaining] = useState<string>('');

  useEffect(() => {
    if (!user?.expires_at) return;

    const interval = setInterval(() => {
      const now = Date.now() / 1000;
      const remaining = user.expires_at - now;
      
      if (remaining <= 0) {
        setTimeRemaining('Token expired');
      } else {
        const minutes = Math.floor(remaining / 60);
        const seconds = Math.floor(remaining % 60);
        setTimeRemaining(`${minutes}m ${seconds}s`);
      }
    }, 1000);

    return () => clearInterval(interval);
  }, [user?.expires_at]);

  return (
    <div>
      <small>Token expires in: {timeRemaining}</small>
    </div>
  );
}
```

## User Profile Display

### User Information Card

```typescript
import { useAuth } from './context/AuthContext';

function UserProfileCard() {
  const { user, roles } = useAuth();

  if (!user) return null;

  return (
    <div className="profile-card">
      <h3>User Profile</h3>
      <dl>
        <dt>Username:</dt>
        <dd>{user.profile?.preferred_username}</dd>
        
        <dt>Name:</dt>
        <dd>{user.profile?.name}</dd>
        
        <dt>Email:</dt>
        <dd>{user.profile?.email}</dd>
        
        <dt>Roles:</dt>
        <dd>{roles.join(', ') || 'No roles assigned'}</dd>
        
        <dt>Email Verified:</dt>
        <dd>{user.profile?.email_verified ? '✅ Yes' : '❌ No'}</dd>
      </dl>
    </div>
  );
}
```

### User Avatar with Dropdown

```typescript
import { useAuth } from './context/AuthContext';
import { useState } from 'react';

function UserAvatar() {
  const { user, logout } = useAuth();
  const [isOpen, setIsOpen] = useState(false);

  if (!user) return null;

  return (
    <div className="user-avatar">
      <button onClick={() => setIsOpen(!isOpen)}>
        {user.profile?.name?.charAt(0) || 'U'}
      </button>
      
      {isOpen && (
        <div className="dropdown">
          <div className="user-info">
            <strong>{user.profile?.name}</strong>
            <small>{user.profile?.email}</small>
          </div>
          <hr />
          <button onClick={() => { /* navigate to profile */ }}>
            Profile
          </button>
          <button onClick={() => { /* navigate to settings */ }}>
            Settings
          </button>
          <hr />
          <button onClick={logout}>
            Logout
          </button>
        </div>
      )}
    </div>
  );
}
```

## Loading States

### Authentication Loading Screen

```typescript
import { useAuth } from './context/AuthContext';

function App() {
  const { isLoading } = useAuth();

  if (isLoading) {
    return (
      <div className="loading-screen">
        <div className="spinner" />
        <h2>Authenticating...</h2>
        <p>Please wait while we verify your credentials.</p>
      </div>
    );
  }

  return <MainApp />;
}
```

### Conditional Loading in Components

```typescript
import { useAuth } from './context/AuthContext';

function MyComponent() {
  const { isLoading, isAuthenticated } = useAuth();

  if (isLoading) {
    return <LoadingSpinner />;
  }

  if (!isAuthenticated) {
    return <LoginPrompt />;
  }

  return <AuthenticatedContent />;
}
```

## Error Handling

### Authentication Error Display

```typescript
import { useAuth } from './context/AuthContext';
import { useEffect, useState } from 'react';

function AuthErrorHandler() {
  const { isAuthenticated } = useAuth();
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    // Check for authentication errors in URL
    const params = new URLSearchParams(window.location.search);
    const errorParam = params.get('error');
    const errorDescription = params.get('error_description');

    if (errorParam) {
      setError(errorDescription || errorParam);
      // Clean up URL
      window.history.replaceState({}, document.title, window.location.pathname);
    }
  }, []);

  if (error) {
    return (
      <div className="error-banner">
        <h3>Authentication Error</h3>
        <p>{error}</p>
        <button onClick={() => setError(null)}>Dismiss</button>
      </div>
    );
  }

  return null;
}
```

## Advanced Patterns

### Automatic Redirect After Login

```typescript
import { useAuth } from './context/AuthContext';
import { useEffect } from 'react';

function LoginPage() {
  const { isAuthenticated, login } = useAuth();

  useEffect(() => {
    if (isAuthenticated) {
      // Redirect to intended page or dashboard
      const intendedPath = sessionStorage.getItem('intended_path') || '/dashboard';
      sessionStorage.removeItem('intended_path');
      window.location.href = intendedPath;
    }
  }, [isAuthenticated]);

  return (
    <div className="login-page">
      <h1>Welcome to Kairo Events</h1>
      <button onClick={login}>Login with Keycloak</button>
    </div>
  );
}
```

### Remember Intended Path

```typescript
import { useAuth } from './context/AuthContext';
import { useEffect } from 'react';

function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const { isAuthenticated, isLoading } = useAuth();

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      // Save current path before redirecting to login
      sessionStorage.setItem('intended_path', window.location.pathname);
    }
  }, [isAuthenticated, isLoading]);

  if (isLoading) {
    return <LoadingSpinner />;
  }

  if (!isAuthenticated) {
    window.location.href = '/login';
    return null;
  }

  return <>{children}</>;
}
```

### Role-Based Component Wrapper

```typescript
import { useAuth } from './context/AuthContext';

interface RequireRoleProps {
  roles: string[];
  children: React.ReactNode;
  fallback?: React.ReactNode;
}

function RequireRole({ roles, children, fallback = null }: RequireRoleProps) {
  const { hasRole } = useAuth();

  const hasRequiredRole = roles.some(role => hasRole(role));

  if (!hasRequiredRole) {
    return <>{fallback}</>;
  }

  return <>{children}</>;
}

// Usage
function Dashboard() {
  return (
    <div>
      <h1>Dashboard</h1>
      
      <RequireRole 
        roles={['Admin']} 
        fallback={<p>Admin access required</p>}
      >
        <AdminPanel />
      </RequireRole>
    </div>
  );
}
```

## Testing

### Mocking Authentication in Tests

```typescript
import { render, screen } from '@testing-library/react';
import { AppAuthProvider } from './context/AuthContext';

// Mock the auth context for testing
const mockAuthContext = {
  user: {
    profile: {
      name: 'Test User',
      email: 'test@example.com',
      preferred_username: 'testuser',
    },
  },
  token: 'mock-token',
  roles: ['Admin'],
  isAuthenticated: true,
  isLoading: false,
  login: jest.fn(),
  logout: jest.fn(),
  hasRole: (role: string) => role === 'Admin',
};

// Test component with mocked auth
test('renders admin panel for admin users', () => {
  render(
    <AppAuthProvider>
      <Dashboard />
    </AppAuthProvider>
  );

  expect(screen.getByText('Admin Panel')).toBeInTheDocument();
});
```

## Best Practices

1. **Always check isLoading**: Show loading states while authentication is in progress
2. **Handle errors gracefully**: Display user-friendly error messages
3. **Clean up on logout**: Ensure all user data is cleared
4. **Use role checks consistently**: Always use `hasRole()` for role-based logic
5. **Protect sensitive routes**: Wrap all protected content with `ProtectedRoute`
6. **Don't store sensitive data**: Only store tokens, not passwords or sensitive user data
7. **Test authentication flows**: Write tests for login, logout, and role-based access

## Common Pitfalls

1. **Not checking isLoading**: Can cause flickering or incorrect redirects
2. **Hardcoding roles**: Use constants or enums for role names
3. **Forgetting to clean up**: Always clean localStorage/sessionStorage on logout
4. **Not handling token expiration**: Let the OIDC client handle automatic renewal
5. **Exposing tokens in logs**: Never log tokens or sensitive data

## Additional Resources

- [Authentication Documentation](./AUTHENTICATION.md)
- [Keycloak Documentation](https://www.keycloak.org/documentation)
- [react-oidc-context GitHub](https://github.com/authts/react-oidc-context)
