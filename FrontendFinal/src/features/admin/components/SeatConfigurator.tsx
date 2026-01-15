import { useState, useEffect } from 'react';
import { X, Save, Plus, Armchair, Loader2, AlertCircle } from 'lucide-react';
import { adminAsientosService } from '../services/admin.asientos.service';
import { Evento } from '../../eventos/types/evento.types';

interface SeatConfiguratorProps {
    evento: Evento;
    onClose: () => void;
}

export default function SeatConfigurator({ evento, onClose }: SeatConfiguratorProps) {
    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [mapaId, setMapaId] = useState<string | null>(null);
    const [categorias, setCategorias] = useState<any[]>([]);

    // Formulario para nueva categoría
    const [newCat, setNewCat] = useState({ nombre: '', precio: 0 });

    // Formulario para lote de asientos
    const [batchSeats, setBatchSeats] = useState({ fila: 1, cantidad: 10, categoria: '' });

    useEffect(() => {
        cargarMapa();
    }, [evento.id]);

    const cargarMapa = async () => {
        try {
            setLoading(true);
            const mapa = await adminAsientosService.getMapaByEvento(evento.id);
            if (mapa) {
                setMapaId(mapa.id);
                if (mapa.categorias) {
                    setCategorias(mapa.categorias.map((c: any) => ({
                        nombre: c.nombre,
                        precioBase: c.precioBase
                    })));
                }
            }
        } catch (err: any) {
            console.error(err);
            setError('Error al cargar la configuración de asientos');
        } finally {
            setLoading(false);
        }
    };

    const handleCreateMapa = async () => {
        try {
            setSaving(true);
            const res = await adminAsientosService.createMapa(evento.id);
            setMapaId(res.mapaId);
        } catch (err: any) {
            setError('No se pudo crear el mapa de asientos');
        } finally {
            setSaving(false);
        }
    };

    const handleAddCategoria = async () => {
        if (!mapaId || !newCat.nombre) return;
        try {
            setSaving(true);
            await adminAsientosService.createCategoria({
                mapaId,
                nombre: newCat.nombre,
                precioBase: newCat.precio,
                tienePrioridad: false
            });
            setCategorias([...categorias, { nombre: newCat.nombre, precioBase: newCat.precio }]);
            setNewCat({ nombre: '', precio: 0 });
        } catch (err) {
            setError('Error al crear categoría');
        } finally {
            setSaving(false);
        }
    };

    const handleBatchCreate = async () => {
        if (!mapaId || !batchSeats.categoria) return;
        try {
            setSaving(true);
            for (let i = 1; i <= batchSeats.cantidad; i++) {
                await adminAsientosService.createAsiento({
                    mapaId,
                    fila: batchSeats.fila,
                    numero: i,
                    categoria: batchSeats.categoria
                });
            }
            alert(`Sincronizados ${batchSeats.cantidad} asientos exitosamente`);
        } catch (err) {
            setError('Error al crear lote de asientos');
        } finally {
            setSaving(false);
        }
    };

    return (
        <div className="fixed inset-0 z-[70] flex items-center justify-center p-4">
            <div className="absolute inset-0 bg-black/90 backdrop-blur-md animate-in fade-in" onClick={onClose} />

            <div className="relative w-full max-w-2xl bg-[#16191f] border border-slate-800 rounded-3xl shadow-2xl overflow-hidden flex flex-col max-h-[90vh]">
                {/* Header */}
                <div className="px-8 py-6 border-b border-slate-800 flex justify-between items-center bg-slate-900/50">
                    <div className="flex items-center gap-3">
                        <div className="w-10 h-10 bg-blue-600/20 rounded-xl flex items-center justify-center text-blue-500">
                            <Armchair className="w-5 h-5" />
                        </div>
                        <div>
                            <h2 className="text-xl font-black text-white italic">Configurar Aforos</h2>
                            <p className="text-xs text-slate-500 font-bold uppercase tracking-widest">{evento.titulo}</p>
                        </div>
                    </div>
                    <button onClick={onClose} className="p-2 hover:bg-slate-800 text-slate-400 hover:text-white rounded-xl transition-all">
                        <X className="w-6 h-6" />
                    </button>
                </div>

                <div className="p-8 overflow-y-auto space-y-8 custom-scrollbar">
                    {loading ? (
                        <div className="flex flex-col items-center justify-center py-12">
                            <Loader2 className="w-8 h-8 animate-spin text-blue-500 mb-4" />
                            <p className="text-slate-400 font-bold text-xs uppercase tracking-widest">Consultando Microservicio de Asientos...</p>
                        </div>
                    ) : !mapaId ? (
                        <div className="text-center py-12 space-y-6">
                            <div className="w-20 h-20 bg-slate-800 rounded-full flex items-center justify-center mx-auto text-slate-600">
                                <Armchair className="w-10 h-10" />
                            </div>
                            <div>
                                <h3 className="text-lg font-bold text-white">Sin Mapa de Asientos</h3>
                                <p className="text-slate-500 text-sm max-w-xs mx-auto mt-2">Este evento aún no tiene una configuración de asientos. Debes inicializarla para empezar a asignar lugares.</p>
                            </div>
                            <button
                                onClick={handleCreateMapa}
                                disabled={saving}
                                className="bg-blue-600 hover:bg-blue-700 text-white px-8 py-3 rounded-2xl font-black text-sm transition-all"
                            >
                                {saving ? 'Inicializando...' : 'Inicializar Mapa para este Evento'}
                            </button>
                        </div>
                    ) : (
                        <>
                            {error && (
                                <div className="bg-rose-500/10 border border-rose-500/20 p-4 rounded-xl flex items-center gap-3 text-rose-500">
                                    <AlertCircle className="w-5 h-5" />
                                    <p className="text-xs font-bold">{error}</p>
                                </div>
                            )}

                            {/* Seccion Categorias */}
                            <div className="space-y-4">
                                <h3 className="text-xs font-black text-slate-500 uppercase tracking-widest flex items-center gap-2">
                                    <div className="w-1.5 h-1.5 bg-blue-500 rounded-full"></div>
                                    1. Definir Categorías de Precios
                                </h3>
                                <div className="flex gap-2">
                                    <input
                                        type="text"
                                        placeholder="Ej: VIP, General"
                                        className="flex-2 bg-slate-900 border border-slate-700 rounded-xl px-4 py-2 text-sm text-white"
                                        value={newCat.nombre}
                                        onChange={e => setNewCat({ ...newCat, nombre: e.target.value })}
                                    />
                                    <input
                                        type="number"
                                        placeholder="Precio $"
                                        className="w-24 bg-slate-900 border border-slate-700 rounded-xl px-4 py-2 text-sm text-white"
                                        value={newCat.precio}
                                        onChange={e => setNewCat({ ...newCat, precio: Number(e.target.value) })}
                                    />
                                    <button
                                        onClick={handleAddCategoria}
                                        className="p-2 bg-slate-800 text-white rounded-xl hover:bg-slate-700"
                                    >
                                        <Plus className="w-5 h-5" />
                                    </button>
                                </div>
                                <div className="flex flex-wrap gap-2">
                                    {categorias.map((c, i) => (
                                        <div key={i} className="bg-blue-500/10 border border-blue-500/20 px-3 py-1.5 rounded-lg flex items-center gap-2">
                                            <span className="text-xs font-bold text-blue-400">{c.nombre}</span>
                                            <span className="text-[10px] text-slate-500">${c.precioBase}</span>
                                        </div>
                                    ))}
                                </div>
                            </div>

                            {/* Seccion Asientos */}
                            <div className="space-y-4 pt-4 border-t border-slate-800/50">
                                <h3 className="text-xs font-black text-slate-500 uppercase tracking-widest flex items-center gap-2">
                                    <div className="w-1.5 h-1.5 bg-emerald-500 rounded-full"></div>
                                    2. Crear Asientos por Filas
                                </h3>
                                <div className="grid grid-cols-3 gap-3">
                                    <div className="space-y-1">
                                        <label className="text-[10px] font-bold text-slate-600 uppercase ml-1">Fila (Nº)</label>
                                        <input
                                            type="number"
                                            className="w-full bg-slate-900 border border-slate-700 rounded-xl px-4 py-2 text-sm text-white"
                                            value={batchSeats.fila}
                                            onChange={e => setBatchSeats({ ...batchSeats, fila: Number(e.target.value) })}
                                        />
                                    </div>
                                    <div className="space-y-1">
                                        <label className="text-[10px] font-bold text-slate-600 uppercase ml-1">Cant. Asientos</label>
                                        <input
                                            type="number"
                                            className="w-full bg-slate-900 border border-slate-700 rounded-xl px-4 py-2 text-sm text-white"
                                            value={batchSeats.cantidad}
                                            onChange={e => setBatchSeats({ ...batchSeats, cantidad: Number(e.target.value) })}
                                        />
                                    </div>
                                    <div className="space-y-1">
                                        <label className="text-[10px] font-bold text-slate-600 uppercase ml-1">Categoría</label>
                                        <select
                                            className="w-full bg-slate-900 border border-slate-700 rounded-xl px-4 py-2 text-sm text-white"
                                            value={batchSeats.categoria}
                                            onChange={e => setBatchSeats({ ...batchSeats, categoria: e.target.value })}
                                        >
                                            <option value="">Seleccionar...</option>
                                            {categorias.map((c, i) => (
                                                <option key={i} value={c.nombre}>{c.nombre}</option>
                                            ))}
                                            {/* Opcion por defecto si no hay categorias */}
                                            <option value="General">General</option>
                                            <option value="VIP">VIP</option>
                                        </select>
                                    </div>
                                </div>
                                <button
                                    onClick={handleBatchCreate}
                                    disabled={saving}
                                    className="w-full bg-emerald-600 hover:bg-emerald-700 text-white py-3 rounded-2xl font-black text-xs uppercase tracking-widest shadow-lg shadow-emerald-500/20 active:scale-95 transition-all flex items-center justify-center gap-2"
                                >
                                    {saving ? <Loader2 className="w-4 h-4 animate-spin" /> : <Save className="w-4 h-4" />}
                                    Generar Lote de Asientos
                                </button>
                            </div>
                        </>
                    )}
                </div>

                <div className="p-6 bg-slate-900/50 border-t border-slate-800 text-center">
                    <p className="text-[10px] text-slate-500 font-medium">Los cambios se guardan directamente en el Microservicio de Gestión de Asientos.</p>
                </div>
            </div>
        </div>
    );
}
