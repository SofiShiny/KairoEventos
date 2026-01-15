import { useState, useEffect } from 'react';
import {
    Plus,
    Search,
    Filter,
    Edit2,
    Trash2,
    MoreHorizontal,
    ChevronLeft,
    ChevronRight,
    Calendar,
    MapPin,
    CheckCircle2,
    Clock,
    AlertCircle,
    Rocket,
    BarChart3,
    LayoutGrid,
    Tag
} from 'lucide-react';
import { useAuth } from 'react-oidc-context';
import { adminEventosService } from '../services/admin.eventos.service';
import { eventosService } from '../../eventos/services/eventos.service';
import { Evento } from '../../eventos/types/evento.types';
import EventForm from '../components/EventForm';
import { getUserRoles } from '../../../lib/auth-utils';
import SeatConfigurator from '../components/SeatConfigurator';
import AdminCuponesManager from '../components/AdminCuponesManager';
import AdminEncuestasManager from '../components/AdminEncuestasManager';

import { useT } from '../../../i18n';

export default function AdminEventos() {
    const auth = useAuth();
    const t = useT();
    const [eventos, setEventos] = useState<Evento[]>([]);
    const [loading, setLoading] = useState(true);
    const [searchTerm, setSearchTerm] = useState('');

    const roles = getUserRoles(auth.user);
    const isAdmin = roles.includes('admin');
    const userId = auth.user?.profile.sub;

    // Modal states
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [selectedEvento, setSelectedEvento] = useState<Evento | null>(null);
    const [showSeatConfig, setShowSeatConfig] = useState(false);
    const [showCuponesManager, setShowCuponesManager] = useState(false);
    const [showEncuestasManager, setShowEncuestasManager] = useState(false);

    useEffect(() => {
        cargarEventos();
    }, []);

    const cargarEventos = async () => {
        try {
            setLoading(true);
            const data = await eventosService.getEventos();
            setEventos(data);
        } catch (err: any) {
            console.error('No se pudieron cargar los eventos', err);
        } finally {
            setLoading(false);
        }
    };

    const handleDelete = async (id: string) => {
        if (confirm(t.adminEvents.dialogs.deleteConfirm)) {
            try {
                await adminEventosService.deleteEvento(id);
                cargarEventos();
            } catch (error) {
                console.error('Error al eliminar evento', error);
                alert(t.adminEvents.messages.deleteError);
            }
        }
    };

    const handlePublish = async (id: string) => {
        if (confirm(t.adminEvents.dialogs.publishConfirm)) {
            try {
                await adminEventosService.publicarEvento(id);
                cargarEventos();
            } catch (error) {
                console.error('Error al publicar evento', error);
                alert(t.adminEvents.messages.publishError);
            }
        }
    };

    const handleFinish = async (id: string) => {
        if (confirm('¿Estás seguro de que deseas marcar este evento como finalizado? Esto permitirá acceder a foros y encuestas.')) {
            try {
                await adminEventosService.finalizarEvento(id);
                cargarEventos();
                alert('Evento finalizado exitosamente');
            } catch (error) {
                console.error('Error al finalizar evento', error);
                alert('Error al finalizar el evento');
            }
        }
    };

    const handleEdit = (evento: Evento) => {
        setSelectedEvento(evento);
        setIsModalOpen(true);
    };

    const handleCreate = () => {
        setSelectedEvento(null);
        setIsModalOpen(true);
    };

    const handleManageSeats = (evento: Evento) => {
        setSelectedEvento(evento);
        setShowSeatConfig(true);
    };

    const handleManageCoupons = (evento: Evento) => {
        setSelectedEvento(evento);
        setShowCuponesManager(true);
    };

    const handleManageEncuestas = (evento: Evento) => {
        setSelectedEvento(evento);
        setShowEncuestasManager(true);
    };

    const handleFormSuccess = () => {
        setIsModalOpen(false);
        cargarEventos();
    };

    const formatFecha = (fecha: string) => {
        const date = new Date(fecha);
        return date.toLocaleDateString(document.documentElement.lang === 'es' ? 'es-ES' : 'en-US', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric'
        });
    };

    const formatHora = (fecha: string) => {
        const date = new Date(fecha);
        return date.toLocaleTimeString(document.documentElement.lang === 'es' ? 'es-ES' : 'en-US', {
            hour: '2-digit',
            minute: '2-digit'
        });
    };

    const filteredEventos = eventos.filter(e => {
        const matchesSearch = e.titulo.toLowerCase().includes(searchTerm.toLowerCase()) ||
            e.lugar.toLowerCase().includes(searchTerm.toLowerCase());

        if (!matchesSearch) return false;
        if (isAdmin) return true;

        return e.organizadorId === userId;
    });

    const getStatusBadge = (estado: string) => {
        const status = (estado || '').toLowerCase();
        if (status === 'publicado') return (
            <span className="flex items-center gap-1.5 px-2.5 py-1 rounded-full bg-emerald-500/10 text-emerald-500 text-[10px] font-black uppercase tracking-wider border border-emerald-500/20">
                <CheckCircle2 className="w-3 h-3" /> {t.adminEvents.status.published}
            </span>
        );
        if (status === 'borrador') return (
            <span className="flex items-center gap-1.5 px-2.5 py-1 rounded-full bg-amber-500/10 text-amber-500 text-[10px] font-black uppercase tracking-wider border border-amber-500/20">
                <Clock className="w-3 h-3" /> {t.adminEvents.status.draft}
            </span>
        );
        return (
            <span className="flex items-center gap-1.5 px-2.5 py-1 rounded-full bg-slate-500/10 text-slate-400 text-[10px] font-black uppercase tracking-wider border border-slate-500/20">
                <AlertCircle className="w-3 h-3" /> {estado || t.adminEvents.status.unknown}
            </span>
        );
    };

    return (
        <div className="space-y-6">
            {/* Header */}
            <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
                <div>
                    <h1 className="text-3xl font-black text-white">{t.adminEvents.title}</h1>
                    <p className="text-slate-400 text-sm mt-1">{t.adminEvents.description}</p>
                </div>
                <button
                    onClick={handleCreate}
                    className="flex items-center gap-2 bg-blue-600 hover:bg-blue-700 text-white px-5 py-3 rounded-xl font-bold shadow-lg shadow-blue-600/20 transition-all hover:scale-[1.03] active:scale-[0.98]"
                >
                    <Plus className="w-5 h-5" />
                    <span>{t.adminEvents.createNew}</span>
                </button>
            </div>

            {/* Table Container */}
            <div className="bg-[#16191f] border border-slate-800 rounded-2xl shadow-xl overflow-hidden">
                {/* Table Filters */}
                <div className="p-4 border-b border-slate-800 flex flex-col md:flex-row justify-between gap-4 bg-slate-900/40">
                    <div className="relative flex-1 max-w-md">
                        <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-500" />
                        <input
                            type="text"
                            placeholder={t.adminEvents.searchPlaceholder}
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                            className="w-full bg-[#0f1115] border border-slate-700 rounded-xl pl-10 pr-4 py-2.5 text-sm text-slate-300 outline-none focus:border-blue-500/50 transition-all"
                        />
                    </div>
                    <div className="flex items-center gap-2">
                        <button className="flex items-center gap-2 px-4 py-2.5 bg-[#0f1115] border border-slate-700 rounded-xl text-xs font-bold text-slate-400 hover:text-white transition-all hover:border-slate-600">
                            <Filter className="w-4 h-4" /> {t.adminEvents.filters}
                        </button>
                    </div>
                </div>

                {/* Table Content */}
                <div className="overflow-x-auto">
                    <table className="w-full text-left border-collapse">
                        <thead>
                            <tr className="bg-slate-900/60">
                                <th className="px-6 py-4 text-[11px] font-black text-slate-500 uppercase tracking-widest border-b border-slate-800">{t.adminEvents.table.event}</th>
                                <th className="px-6 py-4 text-[11px] font-black text-slate-500 uppercase tracking-widest border-b border-slate-800">{t.adminEvents.table.dateTime}</th>
                                <th className="px-6 py-4 text-[11px] font-black text-slate-500 uppercase tracking-widest border-b border-slate-800">{t.adminEvents.table.location}</th>
                                <th className="px-6 py-4 text-[11px] font-black text-slate-500 uppercase tracking-widest border-b border-slate-800 text-center">{t.adminEvents.table.status}</th>
                                <th className="px-6 py-4 text-[11px] font-black text-slate-500 uppercase tracking-widest border-b border-slate-800 text-right">{t.adminEvents.table.actions}</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-slate-800/50">
                            {loading ? (
                                <tr>
                                    <td colSpan={5} className="px-6 py-20 text-center">
                                        <div className="inline-block animate-spin rounded-full h-8 w-8 border-2 border-blue-500 border-t-transparent" />
                                        <p className="mt-4 text-slate-500 font-medium">{t.adminEvents.messages.loading}</p>
                                    </td>
                                </tr>
                            ) : filteredEventos.length === 0 ? (
                                <tr>
                                    <td colSpan={5} className="px-6 py-20 text-center text-slate-500 font-medium">
                                        {t.adminEvents.messages.noResults}
                                    </td>
                                </tr>
                            ) : (
                                filteredEventos.map((evento) => (
                                    <tr key={evento.id} className="hover:bg-slate-800/20 transition-colors group">
                                        <td className="px-6 py-4">
                                            <div className="flex items-center gap-4">
                                                <div className="w-12 h-12 rounded-lg overflow-hidden border border-slate-700 flex-shrink-0 group-hover:scale-110 transition-transform">
                                                    <img
                                                        src={evento.imagenUrl || 'https://via.placeholder.com/150'}
                                                        alt={evento.titulo}
                                                        className="w-full h-full object-cover"
                                                    />
                                                </div>
                                                <div>
                                                    <p className="text-sm font-bold text-white group-hover:text-blue-400 transition-colors">{evento.titulo}</p>
                                                    <p className="text-[11px] text-slate-500 font-bold uppercase tracking-tighter mt-0.5">{evento.categoria}</p>
                                                </div>
                                            </div>
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap">
                                            <div className="flex items-center gap-2 text-slate-300">
                                                <Calendar className="w-4 h-4 text-slate-500" />
                                                <span className="text-sm font-semibold">{formatFecha(evento.fechaInicio)}</span>
                                            </div>
                                            <div className="text-[10px] text-slate-500 font-bold ml-6">
                                                {formatHora(evento.fechaInicio)}
                                            </div>
                                        </td>
                                        <td className="px-6 py-4">
                                            <div className="flex items-center gap-2 text-slate-300">
                                                <MapPin className="w-4 h-4 text-slate-500" />
                                                <span className="text-sm font-semibold truncate max-w-[150px]">{evento.lugar}</span>
                                            </div>
                                        </td>
                                        <td className="px-6 py-4">
                                            <div className="flex justify-center">
                                                {getStatusBadge(evento.estado)}
                                            </div>
                                        </td>
                                        <td className="px-6 py-4 text-right">
                                            <div className="flex justify-end gap-2 opacity-0 group-hover:opacity-100 transition-opacity">
                                                <button
                                                    onClick={() => handleEdit(evento)}
                                                    className="p-2 bg-slate-800 text-slate-300 hover:text-white hover:bg-blue-600 rounded-lg transition-all"
                                                    title={t.common.edit}
                                                >
                                                    <Edit2 className="w-4 h-4" />
                                                </button>
                                                <button
                                                    onClick={() => handleDelete(evento.id)}
                                                    className="p-2 bg-slate-800 text-slate-300 hover:text-white hover:bg-rose-600 rounded-lg transition-all"
                                                    title={t.common.delete}
                                                >
                                                    <Trash2 className="w-4 h-4" />
                                                </button>
                                                <button
                                                    onClick={() => handleManageSeats(evento)}
                                                    className="p-2 bg-slate-800 text-slate-300 hover:text-white hover:bg-emerald-600 rounded-lg transition-all"
                                                    title={t.tickets.seat}
                                                >
                                                    <LayoutGrid className="w-4 h-4" />
                                                </button>
                                                <button
                                                    onClick={() => handleManageCoupons(evento)}
                                                    className="p-2 bg-slate-800 text-slate-300 hover:text-white hover:bg-purple-600 rounded-lg transition-all"
                                                    title="Cupones"
                                                >
                                                    <Tag className="w-4 h-4" />
                                                </button>
                                                <button
                                                    onClick={() => handleManageEncuestas(evento)}
                                                    className="p-2 bg-slate-800 text-slate-300 hover:text-white hover:bg-blue-500 rounded-lg transition-all"
                                                    title="Encuestas"
                                                >
                                                    <BarChart3 className="w-4 h-4" />
                                                </button>
                                                {evento.estado !== 'Publicado' && (
                                                    <button
                                                        onClick={() => handlePublish(evento.id)}
                                                        className="p-2 bg-slate-800 text-slate-300 hover:text-white hover:bg-amber-500 rounded-lg transition-all"
                                                        title="Publicar"
                                                    >
                                                        <Rocket className="w-4 h-4" />
                                                    </button>
                                                )}
                                                {evento.estado === 'Publicado' && (
                                                    <button
                                                        onClick={() => handleFinish(evento.id)}
                                                        className="p-2 bg-slate-800 text-slate-300 hover:text-white hover:bg-green-600 rounded-lg transition-all"
                                                        title="Finalizar Evento"
                                                    >
                                                        <CheckCircle2 className="w-4 h-4" />
                                                    </button>
                                                )}
                                                <button className="p-2 bg-slate-800 text-slate-300 hover:text-white hover:bg-slate-700 rounded-lg transition-all">
                                                    <MoreHorizontal className="w-4 h-4" />
                                                </button>
                                            </div>
                                        </td>
                                    </tr>
                                ))
                            )}
                        </tbody>
                    </table>
                </div>

                {/* Pagination Info */}
                {!loading && filteredEventos.length > 0 && (
                    <div className="px-6 py-4 bg-slate-900/40 border-t border-slate-800 flex items-center justify-between">
                        <p className="text-xs text-slate-500 font-bold uppercase tracking-wider">
                            {t.common.view} {filteredEventos.length} {t.common.of} {eventos.length} {t.events.title.toLowerCase()}
                        </p>
                        <div className="flex gap-2">
                            <button className="p-1.5 border border-slate-800 rounded-lg text-slate-600 hover:text-white transition-colors disabled:opacity-30" disabled>
                                <ChevronLeft className="w-4 h-4" />
                            </button>
                            <button className="p-1.5 border border-slate-800 rounded-lg text-slate-400 hover:text-white transition-colors">
                                <ChevronRight className="w-4 h-4" />
                            </button>
                        </div>
                    </div>
                )}
            </div>

            {/* Render Modal */}
            {isModalOpen && (
                <EventForm
                    evento={selectedEvento || undefined}
                    onSuccess={handleFormSuccess}
                    onCancel={() => setIsModalOpen(false)}
                />
            )}
            {showSeatConfig && selectedEvento && (
                <SeatConfigurator
                    evento={selectedEvento}
                    onClose={() => {
                        setShowSeatConfig(false);
                        setSelectedEvento(null);
                    }}
                />
            )}

            {/* Modal de Gestión de Cupones */}
            {showCuponesManager && selectedEvento && (
                <div className="fixed inset-0 bg-black/80 backdrop-blur-sm flex items-center justify-center z-50 p-4">
                    <div className="bg-neutral-900 rounded-2xl border border-neutral-800 w-full max-w-6xl max-h-[90vh] overflow-y-auto">
                        {/* Header del Modal */}
                        <div className="sticky top-0 bg-neutral-900 border-b border-neutral-800 p-6 flex items-center justify-between z-10">
                            <div>
                                <h2 className="text-2xl font-bold text-white flex items-center gap-3">
                                    <Tag className="w-6 h-6 text-purple-400" />
                                    Gestión de Cupones
                                </h2>
                                <p className="text-sm text-neutral-400 mt-1">
                                    Evento: <span className="text-white font-semibold">{selectedEvento.titulo}</span>
                                </p>
                            </div>
                            <button
                                onClick={() => {
                                    setShowCuponesManager(false);
                                    setSelectedEvento(null);
                                }}
                                className="p-2 hover:bg-neutral-800 rounded-lg transition-colors text-neutral-400 hover:text-white"
                            >
                                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                                </svg>
                            </button>
                        </div>

                        {/* Contenido del Modal */}
                        <div className="p-6">
                            <AdminCuponesManager eventoId={selectedEvento.id} />
                        </div>
                    </div>
                </div>
            )}

            {/* Modal de Gestión de Encuestas */}
            {showEncuestasManager && selectedEvento && (
                <div className="fixed inset-0 bg-black/80 backdrop-blur-sm flex items-center justify-center z-50 p-4">
                    <div className="bg-neutral-900 rounded-2xl border border-neutral-800 w-full max-w-4xl max-h-[90vh] overflow-y-auto">
                        {/* Header del Modal */}
                        <div className="sticky top-0 bg-neutral-900 border-b border-neutral-800 p-6 flex items-center justify-between z-10">
                            <div>
                                <h2 className="text-2xl font-bold text-white flex items-center gap-3">
                                    <BarChart3 className="w-6 h-6 text-blue-400" />
                                    Configuración de Encuesta
                                </h2>
                                <p className="text-sm text-neutral-400 mt-1">
                                    Define las preguntas para los asistentes de <span className="text-white font-semibold">{selectedEvento.titulo}</span>
                                </p>
                            </div>
                            <button
                                onClick={() => {
                                    setShowEncuestasManager(false);
                                    setSelectedEvento(null);
                                }}
                                className="p-2 hover:bg-neutral-800 rounded-lg transition-colors text-neutral-400 hover:text-white"
                            >
                                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                                </svg>
                            </button>
                        </div>

                        {/* Contenido del Modal */}
                        <div className="p-6">
                            <AdminEncuestasManager eventoId={selectedEvento.id} />
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}
