import React, { useState, useRef } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { useKeycloak } from '@react-keycloak/web';
import CartIcon from './CartIcon.jsx';
import './Header.css';

const Header = () => {
  const [isMenuOpen, setIsMenuOpen] = useState(false);
  const [isUserDropdownOpen, setIsUserDropdownOpen] = useState(false);
  const dropdownTimeout = useRef(null);
  const location = useLocation();
  const { keycloak, initialized } = useKeycloak();

  const navLinks = [
    { name: 'Inicio', path: '/' },
    { name: 'Eventos', path: '/events' }
  ];

  const isActive = (path) => {
    return location.pathname === path;
  };

  if (!initialized) {
    return (
      <header className="header">
        <div className="container">
          <div className="header-content">
            <div className="logo-container">
              <div className="logo">
                <span className="logo-text">E</span>
              </div>
              <span className="logo-title">EventManager</span>
            </div>

            <nav className="nav-desktop">
              {navLinks.map((link) => (
                <Link
                  key={link.path}
                  to={link.path}
                  className={`nav-link ${isActive(link.path) ? 'nav-link-active' : ''}`}
                >
                  {link.name}
                </Link>
              ))}
            </nav>

            <div className="user-actions">
              <div className="loading-text">Cargando...</div>
            </div>

            <button 
              className="menu-btn"
              onClick={() => setIsMenuOpen(!isMenuOpen)}
            >
              <svg xmlns="http://www.w3.org/2000/svg" className="icon" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 12h16M4 18h16" />
              </svg>
            </button>
          </div>

          {isMenuOpen && (
            <div className="mobile-nav">
              <nav className="mobile-nav-content">
                {navLinks.map((link) => (
                  <Link
                    key={link.path}
                    to={link.path}
                    className={`mobile-nav-link ${isActive(link.path) ? 'mobile-nav-link-active' : ''}`}
                    onClick={() => setIsMenuOpen(false)}
                  >
                    {link.name}
                  </Link>
                ))}
                <div className="mobile-account-container">
                  <div className="loading-text">Cargando...</div>
                </div>
              </nav>
            </div>
          )}
        </div>
      </header>
    );
  }

  const handleMouseEnter = () => {
    if (dropdownTimeout.current) {
      clearTimeout(dropdownTimeout.current);
    }
    setIsUserDropdownOpen(true);
  };

  const handleMouseLeave = () => {
    dropdownTimeout.current = setTimeout(() => {
      setIsUserDropdownOpen(false);
    }, 300);
  };

  return (
    <header className="header">
      <div className="container">
        <div className="header-content">
          <div className="logo-container">
            <div className="logo">
              <span className="logo-text">E</span>
            </div>
            <span className="logo-title">EventManager</span>
          </div>

          <nav className="nav-desktop">
            {navLinks.map((link) => (
              <Link
                key={link.path}
                to={link.path}
                className={`nav-link ${isActive(link.path) ? 'nav-link-active' : ''}`}
              >
                {link.name}
              </Link>
            ))}
            {keycloak.authenticated && (
              <>
                <Link
                  to="/reservations"
                  className={`nav-link ${isActive('/reservations') ? 'nav-link-active' : ''}`}
                >
                  Mis Reservas
                </Link>
                <Link
                  to="/users"
                  className={`nav-link ${isActive('/users') ? 'nav-link-active' : ''}`}
                >
                  Usuarios
                </Link>
              </>
            )}
          </nav>

          <div className="user-actions">
            <CartIcon />
            <button className="notification-btn">
              <svg xmlns="http://www.w3.org/2000/svg" className="icon" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
              </svg>
            </button>
            {!keycloak.authenticated && (
              <button 
                className="account-btn"
                onClick={() => keycloak.login()}
              >
                Iniciar Sesión
              </button>
            )}
            {!!keycloak.authenticated && (
              <div 
                className="user-dropdown"
                onMouseEnter={handleMouseEnter}
                onMouseLeave={handleMouseLeave}
              >
                <button className="account-btn">
                  {keycloak.tokenParsed.preferred_username}
                </button>
                {isUserDropdownOpen && (
                  <div 
                    className="dropdown-menu"
                    onMouseEnter={handleMouseEnter}
                    onMouseLeave={handleMouseLeave}
                  >
                    <Link 
                      to="/profile" 
                      className="dropdown-item"
                      onClick={() => setIsUserDropdownOpen(false)}
                    >
                      Mi Perfil
                    </Link>
                    <button 
                      className="dropdown-item"
                      onClick={() => {
                        keycloak.logout();
                        setIsUserDropdownOpen(false);
                      }}
                    >
                      Cerrar Sesión
                    </button>
                  </div>
                )}
              </div>
            )}
          </div>

          <button 
            className="menu-btn"
            onClick={() => setIsMenuOpen(!isMenuOpen)}
          >
            <svg xmlns="http://www.w3.org/2000/svg" className="icon" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 12h16M4 18h16" />
            </svg>
          </button>
        </div>

        {isMenuOpen && (
          <div className="mobile-nav">
            <nav className="mobile-nav-content">
              {navLinks.map((link) => (
                <Link
                  key={link.path}
                  to={link.path}
                  className={`mobile-nav-link ${isActive(link.path) ? 'mobile-nav-link-active' : ''}`}
                  onClick={() => setIsMenuOpen(false)}
                >
                  {link.name}
                </Link>
              ))}
              {keycloak.authenticated && (
                <>
                  <Link
                    to="/reservations"
                    className={`mobile-nav-link ${isActive('/reservations') ? 'mobile-nav-link-active' : ''}`}
                    onClick={() => setIsMenuOpen(false)}
                  >
                    Mis Reservas
                  </Link>
                  <Link
                    to="/users"
                    className={`mobile-nav-link ${isActive('/users') ? 'mobile-nav-link-active' : ''}`}
                    onClick={() => setIsMenuOpen(false)}
                  >
                    Usuarios
                  </Link>
                </>
              )}
              <div className="mobile-account-container">
                {!keycloak.authenticated && (
                  <button 
                    className="account-btn mobile-account-btn"
                    onClick={() => {
                      keycloak.login();
                      setIsMenuOpen(false);
                    }}
                  >
                    Iniciar Sesión
                  </button>
                )}
                {!!keycloak.authenticated && (
                  <>
                    <Link 
                      to="/profile" 
                      className="account-btn mobile-account-btn"
                      onClick={() => setIsMenuOpen(false)}
                    >
                      Mi Perfil
                    </Link>
                    <button 
                      className="account-btn mobile-account-btn"
                      onClick={() => {
                        keycloak.logout();
                        setIsMenuOpen(false);
                      }}
                    >
                      Cerrar Sesión
                    </button>
                  </>
                )}
              </div>
            </nav>
          </div>
        )}
      </div>
    </header>
  );
};

export default Header;