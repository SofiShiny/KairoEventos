import {
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Paper,
  Stack,
  TextField,
  Typography,
} from '@mui/material';
import {
  Home as HomeIcon,
  Settings as SettingsIcon,
  Person as PersonIcon,
  Event as EventIcon,
} from '@mui/icons-material';

/**
 * Componente showcase que demuestra todos los componentes y estilos del tema
 */
export function ThemeShowcase() {
  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h3" gutterBottom color="primary">
        Theme Showcase
      </Typography>
      <Typography variant="body1" color="text.secondary" sx={{ mb: 4 }}>
        Demostración de todos los componentes y estilos del tema personalizado
      </Typography>

      {/* Colores */}
      <Paper sx={{ p: 3, mb: 3 }}>
        <Typography variant="h5" gutterBottom>
          Paleta de Colores
        </Typography>
        <Stack direction="row" spacing={2} flexWrap="wrap" useFlexGap>
          <Box sx={{ bgcolor: 'primary.main', color: 'white', p: 2, borderRadius: 2, minWidth: 120 }}>
            Primary
          </Box>
          <Box sx={{ bgcolor: 'secondary.main', color: 'white', p: 2, borderRadius: 2, minWidth: 120 }}>
            Secondary
          </Box>
          <Box sx={{ bgcolor: 'error.main', color: 'white', p: 2, borderRadius: 2, minWidth: 120 }}>
            Error
          </Box>
          <Box sx={{ bgcolor: 'warning.main', color: 'white', p: 2, borderRadius: 2, minWidth: 120 }}>
            Warning
          </Box>
          <Box sx={{ bgcolor: 'info.main', color: 'white', p: 2, borderRadius: 2, minWidth: 120 }}>
            Info
          </Box>
          <Box sx={{ bgcolor: 'success.main', color: 'white', p: 2, borderRadius: 2, minWidth: 120 }}>
            Success
          </Box>
        </Stack>
      </Paper>

      {/* Tipografía */}
      <Paper sx={{ p: 3, mb: 3 }}>
        <Typography variant="h5" gutterBottom>
          Tipografía
        </Typography>
        <Stack spacing={1}>
          <Typography variant="h1">Heading 1 - 2.5rem</Typography>
          <Typography variant="h2">Heading 2 - 2rem</Typography>
          <Typography variant="h3">Heading 3 - 1.75rem</Typography>
          <Typography variant="h4">Heading 4 - 1.5rem</Typography>
          <Typography variant="h5">Heading 5 - 1.25rem</Typography>
          <Typography variant="h6">Heading 6 - 1rem</Typography>
          <Typography variant="body1">Body 1 - 1rem - Lorem ipsum dolor sit amet</Typography>
          <Typography variant="body2">Body 2 - 0.875rem - Lorem ipsum dolor sit amet</Typography>
        </Stack>
      </Paper>

      {/* Botones */}
      <Paper sx={{ p: 3, mb: 3 }}>
        <Typography variant="h5" gutterBottom>
          Botones
        </Typography>
        <Stack direction="row" spacing={2} flexWrap="wrap" useFlexGap sx={{ mb: 2 }}>
          <Button variant="contained" color="primary">
            Primary
          </Button>
          <Button variant="contained" color="secondary">
            Secondary
          </Button>
          <Button variant="contained" color="error">
            Error
          </Button>
          <Button variant="contained" color="success">
            Success
          </Button>
        </Stack>
        <Stack direction="row" spacing={2} flexWrap="wrap" useFlexGap sx={{ mb: 2 }}>
          <Button variant="outlined" color="primary">
            Outlined
          </Button>
          <Button variant="outlined" color="secondary">
            Outlined
          </Button>
          <Button variant="text" color="primary">
            Text
          </Button>
        </Stack>
        <Stack direction="row" spacing={2} flexWrap="wrap" useFlexGap>
          <Button variant="contained" size="small">
            Small
          </Button>
          <Button variant="contained" size="medium">
            Medium
          </Button>
          <Button variant="contained" size="large">
            Large
          </Button>
        </Stack>
      </Paper>

      {/* Iconos */}
      <Paper sx={{ p: 3, mb: 3 }}>
        <Typography variant="h5" gutterBottom>
          Iconos Material UI
        </Typography>
        <Stack direction="row" spacing={3}>
          <HomeIcon fontSize="small" color="primary" />
          <SettingsIcon fontSize="medium" color="secondary" />
          <PersonIcon fontSize="large" color="action" />
          <EventIcon sx={{ fontSize: 48 }} color="error" />
        </Stack>
      </Paper>

      {/* Cards */}
      <Paper sx={{ p: 3, mb: 3 }}>
        <Typography variant="h5" gutterBottom sx={{ mb: 2 }}>
          Cards
        </Typography>
        <Stack direction="row" spacing={2} flexWrap="wrap" useFlexGap>
          <Card sx={{ minWidth: 200 }}>
            <CardContent>
              <Typography variant="h6" color="primary" gutterBottom>
                Card Title
              </Typography>
              <Typography variant="body2" color="text.secondary">
                This is a card with custom border radius and shadow
              </Typography>
            </CardContent>
          </Card>
          <Card sx={{ minWidth: 200 }} elevation={4}>
            <CardContent>
              <Typography variant="h6" color="secondary" gutterBottom>
                Elevated Card
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Card with higher elevation
              </Typography>
            </CardContent>
          </Card>
        </Stack>
      </Paper>

      {/* Chips */}
      <Paper sx={{ p: 3, mb: 3 }}>
        <Typography variant="h5" gutterBottom>
          Chips
        </Typography>
        <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
          <Chip label="Admin" color="primary" />
          <Chip label="Organizator" color="secondary" />
          <Chip label="Asistente" color="default" />
          <Chip label="Active" color="success" />
          <Chip label="Inactive" color="error" />
        </Stack>
      </Paper>

      {/* Text Fields */}
      <Paper sx={{ p: 3, mb: 3 }}>
        <Typography variant="h5" gutterBottom>
          Text Fields
        </Typography>
        <Stack spacing={2} sx={{ maxWidth: 400 }}>
          <TextField label="Standard" variant="outlined" />
          <TextField label="Filled" variant="filled" />
          <TextField label="With Helper Text" helperText="This is helper text" />
          <TextField label="Error State" error helperText="This field has an error" />
        </Stack>
      </Paper>

      {/* Spacing */}
      <Paper sx={{ p: 3 }}>
        <Typography variant="h5" gutterBottom>
          Sistema de Espaciado (base 8px)
        </Typography>
        <Stack spacing={2}>
          <Box sx={{ bgcolor: 'primary.light', p: 1, color: 'white' }}>spacing(1) = 8px</Box>
          <Box sx={{ bgcolor: 'primary.light', p: 2, color: 'white' }}>spacing(2) = 16px</Box>
          <Box sx={{ bgcolor: 'primary.light', p: 3, color: 'white' }}>spacing(3) = 24px</Box>
          <Box sx={{ bgcolor: 'primary.light', p: 4, color: 'white' }}>spacing(4) = 32px</Box>
        </Stack>
      </Paper>
    </Box>
  );
}
