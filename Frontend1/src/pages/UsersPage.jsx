import React, { useState, useEffect } from 'react';
import { useKeycloak } from '@react-keycloak/web';
import { userServices } from '../services/userServices';
import { useCart } from '../context/CartContext';
import './UsersPage.css';

const UsersPage = () => {
  const { initialized, keycloak } = useKeycloak();
  const { getTicketsByUserId } = useCart();
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [fieldErrors, setFieldErrors] = useState({});
  const [showAddUserForm, setShowAddUserForm] = useState(false);
  const [editingUser, setEditingUser] = useState(null);
  const [newUser, setNewUser] = useState({
    username: '',
    name: '',
    email: '',
    password: '',
    confirmPassword: '',
    phone: '',
    address: '',
    role: 'Organizador'
  });
  const [selectedUserTickets, setSelectedUserTickets] = useState(null);
  const [userTickets, setUserTickets] = useState([]);
  const [ticketsLoading, setTicketsLoading] = useState(false);
  const [ticketsFilter, setTicketsFilter] = useState('PorPagar');


  useEffect(() => {
    console.log('UsersPage mounted or keycloak/initialized changed');
    if (initialized && keycloak.authenticated) {
      fetchUsers();
    }
  }, [initialized, keycloak]);

  const fetchUsers = async () => {
    try {
      setLoading(true);
      setError(null);
      if (!keycloak.token) {
        throw new Error('No authentication token available');
      }
      
      const response = await userServices.consultar();
      setUsers(response.data || []);
    } catch (err) {
      console.error('Error fetching users:', err);
      setError('Error al cargar los usuarios: ' + err.message);
    } finally {
      setLoading(false);
    }
  };

  const fetchUserTickets = async (userId, filter = ticketsFilter) => {
    try {
      setTicketsLoading(true);
      setError(null);
      
      // Fetch tickets for the user with the selected status
      const tickets = await getTicketsByUserId(userId, filter);
      setUserTickets(tickets);
      setSelectedUserTickets(userId);
    } catch (err) {
      console.error('Error fetching user tickets:', err);
      setError('Error al cargar las entradas del usuario: ' + err.message);
    } finally {
      setTicketsLoading(false);
    }
  };

  // If Keycloak is not initialized, show a loading state
  if (!initialized) {
    return (
      <div className="users-page">
        <div className="container">
          <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh' }}>
            <div>Cargando...</div>
          </div>
        </div>
      </div>
    );
  }

  // Validation functions for each field
  const validateUsername = (username) => {
    const errors = [];
    if (!username) {
      errors.push('El campo es obligatorio.');
    } else {
      if (username.length < 3 || username.length > 20) {
        errors.push('Debe tener entre 3 y 20 caracteres.');
      }
    }
    return errors;
  };

  const validateName = (name) => {
    const errors = [];
    if (!name) {
      errors.push('Obligatorio.');
    } else {
      if (name.length < 2 || name.length > 30) {
        errors.push('Entre 2 y 30 caracteres.');
      }
      const nameRegex = /^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$/;
      if (!nameRegex.test(name)) {
        errors.push('Solo se permiten letras (incluyendo acentos y ñ) y espacios. Ejemplo válido: "José Pérez"');
      }
    }
    return errors;
  };

  const validateEmail = (email) => {
    const errors = [];
    if (!email) {
      errors.push('Obligatorio.');
    } else {
      const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
      if (!emailRegex.test(email)) {
        errors.push('Debe cumplir con la estructura de un email válido (usuario@dominio.com).');
      }
    }
    return errors;
  };

  const validatePassword = (password) => {
    const errors = [];
    if (!password) {
      errors.push('Obligatoria.');
    } else {
      if (password.length < 8) {
        errors.push('Al menos 8 caracteres.');
      }
      if (!/[A-Z]/.test(password)) {
        errors.push('Una letra mayúscula ([A-Z]).');
      }
      if (!/[a-z]/.test(password)) {
        errors.push('Una letra minúscula ([a-z]).');
      }
      if (!/[0-9]/.test(password)) {
        errors.push('Un número ([0-9]).');
      }
      if (!/[^a-zA-Z0-9]/.test(password)) {
        errors.push('Un carácter especial (cualquier símbolo que no sea letra ni número).');
      }
    }
    return errors;
  };

  const validateConfirmPassword = (confirmPassword, password) => {
    const errors = [];
    if (!confirmPassword) {
      errors.push('Obligatoria.');
    } else {
      if (confirmPassword !== password) {
        errors.push('Debe ser exactamente igual al campo Contraseña.');
      }
    }
    return errors;
  };

  const validatePhone = (phone) => {
    const errors = [];
    if (!phone) {
      errors.push('Obligatorio.');
    } else {
      const phoneRegex = /^\d{11}$/;
      if (!phoneRegex.test(phone)) {
        errors.push('Debe contener exactamente 11 dígitos numéricos. Ejemplo válido: 04121234567');
      }
    }
    return errors;
  };

  const validateAddress = (address) => {
    const errors = [];
    if (!address) {
      errors.push('Obligatoria.');
    }
    return errors;
  };

  const validateForm = () => {
    const errors = {};
    
    const usernameErrors = validateUsername(newUser.username);
    if (usernameErrors.length > 0) {
      errors['Username'] = usernameErrors;
    }
    
    const nameErrors = validateName(newUser.name);
    if (nameErrors.length > 0) {
      errors['Nombre'] = nameErrors;
    }
    
    const emailErrors = validateEmail(newUser.email);
    if (emailErrors.length > 0) {
      errors['Correo'] = emailErrors;
    }
    
    const passwordErrors = validatePassword(newUser.password);
    if (passwordErrors.length > 0) {
      errors['Contrasena'] = passwordErrors;
    }
    
    const confirmPasswordErrors = validateConfirmPassword(newUser.confirmPassword, newUser.password);
    if (confirmPasswordErrors.length > 0) {
      errors['ConfirmarContrasena'] = confirmPasswordErrors;
    }
    
    const phoneErrors = validatePhone(newUser.phone);
    if (phoneErrors.length > 0) {
      errors['Telefono'] = phoneErrors;
    }
    
    const addressErrors = validateAddress(newUser.address);
    if (addressErrors.length > 0) {
      errors['Direccion'] = addressErrors;
    }
    
    return errors;
  };

  const handleAddUser = async () => {
    // Validate form before submitting
    const errors = validateForm();
    if (Object.keys(errors).length > 0) {
      setFieldErrors(errors);
      return;
    }
    
    try {
      const userData = {
        Username: newUser.username,
        Nombre: newUser.name,
        Correo: newUser.email,
        Contrasena: newUser.password,
        ConfirmarContrasena: newUser.confirmPassword,
        Telefono: newUser.phone,
        Direccion: newUser.address,
        Rol: newUser.role
      };
      
      await userServices.agregar(userData);
      await fetchUsers(); // Refresh the user list
      
      setNewUser({ 
        username: '', 
        name: '', 
        email: '', 
        password: '', 
        confirmPassword: '', 
        phone: '', 
        address: '',
        role: 'Organizador'
      });
      setShowAddUserForm(false);
      setFieldErrors({}); // Clear errors on success
    } catch (err) {
      console.error('Error adding user:', err);
      setError('Error al agregar el usuario: ' + (err.response?.data?.message || err.message));
    }
  };

  const handleEditUser = async (user) => {
    try {
      if (!keycloak.token) {
        throw new Error('No authentication token available');
      }
      
      // Fetch detailed user data
      const response = await userServices.consultarId(user.idUsuario);
      const detailedUser = response.data;
      
      setEditingUser(detailedUser);
      setNewUser({ 
        username: detailedUser.username,
        name: detailedUser.nombre,
        email: detailedUser.correo,
        phone: detailedUser.telefono,
        address: detailedUser.direccion,
        role: detailedUser.rol,
        password: '', 
        confirmPassword: '' 
      });
      setFieldErrors({});
    } catch (err) {
      console.error('Error fetching user details:', err);
      setError('Error al cargar los detalles del usuario: ' + err.message);
    }
  };

  const handleUpdateUser = async () => {
    const errors = {};
    
    const nameErrors = validateName(newUser.name);
    if (nameErrors.length > 0) {
      errors['Nombre'] = nameErrors;
    }
    
    const emailErrors = validateEmail(newUser.email);
    if (emailErrors.length > 0) {
      errors['Correo'] = emailErrors;
    }
    
    const phoneErrors = validatePhone(newUser.phone);
    if (phoneErrors.length > 0) {
      errors['Telefono'] = phoneErrors;
    }
    
    const addressErrors = validateAddress(newUser.address);
    if (addressErrors.length > 0) {
      errors['Direccion'] = addressErrors;
    }
    
    if (Object.keys(errors).length > 0) {
      setFieldErrors(errors);
      return;
    }
    
    try {
      const userData = {
        Nombre: newUser.name,
        Correo: newUser.email,
        Telefono: newUser.phone,
        Direccion: newUser.address
      };
      
      await userServices.modificar(editingUser.idUsuario, userData);
      await fetchUsers(); // Refresh the user list
      
      setEditingUser(null);
      setNewUser({ 
        username: '', 
        name: '', 
        email: '', 
        password: '', 
        confirmPassword: '', 
        phone: '', 
        address: '',
        role: 'Organizador'
      });
      setFieldErrors({});
    } catch (err) {
      console.error('Error updating user:', err);
      setError('Error al actualizar el usuario: ' + (err.response?.data?.message || err.message));
    }
  };

  const handleDeleteUser = async (id) => {
    try {
      
      if (!keycloak.token) {
        throw new Error('No authentication token available');
      }
      
      await userServices.eliminar(id);
      await fetchUsers(); 
    } catch (err) {
      console.error('Error deleting user:', err);
      setError('Error al eliminar el usuario: ' + err.message);
    }
  };

  const handleViewUserTickets = async (userId) => {
    if (selectedUserTickets === userId) {
      setSelectedUserTickets(null);
      setUserTickets([]);
    } else {
      await fetchUserTickets(userId, ticketsFilter);
    }
  };

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setNewUser({ ...newUser, [name]: value });
    
    const fieldMapping = {
      'username': 'Username',
      'name': 'Nombre',
      'email': 'Correo',
      'password': 'Contrasena',
      'confirmPassword': 'ConfirmarContrasena',
      'phone': 'Telefono',
      'address': 'Direccion'
    };
    
    const apiFieldName = fieldMapping[name];
    if (apiFieldName && fieldErrors[apiFieldName]) {
      console.log('Clearing error for field:', apiFieldName);
      setFieldErrors(prev => {
        const newErrors = { ...prev };
        delete newErrors[apiFieldName];
        return newErrors;
      });
    }
  };

  const handleTicketsFilterChange = async (e) => {
    const newFilter = e.target.value;
    setTicketsFilter(newFilter);
    if (selectedUserTickets) {
      await fetchUserTickets(selectedUserTickets, newFilter);
    }
  };

  const getRoleDisplayName = (role) => {
    switch (role) {
      case 'Organizador': return 'Organizador';
      case 'Soporte': return 'Soporte';
      default: return role;
    }
  };

  if (loading) {
    return (
      <div className="users-page">
        <div className="container">
          <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh' }}>
            <div>Cargando usuarios...</div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="users-page">
      <div className="container">
        <div className="page-header">
          <h1 className="page-title">Gestión de Usuarios</h1>
          <p className="page-subtitle">Administra los usuarios del sistema</p>
        </div>

        {error && (
          <div className="error-banner">
            {error}
            <button className="close-btn" onClick={() => setError(null)}>×</button>
          </div>
        )}

        <div className="users-actions">
          <button 
            className="btn btn-primary"
            onClick={() => {
              setEditingUser(null);
              setNewUser({ 
                username: '', 
                name: '', 
                email: '', 
                password: '', 
                confirmPassword: '', 
                phone: '', 
                address: '',
                role: 'Organizador'
              });
              setFieldErrors({});
              setShowAddUserForm(true);
            }}
          >
            Agregar Usuario
          </button>
          <button 
            className="btn btn-secondary"
            onClick={fetchUsers}
          >
            Refrescar
          </button>
        </div>

        {showAddUserForm || editingUser ? (
          <div className="user-form-card">
            <h2 className="form-title">{editingUser ? 'Editar Usuario' : 'Agregar Nuevo Usuario'}</h2>
            {!editingUser && (
              <>
                <div className="form-group">
                  <label>Nombre de Usuario *</label>
                  <input
                    type="text"
                    name="username"
                    value={newUser.username}
                    onChange={handleInputChange}
                    className={`form-input ${fieldErrors['Username'] ? 'form-input-error' : ''}`}
                    placeholder="Nombre de usuario"
                  />
                  {fieldErrors['Username'] && fieldErrors['Username'].map((e, i) => (
                    <div key={i} className="field-error-message">{e}</div>
                  ))}
                </div>
                <div className="form-group">
                  <label>Contraseña *</label>
                  <input
                    type="password"
                    name="password"
                    value={newUser.password}
                    onChange={handleInputChange}
                    className={`form-input ${fieldErrors['Contrasena'] ? 'form-input-error' : ''}`}
                    placeholder="Contraseña"
                  />
                  {fieldErrors['Contrasena'] && fieldErrors['Contrasena'].map((e, i) => (
                    <div key={i} className="field-error-message">{e}</div>
                  ))}
                </div>
                <div className="form-group">
                  <label>Confirmar Contraseña *</label>
                  <input
                    type="password"
                    name="confirmPassword"
                    value={newUser.confirmPassword}
                    onChange={handleInputChange}
                    className={`form-input ${fieldErrors['ConfirmarContrasena'] ? 'form-input-error' : ''}`}
                    placeholder="Confirmar contraseña"
                  />
                  {fieldErrors['ConfirmarContrasena'] && fieldErrors['ConfirmarContrasena'].map((e, i) => (
                    <div key={i} className="field-error-message">{e}</div>
                  ))}
                </div>
              </>
            )}
            <div className="form-group">
              <label>Nombre *</label>
              <input
                type="text"
                name="name"
                value={newUser.name}
                onChange={handleInputChange}
                className={`form-input ${fieldErrors['Nombre'] ? 'form-input-error' : ''}`}
                placeholder="Nombre completo"
              />
              {fieldErrors['Nombre'] && fieldErrors['Nombre'].map((e, i) => (
                <div key={i} className="field-error-message">{e}</div>
              ))}
            </div>
            <div className="form-group">
              <label>Correo *</label>
              <input
                type="email"
                name="email"
                value={newUser.email}
                onChange={handleInputChange}
                className={`form-input ${fieldErrors['Correo'] ? 'form-input-error' : ''}`}
                placeholder="Email"
              />
              {fieldErrors['Correo'] && fieldErrors['Correo'].map((e, i) => (
                <div key={i} className="field-error-message">{e}</div>
              ))}
            </div>
            <div className="form-group">
              <label>Teléfono</label>
              <input
                type="text"
                name="phone"
                value={newUser.phone}
                onChange={handleInputChange}
                className={`form-input ${fieldErrors['Telefono'] ? 'form-input-error' : ''}`}
                placeholder="Teléfono"
              />
              {fieldErrors['Telefono'] && fieldErrors['Telefono'].map((e, i) => (
                <div key={i} className="field-error-message">{e}</div>
              ))}
            </div>
            <div className="form-group">
              <label>Dirección</label>
              <input
                type="text"
                name="address"
                value={newUser.address}
                onChange={handleInputChange}
                className={`form-input ${fieldErrors['Direccion'] ? 'form-input-error' : ''}`}
                placeholder="Dirección"
              />
              {fieldErrors['Direccion'] && fieldErrors['Direccion'].map((e, i) => (
                <div key={i} className="field-error-message">{e}</div>
              ))}
            </div>
            {!editingUser && (
              <div className="form-group">
                <label>Rol</label>
                <select
                  name="role"
                  value={newUser.role}
                  onChange={handleInputChange}
                  className="form-select"
                >
                  <option value="Organizador">Organizador</option>
                  <option value="Soporte">Soporte</option>
                </select>
              </div>
            )}
            <div className="form-actions">
              <button 
                className="btn btn-primary"
                onClick={editingUser ? handleUpdateUser : handleAddUser}
              >
                {editingUser ? 'Actualizar' : 'Agregar'}
              </button>
              <button 
                className="btn btn-secondary"
                onClick={() => {
                  setShowAddUserForm(false);
                  setEditingUser(null);
                  setFieldErrors({});
                }}
              >
                Cancelar
              </button>
            </div>
            {!editingUser && newUser.password && newUser.confirmPassword && newUser.password !== newUser.confirmPassword && (
              <div className="error-message">
                Las contraseñas no coinciden
              </div>
            )}
          </div>
        ) : null}

        <div className="users-table-card">
          <h2 className="table-title">Lista de Usuarios</h2>
          <div className="table-container">
            <table className="users-table">
              <thead>
                <tr>
                  <th>Usuario</th>
                  <th>Nombre</th>
                  <th>Correo</th>
                  <th>Teléfono</th>
                  <th>Dirección</th>
                  <th>Rol</th>
                  <th>Acciones</th>
                </tr>
              </thead>
              <tbody>
                {users.map(user => (
                  <React.Fragment key={user.idUsuario}>
                    <tr>
                      <td>{user.username}</td>
                      <td>{user.nombre}</td>
                      <td>{user.correo}</td>
                      <td>{user.telefono}</td>
                      <td>{user.direccion}</td>
                      <td>
                        <span className={`role-badge role-${user.rol}`}>
                          {getRoleDisplayName(user.rol)}
                        </span>
                      </td>
                      <td>
                        <div className="action-buttons">
                          <button 
                            className="btn btn-sm btn-outline-primary"
                            onClick={() => handleEditUser(user)}
                          >
                            Editar
                          </button>
                          <button 
                            className="btn btn-sm btn-outline-danger"
                            onClick={() => handleDeleteUser(user.idUsuario)}
                          >
                            Eliminar
                          </button>
                          <button 
                            className="btn btn-sm btn-outline-info"
                            onClick={() => handleViewUserTickets(user.idUsuario)}
                          >
                            {selectedUserTickets === user.idUsuario ? 'Ocultar Entradas' : 'Ver Entradas'}
                          </button>
                        </div>
                      </td>
                    </tr>
                    {selectedUserTickets === user.idUsuario && (
                      <tr>
                        <td colSpan="7">
                          <div className="user-tickets-section">
                            <div className="user-tickets-header">
                              <h3>Entradas de {user.nombre}</h3>
                              <div className="user-tickets-filters">
                                <label className="filter-label">Estado:</label>
                                <select
                                  className="filter-select"
                                  value={ticketsFilter}
                                  onChange={handleTicketsFilterChange}
                                >
                                  <option value="PorPagar">Por Pagar</option>
                                  <option value="Pagado">Pagado</option>
                                  <option value="Cancelada">Cancelada</option>
                                </select>
                              </div>
                            </div>
                            
                            {ticketsLoading ? (
                              <div className="loading-container">
                                <div className="loading-spinner"></div>
                                <p>Cargando entradas...</p>
                              </div>
                            ) : userTickets.length === 0 ? (
                              <div className="empty-state-card">
                                <p>No se encontraron entradas con el estado seleccionado</p>
                              </div>
                            ) : (
                              <div className="user-tickets-grid">
                                {userTickets.map(ticket => (
                                  <div key={ticket.idEntrada} className="user-ticket-card">
                                    <div className="user-ticket-card-header">
                                      <div className={`user-ticket-card-status user-ticket-card-status-${ticket.estado.toLowerCase()}`}>
                                        {ticket.estado}
                                      </div>
                                    </div>
                                    
                                    <div className="user-ticket-card-content">
                                      <div className="user-ticket-card-info">
                                        <h4 className="user-ticket-card-title">Entrada General</h4>
                                        <div className="user-ticket-card-details">
                                          <div className="user-ticket-card-detail">
                                            <span className="user-ticket-card-detail-label">ID:</span>
                                            <span className="user-ticket-card-detail-value">{ticket.idEntrada.substring(0, 8)}...</span>
                                          </div>
                                          <div className="user-ticket-card-detail">
                                            <span className="user-ticket-card-detail-label">Categoría:</span>
                                            <span className="user-ticket-card-detail-value">{ticket.categoria}</span>
                                          </div>
                                          <div className="user-ticket-card-detail">
                                            <span className="user-ticket-card-detail-label">Precio:</span>
                                            <span className="user-ticket-card-detail-value">${ticket.precio.toFixed(2)} {ticket.moneda}</span>
                                          </div>
                                        </div>
                                      </div>
                                    </div>
                                  </div>
                                ))}
                              </div>
                            )}
                          </div>
                        </td>
                      </tr>
                    )}
                  </React.Fragment>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  );
};

export default UsersPage;