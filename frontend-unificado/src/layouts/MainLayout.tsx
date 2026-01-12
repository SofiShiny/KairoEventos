import { Outlet, useNavigate, useLocation } from 'react-router-dom';
import {
  Box,
  AppBar,
  Toolbar,
  Typography,
  Button,
  Container,
  IconButton,
  Menu,
  MenuItem,
  Chip,
  Drawer,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  useTheme,
  useMediaQuery,
} from '@mui/material';
import { useState } from 'react';
import { useAuth } from '../context/AuthContext';
import { SkipLink } from '../shared/components';
import MenuIcon from '@mui/icons-material/Menu';
import AccountCircleIcon from '@mui/icons-material/AccountCircle';
import LogoutIcon from '@mui/icons-material/Logout';
import DashboardIcon from '@mui/icons-material/Dashboard';
import EventIcon from '@mui/icons-material/Event';
import ConfirmationNumberIcon from '@mui/icons-material/ConfirmationNumber';
import PeopleIcon from '@mui/icons-material/People';
import AssessmentIcon from '@mui/icons-material/Assessment';

/**
 * MainLayout - Main application layout with navbar and sidebar
 * Used for all authenticated routes
 * 
 * Accessibility Features:
 * - Semantic HTML elements (header, nav, main, footer)
 * - Skip link for keyboard navigation
 * - ARIA labels for interactive elements
 * - Keyboard navigation support
 */
export function MainLayout() {
  const { user, roles, logout, hasRole } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));

  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const [drawerOpen, setDrawerOpen] = useState(false);

  const handleUserMenuOpen = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleUserMenuClose = () => {
    setAnchorEl(null);
  };

  const handleLogout = () => {
    handleUserMenuClose();
    logout();
  };

  const toggleDrawer = () => {
    setDrawerOpen(!drawerOpen);
  };

  // Navigation items based on user roles
  const navigationItems = [
    {
      label: 'Dashboard',
      path: '/',
      icon: <DashboardIcon />,
      show: true,
    },
    {
      label: 'Events',
      path: '/eventos',
      icon: <EventIcon />,
      show: true,
    },
    {
      label: 'My Tickets',
      path: '/mis-entradas',
      icon: <ConfirmationNumberIcon />,
      show: true,
    },
    {
      label: 'Users',
      path: '/usuarios',
      icon: <PeopleIcon />,
      show: hasRole('Admin'),
    },
    {
      label: 'Reports',
      path: '/reportes',
      icon: <AssessmentIcon />,
      show: hasRole('Admin') || hasRole('Organizator'),
    },
  ].filter((item) => item.show);

  const handleNavigation = (path: string) => {
    navigate(path);
    if (isMobile) {
      setDrawerOpen(false);
    }
  };

  const isActivePath = (path: string) => {
    if (path === '/') {
      return location.pathname === '/';
    }
    return location.pathname.startsWith(path);
  };

  const drawerContent = (
    <Box sx={{ width: 250, pt: 2 }} component="nav" role="navigation" aria-label="Mobile navigation">
      <List>
        {navigationItems.map((item) => (
          <ListItem key={item.path} disablePadding>
            <ListItemButton
              selected={isActivePath(item.path)}
              onClick={() => handleNavigation(item.path)}
              aria-current={isActivePath(item.path) ? 'page' : undefined}
            >
              <ListItemIcon aria-hidden="true">{item.icon}</ListItemIcon>
              <ListItemText primary={item.label} />
            </ListItemButton>
          </ListItem>
        ))}
      </List>
    </Box>
  );

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', minHeight: '100vh' }}>
      {/* Skip Link for Accessibility */}
      <SkipLink href="#main-content">Skip to main content</SkipLink>

      {/* AppBar - Using semantic header */}
      <AppBar position="sticky" component="header">
        <Toolbar>
          {isMobile && (
            <IconButton
              color="inherit"
              edge="start"
              onClick={toggleDrawer}
              sx={{ mr: 2 }}
              aria-label="Open navigation menu"
            >
              <MenuIcon />
            </IconButton>
          )}
          <Typography
            variant="h6"
            component="div"
            sx={{ flexGrow: 1, cursor: 'pointer' }}
            onClick={() => navigate('/')}
          >
            Kairo
          </Typography>

          {/* Desktop Navigation */}
          {!isMobile && (
            <Box component="nav" sx={{ display: 'flex', gap: 1, mr: 2 }} aria-label="Main navigation">
              {navigationItems.map((item) => (
                <Button
                  key={item.path}
                  color="inherit"
                  onClick={() => handleNavigation(item.path)}
                  sx={{
                    borderBottom: isActivePath(item.path) ? 2 : 0,
                    borderRadius: 0,
                  }}
                  aria-current={isActivePath(item.path) ? 'page' : undefined}
                >
                  {item.label}
                </Button>
              ))}
            </Box>
          )}

          {/* User Menu */}
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            {roles.length > 0 && (
              <Chip
                label={roles[0]}
                size="small"
                color="secondary"
                sx={{ display: { xs: 'none', sm: 'flex' } }}
                aria-label={`Current role: ${roles[0]}`}
              />
            )}
            <IconButton 
              color="inherit" 
              onClick={handleUserMenuOpen}
              aria-label="User menu"
              aria-controls="user-menu"
              aria-haspopup="true"
              aria-expanded={Boolean(anchorEl)}
            >
              <AccountCircleIcon />
            </IconButton>
          </Box>
          <Menu
            id="user-menu"
            anchorEl={anchorEl}
            open={Boolean(anchorEl)}
            onClose={handleUserMenuClose}
            aria-labelledby="user-menu-button"
          >
            <MenuItem disabled>
              <Typography variant="body2">
                {user?.profile?.preferred_username}
              </Typography>
            </MenuItem>
            <MenuItem disabled>
              <Typography variant="caption" color="text.secondary">
                {user?.profile?.email}
              </Typography>
            </MenuItem>
            <MenuItem onClick={handleLogout}>
              <LogoutIcon sx={{ mr: 1 }} fontSize="small" aria-hidden="true" />
              Logout
            </MenuItem>
          </Menu>
        </Toolbar>
      </AppBar>

      {/* Mobile Drawer */}
      <Drawer 
        anchor="left" 
        open={drawerOpen} 
        onClose={toggleDrawer}
        aria-label="Navigation drawer"
      >
        {drawerContent}
      </Drawer>

      {/* Main Content - Using semantic main element */}
      <Box 
        component="main" 
        id="main-content"
        sx={{ flexGrow: 1, bgcolor: 'background.default' }}
        role="main"
        aria-label="Main content"
      >
        <Outlet />
      </Box>

      {/* Footer - Using semantic footer */}
      <Box
        component="footer"
        sx={{
          py: 2,
          px: 2,
          mt: 'auto',
          bgcolor: 'background.paper',
          borderTop: 1,
          borderColor: 'divider',
        }}
        role="contentinfo"
      >
        <Container maxWidth="lg">
          <Typography variant="body2" color="text.secondary" align="center">
            © {new Date().getFullYear()} Kairo - Event Management System
          </Typography>
        </Container>
      </Box>
    </Box>
  );
}
