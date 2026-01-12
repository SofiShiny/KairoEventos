import React, { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { useKeycloak } from '@react-keycloak/web';
import { useCart } from '../context/CartContext';
import { eventData } from '../data/eventsData';
import './EventDetailsPage.css';

const EventDetailsPage = () => {
  const { id } = useParams();
  const { initialized, keycloak } = useKeycloak();
  const { addToCart, getTicketsByEventId } = useCart();
  const [quantity, setQuantity] = useState(1);
  const [eventTickets, setEventTickets] = useState([]);

  useEffect(() => {
    const loadEventTickets = async () => {
      try {
        const eventId = 'dcce62bf-df2d-447b-9df0-8c0ce62534f1'; 
        const tickets = await getTicketsByEventId(eventId);
        const validTickets = tickets.filter(ticket => 
          ticket.estado === 'PorPagar' || ticket.estado === 'Pagado'
        );
        setEventTickets(validTickets);
      } catch (error) {
        console.error('Error loading event tickets:', error);
      }
    };

    if (initialized) {
      loadEventTickets();
    }
  }, [initialized, getTicketsByEventId]);

  if (!initialized) {
    return (
      <div className="event-details-page">
        <div className="container">
          <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh' }}>
            <div>Cargando...</div>
          </div>
        </div>
      </div>
    );
  }

  const event = eventData;

  const handleAddToCart = async () => {
    try {
      await addToCart(event, quantity);
      setQuantity(1);
    } catch (error) {
      console.error('Error adding to cart:', error);
      alert('Error al agregar al carrito. Por favor, inténtalo de nuevo.');
    }
  };

  const incrementQuantity = () => {
    setQuantity(prev => prev + 1);
  };

  const decrementQuantity = () => {
    if (quantity > 1) {
      setQuantity(prev => prev - 1);
    }
  };

  const formatDate = (dateString) => {
    const options = { year: 'numeric', month: 'long', day: 'numeric', hour: '2-digit', minute: '2-digit' };
    return new Date(dateString).toLocaleDateString('es-ES', options);
  };

  return (
    <div className="event-details-page">
      <div className="container">
        <div className="event-details-content">
          <div className="event-details-header">
            <div className="event-details-back">
              <Link to="/events" className="event-details-back-link">
                <svg xmlns="http://www.w3.org/2000/svg" className="event-details-back-icon" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 19l-7-7m0 0l7-7m-7 7h18" />
                </svg>
                Volver a eventos
              </Link>
            </div>
            
            <div className="event-details-main">
              <div className="event-details-image-section">
                <div className="event-details-image-container">
                  <div className="event-details-image-placeholder">
                    <span className="event-details-image-text">Imagen del Evento</span>
                  </div>
                  <div className="event-details-category-badge">
                    {event.category || event.tipo_evento}
                  </div>
                </div>
              </div>
              
              <div className="event-details-info-section">
                <div className="event-details-title-section">
                  <h1 className="event-details-title">{event.title || event.nombre_evento}</h1>
                  <div className="event-details-organizer">
                    <div className="event-details-organizer-avatar">
                      <span className="event-details-organizer-initials">O</span>
                    </div>
                    <div className="event-details-organizer-info">
                      <span className="event-details-organizer-label">Organizado por</span>
                      <span className="event-details-organizer-name">{event.organizer || 'Organizador por definir'}</span>
                    </div>
                  </div>
                </div>
                
                <div className="event-details-meta-section">
                  <div className="event-details-date">
                    <div className="event-details-meta-icon-container">
                      <svg xmlns="http://www.w3.org/2000/svg" className="event-details-meta-icon" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                      </svg>
                    </div>
                    <div className="event-details-date-info">
                      <span className="event-details-meta-label">Fecha y hora</span>
                      <span className="event-details-meta-value">
                        {formatDate(event.date || event.fecha_inicio_evento)}
                      </span>
                    </div>
                  </div>
                  
                  <div className="event-details-location">
                    <div className="event-details-meta-icon-container">
                      <svg xmlns="http://www.w3.org/2000/svg" className="event-details-meta-icon" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                      </svg>
                    </div>
                    <div className="event-details-location-info">
                      <span className="event-details-meta-label">Ubicación</span>
                      <span className="event-details-meta-value">{event.location || event.venueDetails?.address || 'Ubicación por definir'}</span>
                    </div>
                  </div>
                  
                  <div className="event-details-capacity">
                    <div className="event-details-meta-icon-container">
                      <svg xmlns="http://www.w3.org/2000/svg" className="event-details-meta-icon" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
                      </svg>
                    </div>
                    <div className="event-details-capacity-info">
                      <span className="event-details-meta-label">Aforo</span>
                      <span className="event-details-meta-value">{event.capacity || event.aforo_evento} personas</span>
                    </div>
                  </div>
                </div>
                
                <div className="event-details-description-section">
                  <h2 className="event-details-section-title">Descripción</h2>
                  <p className="event-details-description">
                    Este es un evento único en la plataforma. Disfruta de esta experiencia especial.
                  </p>
                </div>
                
                <div className="event-details-description-section">
                  <h2 className="event-details-section-title">Entradas Vendidas</h2>
                  <p className="event-details-description">
                    Total de entradas vendidas: {eventTickets.length}
                  </p>
                  {eventTickets.length > 0 && (
                    <div className="event-tickets-list">
                      <table className="event-tickets-table">
                        <thead>
                          <tr>
                            <th>ID Entrada</th>
                            <th>Categoría</th>
                            <th>Precio</th>
                            <th>Moneda</th>
                            <th>Estado</th>
                            <th>ID Asiento</th>
                          </tr>
                        </thead>
                        <tbody>
                          {eventTickets.map((ticket) => (
                            <tr key={ticket.idEntrada}>
                              <td>{ticket.idEntrada.substring(0, 8)}...</td>
                              <td>{ticket.categoria}</td>
                              <td>${ticket.precio.toFixed(2)}</td>
                              <td>{ticket.moneda}</td>
                              <td>
                                <span className={`ticket-status ticket-status-${ticket.estado.toLowerCase()}`}>
                                  {ticket.estado}
                                </span>
                              </td>
                              <td>{ticket.idasiento ? ticket.idasiento.substring(0, 8) + '...' : 'N/A'}</td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                    </div>
                  )}
                </div>
                
                <div className="event-details-actions">
                  <div className="event-details-price-section">
                    <div className="event-details-price-container">
                      <span className="event-details-price-label">Precio por entrada</span>
                      <span className="event-details-price">${(event.price || 0).toFixed(2)}</span>
                    </div>
                    
                    <div className="event-details-quantity-selector">
                      <span className="event-details-quantity-label">Cantidad</span>
                      <div className="event-details-quantity-controls">
                        <button 
                          className="event-details-quantity-btn"
                          onClick={decrementQuantity}
                          disabled={quantity <= 1}
                        >
                          -
                        </button>
                        <span className="event-details-quantity-value">{quantity}</span>
                        <button 
                          className="event-details-quantity-btn"
                          onClick={incrementQuantity}
                        >
                          +
                        </button>
                      </div>
                    </div>
                  </div>
                  
                  <button 
                    onClick={handleAddToCart}
                    className="event-details-add-to-cart-btn"
                  >
                    Agregar al Carrito
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default EventDetailsPage;