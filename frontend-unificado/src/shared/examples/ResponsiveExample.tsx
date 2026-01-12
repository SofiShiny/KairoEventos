import { Box, Paper, Typography, useTheme, useMediaQuery, Stack } from '@mui/material';

/**
 * Componente de ejemplo que demuestra el diseño responsive con breakpoints de Material UI
 * 
 * Breakpoints configurados:
 * - xs: 0px (móvil)
 * - sm: 600px (tablet pequeña)
 * - md: 960px (tablet)
 * - lg: 1280px (desktop)
 * - xl: 1920px (desktop grande)
 */
export function ResponsiveExample() {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));
  const isTablet = useMediaQuery(theme.breakpoints.between('sm', 'md'));

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4" gutterBottom>
        Responsive Design Example
      </Typography>

      <Paper sx={{ p: 2, mb: 3, bgcolor: 'info.light', color: 'white' }}>
        <Typography variant="h6">
          Current Breakpoint: {isMobile ? 'Mobile' : isTablet ? 'Tablet' : 'Desktop'}
        </Typography>
        <Typography variant="body2">
          Resize your browser window to see the responsive behavior
        </Typography>
      </Paper>

      {/* Responsive cards usando Box y flexbox */}
      <Box
        sx={{
          display: 'flex',
          flexWrap: 'wrap',
          gap: 2,
          mb: 3,
        }}
      >
        <Box
          sx={{
            flex: { xs: '1 1 100%', sm: '1 1 calc(50% - 8px)', md: '1 1 calc(33.333% - 11px)' },
          }}
        >
          <Paper sx={{ p: 2, height: '100%' }}>
            <Typography variant="h6" color="primary">
              Card 1
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Full width on mobile, half on tablet, third on desktop
            </Typography>
          </Paper>
        </Box>
        <Box
          sx={{
            flex: { xs: '1 1 100%', sm: '1 1 calc(50% - 8px)', md: '1 1 calc(33.333% - 11px)' },
          }}
        >
          <Paper sx={{ p: 2, height: '100%' }}>
            <Typography variant="h6" color="primary">
              Card 2
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Full width on mobile, half on tablet, third on desktop
            </Typography>
          </Paper>
        </Box>
        <Box
          sx={{
            flex: { xs: '1 1 100%', sm: '1 1 calc(50% - 8px)', md: '1 1 calc(33.333% - 11px)' },
          }}
        >
          <Paper sx={{ p: 2, height: '100%' }}>
            <Typography variant="h6" color="primary">
              Card 3
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Full width on mobile, half on tablet, third on desktop
            </Typography>
          </Paper>
        </Box>
      </Box>

      {/* Ejemplo de espaciado responsive */}
      <Box
        sx={{
          mt: { xs: 2, sm: 3, md: 4 }, // Margen superior responsive
          p: { xs: 2, sm: 3, md: 4 }, // Padding responsive
          bgcolor: 'background.paper',
          borderRadius: 2,
        }}
      >
        <Typography variant="h6" gutterBottom>
          Responsive Spacing
        </Typography>
        <Typography variant="body2" color="text.secondary">
          This box has different padding and margin based on screen size
        </Typography>
      </Box>

      {/* Ejemplo de tipografía responsive */}
      <Box sx={{ mt: 3 }}>
        <Typography
          variant="h3"
          sx={{
            fontSize: { xs: '1.5rem', sm: '2rem', md: '2.5rem' }, // Tamaño de fuente responsive
          }}
        >
          Responsive Typography
        </Typography>
        <Typography variant="body1" color="text.secondary">
          The heading above changes size based on screen width
        </Typography>
      </Box>

      {/* Ejemplo de Stack responsive */}
      <Stack
        direction={{ xs: 'column', sm: 'row' }}
        spacing={2}
        sx={{ mt: 3 }}
      >
        <Paper sx={{ p: 2, flex: 1 }}>
          <Typography variant="body1">Stack Item 1</Typography>
        </Paper>
        <Paper sx={{ p: 2, flex: 1 }}>
          <Typography variant="body1">Stack Item 2</Typography>
        </Paper>
        <Paper sx={{ p: 2, flex: 1 }}>
          <Typography variant="body1">Stack Item 3</Typography>
        </Paper>
      </Stack>
    </Box>
  );
}
