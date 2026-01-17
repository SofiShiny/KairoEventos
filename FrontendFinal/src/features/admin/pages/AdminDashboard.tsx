import React, { useEffect, useState } from 'react';
import {
    ArrowUpRight,
} from 'lucide-react';
import { useAuth } from 'react-oidc-context';
import { adminReportesService, DashboardMetrics } from '../services/admin.reportes.service';
import { StatCard } from '../components/dashboard/StatCard';
import { VentasChart } from '../components/dashboard/VentasChart';
import { OcupacionPieChart } from '../components/dashboard/OcupacionPieChart';
import { useReportesSignalR, MetricasDashboard as SignalRMetrics } from '../../../hooks/useReportesSignalR';
import { toast } from 'react-hot-toast';
import { useT } from '../../../i18n';

export default function AdminDashboard() {
    const auth = useAuth();
    const t = useT();
    const [metrics, setMetrics] = useState<DashboardMetrics | null>(null);
    const [loading, setLoading] = useState(true);

    const onMetricasReceived = React.useCallback((newMetrics: SignalRMetrics) => {
        setMetrics(prev => {
            if (!prev) return null; // Si aún no ha cargado la data inicial, esperamos
            return {
                ...prev,
                acumulado: {
                    ...prev.acumulado,
                    totalVentas: newMetrics.totalVentasDia, // Mapping simple para demo
                    totalEntradas: newMetrics.entradasVendidas
                    // totalEventos: newMetrics.eventosActivos
                }
            };
        });

        toast.success(t.dashboard.description ? "Métricas actualizadas en tiempo real" : "Metrics updated", {
            id: 'dashboard-update', // Evitar duplicados
            icon: '⚡',
            style: {
                background: '#0f172a',
                color: '#fff',
                border: '1px solid #3b82f6'
            },
            duration: 2000
        });
    }, [t]);

    const { isConnected } = useReportesSignalR({ onMetricasReceived });

    useEffect(() => {
        // ...
        const fetchMetrics = async () => {
            try {
                const data = await adminReportesService.getDashboardMetrics();
                setMetrics(data);
            } catch (error) {
                console.error("Error fetching dashboard metrics:", error);
            } finally {
                setLoading(false);
            }
        };

        fetchMetrics();
    }, []);

    const rawRoles = (auth.user?.profile as any)?.realm_access?.roles || [];
    const userRoles = rawRoles.map((r: string) => r.toLowerCase());
    const isAdmin = userRoles.includes('admin');
    const isOrganizador = userRoles.includes('organizador') || userRoles.includes('organizator');

    const formatCurrency = (val: number) => {
        return new Intl.NumberFormat(document.documentElement.lang === 'es' ? 'es-ES' : 'en-US', {
            style: 'currency',
            currency: 'USD',
            minimumFractionDigits: 0
        }).format(val);
    };

    if (loading) {
        return (
            <div className="flex flex-col items-center justify-center h-96 space-y-4">
                <div className="relative w-16 h-16">
                    <div className="absolute inset-0 rounded-full border-4 border-purple-500/20"></div>
                    <div className="absolute inset-0 rounded-full border-4 border-t-purple-500 animate-spin"></div>
                </div>
                <p className="text-slate-400 font-medium animate-pulse">{t.common.loading}...</p>
            </div>
        );
    }

    return (
        <div className="space-y-8 animate-in fade-in slide-in-from-bottom-4 duration-500 pb-10">
            {/* Welcome Header */}
            <div className="flex flex-col md:flex-row md:items-end justify-between gap-4">
                <div>
                    <h1 className="text-4xl font-black text-white tracking-tight">{t.dashboard.analysis}</h1>
                    <p className="text-slate-400 mt-2 text-lg">{t.dashboard.description}</p>
                </div>

                <div className="flex items-center gap-3">
                    <div className={`px-3 py-1 rounded-full text-xs font-bold border flex items-center gap-2 ${isConnected ? 'bg-green-500/10 border-green-500/30 text-green-400' : 'bg-red-500/10 border-red-500/30 text-red-400'}`}>
                        <span className={`w-2 h-2 rounded-full ${isConnected ? 'bg-green-500 animate-pulse' : 'bg-red-500'}`}></span>
                        {isConnected ? 'En Vivo' : 'Desconectado'}
                    </div>
                    <div className="px-4 py-2 bg-[#16191f] border border-slate-800 rounded-xl text-xs font-bold text-slate-300">
                        {t.dashboard.last7Days}
                    </div>
                </div>
            </div>

            {/* KPI Grid */}
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
                <div className="contents">
                    <StatCard
                        title={t.dashboard.totalSales}
                        value={formatCurrency(metrics?.acumulado.totalVentas || 0)}
                        icon="💰"
                        trend={{ value: '12.5%', positive: true }}
                    />
                    <StatCard
                        title={t.dashboard.ticketsSold}
                        value={metrics?.acumulado.totalEntradas.toLocaleString(document.documentElement.lang === 'es' ? 'es-ES' : 'en-US') || '0'}
                        icon="🎫"
                        trend={{ value: '4.2%', positive: true }}
                    />
                    <StatCard
                        title={t.dashboard.eventsWithData}
                        value={metrics?.ocupacion.length.toString() || '0'}
                        icon="📊"
                    />
                    <StatCard
                        title={t.dashboard.ticketingRate}
                        value={`${((metrics?.acumulado.totalVentas || 0) / (metrics?.acumulado.totalEntradas || 1)).toFixed(2)}`}
                        icon="📈"
                        description={t.dashboard.avgTicketDescription}
                    />
                </div>
            </div>

            {/* Charts & Analytics Section */}
            <div className="grid lg:grid-cols-3 gap-8">
                {/* Sales Chart */}
                <div className="lg:col-span-2 bg-[#16191f] border border-slate-800 rounded-2xl p-8 shadow-2xl relative overflow-hidden group">
                    <div className="absolute top-0 right-0 w-64 h-64 bg-purple-500/5 blur-[100px] -mr-32 -mt-32"></div>
                    <div className="flex justify-between items-center mb-6 relative z-10">
                        <div>
                            <h3 className="text-xl font-bold text-white">{t.dashboard.revenueEvolution}</h3>
                            <p className="text-sm text-slate-500">{t.dashboard.dailySalesVolume}</p>
                        </div>
                    </div>

                    <div className="relative z-10">
                        <VentasChart data={metrics?.ventasSemana || []} />
                    </div>
                </div>

                {/* Occupancy Section */}
                <div className="bg-[#16191f] border border-slate-800 rounded-2xl p-8 shadow-2xl relative overflow-hidden">
                    <div className="absolute top-0 right-0 w-48 h-48 bg-pink-500/5 blur-[80px] -mr-24 -mt-24"></div>
                    <h3 className="text-xl font-bold text-white mb-1">{t.dashboard.occupancy}</h3>
                    <p className="text-sm text-slate-500 mb-8">{t.dashboard.inventoryStatus}</p>

                    <div className="relative z-10">
                        {(() => {
                            // Filtramos datos de ejemplo o basura conocida
                            const validOcupacion = metrics?.ocupacion.filter(e => e.nombre !== 'Concierto Demo') || [];

                            if (validOcupacion.length === 0) {
                                return (
                                    <div className="flex flex-col items-center justify-center h-[300px] text-slate-500">
                                        <p>No hay datos de ocupación disponibles.</p>
                                        <p className="text-xs mt-2 opacity-50">Los datos se generarán con las nuevas ventas.</p>
                                    </div>
                                );
                            }

                            return <OcupacionPieChart data={validOcupacion} />;
                        })()}
                    </div>

                    <div className="mt-8 space-y-4 relative z-10">
                        {(() => {
                            const validOcupacion = metrics?.ocupacion.filter(e => e.nombre !== 'Concierto Demo') || [];

                            return validOcupacion.slice(0, 3).map((evento, idx) => (
                                <div key={idx} className="flex flex-col gap-2">
                                    <div className="flex justify-between text-xs font-bold text-slate-400 uppercase tracking-widest">
                                        <span>{evento.nombre}</span>
                                        <span>{Math.round((evento.vendidas / (evento.vendidas + evento.disponibles || 1)) * 100)}%</span>
                                    </div>
                                    <div className="w-full h-1.5 bg-slate-800 rounded-full overflow-hidden">
                                        <div
                                            className="h-full bg-pink-500 rounded-full"
                                            style={{ width: `${(evento.vendidas / (evento.vendidas + evento.disponibles || 1)) * 100}%` }}
                                        ></div>
                                    </div>
                                </div>
                            ));
                        })()}
                    </div>
                </div>
            </div>

            {!isAdmin && isOrganizador && (
                <div className="mt-8 flex items-center gap-4 p-4 bg-blue-500/5 border border-blue-500/20 rounded-2xl">
                    <div className="w-10 h-10 bg-blue-500/20 rounded-full flex items-center justify-center text-blue-400">
                        <ArrowUpRight className="w-5 h-5" />
                    </div>
                    <div>
                        <p className="text-sm font-bold text-blue-400">{t.dashboard.organizerMode}</p>
                        <p className="text-xs text-blue-400/70 text-slate-400">{t.dashboard.organizerMetricsDescription}</p>
                    </div>
                </div>
            )}
        </div>
    );
}
