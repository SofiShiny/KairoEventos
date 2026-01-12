import axios from 'axios';

const API_URL = import.meta.env.VITE_USUARIOS_API_URL || 'http://localhost:5005';

export interface RegisterData {
    username: string;
    nombre: string;
    correo: string;
    telefono: string;
    direccion: string;
    password: string;
    rol?: number;
}

export interface RegisterResponse {
    id: string;
    message?: string;
}

class AuthService {
    async register(data: RegisterData): Promise<RegisterResponse> {
        try {
            const payload = {
                username: data.username,
                nombre: data.nombre,
                correo: data.correo,
                telefono: data.telefono || '0000000000',
                direccion: data.direccion || 'No especificada',
                rol: data.rol || 1, // 1 = User por defecto
                password: data.password
            };

            const response = await axios.post(`${API_URL}/api/Usuarios`, payload);
            return response.data;
        } catch (error: any) {
            if (error.response?.data?.message) {
                throw new Error(error.response.data.message);
            }
            throw new Error('Error al crear la cuenta. Por favor intenta de nuevo.');
        }
    }
}

export const authService = new AuthService();
