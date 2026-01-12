import React, { createContext, useContext, useReducer, useEffect } from 'react';
import ticketService from '../services/ticketService.jsx';
import { useKeycloak } from '@react-keycloak/web';

const CartContext = createContext();

const SET_CART_ITEMS = 'SET_CART_ITEMS';
const CLEAR_CART = 'CLEAR_CART';

const cartReducer = (state, action) => {
  switch (action.type) {
    case SET_CART_ITEMS:
      return {
        ...state,
        items: action.payload
      };
      
    case CLEAR_CART:
      return {
        ...state,
        items: []
      };
      
    default:
      return state;
  }
};

const initialState = {
  items: []
};

export const CartProvider = ({ children }) => {
  const [state, dispatch] = useReducer(cartReducer, initialState);
  const { keycloak, initialized } = useKeycloak();
  
  useEffect(() => {
    if (initialized && keycloak.authenticated) {
      loadTickets();
    }
  }, [initialized, keycloak.authenticated]);
  
  const loadTickets = async () => {
    try {
      if (!keycloak.tokenParsed) return;
      
      const userId = keycloak.tokenParsed.sub;
      const tickets = await ticketService.consultarEntradasPorIdUsuario(userId, 'PorPagar');
      
      const cartItems = tickets.map(ticket => ({
        eventId: ticket.idEvento || 'dcce62bf-df2d-447b-9df0-8c0ce62534f1',
        eventTitle: 'Evento1',
        eventDate: '2029-12-31',
        eventLocation: 'Ubicación por definir',
        price: ticket.precio,
        quantity: 1,
        ticketId: ticket.idEntrada,
        ticketStatus: ticket.estado
      }));
      
      dispatch({
        type: SET_CART_ITEMS,
        payload: cartItems
      });
    } catch (error) {
      console.error('Error loading tickets:', error);
    }
  };
  
  const addToCart = async (event, quantity = 1) => {
    try {
      if (!keycloak.tokenParsed) {
        throw new Error('User not authenticated');
      }
      
      const tickets = [];
      for (let i = 0; i < quantity; i++) {
        const ticketData = {
          categoria: 'categoria',
          precio: event.price,
          moneda: 'USD',
          idEvento: event.id || event.id_evento,
          idasiento: '8ffcbeb7-40bf-484d-80a6-99f2a3a1d559'
        };
        
        const ticket = await ticketService.agregarEntrada(ticketData);
        tickets.push(ticket);
      }
      
      await loadTickets();
      
      return tickets;
    } catch (error) {
      console.error('Error adding to cart:', error);
      throw error;
    }
  };
  
  const removeFromCart = async (ticketId) => {
    try {
      await ticketService.eliminarEntrada(ticketId);
      await loadTickets();
    } catch (error) {
      console.error('Error removing from cart:', error);
      throw error;
    }
  };
  
  const clearCart = () => {
    dispatch({
      type: CLEAR_CART
    });
  };
  
  const checkoutCart = async () => {
    clearCart();
  };
  
  const cancelTicket = async (ticketId) => {
    try {
      const result = await ticketService.cancelarEntrada(ticketId);
      await loadTickets();
      return result;
    } catch (error) {
      console.error('Error canceling ticket:', error);
      throw error;
    }
  };
  
  const getTicketsByEventId = async (eventId) => {
    try {
      const tickets = await ticketService.consultarEntradasPorIdEvento(eventId);
      return tickets;
    } catch (error) {
      console.error('Error fetching tickets by event ID:', error);
      throw error;
    }
  };
  
  const getTicketsByUserId = async (userId, status) => {
    try {
      const tickets = await ticketService.consultarEntradasPorIdUsuario(userId, status);
      return tickets;
    } catch (error) {
      console.error('Error fetching tickets by user ID:', error);
      throw error;
    }
  };
  
  const getAllUserTickets = async (userId) => {
    try {
      const porPagar = await ticketService.consultarEntradasPorIdUsuario(userId, 'PorPagar');
      const cancelada = await ticketService.consultarEntradasPorIdUsuario(userId, 'Cancelada');
      const pagado = await ticketService.consultarEntradasPorIdUsuario(userId, 'Pagado');
      
      return {
        porPagar,
        cancelada,
        pagado
      };
    } catch (error) {
      console.error('Error fetching all user tickets:', error);
      throw error;
    }
  };
  
  const getTotalItems = () => {
    return state.items.length;
  };
  
  const getTotalPrice = () => {
    return state.items.reduce((total, item) => total + item.price, 0);
  };
  
  const isInCart = (ticketId) => {
    return state.items.some(item => item.ticketId === ticketId);
  };
  
  const getItemQuantity = (ticketId) => {
    const item = state.items.find(item => item.ticketId === ticketId);
    return item ? 1 : 0;
  };
  
  return (
    <CartContext.Provider
      value={{
        items: state.items,
        addToCart,
        removeFromCart,
        clearCart,
        checkoutCart,
        cancelTicket,
        getTicketsByEventId,
        getTicketsByUserId,
        getAllUserTickets,
        getTotalItems,
        getTotalPrice,
        isInCart,
        getItemQuantity,
        loadTickets
      }}
    >
      {children}
    </CartContext.Provider>
  );
};

export const useCart = () => {
  const context = useContext(CartContext);
  if (!context) {
    throw new Error('useCart must be used within a CartProvider');
  }
  return context;
};