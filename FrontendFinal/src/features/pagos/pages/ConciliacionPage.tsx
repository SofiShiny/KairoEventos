import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import {
    DollarSign,
    ChevronLeft,
    Sparkles,
    Loader2,
    TrendingUp,
    Clock,
    CheckCircle2,
    XCircle,
    RefreshCcw,
    Calendar,
    CreditCard,
    BarChart3,
    PieChart,
    Download
} from 'lucide-react';
import { pagosService, Transaccion, EstadoTransaccion, EstadisticasFinancieras } from '../services/pagos.service';
import { toast } from 'react-hot-toast';
import { useT } from '../../../i18n';

export const ConciliacionPage = () => {
    const navigate = useNavigate();
    const t = useT();

    const [loading, setLoading] = useState(true);
    const [transacciones, setTransacciones] = useState<Transaccion[]>([]);
    const [estadisticas, setEstadisticas] = useState<EstadisticasFinancieras | null>(null);
    const [filtro, setFiltro] = useState<'todas' | EstadoTransaccion>('todas');

    useEffect(() => {
        loadData();
    }, []);

    const loadData = async () => {
        try {
            setLoading(true);
            const data = await pagosService.getTodasTransacciones();
            setTransacciones(data);

            const stats = pagosService.calcularEstadisticas(data);
            setEstadisticas(stats);
        } catch (error) {
            console.error('Error cargando datos financieros:', error);
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
        return date.toLocaleDateString(document.documentElement.lang === 'es' ? 'es-ES' : 'en-US', {
            day: '2-digit',
            month: 'short',
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    };

    const getEstadoLabel = (estado: EstadoTransaccion): string => {
        const labels: Record<EstadoTransaccion, string> = {
            [EstadoTransaccion.Pendiente]: t.finance.pending,
            [EstadoTransaccion.Procesando]: t.finance.pending, // Both show as pending for user
            [EstadoTransaccion.Aprobada]: t.finance.approved,
            [EstadoTransaccion.Rechazada]: t.finance.rejected,
            [EstadoTransaccion.Reembolsada]: t.finance.refunded
        };
        return labels[estado];
    };

    const getEstadoColor = (estado: EstadoTransaccion): string => {
        const colors: Record<EstadoTransaccion, string> = {
            [EstadoTransaccion.Pendiente]: 'bg-neutral-500/10 text-neutral-500 border-neutral-500/20',
            [EstadoTransaccion.Procesando]: 'bg-yellow-500/10 text-yellow-500 border-yellow-500/20',
            [EstadoTransaccion.Aprobada]: 'bg-green-500/10 text-green-500 border-green-500/20',
            [EstadoTransaccion.Rechazada]: 'bg-red-500/10 text-red-500 border-red-500/20',
            [EstadoTransaccion.Reembolsada]: 'bg-orange-500/10 text-orange-500 border-orange-500/20'
        };
        return colors[estado];
    };

    const transaccionesFiltradas = filtro === 'todas'
        ? transacciones
        : transacciones.filter(t => t.estado === filtro);

    if (loading) {
        return (
            <div className="min-h-screen bg-black flex flex-col items-center justify-center">
                <Loader2 className="w-12 h-12 text-green-500 animate-spin mb-4" />
                <p className="text-neutral-500 font-bold uppercase tracking-widest text-xs">{t.common.loading}...</p>
            </div>
        );
    }

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
                    <div className="absolute -top-20 -left-20 w-64 h-64 bg-green-600/10 blur-[120px] rounded-full pointer-events-none" />
                    <div className="flex items-center justify-between">
                        <div>
                            <div className="flex items-center gap-2 mb-4">
                                <Sparkles className="w-5 h-5 text-green-500" />
                                <span className="text-green-500 font-black text-xs uppercase tracking-[0.3em]">{t.adminMenu.finance}</span>
                            </div>
                            <h1 className="text-5xl font-black mb-4 tracking-tighter uppercase">
                                {t.finance.title}
                            </h1>
                            <p className="text-neutral-500 text-lg font-medium max-w-2xl">
                                {t.finance.description}
                            </p>
                        </div>
                        <button
                            onClick={loadData}
                            className="p-4 bg-green-500/10 border border-green-500/20 rounded-2xl hover:bg-green-500/20 transition-all group"
                            title={t.common.refresh}
                        >
                            <RefreshCcw className="w-6 h-6 text-green-500 group-hover:rotate-180 transition-transform duration-500" />
                        </button>
                    </div>
                </header>

                {/* KPIs Principales */}
                {estadisticas && (
                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-16">
                        {/* Ingresos Totales */}
                        <div className="bg-gradient-to-br from-green-500/10 to-emerald-500/10 border border-green-500/20 rounded-3xl p-6 relative overflow-hidden">
                            <div className="absolute top-0 right-0 w-32 h-32 bg-green-500/5 rounded-full -translate-y-16 translate-x-16" />
                            <DollarSign className="w-10 h-10 text-green-500 mb-4" />
                            <p className="text-4xl font-black text-white mb-2">
                                {formatMonto(estadisticas.totalIngresos)}
                            </p>
                            <p className="text-sm font-bold text-green-400 uppercase tracking-widest flex items-center gap-2">
                                <TrendingUp className="w-4 h-4" />
                                {t.finance.netIncome}
                            </p>
                        </div>

                        {/* Transacciones Aprobadas */}
                        <div className="bg-neutral-900 border border-neutral-800 rounded-3xl p-6">
                            <CheckCircle2 className="w-10 h-10 text-blue-500 mb-4" />
                            <p className="text-4xl font-black text-white mb-2">
                                {estadisticas.transaccionesAprobadas}
                            </p>
                            <p className="text-sm font-bold text-blue-400 uppercase tracking-widest">
                                {t.finance.approved}
                            </p>
                            <p className="text-xs text-neutral-500 mt-2">
                                {formatMonto(estadisticas.montoAprobado)}
                            </p>
                        </div>

                        {/* Tasa de Aprobación */}
                        <div className="bg-neutral-900 border border-neutral-800 rounded-3xl p-6">
                            <BarChart3 className="w-10 h-10 text-purple-500 mb-4" />
                            <p className="text-4xl font-black text-white mb-2">
                                {estadisticas.tasaAprobacion.toFixed(1)}%
                            </p>
                            <p className="text-sm font-bold text-purple-400 uppercase tracking-widest">
                                {t.finance.approvalRate}
                            </p>
                        </div>

                        {/* Total Transacciones */}
                        <div className="bg-neutral-900 border border-neutral-800 rounded-3xl p-6">
                            <CreditCard className="w-10 h-10 text-orange-500 mb-4" />
                            <p className="text-4xl font-black text-white mb-2">
                                {estadisticas.totalTransacciones}
                            </p>
                            <p className="text-sm font-bold text-orange-400 uppercase tracking-widest">
                                {t.finance.transactions}
                            </p>
                        </div>
                    </div>
                )}

                {/* Métricas Detalladas */}
                {estadisticas && (
                    <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-16">
                        <div className="bg-yellow-500/10 border border-yellow-500/20 rounded-3xl p-6">
                            <div className="flex items-center justify-between mb-4">
                                <Clock className="w-8 h-8 text-yellow-500" />
                                <span className="text-2xl font-black text-white">{estadisticas.transaccionesPendientes}</span>
                            </div>
                            <p className="text-sm font-bold text-yellow-400 uppercase tracking-widest mb-2">{t.finance.pending}</p>
                            <p className="text-xs text-neutral-400">{formatMonto(estadisticas.montoPendiente)}</p>
                        </div>

                        <div className="bg-red-500/10 border border-red-500/20 rounded-3xl p-6">
                            <div className="flex items-center justify-between mb-4">
                                <XCircle className="w-8 h-8 text-red-500" />
                                <span className="text-2xl font-black text-white">{estadisticas.transaccionesRechazadas}</span>
                            </div>
                            <p className="text-sm font-bold text-red-400 uppercase tracking-widest mb-2">{t.finance.rejected}</p>
                            <p className="text-xs text-neutral-400">{formatMonto(estadisticas.montoRechazado)}</p>
                        </div>

                        <div className="bg-orange-500/10 border border-orange-500/20 rounded-3xl p-6">
                            <div className="flex items-center justify-between mb-4">
                                <RefreshCcw className="w-8 h-8 text-orange-500" />
                                <span className="text-2xl font-black text-white">{estadisticas.transaccionesReembolsadas}</span>
                            </div>
                            <p className="text-sm font-bold text-orange-400 uppercase tracking-widest mb-2">{t.finance.refunded}</p>
                            <p className="text-xs text-neutral-400">{formatMonto(estadisticas.montoReembolsado)}</p>
                        </div>
                    </div>
                )}

                {/* Filtros */}
                <div className="flex flex-wrap gap-4 mb-10">
                    <button
                        onClick={() => setFiltro('todas')}
                        className={`px-6 py-3 rounded-2xl font-black text-xs uppercase tracking-widest transition-all ${filtro === 'todas' ? 'bg-white text-black' : 'bg-neutral-900 text-neutral-500 border border-neutral-800'
                            }`}
                    >
                        {t.common.all} ({transacciones.length})
                    </button>
                    <button
                        onClick={() => setFiltro(EstadoTransaccion.Aprobada)}
                        className={`px-6 py-3 rounded-2xl font-black text-xs uppercase tracking-widest transition-all ${filtro === EstadoTransaccion.Aprobada ? 'bg-green-500 text-white' : 'bg-neutral-900 text-neutral-500 border border-neutral-800'
                            }`}
                    >
                        {t.finance.approved} ({estadisticas?.transaccionesAprobadas || 0})
                    </button>
                    <button
                        onClick={() => setFiltro(EstadoTransaccion.Procesando)}
                        className={`px-6 py-3 rounded-2xl font-black text-xs uppercase tracking-widest transition-all ${filtro === EstadoTransaccion.Procesando ? 'bg-yellow-500 text-black' : 'bg-neutral-900 text-neutral-500 border border-neutral-800'
                            }`}
                    >
                        {t.finance.pending} ({estadisticas?.transaccionesPendientes || 0})
                    </button>
                    <button
                        onClick={() => setFiltro(EstadoTransaccion.Rechazada)}
                        className={`px-6 py-3 rounded-2xl font-black text-xs uppercase tracking-widest transition-all ${filtro === EstadoTransaccion.Rechazada ? 'bg-red-500 text-white' : 'bg-neutral-900 text-neutral-500 border border-neutral-800'
                            }`}
                    >
                        {t.finance.rejected} ({estadisticas?.transaccionesRechazadas || 0})
                    </button>
                </div>

                {/* Tabla de Transacciones */}
                <div className="bg-neutral-900/50 border border-neutral-800 rounded-3xl overflow-hidden">
                    <div className="p-6 border-b border-neutral-800 flex items-center justify-between">
                        <h2 className="text-xl font-black uppercase tracking-tight flex items-center gap-3">
                            <PieChart className="text-green-500" />
                            {t.finance.transactionDetails}
                        </h2>
                        <button className="flex items-center gap-2 px-4 py-2 bg-neutral-800 hover:bg-neutral-700 rounded-xl transition-all text-sm font-bold">
                            <Download className="w-4 h-4" />
                            {t.common.export}
                        </button>
                    </div>

                    <div className="overflow-x-auto">
                        <table className="w-full">
                            <thead className="bg-neutral-800/50">
                                <tr>
                                    <th className="px-6 py-4 text-left text-xs font-black uppercase tracking-widest text-neutral-400">ID</th>
                                    <th className="px-6 py-4 text-left text-xs font-black uppercase tracking-widest text-neutral-400">{t.audit.date}</th>
                                    <th className="px-6 py-4 text-left text-xs font-black uppercase tracking-widest text-neutral-400">{t.finance.order}</th>
                                    <th className="px-6 py-4 text-left text-xs font-black uppercase tracking-widest text-neutral-400">{t.finance.card}</th>
                                    <th className="px-6 py-4 text-right text-xs font-black uppercase tracking-widest text-neutral-400">{t.finance.amount}</th>
                                    <th className="px-6 py-4 text-center text-xs font-black uppercase tracking-widest text-neutral-400">{t.tickets.status}</th>
                                </tr>
                            </thead>
                            <tbody>
                                {transaccionesFiltradas.length === 0 ? (
                                    <tr>
                                        <td colSpan={6} className="px-6 py-12 text-center text-neutral-600">
                                            <DollarSign className="w-12 h-12 mx-auto mb-4 text-neutral-800" />
                                            <p className="font-bold uppercase tracking-widest text-xs">{t.common.noResults}</p>
                                        </td>
                                    </tr>
                                ) : (
                                    transaccionesFiltradas.map((tx) => (
                                        <tr key={tx.id} className="border-t border-neutral-800 hover:bg-neutral-800/30 transition-colors">
                                            <td className="px-6 py-4 text-sm font-mono text-neutral-400">
                                                {tx.id.substring(0, 8)}...
                                            </td>
                                            <td className="px-6 py-4 text-sm text-neutral-300 flex items-center gap-2">
                                                <Calendar className="w-4 h-4 text-neutral-600" />
                                                {formatFecha(tx.fechaCreacion)}
                                            </td>
                                            <td className="px-6 py-4 text-sm font-mono text-neutral-400">
                                                {tx.ordenId.substring(0, 8)}...
                                            </td>
                                            <td className="px-6 py-4 text-sm font-mono text-neutral-300">
                                                {tx.tarjetaMascara}
                                            </td>
                                            <td className="px-6 py-4 text-right text-sm font-black text-white">
                                                {formatMonto(tx.monto)}
                                            </td>
                                            <td className="px-6 py-4 text-center">
                                                <span className={`px-3 py-1 rounded-full text-[10px] font-black uppercase tracking-widest border ${getEstadoColor(tx.estado)}`}>
                                                    {getEstadoLabel(tx.estado)}
                                                </span>
                                            </td>
                                        </tr>
                                    ))
                                )}
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        </div>
    );
};
