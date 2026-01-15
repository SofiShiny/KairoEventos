export enum TipoCorreo {
    Confirmacion = 'confirmacion',
    Recordatorio = 'recordatorio',
    Cancelacion = 'cancelacion',
    Reembolso = 'reembolso',
    Bienvenida = 'bienvenida',
    Promocion = 'promocion'
}

export enum EstadoCorreo {
    Enviado = 'enviado',
    Entregado = 'entregado',
    Fallido = 'fallido',
    Pendiente = 'pendiente'
}

export interface Correo {
    id: string;
    usuarioId: string;
    destinatario: string;
    asunto: string;
    tipo: TipoCorreo;
    estado: EstadoCorreo;
    fechaEnvio: string;
    fechaEntrega?: string;
    contenido: string;
    eventoRelacionado?: string;
    ordenRelacionada?: string;
}

export const correosService = {
    // Simula historial de correos basado en las entradas del usuario
    generarHistorialCorreos: (entradas: any[], email: string): Correo[] => {
        const correos: Correo[] = [];

        entradas.forEach((entrada) => {
            // Correo de confirmación de compra
            correos.push({
                id: `conf-${entrada.id}`,
                usuarioId: entrada.usuarioId,
                destinatario: email,
                asunto: `Confirmación de compra - ${entrada.eventoNombre}`,
                tipo: TipoCorreo.Confirmacion,
                estado: EstadoCorreo.Entregado,
                fechaEnvio: entrada.fechaCompra,
                fechaEntrega: entrada.fechaCompra,
                contenido: `Tu entrada para ${entrada.eventoNombre} ha sido confirmada. Asiento: ${entrada.asientoInfo}`,
                eventoRelacionado: entrada.eventoNombre,
                ordenRelacionada: entrada.id
            });

            // Correo de recordatorio (si el evento es futuro)
            const fechaEvento = entrada.fechaEvento ? new Date(entrada.fechaEvento) : null;
            const ahora = new Date();

            if (fechaEvento && fechaEvento > ahora && entrada.estado === 'Pagada') {
                const fechaRecordatorio = new Date(fechaEvento);
                fechaRecordatorio.setDate(fechaRecordatorio.getDate() - 1);

                correos.push({
                    id: `rec-${entrada.id}`,
                    usuarioId: entrada.usuarioId,
                    destinatario: email,
                    asunto: `Recordatorio: ${entrada.eventoNombre} es mañana`,
                    tipo: TipoCorreo.Recordatorio,
                    estado: fechaRecordatorio < ahora ? EstadoCorreo.Entregado : EstadoCorreo.Pendiente,
                    fechaEnvio: fechaRecordatorio.toISOString(),
                    fechaEntrega: fechaRecordatorio < ahora ? fechaRecordatorio.toISOString() : undefined,
                    contenido: `No olvides tu evento ${entrada.eventoNombre}. Presenta tu código QR: ${entrada.codigoQr}`,
                    eventoRelacionado: entrada.eventoNombre,
                    ordenRelacionada: entrada.id
                });
            }

            // Correo de cancelación
            if (entrada.estado === 'Cancelada') {
                correos.push({
                    id: `canc-${entrada.id}`,
                    usuarioId: entrada.usuarioId,
                    destinatario: email,
                    asunto: `Cancelación confirmada - ${entrada.eventoNombre}`,
                    tipo: TipoCorreo.Cancelacion,
                    estado: EstadoCorreo.Entregado,
                    fechaEnvio: entrada.fechaCompra,
                    fechaEntrega: entrada.fechaCompra,
                    contenido: `Tu entrada para ${entrada.eventoNombre} ha sido cancelada.`,
                    eventoRelacionado: entrada.eventoNombre,
                    ordenRelacionada: entrada.id
                });

                // Correo de reembolso
                correos.push({
                    id: `reem-${entrada.id}`,
                    usuarioId: entrada.usuarioId,
                    destinatario: email,
                    asunto: `Reembolso procesado - $${entrada.precio}`,
                    tipo: TipoCorreo.Reembolso,
                    estado: EstadoCorreo.Entregado,
                    fechaEnvio: entrada.fechaCompra,
                    fechaEntrega: entrada.fechaCompra,
                    contenido: `Se ha procesado el reembolso de $${entrada.precio} por la cancelación de ${entrada.eventoNombre}.`,
                    eventoRelacionado: entrada.eventoNombre,
                    ordenRelacionada: entrada.id
                });
            }
        });

        // Correo de bienvenida (simulado)
        if (correos.length > 0) {
            const primeraCompra = correos.reduce((min, c) =>
                new Date(c.fechaEnvio) < new Date(min.fechaEnvio) ? c : min
            );

            correos.push({
                id: 'bienvenida-001',
                usuarioId: primeraCompra.usuarioId,
                destinatario: email,
                asunto: '¡Bienvenido a Kairo Events!',
                tipo: TipoCorreo.Bienvenida,
                estado: EstadoCorreo.Entregado,
                fechaEnvio: primeraCompra.fechaEnvio,
                fechaEntrega: primeraCompra.fechaEnvio,
                contenido: 'Gracias por unirte a Kairo Events. Descubre eventos increíbles y vive experiencias únicas.',
                eventoRelacionado: undefined,
                ordenRelacionada: undefined
            });
        }

        // Ordenar por fecha descendente
        return correos.sort((a, b) =>
            new Date(b.fechaEnvio).getTime() - new Date(a.fechaEnvio).getTime()
        );
    }
};
