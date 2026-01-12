import React, { useState } from 'react';
import { useKeycloak } from '@react-keycloak/web';
import { eventsData } from '../data/eventsData';
import EventCard from '../components/EventCard';
import './EventsPage.css';

const EventsPage = () => {
  const { initialized } = useKeycloak();

  if (!initialized) {
    return (
      <div className="events-page">
        <div className="container">
          <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh' }}>
            <div>Cargando...</div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="events-page">
      <div className="container">
        <div className="page-header">
          <h1 className="page-title">Explorar Eventos</h1>
          <p className="page-subtitle">Descubre una amplia variedad de eventos disponibles</p>
        </div>
        
        <div className="events-grid">
          {eventsData.map(event => (
            <EventCard key={event.id || event.id_evento} event={event} />
          ))}
        </div>
      </div>
    </div>
  );
};

export default EventsPage;