export enum NivelLog {
    Debug = 'debug',
    Info = 'info',
    Warning = 'warning',
    Error = 'error',
    Critical = 'critical'
}

export interface LogEntry {
    id: string;
    timestamp: string;
    nivel: NivelLog;
    servicio: string;
    mensaje: string;
    detalles?: string;
    usuario?: string;
    ip?: string;
    duracion?: number;
    stackTrace?: string;
}

export interface FiltrosLog {
    servicio?: string;
    nivel?: NivelLog;
    busqueda?: string;
    desde?: string;
    hasta?: string;
}

const servicios = [
    'Gateway', 'Eventos', 'Entradas', 'Asientos', 'Reservas',
    'Usuarios', 'Pagos', 'Notificaciones', 'Servicios', 'Streaming',
    'Reportes', 'Recomendaciones'
];

const mensajesEjemplo = {
    [NivelLog.Debug]: [
        'Iniciando procesamiento de solicitud',
        'Cache hit para clave: {key}',
        'Validando parámetros de entrada',
        'Conectando a base de datos',
        'Ejecutando query: SELECT * FROM...'
    ],
    [NivelLog.Info]: [
        'Solicitud procesada exitosamente',
        'Usuario autenticado correctamente',
        'Evento publicado: {eventId}',
        'Entrada vendida: {ticketId}',
        'Pago procesado: {paymentId}',
        'Email enviado a: {email}',
        'Reserva creada: {reservationId}'
    ],
    [NivelLog.Warning]: [
        'Tiempo de respuesta elevado: {duration}ms',
        'Cache miss para clave: {key}',
        'Reintentando conexión a servicio externo',
        'Límite de rate limit alcanzado',
        'Sesión próxima a expirar'
    ],
    [NivelLog.Error]: [
        'Error al procesar pago: {error}',
        'Fallo en conexión a base de datos',
        'Timeout en llamada a servicio externo',
        'Validación fallida: {validation}',
        'Error al enviar email: {error}'
    ],
    [NivelLog.Critical]: [
        'Servicio no disponible',
        'Fallo crítico en base de datos',
        'Memoria insuficiente',
        'Disco lleno',
        'Fallo en sistema de pagos'
    ]
};

export const logsService = {
    // Genera logs de ejemplo
    generarLogs: (cantidad: number = 100): LogEntry[] => {
        const logs: LogEntry[] = [];
        const ahora = new Date();

        for (let i = 0; i < cantidad; i++) {
            const timestamp = new Date(ahora.getTime() - (cantidad - i) * 1000 * Math.random() * 60);
            const nivel = Object.values(NivelLog)[Math.floor(Math.random() * 5)];
            const servicio = servicios[Math.floor(Math.random() * servicios.length)];
            const mensajes = mensajesEjemplo[nivel];
            const mensaje = mensajes[Math.floor(Math.random() * mensajes.length)];

            const log: LogEntry = {
                id: `log-${i}-${Date.now()}`,
                timestamp: timestamp.toISOString(),
                nivel,
                servicio,
                mensaje,
                usuario: Math.random() > 0.5 ? `user-${Math.floor(Math.random() * 100)}` : undefined,
                ip: `192.168.1.${Math.floor(Math.random() * 255)}`,
                duracion: Math.random() > 0.7 ? Math.floor(Math.random() * 1000) : undefined
            };

            // Agregar stack trace para errores
            if (nivel === NivelLog.Error || nivel === NivelLog.Critical) {
                log.stackTrace = `at ${servicio}.Controller.HandleRequest()\nat Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor\nat System.Threading.Tasks.Task.Execute()`;
                log.detalles = `Exception: ${mensaje}\nInner Exception: Connection timeout`;
            }

            logs.push(log);
        }

        return logs.sort((a, b) => new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime());
    },

    filtrarLogs: (logs: LogEntry[], filtros: FiltrosLog): LogEntry[] => {
        return logs.filter(log => {
            if (filtros.servicio && log.servicio !== filtros.servicio) {
                return false;
            }

            if (filtros.nivel && log.nivel !== filtros.nivel) {
                return false;
            }

            if (filtros.busqueda) {
                const busqueda = filtros.busqueda.toLowerCase();
                const coincide =
                    log.mensaje.toLowerCase().includes(busqueda) ||
                    log.servicio.toLowerCase().includes(busqueda) ||
                    (log.detalles && log.detalles.toLowerCase().includes(busqueda));

                if (!coincide) {
                    return false;
                }
            }

            if (filtros.desde) {
                const desde = new Date(filtros.desde);
                if (new Date(log.timestamp) < desde) {
                    return false;
                }
            }

            if (filtros.hasta) {
                const hasta = new Date(filtros.hasta);
                if (new Date(log.timestamp) > hasta) {
                    return false;
                }
            }

            return true;
        });
    },

    formatTimestamp: (timestamp: string): string => {
        const date = new Date(timestamp);
        return date.toLocaleString('es-ES', {
            year: 'numeric',
            month: '2-digit',
            day: '2-digit',
            hour: '2-digit',
            minute: '2-digit',
            second: '2-digit'
        });
    },

    exportarLogs: (logs: LogEntry[]): string => {
        const lineas = logs.map(log => {
            return `[${log.timestamp}] [${log.nivel.toUpperCase()}] [${log.servicio}] ${log.mensaje}`;
        });
        return lineas.join('\n');
    }
};
