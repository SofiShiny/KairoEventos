import { Box, Typography, Button, Alert } from '@mui/material';
import ErrorOutlineIcon from '@mui/icons-material/ErrorOutline';
import RefreshIcon from '@mui/icons-material/Refresh';

interface ErrorMessageProps {
  error: Error | string;
  onRetry?: () => void;
  variant?: 'inline' | 'centered';
}

/**
 * ErrorMessage - Displays error messages with optional retry button
 * Supports both inline and centered display modes
 */
export function ErrorMessage({
  error,
  onRetry,
  variant = 'inline',
}: ErrorMessageProps) {
  const errorMessage = typeof error === 'string' ? error : error.message;

  if (variant === 'centered') {
    return (
      <Box
        sx={{
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          justifyContent: 'center',
          p: 4,
          textAlign: 'center',
        }}
      >
        <ErrorOutlineIcon
          color="error"
          sx={{ fontSize: 60, mb: 2 }}
          aria-hidden="true"
        />
        <Typography variant="h6" color="error" gutterBottom>
          Something went wrong
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          {errorMessage}
        </Typography>
        {onRetry && (
          <Button
            variant="contained"
            startIcon={<RefreshIcon />}
            onClick={onRetry}
          >
            Try Again
          </Button>
        )}
      </Box>
    );
  }

  return (
    <Alert
      severity="error"
      action={
        onRetry && (
          <Button
            color="inherit"
            size="small"
            startIcon={<RefreshIcon />}
            onClick={onRetry}
          >
            Retry
          </Button>
        )
      }
    >
      {errorMessage}
    </Alert>
  );
}
