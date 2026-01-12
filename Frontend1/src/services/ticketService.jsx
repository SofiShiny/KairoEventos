import axios from 'axios';
import keycloak from '../Keycloak';

const API_BASE_URL = 'http://localhost:5268';

const api = axios.create({
  baseURL: API_BASE_URL,
});

api.interceptors.request.use((config) => {
  if (keycloak.token) {
    config.headers.Authorization = `Bearer ${keycloak.token}`;
  }
  return config;
}, (error) => {
  return Promise.reject(error);
});

const ticketService = {
  agregarEntrada: async (ticketData) => {
    try {
      const response = await api.post(`/entradas/agregar`, ticketData);
      return response.data;
    } catch (error) {
      throw error;
    }
  },

  eliminarEntrada: async (id) => {
    try {
      const response = await api.delete(`/entradas/eliminar/${id}`);
      return response.data;
    } catch (error) {
      throw error;
    }
  },

  consultarEntradas: async () => {
    try {
      const response = await api.get(`/entradas/consultar`);
      return response.data;
    } catch (error) {
      throw error;
    }
  },

  consultarEntradaPorId: async (id) => {
    try {
      const response = await api.get(`/entradas/consultar/${id}`);
      return response.data;
    } catch (error) {
      throw error;
    }
  },

  consultarEntradasPorIdEvento: async (idEvento) => {
    try {
      const response = await api.get(`/entradas/consultarIdEvento/${idEvento}`);
      return response.data;
    } catch (error) {
      throw error;
    }
  },

  consultarEntradasPorIdUsuario: async (idUsuario, estado) => {
    try {
      const response = await api.get(`/entradas/consultarIdUsuario/${idUsuario}?estado=${estado}`);
      return response.data;
    } catch (error) {
      throw error;
    }
  },

  cancelarEntrada: async (id) => {
    try {
      const response = await api.put(`/entradas/cancelar/${id}`);
      return response.data;
    } catch (error) {
      throw error;
    }
  }
};

export default ticketService;