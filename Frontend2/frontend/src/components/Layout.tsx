import React from 'react';
import { AppBar, Toolbar, Typography, Button, Box, Container } from '@mui/material';
import { Outlet, Link as RouterLink, useNavigate } from 'react-router-dom';

const Layout: React.FC = () => {
  const navigate = useNavigate();

  return (
    <>
      <AppBar position="static">
        <Toolbar>
          <Typography 
            variant="h6" 
            component="div" 
            sx={{ flexGrow: 1, cursor: 'pointer' }} 
            onClick={() => navigate('/usuario')}
          >
            Sistema de Eventos
          </Typography>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <Button color="inherit" component={RouterLink} to="/usuario">
              Usuario
            </Button>
            {/* Temporary Navigation Buttons as requested */}
            <Button 
              color="inherit" 
              variant="outlined" 
              sx={{ borderColor: 'white' }} 
              component={RouterLink} 
              to="/admin/dashboard"
            >
              Admin
            </Button>
            <Button 
              color="inherit" 
              variant="outlined" 
              sx={{ borderColor: 'white' }} 
              component={RouterLink} 
              to="/organizador/dashboard"
            >
              Organizador
            </Button>
          </Box>
        </Toolbar>
      </AppBar>
      <Container sx={{ mt: 4, mb: 4 }}>
        <Outlet />
      </Container>
    </>
  );
};

export default Layout;
