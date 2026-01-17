import React, { useEffect, useState } from 'react';
import { serviciosService, Proveedor } from '../../servicios/services/servicios.service';
import { Settings, Save, AlertCircle, CheckCircle2, X } from 'lucide-react';

const AdminServiciosPage: React.FC = () => {
    const [servicios, setServicios] = useState<Proveedor[]>([]);
    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState<string | null>(null);
    const [message, setMessage] = useState<{ type: 'success' | 'error', text: string } | null>(null);

    useEffect(() => {
        loadServicios();
    }, []);

    const loadServicios = async () => {
        try {
            const data = await serviciosService.getServiciosExternos();
            setServicios(data);
        } catch (error) {
            setMessage({ type: 'error', text: 'Error al cargar servicios externos' });
        } finally {
            setLoading(false);
        }
    };

    const handleUpdate = async (s: Proveedor) => {
        setSaving(s.externalId);
        try {
            await serviciosService.updateServicioExterno(s.externalId, s.precio, s.estaDisponible);
            setMessage({ type: 'success', text: `"${s.nombreProveedor}" actualizado correctamente` });
            setTimeout(() => setMessage(null), 3000);
        } catch (error) {
            setMessage({ type: 'error', text: 'Error al actualizar el servicio' });
        } finally {
            setSaving(null);
        }
    };

    const updateLocalState = (id: string, field: keyof Proveedor, value: any) => {
        setServicios(prev => prev.map(s => s.externalId === id ? { ...s, [field]: value } : s));
    };

    if (loading) {
        return (
            <div className="flex items-center justify-center min-h-[400px]">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600"></div>
            </div>
        );
    }

    const groupedServices = servicios.reduce((acc, curr: any) => {
        const tipo = curr.tipo || 'otros';
        if (!acc[tipo]) acc[tipo] = [];
        acc[tipo].push(curr);
        return acc;
    }, {} as Record<string, Proveedor[]>);

    return (
        <div className="p-6 max-w-6xl mx-auto">
            <div className="flex items-center justify-between mb-8">
                <div>
                    <h1 className="text-3xl font-bold text-gray-900 flex items-center gap-3">
                        <Settings className="text-indigo-600" size={32} />
                        Control de Proveedores Externos
                    </h1>
                    <p className="text-gray-500 mt-2">Gestiona la disponibilidad y precios de los servicios de terceros en tiempo real</p>
                </div>

                {message && (
                    <div className={`flex items-center gap-2 px-4 py-2 rounded-lg border ${message.type === 'success' ? 'bg-green-50 border-green-200 text-green-700' : 'bg-red-50 border-red-200 text-red-700'
                        } animate-in fade-in slide-in-from-top-4 duration-300`}>
                        {message.type === 'success' ? <CheckCircle2 size={18} /> : <AlertCircle size={18} />}
                        {message.text}
                    </div>
                )}
            </div>

            <div className="grid gap-8">
                {Object.entries(groupedServices).map(([tipo, items]) => (
                    <section key={tipo} className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
                        <div className="bg-gray-50 px-6 py-4 border-b border-gray-100">
                            <h2 className="text-lg font-semibold text-gray-800 capitalize">{tipo}</h2>
                        </div>

                        <div className="overflow-x-auto">
                            <table className="w-full text-left">
                                <thead>
                                    <tr className="text-sm text-gray-500 border-b border-gray-100">
                                        <th className="px-6 py-4 font-medium">Servicio</th>
                                        <th className="px-6 py-4 font-medium">Precio (USD)</th>
                                        <th className="px-6 py-4 font-medium">Estado</th>
                                        <th className="px-6 py-4 font-medium text-right">Acciones</th>
                                    </tr>
                                </thead>
                                <tbody className="divide-y divide-gray-50">
                                    {items.map((s) => (
                                        <tr key={s.externalId} className="hover:bg-gray-50/50 transition-colors">
                                            <td className="px-6 py-4">
                                                <div className="font-medium text-gray-900">{s.nombreProveedor}</div>
                                                <div className="text-xs text-gray-400 font-mono">ID: {s.externalId}</div>
                                            </td>
                                            <td className="px-6 py-4">
                                                <div className="relative w-32">
                                                    <span className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400">$</span>
                                                    <input
                                                        type="number"
                                                        value={s.precio}
                                                        onChange={(e) => updateLocalState(s.externalId, 'precio', parseFloat(e.target.value))}
                                                        className="w-full pl-7 pr-3 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 transition-all outline-none"
                                                        step="0.01"
                                                    />
                                                </div>
                                            </td>
                                            <td className="px-6 py-4">
                                                <button
                                                    onClick={() => updateLocalState(s.externalId, 'estaDisponible', !s.estaDisponible)}
                                                    className={`relative inline-flex h-6 w-11 items-center rounded-full transition-colors focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 ${s.estaDisponible ? 'bg-indigo-600' : 'bg-gray-200'
                                                        }`}
                                                >
                                                    <span
                                                        className={`inline-block h-4 w-4 transform rounded-full bg-white transition-transform ${s.estaDisponible ? 'translate-x-6' : 'translate-x-1'
                                                            }`}
                                                    />
                                                </button>
                                                <span className={`ml-3 text-sm ${s.estaDisponible ? 'text-indigo-600 font-medium' : 'text-gray-400'}`}>
                                                    {s.estaDisponible ? 'Disponible' : 'Agotado'}
                                                </span>
                                            </td>
                                            <td className="px-6 py-4 text-right">
                                                <button
                                                    onClick={() => handleUpdate(s)}
                                                    disabled={saving === s.externalId}
                                                    className="inline-flex items-center gap-2 px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 disabled:opacity-50 transition-colors shadow-sm shadow-indigo-200"
                                                >
                                                    {saving === s.externalId ? (
                                                        <div className="h-4 w-4 animate-spin rounded-full border-2 border-white/20 border-b-white"></div>
                                                    ) : (
                                                        <Save size={18} />
                                                    )}
                                                    {saving === s.externalId ? 'Guardando...' : 'Guardar'}
                                                </button>
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                    </section>
                ))}
            </div>
        </div>
    );
};

export default AdminServiciosPage;
