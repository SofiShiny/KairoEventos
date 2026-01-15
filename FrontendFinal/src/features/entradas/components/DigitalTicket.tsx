import { Calendar, MapPin, Ticket, User, QrCode, Video, ExternalLink } from 'lucide-react';
import { QRCodeSVG } from 'qrcode.react';
import { clsx, type ClassValue } from 'clsx';
import { twMerge } from 'tailwind-merge';

function cn(...inputs: ClassValue[]) {
    return twMerge(clsx(inputs));
}

interface DigitalTicketProps {
    titulo: string;
    fecha: string;
    asiento: string;
    estado: string;
    imagenUrl?: string;
    monto: number;
    codigo: string;
    nombreUsuario: string;
    esVirtual?: boolean;
    eventoId: string;
    eventoEstado?: string;
}

import { MessageSquare, ClipboardCheck } from 'lucide-react';

export const DigitalTicket = ({
    titulo,
    fecha,
    asiento,
    estado,
    imagenUrl,
    monto,
    codigo,
    nombreUsuario,
    esVirtual,
    eventoId,
    eventoEstado
}: DigitalTicketProps) => {
    // Asegurarse de que estado sea string antes de llamar a toLowerCase
    const estadoStr = String(estado || 'Pendiente');
    const isPagado = estadoStr.toLowerCase().includes('pagada') || estadoStr.toLowerCase().includes('pagado');

    const isCompletado = eventoEstado === 'Completado' || new Date(fecha) < new Date();

    return (
        <div className="relative flex flex-col md:flex-row w-full max-w-2xl bg-neutral-900 border border-neutral-800 rounded-3xl overflow-hidden shadow-2xl transition-transform hover:scale-[1.01]">
            {/* Decoración lateral (Círculos de ticket) */}
            <div className="absolute left-[-10px] top-1/2 -translate-y-1/2 w-5 h-5 bg-black rounded-full z-10 hidden md:block" />
            <div className="absolute right-[-10px] top-1/2 -translate-y-1/2 w-5 h-5 bg-black rounded-full z-10 hidden md:block" />

            {/* Izquierda: Imagen */}
            <div className="relative w-full md:w-48 h-48 md:h-auto overflow-hidden">
                {imagenUrl ? (
                    <img
                        src={imagenUrl}
                        alt={titulo}
                        className="w-full h-full object-cover"
                    />
                ) : (
                    <div className="w-full h-full bg-gradient-to-br from-purple-600 to-blue-700 flex items-center justify-center">
                        <Ticket className="w-12 h-12 text-white/50" />
                    </div>
                )}
                <div className="absolute top-4 left-4">
                    <span className={cn(
                        "px-3 py-1 rounded-full text-[10px] font-bold uppercase tracking-wider",
                        isPagado ? "bg-emerald-500 text-white" : "bg-amber-500 text-black"
                    )}>
                        {estadoStr}
                    </span>
                </div>
            </div>

            {/* Centro: Detalles */}
            <div className="flex-1 p-6 flex flex-col justify-between border-b md:border-b-0 md:border-r border-dashed border-neutral-700">
                <div>
                    <h3 className="text-xl font-bold text-white mb-2 line-clamp-2">{titulo}</h3>
                    <div className="space-y-2">
                        <div className="flex items-center gap-2 text-neutral-400 text-sm">
                            <Calendar className="w-4 h-4 text-purple-400" />
                            <span>{new Date(fecha).toLocaleDateString('es-ES', { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' })}</span>
                        </div>
                        <div className="flex items-center gap-2 text-neutral-400 text-sm">
                            <MapPin className="w-4 h-4 text-purple-400" />
                            <span>Kairo Arena - {asiento}</span>
                        </div>
                    </div>
                </div>

                <div className="mt-6 flex items-end justify-between">
                    <div>
                        <p className="text-[10px] uppercase text-neutral-500 font-bold tracking-widest">Inversión</p>
                        <p className="text-2xl font-black text-white">${(monto || 0).toFixed(2)}</p>
                    </div>
                    <div className="text-right">
                        <p className="text-[10px] uppercase text-neutral-500 font-bold tracking-widest">Titular</p>
                        <div className="flex items-center gap-1 text-neutral-300">
                            <User className="w-3 h-3" />
                            <span className="text-xs font-semibold">{nombreUsuario}</span>
                        </div>
                    </div>
                </div>
            </div>

            {/* Derecha: QR */}
            <div className="w-full md:w-48 bg-neutral-800/50 p-6 flex flex-col items-center justify-center gap-4">
                <div className="bg-white p-3 rounded-xl shadow-lg">
                    {/* Generación de QR real usando qrcode.react */}
                    <div className="w-40 h-40 bg-white flex items-center justify-center">
                        {codigo ? (
                            <QRCodeSVG
                                value={codigo}
                                size={160}
                                level="H"
                                includeMargin={true}
                                bgColor="#ffffff"
                                fgColor="#000000"
                            />
                        ) : (
                            <QrCode className="w-32 h-32 text-neutral-300" />
                        )}
                    </div>
                </div>
                <p className="text-[10px] font-mono text-neutral-500 break-all text-center">
                    {codigo ? codigo.toUpperCase() : 'SIN CÓDIGO'}
                </p>
            </div>

            {/* Acceso Streaming para Eventos Virtuales */}
            {isPagado && esVirtual && !isCompletado && (
                <div className="absolute top-4 right-4 md:relative md:top-0 md:right-0 md:w-full md:bg-blue-600/10 md:p-4 md:border-t md:border-blue-500/20 flex flex-col items-center justify-center">
                    <a
                        href={`/streaming/${eventoId}`}
                        className="flex items-center gap-2 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-xl text-xs font-black transition-all shadow-lg shadow-blue-600/20 active:scale-95 group"
                    >
                        <Video className="w-4 h-4" />
                        ACCESO STREAMING
                        <ExternalLink className="w-3 h-3 opacity-0 group-hover:opacity-100 transition-opacity" />
                    </a>
                </div>
            )}

            {/* Acceso Post-Evento (Foros y Encuestas) */}
            {isPagado && isCompletado && (
                <div className="absolute bottom-0 left-0 w-full bg-neutral-800/80 backdrop-blur-md p-4 border-t border-neutral-700 flex flex-wrap items-center justify-center gap-4 z-20">
                    <p className="w-full text-center text-[10px] uppercase font-black tracking-[0.2em] text-blue-400 mb-2">Evento Finalizado - Experiencia Kairo</p>
                    <a
                        href={`/foros/${eventoId}`}
                        className="flex-1 min-w-[140px] flex items-center justify-center gap-2 px-4 py-3 bg-neutral-900 border border-neutral-700 hover:border-blue-500 text-white rounded-2xl text-xs font-black transition-all group"
                    >
                        <MessageSquare className="w-4 h-4 text-blue-500 group-hover:scale-110 transition-transform" />
                        IR AL FORO
                    </a>
                    <a
                        href={`/encuestas/${eventoId}`}
                        className="flex-1 min-w-[140px] flex items-center justify-center gap-2 px-4 py-3 bg-neutral-900 border border-neutral-700 hover:border-purple-500 text-white rounded-2xl text-xs font-black transition-all group"
                    >
                        <ClipboardCheck className="w-4 h-4 text-purple-500 group-hover:scale-110 transition-transform" />
                        VALORAR EVENTO
                    </a>
                </div>
            )}
        </div>
    );
};
