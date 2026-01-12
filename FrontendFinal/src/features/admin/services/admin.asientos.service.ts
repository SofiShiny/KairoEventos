import api from '@/lib/axios';

export interface CategoriaAsiento {
    id: string;
    nombre: string;
    precioBase: number;
    tienePrioridad: boolean;
}

export interface SeatConfig {
    mapaId: string;
    categorias: CategoriaAsiento[];
}

export const adminAsientosService = {
    getMapaByEvento: async (eventoId: string): Promise<any> => {
        try {
            const response = await api.get(`/asientos/mapas/evento/${eventoId}`);
            return response.data;
        } catch (error: any) {
            if (error.response?.status === 404) return null;
            throw error;
        }
    },

    createMapa: async (eventoId: string): Promise<{ mapaId: string }> => {
        const response = await api.post('/asientos/mapas', { eventoId });
        return response.data;
    },

    createCategoria: async (data: {
        mapaId: string;
        nombre: string;
        precioBase?: number;
        tienePrioridad: boolean;
    }): Promise<any> => {
        const response = await api.post('/asientos/categorias', data);
        return response.data;
    },

    createAsiento: async (data: {
        mapaId: string;
        fila: number;
        numero: number;
        categoria: string;
    }): Promise<any> => {
        const response = await api.post('/asientos', data);
        return response.data;
    }
};
