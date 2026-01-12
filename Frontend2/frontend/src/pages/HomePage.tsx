import React from 'react';
import { Typography, Grid, Card, CardContent, CardActions, Button, Box } from '@mui/material';
import { usePublicEvents } from '../api/events';
import { Link as RouterLink } from 'react-router-dom';
import LocationOnIcon from '@mui/icons-material/LocationOn';
import CalendarTodayIcon from '@mui/icons-material/CalendarToday';

const HomePage: React.FC = () => {
  const { data: events, isLoading, error } = usePublicEvents();

  if (isLoading) return <Typography>Cargando eventos...</Typography>;
  if (error) return <Typography color="error">Error al cargar eventos.</Typography>;

  return (
    <Box>
      <Box sx={{ mb: 4, textAlign: 'center' }}>
        <Typography variant="h3" component="h1" gutterBottom>
          Próximos Eventos
        </Typography>
        <Typography variant="h6" color="text.secondary">
          Descubre y reserva tu lugar en los mejores eventos.
        </Typography>
      </Box>

      {events && events.length === 0 ? (
        <Typography align="center">No hay eventos publicados por el momento.</Typography>
      ) : (
        <Grid container spacing={4}>
          {events?.map((event) => (
            <Grid item key={event.id} xs={12} sm={6} md={4}>
              <Card sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
                <CardContent sx={{ flexGrow: 1 }}>
                  <Typography gutterBottom variant="h5" component="h2">
                    {event.titulo}
                  </Typography>
                  <Box sx={{ display: 'flex', alignItems: 'center', mb: 1, color: 'text.secondary' }}>
                    <CalendarTodayIcon fontSize="small" sx={{ mr: 1 }} />
                    <Typography variant="body2">
                      {new Date(event.fechaInicio).toLocaleDateString()} {new Date(event.fechaInicio).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                    </Typography>
                  </Box>
                  <Box sx={{ display: 'flex', alignItems: 'center', mb: 2, color: 'text.secondary' }}>
                    <LocationOnIcon fontSize="small" sx={{ mr: 1 }} />
                    <Typography variant="body2">
                      {event.ubicacion.nombreLugar}, {event.ubicacion.ciudad}
                    </Typography>
                  </Box>
                  <Typography variant="body2" paragraph>
                    {event.descripcion.length > 100 ? `${event.descripcion.substring(0, 100)}...` : event.descripcion}
                  </Typography>
                </CardContent>
                <CardActions>
                  <Button size="small" variant="contained" component={RouterLink} to={`/usuario/evento/${event.id}`}>
                    Ver Detalles y Reservar
                  </Button>
                </CardActions>
              </Card>
            </Grid>
          ))}
        </Grid>
      )}
    </Box>
  );
};

export default HomePage;
