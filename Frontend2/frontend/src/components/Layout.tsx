import React, { useState } from 'react';
import {
  AppBar,
  Toolbar,
  Typography,
  Button,
  Box,
  Container,
  IconButton,
  Menu,
  MenuItem,
  Divider,
  ListItemIcon,
  ListItemText
} from '@mui/material';
import { Outlet, Link as RouterLink, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { UserAvatar } from './UserAvatar';
import { LanguageSwitcher } from './LanguageSwitcher';
import { useAuth } from '../context/AuthContext';
import PersonIcon from '@mui/icons-material/Person';
import LogoutIcon from '@mui/icons-material/Logout';
import SettingsIcon from '@mui/icons-material/Settings';

const Layout: React.FC = () => {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const { user, logout } = useAuth();
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const open = Boolean(anchorEl);

  const handleMenuOpen = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleMenuClose = () => {
    setAnchorEl(null);
  };

  const handleProfileClick = () => {
    handleMenuClose();
    navigate('/perfil');
  };

  const handleLogout = () => {
    handleMenuClose();
    logout();
    navigate('/');
  };

  // Obtener nombre del usuario
  const userName = user?.nombre || user?.username || 'Usuario';

  return (
    <>
      <AppBar position="static" sx={{ background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)' }}>
        <Toolbar>
          <Typography
            variant="h6"
            component="div"
            sx={{
              flexGrow: 1,
              cursor: 'pointer',
              fontWeight: 700,
              letterSpacing: '0.5px'
            }}
            onClick={() => navigate('/usuario')}
          >
            Sistema de Eventos
          </Typography>

          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <Button
              color="inherit"
              component={RouterLink}
              to="/usuario"
              sx={{ fontWeight: 500 }}
            >
              {t('navbar.home')}
            </Button>

            {/* Temporary Navigation Buttons */}
            <Button
              color="inherit"
              variant="outlined"
              sx={{
                borderColor: 'rgba(255, 255, 255, 0.5)',
                '&:hover': {
                  borderColor: 'white',
                  backgroundColor: 'rgba(255, 255, 255, 0.1)'
                }
              }}
              component={RouterLink}
              to="/admin/dashboard"
            >
              Admin
            </Button>

            <Button
              color="inherit"
              variant="outlined"
              sx={{
                borderColor: 'rgba(255, 255, 255, 0.5)',
                '&:hover': {
                  borderColor: 'white',
                  backgroundColor: 'rgba(255, 255, 255, 0.1)'
                }
              }}
              component={RouterLink}
              to="/organizador/dashboard"
            >
              Organizador
            </Button>

            {/* User Avatar with Menu */}
            {user ? (
              <>
                <IconButton
                  onClick={handleMenuOpen}
                  size="small"
                  sx={{ ml: 1 }}
                  aria-controls={open ? 'account-menu' : undefined}
                  aria-haspopup="true"
                  aria-expanded={open ? 'true' : undefined}
                >
                  <UserAvatar name={userName} size="sm" />
                </IconButton>

                <Menu
                  anchorEl={anchorEl}
                  id="account-menu"
                  open={open}
                  onClose={handleMenuClose}
                  onClick={handleMenuClose}
                  PaperProps={{
                    elevation: 3,
                    sx: {
                      overflow: 'visible',
                      filter: 'drop-shadow(0px 2px 8px rgba(0,0,0,0.15))',
                      mt: 1.5,
                      minWidth: 200,
                      '& .MuiAvatar-root': {
                        width: 32,
                        height: 32,
                        ml: -0.5,
                        mr: 1,
                      },
                      '&:before': {
                        content: '""',
                        display: 'block',
                        position: 'absolute',
                        top: 0,
                        right: 14,
                        width: 10,
                        height: 10,
                        bgcolor: 'background.paper',
                        transform: 'translateY(-50%) rotate(45deg)',
                        zIndex: 0,
                      },
                    },
                  }}
                  transformOrigin={{ horizontal: 'right', vertical: 'top' }}
                  anchorOrigin={{ horizontal: 'right', vertical: 'bottom' }}
                >
                  {/* User Info Header */}
                  <Box sx={{ px: 2, py: 1.5, borderBottom: '1px solid #e0e0e0' }}>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
                      <UserAvatar name={userName} size="md" />
                      <Box>
                        <Typography variant="subtitle2" sx={{ fontWeight: 600 }}>
                          {userName}
                        </Typography>
                        <Typography variant="caption" color="text.secondary">
                          @{user.username}
                        </Typography>
                      </Box>
                    </Box>
                  </Box>

                  <MenuItem onClick={handleProfileClick}>
                    <ListItemIcon>
                      <PersonIcon fontSize="small" />
                    </ListItemIcon>
                    <ListItemText>{t('navbar.profile')}</ListItemText>
                  </MenuItem>

                  <MenuItem onClick={handleMenuClose}>
                    <ListItemIcon>
                      <SettingsIcon fontSize="small" />
                    </ListItemIcon>
                    <ListItemText>{t('navbar.settings')}</ListItemText>
                  </MenuItem>

                  <Divider />

                  <MenuItem onClick={handleLogout}>
                    <ListItemIcon>
                      <LogoutIcon fontSize="small" />
                    </ListItemIcon>
                    <ListItemText>{t('navbar.logout')}</ListItemText>
                  </MenuItem>
                </Menu>
              </>
            ) : (
              <Button
                color="inherit"
                variant="outlined"
                sx={{
                  borderColor: 'white',
                  '&:hover': {
                    borderColor: 'white',
                    backgroundColor: 'rgba(255, 255, 255, 0.1)'
                  }
                }}
                component={RouterLink}
                to="/login"
              >
                {t('navbar.login')}
              </Button>
            )}

            <LanguageSwitcher />
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
