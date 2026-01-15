import api from '@/lib/axios';

export enum EstadoTransaccion {
    Procesando = 0,
    Aprobada = 1,
    Rechazada = 2,
    Reembolsada = 3
}

export interface Transaccion {
    id: string;
    ordenId: string;
    usuarioId: string;
    monto: number;
    tarjetaMascara: string;
    estado: EstadoTransaccion;
    fechaCreacion: string;
    fechaActualizacion?: string;
    mensajeError?: string;
}

export interface EstadisticasFinancieras {
    totalTransacciones: number;
    totalIngresos: number;
    transaccionesAprobadas: number;
    transaccionesRechazadas: number;
    transaccionesPendientes: number;
    transaccionesReembolsadas: number;
    montoAprobado: number;
    montoRechazado: number;
    montoPendiente: number;
    montoReembolsado: number;
    tasaAprobacion: number;
}

export interface PagoRequest {
    ordenId: string;
    usuarioId: string;
    tarjeta: string; // El backend espera 'tarjeta'
    titular: string;
    expiracion: string;
    cvv: string;
    monto: number;
    moneda: string;
    codigoCupon?: string;
}

export interface PagoResponse {
    exito: boolean;
    transaccionId: string;
    mensaje: string;
    estado: EstadoTransaccion;
}

export interface Cupon {
    id: string;
    codigo: string;
    porcentajeDescuento: number;
    tipo: string;
    estado: string;
    eventoId?: string;
    fechaCreacion: string;
    fechaExpiracion?: string;
}

export interface CrearCuponGeneralRequest {
    codigo: string;
    porcentajeDescuento: number;
    eventoId?: string;
    esGlobal: boolean;
    fechaExpiracion?: string;
    limiteUsos?: number;
}

export interface GenerarLoteRequest {
    cantidad: number;
    porcentajeDescuento: number;
    eventoId: string;
    fechaExpiracion?: string;
}

export interface ResultadoValidacionCupon {
    esValido: boolean;
    descuento: number;
    nuevoTotal: number;
    porcentajeDescuento: number;
    mensaje: string;
}

export const pagosService = {
    getTodasTransacciones: async (): Promise<Transaccion[]> => {
        const response = await api.get('/pagos');
        return response.data;
    },

    getTransaccion: async (id: string): Promise<Transaccion> => {
        const response = await api.get(`/pagos/${id}`);
        return response.data;
    },

    procesarPago: async (request: PagoRequest): Promise<PagoResponse> => {
        try {
            // Mapeamos a lo que el backend espera si es necesario
            const backendRequest = {
                ordenId: request.ordenId,
                usuarioId: request.usuarioId,
                monto: request.monto,
                tarjeta: request.tarjeta
            };

            const response = await api.post('/pagos', backendRequest);

            // El backend devuelve 202 Accepted con { transaccionId, estado: "Procesando" }
            return {
                exito: true,
                transaccionId: response.data.transaccionId,
                mensaje: 'Pago iniciado correctamente',
                estado: EstadoTransaccion.Procesando
            };
        } catch (error: any) {
            return {
                exito: false,
                transaccionId: '',
                mensaje: error.response?.data?.mensaje || 'Error al procesar el pago',
                estado: EstadoTransaccion.Rechazada
            };
        }
    },

    formatearNumeroTarjeta: (value: string): string => {
        const v = value.replace(/\s+/g, '').replace(/[^0-9]/gi, '');
        const matches = v.match(/\d{4,16}/g);
        const match = (matches && matches[0]) || '';
        const parts = [];

        for (let i = 0, len = match.length; i < len; i += 4) {
            parts.push(match.substring(i, i + 4));
        }

        if (parts.length) {
            return parts.join(' ');
        } else {
            return value;
        }
    },

    validarNumeroTarjeta: (numero: string): boolean => {
        const cleaned = numero.replace(/\s/g, '');
        return cleaned.length >= 13 && cleaned.length <= 16 && /^\d+$/.test(cleaned);
    },

    validarExpiracion: (expiracion: string): boolean => {
        if (!/^\d{2}\/\d{2}$/.test(expiracion)) return false;

        const [month, year] = expiracion.split('/').map(Number);
        const now = new Date();
        const currentYear = now.getFullYear() % 100;
        const currentMonth = now.getMonth() + 1;

        if (month < 1 || month > 12) return false;
        if (year < currentYear) return false;
        if (year === currentYear && month < currentMonth) return false;

        return true;
    },

    validarCVV: (cvv: string): boolean => {
        return /^\d{3,4}$/.test(cvv);
    },

    calcularEstadisticas: (transacciones: Transaccion[]): EstadisticasFinancieras => {
        const aprobadas = transacciones.filter(t => t.estado === EstadoTransaccion.Aprobada);
        const rechazadas = transacciones.filter(t => t.estado === EstadoTransaccion.Rechazada);
        const pendientes = transacciones.filter(t => t.estado === EstadoTransaccion.Procesando);
        const reembolsadas = transacciones.filter(t => t.estado === EstadoTransaccion.Reembolsada);

        const montoAprobado = aprobadas.reduce((sum, t) => sum + t.monto, 0);
        const montoRechazado = rechazadas.reduce((sum, t) => sum + t.monto, 0);
        const montoPendiente = pendientes.reduce((sum, t) => sum + t.monto, 0);
        const montoReembolsado = reembolsadas.reduce((sum, t) => sum + t.monto, 0);

        const totalIngresos = montoAprobado - montoReembolsado;
        const tasaAprobacion = transacciones.length > 0
            ? (aprobadas.length / transacciones.length) * 100
            : 0;

        return {
            totalTransacciones: transacciones.length,
            totalIngresos,
            transaccionesAprobadas: aprobadas.length,
            transaccionesRechazadas: rechazadas.length,
            transaccionesPendientes: pendientes.length,
            transaccionesReembolsadas: reembolsadas.length,
            montoAprobado,
            montoRechazado,
            montoPendiente,
            montoReembolsado,
            tasaAprobacion
        };
    },

    // Métodos de cupones
    getCuponesPorEvento: async (eventoId: string): Promise<Cupon[]> => {
        const response = await api.get(`/pagos/cupones/evento/${eventoId}`);
        return response.data;
    },

    crearCuponGeneral: async (request: CrearCuponGeneralRequest): Promise<Cupon> => {
        const response = await api.post('/pagos/cupones/general', request);
        return response.data;
    },

    generarLoteCupones: async (request: GenerarLoteRequest): Promise<Cupon[]> => {
        const response = await api.post('/pagos/cupones/lote', request);
        return response.data;
    },

    validarCupon: async (codigo: string, eventoId: string | null, montoTotal: number): Promise<ResultadoValidacionCupon> => {
        const response = await api.post('/pagos/cupones/validar', {
            codigo,
            eventoId,
            montoTotal
        });
        return response.data;
    }
};
