# Authentication with Keycloak (OIDC)

This document describes the authentication implementation using Keycloak and OpenID Connect (OIDC) in the Frontend Unificado application.

## Overview

The application uses `react-oidc-context` library to integrate with Keycloak for authentication. This provides:

- **Single Sign-On (SSO)**: Users authenticate once with Keycloak
- **Automatic Token Renewal**: Tokens are automatically renewed before expiration
- **Role-Based Access Control**: Roles are extracted from JWT tokens
- **Secure Logout**: Complete cleanup of authentication state

## Configuration

### Environment Variables

The following environment variables must be configured:

```bash
VITE_KEYCLOAK_URL=http://localhost:8180
VITE_KEYCLOAK_REALM=Kairo
VITE_KEYCLOAK_CLIENT_ID=kairo-web
```

### Keycloak Setup

1. **Realm**: `Kairo`
2. **Client**: `kairo-web`
3. **Client Configuration**:
   - Access Type: `public`
   - Standard Flow Enabled: `ON`
   - Valid Redirect URIs: `http://localhost:3000/*`
   - Web Origins: `http://localhost:3000`

4. **Roles**: The application expects the following roles:
   - `Admin`: Full administrative access
   - `Organizator`: Event organizer access
   - `Asistente`: Regular user access

## Architecture

### Components

1. **AppAuthProvider**: Main authentication provider that wraps the application
2. **AuthContextProvider**: Internal provider that adds custom functionality
3. **useAuth Hook**: Custom hook to access authentication state and methods

### Authentication Flow

```
User clicks "Login" 
  → signinRedirect() 
  → Redirects to Keycloak 
  → User authenticates 
  → Keycloak redirects back with code 
  → OIDC client exchanges code for tokens 
  → User is authenticated
```

### Token Management

- **Access Token**: Stored in memory and localStorage
- **Refresh Token**: Managed automatically by `oidc-client-ts`
- **Automatic Renewal**: Tokens are renewed 5 minutes before expiration
- **Token Expiration**: Displayed in user interface

## Usage

### Basic Authentication

```typescript
import { useAuth } from './context/AuthContext';

function MyComponent() {
  const { isAuthenticated, login, logout, user } = useAuth();

  if (!isAuthenticated) {
    return <button onClick={login}>Login</button>;
  }

  return (
    <div>
      <p>Welcome, {user?.profile?.name}!</p>
      <button onClick={logout}>Logout</button>
    </div>
  );
}
```

### Role-Based Access

```typescript
import { useAuth } from './context/AuthContext';

function AdminPanel() {
  const { hasRole } = useAuth();

  if (!hasRole('Admin')) {
    return <div>Access Denied</div>;
  }

  return <div>Admin Panel Content</div>;
}
```

### Protected Routes

```typescript
import { useAuth } from './context/AuthContext';
import { Navigate } from 'react-router-dom';

function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const { isAuthenticated, isLoading } = useAuth();

  if (isLoading) {
    return <div>Loading...</div>;
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" />;
  }

  return <>{children}</>;
}
```

## API Reference

### useAuth Hook

Returns an object with the following properties and methods:

#### Properties

- `user: User | null | undefined` - The authenticated user object
- `token: string | null` - The current access token
- `roles: string[]` - Array of user roles extracted from JWT
- `isAuthenticated: boolean` - Whether the user is authenticated
- `isLoading: boolean` - Whether authentication is in progress

#### Methods

- `login(): Promise<void>` - Initiates the login flow
- `logout(): void` - Logs out the user and cleans up state
- `hasRole(role: string): boolean` - Checks if user has a specific role

## Token Structure

The JWT token from Keycloak contains:

```json
{
  "sub": "user-id",
  "preferred_username": "username",
  "name": "Full Name",
  "email": "user@example.com",
  "realm_access": {
    "roles": ["Admin", "Organizator"]
  },
  "resource_access": {
    "kairo-web": {
      "roles": ["custom-role"]
    }
  }
}
```

Roles are extracted from both `realm_access.roles` and `resource_access[client_id].roles`.

## State Management

### Persistence

- **Access Token**: Stored in `localStorage` as `auth_token`
- **User Profile**: Managed in memory by `react-oidc-context`
- **Roles**: Computed from token and stored in React state

### Cleanup on Logout

When the user logs out, the following cleanup occurs:

1. Remove `auth_token` from localStorage
2. Remove `auth_user` from localStorage
3. Clear all sessionStorage
4. Redirect to Keycloak logout endpoint
5. Keycloak redirects back to application

## Security Considerations

### Token Storage

- Access tokens are stored in localStorage for persistence across page reloads
- Tokens are short-lived (typically 5-15 minutes)
- Refresh tokens are managed securely by the OIDC client

### CSRF Protection

- OIDC uses state parameter to prevent CSRF attacks
- All requests include the access token in Authorization header

### XSS Protection

- React automatically escapes content to prevent XSS
- Tokens are not exposed in URLs or logs

## Troubleshooting

### Common Issues

1. **"Invalid redirect URI"**
   - Ensure the redirect URI is configured in Keycloak client settings
   - Check that the URL matches exactly (including protocol and port)

2. **"Roles not appearing"**
   - Verify roles are assigned to the user in Keycloak
   - Check that roles are in `realm_access.roles` or `resource_access[client_id].roles`

3. **"Token expired"**
   - Automatic renewal should handle this
   - If renewal fails, user will be redirected to login

4. **"CORS errors"**
   - Ensure Web Origins is configured in Keycloak client
   - Check that the origin matches the application URL

### Debug Mode

To enable debug logging:

```typescript
// In AuthContext.tsx, add to oidcConfig:
{
  ...oidcConfig,
  monitorSession: true,
  onSigninCallback: (user) => {
    console.log('Signin callback:', user);
  },
}
```

## Testing

### Manual Testing

1. Start the application: `npm run dev`
2. Click "Login with Keycloak"
3. Authenticate with Keycloak
4. Verify user information is displayed
5. Verify roles are correct
6. Click "Logout"
7. Verify redirect to login page

### Automated Testing

See `src/context/AuthContext.test.tsx` for unit tests.

## References

- [react-oidc-context Documentation](https://github.com/authts/react-oidc-context)
- [oidc-client-ts Documentation](https://github.com/authts/oidc-client-ts)
- [Keycloak Documentation](https://www.keycloak.org/documentation)
- [OpenID Connect Specification](https://openid.net/connect/)
