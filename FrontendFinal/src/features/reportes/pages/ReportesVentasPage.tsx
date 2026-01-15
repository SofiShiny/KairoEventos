import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import {
    BarChart3,
    ChevronLeft,
    Sparkles,
    Loader2,
    TrendingUp,
    DollarSign,
    Ticket,
    Calendar,
    Clock,
    Award,
    Download,
    RefreshCcw
} from 'lucide-react';
import { entradasService } from '../../entradas/services/entradas.service';
import { reportesService, ReporteVentas } from '../services/reportes.service';
import { toast } from 'react-hot-toast';
import { useT } from '../../../i18n';

export const ReportesVentasPage = () => {
    const navigate = useNavigate();
    const t = useT();

    const [loading, setLoading] = useState(true);
    const [reporte, setReporte] = useState<ReporteVentas | null>(null);

    useEffect(() => {
        loadReporte();
    }, []);

    const loadReporte = async () => {
        try {
            setLoading(true);
            const entradas = await entradasService.getTodasLasEntradas();
            const reporteGenerado = reportesService.generarReporteVentas(entradas);
            setReporte(reporteGenerado);
        } catch (error) {
            console.error('Error cargando reporte:', error);
            toast.error(t.messages.serverError);
        } finally {
            setLoading(false);
        }
    };

    const formatMonto = (monto: number) => {
        return new Intl.NumberFormat(document.documentElement.lang === 'es' ? 'es-ES' : 'en-US', {
            style: 'currency',
            currency: 'USD'
        }).format(monto);
    };

    const formatFecha = (fecha: string) => {
        const date = new Date(fecha);
        return date.toLocaleDateString('es-ES', {
            day: '2-digit',
            month: 'short'
        });
    };

    if (loading) {
        return (
            <div className="min-h-screen bg-black flex flex-col items-center justify-center">
                <Loader2 className="w-12 h-12 text-blue-500 animate-spin mb-4" />
                <p className="text-neutral-500 font-bold uppercase tracking-widest text-xs">{t.common.loading}...</p>
            </div>
        );
    }

    if (!reporte) return null;

    const maxVentasDia = Math.max(...reporte.ventasPorDia.map(v => v.ingresos), 1);
    const maxVentasHora = Math.max(...reporte.ventasPorHora.map(v => v.ventas), 1);

    return (
        <div className="min-h-screen bg-black text-white p-8">
            <div className="max-w-7xl mx-auto">
                <button
                    onClick={() => navigate('/admin')}
                    className="flex items-center gap-2 text-neutral-500 hover:text-white transition-colors mb-12 group"
                >
                    <ChevronLeft className="w-5 h-5 group-hover:-translate-x-1 transition-transform" />
                    <span className="font-bold uppercase tracking-widest text-xs">{t.common.back}</span>
                </button>

                <header className="mb-16 relative">
                    <div className="absolute -top-20 -left-20 w-64 h-64 bg-blue-600/10 blur-[120px] rounded-full pointer-events-none" />
                    <div className="flex items-center justify-between">
                        <div>
                            <div className="flex items-center gap-2 mb-4">
                                <Sparkles className="w-5 h-5 text-blue-500" />
                                <span className="text-blue-500 font-black text-xs uppercase tracking-[0.3em]">{t.reports.title}</span>
                            </div>
                            <h1 className="text-5xl font-black mb-4 tracking-tighter uppercase">
                                {t.reports.salesReport}
                            </h1>
                            <p className="text-neutral-500 text-lg font-medium max-w-2xl">
                                {t.dashboard.title}
                            </p>
                        </div>
                        <div className="flex gap-4">
                            <button
                                onClick={() => {
                                    toast.success(t.common.success);
                                }}
                                className="flex items-center gap-2 px-6 py-4 bg-neutral-900 border border-neutral-800 rounded-2xl text-neutral-400 hover:text-white hover:border-neutral-700 transition-all font-bold text-sm"
                            >
                                <Download className="w-5 h-5" />
                                {t.common.export}
                            </button>
                            <button
                                onClick={loadReporte}
                                className="p-4 bg-blue-500 border border-blue-400 rounded-2xl hover:bg-blue-600 transition-all group shadow-[0_0_20px_rgba(59,130,246,0.3)]"
                            >
                                <RefreshCcw className="w-6 h-6 text-white group-hover:rotate-180 transition-transform duration-500" />
                            </button>
                        </div>
                    </div>
                </header>

                {/* KPIs Principales */}
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-16">
                    <div className="bg-gradient-to-br from-blue-500/10 to-indigo-500/10 border border-blue-500/20 rounded-3xl p-6 relative overflow-hidden">
                        <div className="absolute top-0 right-0 w-32 h-32 bg-blue-500/5 rounded-full -translate-y-16 translate-x-16" />
                        <DollarSign className="w-10 h-10 text-blue-500 mb-4" />
                        <p className="text-4xl font-black text-white mb-2">
                            {formatMonto(reporte.totalIngresos)}
                        </p>
                        <p className="text-sm font-bold text-blue-400 uppercase tracking-widest flex items-center gap-2">
                            <TrendingUp className="w-4 h-4" />
                            {t.reports.revenue}
                        </p>
                    </div>

                    <div className="bg-neutral-900 border border-neutral-800 rounded-3xl p-6">
                        <Ticket className="w-10 h-10 text-emerald-500 mb-4" />
                        <p className="text-4xl font-black text-white mb-2">
                            {reporte.totalEntradas}
                        </p>
                        <p className="text-sm font-bold text-emerald-400 uppercase tracking-widest text-[10px]">
                            {t.reports.tickets}
                        </p>
                    </div>

                    <div className="bg-neutral-900 border border-neutral-800 rounded-3xl p-6">
                        <Award className="w-10 h-10 text-amber-500 mb-4" />
                        <p className="text-4xl font-black text-white mb-2">
                            {formatMonto(reporte.ticketPromedio)}
                        </p>
                        <p className="text-sm font-bold text-amber-400 uppercase tracking-widest text-[10px]">
                            {t.reports.averageTicket}
                        </p>
                    </div>

                    <div className="bg-neutral-900 border border-neutral-800 rounded-3xl p-6">
                        <Calendar className="w-10 h-10 text-rose-500 mb-4" />
                        <p className="text-4xl font-black text-white mb-2">
                            {formatMonto(reporte.ventasHoy)}
                        </p>
                        <p className="text-sm font-bold text-rose-400 uppercase tracking-widest text-[10px]">
                            {t.reports.today}
                        </p>
                    </div>
                </div>

                {/* Ventas por Período */}
                <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-16">
                    <div className="bg-blue-500/10 border border-blue-500/20 rounded-3xl p-6">
                        <div className="flex items-center justify-between mb-4">
                            <span className="text-sm font-bold text-blue-400 uppercase tracking-widest">{t.dates.today}</span>
                            <Clock className="w-5 h-5 text-blue-500" />
                        </div>
                        <p className="text-3xl font-black text-white">{formatMonto(reporte.ventasHoy)}</p>
                    </div>
                    <div className="bg-indigo-500/10 border border-indigo-500/20 rounded-3xl p-6">
                        <div className="flex items-center justify-between mb-4">
                            <span className="text-sm font-bold text-indigo-400 uppercase tracking-widest">{t.dates.thisWeek}</span>
                            <TrendingUp className="w-5 h-5 text-indigo-500" />
                        </div>
                        <p className="text-3xl font-black text-white">{formatMonto(reporte.ventasSemana)}</p>
                    </div>
                    <div className="bg-emerald-500/10 border border-emerald-500/20 rounded-3xl p-6">
                        <div className="flex items-center justify-between mb-4">
                            <span className="text-sm font-bold text-emerald-400 uppercase tracking-widest">{t.dates.thisMonth}</span>
                            <BarChart3 className="w-5 h-5 text-emerald-500" />
                        </div>
                        <p className="text-3xl font-black text-white">{formatMonto(reporte.ventasMes)}</p>
                    </div>
                </div>

                {/* Gráficos */}
                <div className="grid grid-cols-1 lg:grid-cols-2 gap-8 mb-16">
                    {/* Ventas por Día */}
                    <div className="bg-neutral-900 border border-neutral-800 rounded-3xl p-8">
                        <h3 className="text-xl font-black mb-8 flex items-center gap-3">
                            <TrendingUp className="text-blue-500" />
                            {t.reports.salesByDay}
                        </h3>
                        <div className="space-y-3">
                            {reporte.ventasPorDia.slice(-8).map((dia) => (
                                <div key={dia.fecha} className="group">
                                    <div className="flex justify-between items-center mb-1">
                                        <span className="text-xs font-bold text-neutral-500">{formatFecha(dia.fecha)}</span>
                                        <span className="text-xs font-black text-white">{formatMonto(dia.ingresos)}</span>
                                    </div>
                                    <div className="w-full h-2 bg-neutral-800 rounded-full overflow-hidden">
                                        <div
                                            className="h-full bg-blue-500 rounded-full transition-all duration-1000"
                                            style={{ width: `${(dia.ingresos / maxVentasDia) * 100}%` }}
                                        />
                                    </div>
                                </div>
                            ))}
                        </div>
                    </div>

                    {/* Top Eventos */}
                    <div className="bg-neutral-900 border border-neutral-800 rounded-3xl p-8">
                        <h3 className="text-xl font-black mb-8 flex items-center gap-3">
                            <Award className="text-amber-500" />
                            {t.reports.topEvents}
                        </h3>
                        <div className="space-y-6">
                            {reporte.topEventos.map((evento, index) => (
                                <div key={evento.eventoNombre} className="group">
                                    <div className="flex justify-between items-end mb-2">
                                        <div>
                                            <p className="font-bold text-white group-hover:text-amber-400 transition-colors">
                                                <span className="text-neutral-600 mr-2">#{index + 1}</span>
                                                {evento.eventoNombre}
                                            </p>
                                            <p className="text-xs text-neutral-500">{evento.entradasVendidas} {t.reports.tickets}</p>
                                        </div>
                                        <p className="font-black text-amber-500">{formatMonto(evento.totalIngresos)}</p>
                                    </div>
                                    <div className="w-full h-2 bg-neutral-800 rounded-full overflow-hidden">
                                        <div
                                            className="h-full bg-gradient-to-r from-amber-600 to-amber-400 rounded-full"
                                            style={{ width: `${(evento.totalIngresos / reporte.totalIngresos) * 100}%` }}
                                        />
                                    </div>
                                </div>
                            ))}
                        </div>
                    </div>
                </div>

                {/* Ventas por Hora */}
                <div className="bg-neutral-900 border border-neutral-800 rounded-3xl p-8 mb-16">
                    <h3 className="text-xl font-black mb-8 flex items-center gap-3">
                        <Clock className="text-purple-500" />
                        {t.reports.salesByHour}
                    </h3>
                    <div className="h-64 flex items-end gap-2">
                        {reporte.ventasPorHora.filter(h => h.hora % 2 === 0).map((hora) => (
                            <div key={hora.hora} className="flex-1 flex flex-col items-center gap-2 group">
                                <div
                                    className="w-full bg-gradient-to-t from-purple-600 to-indigo-400 rounded-t-xl hover:from-purple-500 hover:to-indigo-300 transition-all relative"
                                    style={{ height: `${(hora.ventas / (maxVentasHora || 1)) * 100}%` }}
                                >
                                    <div className="absolute bottom-full left-1/2 -translate-x-1/2 mb-2 px-3 py-1 bg-white text-black text-xs font-black rounded-lg opacity-0 group-hover:opacity-100 transition-opacity whitespace-nowrap z-10">
                                        {formatMonto(hora.ventas)}
                                    </div>
                                </div>
                                <span className="text-[10px] font-bold text-neutral-600 font-mono">{hora.hora}h</span>
                            </div>
                        ))}
                    </div>
                </div>
            </div>
        </div>
    );
};
