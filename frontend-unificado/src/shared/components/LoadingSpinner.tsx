import { Box, CircularProgress, Backdrop } from '@mui/material';

interface LoadingSpinnerProps {
  size?: 'small' | 'medium' | 'large';
  fullScreen?: boolean;
}

/**
 * LoadingSpinner - Displays a loading indicator
 * Can be used inline or as a full-screen overlay
 */
export function LoadingSpinner({
  size = 'medium',
  fullScreen = false,
}: LoadingSpinnerProps) {
  const sizeMap = {
    small: 24,
    medium: 40,
    large: 60,
  };

  const spinner = (
    <CircularProgress
      size={sizeMap[size]}
      aria-label="Loading..."
      role="status"
    />
  );

  if (fullScreen) {
    return (
      <Backdrop
        open={true}
        sx={{
          color: '#fff',
          zIndex: (theme) => theme.zIndex.drawer + 1,
        }}
      >
        {spinner}
      </Backdrop>
    );
  }

  return (
    <Box
      sx={{
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'center',
        p: 2,
      }}
    >
      {spinner}
    </Box>
  );
}
