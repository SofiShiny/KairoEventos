import { useState, useEffect } from 'react';
import { useAuth } from 'react-oidc-context';
import {
    Coffee,
    ShoppingBag,
    Sparkles,
    Check,
    Star,
    ShieldCheck,
    Zap,
    ChevronRight,
    ArrowLeft
} from 'lucide-react';
import { serviciosService, ServicioGlobal, ReservaServicio } from '../services/servicios.service';
import { entradasService, Entrada } from '../../entradas/services/entradas.service';
import PaymentForm from '../../pagos/components/PaymentForm';
import { toast } from 'react-hot-toast';

export const ServiciosPage = () => {
    const auth = useAuth();
    const [catalogo, setCatalogo] = useState<ServicioGlobal[]>([]);
    const [misReservas, setMisReservas] = useState<ReservaServicio[]>([]);
    const [misEntradas, setMisEntradas] = useState<Entrada[]>([]);
    const [loading, setLoading] = useState(true);
    const [tab, setTab] = useState<'catalogo' | 'mis-servicios'>('catalogo');

    // Booking state
    const [selectedService, setSelectedService] = useState<ServicioGlobal | null>(null);
    const [selectedEventId, setSelectedEventId] = useState<string>('');
    const [showPayment, setShowPayment] = useState(false);
    const [pendingReservaId, setPendingReservaId] = useState<string>('');

    const usuarioId = auth.user?.profile.sub;

    useEffect(() => {
        loadData();
    }, [usuarioId]);

    const loadData = async () => {
        try {
            setLoading(true);
            const [servicios, entradas] = await Promise.all([
                serviciosService.getCatalogo(),
                usuarioId ? entradasService.getMisEntradas(usuarioId) : Promise.resolve([])
            ]);

            // Si el catálogo está vacío, podríamos agregar algunos servicios premium por defecto
            // para que el usuario no vea una página vacía en su primera vez.
            setCatalogo(servicios.length > 0 ? servicios : []);
            setMisEntradas(entradas);

            if (usuarioId) {
                const reservas = await serviciosService.getMisReservas(usuarioId);
                setMisReservas(reservas);
            }
        } catch (error) {
            console.error('Error cargando datos de servicios:', error);
            toast.error('Ocurrió un error al cargar los servicios.');
        } finally {
            setLoading(false);
        }
    };

    const handleContract = async () => {
        if (!auth.isAuthenticated) {
            auth.signinRedirect();
            return;
        }

        if (!selectedService || !selectedEventId) {
            toast.error('Por favor selecciona un servicio y un evento.');
            return;
        }

        try {
            const reservaId = await serviciosService.reservar({
                usuarioId: usuarioId!,
                eventoId: selectedEventId,
                servicioGlobalId: selectedService.id
            });

            setPendingReservaId(reservaId);
            setShowPayment(true);
        } catch (error) {
            toast.error('Error al iniciar la contratación.');
        }
    };

    const handlePaymentSuccess = (_transaccionId: string) => {
        setShowPayment(false);
        toast.success('¡Servicio contratado con éxito!');
        setSelectedService(null);
        setTab('mis-servicios');
        loadData();
    };

    const icons: Record<string, any> = {
        'VIP': <Star className="w-6 h-6 text-yellow-500" />,
        'Streaming': <Zap className="w-6 h-6 text-blue-500" />,
        'Seguridad': <ShieldCheck className="w-6 h-6 text-green-500" />,
        'default': <Sparkles className="w-6 h-6 text-purple-500" />
    };

    const getIcon = (nombre: string) => {
        if (nombre.includes('VIP')) return icons['VIP'];
        if (nombre.includes('Streaming')) return icons['Streaming'];
        if (nombre.includes('Seguro')) return icons['Seguridad'];
        return icons['default'];
    };

    if (loading && catalogo.length === 0) {
        return (
            <div className="min-h-screen bg-black flex items-center justify-center">
                <div className="animate-spin rounded-full h-16 w-16 border-t-2 border-b-2 border-blue-500"></div>
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-black text-white p-8">
            <div className="max-w-7xl mx-auto">
                {/* Header */}
                <header className="mb-12 relative">
                    <div className="absolute -top-10 -left-10 w-64 h-64 bg-purple-600/10 blur-[120px] rounded-full pointer-events-none" />
                    <div className="flex items-center gap-2 mb-4">
                        <Sparkles className="w-5 h-5 text-purple-500" />
                        <span className="text-purple-500 font-black text-xs uppercase tracking-[0.3em]">Servicios Exclusive</span>
                    </div>
                    <h1 className="text-6xl font-black mb-6 tracking-tighter">
                        ELEVA TU <br />
                        <span className="bg-gradient-to-r from-white via-white to-neutral-500 bg-clip-text text-transparent">
                            EXPERIENCIA
                        </span>
                    </h1>
                </header>

                {/* Tabs */}
                <div className="flex gap-4 mb-10 overflow-x-auto pb-2">
                    <button
                        onClick={() => setTab('catalogo')}
                        className={`px-8 py-3 rounded-2xl font-black text-sm uppercase tracking-widest transition-all ${tab === 'catalogo' ? 'bg-white text-black' : 'bg-neutral-900 text-neutral-500 border border-neutral-800'
                            }`}
                    >
                        Catálogo Premium
                    </button>
                    <button
                        onClick={() => setTab('mis-servicios')}
                        className={`px-8 py-3 rounded-2xl font-black text-sm uppercase tracking-widest transition-all ${tab === 'mis-servicios' ? 'bg-white text-black' : 'bg-neutral-900 text-neutral-500 border border-neutral-800'
                            }`}
                    >
                        Mis Contrataciones
                    </button>
                </div>

                {tab === 'catalogo' ? (
                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
                        {catalogo.length === 0 ? (
                            <div className="col-span-full py-20 text-center border-2 border-dashed border-neutral-900 rounded-[3rem]">
                                <Coffee className="w-16 h-16 text-neutral-800 mx-auto mb-4" />
                                <p className="text-neutral-500 font-bold uppercase tracking-widest">Catálogo en mantenimiento</p>
                            </div>
                        ) : (
                            catalogo.map(servicio => (
                                <div
                                    key={servicio.id}
                                    className={`group relative bg-neutral-900/40 border-2 rounded-[2.5rem] p-8 transition-all duration-500 hover:scale-[1.02] ${selectedService?.id === servicio.id ? 'border-purple-500 bg-purple-500/5' : 'border-neutral-800 hover:border-neutral-700'
                                        }`}
                                >
                                    <div className="mb-6 flex justify-between items-start">
                                        <div className="p-4 bg-neutral-800 rounded-3xl group-hover:bg-purple-500/10 transition-colors">
                                            {getIcon(servicio.nombre)}
                                        </div>
                                        <div className="text-right">
                                            <span className="text-xs font-black text-neutral-500 uppercase tracking-widest block mb-1">Inversión</span>
                                            <span className="text-2xl font-black text-white">${servicio.precio}</span>
                                        </div>
                                    </div>

                                    <h3 className="text-2xl font-black mb-4 group-hover:text-purple-400 transition-colors">{servicio.nombre}</h3>
                                    <p className="text-neutral-500 text-sm font-medium leading-relaxed mb-8">
                                        Disfruta de beneficios exclusivos y acceso prioritario con nuestro servicio premium de {servicio.nombre.toLowerCase()}.
                                    </p>

                                    <button
                                        onClick={() => setSelectedService(servicio)}
                                        className={`w-full py-4 rounded-2xl font-black text-xs uppercase tracking-widest transition-all ${selectedService?.id === servicio.id
                                            ? 'bg-purple-500 text-white shadow-[0_0_20px_rgba(168,85,247,0.4)]'
                                            : 'bg-neutral-800 text-white hover:bg-neutral-700'
                                            }`}
                                    >
                                        {selectedService?.id === servicio.id ? 'SELECCIONADO' : 'SELECCIONAR'}
                                    </button>
                                </div>
                            ))
                        )}
                    </div>
                ) : (
                    <div className="grid gap-6">
                        {misReservas.length === 0 ? (
                            <div className="py-20 text-center bg-neutral-900/20 rounded-[3rem] border border-neutral-900">
                                <ShoppingBag className="w-12 h-12 text-neutral-800 mx-auto mb-4" />
                                <p className="text-neutral-600 font-bold uppercase tracking-widest text-xs">No has contratado servicios aún</p>
                            </div>
                        ) : (
                            misReservas.map(reserva => (
                                <div key={reserva.id} className="flex flex-col md:flex-row items-center gap-6 p-6 bg-neutral-900 border border-neutral-800 rounded-3xl">
                                    <div className="p-4 bg-neutral-800 rounded-2xl">
                                        <Check className={`w-8 h-8 ${reserva.estado === 'Confirmado' ? 'text-green-500' : 'text-neutral-500'}`} />
                                    </div>
                                    <div className="flex-1 text-center md:text-left">
                                        <h4 className="text-xl font-black text-white uppercase tracking-tighter">Reserva Premium</h4>
                                        <p className="text-neutral-500 text-sm font-medium">Contratado el {new Date(reserva.fechaCreacion).toLocaleDateString()}</p>
                                    </div>
                                    <div className="flex flex-col items-center md:items-end gap-2">
                                        <span className={`px-4 py-1 rounded-full text-[10px] font-black uppercase tracking-widest ${reserva.estado === 'Confirmado' ? 'bg-green-500/10 text-green-500 border border-green-500/20' : 'bg-yellow-500/10 text-yellow-500 border border-yellow-500/20'
                                            }`}>
                                            {reserva.estado}
                                        </span>
                                    </div>
                                </div>
                            ))
                        )}
                    </div>
                )}

                {/* Booking Sidebar/Modal overlay if service selected */}
                {selectedService && tab === 'catalogo' && (
                    <div className="fixed bottom-10 left-1/2 -translate-x-1/2 w-full max-w-2xl px-4 animate-in slide-in-from-bottom-10 duration-500">
                        <div className="bg-white text-black p-8 rounded-[3rem] shadow-2xl flex flex-col md:flex-row items-center gap-8">
                            <div className="flex-1">
                                <h4 className="text-xs font-black uppercase tracking-[0.3em] text-neutral-400 mb-2">Completar Contratación</h4>
                                <h2 className="text-2xl font-black leading-none">{selectedService.nombre}</h2>

                                <select
                                    className="mt-4 w-full bg-neutral-100 border-none rounded-xl p-3 font-bold text-sm focus:ring-2 focus:ring-purple-500 transition-all"
                                    value={selectedEventId}
                                    onChange={(e) => setSelectedEventId(e.target.value)}
                                >
                                    <option value="">Selecciona tu Evento...</option>
                                    {misEntradas.map(entrada => (
                                        <option key={entrada.id} value={entrada.eventoId}>
                                            {entrada.eventoNombre}
                                        </option>
                                    ))}
                                </select>
                            </div>

                            <div className="flex gap-4">
                                <button
                                    onClick={() => setSelectedService(null)}
                                    className="p-4 bg-neutral-100 rounded-2xl hover:bg-neutral-200 transition"
                                >
                                    <ArrowLeft className="w-6 h-6" />
                                </button>
                                <button
                                    onClick={handleContract}
                                    className="px-8 py-4 bg-black text-white rounded-2xl font-black text-xs uppercase tracking-widest hover:bg-neutral-800 transition shadow-xl flex items-center gap-2"
                                >
                                    CONTRATAR AHORA <ChevronRight className="w-4 h-4" />
                                </button>
                            </div>
                        </div>
                    </div>
                )}
            </div>

            {/* Payment Modal */}
            {showPayment && (
                <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/90 backdrop-blur-sm">
                    <div className="w-full max-w-4xl max-h-[90vh] overflow-y-auto rounded-[3rem]">
                        <PaymentForm
                            monto={selectedService?.precio || 0}
                            ordenId={pendingReservaId}
                            usuarioId={usuarioId || ''}
                            onSuccess={handlePaymentSuccess}
                            onCancel={() => setShowPayment(false)}
                        />
                    </div>
                </div>
            )}
        </div>
    );
};
