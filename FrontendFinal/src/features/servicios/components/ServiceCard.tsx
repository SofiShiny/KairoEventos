import { useState } from 'react';
import { Plus, Minus, Info } from 'lucide-react';
import { ServicioComplementario } from '../services/servicios.service';

interface ServiceCardProps {
    servicio: ServicioComplementario;
    cantidad: number;
    onChangeCantidad: (newCantidad: number) => void;
}

export default function ServiceCard({ servicio, cantidad, onChangeCantidad }: ServiceCardProps) {
    const handleIncrement = () => {
        // En un escenario real, validaríamos contra stock si el campo existiera en el DTO
        onChangeCantidad(cantidad + 1);
    };

    const handleDecrement = () => {
        if (cantidad > 0) {
            onChangeCantidad(cantidad - 1);
        }
    };

    return (
        <div className={`
            relative overflow-hidden rounded-2xl border transition-all duration-300
            ${cantidad > 0
                ? 'border-blue-500 bg-blue-500/10 shadow-[0_0_20px_rgba(59,130,246,0.3)]'
                : 'border-white/10 bg-white/5 hover:border-white/20 hover:bg-white/10'}
        `}>
            <div className="p-5 flex items-center justify-between gap-4">
                <div className="flex items-center gap-4">
                    {/* Placeholder de imagen o icono */}
                    <div className={`
                        w-12 h-12 rounded-xl flex items-center justify-center
                        ${cantidad > 0 ? 'bg-blue-500 text-white' : 'bg-white/10 text-white/50'}
                    `}>
                        <Info size={24} />
                    </div>

                    <div>
                        <h3 className="font-bold text-white text-lg leading-tight mb-1">
                            {servicio.nombre}
                        </h3>
                        <p className="text-white/60 text-sm font-medium">
                            ${servicio.precio.toFixed(2)} <span className="text-xs text-white/40">/ unidad</span>
                        </p>
                    </div>
                </div>

                <div className="flex items-center gap-3 bg-black/40 rounded-full p-1 border border-white/10">
                    <button
                        onClick={handleDecrement}
                        disabled={cantidad === 0}
                        className={`
                            w-8 h-8 rounded-full flex items-center justify-center transition-colors
                            ${cantidad === 0
                                ? 'text-white/20 cursor-not-allowed'
                                : 'bg-white/10 text-white hover:bg-white/20'}
                        `}
                    >
                        <Minus size={14} />
                    </button>

                    <span className="w-6 text-center font-bold text-white">
                        {cantidad}
                    </span>

                    <button
                        onClick={handleIncrement}
                        className="w-8 h-8 rounded-full bg-blue-600 text-white flex items-center justify-center hover:bg-blue-500 transition-colors shadow-lg"
                    >
                        <Plus size={14} />
                    </button>
                </div>
            </div>

            {/* Indicador de seleccionado */}
            {cantidad > 0 && (
                <div className="absolute top-0 right-0 px-3 py-1 bg-blue-500 text-[10px] font-black uppercase tracking-wider text-white rounded-bl-xl">
                    Agregado
                </div>
            )}
        </div>
    );
}
