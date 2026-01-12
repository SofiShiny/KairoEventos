import {
  TextField as MuiTextField,
  type TextFieldProps as MuiTextFieldProps,
} from '@mui/material';
import { forwardRef } from 'react';

export interface TextFieldProps extends Omit<MuiTextFieldProps, 'variant'> {
  variant?: 'outlined' | 'filled' | 'standard';
}

/**
 * TextField - Enhanced MUI TextField with consistent styling
 * Provides a standardized text input component across the app
 * 
 * Accessibility Features:
 * - Automatic label association via MUI
 * - Proper aria-describedby for helper text
 * - Error state announcements
 */
export const TextField = forwardRef<HTMLDivElement, TextFieldProps>(
  ({ variant = 'outlined', fullWidth = true, required, label, ...props }, ref) => {
    return (
      <MuiTextField
        ref={ref}
        variant={variant}
        fullWidth={fullWidth}
        required={required}
        label={label}
        inputProps={{
          'aria-required': required,
          ...props.inputProps,
        }}
        {...props}
      />
    );
  }
);

TextField.displayName = 'TextField';
