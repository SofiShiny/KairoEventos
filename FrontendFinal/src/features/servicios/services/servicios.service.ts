import api from '@/lib/axios';

export interface Proveedor {
    id: string;
    nombreProveedor: string;
    precio: number;
    estaDisponible: boolean;
    externalId: string;
}

export interface ServicioComplementario {
    id: string;
    nombre: string;
    precio: number;
    activo: boolean;
    proveedores: Proveedor[];
}

export interface ReservarServicioRequest {
    usuarioId: string;
    eventoId: string;
    servicioGlobalId: string;
    ordenEntradaId?: string;
}

export const serviciosService = {
    // El endpoint detectado es /api/servicios/catalogo
    // Nota: Aunque el nombre sugiere catálogo global, lo usaremos para listar servicios disponibles.
    getServiciosPorEvento: async (_eventoId: string): Promise<ServicioComplementario[]> => {
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
    },

    // Admin Methods
    getServiciosExternos: async (): Promise<Proveedor[]> => {
        // Mapeamos los campos del DTO externo a nuestra interfaz Proveedor
        const response = await api.get('/AdminServicios/externos');
        return response.data.map((s: any) => ({
            id: s.idServicioExterno, // Usamos el ID externo en la UI de admin
            nombreProveedor: s.nombre,
            precio: s.precio,
            estaDisponible: s.disponible,
            externalId: s.idServicioExterno,
            tipo: s.tipo
        }));
    },

    updateServicioExterno: async (idExterno: string, precio: number, disponible: boolean): Promise<void> => {
        await api.post('/AdminServicios/externos/update', {
            idExterno,
            precio,
            disponible
        });
    },

    getServiciosGlobales: async (): Promise<any[]> => {
        const response = await api.get('/AdminServicios/globales');
        return response.data;
    },

    updateServicioGlobal: async (id: string, nombre: string, precio: number): Promise<void> => {
        await api.post('/AdminServicios/globales/update', {
            id,
            nombre,
            precio
        });
    }
};
