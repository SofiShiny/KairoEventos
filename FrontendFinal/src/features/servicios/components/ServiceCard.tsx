import { useState } from 'react';
import { Plus, Minus, Info, ChevronDown, ChevronUp, CheckCircle, XCircle } from 'lucide-react';
import { ServicioComplementario, Proveedor } from '../services/servicios.service';

interface ServiceCardProps {
    servicio: ServicioComplementario;
    selectedOptions: Record<string, number>; // providerExternalId -> cantidad
    onOptionChange: (provider: Proveedor, newCantidad: number) => void;
}

export default function ServiceCard({ servicio, selectedOptions, onOptionChange }: ServiceCardProps) {
    const [isExpanded, setIsExpanded] = useState(false);

    // Si no tiene proveedores, usamos un "proveedor ficticio" que representa al servicio mismo
    const providers = servicio.proveedores && servicio.proveedores.length > 0
        ? servicio.proveedores
        : [{
            id: servicio.id,
            nombreProveedor: 'Servicio Estándar',
            precio: servicio.precio,
            estaDisponible: true,
            externalId: 'default'
        } as Proveedor];

    const totalSelectedInCategory = providers.reduce((acc, p) => acc + (selectedOptions[p.id] || 0), 0);

    return (
        <div className={`
            relative overflow-hidden rounded-3xl border transition-all duration-500
            ${totalSelectedInCategory > 0
                ? 'border-purple-500 bg-purple-500/5 shadow-[0_0_30px_rgba(168,85,247,0.15)]'
                : 'border-white/10 bg-white/5 hover:border-white/20 hover:bg-white/10'}
        `}>
            {/* Header / Category Info */}
            <div
                className="p-6 cursor-pointer flex items-center justify-between"
                onClick={() => setIsExpanded(!isExpanded)}
            >
                <div className="flex items-center gap-5">
                    <div className={`
                        w-14 h-14 rounded-2xl flex items-center justify-center transition-colors
                        ${totalSelectedInCategory > 0 ? 'bg-purple-600 text-white' : 'bg-white/10 text-white/50'}
                    `}>
                        <Info size={28} />
                    </div>

                    <div>
                        <h3 className="font-black text-white text-xl leading-tight mb-1 uppercase tracking-tight">
                            {servicio.nombre}
                        </h3>
                        <p className="text-neutral-500 text-sm font-bold">
                            {providers.length} Opciones disponibles
                        </p>
                    </div>
                </div>

                <div className="flex items-center gap-4">
                    {totalSelectedInCategory > 0 && (
                        <span className="px-3 py-1 bg-purple-600 text-white text-[10px] font-black rounded-full uppercase tracking-widest">
                            {totalSelectedInCategory} Seleccionados
                        </span>
                    )}
                    {isExpanded ? <ChevronUp className="text-neutral-500" /> : <ChevronDown className="text-neutral-500" />}
                </div>
            </div>

            {/* Providers List (Collapsible) */}
            {isExpanded && (
                <div className="px-6 pb-6 space-y-3 animate-in fade-in slide-in-from-top-2 duration-300">
                    <div className="pt-4 border-t border-white/10 space-y-3">
                        {providers.map((p) => {
                            const qty = selectedOptions[p.id] || 0;
                            return (
                                <div
                                    key={p.id}
                                    className={`
                                        flex items-center justify-between p-4 rounded-2xl border transition-all
                                        ${qty > 0 ? 'bg-white/10 border-purple-500/50' : 'bg-black/20 border-white/5'}
                                        ${!p.estaDisponible ? 'opacity-50 grayscale pointer-events-none' : ''}
                                    `}
                                >
                                    <div className="flex-1">
                                        <div className="flex items-center gap-2">
                                            <span className="font-bold text-white">{p.nombreProveedor}</span>
                                            {p.estaDisponible ? (
                                                <span className="text-[9px] text-emerald-400 font-black uppercase tracking-tighter flex items-center gap-1">
                                                    <CheckCircle size={10} /> Disponible
                                                </span>
                                            ) : (
                                                <span className="text-[9px] text-red-400 font-black uppercase tracking-tighter flex items-center gap-1">
                                                    <XCircle size={10} /> No Disponible
                                                </span>
                                            )}
                                        </div>
                                        <p className="text-neutral-400 text-xs font-bold">${p.precio.toFixed(2)}</p>
                                    </div>

                                    <div className="flex items-center gap-3 bg-black/40 rounded-full p-1 border border-white/10">
                                        <button
                                            onClick={() => onOptionChange(p, Math.max(0, qty - 1))}
                                            disabled={qty === 0}
                                            className={`w-8 h-8 rounded-full flex items-center justify-center transition-colors ${qty === 0 ? 'text-white/20' : 'bg-white/10 text-white hover:bg-white/20'}`}
                                        >
                                            <Minus size={14} />
                                        </button>
                                        <span className="w-6 text-center font-black text-white text-sm">{qty}</span>
                                        <button
                                            onClick={() => onOptionChange(p, qty + 1)}
                                            className="w-8 h-8 rounded-full bg-purple-600 text-white flex items-center justify-center hover:bg-purple-500 transition-colors shadow-lg"
                                        >
                                            <Plus size={14} />
                                        </button>
                                    </div>
                                </div>
                            );
                        })}
                    </div>
                </div>
            )}
        </div>
    );
}
