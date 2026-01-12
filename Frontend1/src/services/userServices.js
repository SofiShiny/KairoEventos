import axios from "axios";
import keycloak from "../Keycloak";

const api = axios.create({
  baseURL: "http://localhost:5268",
});

api.interceptors.request.use((config) => {
  console.log('Interceptor - Token available:', !!keycloak.token);
  if (keycloak.token) {
    config.headers.Authorization = `Bearer ${keycloak.token}`;
    console.log('Interceptor - Token added to request');
  } else {
    console.log('Interceptor - No token available');
  }
  return config;
});

api.interceptors.response.use(
  (response) => {
    console.log('API Response:', response.status);
    return response;
  },
  (error) => {
    console.error('API Error:', error.response?.status, error.response?.data);
    return Promise.reject(error);
  }
);

export const userServices = {
  consultar: () => {
    console.log('Consultar - Making request');
    return api.get("/usuarios/consultar");
  },
  consultarId: (id) => {
    console.log('ConsultarId - Making request for ID:', id);
    return api.get(`/usuarios/consultarId/${id}`);
  },
  consultarPerfilId: (id) => {
    console.log('ConsultarPerfilId - Making request for ID:', id);
    return api.get(`/usuarios/consultarPerfilId/${id}`);
  },
  agregar: (usuario) => {
    console.log('Agregar - Making request with user data');
    return api.post("/usuarios/agregar", usuario);
  },
  agregarToken: (token) => {
    console.log('AgregarToken - Making request with token');
    return api.post("/usuarios/agregarToken", token);
  },
  modificar: (id, usuario) => {
    console.log('Modificar - Making request for ID:', id);
    return api.put(`/usuarios/modificar/${id}`, usuario);
  },
  eliminar: (id) => {
    console.log('Eliminar - Making request for ID:', id);
    return api.delete(`/usuarios/eliminar/${id}`);
  }
};