import React, { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import { useKeycloak } from '@react-keycloak/web';
import ticketService from '../services/ticketService.jsx';
import './ReservationDetailsPage.css';

const ReservationDetailsPage = () => {
  const { id } = useParams();
  const { initialized, keycloak } = useKeycloak();
  const [reservation, setReservation] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchReservation = async () => {
      try {
        setLoading(true);
        setError(null);
        
        const reservationData = await ticketService.consultarEntradaPorId(id);
        
        const transformedData = {
          id: reservationData.idEntrada,
          eventId: reservationData.idEvento,
          userId: reservationData.idUsuario,
          eventName: reservationData.nombreEvento || 'Evento',
          reservationDate: reservationData.fechaReserva,
          status: reservationData.estado.toLowerCase(),
          numberOfTickets: 1, 
          totalPrice: reservationData.precio,
          paymentStatus: reservationData.estadoPago || 'pending',
          eventDetails: {
            title: reservationData.nombreEvento || 'Evento',
            date: reservationData.fechaEvento || reservationData.fechaReserva,
            location: reservationData.lugar || 'Ubicación por definir',
            venueDetails: {
              address: reservationData.direccion || 'Dirección por definir',
              city: reservationData.ciudad || 'Ciudad por definir',
              country: reservationData.pais || 'País por definir'
            }
          },
          attendeeNames: [reservationData.nombreUsuario || 'Usuario']
        };
        
        setReservation(transformedData);
      } catch (err) {
        console.error('Error fetching reservation:', err);
        setError('Error al cargar los detalles de la reserva. Por favor, inténtalo de nuevo.');
      } finally {
        setLoading(false);
      }
    };

    if (initialized && keycloak.authenticated && id) {
      fetchReservation();
    }
  }, [initialized, keycloak, id]);

  const formatDate = (dateString) => {
    const options = { year: 'numeric', month: 'long', day: 'numeric' };
    return new Date(dateString).toLocaleDateString('es-ES', options);
  };

  const formatDateTime = (dateString) => {
    const options = { year: 'numeric', month: 'long', day: 'numeric', hour: '2-digit', minute: '2-digit' };
    return new Date(dateString).toLocaleDateString('es-ES', options);
  };

  const getStatusColor = (status) => {
    switch (status) {
      case 'confirmed': return 'status-badge-confirmed';
      case 'pending': return 'status-badge-pending';
      case 'cancelled': return 'status-badge-cancelled';
      case 'pagado': return 'status-badge-confirmed';
      case 'porpagar': return 'status-badge-pending';
      case 'cancelada': return 'status-badge-cancelled';
      default: return 'status-badge-default';
    }
  };

  const getPaymentStatusColor = (status) => {
    switch (status) {
      case 'paid': return 'payment-badge-paid';
      case 'pending': return 'payment-badge-pending';
      case 'refunded': return 'payment-badge-refunded';
      case 'pagado': return 'payment-badge-paid';
      case 'porpagar': return 'payment-badge-pending';
      case 'reembolsado': return 'payment-badge-refunded';
      default: return 'payment-badge-default';
    }
  };

  if (!initialized) {
    return (
      <div className="reservation-details-page">
        <div className="container">
          <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh' }}>
            <div>Cargando...</div>
          </div>
        </div>
      </div>
    );
  }

  if (loading) {
    return (
      <div className="reservation-details-page">
        <div className="container">
          <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh' }}>
            <div>Cargando detalles de la reserva...</div>
          </div>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="reservation-details-page">
        <div className="container">
          <div className="error-message">
            <p>{error}</p>
            <button onClick={() => window.location.reload()}>Reintentar</button>
          </div>
        </div>
      </div>
    );
  }

  if (!reservation) {
    return (
      <div className="reservation-details-page">
        <div className="container">
          <div className="error-message">
            <p>No se encontraron detalles para esta reserva.</p>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="reservation-details-page">
      <div className="container">
        <div className="reservation-card">

          <div className="reservation-header">
            <div className="reservation-header-content">
              <div className="reservation-header-text">
                <h1 className="reservation-title">{reservation.eventName}</h1>
                <p className="reservation-id">Reserva #{reservation.id}</p>
              </div>
              <div className="reservation-status-container">
                <span className={`reservation-status-badge ${getStatusColor(reservation.status)}`}>
                  {reservation.status === 'confirmed' || reservation.status === 'pagado' ? 'Confirmada' : 
                   reservation.status === 'pending' || reservation.status === 'porpagar' ? 'Pendiente' : 'Cancelada'}
                </span>
                <span className={`reservation-payment-badge ${getPaymentStatusColor(reservation.paymentStatus)}`}>
                  {reservation.paymentStatus === 'paid' || reservation.paymentStatus === 'pagado' ? 'Pagado' : 
                   reservation.paymentStatus === 'pending' || reservation.paymentStatus === 'porpagar' ? 'Pendiente' : 
                   reservation.paymentStatus === 'refunded' || reservation.paymentStatus === 'reembolsado' ? 'Reembolsado' : 'Pendiente'}
                </span>
              </div>
            </div>
          </div>
          
          <div className="reservation-content">

            <div className="reservation-section">
              <h2 className="reservation-section-title">Detalles del Evento</h2>
              <div className="reservation-details-card">
                <div className="reservation-detail-item">
                  <div className="reservation-detail-icon-container">
                    <svg xmlns="http://www.w3.org/2000/svg" className="reservation-detail-icon" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                    </svg>
                  </div>
                  <div>
                    <h4 className="reservation-detail-label">Fecha y Hora</h4>
                    <p className="reservation-detail-value">{formatDateTime(reservation.eventDetails.date)}</p>
                  </div>
                </div>
                
                <div className="reservation-detail-item">
                  <div className="reservation-detail-icon-container">
                    <svg xmlns="http://www.w3.org/2000/svg" className="reservation-detail-icon" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                    </svg>
                  </div>
                  <div>
                    <h4 className="reservation-detail-label">Ubicación</h4>
                    <p className="reservation-detail-value">{reservation.eventDetails.location}</p>
                  </div>
                </div>
                
                <div className="reservation-detail-item">
                  <div className="reservation-detail-icon-container">
                    <svg xmlns="http://www.w3.org/2000/svg" className="reservation-detail-icon" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6" />
                    </svg>
                  </div>
                  <div>
                    <h4 className="reservation-detail-label">Dirección</h4>
                    <p className="reservation-detail-value">{reservation.eventDetails.venueDetails.address}</p>
                    <p className="reservation-detail-value">{reservation.eventDetails.venueDetails.city}, {reservation.eventDetails.venueDetails.country}</p>
                  </div>
                </div>
              </div>
            </div>
            

            <div className="reservation-section">
              <h2 className="reservation-section-title">Detalles de la Reserva</h2>
              <div className="reservation-details-grid">
                <div className="reservation-details-card">
                  <h3 className="reservation-card-title">Información de la Reserva</h3>
                  <div className="reservation-info-item">
                    <span className="reservation-info-label">Fecha de Reserva</span>
                    <span className="reservation-info-value">{formatDate(reservation.reservationDate)}</span>
                  </div>
                  <div className="reservation-info-item">
                    <span className="reservation-info-label">Número de Entradas</span>
                    <span className="reservation-info-value">{reservation.numberOfTickets}</span>
                  </div>
                  <div className="reservation-info-item">
                    <span className="reservation-info-label">Asistentes</span>
                    <div className="reservation-attendees">
                      {reservation.attendeeNames.map((name, index) => (
                        <p key={index} className="reservation-attendee">{name}</p>
                      ))}
                    </div>
                  </div>
                </div>
                
                <div className="reservation-details-card">
                  <h3 className="reservation-card-title">Estado y Pago</h3>
                  <div className="reservation-info-item">
                    <span className="reservation-info-label">Estado</span>
                    <span className={`reservation-status-display ${getStatusColor(reservation.status)}`}>
                      {reservation.status === 'confirmed' || reservation.status === 'pagado' ? 'Confirmada' : 
                       reservation.status === 'pending' || reservation.status === 'porpagar' ? 'Pendiente' : 'Cancelada'}
                    </span>
                  </div>
                  <div className="reservation-info-item">
                    <span className="reservation-info-label">Estado de Pago</span>
                    <span className={`reservation-payment-display ${getPaymentStatusColor(reservation.paymentStatus)}`}>
                      {reservation.paymentStatus === 'paid' || reservation.paymentStatus === 'pagado' ? 'Pagado' : 
                       reservation.paymentStatus === 'pending' || reservation.paymentStatus === 'porpagar' ? 'Pendiente' : 
                       reservation.paymentStatus === 'refunded' || reservation.paymentStatus === 'reembolsado' ? 'Reembolsado' : 'Pendiente'}
                    </span>
                  </div>
                  <div className="reservation-info-item">
                    <span className="reservation-info-label">Total Pagado</span>
                    <span className="reservation-total-value">${reservation.totalPrice.toFixed(2)}</span>
                  </div>
                </div>
              </div>
            </div>
            

            <div className="reservation-actions">
              {(reservation.status === 'pending' || reservation.status === 'porpagar') && (
                <>
                  <button className="reservation-action-btn action-btn-success">
                    <svg xmlns="http://www.w3.org/2000/svg" className="reservation-action-icon" viewBox="0 0 20 20" fill="currentColor">
                      <path fillRule="evenodd" d="M5 9V7a5 5 0 0110 0v2a2 2 0 012 2v5a2 2 0 01-2 2H5a2 2 0 01-2-2v-5a2 2 0 012-2zm8-2v2H7V7a3 3 0 016 0z" clipRule="evenodd" />
                    </svg>
                    Confirmar Pago
                  </button>
                  <button className="reservation-action-btn action-btn-danger">
                    <svg xmlns="http://www.w3.org/2000/svg" className="reservation-action-icon" viewBox="0 0 20 20" fill="currentColor">
                      <path fillRule="evenodd" d="M9 2a1 1 0 00-.894.553L7.382 4H4a1 1 0 000 2v10a2 2 0 002 2h8a2 2 0 002-2V6a1 1 0 100-2h-3.382l-.724-1.447A1 1 0 0011 2H9zM7 8a1 1 0 012 0v6a1 1 0 11-2 0V8zm5-1a1 1 0 00-1 1v6a1 1 0 102 0V8a1 1 0 00-1-1z" clipRule="evenodd" />
                    </svg>
                    Cancelar Reserva
                  </button>
                </>
              )}
              
              {(reservation.status === 'confirmed' || reservation.status === 'pagado') && (
                <>
                  <button className="reservation-action-btn action-btn-primary">
                    <svg xmlns="http://www.w3.org/2000/svg" className="reservation-action-icon" viewBox="0 0 20 20" fill="currentColor">
                      <path fillRule="evenodd" d="M3 17a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1zm3.293-7.707a1 1 0 011.414 0L9 10.586V3a1 1 0 112 0v7.586l1.293-1.293a1 1 0 111.414 1.414l-3 3a1 1 0 01-1.414 0l-3-3a1 1 0 010-1.414z" clipRule="evenodd" />
                    </svg>
                    Descargar Entradas
                  </button>
                  <button className="reservation-action-btn action-btn-secondary">
                    <svg xmlns="http://www.w3.org/2000/svg" className="reservation-action-icon" viewBox="0 0 20 20" fill="currentColor">
                      <path fillRule="evenodd" d="M9 2a1 1 0 00-.894.553L7.382 4H4a1 1 0 000 2v10a2 2 0 002 2h8a2 2 0 002-2V6a1 1 0 100-2h-3.382l-.724-1.447A1 1 0 0011 2H9zM7 8a1 1 0 012 0v6a1 1 0 11-2 0V8zm5-1a1 1 0 00-1 1v6a1 1 0 102 0V8a1 1 0 00-1-1z" clipRule="evenodd" />
                    </svg>
                    Cancelar Reserva
                  </button>
                </>
              )}
              
              <button className="reservation-action-btn action-btn-outline">
                <svg xmlns="http://www.w3.org/2000/svg" className="reservation-action-icon" viewBox="0 0 20 20" fill="currentColor">
                  <path fillRule="evenodd" d="M3 17a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1zm3.293-7.707a1 1 0 011.414 0L9 10.586V3a1 1 0 112 0v7.586l1.293-1.293a1 1 0 111.414 1.414l-3 3a1 1 0 01-1.414 0l-3-3a1 1 0 010-1.414z" clipRule="evenodd" />
                </svg>
                Imprimir Detalles
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ReservationDetailsPage;