import { Box, Typography } from '@mui/material';
import { type ReactNode } from 'react';

interface EmptyStateProps {
  icon?: ReactNode;
  title: string;
  description?: string;
  action?: ReactNode;
}

/**
 * EmptyState - Displays an informative empty state
 * Used when lists or collections have no items
 */
export function EmptyState({
  icon,
  title,
  description,
  action,
}: EmptyStateProps) {
  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        p: 4,
        textAlign: 'center',
        minHeight: 300,
      }}
      role="status"
      aria-live="polite"
    >
      {icon && (
        <Box
          sx={{
            fontSize: 80,
            color: 'text.secondary',
            mb: 2,
            opacity: 0.5,
          }}
          aria-hidden="true"
        >
          {icon}
        </Box>
      )}
      <Typography variant="h6" color="text.primary" gutterBottom>
        {title}
      </Typography>
      {description && (
        <Typography
          variant="body2"
          color="text.secondary"
          sx={{ mb: 3, maxWidth: 400 }}
        >
          {description}
        </Typography>
      )}
      {action && <Box>{action}</Box>}
    </Box>
  );
}
