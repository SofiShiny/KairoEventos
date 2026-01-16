import api from '@/lib/axios';

export interface BuyTicketPayload {
    eventoId: string;
    usuarioId: string;
    asientoIds: string[]; // Ahora es una lista de IDs
    cupones?: string[];
    nombreUsuario?: string;
    email?: string;
}

export interface Entrada {
    id: string;
    eventoId: string;
    usuarioId: string;
    asientoId: string;
    precio: number;
    codigoQr: string;
    estado: string;
    fechaCompra: string;
    eventoNombre: string;
    asientoInfo: string;
    imagenEventoUrl?: string;
    fechaEvento?: string;
    esVirtual?: boolean;
    nombreUsuario?: string;
    emailUsuario?: string;
}

const ESTADOS_MAP: Record<number, string> = {
    0: 'Reservada',
    1: 'Pendiente de Pago',
    2: 'Pagada',
    3: 'Cancelada',
    4: 'Usada'
};

export const entradasService = {
    crearEntrada: async (payload: BuyTicketPayload) => {
        const response = await api.post('/entradas', payload);
        return response.data;
    },

    getMisEntradas: async (usuarioId: string): Promise<Entrada[]> => {
        const response = await api.get(`/entradas/usuario/${usuarioId}`);
        const data = response.data.data || [];

        return data.map((item: any) => {
            // Normalización ultra-robusta de campos
            return {
                ...item,
                // Precio: puede venir como precio, monto o montoFinal
                precio: Number(item.precio || item.monto || item.montoFinal || 0),
                // Estado: puede venir como número o string
                estado: typeof item.estado === 'number' ? (ESTADOS_MAP[item.estado] || 'Desconocido') : String(item.estado || 'Pendiente'),
                // Nombres de evento
                eventoNombre: item.eventoNombre || item.tituloEvento || 'Evento Desconocido',
                // Info Asiento
                asientoInfo: item.asientoInfo || (item.fila ? `Fila ${item.fila}, Asiento ${item.numero}` : 'General')
            };
        });
    },

    getTodasLasEntradas: async (organizadorId?: string): Promise<Entrada[]> => {
        const url = `/entradas/todas${organizadorId ? `?organizadorId=${organizadorId}` : ''}`;
        const response = await api.get(url);
        const data = response.data.data || [];

        return data.map((item: any) => ({
            ...item,
            id: item.id,
            eventoId: item.eventoId,
            usuarioId: item.usuarioId,
            asientoId: item.asientoId,
            precio: Number(item.monto || item.precio || 0),
            codigoQr: item.codigoQr,
            estado: typeof item.estado === 'number' ? (ESTADOS_MAP[item.estado] || 'Desconocido') : String(item.estado || 'Pendiente'),
            eventoNombre: item.tituloEvento || 'Evento Desconocido',
            asientoInfo: item.nombreSector ? `${item.nombreSector}${item.fila ? ` - Fila ${item.fila}` : ''}${item.numeroAsiento ? ` - Asiento ${item.numeroAsiento}` : ''}` : 'General',
            fechaCompra: item.fechaCompra,
            nombreUsuario: item.nombreUsuario,
            emailUsuario: item.emailUsuario
        }));
    },

    cancelarEntrada: async (id: string, usuarioId: string) => {
        const response = await api.put(`/entradas/${id}/cancelar?usuarioId=${usuarioId}`);
        return response.data;
    }
};
