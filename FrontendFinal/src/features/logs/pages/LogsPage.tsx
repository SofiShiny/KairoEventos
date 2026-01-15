import { useState, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import {
    Terminal,
    ChevronLeft,
    Sparkles,
    Loader2,
    Search,
    Filter,
    Download,
    RefreshCcw,
    Play,
    Pause,
    Trash2,
    AlertCircle,
    Info,
    AlertTriangle,
    XCircle,
    Bug
} from 'lucide-react';
import { logsService, LogEntry, NivelLog, FiltrosLog } from '../services/logs.service';
import { toast } from 'react-hot-toast';
import { useT } from '../../../i18n';

export const LogsPage = () => {
    const navigate = useNavigate();
    const t = useT();
    const logsEndRef = useRef<HTMLDivElement>(null);

    const [loading, setLoading] = useState(true);
    const [todosLosLogs, setTodosLosLogs] = useState<LogEntry[]>([]);
    const [logsFiltrados, setLogsFiltrados] = useState<LogEntry[]>([]);
    const [autoScroll, setAutoScroll] = useState(true);
    const [streaming, setStreaming] = useState(false);
    const [logSeleccionado, setLogSeleccionado] = useState<LogEntry | null>(null);

    const [filtros, setFiltros] = useState<FiltrosLog>({});

    useEffect(() => {
        loadLogs();
    }, []);

    useEffect(() => {
        const filtered = logsService.filtrarLogs(todosLosLogs, filtros);
        setLogsFiltrados(filtered);
    }, [todosLosLogs, filtros]);

    useEffect(() => {
        if (autoScroll && logsEndRef.current) {
            logsEndRef.current.scrollIntoView({ behavior: 'smooth' });
        }
    }, [logsFiltrados, autoScroll]);

    useEffect(() => {
        if (streaming) {
            const interval = setInterval(() => {
                // Simular nuevo log cada 2 segundos
                const nuevoLog = logsService.generarLogs(1)[0];
                setTodosLosLogs(prev => [nuevoLog, ...prev].slice(0, 500)); // Mantener máximo 500 logs
            }, 2000);
            return () => clearInterval(interval);
        }
    }, [streaming]);

    const loadLogs = async () => {
        try {
            setLoading(true);
            const logs = logsService.generarLogs(100);
            setTodosLosLogs(logs);
        } catch (error) {
            console.error('Error cargando logs:', error);
            toast.error(t.messages.serverError);
        } finally {
            setLoading(false);
        }
    };

    const limpiarLogs = () => {
        setTodosLosLogs([]);
        setLogSeleccionado(null);
        toast.success(t.messages.deleteSuccess);
    };

    const exportarLogs = () => {
        const contenido = logsService.exportarLogs(logsFiltrados);
        const blob = new Blob([contenido], { type: 'text/plain' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `logs-${new Date().toISOString()}.txt`;
        a.click();
        toast.success(t.common.success);
    };

    const getNivelIcon = (nivel: NivelLog) => {
        switch (nivel) {
            case NivelLog.Debug:
                return <Bug className="w-4 h-4" />;
            case NivelLog.Info:
                return <Info className="w-4 h-4" />;
            case NivelLog.Warning:
                return <AlertTriangle className="w-4 h-4" />;
            case NivelLog.Error:
                return <XCircle className="w-4 h-4" />;
            case NivelLog.Critical:
                return <AlertCircle className="w-4 h-4" />;
        }
    };

    const getNivelColor = (nivel: NivelLog): string => {
        switch (nivel) {
            case NivelLog.Debug:
                return 'text-neutral-500';
            case NivelLog.Info:
                return 'text-blue-400';
            case NivelLog.Warning:
                return 'text-yellow-400';
            case NivelLog.Error:
                return 'text-red-400';
            case NivelLog.Critical:
                return 'text-red-600';
        }
    };

    const getNivelBg = (nivel: NivelLog): string => {
        switch (nivel) {
            case NivelLog.Debug:
                return 'bg-neutral-500/10';
            case NivelLog.Info:
                return 'bg-blue-500/10';
            case NivelLog.Warning:
                return 'bg-yellow-500/10';
            case NivelLog.Error:
                return 'bg-red-500/10';
            case NivelLog.Critical:
                return 'bg-red-600/20';
        }
    };

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
            <div className="max-w-[1800px] mx-auto">
                <button
                    onClick={() => navigate('/admin')}
                    className="flex items-center gap-2 text-neutral-500 hover:text-white transition-colors mb-12 group"
                >
                    <ChevronLeft className="w-5 h-5 group-hover:-translate-x-1 transition-transform" />
                    <span className="font-bold uppercase tracking-widest text-xs">{t.common.back}</span>
                </button>

                <header className="mb-12 relative">
                    <div className="absolute -top-20 -left-20 w-64 h-64 bg-green-600/10 blur-[120px] rounded-full pointer-events-none" />
                    <div className="flex items-center justify-between">
                        <div>
                            <div className="flex items-center gap-2 mb-4">
                                <Sparkles className="w-5 h-5 text-green-500" />
                                <span className="text-green-500 font-black text-xs uppercase tracking-[0.3em]">{t.supervision.services}</span>
                            </div>
                            <h1 className="text-5xl font-black mb-4 tracking-tighter uppercase">
                                {t.logs.title}
                            </h1>
                            <p className="text-neutral-500 text-lg font-medium max-w-2xl">
                                {t.supervision.systemHealth}
                            </p>
                        </div>
                        <div className="flex gap-4">
                            <button
                                onClick={() => setStreaming(!streaming)}
                                className={`flex items-center gap-2 px-6 py-4 rounded-2xl transition-all ${streaming
                                    ? 'bg-green-500/20 border border-green-500/40 text-green-400'
                                    : 'bg-neutral-900 border border-neutral-800 text-neutral-500'
                                    }`}
                            >
                                {streaming ? <Pause className="w-5 h-5" /> : <Play className="w-5 h-5" />}
                                <span className="font-bold text-sm">
                                    {streaming ? 'Streaming ON' : 'Streaming OFF'}
                                </span>
                            </button>
                            <button
                                onClick={loadLogs}
                                className="p-4 bg-green-500/10 border border-green-500/20 rounded-2xl hover:bg-green-500/20 transition-all group"
                                title={t.common.refresh}
                            >
                                <RefreshCcw className="w-6 h-6 text-green-500 group-hover:rotate-180 transition-transform duration-500" />
                            </button>
                        </div>
                    </div>
                </header>

                {/* Filtros y Controles */}
                <div className="mb-8 space-y-4">
                    {/* Búsqueda */}
                    <div className="relative">
                        <Search className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-neutral-600" />
                        <input
                            type="text"
                            value={filtros.busqueda || ''}
                            onChange={(e) => setFiltros({ ...filtros, busqueda: e.target.value })}
                            placeholder={t.common.search + '...'}
                            className="w-full bg-neutral-900 border border-neutral-800 rounded-2xl pl-12 pr-4 py-4 text-white placeholder-neutral-600 focus:ring-2 focus:ring-green-500 focus:border-transparent transition-all font-mono text-sm"
                        />
                    </div>

                    {/* Filtros */}
                    <div className="flex flex-wrap gap-4">
                        <div className="flex items-center gap-2">
                            <Filter className="w-4 h-4 text-neutral-600" />
                            <span className="text-xs font-bold text-neutral-600 uppercase tracking-widest">{t.common.filter}:</span>
                        </div>

                        {/* Servicio */}
                        <select
                            value={filtros.servicio || ''}
                            onChange={(e) => setFiltros({ ...filtros, servicio: e.target.value || undefined })}
                            className="px-4 py-2 bg-neutral-900 border border-neutral-800 rounded-xl text-sm font-bold text-white focus:ring-2 focus:ring-green-500"
                        >
                            <option value="">{t.common.all} {t.adminMenu.users}</option>
                            <option value="Gateway">Gateway</option>
                            <option value="Eventos">Eventos</option>
                            <option value="Entradas">Entradas</option>
                            <option value="Pagos">Pagos</option>
                            <option value="Usuarios">Usuarios</option>
                        </select>

                        {/* Nivel */}
                        <select
                            value={filtros.nivel || ''}
                            onChange={(e) => setFiltros({ ...filtros, nivel: e.target.value as NivelLog || undefined })}
                            className="px-4 py-2 bg-neutral-900 border border-neutral-800 rounded-xl text-sm font-bold text-white focus:ring-2 focus:ring-green-500"
                        >
                            <option value="">{t.common.all} {t.logs.level}</option>
                            <option value={NivelLog.Debug}>{t.logs.debug}</option>
                            <option value={NivelLog.Info}>{t.logs.info}</option>
                            <option value={NivelLog.Warning}>{t.logs.warning}</option>
                            <option value={NivelLog.Error}>{t.logs.error}</option>
                            <option value={NivelLog.Critical}>{t.logs.critical}</option>
                        </select>

                        {/* Controles */}
                        <div className="ml-auto flex gap-2">
                            <button
                                onClick={() => setAutoScroll(!autoScroll)}
                                className={`px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all ${autoScroll ? 'bg-green-500 text-black' : 'bg-neutral-900 text-neutral-500 border border-neutral-800'
                                    }`}
                            >
                                {t.logs.autoScroll}
                            </button>
                            <button
                                onClick={exportarLogs}
                                className="flex items-center gap-2 px-4 py-2 bg-neutral-900 border border-neutral-800 rounded-xl hover:bg-neutral-800 transition-all"
                            >
                                <Download className="w-4 h-4 text-green-500" />
                                <span className="text-xs font-bold text-white">{t.common.export}</span>
                            </button>
                            <button
                                onClick={limpiarLogs}
                                className="flex items-center gap-2 px-4 py-2 bg-red-500/10 border border-red-500/20 rounded-xl hover:bg-red-500/20 transition-all"
                            >
                                <Trash2 className="w-4 h-4 text-red-500" />
                                <span className="text-xs font-bold text-red-400">{t.common.delete}</span>
                            </button>
                        </div>
                    </div>

                    {/* Contador */}
                    <div className="flex items-center gap-4 text-sm">
                        <span className="text-neutral-500">
                            Mostrando <span className="text-white font-bold">{logsFiltrados.length}</span> de <span className="text-white font-bold">{todosLosLogs.length}</span> logs
                        </span>
                        {streaming && (
                            <span className="flex items-center gap-2 text-green-400">
                                <span className="w-2 h-2 bg-green-500 rounded-full animate-pulse" />
                                Streaming activo
                            </span>
                        )}
                    </div>
                </div>

                {/* Terminal de Logs */}
                <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                    {/* Lista de Logs */}
                    <div className="lg:col-span-2 bg-black border border-green-500/20 rounded-3xl overflow-hidden">
                        <div className="bg-neutral-900/50 border-b border-green-500/20 px-6 py-4 flex items-center gap-3">
                            <Terminal className="w-5 h-5 text-green-500" />
                            <span className="font-black text-sm uppercase tracking-widest text-green-400">{t.logs.terminal}</span>
                            <div className="ml-auto flex gap-2">
                                <div className="w-3 h-3 rounded-full bg-red-500" />
                                <div className="w-3 h-3 rounded-full bg-yellow-500" />
                                <div className="w-3 h-3 rounded-full bg-green-500" />
                            </div>
                        </div>

                        <div className="h-[600px] overflow-y-auto p-4 font-mono text-xs bg-black" style={{ fontFamily: 'Monaco, Consolas, monospace' }}>
                            {logsFiltrados.length === 0 ? (
                                <div className="flex flex-col items-center justify-center h-full text-neutral-700">
                                    <Terminal className="w-12 h-12 mb-4" />
                                    <p className="font-bold uppercase tracking-widest text-xs">{t.common.noResults}</p>
                                </div>
                            ) : (
                                logsFiltrados.map((log) => (
                                    <div
                                        key={log.id}
                                        onClick={() => setLogSeleccionado(log)}
                                        className={`mb-1 p-2 rounded cursor-pointer hover:bg-neutral-900/50 transition-colors ${logSeleccionado?.id === log.id ? 'bg-neutral-900' : ''
                                            } ${getNivelBg(log.nivel)}`}
                                    >
                                        <div className="flex items-start gap-2">
                                            <span className="text-neutral-600 flex-shrink-0">
                                                {logsService.formatTimestamp(log.timestamp)}
                                            </span>
                                            <span className={`flex-shrink-0 ${getNivelColor(log.nivel)}`}>
                                                [{log.nivel.toUpperCase().padEnd(8)}]
                                            </span>
                                            <span className="text-cyan-400 flex-shrink-0">
                                                [{log.servicio}]
                                            </span>
                                            <span className="text-neutral-300 flex-1">
                                                {log.mensaje}
                                            </span>
                                        </div>
                                    </div>
                                ))
                            )}
                            <div ref={logsEndRef} />
                        </div>
                    </div>

                    {/* Detalle del Log */}
                    <div className="bg-neutral-900/50 border border-neutral-800 rounded-3xl p-6">
                        <h3 className="text-lg font-black uppercase tracking-tight mb-6 flex items-center gap-3">
                            <Info className="text-green-500" />
                            {t.logs.details}
                        </h3>

                        {logSeleccionado ? (
                            <div className="space-y-4">
                                <div>
                                    <p className="text-xs font-bold text-neutral-600 uppercase tracking-widest mb-2">{t.logs.level}</p>
                                    <div className={`flex items-center gap-2 px-3 py-2 rounded-xl ${getNivelBg(logSeleccionado.nivel)}`}>
                                        {getNivelIcon(logSeleccionado.nivel)}
                                        <span className={`font-bold text-sm ${getNivelColor(logSeleccionado.nivel)}`}>
                                            {logSeleccionado.nivel.toUpperCase()}
                                        </span>
                                    </div>
                                </div>

                                <div>
                                    <p className="text-xs font-bold text-neutral-600 uppercase tracking-widest mb-2">{t.logs.timestamp}</p>
                                    <p className="text-sm font-mono text-white bg-black px-3 py-2 rounded-xl">
                                        {logsService.formatTimestamp(logSeleccionado.timestamp)}
                                    </p>
                                </div>

                                <div>
                                    <p className="text-xs font-bold text-neutral-600 uppercase tracking-widest mb-2">{t.logs.service}</p>
                                    <p className="text-sm font-bold text-cyan-400">{logSeleccionado.servicio}</p>
                                </div>

                                <div>
                                    <p className="text-xs font-bold text-neutral-600 uppercase tracking-widest mb-2">{t.logs.message}</p>
                                    <p className="text-sm text-neutral-300 bg-black px-3 py-2 rounded-xl">
                                        {logSeleccionado.mensaje}
                                    </p>
                                </div>

                                {logSeleccionado.usuario && (
                                    <div>
                                        <p className="text-xs font-bold text-neutral-600 uppercase tracking-widest mb-2">{t.profile.personalInfo}</p>
                                        <p className="text-sm font-mono text-white">{logSeleccionado.usuario}</p>
                                    </div>
                                )}

                                {logSeleccionado.ip && (
                                    <div>
                                        <p className="text-xs font-bold text-neutral-600 uppercase tracking-widest mb-2">IP</p>
                                        <p className="text-sm font-mono text-white">{logSeleccionado.ip}</p>
                                    </div>
                                )}

                                {logSeleccionado.duracion && (
                                    <div>
                                        <p className="text-xs font-bold text-neutral-600 uppercase tracking-widest mb-2">{t.supervision.responseTime}</p>
                                        <p className="text-sm font-mono text-white">{logSeleccionado.duracion}ms</p>
                                    </div>
                                )}

                                {logSeleccionado.stackTrace && (
                                    <div>
                                        <p className="text-xs font-bold text-neutral-600 uppercase tracking-widest mb-2">{t.logs.stackTrace}</p>
                                        <pre className="text-xs font-mono text-red-400 bg-black px-3 py-2 rounded-xl overflow-x-auto">
                                            {logSeleccionado.stackTrace}
                                        </pre>
                                    </div>
                                )}
                            </div>
                        ) : (
                            <div className="flex flex-col items-center justify-center h-64 text-neutral-700">
                                <AlertCircle className="w-12 h-12 mb-4" />
                                <p className="font-bold uppercase tracking-widest text-xs text-center">
                                    {t.messages.loginRequired}
                                </p>
                            </div>
                        )}
                    </div>
                </div>
            </div>
        </div>
    );
};
