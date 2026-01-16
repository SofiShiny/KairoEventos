import api from '@/lib/axios';
import { Evento } from '../types/evento.types';

const getBaseHost = () => {
    const apiUrl = import.meta.env.VITE_API_URL || 'http://localhost:8080/api';
    return apiUrl.replace('/api', '');
};

const formatImageUrl = (url?: string | null) => {
    if (!url) return null;
    if (url.startsWith('http')) return url;
    return `${getBaseHost()}${url}`;
};

export const eventosService = {
    getEventos: async (): Promise<Evento[]> => {
        const response = await api.get<any[]>('/eventos');
        const first = response.data?.[0];
        if (first) {
            console.log('DEBUG: Evento #1 keys:', Object.keys(first));
            console.log('DEBUG: Evento #1 snapshot:', {
                id: first.id || first.Id,
                titulo: first.titulo || first.Titulo,
                esVirtual: first.esVirtual,
                EsVirtual: first.EsVirtual,
                precioBase: first.precioBase,
                PrecioBase: first.PrecioBase
            });
        }
        return response.data.map(e => ({
            id: e.id,
            titulo: e.titulo,
            descripcion: e.descripcion,
            fechaInicio: e.fechaInicio,
            fechaFin: e.fechaFin,
            lugar: e.ubicacion?.nombreLugar || 'Lugar no especificado',
            imagenUrl: formatImageUrl(e.urlImagen),
            categoria: e.categoria || 'General',
            estado: e.estado,
            maximoAsistentes: e.maximoAsistentes,
            organizadorId: e.organizadorId,
            esVirtual: e.esVirtual === true || e.EsVirtual === true,
            precioBase: e.precioBase ?? e.PrecioBase ?? 0
        }));
    },

    getEventosPublicados: async (): Promise<Evento[]> => {
        const response = await api.get<any[]>('/eventos/publicados');
        return response.data.map(e => ({
            id: e.id,
            titulo: e.titulo,
            descripcion: e.descripcion,
            fechaInicio: e.fechaInicio,
            fechaFin: e.fechaFin,
            lugar: e.ubicacion?.nombreLugar || 'Lugar no especificado',
            imagenUrl: formatImageUrl(e.urlImagen),
            categoria: e.categoria || 'General',
            estado: e.estado,
            maximoAsistentes: e.maximoAsistentes,
            organizadorId: e.organizadorId,
            esVirtual: e.esVirtual === true || e.EsVirtual === true,
            precioBase: e.precioBase ?? e.PrecioBase ?? 0
        }));
    },

    getEventoById: async (id: string): Promise<Evento> => {
        const response = await api.get<any>(`/eventos/${id}`);
        const e = response.data;
        return {
            id: e.id,
            titulo: e.titulo,
            descripcion: e.descripcion,
            fechaInicio: e.fechaInicio,
            fechaFin: e.fechaFin,
            lugar: e.ubicacion?.nombreLugar || 'Lugar no especificado',
            imagenUrl: formatImageUrl(e.urlImagen),
            categoria: e.categoria || 'General',
            estado: e.estado,
            maximoAsistentes: e.maximoAsistentes,
            organizadorId: e.organizadorId,
            esVirtual: e.esVirtual === true || e.EsVirtual === true,
            precioBase: e.precioBase ?? e.PrecioBase ?? 0
        };
    },

    crearEvento: async (evento: Partial<Evento>): Promise<Evento> => {
        const payload = {
            titulo: evento.titulo,
            descripcion: evento.descripcion,
            ubicacion: {
                nombreLugar: evento.lugar,
                direccion: 'Dirección por defecto',
                ciudad: 'Ciudad Ejemplo',
                pais: 'País'
            },
            fechaInicio: evento.fechaInicio,
            fechaFin: evento.fechaFin || evento.fechaInicio,
            maximoAsistentes: evento.maximoAsistentes || 100,
            categoria: evento.categoria || 'General'
        };
        const response = await api.post<Evento>('/eventos', payload);
        return response.data;
    },

    actualizarEvento: async (id: string, evento: Partial<Evento>): Promise<Evento> => {
        const payload = {
            titulo: evento.titulo,
            descripcion: evento.descripcion,
            ubicacion: {
                nombreLugar: evento.lugar,
                direccion: 'Dirección por defecto',
                ciudad: 'Ciudad Ejemplo',
                pais: 'País'
            },
            fechaInicio: evento.fechaInicio,
            fechaFin: evento.fechaFin || evento.fechaInicio,
            maximoAsistentes: evento.maximoAsistentes
        };
        const response = await api.put<Evento>(`/eventos/${id}`, payload);
        return response.data;
    },

    eliminarEvento: async (id: string): Promise<void> => {
        await api.delete(`/eventos/${id}`);
    },

    publicarEvento: async (id: string): Promise<void> => {
        await api.patch(`/eventos/${id}/publicar`);
    }
};
