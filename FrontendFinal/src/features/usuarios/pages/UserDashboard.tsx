import { useEffect, useState } from 'react';
import { useAuth } from 'react-oidc-context';
import { useNavigate } from 'react-router-dom';
import { Ticket, Search, User as UserIcon, Settings, LogOut, ChevronRight, Activity } from 'lucide-react';
import { entradasService, Entrada } from '../../entradas/services/entradas.service';
import { DigitalTicket } from '../../entradas/components/DigitalTicket';
import { eventosService } from '../../eventos/services/eventos.service';
import { serviciosService } from '../../servicios/services/servicios.service';

export const UserDashboard = () => {
    const auth = useAuth();
    const navigate = useNavigate();
    const [entradas, setEntradas] = useState<Entrada[]>([]);
    const [eventStatusMap, setEventStatusMap] = useState<Record<string, string>>({});
    const [reservasServicios, setReservasServicios] = useState<any[]>([]);
    const [catalogoServicios, setCatalogoServicios] = useState<any[]>([]);
    const [loading, setLoading] = useState(true);

    const user = auth.user?.profile;
    const usuarioId = user?.sub;

    useEffect(() => {
        if (usuarioId) {
            loadEntradas();
        }
    }, [usuarioId]);

    const loadEntradas = async () => {
        try {
            setLoading(true);
            const [entradasData, eventosData, reservasData, catalogoData] = await Promise.all([
                entradasService.getMisEntradas(usuarioId!),
                eventosService.getEventos(),
                serviciosService.getMisReservas(usuarioId!),
                serviciosService.getServiciosPorEvento('global') // Fetch global catalog
            ]);

            setEntradas(entradasData);
            setReservasServicios(reservasData);
            setCatalogoServicios(catalogoData);

            // Crear mapa de estados de eventos
            const statusMap: Record<string, string> = {};
            eventosData.forEach(e => {
                statusMap[e.id] = e.estado;
            });
            setEventStatusMap(statusMap);
        } catch (error) {
            console.error('Error al cargar dashboard:', error);
        } finally {
            setLoading(false);
        }
    };

    if (!auth.isAuthenticated) {
        return (
            <div className="min-h-screen bg-black flex items-center justify-center p-4">
                <div className="text-center max-w-sm">
                    <div className="w-20 h-20 bg-neutral-900 rounded-full flex items-center justify-center mx-auto mb-6 border border-neutral-800">
                        <UserIcon className="w-10 h-10 text-neutral-600" />
                    </div>
                    <h1 className="text-2xl font-bold text-white mb-2">Acceso Requerido</h1>
                    <p className="text-neutral-400 mb-8">Debes iniciar sesión para ver tu perfil y entradas.</p>
                    <button
                        onClick={() => auth.signinRedirect()}
                        className="w-full py-3 bg-blue-600 text-white font-bold rounded-xl hover:bg-blue-700 transition"
                    >
                        Iniciar Sesión
                    </button>
                </div>
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-black text-white">
            <div className="max-w-6xl mx-auto px-4 py-12">
                {/* Header Perfil */}
                <div className="flex flex-col md:flex-row items-center gap-8 mb-12 bg-neutral-900/50 p-8 rounded-3xl border border-neutral-800">
                    <div className="relative group">
                        <div className="w-24 h-24 rounded-full bg-gradient-to-tr from-purple-600 to-blue-600 p-1">
                            <div className="w-full h-full rounded-full bg-black flex items-center justify-center overflow-hidden">
                                <span className="text-3xl font-bold text-white uppercase">
                                    {(user as any)?.preferred_username?.substring(0, 2) || 'U'}
                                </span>
                            </div>
                        </div>
                        <div className="absolute bottom-0 right-0 w-8 h-8 bg-neutral-800 rounded-full border-2 border-black flex items-center justify-center">
                            <Settings className="w-4 h-4 text-neutral-400" />
                        </div>
                    </div>

                    <div className="text-center md:text-left flex-1">
                        <h1 className="text-3xl font-black mb-1">{(user as any)?.preferred_username || 'Usuario'}</h1>
                        <p className="text-neutral-500 font-medium">{user?.email || 'email@ejemplo.com'}</p>
                        <div className="flex flex-wrap justify-center md:justify-start gap-4 mt-4">
                            <span className="px-3 py-1 bg-neutral-800 rounded-full text-xs font-bold text-neutral-400 border border-neutral-700">
                                Miembro desde 2024
                            </span>
                            <span className="px-3 py-1 bg-blue-500/10 rounded-full text-xs font-bold text-blue-400 border border-blue-500/20">
                                {entradas.filter(e => e.estado === 'Pagada' || e.estado === 'Usada').length} Entradas Compradas
                            </span>
                        </div>
                    </div>

                    <button
                        onClick={() => auth.signoutRedirect()}
                        className="p-4 bg-neutral-800 hover:bg-red-900/20 hover:text-red-500 rounded-2xl transition group"
                        title="Cerrar Sesión"
                    >
                        <LogOut className="w-6 h-6" />
                    </button>
                </div>

                {/* Navegación Rápida */}
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-16">
                    <button
                        onClick={() => navigate('/entradas')}
                        className="group bg-gradient-to-br from-purple-600 to-purple-800 p-1 rounded-3xl transition-transform hover:scale-[1.02]"
                    >
                        <div className="bg-neutral-900 h-full w-full rounded-[20px] p-6 flex flex-col justify-between">
                            <div className="flex justify-between items-start">
                                <div className="p-3 bg-purple-500/10 rounded-2xl">
                                    <Ticket className="w-8 h-8 text-purple-400" />
                                </div>
                                <ChevronRight className="w-6 h-6 text-neutral-600 group-hover:text-purple-400 transform group-hover:translate-x-1 transition-all" />
                            </div>
                            <div className="text-left mt-8">
                                <h2 className="text-xl font-bold text-white">Mis Entradas</h2>
                                <p className="text-sm text-neutral-500">Ver tus códigos QR y detalles</p>
                            </div>
                        </div>
                    </button>

                    <button
                        onClick={() => navigate('/')}
                        className="group bg-neutral-900 border border-neutral-800 p-6 rounded-3xl transition-all hover:border-blue-500/50 hover:bg-neutral-800/50"
                    >
                        <div className="flex justify-between items-start">
                            <div className="p-3 bg-blue-500/10 rounded-2xl">
                                <Search className="w-8 h-8 text-blue-400" />
                            </div>
                            <ChevronRight className="w-6 h-6 text-neutral-600 group-hover:text-blue-400 transform group-hover:translate-x-1 transition-all" />
                        </div>
                        <div className="text-left mt-8">
                            <h2 className="text-xl font-bold text-white">Explorar Eventos</h2>
                            <p className="text-sm text-neutral-500">Busca nuevas experiencias</p>
                        </div>
                    </button>

                    <button
                        onClick={() => navigate('/perfil/editar')}
                        className="group bg-neutral-900 border border-neutral-800 p-6 rounded-3xl transition-all hover:border-neutral-700 hover:bg-neutral-800/50"
                    >
                        <div className="flex justify-between items-start">
                            <div className="p-3 bg-neutral-800 rounded-2xl group-hover:bg-neutral-700 transition-colors">
                                <Settings className="w-8 h-8 text-neutral-400 group-hover:text-white transition-colors" />
                            </div>
                            <ChevronRight className="w-6 h-6 text-neutral-600 group-hover:text-neutral-400 transform group-hover:translate-x-1 transition-all" />
                        </div>
                        <div className="text-left mt-8">
                            <h2 className="text-xl font-bold text-white">Configuración</h2>
                            <p className="text-sm text-neutral-500">Edita tu perfil y preferencias</p>
                        </div>
                    </button>

                    <button
                        onClick={() => navigate('/perfil/historial')}
                        className="group bg-neutral-900 border border-neutral-800 p-6 rounded-3xl transition-all hover:border-orange-500/50 hover:bg-neutral-800/50"
                    >
                        <div className="flex justify-between items-start">
                            <div className="p-3 bg-orange-500/10 rounded-2xl">
                                <Activity className="w-8 h-8 text-orange-400" />
                            </div>
                            <ChevronRight className="w-6 h-6 text-neutral-600 group-hover:text-orange-400 transform group-hover:translate-x-1 transition-all" />
                        </div>
                        <div className="text-left mt-8">
                            <h2 className="text-xl font-bold text-white">Historial</h2>
                            <p className="text-sm text-neutral-500">Auditoría de tus acciones</p>
                        </div>
                    </button>
                </div>

                {/* Sección Mis Entradas */}
                <div>
                    <div className="flex items-center justify-between mb-8">
                        <h2 className="text-2xl font-black flex items-center gap-3">
                            <Ticket className="text-purple-500" />
                            Próximos Eventos
                        </h2>
                        <span className="text-sm font-bold text-neutral-400 px-3 py-1 bg-neutral-900 rounded-lg">
                            {entradas.filter(e => e.estado !== 'Cancelada').length} Total
                        </span>
                    </div>

                    {loading ? (
                        <div className="grid gap-6">
                            {[1, 2].map(i => (
                                <div key={i} className="h-48 w-full bg-neutral-900 animate-pulse rounded-3xl border border-neutral-800" />
                            ))}
                        </div>
                    ) : entradas.filter(e => e.estado !== 'Cancelada').length === 0 ? (
                        <div className="text-center py-20 bg-neutral-900/30 rounded-3xl border border-dashed border-neutral-800">
                            <Ticket className="w-16 h-16 text-neutral-700 mx-auto mb-4" />
                            <h3 className="text-xl font-bold text-white mb-2">No tienes entradas aún</h3>
                            <p className="text-neutral-500 mb-8 max-w-xs mx-auto">
                                Tus tickets comprados aparecerán aquí automáticamente.
                            </p>
                            <button
                                onClick={() => navigate('/')}
                                className="px-8 py-3 bg-white text-black font-black rounded-xl hover:bg-neutral-200 transition shadow-lg"
                            >
                                IR AL CATÁLOGO
                            </button>
                        </div>
                    ) : (
                        <div className="grid gap-8">
                            {entradas
                                .filter(e => e.estado !== 'Cancelada')
                                .map((entrada) => {
                                    const serviciosDeEstaEntrada = reservasServicios
                                        .filter(r => r.eventoId === entrada.eventoId)
                                        .map(r => {
                                            const info = catalogoServicios.find(s => s.id === r.servicioGlobalId);
                                            return {
                                                nombre: info ? info.nombre : 'Servicio Extra',
                                                estado: r.estado === 1 ? 'Confirmado' : 'Pendiente',
                                                precio: info ? info.precio : 0
                                            };
                                        });

                                    return (
                                        <DigitalTicket
                                            key={entrada.id}
                                            id={entrada.id}
                                            usuarioId={usuarioId!}
                                            onCancel={loadEntradas}
                                            titulo={entrada.eventoNombre}
                                            fecha={entrada.fechaEvento || entrada.fechaCompra}
                                            asiento={entrada.asientoInfo}
                                            estado={entrada.estado}
                                            imagenUrl={entrada.imagenEventoUrl}
                                            monto={entrada.precio}
                                            codigo={entrada.codigoQr}
                                            nombreUsuario={(user as any)?.preferred_username || 'Usuario'}
                                            esVirtual={entrada.esVirtual}
                                            eventoId={entrada.eventoId}
                                            eventoEstado={eventStatusMap[entrada.eventoId]}
                                            serviciosExtras={serviciosDeEstaEntrada}
                                        />
                                    );
                                })}
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
};
