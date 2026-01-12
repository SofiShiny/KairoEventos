import React from 'react';
import { Link } from 'react-router-dom';
import { useKeycloak } from '@react-keycloak/web';
import { eventsData } from '../data/eventsData';
import EventCard from '../components/EventCard';
import './SecuredPage.css';

const SecuredPage = () => {
  const { keycloak, initialized } = useKeycloak();
  
  const featuredEvents = eventsData.slice(0, 3);

  if (!initialized) {
    return (
      <div className="secured-page">
        <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh' }}>
          <div>Cargando...</div>
        </div>
      </div>
    );
  }

  return (
    <div className="secured-page">
      <section className="hero-section">
        <div className="container hero-container">
          <div className="hero-content">
            <h1 className="hero-title">
              Descubre y Reserva <span className="hero-title-highlight">Eventos Increíbles</span>
            </h1>
            <p className="hero-subtitle">
              La plataforma integral para gestionar tus eventos, reservas y servicios complementarios
            </p>
            <div className="hero-buttons">
              {!keycloak.authenticated && (
                <button 
                  className="hero-button-primary"
                  onClick={() => keycloak.login()}
                >
                  Iniciar Sesión
                </button>
              )}
              {keycloak.authenticated && (
                <>
                  <Link 
                    to="/events" 
                    className="hero-button-primary"
                  >
                    Explorar Eventos
                  </Link>
                  <Link 
                    to="/services" 
                    className="hero-button-secondary"
                  >
                    Ver Servicios
                  </Link>
                  <Link 
                    to="/users" 
                    className="hero-button-secondary"
                  >
                    Gestión de Usuarios
                  </Link>
                </>
              )}
            </div>
          </div>
        </div>
      </section>

      <section className="featured-section">
        <div className="container">
          <div className="section-header">
            <div>
              <h2 className="section-title">Eventos Destacados</h2>
              <p className="section-subtitle">Descubre los eventos más populares de la semana</p>
            </div>
            <Link to="/events" className="section-link">
              Ver todos
              <svg xmlns="http://www.w3.org/2000/svg" className="section-link-icon" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
              </svg>
            </Link>
          </div>
          
          <div className="events-grid">
            {featuredEvents.map(event => (
              <EventCard key={event.id} event={event} />
            ))}
          </div>
        </div>
      </section>

      <section className="services-section">
        <div className="container">
          <div className="section-header-center">
            <h2 className="section-title">Servicios Complementarios</h2>
            <p className="section-subtitle">Mejora tu experiencia con nuestros servicios adicionales disponibles para cada evento</p>
          </div>
          
          <div className="services-grid">
            <div className="service-card-custom">
              <div className="service-icon-container">
                <svg xmlns="http://www.w3.org/2000/svg" className="service-icon" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6" />
                </svg>
              </div>
              <h3 className="service-title">Transporte</h3>
              <p className="service-description">Servicio de transporte desde tu ubicación hasta el evento</p>
              <Link to="/services" className="service-link">
                Ver opciones
              </Link>
            </div>
            
            <div className="service-card-custom">
              <div className="service-icon-container">
                <svg xmlns="http://www.w3.org/2000/svg" className="service-icon" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v13m0-13V6a2 2 0 112 2h-2zm0 0V5.5A2.5 2.5 0 109.5 8H12zm-7 4h14M5 12a2 2 0 110-4h14a2 2 0 110 4M5 12v7a2 2 0 002 2h10a2 2 0 002-2v-7" />
                </svg>
              </div>
              <h3 className="service-title">Catering</h3>
              <p className="service-description">Menús especiales durante el evento</p>
              <Link to="/services" className="service-link">
                Ver opciones
              </Link>
            </div>
            
            <div className="service-card-custom">
              <div className="service-icon-container">
                <svg xmlns="http://www.w3.org/2000/svg" className="service-icon" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 11V7a4 4 0 00-8 0v4M5 9h14l1 12H4L5 9z" />
                </svg>
              </div>
              <h3 className="service-title">Merchandising</h3>
              <p className="service-description">Productos oficiales del evento</p>
              <Link to="/services" className="service-link">
                Ver opciones
              </Link>
            </div>
          </div>
        </div>
      </section>

      <section className="how-it-works-section">
        <div className="container">
          <div className="section-header-center">
            <h2 className="section-title">¿Cómo Funciona?</h2>
            <p className="section-subtitle">Reservar tu próximo evento nunca ha sido tan fácil</p>
          </div>
          
          <div className="steps-grid">
            <div className="step-card">
              <div className="step-number-container">
                <span className="step-number">1</span>
              </div>
              <h3 className="step-title">Explora Eventos</h3>
              <p className="step-description">Busca entre cientos de eventos disponibles</p>
            </div>
            
            <div className="step-card">
              <div className="step-number-container">
                <span className="step-number">2</span>
              </div>
              <h3 className="step-title">Reserva Entradas</h3>
              <p className="step-description">Selecciona tus entradas y servicios adicionales</p>
            </div>
            
            <div className="step-card">
              <div className="step-number-container">
                <span className="step-number">3</span>
              </div>
              <h3 className="step-title">Realiza el Pago</h3>
              <p className="step-description">Paga de forma segura y confirma tu reserva</p>
            </div>
            
            <div className="step-card">
              <div className="step-number-container">
                <span className="step-number">4</span>
              </div>
              <h3 className="step-title">Disfruta</h3>
              <p className="step-description">Asiste al evento y disfruta de tu experiencia</p>
            </div>
          </div>
        </div>
      </section>

      <section className="cta-section">
        <div className="container cta-container">
          <h2 className="cta-title">¿Listo para tu próximo evento?</h2>
          <p className="cta-subtitle">
            Únete a miles de usuarios que ya disfrutan de la mejor experiencia en eventos
          </p>
          <Link 
            to="/events" 
            className="cta-button"
          >
            Comenzar Ahora
          </Link>
        </div>
      </section>
    </div>
  );
};

export default SecuredPage;