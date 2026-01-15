import axios from 'axios';

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

export interface ServicioGlobal {
    id: string;
    nombre: string;
    precio: number;
    activo: boolean;
}

export interface ReservaServicio {
    id: string;
    usuarioId: string;
    eventoId: string;
    servicioGlobalId: string;
    estado: string;
    fechaCreacion: string;
}

export const serviciosService = {
    getCatalogo: async (): Promise<ServicioGlobal[]> => {
        const response = await axios.get(`${API_URL}/servicios/catalogo`);
        return response.data;
    },

    reservar: async (command: { usuarioId: string; eventoId: string; servicioGlobalId: string }): Promise<string> => {
        const response = await axios.post(`${API_URL}/servicios/reservar`, command);
        return response.data;
    },

    getMisReservas: async (usuarioId: string): Promise<ReservaServicio[]> => {
        const response = await axios.get(`${API_URL}/servicios/mis-reservas/${usuarioId}`);
        return response.data;
    }
};
