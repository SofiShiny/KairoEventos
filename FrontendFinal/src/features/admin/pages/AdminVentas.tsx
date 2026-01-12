import { useState, useEffect } from 'react';
import {
    Search,
    Filter,
    Download,
    ExternalLink,
    TrendingUp,
    Users,
    Ticket,
    DollarSign,
    MoreVertical,
    CheckCircle2,
    XCircle,
    Clock,
    Loader2
} from 'lucide-react';
import { entradasService, Entrada } from '../../entradas/services/entradas.service';
import { useAuth } from 'react-oidc-context';
import { getUserRoles } from '../../../lib/auth-utils';

export default function AdminVentas() {
    const auth = useAuth();
    const [searchTerm, setSearchTerm] = useState('');
    const [ventas, setVentas] = useState<Entrada[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        cargarVentas();
    }, []);

    const cargarVentas = async () => {
        try {
            setLoading(true);
            const roles = getUserRoles(auth.user);
            const isAdmin = roles.includes('admin');
            const organizadorId = !isAdmin ? auth.user?.profile.sub : undefined;

            const data = await entradasService.getTodasLasEntradas(organizadorId);
            setVentas(data);
        } catch (error) {
            console.error('Error al cargar ventas:', error);
        } finally {
            setLoading(false);
        }
    };

    // Cálculos basados en datos reales
    const ingresosTotales = ventas.reduce((acc, curr) => acc + curr.precio, 0);
    const ticketsVendidos = ventas.filter(v => v.estado.toLowerCase() === 'pagada').length;
    const ticketPromedio = ventas.length > 0 ? ingresosTotales / ventas.length : 0;

    const kpis = [
        { label: 'Ingresos Totales', value: `$${ingresosTotales.toLocaleString()}`, icon: DollarSign, color: 'blue', trend: '+12%' },
        { label: 'Tickets Vendidos', value: ticketsVendidos.toLocaleString(), icon: Ticket, color: 'emerald', trend: '+8%' },
        { label: 'Ticket Promedio', value: `$${ticketPromedio.toFixed(2)}`, icon: TrendingUp, color: 'purple', trend: '+4%' },
        { label: 'Nuevos Clientes', value: Array.from(new Set(ventas.map(v => v.usuarioId))).length.toString(), icon: Users, color: 'amber', trend: '+15%' },
    ];

    const getStatusBadge = (estado: string) => {
        switch (estado) {
            case 'pagada':
                return (
                    <span className="flex items-center gap-1.5 px-2.5 py-1 rounded-full bg-emerald-500/10 text-emerald-500 text-[10px] font-black uppercase tracking-wider border border-emerald-500/20">
                        <CheckCircle2 className="w-3 h-3" /> Pagada
                    </span>
                );
            case 'cancelada':
                return (
                    <span className="flex items-center gap-1.5 px-2.5 py-1 rounded-full bg-rose-500/10 text-rose-500 text-[10px] font-black uppercase tracking-wider border border-rose-500/20">
                        <XCircle className="w-3 h-3" /> Cancelada
                    </span>
                );
            default:
                return (
                    <span className="flex items-center gap-1.5 px-2.5 py-1 rounded-full bg-amber-500/10 text-amber-500 text-[10px] font-black uppercase tracking-wider border border-amber-500/20">
                        <Clock className="w-3 h-3" /> Pendiente
                    </span>
                );
        }
    };

    return (
        <div className="space-y-8 animate-in fade-in slide-in-from-bottom-4 duration-500">
            {/* Header */}
            <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
                <div>
                    <h1 className="text-3xl font-black text-white italic">Gestión de Ventas</h1>
                    <p className="text-slate-400 text-sm mt-1">Monitorea ingresos y transacciones en tiempo real.</p>
                </div>
                <button className="flex items-center gap-2 bg-slate-800 hover:bg-slate-700 text-white px-4 py-2.5 rounded-xl text-sm font-bold transition-all border border-slate-700">
                    <Download className="w-4 h-4" />
                    <span>Exportar Reporte</span>
                </button>
            </div>

            {/* KPI Grid */}
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
                {kpis.map((kpi, idx) => (
                    <div key={idx} className="bg-[#16191f] border border-slate-800 p-6 rounded-2xl shadow-lg group hover:border-blue-500/30 transition-all">
                        <div className="flex justify-between items-start mb-4">
                            <div className="p-3 rounded-xl bg-slate-900 text-blue-500 border border-slate-800 group-hover:scale-110 transition-transform">
                                <kpi.icon className="w-6 h-6" />
                            </div>
                            <span className="text-[10px] font-black text-emerald-500 bg-emerald-500/10 px-2 py-0.5 rounded-full">{kpi.trend}</span>
                        </div>
                        <p className="text-slate-500 text-xs font-bold uppercase tracking-widest">{kpi.label}</p>
                        <p className="text-2xl font-black text-white mt-1">{kpi.value}</p>
                    </div>
                ))}
            </div>

            {/* Sales Table Container */}
            <div className="bg-[#16191f] border border-slate-800 rounded-2xl shadow-xl overflow-hidden relative min-h-[400px]">
                {loading && (
                    <div className="absolute inset-0 bg-[#16191f]/80 backdrop-blur-sm z-10 flex flex-col items-center justify-center">
                        <Loader2 className="w-10 h-10 animate-spin text-blue-500 mb-4" />
                        <p className="text-slate-400 font-bold animate-pulse text-xs uppercase tracking-widest">Sincronizando con Microservicios...</p>
                    </div>
                )}

                {/* Filters */}
                <div className="p-4 border-b border-slate-800 flex flex-col md:flex-row justify-between gap-4 bg-slate-900/40">
                    <div className="relative flex-1 max-w-md">
                        <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-500" />
                        <input
                            type="text"
                            placeholder="Buscar por orden, evento o usuario..."
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                            className="w-full bg-[#0f1115] border border-slate-700 rounded-xl pl-10 pr-4 py-2.5 text-sm text-slate-300 outline-none focus:border-blue-500/50 transition-all placeholder:text-slate-600"
                        />
                    </div>
                    <div className="flex items-center gap-2">
                        <button className="flex items-center gap-2 px-4 py-2.5 bg-[#0f1115] border border-slate-700 rounded-xl text-xs font-bold text-slate-400 hover:text-white transition-all hover:border-slate-600">
                            <Filter className="w-4 h-4" /> Filtros Avanzados
                        </button>
                    </div>
                </div>

                {/* Table Content */}
                <div className="overflow-x-auto">
                    <table className="w-full text-left border-collapse">
                        <thead>
                            <tr className="bg-slate-900/60">
                                <th className="px-6 py-4 text-[11px] font-black text-slate-600 uppercase tracking-widest border-b border-slate-800">Orden</th>
                                <th className="px-6 py-4 text-[11px] font-black text-slate-600 uppercase tracking-widest border-b border-slate-800">Evento / Asiento</th>
                                <th className="px-6 py-4 text-[11px] font-black text-slate-600 uppercase tracking-widest border-b border-slate-800">ID Usuario</th>
                                <th className="px-6 py-4 text-[11px] font-black text-slate-600 uppercase tracking-widest border-b border-slate-800">Monto</th>
                                <th className="px-6 py-4 text-[11px] font-black text-slate-600 uppercase tracking-widest border-b border-slate-800">Fecha Compra</th>
                                <th className="px-6 py-4 text-[11px] font-black text-slate-600 uppercase tracking-widest border-b border-slate-800 text-center">Estado</th>
                                <th className="px-6 py-4 text-[11px] font-black text-slate-600 uppercase tracking-widest border-b border-slate-800 text-right">Acciones</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-slate-800/50">
                            {ventas
                                .filter(v =>
                                    v.id.toLowerCase().includes(searchTerm.toLowerCase()) ||
                                    v.eventoNombre.toLowerCase().includes(searchTerm.toLowerCase())
                                )
                                .map((venta) => (
                                    <tr key={venta.id} className="hover:bg-slate-800/10 transition-colors group">
                                        <td className="px-6 py-4">
                                            <span className="text-xs font-black text-blue-500 bg-blue-500/10 px-2 py-1 rounded-md">{venta.id.substring(0, 8)}</span>
                                        </td>
                                        <td className="px-6 py-4">
                                            <p className="text-sm font-bold text-white leading-none mb-1 group-hover:text-blue-400 transition-colors">{venta.eventoNombre}</p>
                                            <p className="text-[10px] text-slate-500 font-medium">{venta.asientoInfo}</p>
                                        </td>
                                        <td className="px-6 py-4">
                                            <div className="flex items-center gap-2">
                                                <div className="w-7 h-7 bg-slate-800 rounded-full flex items-center justify-center text-[10px] font-bold text-slate-400 border border-slate-700">
                                                    U
                                                </div>
                                                <span className="text-xs font-semibold text-slate-300 truncate max-w-[100px]">{venta.usuarioId}</span>
                                            </div>
                                        </td>
                                        <td className="px-6 py-4">
                                            <span className="text-sm font-black text-white">${venta.precio.toFixed(2)}</span>
                                        </td>
                                        <td className="px-6 py-4">
                                            <p className="text-xs font-bold text-slate-400">{new Date(venta.fechaCompra).toLocaleDateString()}</p>
                                            <p className="text-[10px] text-slate-600">{new Date(venta.fechaCompra).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</p>
                                        </td>
                                        <td className="px-6 py-4">
                                            <div className="flex justify-center">
                                                {getStatusBadge(venta.estado.toLowerCase())}
                                            </div>
                                        </td>
                                        <td className="px-6 py-4 text-right">
                                            <div className="flex justify-end gap-2">
                                                <button className="p-2 text-slate-500 hover:text-white hover:bg-slate-800 rounded-lg transition-all" title="Ver Detalles">
                                                    <ExternalLink className="w-4 h-4" />
                                                </button>
                                                <button className="p-2 text-slate-500 hover:text-white hover:bg-slate-800 rounded-lg transition-all">
                                                    <MoreVertical className="w-4 h-4" />
                                                </button>
                                            </div>
                                        </td>
                                    </tr>
                                ))}
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    );
}
