import { Entrada } from '../../entradas/services/entradas.service';
import { Transaccion } from '../../pagos/services/pagos.service';

export interface ReporteVentas {
    // Métricas generales
    totalVentas: number;
    totalIngresos: number;
    totalEntradas: number;
    ticketPromedio: number;

    // Por período
    ventasHoy: number;
    ventasSemana: number;
    ventasMes: number;

    // Por estado
    entradasPagadas: number;
    entradasPendientes: number;
    entradasCanceladas: number;
    entradasUsadas: number;

    // Top eventos
    topEventos: EventoVentas[];

    // Ventas por día (últimos 30 días)
    ventasPorDia: VentaDiaria[];

    // Ventas por hora (hoy)
    ventasPorHora: VentaHoraria[];
}

export interface EventoVentas {
    eventoNombre: string;
    totalVentas: number;
    totalIngresos: number;
    entradasVendidas: number;
}

export interface VentaDiaria {
    fecha: string;
    ventas: number;
    ingresos: number;
    entradas: number;
}

export interface VentaHoraria {
    hora: number;
    ventas: number;
    entradas: number;
}

export const reportesService = {
    generarReporteVentas: (entradas: Entrada[]): ReporteVentas => {
        const ahora = new Date();
        const hoy = new Date(ahora.getFullYear(), ahora.getMonth(), ahora.getDate());
        const hace7Dias = new Date(hoy);
        hace7Dias.setDate(hace7Dias.getDate() - 7);
        const hace30Dias = new Date(hoy);
        hace30Dias.setDate(hace30Dias.getDate() - 30);

        // Filtrar entradas pagadas
        const entradasPagadas = entradas.filter(e => e.estado === 'Pagada');

        // Calcular métricas generales
        const totalIngresos = entradasPagadas.reduce((sum, e) => sum + e.precio, 0);
        const totalEntradas = entradasPagadas.length;
        const ticketPromedio = totalEntradas > 0 ? totalIngresos / totalEntradas : 0;

        // Ventas por período
        const ventasHoy = entradasPagadas.filter(e =>
            new Date(e.fechaCompra) >= hoy
        ).reduce((sum, e) => sum + e.precio, 0);

        const ventasSemana = entradasPagadas.filter(e =>
            new Date(e.fechaCompra) >= hace7Dias
        ).reduce((sum, e) => sum + e.precio, 0);

        const ventasMes = entradasPagadas.filter(e =>
            new Date(e.fechaCompra) >= hace30Dias
        ).reduce((sum, e) => sum + e.precio, 0);

        // Por estado
        const entradasPorEstado = {
            pagadas: entradas.filter(e => e.estado === 'Pagada').length,
            pendientes: entradas.filter(e => e.estado === 'Pendiente').length,
            canceladas: entradas.filter(e => e.estado === 'Cancelada').length,
            usadas: entradas.filter(e => e.estado === 'Usada').length
        };

        // Top eventos
        const eventoMap = new Map<string, EventoVentas>();
        entradasPagadas.forEach(entrada => {
            const existing = eventoMap.get(entrada.eventoNombre) || {
                eventoNombre: entrada.eventoNombre,
                totalVentas: 0,
                totalIngresos: 0,
                entradasVendidas: 0
            };

            existing.totalVentas += 1;
            existing.totalIngresos += entrada.precio;
            existing.entradasVendidas += 1;

            eventoMap.set(entrada.eventoNombre, existing);
        });

        const topEventos = Array.from(eventoMap.values())
            .sort((a, b) => b.totalIngresos - a.totalIngresos)
            .slice(0, 5);

        // Ventas por día (últimos 30 días)
        const ventasPorDiaMap = new Map<string, VentaDiaria>();

        // Inicializar todos los días
        for (let i = 0; i < 30; i++) {
            const fecha = new Date(hace30Dias);
            fecha.setDate(fecha.getDate() + i);
            const fechaStr = fecha.toISOString().split('T')[0];
            ventasPorDiaMap.set(fechaStr, {
                fecha: fechaStr,
                ventas: 0,
                ingresos: 0,
                entradas: 0
            });
        }

        // Llenar con datos reales
        entradasPagadas.forEach(entrada => {
            const fecha = new Date(entrada.fechaCompra);
            if (fecha >= hace30Dias) {
                const fechaStr = fecha.toISOString().split('T')[0];
                const dia = ventasPorDiaMap.get(fechaStr);
                if (dia) {
                    dia.ventas += 1;
                    dia.ingresos += entrada.precio;
                    dia.entradas += 1;
                }
            }
        });

        const ventasPorDia = Array.from(ventasPorDiaMap.values())
            .sort((a, b) => a.fecha.localeCompare(b.fecha));

        // Ventas por hora (hoy)
        const ventasPorHoraMap = new Map<number, VentaHoraria>();

        // Inicializar todas las horas
        for (let i = 0; i < 24; i++) {
            ventasPorHoraMap.set(i, {
                hora: i,
                ventas: 0,
                entradas: 0
            });
        }

        // Llenar con datos de hoy
        entradasPagadas.forEach(entrada => {
            const fecha = new Date(entrada.fechaCompra);
            if (fecha >= hoy) {
                const hora = fecha.getHours();
                const horaria = ventasPorHoraMap.get(hora);
                if (horaria) {
                    horaria.ventas += entrada.precio;
                    horaria.entradas += 1;
                }
            }
        });

        const ventasPorHora = Array.from(ventasPorHoraMap.values());

        return {
            totalVentas: totalEntradas,
            totalIngresos,
            totalEntradas,
            ticketPromedio,
            ventasHoy,
            ventasSemana,
            ventasMes,
            entradasPagadas: entradasPorEstado.pagadas,
            entradasPendientes: entradasPorEstado.pendientes,
            entradasCanceladas: entradasPorEstado.canceladas,
            entradasUsadas: entradasPorEstado.usadas,
            topEventos,
            ventasPorDia,
            ventasPorHora
        };
    }
};
