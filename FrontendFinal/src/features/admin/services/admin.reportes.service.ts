import api from '@/lib/axios';

export interface DashboardMetrics {
    ventasSemana: Array<{
        fecha: string;
        totalVentas: number;
        entradasVendidas: number;
    }>;
    acumulado: {
        totalVentas: number;
        totalEntradas: number;
    };
    ocupacion: Array<{
        nombre: string;
        vendidas: number;
        disponibles: number;
    }>;
}

export const adminReportesService = {
    /**
     * Obtiene las métricas consolidadas para el dashboard administrativo
     */
    getDashboardMetrics: async (): Promise<DashboardMetrics> => {
        const response = await api.get<DashboardMetrics>('/reportes/dashboard');
        return response.data;
    }
};
