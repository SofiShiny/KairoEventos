import React, { useState } from 'react';
import { useKeycloak } from '@react-keycloak/web';
import { useCart } from '../context/CartContext';
import { Link } from 'react-router-dom';
import './CartPage.css';

const CartPage = () => {
  const { initialized, keycloak } = useKeycloak();
  const { items, removeFromCart, getTotalItems, getTotalPrice } = useCart();
  const [isCheckingOut, setIsCheckingOut] = useState(false);
  const [checkoutMessage, setCheckoutMessage] = useState('');

  if (!initialized) {
    return (
      <div className="cart-page">
        <div className="container">
          <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh' }}>
            <div>Cargando...</div>
          </div>
        </div>
      </div>
    );
  }

  const handleRemoveFromCart = async (ticketId) => {
    try {
      await removeFromCart(ticketId);
    } catch (error) {
      console.error('Error removing from cart:', error);
      setCheckoutMessage('Error al eliminar la entrada del carrito.');
    }
  };

  const formatDate = (dateString) => {
    const options = { year: 'numeric', month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' };
    return new Date(dateString).toLocaleDateString('es-ES', options);
  };

  const handleCheckout = async () => {
    if (!keycloak.tokenParsed) {
      setCheckoutMessage('Debes iniciar sesión para realizar la compra.');
      return;
    }
    
    setIsCheckingOut(true);
    setCheckoutMessage('');
    
    try {
      setCheckoutMessage('¡Compra realizada con éxito! Las entradas han sido procesadas.');
    } catch (error) {
      console.error('Error during checkout:', error);
      setCheckoutMessage('Hubo un error al procesar la compra. Por favor, inténtalo de nuevo.');
    } finally {
      setIsCheckingOut(false);
    }
  };

  return (
    <div className="cart-page">
      <div className="container">
        <div className="page-header">
          <h1 className="page-title">Carrito de Compras</h1>
          <p className="page-subtitle">Revisa y gestiona tus entradas seleccionadas</p>
        </div>

        {checkoutMessage && (
          <div className={`checkout-message ${checkoutMessage.includes('éxito') ? 'success' : 'error'}`}>
            {checkoutMessage}
          </div>
        )}

        {items.length === 0 ? (
          <div className="empty-cart">
            <div className="empty-cart-icon-container">
              <svg xmlns="http://www.w3.org/2000/svg" className="empty-cart-icon" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 3h2l.4 2M7 13h10l4-8H5.4M7 13L5.4 5M7 13l-2.293 2.293c-.63.63-.184 1.707.707 1.707H17m0 0a2 2 0 100 4 2 2 0 000-4zm-8 2a2 2 0 11-4 0 2 2 0 014 0z" />
              </svg>
            </div>
            <h3 className="empty-cart-title">Tu carrito está vacío</h3>
            <p className="empty-cart-description">Agrega entradas a tu carrito para continuar</p>
            <Link to="/events" className="empty-cart-button">
              Explorar Eventos
            </Link>
          </div>
        ) : (
          <div className="cart-content">
            <div className="cart-items">
              <div className="cart-items-header">
                <h2 className="cart-items-title">Entradas Seleccionadas ({getTotalItems()})</h2>
              </div>
              
              {items.map(item => (
                <div key={item.ticketId} className="cart-item">
                  <div className="cart-item-content">
                    <div className="cart-item-info">
                      <h3 className="cart-item-title">{item.eventTitle}</h3>
                      <div className="cart-item-details">
                        <div className="cart-item-date">
                          <svg xmlns="http://www.w3.org/2000/svg" className="cart-item-icon" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                          </svg>
                          <span>{formatDate(item.eventDate)}</span>
                        </div>
                        <div className="cart-item-location">
                          <svg xmlns="http://www.w3.org/2000/svg" className="cart-item-icon" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                          </svg>
                          <span>{item.eventLocation}</span>
                        </div>
                      </div>
                    </div>
                    
                    <div className="cart-item-actions">
                      <div className="cart-item-price">
                        <span className="cart-item-price-label">Precio unitario:</span>
                        <span className="cart-item-price-value">${item.price.toFixed(2)}</span>
                      </div>
                      
                      <div className="cart-item-quantity">
                        <label className="cart-item-quantity-label">Cantidad:</label>
                        <div className="quantity-controls">
                          <span className="quantity-value">1</span>
                        </div>
                      </div>
                      
                      <div className="cart-item-total">
                        <span className="cart-item-total-label">Total:</span>
                        <span className="cart-item-total-value">${item.price.toFixed(2)}</span>
                      </div>
                      
                      <button 
                        className="cart-item-remove"
                        onClick={() => handleRemoveFromCart(item.ticketId)}
                      >
                        <svg xmlns="http://www.w3.org/2000/svg" className="cart-item-remove-icon" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                        </svg>
                      </button>
                    </div>
                  </div>
                </div>
              ))}
            </div>
            
            <div className="cart-summary">
              <div className="cart-summary-card">
                <h2 className="cart-summary-title">Resumen del Pedido</h2>
                
                <div className="cart-summary-details">
                  <div className="cart-summary-row">
                    <span className="cart-summary-label">Subtotal ({getTotalItems()} entradas)</span>
                    <span className="cart-summary-value">${getTotalPrice().toFixed(2)}</span>
                  </div>
                  
                  <div className="cart-summary-row">
                    <span className="cart-summary-label">Impuestos</span>
                    <span className="cart-summary-value">$0.00</span>
                  </div>
                  
                  <div className="cart-summary-divider"></div>
                  
                  <div className="cart-summary-row cart-summary-total">
                    <span className="cart-summary-label">Total</span>
                    <span className="cart-summary-value">${getTotalPrice().toFixed(2)}</span>
                  </div>
                </div>
                
                <Link to="/events" className="cart-summary-continue">
                  Continuar Explorando
                </Link>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default CartPage;