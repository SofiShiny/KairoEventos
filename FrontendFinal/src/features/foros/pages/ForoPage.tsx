import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useAuth } from 'react-oidc-context';
import {
    MessageCircle,
    Send,
    Reply,
    ChevronLeft,
    Sparkles,
    Loader2,
    User,
    Trash2
} from 'lucide-react';
import { forosService, Comentario } from '../services/foros.service';
import { toast } from 'react-hot-toast';
import { getUserRoles } from '../../../lib/auth-utils';

export const ForoPage = () => {
    const { id: eventoId } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const auth = useAuth();

    const [comentarios, setComentarios] = useState<Comentario[]>([]);
    const [loading, setLoading] = useState(true);
    const [nuevoComentario, setNuevoComentario] = useState('');
    const [respuestaActiva, setRespuestaActiva] = useState<string | null>(null);
    const [respuestaTexto, setRespuestaTexto] = useState('');
    const [submitting, setSubmitting] = useState(false);

    // Roles de usuario
    const roles = getUserRoles(auth.user);
    const canModerate = roles.includes('admin') || roles.includes('organizador');


    const usuarioId = auth.user?.profile.sub;
    const username = (auth.user?.profile as any)?.preferred_username || 'Usuario';

    useEffect(() => {
        if (eventoId) {
            loadComentarios();
        }
    }, [eventoId]);

    const loadComentarios = async () => {
        try {
            setLoading(true);
            const data = await forosService.getComentariosPorEvento(eventoId!);
            setComentarios(data);
        } catch (error) {
            console.error('Error cargando comentarios:', error);
            toast.error('Error al cargar los comentarios del foro.');
        } finally {
            setLoading(false);
        }
    };

    const handleCrearComentario = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!auth.isAuthenticated) {
            auth.signinRedirect();
            return;
        }

        if (!nuevoComentario.trim()) {
            toast.error('Escribe algo antes de publicar.');
            return;
        }

        try {
            setSubmitting(true);
            await forosService.crearComentario({
                foroId: eventoId!,
                usuarioId: usuarioId!,
                contenido: nuevoComentario
            });

            setNuevoComentario('');
            toast.success('¡Comentario publicado!');
            await loadComentarios();
        } catch (error) {
            toast.error('Error al publicar el comentario.');
        } finally {
            setSubmitting(false);
        }
    };

    const handleResponder = async (comentarioId: string) => {
        if (!auth.isAuthenticated) {
            auth.signinRedirect();
            return;
        }

        if (!respuestaTexto.trim()) {
            toast.error('Escribe una respuesta.');
            return;
        }

        try {
            setSubmitting(true);
            await forosService.responderComentario(comentarioId, {
                usuarioId: usuarioId!,
                contenido: respuestaTexto
            });

            setRespuestaTexto('');
            setRespuestaActiva(null);
            toast.success('¡Respuesta publicada!');
            await loadComentarios();
        } catch (error) {
            toast.error('Error al publicar la respuesta.');
        } finally {
            setSubmitting(false);
        }
    };

    const handleConfirmarOcultarComentario = async (comentarioId: string) => {
        if (!window.confirm('¿Estás seguro de que deseas ocultar este comentario? Se ocultarán todas sus respuestas.')) {
            return;
        }

        try {
            await forosService.ocultarComentario(comentarioId);
            toast.success('Comentario ocultado correctamente');
            await loadComentarios();
        } catch (error) {
            console.error('[MODERACION] Error:', error);
            toast.error('Error al ocultar el comentario.');
        }
    };

    const handleConfirmarOcultarRespuesta = async (comentarioId: string, respuestaId: string) => {
        if (!window.confirm('¿Estás seguro de que deseas ocultar esta respuesta?')) {
            return;
        }

        try {
            await forosService.ocultarRespuesta(comentarioId, respuestaId);
            toast.success('Respuesta ocultada correctamente');
            await loadComentarios();
        } catch (error) {
            console.error('[MODERACION] Error:', error);
            toast.error('Error al ocultar la respuesta.');
        }
    };

    const formatFecha = (fecha: string) => {
        const date = new Date(fecha);
        const ahora = new Date();
        const diff = ahora.getTime() - date.getTime();
        const minutos = Math.floor(diff / 60000);
        const horas = Math.floor(minutos / 60);
        const dias = Math.floor(horas / 24);

        if (minutos < 1) return 'Ahora';
        if (minutos < 60) return `Hace ${minutos}m`;
        if (horas < 24) return `Hace ${horas}h`;
        if (dias < 7) return `Hace ${dias}d`;
        return date.toLocaleDateString('es-ES', { day: '2-digit', month: 'short' });
    };

    const renderComentario = (item: any, index?: number, esRespuesta = false, parentComentarioId?: string) => {
        const itemKey = item.id || `res-${item.usuarioId}-${index}-${item.fechaCreacion}`;

        return (
            <div
                key={itemKey}
                className="transition-all duration-300"
                style={{
                    marginLeft: esRespuesta ? '40px' : '0px',
                    marginBottom: '16px'
                }}
            >
                <div style={{
                    backgroundColor: esRespuesta ? '#121212' : '#1a1a1a',
                    border: '1px solid #333',
                    borderRadius: '24px',
                    padding: '20px',
                    position: 'relative'
                }}>
                    <div style={{ display: 'flex', gap: '12px', alignItems: 'flex-start' }}>
                        <div style={{
                            width: esRespuesta ? '32px' : '40px',
                            height: esRespuesta ? '32px' : '40px',
                            backgroundColor: esRespuesta ? '#333' : '#2563eb',
                            borderRadius: '12px',
                            display: 'flex',
                            alignItems: 'center',
                            justifyContent: 'center',
                            flexShrink: 0
                        }}>
                            <User className={esRespuesta ? 'w-4 h-4 text-white/50' : 'w-5 h-5 text-white'} />
                        </div>

                        <div className="flex-1 min-w-0">
                            <div className="flex items-center gap-2 mb-1">
                                <span style={{
                                    fontWeight: 900,
                                    textTransform: 'uppercase',
                                    fontSize: '11px',
                                    color: esRespuesta ? '#777' : '#3b82f6'
                                }}>
                                    {esRespuesta ? 'Respuesta' : 'Usuario'}
                                </span>
                                <span className="text-neutral-700">•</span>
                                <span style={{ fontSize: '10px', color: '#555', fontWeight: 'bold' }}>
                                    {formatFecha(item.fechaCreacion)}
                                </span>
                            </div>

                            <p style={{
                                margin: 0,
                                color: esRespuesta ? '#aaa' : '#eee',
                                fontSize: esRespuesta ? '13px' : '15px',
                                lineHeight: '1.5',
                                marginBottom: '12px'
                            }}>
                                {item.contenido}
                            </p>

                            <div className="flex items-center gap-3">
                                {!esRespuesta && (
                                    <button
                                        onClick={() => setRespuestaActiva(item.id === respuestaActiva ? null : item.id)}
                                        style={{
                                            padding: '6px 16px',
                                            backgroundColor: '#2563eb',
                                            color: 'white',
                                            border: 'none',
                                            borderRadius: '99px',
                                            fontSize: '10px',
                                            fontWeight: 900,
                                            textTransform: 'uppercase',
                                            letterSpacing: '0.05em',
                                            cursor: 'pointer',
                                            display: 'flex',
                                            alignItems: 'center',
                                            gap: '6px'
                                        }}
                                    >
                                        <Reply size={12} />
                                        Responder
                                    </button>
                                )}

                                {canModerate && (
                                    <button
                                        onClick={() => {
                                            // Detección ultra-robusta de ID (case-insensitive + fallback determinista)
                                            const itemId = item.id || item.Id || item.ID || item._id ||
                                                (esRespuesta ? `00000000-0000-0000-0000-${String(index).padStart(12, '0')}` : null);

                                            if (!itemId) {
                                                toast.error('Error: No se pudo identificar el ID.');
                                                return;
                                            }

                                            if (esRespuesta) {
                                                handleConfirmarOcultarRespuesta(parentComentarioId!, itemId);
                                            } else {
                                                handleConfirmarOcultarComentario(itemId);
                                            }
                                        }}
                                        className="hover:scale-105 active:scale-95 transition-transform"
                                        style={{
                                            padding: '6px 16px',
                                            backgroundColor: 'rgba(239, 68, 68, 0.1)',
                                            color: '#f87171',
                                            border: '1px solid rgba(239, 68, 68, 0.2)',
                                            borderRadius: '99px',
                                            fontSize: '10px',
                                            fontWeight: 900,
                                            textTransform: 'uppercase',
                                            letterSpacing: '0.05em',
                                            cursor: 'pointer',
                                            display: 'flex',
                                            alignItems: 'center',
                                            gap: '6px'
                                        }}
                                    >
                                        <Trash2 size={12} />
                                        Eliminar
                                    </button>
                                )}
                            </div>
                        </div>
                    </div>

                    {/* Formulario de respuesta */}
                    {!esRespuesta && respuestaActiva === item.id && (
                        <div style={{
                            marginTop: '16px',
                            padding: '12px',
                            backgroundColor: '#000',
                            borderRadius: '16px',
                            border: '1px solid #333'
                        }}>
                            <div style={{ display: 'flex', gap: '8px' }}>
                                <input
                                    type="text"
                                    value={respuestaTexto}
                                    onChange={(e) => setRespuestaTexto(e.target.value)}
                                    placeholder="Escribe tu respuesta..."
                                    style={{
                                        flex: 1,
                                        backgroundColor: 'transparent',
                                        border: 'none',
                                        color: 'white',
                                        outline: 'none',
                                        fontSize: '13px'
                                    }}
                                    onKeyPress={(e) => e.key === 'Enter' && handleResponder(item.id)}
                                    autoFocus
                                />
                                <button
                                    onClick={() => handleResponder(item.id)}
                                    disabled={submitting || !respuestaTexto.trim()}
                                    style={{
                                        backgroundColor: '#2563eb',
                                        color: 'white',
                                        border: 'none',
                                        padding: '6px 12px',
                                        borderRadius: '8px',
                                        cursor: 'pointer'
                                    }}
                                >
                                    {submitting ? <Loader2 size={14} className="animate-spin" /> : <Send size={14} />}
                                </button>
                            </div>
                        </div>
                    )}

                    {/* Cascada de respuestas */}
                    {!esRespuesta && item.respuestas && item.respuestas.length > 0 && (
                        <div style={{ marginTop: '12px' }}>
                            {item.respuestas.map((resp: any, idx: number) => renderComentario(resp, idx, true, item.id))}
                        </div>
                    )}
                </div>
            </div>
        );
    };

    if (loading) {
        return (
            <div className="min-h-screen bg-black flex flex-col items-center justify-center">
                <Loader2 className="w-12 h-12 text-blue-500 animate-spin mb-4" />
                <p className="text-neutral-500 font-bold uppercase tracking-widest text-xs">Cargando Foro...</p>
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-black text-white p-6 md:p-8">
            <div className="max-w-3xl mx-auto">
                <button
                    onClick={() => navigate(-1)}
                    className="flex items-center gap-2 text-neutral-500 hover:text-white transition-colors mb-10 group"
                >
                    <ChevronLeft className="w-5 h-5 group-hover:-translate-x-1 transition-transform" />
                    <span className="font-bold uppercase tracking-widest text-[10px]">Volver</span>
                </button>

                <header className="mb-12 relative">
                    <div className="absolute -top-20 -left-20 w-64 h-64 bg-purple-600/10 blur-[120px] rounded-full pointer-events-none" />
                    <div className="flex items-center gap-2 mb-3">
                        <Sparkles className="w-4 h-4 text-purple-500" />
                        <span className="text-purple-500 font-black text-[10px] uppercase tracking-[0.2em]">Comunidad</span>
                    </div>
                    <h1 className="text-4xl md:text-5xl font-black mb-4 tracking-tighter uppercase leading-none">
                        FORO DEL EVENTO
                    </h1>
                    <p className="text-neutral-500 text-base md:text-lg font-medium max-w-xl">
                        Conecta con otros asistentes y comparte tus pensamientos sobre este evento.
                    </p>
                </header>

                {/* Formulario nuevo comentario */}
                <form onSubmit={handleCrearComentario} className="mb-12">
                    <div style={{ backgroundColor: '#111', border: '1px solid #222', borderRadius: '32px', padding: '24px' }}>
                        <div className="flex items-start gap-4">
                            <div className="w-10 h-10 rounded-full bg-gradient-to-br from-blue-500 to-purple-600 flex items-center justify-center flex-shrink-0">
                                <span className="text-white font-black text-xs uppercase">{username.substring(0, 2)}</span>
                            </div>

                            <div className="flex-1">
                                <textarea
                                    value={nuevoComentario}
                                    onChange={(e) => setNuevoComentario(e.target.value)}
                                    placeholder="¿Qué tienes en mente?"
                                    style={{
                                        width: '100%',
                                        backgroundColor: '#1a1a1a',
                                        border: 'none',
                                        borderRadius: '16px',
                                        padding: '16px',
                                        color: 'white',
                                        minHeight: '100px',
                                        resize: 'none',
                                        outline: 'none'
                                    }}
                                />

                                <div className="flex justify-end mt-4">
                                    <button
                                        type="submit"
                                        disabled={submitting || !nuevoComentario.trim()}
                                        style={{
                                            padding: '12px 24px',
                                            backgroundColor: '#2563eb',
                                            color: 'white',
                                            border: 'none',
                                            borderRadius: '16px',
                                            fontWeight: 900,
                                            fontSize: '11px',
                                            textTransform: 'uppercase',
                                            letterSpacing: '0.1em',
                                            cursor: 'pointer',
                                            display: 'flex',
                                            alignItems: 'center',
                                            gap: '8px'
                                        }}
                                    >
                                        {submitting ? (
                                            <Loader2 size={16} className="animate-spin" />
                                        ) : (
                                            <>
                                                <MessageCircle size={16} />
                                                PUBLICAR
                                            </>
                                        )}
                                    </button>
                                </div>
                            </div>
                        </div>
                    </div>
                </form>

                {/* Lista de comentarios */}
                <div className="space-y-6">
                    <h2 className="text-xl font-black mb-6 flex items-center gap-2 uppercase tracking-tight">
                        <MessageCircle className="text-purple-500 w-5 h-5" />
                        Conversaciones ({comentarios.length})
                    </h2>

                    {comentarios.length === 0 ? (
                        <div className="text-center py-16 bg-neutral-900/10 rounded-[2rem] border border-neutral-900">
                            <MessageCircle className="w-8 h-8 text-neutral-800 mx-auto mb-3" />
                            <p className="text-neutral-600 font-bold uppercase tracking-widest text-[10px]">
                                No hay comentarios aún. ¡Sé el primero!
                            </p>
                        </div>
                    ) : (
                        <div>
                            {comentarios.map(comentario => renderComentario(comentario))}
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
};
