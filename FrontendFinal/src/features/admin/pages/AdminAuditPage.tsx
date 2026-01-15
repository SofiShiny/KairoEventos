import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import {
    Activity,
    ChevronLeft,
    Sparkles,
    Loader2,
    ShoppingCart,
    CreditCard,
    Ticket,
    Calendar,
    CheckCircle2,
    XCircle,
    Clock,
    Filter,
    User,
    Search
} from 'lucide-react';
import { entradasService, Entrada } from '../../entradas/services/entradas.service';
import { toast } from 'react-hot-toast';

interface AuditAction {
    id: string;
    usuarioId: string;
    usuarioNombre: string;
    tipo: 'compra' | 'pago' | 'cancelacion' | 'uso';
    descripcion: string;
    fecha: string;
    estado: 'exitoso' | 'fallido' | 'pendiente';
    detalles: string;
    icono: any;
    color: string;
}

export const AdminAuditPage = () => {
    const navigate = useNavigate();

    const [loading, setLoading] = useState(true);
    const [acciones, setAcciones] = useState<AuditAction[]>([]);
    const [filtro, setFiltro] = useState<'todos' | 'exitoso' | 'fallido' | 'pendiente'>('todos');
    const [busqueda, setBusqueda] = useState('');

    useEffect(() => {
        loadAllAuditHistory();
    }, []);

    const loadAllAuditHistory = async () => {
        try {
            setLoading(true);
            // Obtener TODAS las entradas del sistema (sin filtro de usuario)
            const entradas = await entradasService.getTodasLasEntradas();

            // Convertir entradas a acciones de auditoría
            const accionesGeneradas: AuditAction[] = [];

            entradas.forEach((entrada: Entrada) => {
                // Acción de compra
                accionesGeneradas.push({
                    id: `compra-${entrada.id}`,
                    usuarioId: entrada.usuarioId,
                    usuarioNombre: entrada.nombreUsuario || entrada.emailUsuario || `Usuario ${entrada.usuarioId.substring(0, 8)}`,
                    tipo: 'compra',
                    descripcion: `Compra de entrada para ${entrada.eventoNombre}`,
                    fecha: entrada.fechaCompra,
                    estado: entrada.estado === 'Pagada' ? 'exitoso' :
                        entrada.estado === 'Cancelada' ? 'fallido' : 'pendiente',
                    detalles: `Asiento: ${entrada.asientoInfo} - Monto: $${entrada.precio}`,
                    icono: ShoppingCart,
                    color: 'blue'
                });

                // Acción de pago si está pagada
                if (entrada.estado === 'Pagada') {
                    accionesGeneradas.push({
                        id: `pago-${entrada.id}`,
                        usuarioId: entrada.usuarioId,
                        usuarioNombre: entrada.nombreUsuario || entrada.emailUsuario || `Usuario ${entrada.usuarioId.substring(0, 8)}`,
                        tipo: 'pago',
                        descripcion: `Pago procesado para ${entrada.eventoNombre}`,
                        fecha: entrada.fechaCompra,
                        estado: 'exitoso',
                        detalles: `Método: Tarjeta - Monto: $${entrada.precio}`,
                        icono: CreditCard,
                        color: 'green'
                    });
                }

                // Acción de uso si está usada
                if (entrada.estado === 'Usada') {
                    accionesGeneradas.push({
                        id: `uso-${entrada.id}`,
                        usuarioId: entrada.usuarioId,
                        usuarioNombre: entrada.nombreUsuario || entrada.emailUsuario || `Usuario ${entrada.usuarioId.substring(0, 8)}`,
                        tipo: 'uso',
                        descripcion: `Entrada utilizada en ${entrada.eventoNombre}`,
                        fecha: entrada.fechaEvento || entrada.fechaCompra,
                        estado: 'exitoso',
                        detalles: `Código QR: ${entrada.codigoQr}`,
                        icono: Ticket,
                        color: 'purple'
                    });
                }

                // Acción de cancelación si está cancelada
                if (entrada.estado === 'Cancelada') {
                    accionesGeneradas.push({
                        id: `cancel-${entrada.id}`,
                        usuarioId: entrada.usuarioId,
                        usuarioNombre: entrada.nombreUsuario || entrada.emailUsuario || `Usuario ${entrada.usuarioId.substring(0, 8)}`,
                        tipo: 'cancelacion',
                        descripcion: `Cancelación de entrada para ${entrada.eventoNombre}`,
                        fecha: entrada.fechaCompra,
                        estado: 'fallido',
                        detalles: `Reembolso procesado: $${entrada.precio}`,
                        icono: XCircle,
                        color: 'red'
                    });
                }
            });

            // Ordenar por fecha descendente
            accionesGeneradas.sort((a, b) =>
                new Date(b.fecha).getTime() - new Date(a.fecha).getTime()
            );

            setAcciones(accionesGeneradas);
        } catch (error) {
            console.error('Error cargando historial:', error);
            toast.error('Error al cargar el historial de auditoría');
        } finally {
            setLoading(false);
        }
    };

    const formatFecha = (fecha: string) => {
        const date = new Date(fecha);
        return date.toLocaleDateString('es-ES', {
            day: '2-digit',
            month: 'long',
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    };

    const accionesFiltradas = acciones
        .filter(a => filtro === 'todos' || a.estado === filtro)
        .filter(a =>
            busqueda === '' ||
            a.descripcion.toLowerCase().includes(busqueda.toLowerCase()) ||
            a.usuarioNombre.toLowerCase().includes(busqueda.toLowerCase()) ||
            a.detalles.toLowerCase().includes(busqueda.toLowerCase())
        );

    const getEstadoIcon = (estado: string) => {
        switch (estado) {
            case 'exitoso':
                return <CheckCircle2 className="w-5 h-5 text-green-500" />;
            case 'fallido':
                return <XCircle className="w-5 h-5 text-red-500" />;
            case 'pendiente':
                return <Clock className="w-5 h-5 text-yellow-500" />;
            default:
                return null;
        }
    };

    const getColorClasses = (color: string) => {
        const colors: Record<string, string> = {
            blue: 'bg-blue-500/10 border-blue-500/20 text-blue-400',
            green: 'bg-green-500/10 border-green-500/20 text-green-400',
            purple: 'bg-purple-500/10 border-purple-500/20 text-purple-400',
            red: 'bg-red-500/10 border-red-500/20 text-red-400',
            yellow: 'bg-yellow-500/10 border-yellow-500/20 text-yellow-400'
        };
        return colors[color] || colors.blue;
    };

    if (loading) {
        return (
            <div className="min-h-screen bg-black flex flex-col items-center justify-center">
                <Loader2 className="w-12 h-12 text-blue-500 animate-spin mb-4" />
                <p className="text-neutral-500 font-bold uppercase tracking-widest text-xs">Cargando auditoría del sistema...</p>
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
                    <span className="font-bold uppercase tracking-widest text-xs">Volver al panel</span>
                </button>

                <header className="mb-16 relative">
                    <div className="absolute -top-20 -left-20 w-64 h-64 bg-orange-600/10 blur-[120px] rounded-full pointer-events-none" />
                    <div className="flex items-center gap-2 mb-4">
                        <Sparkles className="w-5 h-5 text-orange-500" />
                        <span className="text-orange-500 font-black text-xs uppercase tracking-[0.3em]">Administración</span>
                    </div>
                    <h1 className="text-5xl font-black mb-4 tracking-tighter uppercase">
                        AUDITORÍA DEL SISTEMA
                    </h1>
                    <p className="text-neutral-500 text-lg font-medium max-w-2xl">
                        Registro completo de todas las acciones y transacciones de todos los usuarios en la plataforma.
                    </p>
                </header>

                {/* Search and Filters */}
                <div className="mb-10 space-y-6">
                    {/* Search Bar */}
                    <div className="relative">
                        <Search className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-neutral-600" />
                        <input
                            type="text"
                            value={busqueda}
                            onChange={(e) => setBusqueda(e.target.value)}
                            placeholder="Buscar por usuario, evento o detalles..."
                            className="w-full bg-neutral-900 border border-neutral-800 rounded-2xl pl-12 pr-4 py-4 text-white placeholder-neutral-600 focus:ring-2 focus:ring-orange-500 focus:border-transparent transition-all"
                        />
                    </div>

                    {/* Filters */}
                    <div className="flex flex-wrap gap-4">
                        <button
                            onClick={() => setFiltro('todos')}
                            className={`px-6 py-3 rounded-2xl font-black text-xs uppercase tracking-widest transition-all ${filtro === 'todos' ? 'bg-white text-black' : 'bg-neutral-900 text-neutral-500 border border-neutral-800'
                                }`}
                        >
                            <Filter className="w-4 h-4 inline mr-2" />
                            Todos ({acciones.length})
                        </button>
                        <button
                            onClick={() => setFiltro('exitoso')}
                            className={`px-6 py-3 rounded-2xl font-black text-xs uppercase tracking-widest transition-all ${filtro === 'exitoso' ? 'bg-green-500 text-white' : 'bg-neutral-900 text-neutral-500 border border-neutral-800'
                                }`}
                        >
                            Exitosos ({acciones.filter(a => a.estado === 'exitoso').length})
                        </button>
                        <button
                            onClick={() => setFiltro('pendiente')}
                            className={`px-6 py-3 rounded-2xl font-black text-xs uppercase tracking-widest transition-all ${filtro === 'pendiente' ? 'bg-yellow-500 text-black' : 'bg-neutral-900 text-neutral-500 border border-neutral-800'
                                }`}
                        >
                            Pendientes ({acciones.filter(a => a.estado === 'pendiente').length})
                        </button>
                        <button
                            onClick={() => setFiltro('fallido')}
                            className={`px-6 py-3 rounded-2xl font-black text-xs uppercase tracking-widest transition-all ${filtro === 'fallido' ? 'bg-red-500 text-white' : 'bg-neutral-900 text-neutral-500 border border-neutral-800'
                                }`}
                        >
                            Fallidos ({acciones.filter(a => a.estado === 'fallido').length})
                        </button>
                    </div>
                </div>

                {/* Results Count */}
                <div className="mb-6 flex items-center justify-between">
                    <p className="text-neutral-500 text-sm font-bold">
                        Mostrando <span className="text-white">{accionesFiltradas.length}</span> de <span className="text-white">{acciones.length}</span> registros
                    </p>
                </div>

                {/* Timeline */}
                <div className="space-y-6">
                    {accionesFiltradas.length === 0 ? (
                        <div className="text-center py-20 bg-neutral-900/20 rounded-[3rem] border border-neutral-900">
                            <Activity className="w-12 h-12 text-neutral-800 mx-auto mb-4" />
                            <p className="text-neutral-600 font-bold uppercase tracking-widest text-xs">
                                {busqueda ? 'No se encontraron resultados' : 'No hay actividad registrada'}
                            </p>
                        </div>
                    ) : (
                        accionesFiltradas.map((accion, index) => {
                            const Icon = accion.icono;
                            return (
                                <div
                                    key={accion.id}
                                    className="relative bg-neutral-900/50 border border-neutral-800 rounded-3xl p-6 hover:bg-neutral-900/80 transition-all group"
                                >
                                    {/* Timeline line */}
                                    {index !== accionesFiltradas.length - 1 && (
                                        <div className="absolute left-[52px] top-[80px] w-0.5 h-[calc(100%+24px)] bg-neutral-800" />
                                    )}

                                    <div className="flex gap-6">
                                        {/* Icon */}
                                        <div className={`relative z-10 w-16 h-16 rounded-2xl flex items-center justify-center flex-shrink-0 border ${getColorClasses(accion.color)}`}>
                                            <Icon className="w-8 h-8" />
                                        </div>

                                        {/* Content */}
                                        <div className="flex-1">
                                            <div className="flex items-start justify-between mb-3">
                                                <div>
                                                    <div className="flex items-center gap-3 mb-2">
                                                        <User className="w-4 h-4 text-neutral-600" />
                                                        <span className="text-sm font-bold text-neutral-400 uppercase tracking-wide">
                                                            {accion.usuarioNombre}
                                                        </span>
                                                    </div>
                                                    <h3 className="text-xl font-black text-white mb-1 uppercase tracking-tight">
                                                        {accion.descripcion}
                                                    </h3>
                                                    <div className="flex items-center gap-3 text-sm text-neutral-500">
                                                        <Calendar className="w-4 h-4" />
                                                        <span>{formatFecha(accion.fecha)}</span>
                                                    </div>
                                                </div>
                                                {getEstadoIcon(accion.estado)}
                                            </div>

                                            <p className="text-neutral-400 text-sm font-medium mb-4">
                                                {accion.detalles}
                                            </p>

                                            <div className="flex gap-3">
                                                <span className={`px-3 py-1 rounded-full text-[10px] font-black uppercase tracking-widest ${accion.estado === 'exitoso' ? 'bg-green-500/10 text-green-500 border border-green-500/20' :
                                                    accion.estado === 'fallido' ? 'bg-red-500/10 text-red-500 border border-red-500/20' :
                                                        'bg-yellow-500/10 text-yellow-500 border border-yellow-500/20'
                                                    }`}>
                                                    {accion.estado}
                                                </span>
                                                <span className="px-3 py-1 bg-neutral-800 rounded-full text-[10px] font-black uppercase tracking-widest text-neutral-400">
                                                    {accion.tipo}
                                                </span>
                                                <span className="px-3 py-1 bg-orange-500/10 rounded-full text-[10px] font-black uppercase tracking-widest text-orange-400 border border-orange-500/20">
                                                    ID: {accion.usuarioId.substring(0, 8)}
                                                </span>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            );
                        })
                    )}
                </div>

                {/* Stats Summary */}
                {acciones.length > 0 && (
                    <div className="mt-16 grid grid-cols-1 md:grid-cols-4 gap-6">
                        <div className="bg-neutral-900 border border-neutral-800 rounded-3xl p-6">
                            <Activity className="w-8 h-8 text-orange-500 mb-3" />
                            <p className="text-3xl font-black text-white mb-1">
                                {acciones.length}
                            </p>
                            <p className="text-sm font-bold text-orange-400 uppercase tracking-widest">Total Acciones</p>
                        </div>

                        <div className="bg-green-500/10 border border-green-500/20 rounded-3xl p-6">
                            <CheckCircle2 className="w-8 h-8 text-green-500 mb-3" />
                            <p className="text-3xl font-black text-white mb-1">
                                {acciones.filter(a => a.estado === 'exitoso').length}
                            </p>
                            <p className="text-sm font-bold text-green-400 uppercase tracking-widest">Exitosas</p>
                        </div>

                        <div className="bg-yellow-500/10 border border-yellow-500/20 rounded-3xl p-6">
                            <Clock className="w-8 h-8 text-yellow-500 mb-3" />
                            <p className="text-3xl font-black text-white mb-1">
                                {acciones.filter(a => a.estado === 'pendiente').length}
                            </p>
                            <p className="text-sm font-bold text-yellow-400 uppercase tracking-widest">Pendientes</p>
                        </div>

                        <div className="bg-red-500/10 border border-red-500/20 rounded-3xl p-6">
                            <XCircle className="w-8 h-8 text-red-500 mb-3" />
                            <p className="text-3xl font-black text-white mb-1">
                                {acciones.filter(a => a.estado === 'fallido').length}
                            </p>
                            <p className="text-sm font-bold text-red-400 uppercase tracking-widest">Fallidas</p>
                        </div>
                    </div>
                )}
            </div>
        </div>
    );
};
