import { User as OidcUser } from 'oidc-client-ts';

// Extend the OIDC User type with our custom profile properties
export interface UserProfile {
  sub?: string;
  name?: string;
  preferred_username?: string;
  email?: string;
  email_verified?: boolean;
  realm_access?: {
    roles: string[];
  };
  resource_access?: {
    [key: string]: {
      roles: string[];
    };
  };
}

export type User = OidcUser;

export interface AuthState {
  user: User | null | undefined;
  token: string | null;
  roles: string[];
  isAuthenticated: boolean;
  isLoading: boolean;
}
