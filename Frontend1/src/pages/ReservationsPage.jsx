import React, { useState, useEffect } from 'react';
import { useKeycloak } from '@react-keycloak/web';
import { useCart } from '../context/CartContext';
import './ReservationsPage.css';

const ReservationsPage = () => {
  const { initialized, keycloak } = useKeycloak();
  const { getTicketsByUserId, cancelTicket, removeFromCart } = useCart();
  const [tickets, setTickets] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [filter, setFilter] = useState('PorPagar');

  useEffect(() => {
    if (initialized && keycloak.tokenParsed) {
      fetchUserTickets();
    }
  }, [initialized, keycloak, filter]);

  const fetchUserTickets = async () => {
    try {
      setLoading(true);
      setError('');
      
      const userId = keycloak.tokenParsed.sub;
      
      const userTickets = await getTicketsByUserId(userId, filter);
      setTickets(userTickets);
    } catch (err) {
      console.error('Error fetching tickets:', err);
      setError('Error al cargar las reservas. Por favor, inténtalo de nuevo.');
    } finally {
      setLoading(false);
    }
  };

  const handleDeleteTicket = async (ticketId) => {
    try {
      await removeFromCart(ticketId);
      fetchUserTickets();
    } catch (err) {
      console.error('Error deleting ticket:', err);
      setError('Error al borrar la entrada. Por favor, inténtalo de nuevo.');
    }
  };

  const handleCancelTicket = async (ticketId) => {
    try {
      await cancelTicket(ticketId);
      fetchUserTickets();
    } catch (err) {
      console.error('Error canceling ticket:', err);
      setError('Error al cancelar la entrada. Por favor, inténtalo de nuevo.');
    }
  };

  const formatDate = (dateString) => {
    const options = { year: 'numeric', month: 'short', day: 'numeric' };
    return new Date(dateString).toLocaleDateString('es-ES', options);
  };

  if (!initialized) {
    return (
      <div className="reservations-page">
        <div className="container">
          <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh' }}>
            <div>Cargando...</div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="reservations-page">
      <div className="container">
        <div className="page-header">
          <h1 className="page-title">Mis Reservas</h1>
          <p className="page-subtitle">Gestiona tus entradas y reservas</p>
        </div>

        <div className="reservations-filters">
          <div className="filter-group">
            <label className="filter-label">Estado</label>
            <select
              className="filter-select"
              value={filter}
              onChange={(e) => setFilter(e.target.value)}
            >
              <option value="PorPagar">Por Pagar</option>
              <option value="Pagado">Pagado</option>
              <option value="Cancelada">Cancelada</option>
            </select>
          </div>
        </div>

        {error && (
          <div className="error-message">
            {error}
          </div>
        )}

        {loading ? (
          <div className="loading-container">
            <div className="loading-spinner"></div>
            <p>Cargando reservas...</p>
          </div>
        ) : tickets.length === 0 ? (
          <div className="empty-state-card">
            <div className="empty-state-icon-container">
              <svg xmlns="http://www.w3.org/2000/svg" className="empty-state-icon" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 5v2m0 4v2m0 4v2M5 5a2 2 0 00-2 2v3a2 2 0 110 4v3a2 2 0 002 2h14a2 2 0 002-2v-3a2 2 0 110-4V7a2 2 0 00-2-2H5z" />
              </svg>
            </div>
            <h3 className="empty-state-title">No tienes reservas</h3>
            <p className="empty-state-description">No se encontraron entradas con el estado seleccionado</p>
            <a href="/events" className="empty-state-button">
              Explorar Eventos
            </a>
          </div>
        ) : (
          <div className="reservations-list">
            <div className="reservations-header">
              <h2 className="reservations-title">Tus Entradas ({tickets.length})</h2>
            </div>
            
            <div className="reservations-grid">
              {tickets.map(ticket => (
                <div key={ticket.idEntrada} className="reservation-card">
                  <div className="reservation-card-header">
                    <div className={`reservation-card-status reservation-card-status-${ticket.estado.toLowerCase()}`}>
                      {ticket.estado}
                    </div>
                  </div>
                  
                  <div className="reservation-card-content">
                    <div className="reservation-card-info">
                      <h3 className="reservation-card-title">Entrada General</h3>
                      <div className="reservation-card-details">
                        <div className="reservation-card-detail">
                          <span className="reservation-card-detail-label">ID:</span>
                          <span className="reservation-card-detail-value">{ticket.idEntrada}</span>
                        </div>
                        <div className="reservation-card-detail">
                          <span className="reservation-card-detail-label">Categoría:</span>
                          <span className="reservation-card-detail-value">{ticket.categoria}</span>
                        </div>
                        <div className="reservation-card-detail">
                          <span className="reservation-card-detail-label">Precio:</span>
                          <span className="reservation-card-detail-value">${ticket.precio.toFixed(2)} {ticket.moneda}</span>
                        </div>
                      </div>
                    </div>
                    
                    <div className="reservation-card-actions">
                      {ticket.estado === 'PorPagar' && (
                        <button 
                          className="reservation-card-cancel-btn"
                          onClick={() => handleDeleteTicket(ticket.idEntrada)}
                        >
                          Borrar
                        </button>
                      )}
                      {ticket.estado === 'Pagado' && (
                        <button 
                          className="reservation-card-cancel-btn"
                          onClick={() => handleCancelTicket(ticket.idEntrada)}
                        >
                          Cancelar
                        </button>
                      )}
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default ReservationsPage;