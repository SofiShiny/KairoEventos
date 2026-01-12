import React, { createContext, useContext, useEffect, useState } from 'react';
import { AuthProvider, useAuth as useOidcAuth } from 'react-oidc-context';
import { User } from 'oidc-client-ts';
import { clearQueryCache } from '@shared/config';

// Tipos para el contexto de autenticación
interface AuthContextType {
  user: User | null | undefined;
  token: string | null;
  roles: string[];
  isAuthenticated: boolean;
  isLoading: boolean;
  login: () => Promise<void>;
  logout: () => void;
  hasRole: (role: string) => boolean;
}

// Contexto interno para funcionalidades adicionales
const AuthContextInternal = createContext<AuthContextType | undefined>(undefined);

// Configuración OIDC para Keycloak
const oidcConfig = {
  authority: `${import.meta.env.VITE_KEYCLOAK_URL}/realms/${import.meta.env.VITE_KEYCLOAK_REALM}`,
  client_id: import.meta.env.VITE_KEYCLOAK_CLIENT_ID,
  redirect_uri: window.location.origin,
  post_logout_redirect_uri: window.location.origin,
  response_type: 'code',
  scope: 'openid profile email',
  automaticSilentRenew: true, // Renovación automática de tokens
  loadUserInfo: true,
  onSigninCallback: () => {
    // Limpiar parámetros de la URL después del login
    window.history.replaceState({}, document.title, window.location.pathname);
  },
};

// Provider interno que envuelve la funcionalidad de react-oidc-context
function AuthContextProvider({ children }: { children: React.ReactNode }) {
  const auth = useOidcAuth();
  const [roles, setRoles] = useState<string[]>([]);

  // Extraer roles del JWT cuando el usuario cambia
  useEffect(() => {
    if (auth.user?.profile) {
      // Los roles en Keycloak pueden estar en diferentes ubicaciones del token
      // Intentamos extraerlos de las ubicaciones más comunes
      const realmRoles = (auth.user.profile as any).realm_access?.roles || [];
      const resourceRoles = (auth.user.profile as any).resource_access?.[import.meta.env.VITE_KEYCLOAK_CLIENT_ID]?.roles || [];
      const allRoles = [...realmRoles, ...resourceRoles];
      setRoles(allRoles);
    } else {
      setRoles([]);
    }
  }, [auth.user]);

  // Función para verificar si el usuario tiene un rol específico
  const hasRole = (role: string): boolean => {
    return roles.includes(role);
  };

  // Función de login
  const login = async () => {
    await auth.signinRedirect();
  };

  // Función de logout con limpieza de estado
  const logout = () => {
    // Limpiar caché de React Query
    clearQueryCache();
    
    // Limpiar localStorage
    localStorage.removeItem('auth_token');
    localStorage.removeItem('auth_user');
    
    // Limpiar sessionStorage
    sessionStorage.clear();
    
    // Realizar logout en Keycloak
    auth.signoutRedirect();
  };

  // Persistir token en localStorage cuando cambia
  useEffect(() => {
    if (auth.user?.access_token) {
      localStorage.setItem('auth_token', auth.user.access_token);
    } else {
      localStorage.removeItem('auth_token');
    }
  }, [auth.user?.access_token]);

  const contextValue: AuthContextType = {
    user: auth.user,
    token: auth.user?.access_token || null,
    roles,
    isAuthenticated: auth.isAuthenticated,
    isLoading: auth.isLoading,
    login,
    logout,
    hasRole,
  };

  return (
    <AuthContextInternal.Provider value={contextValue}>
      {children}
    </AuthContextInternal.Provider>
  );
}

// Provider principal que combina AuthProvider de react-oidc-context con nuestro contexto
export function AppAuthProvider({ children }: { children: React.ReactNode }) {
  return (
    <AuthProvider {...oidcConfig}>
      <AuthContextProvider>
        {children}
      </AuthContextProvider>
    </AuthProvider>
  );
}

// Hook personalizado para acceder al contexto de autenticación
export function useAuth(): AuthContextType {
  const context = useContext(AuthContextInternal);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AppAuthProvider');
  }
  return context;
}
