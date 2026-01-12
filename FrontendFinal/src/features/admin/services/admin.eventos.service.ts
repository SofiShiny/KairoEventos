import api from '@/lib/axios';
import { Evento } from '../../eventos/types/evento.types';

export const adminEventosService = {
    /**
     * Crea un nuevo evento enviando JSON
     */
    createEvento: async (data: any): Promise<Evento> => {
        const response = await api.post<Evento>('/eventos', data);
        return response.data;
    },

    /**
     * Actualiza un evento existente enviando JSON
     */
    updateEvento: async (id: string, data: any): Promise<Evento> => {
        const response = await api.put<Evento>(`/eventos/${id}`, data);
        return response.data;
    },

    /**
     * Sube la imagen del evento como una operación separada
     */
    uploadImagen: async (id: string, file: File): Promise<{ url: string }> => {
        const formData = new FormData();
        formData.append('archivo', file);
        const response = await api.post<{ url: string }>(`/eventos/${id}/imagen`, formData, {
            headers: {
                'Content-Type': 'multipart/form-data',
            },
        });
        return response.data;
    },

    /**
     * Elimina un evento
     */
    deleteEvento: async (id: string): Promise<void> => {
        await api.delete(`/eventos/${id}`);
    },

    /**
     * Publica un evento (cambia estado a Publicado)
     */
    publicarEvento: async (id: string): Promise<void> => {
        await api.patch(`/eventos/${id}/publicar`);
    }
};
