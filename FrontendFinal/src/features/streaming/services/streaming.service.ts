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
    }
};
