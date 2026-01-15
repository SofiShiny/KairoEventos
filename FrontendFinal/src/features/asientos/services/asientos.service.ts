import api from '@/lib/axios';

export interface Asiento {
    id: string;
    fila: number;
    numero: number;
    categoria: string;
    precio: number;
    reservado: boolean;
    usuarioId?: string;
    estado: 'Disponible' | 'Reservado' | 'Ocupado';
}

export interface MapaAsientos {
    id: string;
    eventoId: string;
    filas: number;
    columnas: number;
    asientos: Asiento[];
}

class AsientosService {
    async getByEvento(eventoId: string): Promise<Asiento[]> {
        try {
            // Primero obtenemos el mapa por eventoId usando la instancia 'api' (que ya tiene /api de base)
            const mapaResponse = await api.get(`/asientos/mapas/evento/${eventoId}`);
            const mapaId = mapaResponse.data.id;

            // Luego obtenemos los asientos completos del mapa
            const response = await api.get(`/asientos/mapas/${mapaId}`);

            console.log('Datos del mapa:', response.data);

            // Transformamos los asientos para incluir el estado
            const asientos = response.data.asientos.map((asiento: any) => ({
                id: asiento.id,
                fila: asiento.fila,
                numero: asiento.numero,
                categoria: asiento.categoria,
                precio: asiento.precio || 0,
                reservado: asiento.reservado,
                usuarioId: asiento.usuarioId,
                estado: asiento.usuarioId ? 'Ocupado' : (asiento.reservado ? 'Reservado' : 'Disponible')
            }));

            console.log('Asientos transformados:', asientos);

            return asientos;
        } catch (error) {
            console.error('Error al obtener asientos:', error);
            throw new Error('No se pudieron cargar los asientos del evento');
        }
    }

    async reservarAsiento(mapaId: string, asientoId: string, usuarioId: string): Promise<void> {
        try {
            await api.post('/asientos/reservar', {
                mapaId,
                asientoId,
                usuarioId
            });
        } catch (error: any) {
            if (error.response?.data?.message) {
                throw new Error(error.response.data.message);
            }
            throw new Error('Error al reservar el asiento');
        }
    }

    async liberarAsiento(mapaId: string, asientoId: string): Promise<void> {
        try {
            await api.post('/asientos/liberar', {
                mapaId,
                asientoId
            });
        } catch (error: any) {
            if (error.response?.data?.message) {
                throw new Error(error.response.data.message);
            }
            throw new Error('Error al liberar el asiento');
        }
    }
}

export const asientosService = new AsientosService();
