import { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useAuth } from 'react-oidc-context';
import SeatMap from '../../asientos/components/SeatMap';
import PaymentForm from '../../pagos/components/PaymentForm';
import CouponInput from '../../pagos/components/CouponInput';
import { asientosService, Asiento } from '../../asientos/services/asientos.service';
import { entradasService } from '../services/entradas.service';
import { serviciosService, ServicioComplementario } from '../../servicios/services/servicios.service';
import ServiceCard from '../../servicios/components/ServiceCard';
import { useAsientosSignalR } from '../../../hooks/useAsientosSignalR';
import { useServiciosSignalR } from '../../../hooks/useServiciosSignalR';
import { toast } from 'react-hot-toast';
import { Video, ChevronRight, ShoppingBag, ArrowLeft, Check } from 'lucide-react';
import { eventosService } from '../../eventos/services/eventos.service';
import { Evento } from '../../eventos/types/evento.types';

export default function CheckoutPage() {
    const { eventoId } = useParams<{ eventoId: string }>();
    const navigate = useNavigate();
    const auth = useAuth();

    // Steps: 1 = Asientos, 2 = Servicios, 3 = Pago
    const [step, setStep] = useState(1);

    const [asientos, setAsientos] = useState<Asiento[]>([]);
    const [selectedAsientos, setSelectedAsientos] = useState<Asiento[]>([]);

    // Servicios
    const [servicios, setServicios] = useState<ServicioComplementario[]>([]);
    const [selectedServicios, setSelectedServicios] = useState<Record<string, number>>({});

    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [showPaymentForm, setShowPaymentForm] = useState(false);
    const [processingPurchase, setProcessingPurchase] = useState(false);
    const [ordenId, setOrdenId] = useState<string>('');
    const [evento, setEvento] = useState<Evento | null>(null);

    // Estados para cupones
    const [cuponAplicado, setCuponAplicado] = useState<{
        codigo: string;
        descuento: number;
        nuevoTotal: number;
    } | null>(null);

    useEffect(() => {
        if (eventoId) {
            loadInitialData();
        }
    }, [eventoId]);

    // SignalR Callbacks
    const onAsientoReservado = useCallback((asientoId: string, usuarioId: string) => {
        setAsientos(prev => prev.map(a =>
            a.id === asientoId
                ? { ...a, reservado: true, usuarioId, estado: 'Ocupado' }
                : a
        ));
        if (auth.user?.profile.sub !== usuarioId) {
            setSelectedAsientos(prev => {
                const isSelectedByMe = prev.some(a => a.id === asientoId);
                if (isSelectedByMe) {
                    toast.error('Un asiento de tu selección acaba de ser reservado por otro usuario.');
                    return prev.filter(a => a.id !== asientoId);
                }
                return prev;
            });
        }
    }, [auth.user?.profile.sub]);

    const onAsientoLiberado = useCallback((asientoId: string) => {
        setAsientos(prev => prev.map(a =>
            a.id === asientoId
                ? { ...a, reservado: false, usuarioId: undefined, estado: 'Disponible' }
                : a
        ));
    }, []);

    // SignalR Callback para Servicios
    const onServiceUpdate = useCallback((notif: any) => {
        if (!notif.idServicioExterno) return;

        console.log('🔄 Actualizando estado de servicios localmente...', notif);

        setServicios(prevServicios => prevServicios.map(servicio => ({
            ...servicio,
            proveedores: servicio.proveedores?.map(p => {
                // Comparamos por el ID externo que viene del backend
                if (p.externalId === notif.idServicioExterno || p.id === notif.idServicioExterno) {
                    return {
                        ...p,
                        precio: notif.precio,
                        estaDisponible: notif.disponible
                    };
                }
                return p;
            })
        })));

        // Si el servicio se agotó y el usuario lo tenía seleccionado, avisamos
        if (!notif.disponible && selectedServicios[notif.idServicioExterno] > 0) {
            toast.error(`Lo sentimos, el servicio '${notif.nombre}' se acaba de agotar.`);
            // Opcionalmente podrías quitarlo de la selección automáticamente:
            // setSelectedServicios(prev => ({ ...prev, [notif.idServicioExterno]: 0 }));
        }
    }, [selectedServicios]);

    useAsientosSignalR({
        eventoId: eventoId || '',
        onAsientoReservado,
        onAsientoLiberado
    });

    useServiciosSignalR({
        onNotification: onServiceUpdate
    });

    const loadInitialData = async () => {
        try {
            setLoading(true);
            setError('');

            // Cargar evento
            const eventoData = await eventosService.getEventoById(eventoId!);
            setEvento(eventoData);

            // Cargar asientos si aplica
            if (!eventoData.esVirtual) {
                const data = await asientosService.getByEvento(eventoId!);
                setAsientos(data);
            }

            // Cargar servicios complementarios
            try {
                const serviciosData = await serviciosService.getServiciosPorEvento(eventoId!);
                setServicios(serviciosData.filter(s => s.activo));
            } catch (serviceError) {
                console.warn('Error cargando servicios (continuando sin ellos):', serviceError);
                setServicios([]);
            }

        } catch (err: any) {
            console.error('Error al cargar datos:', err);
            setError(err.message || 'Error al cargar los datos del evento');
        } finally {
            setLoading(false);
        }
    };

    const handleSelectionChange = (selected: Asiento[]) => {
        setSelectedAsientos(selected);
    };

    const handleServiceQuantityChange = (servicioId: string, cantidad: number) => {
        setSelectedServicios(prev => ({
            ...prev,
            [servicioId]: cantidad
        }));
    };

    // Cálculos financieros
    const precioAsientos = evento?.esVirtual
        ? (evento.precioBase || 0)
        : selectedAsientos.reduce((sum, asiento) => sum + asiento.precio, 0);

    const precioServicios = servicios.reduce((sum, service) => {
        const providers = (service.proveedores && service.proveedores.length > 0) ? service.proveedores : [];
        const serviceSum = providers.reduce((pSum, p) => {
            const qty = selectedServicios[p.id] || 0;
            return pSum + (p.precio * qty);
        }, 0);
        return sum + serviceSum;
    }, 0);

    const subtotal = precioAsientos + precioServicios;

    // Si hay cupón, recalculamos. Nota: El cupón suele aplicar al total.
    const totalPrice = cuponAplicado ? cuponAplicado.nuevoTotal : subtotal;

    // Validación de Pasos
    const canProceedToServices = () => {
        if (evento?.esVirtual) return true;
        return selectedAsientos.length > 0;
    };

    const nextStep = () => {
        if (step === 1) {
            if (!canProceedToServices()) {
                toast.error('Selecciona al menos un asiento para continuar');
                return;
            }
            setStep(2);
        } else if (step === 2) {
            setStep(3);
        }
    };

    const prevStep = () => {
        if (step > 1) setStep(step - 1);
    };

    const handleInitiatePayment = async () => {
        if (!auth.isAuthenticated) {
            auth.signinRedirect();
            return;
        }

        setProcessingPurchase(true);
        try {
            // 1. Crear Orden de Entradas
            const asientoIds = selectedAsientos.map(a => a.id);
            const response = await entradasService.crearEntrada({
                eventoId: eventoId!,
                usuarioId: auth.user?.profile.sub || '',
                asientoIds: asientoIds,
                nombreUsuario: (auth.user?.profile as any)?.preferred_username || auth.user?.profile.name,
                email: auth.user?.profile.email
            });

            const ordenIdVal = response.ordenId || response.data?.ordenId || response.data?.id || response.id;

            if (!ordenIdVal || ordenIdVal === '00000000-0000-0000-0000-000000000000') {
                throw new Error('No se pudo generar un número de orden válido.');
            }

            setOrdenId(ordenIdVal);

            // 2. Pre-reservar Servicios Extras (Antes del pago)
            const providersWithSelection = Object.entries(selectedServicios).filter(([_, qty]) => qty > 0);
            if (providersWithSelection.length > 0) {
                const reservasPromesas = [];
                for (const [providerId, qty] of providersWithSelection) {
                    const servicioPadre = servicios.find(s => s.proveedores?.some(p => p.id === providerId));
                    if (!servicioPadre) continue;

                    for (let i = 0; i < qty; i++) {
                        reservasPromesas.push(serviciosService.reservarServicio({
                            usuarioId: auth.user?.profile.sub || '',
                            eventoId: eventoId!,
                            servicioGlobalId: servicioPadre.id,
                            ordenEntradaId: ordenIdVal
                        }));
                    }
                }

                if (reservasPromesas.length > 0) {
                    const loadingToast = toast.loading('Reservando servicios seleccionados...');
                    await Promise.all(reservasPromesas);
                    toast.success('Servicios reservados. Proceda al pago.', { id: loadingToast });
                }
            }

            setShowPaymentForm(true);
        } catch (err: any) {
            console.error('Error al iniciar compra:', err);
            toast.error('Error al iniciar compra: ' + (err.response?.data?.detail || err.message));
        } finally {
            setProcessingPurchase(false);
        }
    };

    const handlePaymentSuccess = async (transaccionId: string) => {
        console.log('[PAGO] Éxitoso, ID de Transacción:', transaccionId);
        setShowPaymentForm(false);

        toast.success('¡Todo listo! Entradas y servicios confirmados.', {
            duration: 6000,
            icon: '🎉'
        });

        setTimeout(() => {
            navigate('/entradas');
        }, 2500);
    };

    if (loading) {
        return (
            <div className="min-h-screen bg-black flex items-center justify-center">
                <div className="text-center">
                    <div className="inline-block animate-spin rounded-full h-16 w-16 border-t-2 border-b-2 border-purple-500 mb-4"></div>
                    <p className="text-gray-400">Cargando experiencia...</p>
                </div>
            </div>
        );
    }

    if (error) {
        return (
            <div className="min-h-screen bg-black flex items-center justify-center px-4">
                <div className="text-center max-w-md">
                    <div className="text-red-500 text-5xl mb-4">⚠️</div>
                    <h2 className="text-2xl font-bold text-white mb-2">Error</h2>
                    <p className="text-gray-400 mb-6">{error}</p>
                    <button onClick={() => navigate(-1)} className="px-6 py-2 bg-purple-600 rounded-lg">Volver</button>
                </div>
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-black text-white pb-20">
            {/* Background */}
            <div className="fixed inset-0 overflow-hidden pointer-events-none">
                <div className="absolute top-0 left-1/4 w-96 h-96 bg-purple-500/10 rounded-full blur-3xl"></div>
                <div className="absolute bottom-0 right-1/4 w-96 h-96 bg-cyan-500/10 rounded-full blur-3xl"></div>
            </div>

            <div className="relative container mx-auto px-4 py-8">
                {/* Header & Stepper */}
                <div className="mb-8">
                    <div className="flex items-center justify-between mb-6">
                        <button onClick={() => navigate(-1)} className="flex items-center gap-2 text-gray-400 hover:text-white transition">
                            <ArrowLeft size={20} /> Volver
                        </button>
                        <div className="flex items-center gap-2">
                            {[1, 2, 3].map(s => (
                                <div key={s} className={`flex items-center ${s < 3 ? 'after:content-[""] after:w-8 after:h-[2px] after:bg-gray-800 after:mx-2' : ''}`}>
                                    <div className={`w-8 h-8 rounded-full flex items-center justify-center text-xs font-bold ${step === s ? 'bg-purple-600 text-white' :
                                        step > s ? 'bg-green-500 text-white' : 'bg-gray-800 text-gray-500'
                                        }`}>
                                        {step > s ? <Check size={14} /> : s}
                                    </div>
                                </div>
                            ))}
                        </div>
                    </div>

                    <h1 className="text-3xl font-black text-white mb-1">
                        {step === 1 && (evento?.esVirtual ? 'Pase Digital' : 'Selecciona Asientos')}
                        {step === 2 && 'Personaliza tu Experiencia'}
                        {step === 3 && 'Resumen & Pago'}
                    </h1>
                </div>

                <div className="grid lg:grid-cols-3 gap-8">
                    {/* Main Content Area */}
                    <div className="lg:col-span-2">
                        <div className="bg-gray-900/50 border border-white/10 rounded-3xl p-6 min-h-[500px]">
                            {/* PASO 1: ASIENTOS */}
                            {step === 1 && (
                                <>
                                    {evento?.esVirtual ? (
                                        <div className="h-full flex flex-col items-center justify-center text-center py-10">
                                            <div className="w-24 h-24 bg-blue-500/20 rounded-full flex items-center justify-center text-blue-400 mb-6">
                                                <Video size={48} />
                                            </div>
                                            <h2 className="text-2xl font-bold mb-2">Evento Virtual Global</h2>
                                            <p className="text-gray-400 max-w-md">Tu pase te da acceso ilimitado al streaming en vivo y repeticiones on-demand.</p>
                                        </div>
                                    ) : (
                                        <SeatMap asientos={asientos} onSelectionChange={handleSelectionChange} />
                                    )}
                                </>
                            )}

                            {/* PASO 2: SERVICIOS */}
                            {step === 2 && (
                                <div>
                                    <h2 className="text-xl font-bold mb-6 flex items-center gap-2">
                                        <ShoppingBag size={20} className="text-purple-400" /> Complementos Disponibles
                                    </h2>
                                    {servicios.length === 0 ? (
                                        <div className="text-center py-12 bg-white/5 rounded-2xl">
                                            <p className="text-gray-400">No hay servicios extras disponibles para este evento.</p>
                                        </div>
                                    ) : (
                                        <div className="grid md:grid-cols-2 gap-4">
                                            {servicios.map(servicio => (
                                                <ServiceCard
                                                    key={servicio.id}
                                                    servicio={servicio}
                                                    selectedOptions={selectedServicios}
                                                    onOptionChange={(provider, qty) => handleServiceQuantityChange(provider.id, qty)}
                                                />
                                            ))}
                                        </div>
                                    )}
                                </div>
                            )}

                            {/* PASO 3: RESUMEN FINAL visualizado en panel lateral, aquí mostramos detalles extra? */}
                            {step === 3 && (
                                <div className="space-y-6">
                                    <div className="bg-white/5 rounded-2xl p-6 border border-white/10">
                                        <h3 className="font-bold text-lg mb-4">Detalles de la Reserva</h3>
                                        <div className="space-y-4 text-sm">
                                            <div className="flex justify-between border-b border-white/10 pb-2">
                                                <span className="text-gray-400">Evento</span>
                                                <span className="font-medium text-right">{evento?.titulo}</span>
                                            </div>
                                            <div className="flex justify-between border-b border-white/10 pb-2">
                                                <span className="text-gray-400">Fecha</span>
                                                <span className="font-medium text-right">{new Date(evento?.fechaInicio || '').toLocaleDateString()}</span>
                                            </div>
                                            <div className="flex justify-between border-b border-white/10 pb-2">
                                                <span className="text-gray-400">Entradas</span>
                                                <span className="font-medium text-right">{selectedAsientos.length === 0 ? '1 Pase Virtual' : `${selectedAsientos.length} Asientos`}</span>
                                            </div>
                                            <div className="flex justify-between pb-2">
                                                <span className="text-gray-400">Servicios Extra</span>
                                                <span className="font-medium text-right">
                                                    {Object.values(selectedServicios).reduce((a, b) => a + b, 0)} ítems
                                                </span>
                                            </div>
                                        </div>
                                    </div>

                                    <div className="bg-blue-900/20 border border-blue-500/30 p-4 rounded-xl flex gap-3">
                                        <div className="bg-blue-500/20 p-2 rounded-lg h-fit text-blue-400">
                                            <Check size={20} />
                                        </div>
                                        <div>
                                            <p className="font-bold text-blue-400 text-sm">Garantía de Satisfacción</p>
                                            <p className="text-xs text-blue-200/60 mt-1">Si el evento se cancela, te devolvemos el 100% de tu dinero, incluyendo los servicios extras contratados.</p>
                                        </div>
                                    </div>
                                </div>
                            )}
                        </div>

                        {/* Navigation Buttons */}
                        <div className="flex justify-between mt-6">
                            <button
                                onClick={prevStep}
                                disabled={step === 1}
                                className={`px-6 py-3 rounded-xl font-bold transition ${step === 1 ? 'opacity-0' : 'bg-gray-800 hover:bg-gray-700'}`}
                            >
                                Atrás
                            </button>

                            {step < 3 ? (
                                <button
                                    onClick={nextStep}
                                    className="px-8 py-3 bg-white text-black font-black rounded-xl hover:bg-gray-200 transition flex items-center gap-2"
                                >
                                    Siguiente <ChevronRight size={18} />
                                </button>
                            ) : (
                                <button
                                    onClick={handleInitiatePayment}
                                    disabled={processingPurchase}
                                    className="px-8 py-3 bg-gradient-to-r from-purple-600 to-pink-600 text-white font-black rounded-xl hover:opacity-90 transition shadow-lg shadow-purple-900/50"
                                >
                                    {processingPurchase ? 'Procesando...' : 'Pagar Ahora'}
                                </button>
                            )}
                        </div>
                    </div>

                    {/* Sidebar Resumen */}
                    <div className="lg:col-span-1">
                        <div className="bg-black border border-gray-800 rounded-2xl p-6 sticky top-8">
                            <h3 className="text-xl font-bold mb-6">Resumen</h3>

                            {/* Desglose Asientos */}
                            <div className="mb-4">
                                <p className="text-xs font-bold text-gray-500 uppercase tracking-widest mb-2">Entradas</p>
                                {evento?.esVirtual ? (
                                    <div className="flex justify-between text-sm mb-1">
                                        <span>Pase Virtual</span>
                                        <span>${(evento.precioBase || 0).toFixed(2)}</span>
                                    </div>
                                ) : (
                                    selectedAsientos.map(asiento => (
                                        <div key={asiento.id} className="flex justify-between text-sm mb-1 text-gray-300">
                                            <span>{asiento.numero} ({asiento.categoria})</span>
                                            <span>${asiento.precio.toFixed(2)}</span>
                                        </div>
                                    ))
                                )}
                                {selectedAsientos.length === 0 && !evento?.esVirtual && <p className="text-gray-600 text-sm italic">Ningún asiento seleccionado</p>}
                            </div>

                            {/* Desglose Servicios */}
                            {Object.values(selectedServicios).some(qty => qty > 0) && (
                                <div className="mb-4 pt-4 border-t border-gray-800">
                                    <p className="text-xs font-bold text-gray-500 uppercase tracking-widest mb-2">Extras</p>
                                    {servicios.map(service => {
                                        const providers = service.proveedores || [];
                                        return providers
                                            .filter(p => selectedServicios[p.id] > 0)
                                            .map(p => (
                                                <div key={p.id} className="flex justify-between text-sm mb-1 text-gray-300">
                                                    <span>{selectedServicios[p.id]}x {p.nombreProveedor}</span>
                                                    <span>${(p.precio * selectedServicios[p.id]).toFixed(2)}</span>
                                                </div>
                                            ));
                                    })}
                                </div>
                            )}

                            {/* Cupón */}
                            {step === 3 && (
                                <div className="mb-4 pt-4 border-t border-gray-800">
                                    <CouponInput
                                        eventoId={eventoId!}
                                        montoOriginal={subtotal}
                                        onCouponApplied={(c, d, t) => { setCuponAplicado({ codigo: c, descuento: d, nuevoTotal: t }); toast.success('Cupón aplicado'); }}
                                        onCouponRemoved={() => setCuponAplicado(null)}
                                        disabled={processingPurchase}
                                    />
                                </div>
                            )}

                            {/* Totales */}
                            <div className="mt-6 pt-4 border-t border-white/20">
                                <div className="flex justify-between mb-2 text-gray-400">
                                    <span>Subtotal</span>
                                    <span>${subtotal.toFixed(2)}</span>
                                </div>
                                {cuponAplicado && (
                                    <div className="flex justify-between mb-2 text-green-400">
                                        <span>Descuento</span>
                                        <span>-${cuponAplicado.descuento.toFixed(2)}</span>
                                    </div>
                                )}
                                <div className="flex justify-between items-center text-xl font-bold text-white mt-4">
                                    <span>Total</span>
                                    <span>${totalPrice.toFixed(2)}</span>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            {/* Modal de Pago */}
            {showPaymentForm && auth.isAuthenticated && (
                <PaymentForm
                    monto={totalPrice} // Pasamos el total completo (Entradas + Servicios)
                    ordenId={ordenId} // El ID de la orden de ENTRADAS
                    usuarioId={auth.user?.profile.sub || ''}
                    onSuccess={handlePaymentSuccess}
                    onCancel={() => setShowPaymentForm(false)}
                    codigoCupon={cuponAplicado?.codigo}
                />
            )}

            <style>{`
                .custom-scrollbar::-webkit-scrollbar { width: 4px; }
                .custom-scrollbar::-webkit-scrollbar-thumb { background: #555; border-radius: 4px; }
            `}</style>
        </div>
    );
}
