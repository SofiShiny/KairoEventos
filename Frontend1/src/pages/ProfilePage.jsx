import React, { useState, useEffect } from 'react';
import { useKeycloak } from '@react-keycloak/web';
import { userServices } from '../services/userServices';
import './ProfilePage.css';

const ProfilePage = () => {
  const { initialized, keycloak } = useKeycloak();
  const [profile, setProfile] = useState({
    nombre: '',
    correo: '',
    telefono: '',
    direccion: ''
  });
  const [isEditing, setIsEditing] = useState(false);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [success, setSuccess] = useState(false);

  useEffect(() => {
    if (initialized && keycloak.authenticated) {
      sendTokenToBackend();
      fetchProfile();
    }
  }, [initialized, keycloak]);

  const sendTokenToBackend = async () => {
    try {
      if (!keycloak.token) {
        throw new Error('No authentication token available');
      }
      
      const tokenData = {
        Token: keycloak.token
      };
      await userServices.agregarToken(tokenData);
      console.log('Token sent successfully');
    } catch (err) {
      console.error('Error sending token:', err);
    }
  };

  const fetchProfile = async () => {
    try {
      setLoading(true);
      setError(null);
      
      if (!keycloak.token) {
        throw new Error('No authentication token available');
      }
      
      const userId = keycloak.tokenParsed.sub;
      const response = await userServices.consultarPerfilId(userId);
      setProfile(response.data || {});
    } catch (err) {
      console.error('Error fetching profile:', err);
      setError('Error al cargar el perfil: ' + err.message);
    } finally {
      setLoading(false);
    }
  };

  if (!initialized) {
    return (
      <div className="profile-page">
        <div className="container">
          <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh' }}>
            <div>Cargando...</div>
          </div>
        </div>
      </div>
    );
  }

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setProfile({ ...profile, [name]: value });
  };

  const handleUpdateProfile = async () => {
    try {
      setLoading(true);
      setError(null);
      setSuccess(false);
      
      if (!keycloak.token) {
        throw new Error('No authentication token available');
      }
      
      const userId = keycloak.tokenParsed.sub;
      
      const userData = {
        Nombre: profile.nombre,
        Correo: profile.correo,
        Telefono: profile.telefono,
        Direccion: profile.direccion
      };
      
      await userServices.modificar(userId, userData);
      setSuccess(true);
      setIsEditing(false);
      
      setTimeout(() => {
        setSuccess(false);
      }, 3000);
    } catch (err) {
      console.error('Error updating profile:', err);
      setError('Error al actualizar el perfil: ' + err.message);
    } finally {
      setLoading(false);
    }
  };

  if (loading && !profile.nombre) {
    return (
      <div className="profile-page">
        <div className="container">
          <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh' }}>
            <div>Cargando perfil...</div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="profile-page">
      <div className="container">
        <div className="page-header">
          <h1 className="page-title">Mi Perfil</h1>
          <p className="page-subtitle">Administra tu información personal</p>
        </div>

        {error && (
          <div className="error-banner">
            {error}
            <button className="close-btn" onClick={() => setError(null)}>×</button>
          </div>
        )}

        {success && (
          <div className="success-banner">
            Perfil actualizado correctamente
            <button className="close-btn" onClick={() => setSuccess(false)}>×</button>
          </div>
        )}

        <div className="profile-card">
          <div className="profile-header">
            <h2 className="profile-title">Información Personal</h2>
            {!isEditing && (
              <button 
                className="btn btn-primary"
                onClick={() => setIsEditing(true)}
              >
                Editar Perfil
              </button>
            )}
          </div>

          <div className="profile-form">
            <div className="form-group">
              <label>Nombre</label>
              {isEditing ? (
                <input
                  type="text"
                  name="nombre"
                  value={profile.nombre}
                  onChange={handleInputChange}
                  className="form-input"
                  placeholder="Nombre completo"
                />
              ) : (
                <div className="profile-value">{profile.nombre}</div>
              )}
            </div>

            <div className="form-group">
              <label>Correo</label>
              {isEditing ? (
                <input
                  type="email"
                  name="correo"
                  value={profile.correo}
                  onChange={handleInputChange}
                  className="form-input"
                  placeholder="Email"
                />
              ) : (
                <div className="profile-value">{profile.correo}</div>
              )}
            </div>

            <div className="form-group">
              <label>Teléfono</label>
              {isEditing ? (
                <input
                  type="text"
                  name="telefono"
                  value={profile.telefono}
                  onChange={handleInputChange}
                  className="form-input"
                  placeholder="Teléfono"
                />
              ) : (
                <div className="profile-value">{profile.telefono}</div>
              )}
            </div>

            <div className="form-group">
              <label>Dirección</label>
              {isEditing ? (
                <input
                  type="text"
                  name="direccion"
                  value={profile.direccion}
                  onChange={handleInputChange}
                  className="form-input"
                  placeholder="Dirección"
                />
              ) : (
                <div className="profile-value">{profile.direccion}</div>
              )}
            </div>

            {isEditing && (
              <div className="form-actions">
                <button 
                  className="btn btn-primary"
                  onClick={handleUpdateProfile}
                  disabled={loading}
                >
                  {loading ? 'Guardando...' : 'Guardar Cambios'}
                </button>
                <button 
                  className="btn btn-secondary"
                  onClick={() => {
                    setIsEditing(false);
                    fetchProfile();
                  }}
                  disabled={loading}
                >
                  Cancelar
                </button>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default ProfilePage;