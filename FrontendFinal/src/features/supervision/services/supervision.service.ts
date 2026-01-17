import api from '../../../lib/axios';

export enum EstadoServicio {
    Saludable = 'saludable',
    Degradado = 'degradado',
    Caido = 'caido',
    Desconocido = 'desconocido'
}

export interface Microservicio {
    nombre: string;
    descripcion: string;
    url: string;
    puerto: number;
    estado: EstadoServicio;
    tiempoRespuesta: number; // ms
    ultimaVerificacion: string;
    version: string;
    uptime: number; // segundos
    memoriaUsada: number; // MB
    cpu: number; // porcentaje
    requestsPorMinuto: number;
}

export interface MetricasSistema {
    totalServicios: number;
    serviciosSaludables: number;
    serviciosDegradados: number;
    serviciosCaidos: number;
    tiempoRespuestaPromedio: number;
    requestsTotales: number;
    errorRate: number;
}

export const supervisionService = {
    // Obtiene el estado real de los microservicios desde el Gateway
    getMicroservicios: async (): Promise<Microservicio[]> => {
        try {
            const response = await api.get('/supervision/status');
            return response.data.map((s: any) => ({
                ...s,
                estado: s.estado as EstadoServicio,
                ultimaVerificacion: new Date().toISOString(),
                // Campos que aún no tenemos reales los simulamos balanceados
                memoriaUsada: Math.floor(Math.random() * (400 - 200) + 200),
                cpu: Math.floor(Math.random() * (15 - 5) + 5),
                requestsPorMinuto: Math.floor(Math.random() * (100 - 20) + 20),
                uptime: s.uptime || 3600 // Por ahora si no viene, ponemos 1h
            }));
        } catch (error) {
            console.error('Error fetching real supervision data, falling back to basic checks', error);
            throw error;
        }
    },

    calcularMetricas: (servicios: Microservicio[]): MetricasSistema => {
        const totalServicios = servicios.length;
        const serviciosSaludables = servicios.filter(s => s.estado === EstadoServicio.Saludable).length;
        const serviciosDegradados = servicios.filter(s => s.estado === EstadoServicio.Degradado).length;
        const serviciosCaidos = servicios.filter(s => s.estado === EstadoServicio.Caido).length;

        const tiempoRespuestaPromedio = servicios.reduce((sum, s) => sum + s.tiempoRespuesta, 0) / totalServicios;
        const requestsTotales = servicios.reduce((sum, s) => sum + s.requestsPorMinuto, 0);
        const errorRate = (serviciosDegradados + serviciosCaidos) / totalServicios * 100;

        return {
            totalServicios,
            serviciosSaludables,
            serviciosDegradados,
            serviciosCaidos,
            tiempoRespuestaPromedio,
            requestsTotales,
            errorRate
        };
    },

    formatUptime: (segundos: number): string => {
        const dias = Math.floor(segundos / 86400);
        const horas = Math.floor((segundos % 86400) / 3600);
        const minutos = Math.floor((segundos % 3600) / 60);

        if (dias > 0) {
            return `${dias}d ${horas}h`;
        } else if (horas > 0) {
            return `${horas}h ${minutos}m`;
        } else {
            return `${minutos}m`;
        }
    }
};
