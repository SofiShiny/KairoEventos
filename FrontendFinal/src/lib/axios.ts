import axios, { AxiosError, InternalAxiosRequestConfig } from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:8080/api';
const OIDC_KEY = 'oidc.user:http://localhost:8180/realms/Kairo:kairo-web';

const axiosInstance = axios.create({
    baseURL: API_BASE_URL,
    headers: {
        'Content-Type': 'application/json',
    },
});

// Obtener el token del almacenamiento OIDC sin hooks
const getOidcToken = () => {
    const oidcStorage = localStorage.getItem(OIDC_KEY);
    if (!oidcStorage) return null;

    try {
        const user = JSON.parse(oidcStorage);
        return user.access_token;
    } catch (e) {
        return null;
    }
};

axiosInstance.interceptors.request.use(
    (config: InternalAxiosRequestConfig) => {
        const token = getOidcToken();
        if (token && config.headers) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => Promise.reject(error)
);

axiosInstance.interceptors.response.use(
    (response) => response,
    async (error: AxiosError) => {
        if (error.response?.status === 401) {
            // Nota: Aquí se podría disparar un evento global o usar el UserManager 
            // para refrescar el token, pero la redirección es lo más seguro.
            localStorage.removeItem(OIDC_KEY);
            window.location.href = '/login';
        }
        return Promise.reject(error);
    }
);

export default axiosInstance;
