import { createBrowserRouter, Navigate } from 'react-router-dom';
import { EventosPage } from './features/eventos/pages/EventosPage';
import RegisterPage from './features/auth/pages/RegisterPage';
import CheckoutPage from './features/entradas/pages/CheckoutPage';
import { UserDashboard } from './features/usuarios/pages/UserDashboard';
import { MainLayout } from './components/layout/MainLayout';
import AdminLayout from './layouts/AdminLayout';
import AdminDashboard from './features/admin/pages/AdminDashboard';
import AdminEventos from './features/admin/pages/AdminEventos';
import AdminVentas from './features/admin/pages/AdminVentas';
import AdminUsuarios from './features/admin/pages/AdminUsuarios';
import { ProtectedRoute } from './components/auth/ProtectedRoute';

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
            { path: 'entradas', element: <UserDashboard /> },
            { path: 'checkout/:eventoId', element: <CheckoutPage /> },
            { path: 'asientos/:eventoId', element: <Placeholder title="Selector de Asientos" /> },
            { path: 'pagos', element: <Placeholder title="Billetera y Pagos" /> },
            { path: 'servicios', element: <Placeholder title="Servicios Extras" /> },
            { path: 'foros', element: <Placeholder title="Comunidad y Foros" /> },
            { path: 'streaming/:id', element: <Placeholder title="Transmisión en Vivo" /> },
            { path: 'notificaciones', element: <Placeholder title="Centro de Notificaciones" /> },
            { path: 'encuestas/:id', element: <Placeholder title="Encuesta de Satisfacción" /> },
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
            { path: 'ventas', element: <AdminVentas /> },
            {
                path: 'usuarios', element: (
                    <ProtectedRoute allowedRoles={['admin']}>
                        <AdminUsuarios />
                    </ProtectedRoute>
                )
            },
        ]
    },
    { path: '/login', element: <Navigate to="/perfil" replace /> },
    { path: '/register', element: <RegisterPage /> },
    { path: '*', element: <Navigate to="/" replace /> },
]);
