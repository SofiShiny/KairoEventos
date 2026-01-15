import { useState } from 'react';
import { pagosService, PagoRequest } from '../services/pagos.service';

interface PaymentFormProps {
    monto: number;
    ordenId: string;
    usuarioId: string;
    onSuccess: (transaccionId: string) => void;
    onCancel: () => void;
    codigoCupon?: string; // Código de cupón opcional
}

export default function PaymentForm({ monto, ordenId, usuarioId, onSuccess, onCancel, codigoCupon }: PaymentFormProps) {
    const [formData, setFormData] = useState({
        tarjetaNumero: '',
        titular: '',
        expiracion: '',
        cvv: ''
    });

    const [errors, setErrors] = useState<Record<string, string>>({});
    const [isProcessing, setIsProcessing] = useState(false);
    const [paymentError, setPaymentError] = useState('');

    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { name, value } = e.target;
        let formattedValue = value;

        // Formateo automático
        if (name === 'tarjetaNumero') {
            const cleaned = value.replace(/\s/g, '');
            if (cleaned.length <= 16) {
                formattedValue = pagosService.formatearNumeroTarjeta(cleaned);
            } else {
                return;
            }
        } else if (name === 'expiracion') {
            const cleaned = value.replace(/\D/g, '');
            if (cleaned.length <= 4) {
                if (cleaned.length >= 2) {
                    formattedValue = cleaned.slice(0, 2) + '/' + cleaned.slice(2);
                } else {
                    formattedValue = cleaned;
                }
            } else {
                return;
            }
        } else if (name === 'cvv') {
            if (value.length <= 4 && /^\d*$/.test(value)) {
                formattedValue = value;
            } else {
                return;
            }
        }

        setFormData(prev => ({ ...prev, [name]: formattedValue }));

        // Limpiar error del campo
        if (errors[name]) {
            setErrors(prev => ({ ...prev, [name]: '' }));
        }
        setPaymentError('');
    };

    const validateForm = (): boolean => {
        const newErrors: Record<string, string> = {};

        if (!formData.titular.trim()) {
            newErrors.titular = 'El nombre del titular es requerido';
        }

        if (!pagosService.validarNumeroTarjeta(formData.tarjetaNumero)) {
            newErrors.tarjetaNumero = 'Número de tarjeta inválido';
        }

        if (!pagosService.validarExpiracion(formData.expiracion)) {
            newErrors.expiracion = 'Fecha de expiración inválida o vencida';
        }

        if (!pagosService.validarCVV(formData.cvv)) {
            newErrors.cvv = 'CVV inválido';
        }

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        if (!validateForm()) return;

        setIsProcessing(true);
        setPaymentError('');

        try {
            const pagoRequest: PagoRequest = {
                ordenId: ordenId,
                usuarioId: usuarioId,
                tarjeta: formData.tarjetaNumero.replace(/\s/g, ''),
                titular: formData.titular,
                expiracion: formData.expiracion,
                cvv: formData.cvv,
                monto: monto,
                moneda: 'USD',
                codigoCupon: codigoCupon // Incluir cupón si existe
            };

            const response = await pagosService.procesarPago(pagoRequest);

            if (response.exito) {
                onSuccess(response.transaccionId);
            } else {
                setPaymentError(response.mensaje);
            }
        } catch (error: any) {
            setPaymentError(error.message || 'Error al procesar el pago');
        } finally {
            setIsProcessing(false);
        }
    };

    const getCardType = (numero: string): string => {
        const cleaned = numero.replace(/\s/g, '');
        if (cleaned.startsWith('4')) return 'Visa';
        if (cleaned.startsWith('5')) return 'Mastercard';
        if (cleaned.startsWith('3')) return 'Amex';
        return 'Tarjeta';
    };

    return (
        <div className="fixed inset-0 bg-black/80 backdrop-blur-sm flex items-center justify-center z-50 p-4">
            <div className="bg-gradient-to-br from-gray-900 to-black border border-gray-800 rounded-2xl max-w-md w-full shadow-2xl">
                {/* Header */}
                <div className="p-6 border-b border-gray-800">
                    <div className="flex items-center justify-between mb-2">
                        <h2 className="text-2xl font-bold text-white">Pago Seguro</h2>
                        <button
                            onClick={onCancel}
                            className="text-gray-400 hover:text-white transition"
                            disabled={isProcessing}
                        >
                            <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                            </svg>
                        </button>
                    </div>
                    <p className="text-gray-400 text-sm">Completa los datos de tu tarjeta</p>
                </div>

                {/* Form */}
                <form onSubmit={handleSubmit} className="p-6 space-y-4">
                    {/* Tarjeta Visual */}
                    <div className="relative h-48 bg-gradient-to-br from-purple-600 via-pink-600 to-blue-600 rounded-xl p-6 shadow-lg mb-6">
                        <div className="absolute top-4 right-4">
                            <div className="w-12 h-8 bg-yellow-400/30 rounded backdrop-blur-sm"></div>
                        </div>

                        <div className="flex flex-col justify-between h-full">
                            <div className="text-white/80 text-xs font-semibold">
                                {getCardType(formData.tarjetaNumero)}
                            </div>

                            <div className="text-white text-lg font-mono tracking-wider">
                                {formData.tarjetaNumero || '•••• •••• •••• ••••'}
                            </div>

                            <div className="flex justify-between items-end">
                                <div>
                                    <div className="text-white/60 text-xs mb-1">TITULAR</div>
                                    <div className="text-white text-sm font-semibold uppercase">
                                        {formData.titular || 'NOMBRE APELLIDO'}
                                    </div>
                                </div>
                                <div>
                                    <div className="text-white/60 text-xs mb-1">VENCE</div>
                                    <div className="text-white text-sm font-semibold">
                                        {formData.expiracion || 'MM/YY'}
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                    {/* Número de Tarjeta */}
                    <div>
                        <label className="block text-sm font-medium text-gray-300 mb-2">
                            Número de Tarjeta
                        </label>
                        <input
                            type="text"
                            name="tarjetaNumero"
                            value={formData.tarjetaNumero}
                            onChange={handleInputChange}
                            placeholder="0000 0000 0000 0000"
                            className={`w-full px-4 py-3 bg-black/50 border ${errors.tarjetaNumero ? 'border-red-500' : 'border-gray-700'
                                } rounded-lg text-white placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-transparent transition font-mono`}
                            disabled={isProcessing}
                        />
                        {errors.tarjetaNumero && (
                            <p className="mt-1 text-sm text-red-400">{errors.tarjetaNumero}</p>
                        )}
                    </div>

                    {/* Titular */}
                    <div>
                        <label className="block text-sm font-medium text-gray-300 mb-2">
                            Nombre del Titular
                        </label>
                        <input
                            type="text"
                            name="titular"
                            value={formData.titular}
                            onChange={handleInputChange}
                            placeholder="Como aparece en la tarjeta"
                            className={`w-full px-4 py-3 bg-black/50 border ${errors.titular ? 'border-red-500' : 'border-gray-700'
                                } rounded-lg text-white placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-transparent transition uppercase`}
                            disabled={isProcessing}
                        />
                        {errors.titular && (
                            <p className="mt-1 text-sm text-red-400">{errors.titular}</p>
                        )}
                    </div>

                    {/* Expiración y CVV */}
                    <div className="grid grid-cols-2 gap-4">
                        <div>
                            <label className="block text-sm font-medium text-gray-300 mb-2">
                                Expiración
                            </label>
                            <input
                                type="text"
                                name="expiracion"
                                value={formData.expiracion}
                                onChange={handleInputChange}
                                placeholder="MM/YY"
                                className={`w-full px-4 py-3 bg-black/50 border ${errors.expiracion ? 'border-red-500' : 'border-gray-700'
                                    } rounded-lg text-white placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-transparent transition font-mono`}
                                disabled={isProcessing}
                            />
                            {errors.expiracion && (
                                <p className="mt-1 text-sm text-red-400">{errors.expiracion}</p>
                            )}
                        </div>

                        <div>
                            <label className="block text-sm font-medium text-gray-300 mb-2">
                                CVV
                            </label>
                            <input
                                type="text"
                                name="cvv"
                                value={formData.cvv}
                                onChange={handleInputChange}
                                placeholder="123"
                                className={`w-full px-4 py-3 bg-black/50 border ${errors.cvv ? 'border-red-500' : 'border-gray-700'
                                    } rounded-lg text-white placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-transparent transition font-mono text-center`}
                                disabled={isProcessing}
                            />
                            {errors.cvv && (
                                <p className="mt-1 text-sm text-red-400">{errors.cvv}</p>
                            )}
                        </div>
                    </div>

                    {/* Error de Pago */}
                    {paymentError && (
                        <div className="p-4 bg-red-500/10 border border-red-500/50 rounded-lg">
                            <p className="text-red-400 text-sm flex items-center gap-2">
                                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                                </svg>
                                {paymentError}
                            </p>
                        </div>
                    )}

                    {/* Botón de Pago */}
                    <button
                        type="submit"
                        disabled={isProcessing}
                        className="w-full py-4 px-6 bg-gradient-to-r from-purple-600 to-pink-600 hover:from-purple-700 hover:to-pink-700 disabled:from-gray-700 disabled:to-gray-800 text-white font-bold rounded-xl shadow-lg hover:shadow-purple-500/50 transition-all duration-300 transform hover:scale-[1.02] active:scale-[0.98] disabled:transform-none disabled:shadow-none disabled:cursor-not-allowed"
                    >
                        {isProcessing ? (
                            <span className="flex items-center justify-center gap-2">
                                <svg className="animate-spin h-5 w-5" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                                </svg>
                                Procesando...
                            </span>
                        ) : (
                            <span className="flex items-center justify-center gap-2">
                                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
                                </svg>
                                Pagar ${monto.toFixed(2)}
                            </span>
                        )}
                    </button>

                    {/* Seguridad */}
                    <div className="flex items-center justify-center gap-2 text-xs text-gray-500 pt-2">
                        <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
                        </svg>
                        Pago seguro encriptado SSL
                    </div>
                </form>
            </div>
        </div>
    );
}
