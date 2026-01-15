import api from '@/lib/axios';

export interface Respuesta {
    usuarioId: string;
    contenido: string;
    fechaCreacion: string;
}

export interface Comentario {
    id: string;
    foroId: string;
    usuarioId: string;
    contenido: string;
    fechaCreacion: string;
    visible: boolean;
    respuestas: Respuesta[];
}

export interface CrearComentarioRequest {
    foroId: string;
    usuarioId: string;
    contenido: string;
}

export interface ResponderComentarioRequest {
    usuarioId: string;
    contenido: string;
}

export const forosService = {
    getComentariosPorEvento: async (eventoId: string): Promise<Comentario[]> => {
        const response = await api.get(`/comunidad/foros/${eventoId}`);
        return response.data;
    },

    crearComentario: async (request: CrearComentarioRequest): Promise<string> => {
        const response = await api.post(`/comunidad/comentarios`, request);
        return response.data;
    },

    responderComentario: async (comentarioId: string, request: ResponderComentarioRequest): Promise<void> => {
        await api.post(`/comunidad/comentarios/${comentarioId}/responder`, request);
    },

    ocultarComentario: async (comentarioId: string): Promise<void> => {
        await api.delete(`/comunidad/comentarios/${comentarioId}`);
    }
};
