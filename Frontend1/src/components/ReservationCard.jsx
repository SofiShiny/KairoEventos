import React from 'react';
import { Link } from 'react-router-dom';
import './ReservationCard.css';

const ReservationCard = ({ reservation }) => {
  const formatDate = (dateString) => {
    const options = { year: 'numeric', month: 'short', day: 'numeric' };
    return new Date(dateString).toLocaleDateString('es-ES', options);
  };

  const getStatusColor = (status) => {
    switch (status) {
      case 'confirmed': return 'reservation-status-confirmed';
      case 'pending': return 'reservation-status-pending';
      case 'cancelled': return 'reservation-status-cancelled';
      default: return 'reservation-status-default';
    }
  };

  const getPaymentStatusColor = (status) => {
    switch (status) {
      case 'paid': return 'payment-status-paid';
      case 'pending': return 'payment-status-pending';
      case 'refunded': return 'payment-status-refunded';
      default: return 'payment-status-default';
    }
  };

  return (
    <div className="reservation-card">
      <div className="reservation-card-header">
        <h3 className="reservation-card-title">{reservation.eventName}</h3>
        <span className={`reservation-status-badge ${getStatusColor(reservation.status)}`}>
          {reservation.status === 'confirmed' ? 'Confirmada' : 
           reservation.status === 'pending' ? 'Pendiente' : 'Cancelada'}
        </span>
      </div>
      
      <div className="reservation-card-grid">
        <div className="reservation-card-info">
          <p className="reservation-card-label">Fecha de reserva</p>
          <p className="reservation-card-value">{formatDate(reservation.reservationDate)}</p>
        </div>
        <div className="reservation-card-info">
          <p className="reservation-card-label">Entradas</p>
          <p className="reservation-card-value">{reservation.numberOfTickets}</p>
        </div>
        <div className="reservation-card-info">
          <p className="reservation-card-label">Total</p>
          <p className="reservation-card-value">${reservation.totalPrice.toFixed(2)}</p>
        </div>
        <div className="reservation-card-info">
          <p className="reservation-card-label">Pago</p>
          <span className={`reservation-payment-badge ${getPaymentStatusColor(reservation.paymentStatus)}`}>
            {reservation.paymentStatus === 'paid' ? 'Pagado' : 
             reservation.paymentStatus === 'pending' ? 'Pendiente' : 'Reembolsado'}
          </span>
        </div>
      </div>
      
      <div className="reservation-card-footer">
        <Link 
          to={`/reservation/${reservation.id}`} 
          className="reservation-card-link"
        >
          Ver detalles
          <svg xmlns="http://www.w3.org/2000/svg" className="reservation-card-link-icon" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
          </svg>
        </Link>
      </div>
    </div>
  );
};

export default ReservationCard;