import { useState, useEffect } from 'react';
import { useAuth } from 'react-oidc-context';
import { useNavigate } from 'react-router-dom';
import {
    Mail,
    ChevronLeft,
    Sparkles,
    Loader2,
    CheckCircle2,
    XCircle,
    Clock,
    Send,
    Calendar,
    Filter,
    Search,
    AlertCircle,
    Gift,
    Ban,
    RefreshCcw
} from 'lucide-react';
import { entradasService } from '../../entradas/services/entradas.service';
import { correosService, Correo, TipoCorreo, EstadoCorreo } from '../services/correos.service';
import { toast } from 'react-hot-toast';
import { useT } from '../../../i18n';

export const HistorialCorreosPage = () => {
    const auth = useAuth();
    const navigate = useNavigate();
    const t = useT();

    const [loading, setLoading] = useState(true);
    const [correos, setCorreos] = useState<Correo[]>([]);
    const [filtroTipo, setFiltroTipo] = useState<'todos' | TipoCorreo>('todos');
    const [busqueda, setBusqueda] = useState('');
    const [correoSeleccionado, setCorreoSeleccionado] = useState<Correo | null>(null);

    const usuarioId = auth.user?.profile.sub;
    const email = auth.user?.profile.email;

    useEffect(() => {
        if (usuarioId && email) {
            loadCorreos();
        }
    }, [usuarioId, email]);

    const loadCorreos = async () => {
        try {
            setLoading(true);
            const entradas = await entradasService.getMisEntradas(usuarioId!);
            const historial = correosService.generarHistorialCorreos(entradas, email!);
            setCorreos(historial);
        } catch (error) {
            console.error('Error cargando historial de correos:', error);
            toast.error(t.messages.serverError);
        } finally {
            setLoading(false);
        }
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

    const getTipoIcon = (tipo: TipoCorreo) => {
        switch (tipo) {
            case TipoCorreo.Confirmacion: return CheckCircle2;
            case TipoCorreo.Recordatorio: return Clock;
            case TipoCorreo.Cancelacion: return Ban;
            case TipoCorreo.Reembolso: return RefreshCcw;
            case TipoCorreo.Bienvenida: return Sparkles;
            case TipoCorreo.Promocion: return Gift;
            default: return Mail;
        }
    };

    const getTipoColor = (tipo: TipoCorreo): string => {
        const colors: Record<TipoCorreo, string> = {
            [TipoCorreo.Confirmacion]: 'bg-green-500/10 text-green-500 border-green-500/20',
            [TipoCorreo.Recordatorio]: 'bg-blue-500/10 text-blue-500 border-blue-500/20',
            [TipoCorreo.Cancelacion]: 'bg-red-500/10 text-red-500 border-red-500/20',
            [TipoCorreo.Reembolso]: 'bg-orange-500/10 text-orange-500 border-orange-500/20',
            [TipoCorreo.Bienvenida]: 'bg-purple-500/10 text-purple-500 border-purple-500/20',
            [TipoCorreo.Promocion]: 'bg-pink-500/10 text-pink-500 border-pink-500/20'
        };
        return colors[tipo];
    };

    const getEstadoIcon = (estado: EstadoCorreo) => {
        switch (estado) {
            case EstadoCorreo.Entregado: return CheckCircle2;
            case EstadoCorreo.Enviado: return Send;
            case EstadoCorreo.Fallido: return XCircle;
            default: return Clock;
        }
    };

    const getEstadoColor = (estado: EstadoCorreo): string => {
        const colors: Record<EstadoCorreo, string> = {
            [EstadoCorreo.Entregado]: 'text-green-500',
            [EstadoCorreo.Enviado]: 'text-blue-500',
            [EstadoCorreo.Fallido]: 'text-red-500',
            [EstadoCorreo.Pendiente]: 'text-yellow-500'
        };
        return colors[estado];
    };

    const getTipoLabel = (tipo: TipoCorreo): string => {
        const labels: Record<TipoCorreo, string> = {
            [TipoCorreo.Confirmacion]: t.emails.confirmation,
            [TipoCorreo.Recordatorio]: t.emails.reminder,
            [TipoCorreo.Cancelacion]: t.emails.cancellation,
            [TipoCorreo.Reembolso]: t.emails.refund,
            [TipoCorreo.Bienvenida]: t.emails.welcome,
            [TipoCorreo.Promocion]: t.emails.promotion
        };
        return labels[tipo];
    };

    const correosFiltrados = correos.filter(c => {
        const matchesFiltro = filtroTipo === 'todos' || c.tipo === filtroTipo;
        const matchesBusqueda = c.asunto.toLowerCase().includes(busqueda.toLowerCase()) ||
            c.contenido.toLowerCase().includes(busqueda.toLowerCase());
        return matchesFiltro && matchesBusqueda;
    });

    if (loading) {
        return (
            <div className="min-h-screen bg-black flex flex-col items-center justify-center">
                <Loader2 className="w-12 h-12 text-blue-500 animate-spin mb-4" />
                <p className="text-neutral-500 font-bold uppercase tracking-widest text-xs">{t.common.loading}...</p>
            </div>
        );
    }

    if (!auth.isAuthenticated) {
        return (
            <div className="min-h-screen bg-black flex flex-col items-center justify-center">
                <AlertCircle className="w-12 h-12 text-amber-500 mb-4" />
                <p className="text-white font-black text-xl mb-4 uppercase tracking-tighter">
                    {t.messages.loginRequired}
                </p>
                <button
                    onClick={() => auth.signinRedirect()}
                    className="px-8 py-3 bg-white text-black font-black rounded-2xl uppercase tracking-widest text-xs hover:bg-neutral-200 transition-all"
                >
                    {t.nav.login}
                </button>
            </div>
        );
    }

    return (
        <div className="min-h-[calc(100vh-80px)] bg-black text-white p-8">
            <div className="max-w-6xl mx-auto">
                <button
                    onClick={() => navigate('/perfil')}
                    className="flex items-center gap-2 text-neutral-500 hover:text-white transition-colors mb-12 group"
                >
                    <ChevronLeft className="w-5 h-5 group-hover:-translate-x-1 transition-transform" />
                    <span className="font-bold uppercase tracking-widest text-xs">{t.common.back}</span>
                </button>

                <header className="mb-16 relative">
                    <div className="flex items-center gap-2 mb-4">
                        <Sparkles className="w-5 h-5 text-blue-500" />
                        <span className="text-blue-500 font-black text-xs uppercase tracking-[0.3em]">{t.profile.title}</span>
                    </div>
                    <h1 className="text-5xl font-black mb-4 tracking-tighter uppercase">
                        {t.emails.title}
                    </h1>
                    <p className="text-neutral-500 text-lg font-medium max-w-2xl">
                        {t.emails.emailHistory} {email}
                    </p>
                </header>

                {/* Filters and Search */}
                <div className="flex flex-col md:flex-row gap-6 mb-10">
                    <div className="flex-1 relative group">
                        <Search className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-neutral-600 group-focus-within:text-blue-500 transition-colors" />
                        <input
                            type="text"
                            placeholder={t.common.search + '...'}
                            value={busqueda}
                            onChange={(e) => setBusqueda(e.target.value)}
                            className="w-full bg-neutral-900/50 border border-neutral-800 rounded-2xl py-4 pl-12 pr-6 text-white placeholder:text-neutral-600 focus:outline-none focus:border-blue-500 transition-all font-medium"
                        />
                    </div>
                    <div className="flex items-center gap-2 overflow-x-auto pb-2 md:pb-0 no-scrollbar">
                        <div className="flex items-center gap-2 mr-2">
                            <Filter className="w-4 h-4 text-neutral-600" />
                            <span className="text-xs font-bold text-neutral-600 uppercase tracking-widest">{t.common.filter}:</span>
                        </div>
                        <button
                            onClick={() => setFiltroTipo('todos')}
                            className={`px-4 py-2 rounded-xl font-bold text-xs uppercase tracking-widest transition-all ${filtroTipo === 'todos' ? 'bg-white text-black' : 'bg-neutral-900 text-neutral-500 border border-neutral-800'
                                }`}
                        >
                            {t.common.all}
                        </button>
                        <button
                            onClick={() => setFiltroTipo(TipoCorreo.Confirmacion)}
                            className={`px-4 py-2 rounded-xl font-bold text-xs uppercase tracking-widest transition-all ${filtroTipo === TipoCorreo.Confirmacion ? 'bg-green-500 text-white' : 'bg-neutral-900 text-neutral-500 border border-neutral-800'
                                }`}
                        >
                            {t.emails.confirmation}
                        </button>
                        <button
                            onClick={() => setFiltroTipo(TipoCorreo.Recordatorio)}
                            className={`px-4 py-2 rounded-xl font-bold text-xs uppercase tracking-widest transition-all ${filtroTipo === TipoCorreo.Recordatorio ? 'bg-blue-500 text-white' : 'bg-neutral-900 text-neutral-500 border border-neutral-800'
                                }`}
                        >
                            {t.emails.reminder}
                        </button>
                        <button
                            onClick={() => setFiltroTipo(TipoCorreo.Cancelacion)}
                            className={`px-4 py-2 rounded-xl font-bold text-xs uppercase tracking-widest transition-all ${filtroTipo === TipoCorreo.Cancelacion ? 'bg-red-500 text-white' : 'bg-neutral-900 text-neutral-500 border border-neutral-800'
                                }`}
                        >
                            {t.emails.cancellation}
                        </button>
                    </div>
                </div>

                {/* Results Count */}
                <div className="mb-6">
                    <p className="text-neutral-500 text-sm font-bold">
                        {t.common.view} <span className="text-white">{correosFiltrados.length}</span> {t.common.of} <span className="text-white">{correos.length}</span> {t.emails.title.toLowerCase()}
                    </p>
                </div>

                {/* Email List */}
                <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                    {/* Lista de correos */}
                    <div className="space-y-4">
                        {correosFiltrados.length === 0 ? (
                            <div className="text-center py-20 bg-neutral-900/20 rounded-[3rem] border border-neutral-900">
                                <Mail className="w-12 h-12 text-neutral-800 mx-auto mb-4" />
                                <p className="text-neutral-600 font-bold uppercase tracking-widest text-xs">
                                    {busqueda ? t.common.noResults : t.common.noResults}
                                </p>
                            </div>
                        ) : (
                            correosFiltrados.map((correo) => {
                                const TipoIcon = getTipoIcon(correo.tipo);
                                const EstadoIcon = getEstadoIcon(correo.estado);
                                const isSelected = correoSeleccionado?.id === correo.id;

                                return (
                                    <button
                                        key={correo.id}
                                        onClick={() => setCorreoSeleccionado(correo)}
                                        className={`w-full text-left bg-neutral-900/50 border rounded-3xl p-6 transition-all hover:bg-neutral-900/80 ${isSelected ? 'border-blue-500 bg-neutral-900' : 'border-neutral-800'
                                            }`}
                                    >
                                        <div className="flex items-start gap-4">
                                            <div className={`w-12 h-12 rounded-2xl flex items-center justify-center border ${getTipoColor(correo.tipo)}`}>
                                                <TipoIcon className="w-6 h-6" />
                                            </div>

                                            <div className="flex-1 min-w-0">
                                                <div className="flex items-center gap-2 mb-2">
                                                    <h3 className="text-lg font-black text-white truncate">
                                                        {correo.asunto}
                                                    </h3>
                                                    <EstadoIcon className={`w-4 h-4 flex-shrink-0 ${getEstadoColor(correo.estado)}`} />
                                                </div>

                                                <p className="text-sm text-neutral-400 mb-3 line-clamp-2">
                                                    {correo.contenido}
                                                </p>

                                                <div className="flex items-center gap-3 text-xs text-neutral-500">
                                                    <Calendar className="w-3 h-3" />
                                                    <span>{formatFecha(correo.fechaEnvio)}</span>
                                                </div>
                                            </div>
                                        </div>
                                    </button>
                                );
                            })
                        )}
                    </div>

                    {/* Detalle del correo seleccionado */}
                    <div className="sticky top-8">
                        {correoSeleccionado ? (
                            <div className="bg-neutral-900/50 border border-neutral-800 rounded-3xl p-8">
                                <div className="flex items-start justify-between mb-6">
                                    <div className={`w-16 h-16 rounded-2xl flex items-center justify-center border ${getTipoColor(correoSeleccionado.tipo)}`}>
                                        {(() => {
                                            const Icon = getTipoIcon(correoSeleccionado.tipo);
                                            return <Icon className="w-8 h-8" />;
                                        })()}
                                    </div>
                                    <div className="flex items-center gap-2">
                                        {(() => {
                                            const Icon = getEstadoIcon(correoSeleccionado.estado);
                                            return <Icon className={`w-5 h-5 ${getEstadoColor(correoSeleccionado.estado)}`} />;
                                        })()}
                                        <span className={`text-sm font-bold uppercase tracking-widest ${getEstadoColor(correoSeleccionado.estado)}`}>
                                            {correoSeleccionado.estado}
                                        </span>
                                    </div>
                                </div>

                                <h2 className="text-2xl font-black text-white mb-4">
                                    {correoSeleccionado.asunto}
                                </h2>

                                <div className="space-y-4 mb-6">
                                    <div>
                                        <p className="text-xs font-bold text-neutral-600 uppercase tracking-widest mb-1">{t.common.to}:</p>
                                        <p className="text-sm text-neutral-300">{correoSeleccionado.destinatario}</p>
                                    </div>

                                    <div>
                                        <p className="text-xs font-bold text-neutral-600 uppercase tracking-widest mb-1">{t.common.type}:</p>
                                        <span className={`inline-block px-3 py-1 rounded-full text-xs font-black uppercase tracking-widest border ${getTipoColor(correoSeleccionado.tipo)}`}>
                                            {getTipoLabel(correoSeleccionado.tipo)}
                                        </span>
                                    </div>

                                    <div>
                                        <p className="text-xs font-bold text-neutral-600 uppercase tracking-widest mb-1">{t.audit.date}:</p>
                                        <p className="text-sm text-neutral-300">{formatFecha(correoSeleccionado.fechaEnvio)}</p>
                                    </div>

                                    {correoSeleccionado.fechaEntrega && (
                                        <div>
                                            <p className="text-xs font-bold text-neutral-600 uppercase tracking-widest mb-1">{t.emails.delivered}:</p>
                                            <p className="text-sm text-neutral-300">{formatFecha(correoSeleccionado.fechaEntrega)}</p>
                                        </div>
                                    )}

                                    {correoSeleccionado.eventoRelacionado && (
                                        <div>
                                            <p className="text-xs font-bold text-neutral-600 uppercase tracking-widest mb-1">{t.events.event}:</p>
                                            <p className="text-sm text-neutral-300">{correoSeleccionado.eventoRelacionado}</p>
                                        </div>
                                    )}
                                </div>

                                <div className="p-4 bg-neutral-800/50 rounded-2xl border border-neutral-700">
                                    <p className="text-xs font-bold text-neutral-600 uppercase tracking-widest mb-3">{t.emails.content}:</p>
                                    <p className="text-sm text-neutral-300 leading-relaxed">
                                        {correoSeleccionado.contenido}
                                    </p>
                                </div>
                            </div>
                        ) : (
                            <div className="bg-neutral-900/20 border border-neutral-900 rounded-3xl p-12 text-center">
                                <AlertCircle className="w-12 h-12 text-neutral-800 mx-auto mb-4" />
                                <p className="text-neutral-600 font-bold uppercase tracking-widest text-xs">
                                    {t.logs.details}
                                </p>
                            </div>
                        )}
                    </div>
                </div>
            </div>
        </div>
    );
};
