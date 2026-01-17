import { createBrowserRouter, Navigate } from 'react-router-dom';
import StreamingPage from './features/streaming/pages/StreamingPage';
import { ForoPage } from './features/foros/pages/ForoPage';
import { EncuestaPage } from './features/encuestas/pages/EncuestaPage';
import { EventosPage } from './features/eventos/pages/EventosPage';
import RegisterPage from './features/auth/pages/RegisterPage';
import CheckoutPage from './features/entradas/pages/CheckoutPage';
import { ServiciosPage } from './features/servicios/pages/ServiciosPage';
import { UserDashboard } from './features/usuarios/pages/UserDashboard';
import { ProfileEditPage } from './features/usuarios/pages/ProfileEditPage';
import { AuditHistoryPage } from './features/usuarios/pages/AuditHistoryPage';
import { HistorialCorreosPage } from './features/notificaciones/pages/HistorialCorreosPage';
import { MainLayout } from './components/layout/MainLayout';
import AdminLayout from './layouts/AdminLayout';
import AdminDashboard from './features/admin/pages/AdminDashboard';
import AdminEventos from './features/admin/pages/AdminEventos';
import AdminUsuarios from './features/admin/pages/AdminUsuarios';
import { AdminAuditPage } from './features/admin/pages/AdminAuditPage';
import { ConciliacionPage } from './features/pagos/pages/ConciliacionPage';
import { ReportesVentasPage } from './features/reportes/pages/ReportesVentasPage';
import { SupervisionPage } from './features/supervision/pages/SupervisionPage';
import { LogsPage } from './features/logs/pages/LogsPage';
import { ProtectedRoute } from './components/auth/ProtectedRoute';
import AdminServiciosPage from './features/admin/pages/AdminServiciosPage';

const Placeholder = ({ title }: { title: string }) => (
    <div className="flex flex-col items-center justify-center p-8 min-h-[60vh]">
        <div className="bg-neutral-900 border border-neutral-800 p-12 rounded-3xl max-w-2xl w-full text-center">
            <h1 className="text-4xl font-black mb-4 bg-gradient-to-r from-blue-400 to-purple-500 bg-clip-text text-transparent">
                {title}
            </h1>
            <div className="w-16 h-1 bg-blue-600 mx-auto mb-6 rounded-full"></div>
            <p className="text-lg text-neutral-400 mb-8 font-medium">
                Este módulo está siendo optimizado para la experiencia Kairo Dark Premium.
            </p>
            <div className="inline-block px-6 py-2 bg-neutral-800 text-neutral-500 rounded-xl font-bold text-xs uppercase tracking-widest">
                En Desarrollo
            </div>
        </div>
    </div>
);


export const router = createBrowserRouter([
    {
        path: '/',
        element: <MainLayout />,
        children: [
            { index: true, element: <EventosPage /> },
            { path: 'perfil', element: <UserDashboard /> },
            { path: 'perfil/editar', element: <ProfileEditPage /> },
            { path: 'perfil/historial', element: <AuditHistoryPage /> },
            { path: 'perfil/correos', element: <HistorialCorreosPage /> },
            { path: 'entradas', element: <UserDashboard /> },
            { path: 'checkout/:eventoId', element: <CheckoutPage /> },
            { path: 'asientos/:eventoId', element: <Placeholder title="Selector de Asientos" /> },
            { path: 'pagos', element: <Placeholder title="Billetera y Pagos" /> },
            { path: 'servicios', element: <ServiciosPage /> },
            { path: 'foros/:id', element: <ForoPage /> },
            { path: 'streaming/:id', element: <StreamingPage /> },
            { path: 'notificaciones', element: <Placeholder title="Centro de Notificaciones" /> },
            { path: 'encuestas/:id', element: <EncuestaPage /> },
        ]
    },
    {
        path: '/admin',
        element: (
            <ProtectedRoute allowedRoles={['admin', 'organizador', 'organizator']}>
                <AdminLayout />
            </ProtectedRoute>
        ),
        children: [
            { index: true, element: <AdminDashboard /> },
            { path: 'eventos', element: <AdminEventos /> },
            { path: 'ventas', element: <ReportesVentasPage /> },
            { path: 'auditoria', element: <AdminAuditPage /> },
            { path: 'finanzas', element: <ConciliacionPage /> },
            { path: 'supervision', element: <SupervisionPage /> },
            { path: 'logs', element: <LogsPage /> },
            {
                path: 'usuarios', element: (
                    <ProtectedRoute allowedRoles={['admin']}>
                        <AdminUsuarios />
                    </ProtectedRoute>
                )
            },
            {
                path: 'servicios', element: (
                    <ProtectedRoute allowedRoles={['admin']}>
                        <AdminServiciosPage />
                    </ProtectedRoute>
                )
            },
        ]
    },
    { path: '/login', element: <Navigate to="/perfil" replace /> },
    { path: '/register', element: <RegisterPage /> },
    { path: '*', element: <Navigate to="/" replace /> },
]);
