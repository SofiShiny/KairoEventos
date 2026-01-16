import api from '@/lib/axios';

export interface ServicioComplementario {
    id: string;
    nombre: string;
    precio: number;
    activo: boolean;
    proveedores: any[];
}

export interface ReservarServicioRequest {
    usuarioId: string;
    eventoId: string;
    servicioGlobalId: string;
}

export const serviciosService = {
    // El endpoint detectado es /api/servicios/catalogo
    // Nota: Aunque el nombre sugiere catálogo global, lo usaremos para listar servicios disponibles.
    getServiciosPorEvento: async (eventoId: string): Promise<ServicioComplementario[]> => {
        // En una implementación ideal, filtraríamos por eventoId. 
        // Dado que el endpoint es genérico, obtenemos todo el catálogo.
        const response = await api.get('/servicios/catalogo');
        return response.data;
    },

    reservarServicio: async (request: ReservarServicioRequest): Promise<string> => {
        const response = await api.post('/servicios/reservar', request);
        return response.data; // Retorna el Guid de la reserva
    },

    getMisReservas: async (usuarioId: string): Promise<any[]> => {
        const response = await api.get(`/servicios/mis-reservas/${usuarioId}`);
        return response.data;
    }
};
