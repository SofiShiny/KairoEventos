import React, { Component, ErrorInfo, ReactNode } from 'react';
import { Box, Typography, Button, Paper } from '@mui/material';

interface Props {
  children: ReactNode;
}

interface State {
  hasError: boolean;
  error: Error | null;
}

class ErrorBoundary extends Component<Props, State> {
  public state: State = {
    hasError: false,
    error: null,
  };

  public static getDerivedStateFromError(error: Error): State {
    return { hasError: true, error };
  }

  public componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    console.error('ErrorBoundary caught an error:', error, errorInfo);
  }

  private handleReload = () => {
    window.location.reload();
  };

  public render() {
    if (this.state.hasError) {
      return (
        <Box
          sx={{
            display: 'flex',
            justifyContent: 'center',
            alignItems: 'center',
            minHeight: '100vh',
            p: 3,
          }}
        >
          <Paper elevation={3} sx={{ p: 4, maxWidth: 500 }}>
            <Typography variant="h5" gutterBottom color="error">
              Algo salió mal
            </Typography>
            <Typography variant="body1" paragraph>
              La aplicación encontró un error inesperado. Por favor, recarga la página.
            </Typography>
            {this.state.error && (
              <Typography variant="body2" color="text.secondary" paragraph>
                Error: {this.state.error.message}
              </Typography>
            )}
            <Button variant="contained" onClick={this.handleReload} fullWidth>
              Recargar Página
            </Button>
          </Paper>
        </Box>
      );
    }

    return this.props.children;
  }
}

export default ErrorBoundary;
