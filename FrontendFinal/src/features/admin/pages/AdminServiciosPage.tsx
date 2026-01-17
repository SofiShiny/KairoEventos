import React, { useEffect, useState } from 'react';
import { serviciosService, Proveedor } from '../../servicios/services/servicios.service';
import { Save, AlertCircle, CheckCircle2, RefreshCcw, Package, DollarSign, Globe, Layers } from 'lucide-react';

interface ServicioGlobal {
    id: string;
    nombre: string;
    precio: number;
    activo: boolean;
}

const AdminServiciosPage: React.FC = () => {
    const [serviciosExternos, setServiciosExternos] = useState<Proveedor[]>([]);
    const [serviciosGlobales, setServiciosGlobales] = useState<ServicioGlobal[]>([]);
    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState<string | null>(null);
    const [message, setMessage] = useState<{ type: 'success' | 'error', text: string } | null>(null);

    useEffect(() => {
        loadAll();
    }, []);

    const loadAll = async () => {
        try {
            setLoading(true);
            const [externos, globales] = await Promise.all([
                serviciosService.getServiciosExternos(),
                serviciosService.getServiciosGlobales()
            ]);
            setServiciosExternos(externos);
            setServiciosGlobales(globales);
        } catch (error) {
            setMessage({ type: 'error', text: 'Error al sincronizar catálogos' });
        } finally {
            setLoading(false);
        }
    };

    const handleUpdateExterno = async (s: Proveedor) => {
        setSaving(s.externalId);
        try {
            await serviciosService.updateServicioExterno(s.externalId, s.precio, s.estaDisponible);
            setMessage({ type: 'success', text: `"${s.nombreProveedor}" actualizado` });
            setTimeout(() => setMessage(null), 3000);
        } catch (error) {
            setMessage({ type: 'error', text: 'Error al actualizar externo' });
        } finally {
            setSaving(null);
        }
    };

    const handleUpdateGlobal = async (s: ServicioGlobal) => {
        setSaving(s.id);
        try {
            await serviciosService.updateServicioGlobal(s.id, s.nombre, s.precio);
            setMessage({ type: 'success', text: `"${s.nombre}" actualizado` });
            setTimeout(() => setMessage(null), 3000);
        } catch (error) {
            setMessage({ type: 'error', text: 'Error al actualizar global' });
        } finally {
            setSaving(null);
        }
    };

    const updateExternoState = (id: string, field: keyof Proveedor, value: any) => {
        setServiciosExternos(prev => prev.map(s => s.externalId === id ? { ...s, [field]: value } : s));
    };

    const updateGlobalState = (id: string, field: keyof ServicioGlobal, value: any) => {
        setServiciosGlobales(prev => prev.map(s => s.id === id ? { ...s, [field]: value } : s));
    };

    if (loading) {
        return (
            <div className="flex flex-col items-center justify-center min-h-[400px] space-y-4">
                <div className="w-12 h-12 border-4 border-blue-500/20 border-t-blue-500 rounded-full animate-spin"></div>
                <p className="text-slate-500 font-black text-xs uppercase tracking-[0.3em] animate-pulse">Sincronizando Plataformas...</p>
            </div>
        );
    }

    const groupedExternos = serviciosExternos.reduce((acc, curr: any) => {
        const tipo = curr.tipo || 'otros';
        if (!acc[tipo]) acc[tipo] = [];
        acc[tipo].push(curr);
        return acc;
    }, {} as Record<string, Proveedor[]>);

    return (
        <div className="max-w-7xl mx-auto space-y-12 animate-in fade-in duration-500 pb-20">
            {/* Header Section */}
            <div className="flex flex-col md:flex-row md:items-center justify-between gap-6">
                <div className="relative">
                    <div className="absolute -top-10 -left-10 w-32 h-32 bg-blue-500/10 blur-3xl rounded-full"></div>
                    <div className="flex items-center gap-3 mb-2">
                        <Globe className="text-blue-500 w-5 h-5" />
                        <span className="text-blue-500 font-black text-xs uppercase tracking-[0.3em]">Centro de Control de Servicios</span>
                    </div>
                    <h1 className="text-4xl font-black text-white tracking-tight uppercase italic">
                        Gestión de <span className="text-blue-500">Catálogos</span>
                    </h1>
                    <p className="text-slate-500 mt-2 font-medium">Control unificado de servicios internos y proveedores de terceros.</p>
                </div>

                <div className="flex items-center gap-4">
                    {message && (
                        <div className={`flex items-center gap-2 px-4 py-2 rounded-xl border text-sm font-bold ${message.type === 'success'
                                ? 'bg-emerald-500/10 border-emerald-500/20 text-emerald-400'
                                : 'bg-rose-500/10 border-rose-500/20 text-rose-400'
                            } animate-in slide-in-from-right-4`}>
                            {message.type === 'success' ? <CheckCircle2 size={16} /> : <AlertCircle size={16} />}
                            {message.text}
                        </div>
                    )}
                    <button
                        onClick={loadAll}
                        className="p-3 bg-slate-900 border border-slate-800 rounded-xl hover:bg-slate-800 transition-colors text-slate-400 group"
                    >
                        <RefreshCcw size={20} className="group-hover:rotate-180 transition-transform duration-500" />
                    </button>
                </div>
            </div>

            {/* SECCIÓN 1: SERVICIOS GLOBALES (INTERNOS) */}
            <section className="space-y-6">
                <div className="flex items-center gap-4">
                    <div className="flex items-center gap-3 text-white">
                        <Layers className="w-6 h-6 text-indigo-500" />
                        <h2 className="text-xl font-black uppercase tracking-widest italic">Servicios del <span className="text-indigo-500">Sistema</span></h2>
                    </div>
                    <div className="flex-1 h-[1px] bg-slate-800"></div>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                    {serviciosGlobales.map(s => (
                        <div key={s.id} className="bg-[#16191f] border border-slate-800 p-6 rounded-3xl hover:border-indigo-500/50 transition-all group">
                            <div className="flex justify-between items-start mb-6">
                                <div className="p-3 bg-indigo-500/10 rounded-2xl">
                                    <Package className="w-6 h-6 text-indigo-500" />
                                </div>
                                <div className={`text-[10px] font-black uppercase tracking-widest px-2 py-1 rounded-lg ${s.activo ? 'bg-emerald-500/10 text-emerald-500' : 'bg-slate-800 text-slate-500'}`}>
                                    {s.activo ? 'Vigente' : 'Inactivo'}
                                </div>
                            </div>

                            <input
                                type="text"
                                value={s.nombre}
                                onChange={(e) => updateGlobalState(s.id, 'nombre', e.target.value)}
                                className="w-full bg-transparent border-none text-white font-black text-lg mb-4 outline-none focus:text-indigo-400 transition-colors uppercase italic"
                            />

                            <div className="flex items-center gap-4 mb-6">
                                <div className="flex-1 relative">
                                    <DollarSign className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-600 w-4 h-4" />
                                    <input
                                        type="number"
                                        value={s.precio}
                                        onChange={(e) => updateGlobalState(s.id, 'precio', parseFloat(e.target.value))}
                                        className="w-full bg-slate-900 border border-slate-800 rounded-xl pl-9 pr-4 py-3 text-white font-bold outline-none focus:border-indigo-500 transition-all"
                                        step="0.01"
                                    />
                                </div>
                                <button
                                    onClick={() => handleUpdateGlobal(s)}
                                    disabled={saving === s.id}
                                    className="p-3 bg-indigo-600 hover:bg-indigo-500 text-white rounded-xl transition-all disabled:opacity-50"
                                >
                                    {saving === s.id ? <RefreshCcw size={18} className="animate-spin" /> : <Save size={18} />}
                                </button>
                            </div>

                            <div className="text-[9px] font-mono text-slate-700 truncate">ID: {s.id}</div>
                        </div>
                    ))}
                </div>
            </section>

            {/* SECCIÓN 2: PROVEEDORES EXTERNOS */}
            <section className="space-y-6">
                <div className="flex items-center gap-4">
                    <div className="flex items-center gap-3 text-white">
                        <Globe className="w-6 h-6 text-blue-500" />
                        <h2 className="text-xl font-black uppercase tracking-widest italic">Conexiones <span className="text-blue-500">Externas</span></h2>
                    </div>
                    <div className="flex-1 h-[1px] bg-slate-800"></div>
                </div>

                {serviciosExternos.length === 0 ? (
                    <div className="bg-slate-900/50 border border-slate-800 border-dashed rounded-3xl p-16 text-center">
                        <Globe className="w-12 h-12 text-slate-800 mx-auto mb-4" />
                        <h3 className="text-lg font-bold text-slate-500 mb-2">No hay proveedores externos en línea</h3>
                        <p className="text-slate-600 text-sm">Las APIs de terceros parece que no están respondiendo en este momento.</p>
                    </div>
                ) : (
                    <div className="grid gap-8">
                        {Object.entries(groupedExternos).map(([tipo, items]) => (
                            <div key={tipo} className="bg-[#16191f] border border-slate-800 rounded-3xl overflow-hidden shadow-2xl">
                                <div className="bg-slate-900/50 px-8 py-4 border-b border-slate-800 flex items-center justify-between">
                                    <span className="text-[10px] font-black uppercase tracking-[0.3em] text-blue-500 italic">{tipo}</span>
                                    <span className="text-[10px] text-slate-600 font-bold">{items.length} proveedores</span>
                                </div>
                                <div className="overflow-x-auto">
                                    <table className="w-full">
                                        <tbody className="divide-y divide-slate-800/30">
                                            {items.map((s) => (
                                                <tr key={s.externalId} className="group hover:bg-blue-500/[0.02] transition-colors">
                                                    <td className="px-8 py-6">
                                                        <div className="flex items-center gap-4">
                                                            <div className="w-10 h-10 bg-slate-800 rounded-xl flex items-center justify-center text-slate-500 font-black group-hover:text-blue-500 transition-colors uppercase">
                                                                {s.nombreProveedor.charAt(0)}
                                                            </div>
                                                            <div>
                                                                <div className="font-bold text-white uppercase italic tracking-tight">{s.nombreProveedor}</div>
                                                                <div className="text-[10px] font-mono text-slate-600 uppercase tracking-widest">EXT-ID: {s.externalId}</div>
                                                            </div>
                                                        </div>
                                                    </td>
                                                    <td className="px-8 py-6">
                                                        <div className="relative w-40">
                                                            <DollarSign className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-600 w-4 h-4" />
                                                            <input
                                                                type="number"
                                                                value={s.precio}
                                                                onChange={(e) => updateExternoState(s.externalId, 'precio', parseFloat(e.target.value))}
                                                                className="w-full bg-slate-900 border border-slate-700 rounded-xl pl-9 pr-4 py-2.5 text-white font-bold focus:border-blue-500 outline-none transition-all"
                                                                step="0.01"
                                                            />
                                                        </div>
                                                    </td>
                                                    <td className="px-8 py-6">
                                                        <div className="flex flex-col items-center">
                                                            <button
                                                                onClick={() => updateExternoState(s.externalId, 'estaDisponible', !s.estaDisponible)}
                                                                className={`relative inline-flex h-6 w-11 items-center rounded-full transition-all ${s.estaDisponible ? 'bg-blue-600' : 'bg-slate-700'
                                                                    }`}
                                                            >
                                                                <span className={`inline-block h-4 w-4 transform rounded-full bg-white transition-transform ${s.estaDisponible ? 'translate-x-6' : 'translate-x-1'
                                                                    }`} />
                                                            </button>
                                                        </div>
                                                    </td>
                                                    <td className="px-8 py-6 text-right">
                                                        <button
                                                            onClick={() => handleUpdateExterno(s)}
                                                            disabled={saving === s.externalId}
                                                            className="inline-flex items-center gap-2 px-6 py-2.5 bg-slate-800 hover:bg-slate-700 text-white text-[10px] font-black uppercase tracking-widest rounded-xl transition-all border border-slate-700"
                                                        >
                                                            {saving === s.externalId ? <RefreshCcw size={14} className="animate-spin" /> : <Save size={14} />}
                                                            {saving === s.externalId ? '' : 'Guardar'}
                                                        </button>
                                                    </td>
                                                </tr>
                                            ))}
                                        </tbody>
                                    </table>
                                </div>
                            </div>
                        ))}
                    </div>
                )}
            </section>
        </div>
    );
};

export default AdminServiciosPage;
