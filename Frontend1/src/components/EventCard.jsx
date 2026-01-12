import React from 'react';
import { Link } from 'react-router-dom';
import './EventCard.css';

const EventCard = ({ event }) => {
  
  const formatDate = (dateString) => {
    const options = { year: 'numeric', month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' };
    return new Date(dateString).toLocaleDateString('es-ES', options);
  };

  return (
    <div className="event-card">
      <div className="event-card-image-container">
        <div className="event-card-image">
          <span className="event-card-image-text">Imagen del Evento</span>
        </div>
        <div className="event-card-category">
          {event.category || event.tipo_evento}
        </div>
      </div>
      
      <div className="event-card-content">
        <div className="event-card-header">
          <h3 className="event-card-title">{event.title || event.nombre_evento}</h3>
        </div>
        
        <p className="event-card-description">{event.description || 'Este es un evento único en la plataforma. Disfruta de esta experiencia especial.'}</p>
        
        <div className="event-card-location">
          <svg xmlns="http://www.w3.org/2000/svg" className="event-card-icon" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
          </svg>
          <span>{event.location || event.venueDetails?.address || 'Ubicación por definir'}</span>
        </div>
        
        <div className="event-card-meta">
          <div className="event-card-date">
            <svg xmlns="http://www.w3.org/2000/svg" className="event-card-icon" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
            </svg>
            <span className="event-card-date-text">{formatDate(event.date || event.fecha_inicio_evento)}</span>
          </div>
          
          <div className="event-card-availability">
            {event.availableSeats || event.aforo_evento} disponibles
          </div>
        </div>
        
        <div className="event-card-footer">
          <div className="event-card-price-container">
            <span className="event-card-price">${(event.price || 0).toFixed(2)}</span>
            <span className="event-card-price-label">por entrada</span>
          </div>
          
          <div className="event-card-actions">
            <Link 
              to={`/event/${event.id || event.id_evento}`} 
              className="event-card-button"
            >
              Ver Detalles
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
};

export default EventCard;