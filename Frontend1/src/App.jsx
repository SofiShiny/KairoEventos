import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { ReactKeycloakProvider } from "@react-keycloak/web";
import keycloak from "./Keycloak";
import Header from './components/Header';
import HomePage from './pages/HomePage';
import EventsPage from './pages/EventsPage';
import EventDetailsPage from './pages/EventDetailsPage';
import ReservationsPage from './pages/ReservationsPage';
import ReservationDetailsPage from './pages/ReservationDetailsPage';
import UsersPage from './pages/UsersPage';
import ProfilePage from './pages/ProfilePage';
import CartPage from './pages/CartPage';
import SecuredPage from "./pages/SecuredPage";
import PrivateRoute from "./PrivateRoute";
import { CartProvider } from './context/CartContext';
import './App.css';

function App() {
  return (
    <ReactKeycloakProvider 
      authClient={keycloak}
      initOptions={{
        onLoad: 'login-required',
        checkLoginIframe: false
      }}
    >
      <Router>
        <CartProvider>
          <div className="app-container">
            <Header />
            <main className="main-content">
              <Routes>
                <Route path="/" element={<HomePage />} />
                <Route
                  path="/secured"
                  element={
                    <PrivateRoute>
                      <SecuredPage />
                    </PrivateRoute>
                  }
                />
                <Route path="/events" element={<EventsPage />} />
                <Route path="/event/:id" element={<EventDetailsPage />} />
                <Route path="/reservations" element={<ReservationsPage />} />
                <Route path="/reservation/:id" element={<ReservationDetailsPage />} />
                <Route path="/users" element={<UsersPage />} />
                <Route path="/profile" element={<ProfilePage />} />
                <Route path="/cart" element={<CartPage />} />
              </Routes>
            </main>
          </div>
        </CartProvider>
      </Router>
    </ReactKeycloakProvider>
  );
}

export default App;