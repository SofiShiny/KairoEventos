import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useAuth } from 'react-oidc-context';
import SeatMap from '../../asientos/components/SeatMap';
import PaymentForm from '../../pagos/components/PaymentForm';
import CouponInput from '../../pagos/components/CouponInput';
import { asientosService, Asiento } from '../../asientos/services/asientos.service';
import { entradasService } from '../services/entradas.service';
import { useAsientosSignalR } from '../../../hooks/useAsientosSignalR';
import { toast } from 'react-hot-toast';

export default function CheckoutPage() {
    const { eventoId } = useParams<{ eventoId: string }>();
    const navigate = useNavigate();
    const auth = useAuth();

    const [asientos, setAsientos] = useState<Asiento[]>([]);
    const [selectedAsientos, setSelectedAsientos] = useState<Asiento[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [showPaymentForm, setShowPaymentForm] = useState(false);
    const [processingPurchase, setProcessingPurchase] = useState(false);
    const [ordenId, setOrdenId] = useState<string>('');

    // Estados para cupones
    const [cuponAplicado, setCuponAplicado] = useState<{
        codigo: string;
        descuento: number;
        nuevoTotal: number;
    } | null>(null);

    useEffect(() => {
        if (eventoId) {
            loadAsientos();
        }
    }, [eventoId]);

    // Suscripción a SignalR para actualizaciones en tiempo real
    useAsientosSignalR({
        eventoId: eventoId || '',
        onAsientoReservado: (asientoId, usuarioId) => {
            setAsientos(prev => prev.map(a =>
                a.id === asientoId
                    ? { ...a, reservado: true, usuarioId, estado: 'Ocupado' }
                    : a
            ));

            // Si el asiento estaba en mi selección pero lo reservó otro, notificar y quitar
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
        },
        onAsientoLiberado: (asientoId) => {
            setAsientos(prev => prev.map(a =>
                a.id === asientoId
                    ? { ...a, reservado: false, usuarioId: undefined, estado: 'Disponible' }
                    : a
            ));
        }
    });

    const loadAsientos = async () => {
        try {
            setLoading(true);
            const data = await asientosService.getByEvento(eventoId!);
            setAsientos(data);
        } catch (err: any) {
            setError(err.message || 'Error al cargar los asientos');
        } finally {
            setLoading(false);
        }
    };

    const handleSelectionChange = (selected: Asiento[]) => {
        setSelectedAsientos(selected);
    };

    const subtotal = selectedAsientos.reduce((sum, asiento) => sum + asiento.precio, 0);
    const totalPrice = cuponAplicado ? cuponAplicado.nuevoTotal : subtotal;

    const handlePaymentSuccess = async (transaccionId: string) => {
        setShowPaymentForm(false);
        // La entrada ya fue creada en handleInitiatePayment
        // El consumidor de PagoAprobado en el backend se encargará de marcarla como pagada
        // y de confirmar el asiento.

        alert(`¡Pago procesado!\n\nTransacción: ${transaccionId}\n\nTu entrada está siendo confirmada. En unos momentos aparecerá en "Mis Entradas".`);

        // Redirigir a mis entradas
        navigate('/entradas');
    };

    const handleInitiatePayment = async () => {
        if (selectedAsientos.length === 0) return;

        // Verificar autenticación
        if (!auth.isAuthenticated) {
            alert('Debes iniciar sesión para continuar con la compra');
            auth.signinRedirect();
            return;
        }

        setProcessingPurchase(true);
        try {
            // Crear entradas para todos los asientos seleccionados
            const asientoIds = selectedAsientos.map(a => a.id);

            const response = await entradasService.crearEntrada({
                eventoId: eventoId!,
                usuarioId: auth.user?.profile.sub || '',
                asientoIds: asientoIds, // Enviar todos los IDs
                nombreUsuario: (auth.user?.profile as any)?.preferred_username || auth.user?.profile.name,
                email: auth.user?.profile.email
            });

            // El backend retorna un resumen con todas las entradas creadas
            // El ordenId puede venir como ordenId (múltiples) o id (individual) dentro de data
            const ordenIdVal = response.ordenId || response.data?.ordenId || response.data?.id || response.id;

            if (!ordenIdVal || ordenIdVal === '00000000-0000-0000-0000-000000000000') {
                console.error('Error: Se recibió un OrdenId inválido:', response);
                throw new Error('No se pudo generar un número de orden válido.');
            }

            setOrdenId(ordenIdVal);
            setShowPaymentForm(true);
        } catch (err: any) {
            console.error('Error al crear entradas pre-pago:', err);
            alert('No se pudo iniciar el proceso de compra: ' + (err.response?.data?.detail || err.message));
        } finally {
            setProcessingPurchase(false);
        }
    };

    if (loading) {
        return (
            <div className="min-h-screen bg-black flex items-center justify-center">
                <div className="text-center">
                    <div className="inline-block animate-spin rounded-full h-16 w-16 border-t-2 border-b-2 border-purple-500 mb-4"></div>
                    <p className="text-gray-400">Cargando asientos...</p>
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
                    <button
                        onClick={() => navigate(-1)}
                        className="px-6 py-2 bg-purple-600 hover:bg-purple-700 text-white rounded-lg transition"
                    >
                        Volver
                    </button>
                </div>
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-black text-white">
            {/* Fondo con efectos */}
            <div className="fixed inset-0 overflow-hidden pointer-events-none">
                <div className="absolute top-0 left-1/4 w-96 h-96 bg-purple-500/10 rounded-full blur-3xl"></div>
                <div className="absolute bottom-0 right-1/4 w-96 h-96 bg-cyan-500/10 rounded-full blur-3xl"></div>
            </div>

            <div className="relative container mx-auto px-4 py-8">
                {/* Header */}
                <div className="mb-8">
                    <button
                        onClick={() => navigate(-1)}
                        className="flex items-center gap-2 text-gray-400 hover:text-white transition mb-4"
                    >
                        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
                        </svg>
                        Volver
                    </button>

                    <h1 className="text-4xl font-black bg-gradient-to-r from-purple-400 to-pink-500 bg-clip-text text-transparent">
                        Selecciona tus Asientos
                    </h1>
                    <p className="text-gray-400 mt-2">Elige los mejores lugares para tu experiencia</p>
                </div>

                {/* Layout Principal */}
                <div className="grid lg:grid-cols-3 gap-8">
                    {/* Mapa de Asientos - 2 columnas */}
                    <div className="lg:col-span-2">
                        <div className="bg-gradient-to-br from-gray-900 to-black border border-gray-800 rounded-2xl p-6 shadow-2xl">
                            <SeatMap
                                asientos={asientos}
                                onSelectionChange={handleSelectionChange}
                            />
                        </div>
                    </div>

                    {/* Resumen de Compra - 1 columna */}
                    <div className="lg:col-span-1">
                        <div className="bg-gradient-to-br from-gray-900 to-black border border-gray-800 rounded-2xl p-6 shadow-2xl sticky top-8">
                            <h2 className="text-2xl font-bold mb-6 flex items-center gap-2">
                                <span className="text-2xl">🎫</span>
                                Tu Ticket
                            </h2>

                            {/* Lista de Asientos Seleccionados */}
                            <div className="space-y-3 mb-6 max-h-64 overflow-y-auto custom-scrollbar">
                                {selectedAsientos.length === 0 ? (
                                    <div className="text-center py-8">
                                        <div className="text-gray-600 text-5xl mb-3">💺</div>
                                        <p className="text-gray-500 text-sm">
                                            Selecciona asientos del mapa
                                        </p>
                                    </div>
                                ) : (
                                    selectedAsientos.map((asiento) => (
                                        <div
                                            key={asiento.id}
                                            className="flex items-center justify-between p-3 bg-black/50 rounded-lg border border-gray-800 hover:border-purple-500/50 transition"
                                        >
                                            <div className="flex items-center gap-3">
                                                <div className="w-10 h-10 bg-gradient-to-br from-purple-500 to-pink-500 rounded-lg flex items-center justify-center font-bold text-sm">
                                                    {String.fromCharCode(64 + asiento.fila)}{asiento.numero}
                                                </div>
                                                <div>
                                                    <p className="font-semibold text-sm">
                                                        Fila {String.fromCharCode(64 + asiento.fila)} - Asiento {asiento.numero}
                                                    </p>
                                                    <p className="text-xs text-gray-400 capitalize">{asiento.categoria}</p>
                                                </div>
                                            </div>
                                            <div className="text-right">
                                                <p className="font-bold text-purple-400">${asiento.precio.toFixed(2)}</p>
                                            </div>
                                        </div>
                                    ))
                                )}
                            </div>

                            {/* Separador */}
                            {selectedAsientos.length > 0 && (
                                <div className="border-t border-gray-800 my-6"></div>
                            )}

                            {/* Cupón de Descuento */}
                            {selectedAsientos.length > 0 && (
                                <div className="mb-6">
                                    <CouponInput
                                        eventoId={eventoId!}
                                        montoOriginal={subtotal}
                                        onCouponApplied={(codigo, descuento, nuevoTotal) => {
                                            setCuponAplicado({ codigo, descuento, nuevoTotal });
                                            toast.success(`¡Cupón ${codigo} aplicado! Ahorraste $${descuento.toFixed(2)}`);
                                        }}
                                        onCouponRemoved={() => {
                                            setCuponAplicado(null);
                                            toast.success('Cupón removido');
                                        }}
                                        disabled={processingPurchase}
                                    />
                                </div>
                            )}

                            {/* Resumen de Precio */}
                            <div className="space-y-3 mb-6">
                                <div className="flex justify-between text-gray-400">
                                    <span>Subtotal ({selectedAsientos.length} asiento{selectedAsientos.length !== 1 ? 's' : ''})</span>
                                    <span>${subtotal.toFixed(2)}</span>
                                </div>
                                {cuponAplicado && (
                                    <div className="flex justify-between text-green-400 font-medium animate-in fade-in slide-in-from-top-2 duration-300">
                                        <span className="flex items-center gap-2">
                                            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A1.994 1.994 0 013 12V7a4 4 0 014-4z" />
                                            </svg>
                                            Descuento ({cuponAplicado.codigo})
                                        </span>
                                        <span>-${cuponAplicado.descuento.toFixed(2)}</span>
                                    </div>
                                )}
                                <div className="flex justify-between text-gray-400">
                                    <span>Cargo por servicio</span>
                                    <span>$0.00</span>
                                </div>
                                <div className="border-t border-gray-800 pt-3">
                                    <div className="flex justify-between items-center">
                                        <span className="text-xl font-bold">Total a Pagar</span>
                                        <span className={`text-3xl font-black bg-gradient-to-r ${cuponAplicado ? 'from-green-400 to-emerald-500' : 'from-purple-400 to-pink-500'} bg-clip-text text-transparent transition-all duration-300`}>
                                            ${totalPrice.toFixed(2)}
                                        </span>
                                    </div>
                                    {cuponAplicado && totalPrice === 0 && (
                                        <p className="text-sm text-green-400 mt-2 text-center animate-pulse">
                                            🎉 ¡Entrada gratis con tu cupón!
                                        </p>
                                    )}
                                </div>
                            </div>

                            {/* Botón de Pago */}
                            <button
                                onClick={handleInitiatePayment}
                                disabled={selectedAsientos.length === 0 || processingPurchase}
                                className="w-full py-4 px-6 bg-gradient-to-r from-purple-600 to-pink-600 hover:from-purple-700 hover:to-pink-700 disabled:from-gray-700 disabled:to-gray-800 disabled:cursor-not-allowed text-white font-bold rounded-xl shadow-lg hover:shadow-purple-500/50 transition-all duration-300 transform hover:scale-[1.02] active:scale-[0.98] disabled:transform-none disabled:shadow-none"
                            >
                                {processingPurchase ? (
                                    <span className="flex items-center justify-center gap-2">
                                        <svg className="animate-spin h-5 w-5" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                                            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                                            <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                                        </svg>
                                        Generando entradas...
                                    </span>
                                ) : (
                                    <span className="flex items-center justify-center gap-2">
                                        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 10h18M7 15h1m4 0h1m-7 4h12a3 3 0 003-3V8a3 3 0 00-3-3H6a3 3 0 00-3 3v8a3 3 0 003 3z" />
                                        </svg>
                                        {selectedAsientos.length === 0 ? 'Selecciona Asientos' : 'Proceder al Pago'}
                                    </span>
                                )}
                            </button>

                            {/* Información Adicional */}
                            <div className="mt-6 p-4 bg-purple-500/10 border border-purple-500/30 rounded-lg">
                                <div className="flex gap-2">
                                    <svg className="w-5 h-5 text-purple-400 flex-shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                                    </svg>
                                    <div className="text-xs text-gray-400">
                                        <p className="font-semibold text-purple-400 mb-1">Pago seguro</p>
                                        <p>Tus datos están protegidos con encriptación SSL de última generación.</p>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            {/* Modal de Pago */}
            {showPaymentForm && auth.isAuthenticated && (
                <PaymentForm
                    monto={totalPrice}
                    ordenId={ordenId}
                    usuarioId={auth.user?.profile.sub || ''}
                    onSuccess={handlePaymentSuccess}
                    onCancel={() => setShowPaymentForm(false)}
                    codigoCupon={cuponAplicado?.codigo}
                />
            )}

            <style>{`
        .custom-scrollbar::-webkit-scrollbar {
          width: 4px;
        }
        .custom-scrollbar::-webkit-scrollbar-track {
          background: rgba(31, 41, 55, 0.5);
          border-radius: 10px;
        }
        .custom-scrollbar::-webkit-scrollbar-thumb {
          background: linear-gradient(to bottom, #8b5cf6, #ec4899);
          border-radius: 10px;
        }
      `}</style>
        </div>
    );
}
