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
    Rocket
} from 'lucide-react';
import { useAuth } from 'react-oidc-context';
import { adminEventosService } from '../services/admin.eventos.service';
import { eventosService } from '../../eventos/services/eventos.service';
import { Evento } from '../../eventos/types/evento.types';
import EventForm from '../components/EventForm';
import { getUserRoles } from '../../../lib/auth-utils';
import SeatConfigurator from '../components/SeatConfigurator';
import { LayoutGrid, Tag } from 'lucide-react';
import AdminCuponesManager from '../components/AdminCuponesManager';

export default function AdminEventos() {
    const auth = useAuth();
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

    useEffect(() => {
        cargarEventos();
    }, []);

    const cargarEventos = async () => {
        try {
            setLoading(true);

            // Debug: ver información del usuario autenticado
            console.log('👤 Usuario autenticado:', {
                isAuthenticated: auth.isAuthenticated,
                userId: userId,
                username: auth.user?.profile.preferred_username,
                roles: roles,
                isAdmin: isAdmin
            });

            const data = await eventosService.getEventos();
            console.log('📋 Eventos cargados:', data.length, data);
            setEventos(data);
        } catch (err: any) {
            console.error('No se pudieron cargar los eventos', err);
        } finally {
            setLoading(false);
        }
    };

    const handleDelete = async (id: string) => {
        if (confirm('¿Estás seguro de que deseas eliminar este evento? Esta acción no se puede deshacer.')) {
            try {
                await adminEventosService.deleteEvento(id);
                cargarEventos();
            } catch (error) {
                console.error('Error al eliminar evento', error);
                alert('No se pudo eliminar el evento');
            }
        }
    };

    const handlePublish = async (id: string) => {
        if (confirm('¿Deseas publicar este evento? Una vez publicado será visible para todos los usuarios.')) {
            try {
                await adminEventosService.publicarEvento(id);
                cargarEventos();
            } catch (error) {
                console.error('Error al publicar evento', error);
                alert('No se pudo publicar el evento');
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

    const handleFormSuccess = () => {
        setIsModalOpen(false);
        cargarEventos();
    };

    const filteredEventos = eventos.filter(e => {
        const matchesSearch = e.titulo.toLowerCase().includes(searchTerm.toLowerCase()) ||
            e.lugar.toLowerCase().includes(searchTerm.toLowerCase());

        if (!matchesSearch) return false;
        if (isAdmin) return true;

        // Debug: ver qué se está comparando
        console.log('🔍 Comparando:', {
            eventoId: e.id,
            eventoTitulo: e.titulo,
            organizadorId: e.organizadorId,
            userId: userId,
            match: e.organizadorId === userId
        });

        return e.organizadorId === userId;
    });

    const getStatusBadge = (estado: string) => {
        const status = (estado || '').toLowerCase();
        if (status === 'publicado') return (
            <span className="flex items-center gap-1.5 px-2.5 py-1 rounded-full bg-emerald-500/10 text-emerald-500 text-[10px] font-black uppercase tracking-wider border border-emerald-500/20">
                <CheckCircle2 className="w-3 h-3" /> Publicado
            </span>
        );
        if (status === 'borrador') return (
            <span className="flex items-center gap-1.5 px-2.5 py-1 rounded-full bg-amber-500/10 text-amber-500 text-[10px] font-black uppercase tracking-wider border border-amber-500/20">
                <Clock className="w-3 h-3" /> Borrador
            </span>
        );
        return (
            <span className="flex items-center gap-1.5 px-2.5 py-1 rounded-full bg-slate-500/10 text-slate-400 text-[10px] font-black uppercase tracking-wider border border-slate-500/20">
                <AlertCircle className="w-3 h-3" /> {estado || 'Desconocido'}
            </span>
        );
    };

    return (
        <div className="space-y-6">
            {/* Header */}
            <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
                <div>
                    <h1 className="text-3xl font-black text-white">Gestión de Eventos</h1>
                    <p className="text-slate-400 text-sm mt-1">Crea, edita y organiza todos tus eventos desde aquí.</p>
                </div>
                <button
                    onClick={handleCreate}
                    className="flex items-center gap-2 bg-blue-600 hover:bg-blue-700 text-white px-5 py-3 rounded-xl font-bold shadow-lg shadow-blue-600/20 transition-all hover:scale-[1.03] active:scale-[0.98]"
                >
                    <Plus className="w-5 h-5" />
                    <span>Crear Nuevo Evento</span>
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
                            placeholder="Buscar por título o lugar..."
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                            className="w-full bg-[#0f1115] border border-slate-700 rounded-xl pl-10 pr-4 py-2.5 text-sm text-slate-300 outline-none focus:border-blue-500/50 transition-all"
                        />
                    </div>
                    <div className="flex items-center gap-2">
                        <button className="flex items-center gap-2 px-4 py-2.5 bg-[#0f1115] border border-slate-700 rounded-xl text-xs font-bold text-slate-400 hover:text-white transition-all hover:border-slate-600">
                            <Filter className="w-4 h-4" /> Filtros
                        </button>
                    </div>
                </div>

                {/* Table Content */}
                <div className="overflow-x-auto">
                    <table className="w-full text-left border-collapse">
                        <thead>
                            <tr className="bg-slate-900/60">
                                <th className="px-6 py-4 text-[11px] font-black text-slate-500 uppercase tracking-widest border-b border-slate-800">Evento</th>
                                <th className="px-6 py-4 text-[11px] font-black text-slate-500 uppercase tracking-widest border-b border-slate-800">Fecha y Hora</th>
                                <th className="px-6 py-4 text-[11px] font-black text-slate-500 uppercase tracking-widest border-b border-slate-800">Ubicación</th>
                                <th className="px-6 py-4 text-[11px] font-black text-slate-500 uppercase tracking-widest border-b border-slate-800 text-center">Estado</th>
                                <th className="px-6 py-4 text-[11px] font-black text-slate-500 uppercase tracking-widest border-b border-slate-800 text-right">Acciones</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-slate-800/50">
                            {loading ? (
                                <tr>
                                    <td colSpan={5} className="px-6 py-20 text-center">
                                        <div className="inline-block animate-spin rounded-full h-8 w-8 border-2 border-blue-500 border-t-transparent" />
                                        <p className="mt-4 text-slate-500 font-medium">Cargando catálogo...</p>
                                    </td>
                                </tr>
                            ) : filteredEventos.length === 0 ? (
                                <tr>
                                    <td colSpan={5} className="px-6 py-20 text-center text-slate-500 font-medium">
                                        No se encontraron eventos con los criterios de búsqueda.
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
                                                <span className="text-sm font-semibold">{new Date(evento.fechaInicio).toLocaleDateString()}</span>
                                            </div>
                                            <div className="text-[10px] text-slate-500 font-bold ml-6">
                                                {new Date(evento.fechaInicio).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
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
                                                    title="Editar"
                                                >
                                                    <Edit2 className="w-4 h-4" />
                                                </button>
                                                <button
                                                    onClick={() => handleDelete(evento.id)}
                                                    className="p-2 bg-slate-800 text-slate-300 hover:text-white hover:bg-rose-600 rounded-lg transition-all"
                                                    title="Eliminar"
                                                >
                                                    <Trash2 className="w-4 h-4" />
                                                </button>
                                                <button
                                                    onClick={() => handleManageSeats(evento)}
                                                    className="p-2 bg-slate-800 text-slate-300 hover:text-white hover:bg-emerald-600 rounded-lg transition-all"
                                                    title="Gestionar Asientos"
                                                >
                                                    <LayoutGrid className="w-4 h-4" />
                                                </button>
                                                <button
                                                    onClick={() => handleManageCoupons(evento)}
                                                    className="p-2 bg-slate-800 text-slate-300 hover:text-white hover:bg-purple-600 rounded-lg transition-all"
                                                    title="Gestionar Cupones"
                                                >
                                                    <Tag className="w-4 h-4" />
                                                </button>
                                                {evento.estado !== 'Publicado' && (
                                                    <button
                                                        onClick={() => handlePublish(evento.id)}
                                                        className="p-2 bg-slate-800 text-slate-300 hover:text-white hover:bg-amber-500 rounded-lg transition-all"
                                                        title="Publicar Evento"
                                                    >
                                                        <Rocket className="w-4 h-4" />
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
                            Mostrando {filteredEventos.length} de {eventos.length} eventos
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
        </div>
    );
}
