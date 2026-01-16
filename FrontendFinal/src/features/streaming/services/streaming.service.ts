import api from '@/lib/axios';

export interface Transmision {
    id: string;
    eventoId: string;
    plataforma: string;
    urlAcceso: string;
    estado: string;
}

export const streamingService = {
    getTransmision: async (eventoId: string): Promise<Transmision | null> => {
        try {
            const response = await api.get<Transmision>(`/streaming/${eventoId}`);
            return response.data;
        } catch (error) {
            console.error('Error al obtener transmisión:', error);
            return null;
        }
    },
    crearObtenerTransmision: async (eventoId: string): Promise<Transmision | null> => {
        try {
            const response = await api.post<Transmision>(`/streaming/${eventoId}`);
            return response.data;
        } catch (error) {
            console.error('Error al crear/obtener transmisión:', error);
            return null;
        }
    },
    actualizarUrl: async (eventoId: string, url: string): Promise<Transmision | null> => {
        try {
            const response = await api.put<Transmision>(`/streaming/${eventoId}/url`, { url });
            return response.data;
        } catch (error) {
            console.error('Error al actualizar URL de transmisión:', error);
            return null;
        }
    }
};
