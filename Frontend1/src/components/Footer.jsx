import React from 'react';
import './Footer.css';

const Footer = () => {
  return (
    <footer className="footer">
      <div className="container">
        <div className="footer-grid">
          <div className="footer-col footer-col-wide">
            <div className="footer-logo-container">
              <div className="footer-logo">
                <span className="footer-logo-text">E</span>
              </div>
              <span className="footer-logo-title">EventManager</span>
            </div>
            <p className="footer-description">
              Plataforma integral para gestión de eventos, reservas y servicios con procesamiento asíncrono. 
              Descubre y reserva los mejores eventos de manera rápida y segura.
            </p>
            <div className="social-icons">
              <a href="#" className="social-icon">
                <svg className="social-icon-svg" fill="currentColor" viewBox="0 0 24 24">
                  <path d="M22 12c0-5.523-4.477-10-10-10S2 6.477 2 12c0 4.991 3.657 9.128 8.438 9.878v-6.987h-2.54V12h2.54V9.797c0-2.506 1.492-3.89 3.777-3.89 1.094 0 2.238.195 2.238.195v2.46h-1.26c-1.243 0-1.63.771-1.63 1.562V12h2.773l-.443 2.89h-2.33v6.988C18.343 21.128 22 16.991 22 12z"/>
                </svg>
              </a>
              <a href="#" className="social-icon">
                <svg className="social-icon-svg" fill="currentColor" viewBox="0 0 24 24">
                  <path d="M8.29 20.251c7.547 0 11.675-6.253 11.675-11.675 0-.178 0-.355-.012-.53A8.348 8.348 0 0022 5.92a8.19 8.19 0 01-2.357.646 4.118 4.118 0 001.804-2.27 8.224 8.224 0 01-2.605.996 4.107 4.107 0 00-6.993 3.743 11.65 11.65 0 01-8.457-4.287 4.106 4.106 0 001.27 5.477A4.072 4.072 0 012.8 9.713v.052a4.105 4.105 0 003.292 4.022 4.095 4.095 0 01-1.853.07 4.108 4.108 0 003.834 2.85A8.233 8.233 0 012 18.407a11.616 11.616 0 006.29 1.84"/>
                </svg>
              </a>
              <a href="#" className="social-icon">
                <svg className="social-icon-svg" fill="currentColor" viewBox="0 0 24 24">
                  <path d="M12 2C6.477 2 2 6.477 2 12c0 5.523 4.477 10 10 10 5.523 0 10-4.477 10-10 0-5.523-4.477-10-10-10zm0 18c-4.418 0-8-3.582-8-8s3.582-8 8-8 8 3.582 8 8-3.582 8-8 8zm-2-12h4v4h-4V8zm0 6h4v2h-4v-2z"/>
                </svg>
              </a>
            </div>
          </div>
          
          <div className="footer-col">
            <h4 className="footer-heading">Enlaces Rápidos</h4>
            <ul className="footer-links">
              <li><a href="#" className="footer-link">Eventos</a></li>
              <li><a href="#" className="footer-link">Mis Reservas</a></li>
              <li><a href="#" className="footer-link">Servicios</a></li>
              <li><a href="#" className="footer-link">Promociones</a></li>
              <li><a href="#" className="footer-link">Contacto</a></li>
            </ul>
          </div>
          
          <div className="footer-col">
            <h4 className="footer-heading">Contacto</h4>
            <address className="footer-contact">
              <p className="contact-item">Email: info@eventmanager.com</p>
              <p className="contact-item">Teléfono: +1 (555) 123-4567</p>
              <p className="contact-item">Av. Principal 123, Ciudad</p>
            </address>
          </div>
        </div>
        
        <div className="footer-bottom">
          <p className="footer-copyright">&copy; {new Date().getFullYear()} EventManager. Todos los derechos reservados.</p>
        </div>
      </div>
    </footer>
  );
};

export default Footer;