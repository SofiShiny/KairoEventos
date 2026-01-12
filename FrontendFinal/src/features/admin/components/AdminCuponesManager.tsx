import { useState, useEffect } from 'react';
import { Tag, Plus, Download, Copy, CheckCircle, XCircle, Clock } from 'lucide-react';
import { pagosService, Cupon } from '../../pagos/services/pagos.service';
import { toast } from 'react-hot-toast';

interface AdminCuponesManagerProps {
    eventoId: string;
}

type TabType = 'crear' | 'lote' | 'lista';

export default function AdminCuponesManager({ eventoId }: AdminCuponesManagerProps) {
    const [activeTab, setActiveTab] = useState<TabType>('lista');
    const [cupones, setCupones] = useState<Cupon[]>([]);
    const [loading, setLoading] = useState(false);
    const [submitting, setSubmitting] = useState(false);

    // Estado para crear cupón general
    const [formGeneral, setFormGeneral] = useState({
        codigo: '',
        porcentajeDescuento: '',
        fechaExpiracion: '',
        esGlobal: false,
        limiteUsos: ''
    });

    // Estado para generar lote
    const [formLote, setFormLote] = useState({
        cantidad: '',
        porcentajeDescuento: '',
        fechaExpiracion: ''
    });

    useEffect(() => {
        loadCupones();
    }, [eventoId]);

    const loadCupones = async () => {
        setLoading(true);
        try {
            const data = await pagosService.getCuponesPorEvento(eventoId);
            setCupones(data || []);
        } catch (error: any) {
            // Si el error es 404 o la lista está vacía, no es un error real
            if (error.message?.includes('404') || error.response?.status === 404) {
                setCupones([]);
            } else {
                console.error('Error al cargar cupones:', error);
                // Solo mostrar toast si es un error real de servidor
                if (error.response?.status >= 500) {
                    toast.error('Error al conectar con el servidor');
                }
            }
        } finally {
            setLoading(false);
        }
    };

    const handleCrearGeneral = async (e: React.FormEvent) => {
        e.preventDefault();

        if (!formGeneral.codigo || !formGeneral.porcentajeDescuento) {
            toast.error('Completa todos los campos requeridos');
            return;
        }

        const porcentaje = parseFloat(formGeneral.porcentajeDescuento);
        if (porcentaje <= 0 || porcentaje > 100) {
            toast.error('El porcentaje debe estar entre 1 y 100');
            return;
        }

        setSubmitting(true);
        try {
            await pagosService.crearCuponGeneral({
                codigo: formGeneral.codigo,
                porcentajeDescuento: porcentaje,
                fechaExpiracion: formGeneral.fechaExpiracion || undefined,
                eventoId: formGeneral.esGlobal ? undefined : eventoId,
                esGlobal: formGeneral.esGlobal,
                limiteUsos: formGeneral.limiteUsos ? parseInt(formGeneral.limiteUsos) : undefined
            });

            toast.success('Cupón creado exitosamente');
            setFormGeneral({ codigo: '', porcentajeDescuento: '', fechaExpiracion: '', esGlobal: false, limiteUsos: '' });
            setActiveTab('lista');
            loadCupones();
        } catch (error: any) {
            toast.error(error.message || 'Error al crear cupón');
        } finally {
            setSubmitting(false);
        }
    };

    const handleGenerarLote = async (e: React.FormEvent) => {
        e.preventDefault();

        if (!formLote.cantidad || !formLote.porcentajeDescuento) {
            toast.error('Completa todos los campos requeridos');
            return;
        }

        const cantidad = parseInt(formLote.cantidad);
        if (cantidad < 1 || cantidad > 1000) {
            toast.error('La cantidad debe estar entre 1 y 1000');
            return;
        }

        const porcentaje = parseFloat(formLote.porcentajeDescuento);
        if (porcentaje <= 0 || porcentaje > 100) {
            toast.error('El porcentaje debe estar entre 1 y 100');
            return;
        }

        setSubmitting(true);
        try {
            const cupones = await pagosService.generarLoteCupones({
                cantidad,
                porcentajeDescuento: porcentaje,
                eventoId,
                fechaExpiracion: formLote.fechaExpiracion || undefined
            });

            toast.success(`${cupones.length} cupones generados exitosamente`);
            setFormLote({ cantidad: '', porcentajeDescuento: '', fechaExpiracion: '' });
            setActiveTab('lista');
            loadCupones();
        } catch (error: any) {
            toast.error(error.message || 'Error al generar cupones');
        } finally {
            setSubmitting(false);
        }
    };

    const handleCopiarCodigos = () => {
        const codigos = cupones.map(c => c.codigo).join('\n');
        navigator.clipboard.writeText(codigos);
        toast.success('Códigos copiados al portapapeles');
    };

    const handleExportarCSV = () => {
        const csv = [
            'Código,Porcentaje Descuento,Tipo,Estado,Fecha Expiración',
            ...cupones.map(c =>
                `${c.codigo},${c.porcentajeDescuento}%,${c.tipo},${c.estado},${c.fechaExpiracion || 'Sin expiración'}`
            )
        ].join('\n');

        const blob = new Blob([csv], { type: 'text/csv' });
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `cupones-evento-${eventoId}.csv`;
        a.click();
        toast.success('Cupones exportados');
    };

    const getEstadoBadge = (estado: string) => {
        const styles = {
            Activo: 'bg-green-500/20 text-green-400 border-green-500/30',
            Usado: 'bg-blue-500/20 text-blue-400 border-blue-500/30',
            Expirado: 'bg-red-500/20 text-red-400 border-red-500/30'
        };

        const icons = {
            Activo: CheckCircle,
            Usado: XCircle,
            Expirado: Clock
        };

        const Icon = icons[estado as keyof typeof icons] || Clock;

        return (
            <span className={`inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-medium border ${styles[estado as keyof typeof styles] || styles.Expirado}`}>
                <Icon className="w-3.5 h-3.5" />
                {estado}
            </span>
        );
    };

    return (
        <div className="bg-neutral-900/50 rounded-2xl border border-neutral-800 overflow-hidden">
            {/* Header con pestañas */}
            <div className="border-b border-neutral-800">
                <div className="flex items-center gap-1 p-1">
                    <button
                        onClick={() => setActiveTab('lista')}
                        className={`flex-1 px-4 py-3 rounded-lg font-medium transition-all ${activeTab === 'lista'
                            ? 'bg-neutral-800 text-white'
                            : 'text-neutral-400 hover:text-white hover:bg-neutral-800/50'
                            }`}
                    >
                        <Tag className="w-4 h-4 inline mr-2" />
                        Lista de Cupones
                    </button>
                    <button
                        onClick={() => setActiveTab('crear')}
                        className={`flex-1 px-4 py-3 rounded-lg font-medium transition-all ${activeTab === 'crear'
                            ? 'bg-neutral-800 text-white'
                            : 'text-neutral-400 hover:text-white hover:bg-neutral-800/50'
                            }`}
                    >
                        <Plus className="w-4 h-4 inline mr-2" />
                        Crear Promoción
                    </button>
                    <button
                        onClick={() => setActiveTab('lote')}
                        className={`flex-1 px-4 py-3 rounded-lg font-medium transition-all ${activeTab === 'lote'
                            ? 'bg-neutral-800 text-white'
                            : 'text-neutral-400 hover:text-white hover:bg-neutral-800/50'
                            }`}
                    >
                        <Download className="w-4 h-4 inline mr-2" />
                        Generar Lote
                    </button>
                </div>
            </div>

            {/* Contenido */}
            <div className="p-6">
                {/* Tab: Lista de Cupones */}
                {activeTab === 'lista' && (
                    <div className="space-y-4">
                        <div className="flex items-center justify-between">
                            <h3 className="text-lg font-semibold text-white">
                                Cupones Activos ({cupones.length})
                            </h3>
                            {cupones.length > 0 && (
                                <div className="flex gap-2">
                                    <button
                                        onClick={handleCopiarCodigos}
                                        className="px-4 py-2 bg-neutral-800 hover:bg-neutral-700 text-white rounded-lg transition-colors flex items-center gap-2"
                                    >
                                        <Copy className="w-4 h-4" />
                                        Copiar Todos
                                    </button>
                                    <button
                                        onClick={handleExportarCSV}
                                        className="px-4 py-2 bg-blue-600 hover:bg-blue-500 text-white rounded-lg transition-colors flex items-center gap-2"
                                    >
                                        <Download className="w-4 h-4" />
                                        Exportar CSV
                                    </button>
                                </div>
                            )}
                        </div>

                        {loading ? (
                            <div className="text-center py-12 text-neutral-400">
                                Cargando cupones...
                            </div>
                        ) : cupones.length === 0 ? (
                            <div className="text-center py-12">
                                <Tag className="w-12 h-12 mx-auto text-neutral-600 mb-3" />
                                <p className="text-neutral-400">No hay cupones creados</p>
                                <p className="text-sm text-neutral-500 mt-1">
                                    Crea una promoción o genera un lote de cupones
                                </p>
                            </div>
                        ) : (
                            <div className="overflow-x-auto">
                                <table className="w-full">
                                    <thead>
                                        <tr className="border-b border-neutral-800">
                                            <th className="text-left py-3 px-4 text-sm font-medium text-neutral-400">Código</th>
                                            <th className="text-left py-3 px-4 text-sm font-medium text-neutral-400">Descuento</th>
                                            <th className="text-left py-3 px-4 text-sm font-medium text-neutral-400">Tipo</th>
                                            <th className="text-left py-3 px-4 text-sm font-medium text-neutral-400">Estado</th>
                                            <th className="text-left py-3 px-4 text-sm font-medium text-neutral-400">Expira</th>
                                            <th className="text-left py-3 px-4 text-sm font-medium text-neutral-400">Usos</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {cupones.map((cupon) => (
                                            <tr key={cupon.id} className="border-b border-neutral-800/50 hover:bg-neutral-800/30 transition-colors">
                                                <td className="py-3 px-4">
                                                    <code className="bg-neutral-800 px-2 py-1 rounded text-sm font-mono text-blue-400">
                                                        {cupon.codigo}
                                                    </code>
                                                </td>
                                                <td className="py-3 px-4 text-white font-medium">
                                                    {cupon.porcentajeDescuento}%
                                                </td>
                                                <td className="py-3 px-4">
                                                    <span className={`px-2 py-1 rounded text-xs ${cupon.tipo === 'General'
                                                        ? 'bg-purple-500/20 text-purple-400'
                                                        : 'bg-orange-500/20 text-orange-400'
                                                        }`}>
                                                        {cupon.tipo}
                                                    </span>
                                                </td>
                                                <td className="py-3 px-4">
                                                    {getEstadoBadge(cupon.estado)}
                                                </td>
                                                <td className="py-3 px-4 text-sm text-neutral-400">
                                                    {cupon.fechaExpiracion
                                                        ? new Date(cupon.fechaExpiracion).toLocaleDateString()
                                                        : 'Sin expiración'
                                                    }
                                                </td>
                                                <td className="py-3 px-4 text-sm text-neutral-400">
                                                    {cupon.tipo === 'Unico' ? '1 uso' : 'Ilimitado'}
                                                </td>
                                            </tr>
                                        ))}
                                    </tbody>
                                </table>
                            </div>
                        )}
                    </div>
                )}

                {/* Tab: Crear Promoción */}
                {activeTab === 'crear' && (
                    <form onSubmit={handleCrearGeneral} className="space-y-6">
                        <div>
                            <h3 className="text-lg font-semibold text-white mb-4">
                                Crear Cupón General
                            </h3>
                            <p className="text-sm text-neutral-400 mb-6">
                                Un cupón general puede ser usado por múltiples usuarios con el mismo código
                            </p>
                        </div>

                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                            <div>
                                <label className="block text-sm font-medium text-neutral-300 mb-2">
                                    Código del Cupón *
                                </label>
                                <input
                                    type="text"
                                    value={formGeneral.codigo}
                                    onChange={(e) => setFormGeneral({ ...formGeneral, codigo: e.target.value.toUpperCase() })}
                                    placeholder="Ej: PROMO2026"
                                    required
                                    className="w-full px-4 py-2.5 bg-neutral-800/50 border border-neutral-700 rounded-lg text-white placeholder-neutral-500 focus:outline-none focus:ring-2 focus:ring-blue-500/50 uppercase font-mono"
                                />
                            </div>

                            <div>
                                <label className="block text-sm font-medium text-neutral-300 mb-2">
                                    Porcentaje de Descuento (%) *
                                </label>
                                <input
                                    type="number"
                                    step="0.01"
                                    min="1"
                                    max="100"
                                    value={formGeneral.porcentajeDescuento}
                                    onChange={(e) => setFormGeneral({ ...formGeneral, porcentajeDescuento: e.target.value })}
                                    placeholder="10"
                                    required
                                    className="w-full px-4 py-2.5 bg-neutral-800/50 border border-neutral-700 rounded-lg text-white placeholder-neutral-500 focus:outline-none focus:ring-2 focus:ring-blue-500/50"
                                />
                                <p className="text-xs text-neutral-500 mt-1">Ej: 10 = 10% de descuento, 50 = 50% de descuento</p>
                            </div>

                            <div>
                                <label className="block text-sm font-medium text-neutral-300 mb-2">
                                    Fecha de Expiración (Opcional)
                                </label>
                                <input
                                    type="date"
                                    value={formGeneral.fechaExpiracion}
                                    onChange={(e) => setFormGeneral({ ...formGeneral, fechaExpiracion: e.target.value })}
                                    className="w-full px-4 py-2.5 bg-neutral-800/50 border border-neutral-700 rounded-lg text-white focus:outline-none focus:ring-2 focus:ring-blue-500/50"
                                />
                            </div>

                            <div>
                                <label className="block text-sm font-medium text-neutral-300 mb-2">
                                    Límite de Usos (Opcional)
                                </label>
                                <input
                                    type="number"
                                    min="1"
                                    value={formGeneral.limiteUsos}
                                    onChange={(e) => setFormGeneral({ ...formGeneral, limiteUsos: e.target.value })}
                                    placeholder="100"
                                    className="w-full px-4 py-2.5 bg-neutral-800/50 border border-neutral-700 rounded-lg text-white placeholder-neutral-500 focus:outline-none focus:ring-2 focus:ring-blue-500/50"
                                />
                                <p className="text-xs text-neutral-500 mt-1">Dejar vacío para usos ilimitados</p>
                            </div>

                            <div className="flex items-center">
                                <label className="flex items-center gap-3 cursor-pointer">
                                    <input
                                        type="checkbox"
                                        checked={formGeneral.esGlobal}
                                        onChange={(e) => setFormGeneral({ ...formGeneral, esGlobal: e.target.checked })}
                                        className="w-5 h-5 rounded border-neutral-700 bg-neutral-800 text-blue-600 focus:ring-2 focus:ring-blue-500/50"
                                    />
                                    <span className="text-sm text-neutral-300">
                                        Cupón global (válido para todos los eventos)
                                    </span>
                                </label>
                            </div>
                        </div>

                        <div className="flex gap-3">
                            <button
                                type="submit"
                                disabled={submitting}
                                className="px-6 py-3 bg-gradient-to-r from-blue-600 to-blue-700 hover:from-blue-500 hover:to-blue-600 text-white font-medium rounded-lg transition-all disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2"
                            >
                                {submitting ? 'Creando...' : 'Crear Cupón'}
                            </button>
                            <button
                                type="button"
                                onClick={() => setActiveTab('lista')}
                                className="px-6 py-3 bg-neutral-800 hover:bg-neutral-700 text-white font-medium rounded-lg transition-colors"
                            >
                                Cancelar
                            </button>
                        </div>
                    </form>
                )}

                {/* Tab: Generar Lote */}
                {activeTab === 'lote' && (
                    <form onSubmit={handleGenerarLote} className="space-y-6">
                        <div>
                            <h3 className="text-lg font-semibold text-white mb-4">
                                Generar Lote de Cupones
                            </h3>
                            <p className="text-sm text-neutral-400 mb-6">
                                Genera múltiples cupones únicos de un solo uso con códigos aleatorios
                            </p>
                        </div>

                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                            <div>
                                <label className="block text-sm font-medium text-neutral-300 mb-2">
                                    Cantidad de Cupones *
                                </label>
                                <input
                                    type="number"
                                    min="1"
                                    max="1000"
                                    value={formLote.cantidad}
                                    onChange={(e) => setFormLote({ ...formLote, cantidad: e.target.value })}
                                    placeholder="50"
                                    required
                                    className="w-full px-4 py-2.5 bg-neutral-800/50 border border-neutral-700 rounded-lg text-white placeholder-neutral-500 focus:outline-none focus:ring-2 focus:ring-blue-500/50"
                                />
                                <p className="text-xs text-neutral-500 mt-1">Máximo 1000 cupones por lote</p>
                            </div>

                            <div>
                                <label className="block text-sm font-medium text-neutral-300 mb-2">
                                    Porcentaje de Descuento (%) *
                                </label>
                                <input
                                    type="number"
                                    step="0.01"
                                    min="1"
                                    max="100"
                                    value={formLote.porcentajeDescuento}
                                    onChange={(e) => setFormLote({ ...formLote, porcentajeDescuento: e.target.value })}
                                    placeholder="15"
                                    required
                                    className="w-full px-4 py-2.5 bg-neutral-800/50 border border-neutral-700 rounded-lg text-white placeholder-neutral-500 focus:outline-none focus:ring-2 focus:ring-blue-500/50"
                                />
                                <p className="text-xs text-neutral-500 mt-1">Ej: 15 = 15% de descuento para cada cupón</p>
                            </div>

                            <div>
                                <label className="block text-sm font-medium text-neutral-300 mb-2">
                                    Fecha de Expiración (Opcional)
                                </label>
                                <input
                                    type="date"
                                    value={formLote.fechaExpiracion}
                                    onChange={(e) => setFormLote({ ...formLote, fechaExpiracion: e.target.value })}
                                    className="w-full px-4 py-2.5 bg-neutral-800/50 border border-neutral-700 rounded-lg text-white focus:outline-none focus:ring-2 focus:ring-blue-500/50"
                                />
                            </div>
                        </div>

                        <div className="bg-blue-500/10 border border-blue-500/30 rounded-lg p-4">
                            <p className="text-sm text-blue-300">
                                💡 <strong>Tip:</strong> Los códigos generados serán aleatorios y únicos.
                                Podrás exportarlos en CSV para enviarlos por email a tus clientes.
                            </p>
                        </div>

                        <div className="flex gap-3">
                            <button
                                type="submit"
                                disabled={submitting}
                                className="px-6 py-3 bg-gradient-to-r from-purple-600 to-purple-700 hover:from-purple-500 hover:to-purple-600 text-white font-medium rounded-lg transition-all disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2"
                            >
                                {submitting ? 'Generando...' : 'Generar Lote'}
                            </button>
                            <button
                                type="button"
                                onClick={() => setActiveTab('lista')}
                                className="px-6 py-3 bg-neutral-800 hover:bg-neutral-700 text-white font-medium rounded-lg transition-colors"
                            >
                                Cancelar
                            </button>
                        </div>
                    </form>
                )}
            </div>
        </div >
    );
}
