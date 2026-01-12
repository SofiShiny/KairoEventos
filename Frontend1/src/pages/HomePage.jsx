import React from 'react';
import { Link } from 'react-router-dom';
import { useKeycloak } from '@react-keycloak/web';
import './HomePage.css';

const HomePage = () => {
  const { keycloak, initialized } = useKeycloak();

  return (
    <div className="home-page">
      <section className="hero-section">
        <div className="container hero-container">
          <div className="hero-content">
            <h1 className="hero-title">
              Bienvenido a <span className="hero-title-highlight">EventManager</span>
            </h1>
            <p className="hero-subtitle">
              La plataforma integral para descubrir, reservar y gestionar eventos excepcionales
            </p>
            <div className="hero-buttons">
              <Link to="/events" className="hero-button-primary">
                Explorar Eventos
              </Link>
              <Link to="/services" className="hero-button-secondary">
                Ver Servicios
              </Link>
            </div>
          </div>
        </div>
      </section>
    </div>
  );
};

export default HomePage;