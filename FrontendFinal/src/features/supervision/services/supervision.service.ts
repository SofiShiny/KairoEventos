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
    // Simula el estado de los microservicios
    getMicroservicios: async (): Promise<Microservicio[]> => {
        // En producción, esto haría llamadas reales a health check endpoints
        return [
            {
                nombre: 'Gateway',
                descripcion: 'API Gateway - Punto de entrada principal',
                url: 'http://localhost:5000',
                puerto: 5000,
                estado: EstadoServicio.Saludable,
                tiempoRespuesta: 45,
                ultimaVerificacion: new Date().toISOString(),
                version: '1.0.0',
                uptime: 86400, // 1 día
                memoriaUsada: 256,
                cpu: 15,
                requestsPorMinuto: 120
            },
            {
                nombre: 'Eventos',
                descripcion: 'Gestión de eventos y publicaciones',
                url: 'http://localhost:5001',
                puerto: 5001,
                estado: EstadoServicio.Saludable,
                tiempoRespuesta: 52,
                ultimaVerificacion: new Date().toISOString(),
                version: '1.2.1',
                uptime: 172800, // 2 días
                memoriaUsada: 512,
                cpu: 25,
                requestsPorMinuto: 85
            },
            {
                nombre: 'Entradas',
                descripcion: 'Venta y gestión de entradas',
                url: 'http://localhost:5002',
                puerto: 5002,
                estado: EstadoServicio.Saludable,
                tiempoRespuesta: 38,
                ultimaVerificacion: new Date().toISOString(),
                version: '1.1.0',
                uptime: 259200, // 3 días
                memoriaUsada: 384,
                cpu: 18,
                requestsPorMinuto: 95
            },
            {
                nombre: 'Asientos',
                descripcion: 'Gestión de asientos y mapas',
                url: 'http://localhost:5003',
                puerto: 5003,
                estado: EstadoServicio.Saludable,
                tiempoRespuesta: 41,
                ultimaVerificacion: new Date().toISOString(),
                version: '1.0.5',
                uptime: 432000, // 5 días
                memoriaUsada: 320,
                cpu: 12,
                requestsPorMinuto: 60
            },
            {
                nombre: 'Reservas',
                descripcion: 'Sistema de reservas temporales',
                url: 'http://localhost:5004',
                puerto: 5004,
                estado: EstadoServicio.Degradado,
                tiempoRespuesta: 180,
                ultimaVerificacion: new Date().toISOString(),
                version: '1.0.2',
                uptime: 345600, // 4 días
                memoriaUsada: 450,
                cpu: 45,
                requestsPorMinuto: 75
            },
            {
                nombre: 'Usuarios',
                descripcion: 'Gestión de usuarios y perfiles',
                url: 'http://localhost:5023',
                puerto: 5023,
                estado: EstadoServicio.Saludable,
                tiempoRespuesta: 35,
                ultimaVerificacion: new Date().toISOString(),
                version: '1.1.3',
                uptime: 518400, // 6 días
                memoriaUsada: 280,
                cpu: 10,
                requestsPorMinuto: 50
            },
            {
                nombre: 'Pagos',
                descripcion: 'Procesamiento de pagos',
                url: 'http://localhost:5007',
                puerto: 5007,
                estado: EstadoServicio.Saludable,
                tiempoRespuesta: 65,
                ultimaVerificacion: new Date().toISOString(),
                version: '1.0.8',
                uptime: 604800, // 7 días
                memoriaUsada: 410,
                cpu: 22,
                requestsPorMinuto: 110
            },
            {
                nombre: 'Notificaciones',
                descripcion: 'Sistema de notificaciones en tiempo real',
                url: 'http://localhost:5006',
                puerto: 5006,
                estado: EstadoServicio.Saludable,
                tiempoRespuesta: 28,
                ultimaVerificacion: new Date().toISOString(),
                version: '1.0.4',
                uptime: 691200, // 8 días
                memoriaUsada: 195,
                cpu: 8,
                requestsPorMinuto: 200
            },
            {
                nombre: 'Servicios',
                descripcion: 'Servicios complementarios',
                url: 'http://localhost:5008',
                puerto: 5008,
                estado: EstadoServicio.Saludable,
                tiempoRespuesta: 48,
                ultimaVerificacion: new Date().toISOString(),
                version: '1.0.1',
                uptime: 777600, // 9 días
                memoriaUsada: 240,
                cpu: 14,
                requestsPorMinuto: 40
            },
            {
                nombre: 'Streaming',
                descripcion: 'Gestión de streaming de eventos',
                url: 'http://localhost:5009',
                puerto: 5009,
                estado: EstadoServicio.Saludable,
                tiempoRespuesta: 72,
                ultimaVerificacion: new Date().toISOString(),
                version: '1.1.0',
                uptime: 864000, // 10 días
                memoriaUsada: 620,
                cpu: 35,
                requestsPorMinuto: 30
            },
            {
                nombre: 'Reportes',
                descripcion: 'Generación de reportes y analítica',
                url: 'http://localhost:5010',
                puerto: 5010,
                estado: EstadoServicio.Saludable,
                tiempoRespuesta: 95,
                ultimaVerificacion: new Date().toISOString(),
                version: '1.0.6',
                uptime: 950400, // 11 días
                memoriaUsada: 480,
                cpu: 28,
                requestsPorMinuto: 25
            },
            {
                nombre: 'Recomendaciones',
                descripcion: 'Motor de recomendaciones',
                url: 'http://localhost:5011',
                puerto: 5011,
                estado: EstadoServicio.Saludable,
                tiempoRespuesta: 88,
                ultimaVerificacion: new Date().toISOString(),
                version: '1.0.3',
                uptime: 1036800, // 12 días
                memoriaUsada: 350,
                cpu: 20,
                requestsPorMinuto: 35
            }
        ];
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
