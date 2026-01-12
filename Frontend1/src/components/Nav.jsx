import React from "react";
import { Link } from "react-router-dom";
import './Nav.css';

const Nav = () => {
  return (
    <nav className="nav">
      <div className="container">
        <div className="nav-content">
          <div className="logo-container">
            <div className="logo">
              <span className="logo-text">E</span>
            </div>
            <span className="logo-title">EventManager</span>
          </div>

          <div className="nav-links">
            <Link to="/" className="nav-link">
              Inicio
            </Link>
            <Link to="/events" className="nav-link">
              Eventos
            </Link>
          </div>

          <button className="login-btn">
            Iniciar Sesión
          </button>
        </div>
      </div>
    </nav>
  );
};

export default Nav;