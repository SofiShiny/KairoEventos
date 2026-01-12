import {
  Box,
  LinearProgress,
  Typography,
  CircularProgress,
} from '@mui/material';

interface ProgressIndicatorProps {
  value?: number; // 0-100 for determinate progress
  variant?: 'linear' | 'circular';
  label?: string;
  showPercentage?: boolean;
}

/**
 * ProgressIndicator - Displays progress for long-running operations
 * Supports both linear and circular variants with optional labels
 */
export function ProgressIndicator({
  value,
  variant = 'linear',
  label,
  showPercentage = true,
}: ProgressIndicatorProps) {
  const isDeterminate = value !== undefined;

  if (variant === 'circular') {
    return (
      <Box
        sx={{
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          gap: 2,
        }}
      >
        <Box sx={{ position: 'relative', display: 'inline-flex' }}>
          <CircularProgress
            variant={isDeterminate ? 'determinate' : 'indeterminate'}
            value={value}
            size={60}
          />
          {isDeterminate && showPercentage && (
            <Box
              sx={{
                top: 0,
                left: 0,
                bottom: 0,
                right: 0,
                position: 'absolute',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
              }}
            >
              <Typography variant="caption" component="div" color="text.secondary">
                {`${Math.round(value)}%`}
              </Typography>
            </Box>
          )}
        </Box>
        {label && (
          <Typography variant="body2" color="text.secondary">
            {label}
          </Typography>
        )}
      </Box>
    );
  }

  // Linear variant
  return (
    <Box sx={{ width: '100%' }}>
      {label && (
        <Box
          sx={{
            display: 'flex',
            justifyContent: 'space-between',
            mb: 1,
          }}
        >
          <Typography variant="body2" color="text.secondary">
            {label}
          </Typography>
          {isDeterminate && showPercentage && (
            <Typography variant="body2" color="text.secondary">
              {`${Math.round(value)}%`}
            </Typography>
          )}
        </Box>
      )}
      <LinearProgress
        variant={isDeterminate ? 'determinate' : 'indeterminate'}
        value={value}
      />
    </Box>
  );
}
