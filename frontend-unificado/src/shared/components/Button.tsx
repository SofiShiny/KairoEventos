import {
  Button as MuiButton,
  type ButtonProps as MuiButtonProps,
  CircularProgress,
} from '@mui/material';
import { type ReactNode } from 'react';

interface ButtonProps extends Omit<MuiButtonProps, 'children'> {
  children: ReactNode;
  loading?: boolean;
}

/**
 * Button - Enhanced MUI Button with loading state
 * Automatically disables and shows spinner when loading
 * 
 * Accessibility Features:
 * - Proper aria-busy state when loading
 * - Disabled state management
 */
export function Button({ children, loading, disabled, ...props }: ButtonProps) {
  return (
    <MuiButton
      {...props}
      disabled={disabled || loading}
      aria-busy={loading}
      startIcon={
        loading ? (
          <CircularProgress size={16} color="inherit" aria-hidden="true" />
        ) : (
          props.startIcon
        )
      }
    >
      {children}
    </MuiButton>
  );
}
