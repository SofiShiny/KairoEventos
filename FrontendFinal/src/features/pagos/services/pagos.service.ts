import api from '@/lib/axios';

export interface PagoRequest {
    ordenId: string;
    usuarioId: string;
    tarjetaNumero: string;
    titular: string;
    expiracion: string;
    cvv: string;
    monto: number;
    moneda: string;
    codigoCupon?: string; // Nuevo campo para cupones
}

export interface PagoResponse {
    exito: boolean;
    mensaje: string;
    transaccionId: string;
    estado?: string;
}

export interface ValidarCuponResponse {
    esValido: boolean;
    descuento: number;
    nuevoTotal: number;
    mensaje: string;
    porcentajeDescuento?: number;
}

export interface CrearCuponGeneralRequest {
    codigo: string;
    porcentajeDescuento: number; // Porcentaje 0-100
    fechaExpiracion?: string;
    eventoId?: string;
    esGlobal?: boolean;
    limiteUsos?: number;
}

export interface GenerarLoteCuponesRequest {
    cantidad: number;
    porcentajeDescuento: number; // Porcentaje 0-100
    eventoId: string;
    fechaExpiracion?: string;
}

export interface Cupon {
    id: string;
    codigo: string;
    porcentajeDescuento: number; // Porcentaje 0-100
    tipo: 'General' | 'Unico';
    estado: 'Activo' | 'Usado' | 'Expirado' | 'Agotado';
    fechaExpiracion?: string;
    eventoId?: string;
    usosRestantes?: number;
    fechaCreacion: string;
}

class PagosService {
    async procesarPago(datos: PagoRequest): Promise<PagoResponse> {
        try {
            // Llamada al microservicio de Pagos a través del Gateway
            const response = await api.post('/pagos', {
                ordenId: datos.ordenId,
                usuarioId: datos.usuarioId,
                monto: datos.monto,
                tarjeta: datos.tarjetaNumero.replace(/\s/g, ''), // Enviar sin espacios
                codigoCupon: datos.codigoCupon // Incluir cupón si existe
            });

            // El backend retorna 202 Accepted con { transaccionId, estado: "Procesando" }
            return {
                exito: true,
                mensaje: 'Pago procesado exitosamente',
                transaccionId: response.data.transaccionId,
                estado: response.data.estado
            };
        } catch (error: any) {
            console.error('Error al procesar pago:', error);

            if (error.response?.data?.mensaje) {
                return {
                    exito: false,
                    mensaje: error.response.data.mensaje,
                    transaccionId: ''
                };
            }

            if (error.response?.status === 400) {
                return {
                    exito: false,
                    mensaje: 'Datos de pago inválidos. Por favor verifica la información.',
                    transaccionId: ''
                };
            }

            return {
                exito: false,
                mensaje: 'Error al procesar el pago. Por favor intenta de nuevo.',
                transaccionId: ''
            };
        }
    }

    async consultarEstadoPago(transaccionId: string): Promise<any> {
        try {
            const response = await api.get(`/pagos/${transaccionId}`);
            return response.data;
        } catch (error) {
            console.error('Error al consultar estado de pago:', error);
            throw error;
        }
    }

    // ==================== MÉTODOS DE CUPONES ====================

    /**
     * Valida un cupón y calcula el descuento aplicable
     */
    async validarCupon(
        codigo: string,
        eventoId: string,
        montoTotal: number
    ): Promise<ValidarCuponResponse> {
        try {
            const response = await api.post('/pagos/cupones/validar', {
                codigo,
                eventoId,
                montoTotal
            });

            return {
                esValido: true,
                descuento: response.data.descuento,
                nuevoTotal: response.data.nuevoTotal,
                mensaje: response.data.mensaje || 'Cupón aplicado exitosamente',
                porcentajeDescuento: response.data.porcentajeDescuento
            };
        } catch (error: any) {
            console.error('Error al validar cupón:', error);

            return {
                esValido: false,
                descuento: 0,
                nuevoTotal: montoTotal,
                mensaje: error.response?.data?.mensaje || 'Cupón inválido o expirado'
            };
        }
    }

    /**
     * Crea un cupón general (un código usado por muchos usuarios)
     */
    async crearCuponGeneral(data: CrearCuponGeneralRequest): Promise<Cupon> {
        try {
            const response = await api.post('/pagos/cupones/general', {
                codigo: data.codigo.toUpperCase(),
                porcentajeDescuento: data.porcentajeDescuento,
                fechaExpiracion: data.fechaExpiracion,
                eventoId: data.eventoId,
                esGlobal: data.esGlobal || false,
                limiteUsos: data.limiteUsos
            });

            return response.data;
        } catch (error: any) {
            console.error('Error al crear cupón general:', error);
            throw new Error(error.response?.data?.mensaje || 'Error al crear el cupón');
        }
    }

    /**
     * Genera un lote de cupones únicos (códigos aleatorios de un solo uso)
     */
    async generarLoteCupones(data: GenerarLoteCuponesRequest): Promise<Cupon[]> {
        try {
            const response = await api.post('/pagos/cupones/lote', {
                cantidad: data.cantidad,
                porcentajeDescuento: data.porcentajeDescuento,
                eventoId: data.eventoId,
                fechaExpiracion: data.fechaExpiracion
            });

            return response.data;
        } catch (error: any) {
            console.error('Error al generar lote de cupones:', error);
            throw new Error(error.response?.data?.mensaje || 'Error al generar los cupones');
        }
    }

    /**
     * Obtiene todos los cupones de un evento específico
     */
    async getCuponesPorEvento(eventoId: string): Promise<Cupon[]> {
        try {
            const response = await api.get(`/pagos/cupones/evento/${eventoId}`);
            return response.data;
        } catch (error: any) {
            console.error('Error al obtener cupones:', error);
            throw new Error(error.response?.data?.mensaje || 'Error al cargar los cupones');
        }
    }

    /**
     * Obtiene todos los cupones globales (no asociados a un evento específico)
     */
    async getCuponesGlobales(): Promise<Cupon[]> {
        try {
            const response = await api.get('/pagos/cupones/globales');
            return response.data;
        } catch (error: any) {
            console.error('Error al obtener cupones globales:', error);
            throw new Error(error.response?.data?.mensaje || 'Error al cargar los cupones');
        }
    }

    // ==================== VALIDACIONES DEL LADO DEL CLIENTE ====================

    validarNumeroTarjeta(numero: string): boolean {
        // Algoritmo de Luhn simplificado
        const cleaned = numero.replace(/\s/g, '');
        return cleaned.length === 16 && /^\d+$/.test(cleaned);
    }

    validarExpiracion(expiracion: string): boolean {
        const match = expiracion.match(/^(0[1-9]|1[0-2])\/(\d{2})$/);
        if (!match) return false;

        const mes = parseInt(match[1]);
        const año = parseInt('20' + match[2]);
        const ahora = new Date();
        const fechaExpiracion = new Date(año, mes - 1);

        return fechaExpiracion > ahora;
    }

    validarCVV(cvv: string): boolean {
        return /^\d{3,4}$/.test(cvv);
    }

    formatearNumeroTarjeta(numero: string): string {
        const cleaned = numero.replace(/\s/g, '');
        const chunks = cleaned.match(/.{1,4}/g) || [];
        return chunks.join(' ');
    }
}

export const pagosService = new PagosService();
