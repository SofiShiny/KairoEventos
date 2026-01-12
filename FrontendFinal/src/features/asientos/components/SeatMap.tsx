import { useState, useEffect } from 'react';
import { Asiento } from '../services/asientos.service';

interface SeatMapProps {
    asientos: Asiento[];
    onSelectionChange: (selectedAsientos: Asiento[]) => void;
}

export default function SeatMap({ asientos, onSelectionChange }: SeatMapProps) {
    const [selectedSeats, setSelectedSeats] = useState<Set<string>>(new Set());

    // Agrupar asientos por fila
    const asientosPorFila = asientos.reduce((acc, asiento) => {
        if (!acc[asiento.fila]) {
            acc[asiento.fila] = [];
        }
        acc[asiento.fila].push(asiento);
        return acc;
    }, {} as Record<number, Asiento[]>);

    // Ordenar filas y asientos
    const filasOrdenadas = Object.keys(asientosPorFila)
        .map(Number)
        .sort((a, b) => a - b);

    Object.values(asientosPorFila).forEach(fila => {
        fila.sort((a, b) => a.numero - b.numero);
    });

    const handleSeatClick = (asiento: Asiento) => {
        if (asiento.estado !== 'Disponible' && !selectedSeats.has(asiento.id)) {
            return; // No se puede seleccionar si está ocupado o reservado
        }

        const newSelected = new Set(selectedSeats);

        if (newSelected.has(asiento.id)) {
            newSelected.delete(asiento.id);
        } else {
            newSelected.add(asiento.id);
        }

        setSelectedSeats(newSelected);

        // Notificar al padre
        const selectedAsientos = asientos.filter(a => newSelected.has(a.id));
        onSelectionChange(selectedAsientos);
    };

    const getSeatClassName = (asiento: Asiento): string => {
        const isSelected = selectedSeats.has(asiento.id);

        const baseClasses = 'w-10 h-10 rounded-lg font-semibold text-xs flex items-center justify-center transition-all duration-200 relative';

        if (isSelected) {
            return `${baseClasses} bg-gradient-to-br from-blue-500 to-blue-600 text-white ring-2 ring-white scale-110 shadow-lg shadow-blue-500/50 cursor-pointer hover:scale-115`;
        }

        switch (asiento.estado) {
            case 'Disponible':
                return `${baseClasses} bg-gradient-to-br from-green-500 to-green-600 text-white hover:from-green-400 hover:to-green-500 hover:scale-105 cursor-pointer shadow-md hover:shadow-green-500/50`;
            case 'Reservado':
                return `${baseClasses} bg-gradient-to-br from-yellow-500 to-yellow-600 text-white cursor-not-allowed opacity-60`;
            case 'Ocupado':
                return `${baseClasses} bg-gradient-to-br from-red-500 to-red-600 text-white cursor-not-allowed opacity-60`;
            default:
                return `${baseClasses} bg-gray-700 text-gray-400 cursor-not-allowed`;
        }
    };

    const getSeatIcon = (asiento: Asiento): string => {
        const isSelected = selectedSeats.has(asiento.id);
        if (isSelected) return '✓';
        if (asiento.estado === 'Ocupado') return '✕';
        if (asiento.estado === 'Reservado') return '⏱';
        return asiento.numero.toString();
    };

    return (
        <div className="w-full">
            {/* Pantalla/Escenario */}
            <div className="mb-8">
                <div className="w-full h-2 bg-gradient-to-r from-transparent via-purple-500 to-transparent rounded-full mb-2"></div>
                <p className="text-center text-gray-400 text-sm font-semibold">ESCENARIO</p>
            </div>

            {/* Mapa de Asientos */}
            <div className="space-y-3 max-h-[500px] overflow-y-auto pr-2 custom-scrollbar">
                {filasOrdenadas.map(fila => (
                    <div key={fila} className="flex items-center gap-2">
                        {/* Indicador de Fila */}
                        <div className="w-8 h-10 flex items-center justify-center text-purple-400 font-bold text-sm">
                            {String.fromCharCode(64 + fila)}
                        </div>

                        {/* Asientos de la Fila */}
                        <div className="flex gap-2 flex-wrap">
                            {asientosPorFila[fila].map(asiento => (
                                <button
                                    key={asiento.id}
                                    onClick={() => handleSeatClick(asiento)}
                                    disabled={asiento.estado !== 'Disponible' && !selectedSeats.has(asiento.id)}
                                    className={getSeatClassName(asiento)}
                                    title={`Fila ${String.fromCharCode(64 + asiento.fila)}-${asiento.numero} | ${asiento.categoria} | $${asiento.precio} | ${asiento.estado}`}
                                >
                                    {getSeatIcon(asiento)}

                                    {/* Badge de categoría */}
                                    {selectedSeats.has(asiento.id) && (
                                        <span className="absolute -top-1 -right-1 w-3 h-3 bg-white rounded-full animate-pulse"></span>
                                    )}
                                </button>
                            ))}
                        </div>
                    </div>
                ))}
            </div>

            {/* Leyenda */}
            <div className="mt-8 pt-6 border-t border-gray-800">
                <h3 className="text-sm font-semibold text-gray-400 mb-3">LEYENDA</h3>
                <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
                    <div className="flex items-center gap-2">
                        <div className="w-6 h-6 rounded bg-gradient-to-br from-green-500 to-green-600"></div>
                        <span className="text-xs text-gray-300">Disponible</span>
                    </div>
                    <div className="flex items-center gap-2">
                        <div className="w-6 h-6 rounded bg-gradient-to-br from-blue-500 to-blue-600 ring-2 ring-white"></div>
                        <span className="text-xs text-gray-300">Seleccionado</span>
                    </div>
                    <div className="flex items-center gap-2">
                        <div className="w-6 h-6 rounded bg-gradient-to-br from-yellow-500 to-yellow-600 opacity-60"></div>
                        <span className="text-xs text-gray-300">Reservado</span>
                    </div>
                    <div className="flex items-center gap-2">
                        <div className="w-6 h-6 rounded bg-gradient-to-br from-red-500 to-red-600 opacity-60"></div>
                        <span className="text-xs text-gray-300">Ocupado</span>
                    </div>
                </div>
            </div>

            <style>{`
        .custom-scrollbar::-webkit-scrollbar {
          width: 6px;
        }
        .custom-scrollbar::-webkit-scrollbar-track {
          background: rgba(31, 41, 55, 0.5);
          border-radius: 10px;
        }
        .custom-scrollbar::-webkit-scrollbar-thumb {
          background: linear-gradient(to bottom, #8b5cf6, #ec4899);
          border-radius: 10px;
        }
        .custom-scrollbar::-webkit-scrollbar-thumb:hover {
          background: linear-gradient(to bottom, #7c3aed, #db2777);
        }
      `}</style>
        </div>
    );
}
