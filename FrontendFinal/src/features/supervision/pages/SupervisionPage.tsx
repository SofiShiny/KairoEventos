import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import {
    Activity,
    ChevronLeft,
    Sparkles,
    Loader2,
    Server,
    CheckCircle2,
    AlertTriangle,
    XCircle,
    Clock,
    Cpu,
    HardDrive,
    Zap,
    RefreshCcw,
    TrendingUp
} from 'lucide-react';
import { supervisionService, Microservicio, EstadoServicio, MetricasSistema } from '../services/supervision.service';
import { toast } from 'react-hot-toast';
import { useT } from '../../../i18n';

export const SupervisionPage = () => {
    const navigate = useNavigate();
    const t = useT();

    const [loading, setLoading] = useState(true);
    const [servicios, setServicios] = useState<Microservicio[]>([]);
    const [metricas, setMetricas] = useState<MetricasSistema | null>(null);
    const [autoRefresh, setAutoRefresh] = useState(false);

    useEffect(() => {
        loadData();
    }, []);

    useEffect(() => {
        if (autoRefresh) {
            const interval = setInterval(loadData, 5000); // Cada 5 segundos
            return () => clearInterval(interval);
        }
    }, [autoRefresh]);

    const loadData = async () => {
        try {
            if (loading) setLoading(true);
            const data = await supervisionService.getMicroservicios();
            setServicios(data);

            const metrics = supervisionService.calcularMetricas(data);
            setMetricas(metrics);
        } catch (error) {
            console.error('Error cargando datos de supervisión:', error);
            toast.error(t.messages.serverError);
        } finally {
            setLoading(false);
        }
    };

    const getEstadoIcon = (estado: EstadoServicio) => {
        switch (estado) {
            case EstadoServicio.Saludable:
                return <CheckCircle2 className="w-5 h-5 text-green-500" />;
            case EstadoServicio.Degradado:
                return <AlertTriangle className="w-5 h-5 text-yellow-500" />;
            case EstadoServicio.Caido:
                return <XCircle className="w-5 h-5 text-red-500" />;
            default:
                return <Clock className="w-5 h-5 text-neutral-500" />;
        }
    };

    const getEstadoColor = (estado: EstadoServicio): string => {
        switch (estado) {
            case EstadoServicio.Saludable:
                return 'bg-green-500/10 border-green-500/20 text-green-500';
            case EstadoServicio.Degradado:
                return 'bg-yellow-500/10 border-yellow-500/20 text-yellow-500';
            case EstadoServicio.Caido:
                return 'bg-red-500/10 border-red-500/20 text-red-500';
            default:
                return 'bg-neutral-500/10 border-neutral-500/20 text-neutral-500';
        }
    };

    const getEstadoLabel = (estado: EstadoServicio): string => {
        const labels: Record<EstadoServicio, string> = {
            [EstadoServicio.Saludable]: t.supervision.healthy,
            [EstadoServicio.Degradado]: t.supervision.degraded,
            [EstadoServicio.Caido]: t.supervision.down,
            [EstadoServicio.Desconocido]: t.common.none
        };
        return labels[estado];
    };

    if (loading) {
        return (
            <div className="min-h-screen bg-black flex flex-col items-center justify-center">
                <Loader2 className="w-12 h-12 text-cyan-500 animate-spin mb-4" />
                <p className="text-neutral-500 font-bold uppercase tracking-widest text-xs">{t.common.loading}...</p>
            </div>
        );
    }

    if (!metricas) {
        return null;
    }

    const saludSistema = (metricas.serviciosSaludables / metricas.totalServicios) * 100;

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
                    <div className="absolute -top-20 -left-20 w-64 h-64 bg-cyan-600/10 blur-[120px] rounded-full pointer-events-none" />
                    <div className="flex items-center justify-between">
                        <div>
                            <div className="flex items-center gap-2 mb-4">
                                <Sparkles className="w-5 h-5 text-cyan-500" />
                                <span className="text-cyan-500 font-black text-xs uppercase tracking-[0.3em]">{t.supervision.services}</span>
                            </div>
                            <h1 className="text-5xl font-black mb-4 tracking-tighter uppercase">
                                {t.supervision.title}
                            </h1>
                            <p className="text-neutral-500 text-lg font-medium max-w-2xl">
                                {t.supervision.systemHealth}
                            </p>
                        </div>
                        <div className="flex gap-4">
                            <button
                                onClick={() => setAutoRefresh(!autoRefresh)}
                                className={`flex items-center gap-2 px-6 py-4 rounded-2xl transition-all ${autoRefresh
                                    ? 'bg-cyan-500/20 border border-cyan-500/40 text-cyan-400'
                                    : 'bg-neutral-900 border border-neutral-800 text-neutral-500'
                                    }`}
                            >
                                <Activity className={`w-5 h-5 ${autoRefresh ? 'animate-pulse' : ''}`} />
                                <span className="font-bold text-sm">
                                    {autoRefresh ? 'Auto-refresh ON' : 'Auto-refresh OFF'}
                                </span>
                            </button>
                            <button
                                onClick={loadData}
                                className="p-4 bg-cyan-500/10 border border-cyan-500/20 rounded-2xl hover:bg-cyan-500/20 transition-all group"
                                title={t.common.refresh}
                            >
                                <RefreshCcw className="w-6 h-6 text-cyan-500 group-hover:rotate-180 transition-transform duration-500" />
                            </button>
                        </div>
                    </div>
                </header>

                {/* Métricas Globales */}
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-16">
                    {/* Salud del Sistema */}
                    <div className="bg-gradient-to-br from-cyan-500/10 to-blue-500/10 border border-cyan-500/20 rounded-3xl p-6 relative overflow-hidden">
                        <div className="absolute top-0 right-0 w-32 h-32 bg-cyan-500/5 rounded-full -translate-y-16 translate-x-16" />
                        <Activity className="w-10 h-10 text-cyan-500 mb-4" />
                        <p className="text-4xl font-black text-white mb-2">
                            {saludSistema.toFixed(0)}%
                        </p>
                        <p className="text-sm font-bold text-cyan-400 uppercase tracking-widest flex items-center gap-2">
                            <TrendingUp className="w-4 h-4" />
                            {t.supervision.systemHealth}
                        </p>
                    </div>

                    {/* Servicios Saludables */}
                    <div className="bg-neutral-900 border border-neutral-800 rounded-3xl p-6">
                        <CheckCircle2 className="w-10 h-10 text-green-500 mb-4" />
                        <p className="text-4xl font-black text-white mb-2">
                            {metricas.serviciosSaludables}/{metricas.totalServicios}
                        </p>
                        <p className="text-sm font-bold text-green-400 uppercase tracking-widest">
                            {t.supervision.active}
                        </p>
                    </div>

                    {/* Tiempo de Respuesta */}
                    <div className="bg-neutral-900 border border-neutral-800 rounded-3xl p-6">
                        <Zap className="w-10 h-10 text-yellow-500 mb-4" />
                        <p className="text-4xl font-black text-white mb-2">
                            {metricas.tiempoRespuestaPromedio.toFixed(0)}ms
                        </p>
                        <p className="text-sm font-bold text-yellow-400 uppercase tracking-widest">
                            {t.supervision.responseTime}
                        </p>
                    </div>

                    {/* Requests Totales */}
                    <div className="bg-neutral-900 border border-neutral-800 rounded-3xl p-6">
                        <TrendingUp className="w-10 h-10 text-purple-500 mb-4" />
                        <p className="text-4xl font-black text-white mb-2">
                            {metricas.requestsTotales}
                        </p>
                        <p className="text-sm font-bold text-purple-400 uppercase tracking-widest">
                            {t.supervision.requests}
                        </p>
                    </div>
                </div>

                {/* Alertas */}
                {metricas.serviciosDegradados > 0 || metricas.serviciosCaidos > 0 ? (
                    <div className="mb-10 p-6 bg-yellow-500/10 border border-yellow-500/20 rounded-3xl">
                        <div className="flex items-center gap-3 mb-3">
                            <AlertTriangle className="w-6 h-6 text-yellow-500" />
                            <h3 className="text-lg font-black text-white uppercase">{t.common.warning}</h3>
                        </div>
                        <div className="space-y-2">
                            {metricas.serviciosDegradados > 0 && (
                                <p className="text-sm text-yellow-400">
                                    ⚠️ {metricas.serviciosDegradados} {t.supervision.degraded}
                                </p>
                            )}
                            {metricas.serviciosCaidos > 0 && (
                                <p className="text-sm text-red-400">
                                    🔴 {metricas.serviciosCaidos} {t.supervision.down}
                                </p>
                            )}
                        </div>
                    </div>
                ) : null}

                {/* Grid de Servicios */}
                <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                    {servicios.map((servicio) => (
                        <div
                            key={servicio.nombre}
                            className="bg-neutral-900/50 border border-neutral-800 rounded-3xl p-6 hover:bg-neutral-900/80 transition-all"
                        >
                            <div className="flex items-start justify-between mb-4">
                                <div className="flex items-center gap-3">
                                    <div className="w-12 h-12 bg-neutral-800 rounded-2xl flex items-center justify-center">
                                        <Server className="w-6 h-6 text-cyan-500" />
                                    </div>
                                    <div>
                                        <h3 className="text-xl font-black text-white">{servicio.nombre}</h3>
                                        <p className="text-sm text-neutral-500">{servicio.descripcion}</p>
                                    </div>
                                </div>
                                {getEstadoIcon(servicio.estado)}
                            </div>

                            <div className="grid grid-cols-2 gap-4 mb-4">
                                <div>
                                    <p className="text-xs text-neutral-600 uppercase tracking-widest mb-1">{t.tickets.status}</p>
                                    <span className={`inline-block px-3 py-1 rounded-full text-xs font-black uppercase tracking-widest border ${getEstadoColor(servicio.estado)}`}>
                                        {getEstadoLabel(servicio.estado)}
                                    </span>
                                </div>
                                <div>
                                    <p className="text-xs text-neutral-600 uppercase tracking-widest mb-1">{t.supervision.version}</p>
                                    <p className="text-sm font-bold text-white">{servicio.version}</p>
                                </div>
                                <div>
                                    <p className="text-xs text-neutral-600 uppercase tracking-widest mb-1">{t.supervision.uptime}</p>
                                    <p className="text-sm font-bold text-white">
                                        {supervisionService.formatUptime(servicio.uptime)}
                                    </p>
                                </div>
                                <div>
                                    <p className="text-xs text-neutral-600 uppercase tracking-widest mb-1">{t.supervision.port}</p>
                                    <p className="text-sm font-mono text-white">{servicio.puerto}</p>
                                </div>
                            </div>

                            {/* Métricas del Servicio */}
                            <div className="grid grid-cols-4 gap-3">
                                <div className="bg-neutral-800/50 rounded-xl p-3">
                                    <Clock className="w-4 h-4 text-cyan-500 mb-1" />
                                    <p className="text-xs text-neutral-500">{t.logs.timestamp}</p>
                                    <p className="text-sm font-black text-white">{servicio.tiempoRespuesta}ms</p>
                                </div>
                                <div className="bg-neutral-800/50 rounded-xl p-3">
                                    <Cpu className="w-4 h-4 text-purple-500 mb-1" />
                                    <p className="text-xs text-neutral-500">{t.supervision.cpu}</p>
                                    <p className="text-sm font-black text-white">{servicio.cpu}%</p>
                                </div>
                                <div className="bg-neutral-800/50 rounded-xl p-3">
                                    <HardDrive className="w-4 h-4 text-blue-500 mb-1" />
                                    <p className="text-xs text-neutral-500">{t.supervision.memory}</p>
                                    <p className="text-sm font-black text-white">{servicio.memoriaUsada}MB</p>
                                </div>
                                <div className="bg-neutral-800/50 rounded-xl p-3">
                                    <Zap className="w-4 h-4 text-yellow-500 mb-1" />
                                    <p className="text-xs text-neutral-500">{t.supervision.requests}</p>
                                    <p className="text-sm font-black text-white">{servicio.requestsPorMinuto}</p>
                                </div>
                            </div>
                        </div>
                    ))}
                </div>
            </div>
        </div>
    );
};
