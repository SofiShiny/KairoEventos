import { useState } from 'react';
import { Tag, Check, X, Loader2 } from 'lucide-react';

interface CouponInputProps {
    onCouponApplied: (codigo: string, descuento: number, nuevoTotal: number) => void;
    onCouponRemoved: () => void;
    eventoId: string;
    montoOriginal: number;
    disabled?: boolean;
}

export default function CouponInput({
    onCouponApplied,
    onCouponRemoved,
    eventoId,
    montoOriginal,
    disabled = false
}: CouponInputProps) {
    const [codigo, setCodigo] = useState('');
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');
    const [cuponAplicado, setCuponAplicado] = useState<{
        codigo: string;
        descuento: number;
        nuevoTotal: number;
    } | null>(null);

    const handleValidarCupon = async () => {
        if (!codigo.trim()) {
            setError('Ingresa un código de cupón');
            return;
        }

        setLoading(true);
        setError('');

        try {
            // Importar el servicio dinámicamente para evitar problemas de dependencias circulares
            const { pagosService } = await import('../../pagos/services/pagos.service');

            const resultado = await pagosService.validarCupon(
                codigo.toUpperCase(),
                eventoId,
                montoOriginal
            );

            if (resultado.esValido) {
                const cuponData = {
                    codigo: codigo.toUpperCase(),
                    descuento: resultado.descuento,
                    nuevoTotal: resultado.nuevoTotal
                };

                setCuponAplicado(cuponData);
                onCouponApplied(cuponData.codigo, cuponData.descuento, cuponData.nuevoTotal);
                setError('');
            } else {
                setError(resultado.mensaje);
            }
        } catch (err: any) {
            setError(err.message || 'Error al validar el cupón');
        } finally {
            setLoading(false);
        }
    };

    const handleRemoverCupon = () => {
        setCuponAplicado(null);
        setCodigo('');
        setError('');
        onCouponRemoved();
    };

    const handleKeyPress = (e: React.KeyboardEvent) => {
        if (e.key === 'Enter') {
            handleValidarCupon();
        }
    };

    if (cuponAplicado) {
        return (
            <div className="bg-gradient-to-r from-green-500/10 to-emerald-500/10 border border-green-500/30 rounded-xl p-4 animate-in fade-in slide-in-from-top-2 duration-300">
                <div className="flex items-center justify-between">
                    <div className="flex items-center gap-3">
                        <div className="bg-green-500/20 p-2 rounded-lg">
                            <Check className="w-5 h-5 text-green-400" />
                        </div>
                        <div>
                            <p className="text-sm font-medium text-green-400">
                                Cupón aplicado: {cuponAplicado.codigo}
                            </p>
                            <p className="text-xs text-green-300/70">
                                Ahorraste ${cuponAplicado.descuento.toFixed(2)}
                            </p>
                        </div>
                    </div>
                    <button
                        onClick={handleRemoverCupon}
                        disabled={disabled}
                        className="text-red-400 hover:text-red-300 transition-colors disabled:opacity-50"
                        title="Quitar cupón"
                    >
                        <X className="w-5 h-5" />
                    </button>
                </div>
            </div>
        );
    }

    return (
        <div className="space-y-3">
            <div className="flex items-center gap-2">
                <Tag className="w-4 h-4 text-neutral-400" />
                <label className="text-sm font-medium text-neutral-300">
                    ¿Tienes un cupón de descuento?
                </label>
            </div>

            <div className="flex gap-2">
                <div className="flex-1">
                    <input
                        type="text"
                        value={codigo}
                        onChange={(e) => {
                            setCodigo(e.target.value.toUpperCase());
                            setError('');
                        }}
                        onKeyPress={handleKeyPress}
                        placeholder="Ej: PROMO2026"
                        disabled={disabled || loading}
                        className="w-full px-4 py-2.5 bg-neutral-800/50 border border-neutral-700 rounded-lg text-white placeholder-neutral-500 focus:outline-none focus:ring-2 focus:ring-blue-500/50 focus:border-blue-500 transition-all disabled:opacity-50 disabled:cursor-not-allowed uppercase"
                    />
                </div>
                <button
                    onClick={handleValidarCupon}
                    disabled={disabled || loading || !codigo.trim()}
                    className="px-6 py-2.5 bg-gradient-to-r from-blue-600 to-blue-700 hover:from-blue-500 hover:to-blue-600 text-white font-medium rounded-lg transition-all disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2 shadow-lg shadow-blue-500/20"
                >
                    {loading ? (
                        <>
                            <Loader2 className="w-4 h-4 animate-spin" />
                            <span>Validando...</span>
                        </>
                    ) : (
                        <span>Aplicar</span>
                    )}
                </button>
            </div>

            {error && (
                <div className="flex items-center gap-2 text-red-400 text-sm animate-in fade-in slide-in-from-top-1 duration-200">
                    <X className="w-4 h-4" />
                    <span>{error}</span>
                </div>
            )}

            <p className="text-xs text-neutral-500">
                Los cupones son sensibles a mayúsculas y pueden tener restricciones por evento
            </p>
        </div>
    );
}
