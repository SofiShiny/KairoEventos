import React, { useEffect, useState } from 'react';
import { Typography, Grid, Card, CardContent, CardActions, Button, Box, Container } from '@mui/material';
import { useTranslation } from 'react-i18next';
import { usePublicEvents } from '../api/events';
import { Link as RouterLink } from 'react-router-dom';
import LocationOnIcon from '@mui/icons-material/LocationOn';
import CalendarTodayIcon from '@mui/icons-material/CalendarToday';
import { RecomendacionesCarousel } from '../components/RecomendacionesCarousel';
import { recomendacionesService, EventoRecomendado, RecomendacionesPersonalizadas } from '../services/recomendaciones.service';
import { useAuth } from '../context/AuthContext';

const HomePage: React.FC = () => {
  const { t } = useTranslation();
  const { data: events, isLoading, error } = usePublicEvents();
  const { user } = useAuth();

  const [tendencias, setTendencias] = useState<EventoRecomendado[]>([]);
  const [recomendacionesPersonalizadas, setRecomendacionesPersonalizadas] = useState<RecomendacionesPersonalizadas | null>(null);
  const [loadingRecomendaciones, setLoadingRecomendaciones] = useState(true);

  useEffect(() => {
    const cargarRecomendaciones = async () => {
      try {
        setLoadingRecomendaciones(true);

        // Cargar tendencias (siempre visible)
        const tendenciasData = await recomendacionesService.getTendencias(5);
        setTendencias(tendenciasData);

        // Cargar recomendaciones personalizadas si el usuario está logueado
        if (user?.id) {
          const personalizadas = await recomendacionesService.getRecomendacionesUsuario(user.id);
          setRecomendacionesPersonalizadas(personalizadas);
        }
      } catch (error) {
        console.error('Error al cargar recomendaciones:', error);
      } finally {
        setLoadingRecomendaciones(false);
      }
    };

    cargarRecomendaciones();
  }, [user]);

  if (isLoading) return <Typography>{t('common.loading')}</Typography>;
  if (error) return <Typography color="error">{t('common.error')}</Typography>;

  return (
    <Container maxWidth="xl">
      {/* Hero Section */}
      <Box sx={{ mb: 6, mt: 4, textAlign: 'center' }}>
        <Typography
          variant="h2"
          component="h1"
          gutterBottom
          sx={{
            fontWeight: 800,
            background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
            WebkitBackgroundClip: 'text',
            WebkitTextFillColor: 'transparent',
            backgroundClip: 'text',
            mb: 2
          }}
        >
          {t('home.hero_title')}
        </Typography>
        <Typography variant="h6" color="text.secondary" sx={{ maxWidth: '700px', mx: 'auto' }}>
          {t('home.hero_subtitle')}
        </Typography>
      </Box>

      {/* Recomendaciones Personalizadas (solo si está logueado) */}
      {!loadingRecomendaciones && user && recomendacionesPersonalizadas && recomendacionesPersonalizadas.eventos.length > 0 && (
        <RecomendacionesCarousel
          titulo={recomendacionesPersonalizadas.tituloSeccion}
          eventos={recomendacionesPersonalizadas.eventos}
          subtitulo={
            recomendacionesPersonalizadas.tipoRecomendacion === 'Personalizado'
              ? t('home.selection_for_you')
              : t('home.popular_subtitle')
          }
        />
      )}

      {/* Tendencias / Populares (siempre visible) */}
      {!loadingRecomendaciones && tendencias.length > 0 && (
        <RecomendacionesCarousel
          titulo={user ? t('navbar.events') : t('home.popular_today')}
          eventos={tendencias}
          subtitulo={t('home.popular_subtitle')}
        />
      )}

      {/* Todos los Eventos */}
      <Box sx={{ mt: 8, mb: 4 }}>
        <Typography
          variant="h4"
          component="h2"
          gutterBottom
          sx={{ fontWeight: 700, mb: 3 }}
        >
          {t('home.all_events')}
        </Typography>

        {events && events.length === 0 ? (
          <Typography align="center" color="text.secondary" sx={{ py: 8 }}>
            No hay eventos publicados por el momento.
          </Typography>
        ) : (
          <Grid container spacing={4}>
            {events?.map((event) => (
              <Grid item key={event.id} xs={12} sm={6} md={4}>
                <Card
                  sx={{
                    height: '100%',
                    display: 'flex',
                    flexDirection: 'column',
                    transition: 'transform 0.2s, box-shadow 0.2s',
                    '&:hover': {
                      transform: 'translateY(-4px)',
                      boxShadow: 6
                    }
                  }}
                >
                  <CardContent sx={{ flexGrow: 1 }}>
                    <Typography gutterBottom variant="h5" component="h2" sx={{ fontWeight: 600 }}>
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
                    <Typography variant="body2" paragraph color="text.secondary">
                      {event.descripcion.length > 100 ? `${event.descripcion.substring(0, 100)}...` : event.descripcion}
                    </Typography>
                  </CardContent>
                  <CardActions>
                    <Button
                      size="small"
                      variant="contained"
                      component={RouterLink}
                      to={`/usuario/evento/${event.id}`}
                      sx={{
                        background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                        '&:hover': {
                          background: 'linear-gradient(135deg, #5568d3 0%, #6a3f8f 100%)',
                        }
                      }}
                    >
                      {t('home.view_details')}
                    </Button>
                  </CardActions>
                </Card>
              </Grid>
            ))}
          </Grid>
        )}
      </Box>
    </Container>
  );
};

export default HomePage;
