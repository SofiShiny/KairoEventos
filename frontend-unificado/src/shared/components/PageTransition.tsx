import { Box, Fade, Slide, Grow, Zoom } from '@mui/material';
import { type ReactNode } from 'react';

interface PageTransitionProps {
  children: ReactNode;
  variant?: 'fade' | 'slide' | 'grow' | 'zoom';
  direction?: 'up' | 'down' | 'left' | 'right';
  timeout?: number;
}

/**
 * PageTransition - Adds smooth transitions when navigating between pages
 * Supports multiple animation variants
 */
export function PageTransition({
  children,
  variant = 'fade',
  direction = 'up',
  timeout = 300,
}: PageTransitionProps) {
  if (variant === 'slide') {
    return (
      <Slide direction={direction} in timeout={timeout}>
        <Box>{children}</Box>
      </Slide>
    );
  }

  if (variant === 'grow') {
    return (
      <Grow in timeout={timeout}>
        <Box>{children}</Box>
      </Grow>
    );
  }

  if (variant === 'zoom') {
    return (
      <Zoom in timeout={timeout}>
        <Box>{children}</Box>
      </Zoom>
    );
  }

  // Default: fade
  return (
    <Fade in timeout={timeout}>
      <Box>{children}</Box>
    </Fade>
  );
}
