import { Calendar, MapPin, Ticket, User, QrCode, Video, ExternalLink, XCircle } from 'lucide-react';
import { QRCodeSVG } from 'qrcode.react';
import { useState } from 'react';
import { toast } from 'react-hot-toast';
import { entradasService } from '../services/entradas.service';
import { clsx, type ClassValue } from 'clsx';
import { twMerge } from 'tailwind-merge';
import { Link } from 'react-router-dom';
import { MessageSquare, ClipboardCheck, Star } from 'lucide-react';

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
    id: string;
    usuarioId: string;
    onCancel?: (id: string) => void;
    serviciosExtras?: { nombre: string; estado: string; precio: number }[];
}

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
    eventoEstado,
    id,
    usuarioId,
    onCancel,
    serviciosExtras = []
}: DigitalTicketProps) => {
    const [isCancelling, setIsCancelling] = useState(false);
    // Asegurarse de que estado sea string antes de llamar a toLowerCase
    const estadoStr = String(estado || 'Pendiente');
    const isPagado = estadoStr.toLowerCase().includes('pagada') || estadoStr.toLowerCase().includes('pagado');

    const eventDate = new Date(fecha);
    const today = new Date();
    // No considerar completado si es el mismo día, a menos que el estado lo diga explícitamente
    const isCompletado = eventoEstado === 'Completado' || eventoEstado === 'Finalizado' ||
        (new Date(eventDate.getFullYear(), eventDate.getMonth(), eventDate.getDate() + 1) < today);

    const totalExtras = serviciosExtras.reduce((acc, current) => acc + (Number(current.precio) || 0), 0);
    const inversionTotal = (Number(monto) || 0) + totalExtras;

    return (
        <div className="relative flex flex-col md:flex-row w-full max-w-4xl bg-neutral-900 border border-neutral-800 rounded-3xl overflow-hidden shadow-2xl transition-transform hover:scale-[1.01]">
            {/* Decoración lateral (Círculos de ticket) */}
            <div className="absolute left-[-10px] top-1/2 -translate-y-1/2 w-5 h-5 bg-black rounded-full z-10 hidden md:block" />
            <div className="absolute right-[-10px] top-1/2 -translate-y-1/2 w-5 h-5 bg-black rounded-full z-10 hidden md:block" />

            {/* Izquierda: Imagen */}
            <div className="relative w-full md:w-40 h-40 md:h-auto overflow-hidden flex-shrink-0">
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
                            <span>{esVirtual ? 'Plataforma Online' : (asiento ? `Kairo Arena - ${asiento}` : 'Kairo Arena')}</span>
                        </div>

                        {/* Servicios Extras */}
                        {serviciosExtras.length > 0 && (
                            <div className="mt-4 pt-3 border-t border-dashed border-neutral-700/50">
                                <p className="text-[10px] uppercase text-purple-400 font-bold tracking-widest mb-2 flex items-center gap-1">
                                    <Star className="w-3 h-3" /> Incluye
                                </p>
                                <div className="flex flex-wrap gap-2">
                                    {serviciosExtras.map((serv, idx) => (
                                        <div
                                            key={idx}
                                            className={cn(
                                                "px-2 py-1 rounded-md border text-[10px] font-bold uppercase flex items-center gap-1.5",
                                                serv.estado === 'Confirmado'
                                                    ? "bg-neutral-800 border-neutral-700 text-white"
                                                    : "bg-amber-900/20 border-amber-900/50 text-amber-500"
                                            )}
                                        >
                                            {serv.nombre}
                                            {serv.estado !== 'Confirmado' && (
                                                <span className="w-1.5 h-1.5 rounded-full bg-amber-500" title="Pendiente" />
                                            )}
                                        </div>
                                    ))}
                                </div>
                            </div>
                        )}
                    </div>
                </div>

                {/* Footer Inversión y Cancelar (Dentro del centro para mantener layout) */}
                <div>
                    <div className="mt-6 pt-4 border-t border-neutral-800/50 flex items-center justify-between gap-4">
                        <div className="flex-shrink-0">
                            <p className="text-[9px] uppercase text-neutral-500 font-bold tracking-widest mb-1">Inversión Total</p>
                            <p className="text-xl font-black text-white">${inversionTotal.toFixed(2)}</p>
                        </div>
                        <div className="text-right flex-1 min-w-0">
                            <p className="text-[9px] uppercase text-neutral-500 font-bold tracking-widest mb-1">Titular</p>
                            <div className="flex items-center justify-end gap-1 text-neutral-300">
                                <User className="w-3 h-3 flex-shrink-0" />
                                <span className="text-xs font-semibold truncate">{nombreUsuario}</span>
                            </div>
                        </div>
                    </div>

                    {/* Botón de Cancelación */}
                    {isPagado && !isCompletado && !isCancelling && (
                        <div className="mt-4 pt-4 border-t border-neutral-800/30">
                            <button
                                onClick={async () => {
                                    if (window.confirm('¿Estás seguro de que deseas cancelar esta entrada? Se procesará un reembolso simulado.')) {
                                        setIsCancelling(true);
                                        try {
                                            await entradasService.cancelarEntrada(id, usuarioId);
                                            toast.success('Entrada cancelada exitosamente');
                                            if (onCancel) onCancel(id);
                                        } catch (error: any) {
                                            toast.error(error.response?.data?.message || 'Error al cancelar entrada');
                                        } finally {
                                            setIsCancelling(false);
                                        }
                                    }
                                }}
                                className="text-[10px] font-bold text-red-500 hover:text-red-400 transition-colors flex items-center gap-1 uppercase tracking-wider"
                            >
                                <XCircle className="w-3 h-3" />
                                Cancelar Entrada
                            </button>
                        </div>
                    )}
                    {isCancelling && (
                        <div className="mt-4 pt-4 border-t border-neutral-800/30">
                            <p className="text-[10px] font-bold text-neutral-500 animate-pulse uppercase tracking-wider">
                                Procesando cancelación...
                            </p>
                        </div>
                    )}
                </div>
            </div>

            {/* Derecha: QR */}
            <div className="w-full md:w-44 bg-neutral-800/30 p-4 flex flex-col items-center justify-center gap-2 flex-shrink-0 border-b md:border-b-0 md:border-r border-dashed border-neutral-700">
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
                <p className="text-[10px] font-mono text-neutral-500 text-center opacity-50">
                    {codigo ? codigo.slice(-12).toUpperCase() : 'SIN CÓDIGO'}
                </p>
            </div>

            {/* Acceso Streaming para Eventos Virtuales */}
            {isPagado && esVirtual && !isCompletado && (
                <div className="w-full md:w-64 flex flex-col items-center justify-center p-6 bg-blue-600/5 flex-shrink-0">
                    <Link
                        to={`/streaming/${eventoId}`}
                        className="w-full flex items-center justify-center gap-3 px-4 py-4 bg-blue-600 hover:bg-blue-700 text-white rounded-2xl text-[11px] font-black transition-all shadow-xl shadow-blue-600/20 active:scale-95 group uppercase tracking-widest"
                    >
                        <Video className="w-4 h-4" />
                        ENTRAR
                        <ExternalLink className="w-3 h-3 opacity-50 group-hover:opacity-100 transition-opacity" />
                    </Link>
                    <div className="mt-4 flex items-center gap-2">
                        <div className="w-1.5 h-1.5 bg-emerald-500 rounded-full animate-pulse" />
                        <p className="text-[9px] text-emerald-400 font-bold uppercase tracking-[0.2em]">Disponible Ahora</p>
                    </div>
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
