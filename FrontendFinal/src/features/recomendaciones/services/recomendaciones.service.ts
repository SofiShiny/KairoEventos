import api from '@/lib/axios';
import { Evento } from '../../eventos/types/evento.types';

export interface EventoRecomendado {
    id: string;
    titulo: string;
    descripcion?: string;
    fechaInicio: string;
    lugar: string;
    imagenUrl?: string;
    categoria: string;
    puntuacion: number; // Score de afinidad
    razonRecomendacion?: string;
}

export interface RecomendacionesPersonalizadas {
    usuarioId: string;
    recomendaciones: EventoRecomendado[];
    fechaGeneracion: string;
}

export const recomendacionesService = {
    getMisRecomendaciones: async (): Promise<RecomendacionesPersonalizadas> => {
        const response = await api.get('/recomendaciones/mi-perfil');
        return response.data;
    },

    getTendencias: async (limite: number = 5): Promise<EventoRecomendado[]> => {
        const response = await api.get(`/recomendaciones/tendencias?limite=${limite}`);
        return response.data;
    }
};
