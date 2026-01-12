# Material UI Theme Configuration

## Overview

El Frontend Unificado utiliza Material UI (MUI) como librería de componentes UI con un tema personalizado que define la paleta de colores, tipografía, espaciado y estilos de componentes.

## Instalación

Las siguientes dependencias están instaladas:

```bash
npm install @mui/material @mui/icons-material @emotion/react @emotion/styled
```

## Estructura del Tema

El tema está definido en `src/shared/theme/theme.ts` y se aplica globalmente mediante `ThemeProvider` en `App.tsx`.

### Paleta de Colores

```typescript
primary: {
  main: '#1976d2',    // Azul principal
  light: '#42a5f5',
  dark: '#1565c0',
}

secondary: {
  main: '#9c27b0',    // Púrpura
  light: '#ba68c8',
  dark: '#7b1fa2',
}

error: '#d32f2f'      // Rojo para errores
warning: '#ed6c02'    // Naranja para advertencias
info: '#0288d1'       // Azul para información
success: '#2e7d32'    // Verde para éxito
```

### Tipografía

Fuente base: System fonts (Roboto, Segoe UI, etc.)

```typescript
h1: 2.5rem (40px)
h2: 2rem (32px)
h3: 1.75rem (28px)
h4: 1.5rem (24px)
h5: 1.25rem (20px)
h6: 1rem (16px)
body1: 1rem (16px)
body2: 0.875rem (14px)
```

**Nota**: Los botones NO tienen texto en mayúsculas automáticamente (`textTransform: 'none'`).

### Espaciado

Base: 8px

```typescript
spacing(1) = 8px
spacing(2) = 16px
spacing(3) = 24px
spacing(4) = 32px
```

### Breakpoints Responsive

```typescript
xs: 0px      // Móvil
sm: 600px    // Tablet pequeña
md: 960px    // Tablet
lg: 1280px   // Desktop
xl: 1920px   // Desktop grande
```

## Uso del Tema

### ThemeProvider

El tema se aplica globalmente en `App.tsx`:

```tsx
import { ThemeProvider } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import { theme } from './shared/theme';

function App() {
  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      {/* Tu aplicación */}
    </ThemeProvider>
  );
}
```

**Importante**: `CssBaseline` normaliza estilos CSS entre navegadores.

### Acceder al Tema en Componentes

```tsx
import { useTheme } from '@mui/material/styles';

function MyComponent() {
  const theme = useTheme();
  
  return (
    <Box sx={{ color: theme.palette.primary.main }}>
      Texto en color primario
    </Box>
  );
}
```

### Usar Colores del Tema

```tsx
// Usando sx prop
<Button sx={{ bgcolor: 'primary.main', color: 'primary.contrastText' }}>
  Button
</Button>

// Usando color prop
<Typography color="primary">Primary Text</Typography>
<Typography color="text.secondary">Secondary Text</Typography>
```

### Diseño Responsive

#### Usando Breakpoints en sx

```tsx
<Box
  sx={{
    width: { xs: '100%', sm: '50%', md: '33%' },
    p: { xs: 2, sm: 3, md: 4 },
  }}
>
  Responsive Box
</Box>
```

#### Usando useMediaQuery

```tsx
import { useMediaQuery, useTheme } from '@mui/material';

function MyComponent() {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));
  const isDesktop = useMediaQuery(theme.breakpoints.up('md'));
  
  return (
    <div>
      {isMobile && <MobileView />}
      {isDesktop && <DesktopView />}
    </div>
  );
}
```

## Componentes Personalizados

### Botones

```tsx
<Button variant="contained" color="primary">
  Primary Button
</Button>

<Button variant="outlined" color="secondary">
  Secondary Button
</Button>

<Button variant="text">
  Text Button
</Button>
```

Estilos personalizados:
- Border radius: 8px
- Sin sombra por defecto
- Sombra sutil al hover

### Cards

```tsx
<Card>
  <CardContent>
    <Typography variant="h5">Card Title</Typography>
    <Typography variant="body2" color="text.secondary">
      Card content
    </Typography>
  </CardContent>
</Card>
```

Estilos personalizados:
- Border radius: 12px
- Sombra sutil: `0px 2px 8px rgba(0, 0, 0, 0.08)`

### Text Fields

```tsx
<TextField
  label="Email"
  variant="outlined"
  fullWidth
/>
```

Estilos personalizados:
- Border radius: 8px

### Papers

```tsx
<Paper elevation={2} sx={{ p: 3 }}>
  Content
</Paper>
```

Estilos personalizados:
- Border radius: 12px
- Sombras suaves

## Grid System

Material UI usa un sistema de 12 columnas:

```tsx
<Grid container spacing={2}>
  <Grid item xs={12} sm={6} md={4}>
    {/* Full width en móvil, mitad en tablet, tercio en desktop */}
  </Grid>
  <Grid item xs={12} sm={6} md={4}>
    {/* ... */}
  </Grid>
  <Grid item xs={12} sm={6} md={4}>
    {/* ... */}
  </Grid>
</Grid>
```

## Iconos

Material UI Icons están disponibles:

```tsx
import { Home, Settings, Person } from '@mui/icons-material';

<Home />
<Settings color="primary" />
<Person fontSize="large" />
```

## Accesibilidad

El tema está configurado con:
- Contraste de colores WCAG AA compliant
- Tamaños de fuente legibles
- Espaciado apropiado para touch targets
- Soporte completo para navegación con teclado

## Ejemplos

Ver `src/shared/examples/ResponsiveExample.tsx` para ejemplos de diseño responsive.

## Referencias

- [Material UI Documentation](https://mui.com/)
- [Material UI Theming](https://mui.com/material-ui/customization/theming/)
- [Material UI Breakpoints](https://mui.com/material-ui/customization/breakpoints/)
- [Material UI Icons](https://mui.com/material-ui/material-icons/)
