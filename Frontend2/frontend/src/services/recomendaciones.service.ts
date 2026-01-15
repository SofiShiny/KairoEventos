import axios from 'axios';

const GATEWAY_URL = import.meta.env.VITE_GATEWAY_URL || 'http://localhost:8080';

export interface EventoRecomendado {
    id: string;
    titulo: string;
    descripcion?: string;
    categoria?: string;
    fechaInicio: string;
    fechaFin: string;
    urlImagen?: string;
    nombreLugar?: string;
    ciudad?: string;
    entradasVendidas: number;
    precioDesde: number;
}

export interface RecomendacionesPersonalizadas {
    tipoRecomendacion: string;
    categoriaFavorita?: string;
    tituloSeccion: string;
    eventos: EventoRecomendado[];
}

class RecomendacionesService {
    /**
     * Obtiene los eventos más populares (tendencias)
     */
    async getTendencias(limite: number = 5): Promise<EventoRecomendado[]> {
        try {
            const response = await axios.get<EventoRecomendado[]>(
                `${GATEWAY_URL}/api/recomendaciones/tendencias`,
                {
                    params: { limite }
                }
            );
            return response.data;
        } catch (error) {
            console.error('Error al obtener tendencias:', error);
            return [];
        }
    }

    /**
     * Obtiene recomendaciones personalizadas para un usuario
     */
    async getRecomendacionesUsuario(usuarioId: string): Promise<RecomendacionesPersonalizadas> {
        try {
            const response = await axios.get<RecomendacionesPersonalizadas>(
                `${GATEWAY_URL}/api/recomendaciones/usuario/${usuarioId}`
            );
            return response.data;
        } catch (error) {
            console.error('Error al obtener recomendaciones personalizadas:', error);
            // Fallback a tendencias
            const tendencias = await this.getTendencias(6);
            return {
                tipoRecomendacion: 'Tendencias',
                tituloSeccion: 'Descubre eventos populares',
                eventos: tendencias
            };
        }
    }

    /**
     * Obtiene recomendaciones para el usuario autenticado
     */
    async getMisRecomendaciones(token: string): Promise<RecomendacionesPersonalizadas> {
        try {
            const response = await axios.get<RecomendacionesPersonalizadas>(
                `${GATEWAY_URL}/api/recomendaciones/mi-perfil`,
                {
                    headers: {
                        Authorization: `Bearer ${token}`
                    }
                }
            );
            return response.data;
        } catch (error) {
            console.error('Error al obtener mis recomendaciones:', error);
            // Fallback a tendencias
            const tendencias = await this.getTendencias(6);
            return {
                tipoRecomendacion: 'Tendencias',
                tituloSeccion: 'Descubre eventos populares',
                eventos: tendencias
            };
        }
    }
}

export const recomendacionesService = new RecomendacionesService();
