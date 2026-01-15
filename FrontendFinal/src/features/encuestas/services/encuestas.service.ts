import api from '@/lib/axios';

export enum TipoPregunta {
    Estrellas = 0,
    Texto = 1
}

export interface Pregunta {
    id: string;
    enunciado: string;
    tipo: TipoPregunta;
}

export interface Encuesta {
    id: string;
    eventoId: string;
    titulo: string;
    publicada: boolean;
    preguntas: Pregunta[];
}

export interface RespuestaValor {
    preguntaId: string;
    valor: string;
}

export interface ResponderEncuestaCommand {
    encuestaId: string;
    usuarioId: string;
    respuestas: RespuestaValor[];
}

const BASE_URL = 'http://localhost:5055/api';

export const encuestasService = {
    getPorEvento: async (eventoId: string): Promise<Encuesta> => {
        const response = await api.get(`${BASE_URL}/encuestas/evento/${eventoId}`);
        return response.data;
    },

    crear: async (data: { eventoId: string, titulo: string, preguntas: any[] }): Promise<string> => {
        const response = await api.post(`${BASE_URL}/encuestas`, data);
        return response.data;
    },

    publicar: async (encuestaId: string): Promise<void> => {
        await api.post(`${BASE_URL}/encuestas/${encuestaId}/publicar`);
    },

    responder: async (command: ResponderEncuestaCommand): Promise<string> => {
        const response = await api.post(`${BASE_URL}/encuestas/responder`, command);
        return response.data;
    }
};
