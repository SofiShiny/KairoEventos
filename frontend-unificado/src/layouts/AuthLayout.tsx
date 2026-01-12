import { Box, Container, Paper } from '@mui/material';
import { type ReactNode } from 'react';

interface AuthLayoutProps {
  children: ReactNode;
}

/**
 * AuthLayout - Layout for authentication screens (login, register, etc.)
 * Provides a centered, card-based layout with minimal chrome
 * 
 * Accessibility Features:
 * - Semantic main element
 * - Proper ARIA roles
 */
export function AuthLayout({ children }: AuthLayoutProps) {
  return (
    <Box
      component="main"
      sx={{
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        bgcolor: 'background.default',
        backgroundImage: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
      }}
      role="main"
    >
      <Container maxWidth="sm">
        <Paper
          elevation={6}
          sx={{
            p: 4,
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            borderRadius: 2,
          }}
          component="section"
          aria-label="Authentication form"
        >
          {children}
        </Paper>
      </Container>
    </Box>
  );
}
