import { createBrowserRouter, Navigate } from 'react-router-dom';

// Placeholders para componentes (Pueden ser movidos a sus respectivas carpetas de features después)
const Placeholder = ({ title }: { title: string }) => (
    <div className="p-8">
        <h1 className="text-2xl font-bold">{title}</h1>
        <p className="text-gray-600">Este módulo está en construcción.</p>
    </div>
);

export const router = createBrowserRouter([
    {
        path: '/',
        element: <Placeholder title="Home - Catálogo de Eventos" />,
    },
    {
        path: '/login',
        element: <Placeholder title="Autenticación" />,
    },
    {
        path: '/perfil',
        element: <Placeholder title="Mi Perfil / Historial" />,
    },
    {
        path: '/entradas',
        element: <Placeholder title="Mis Entradas" />,
    },
    {
        path: '/entradas/checkout',
        element: <Placeholder title="Checkout" />,
    },
    {
        path: '/asientos/:eventoId',
        element: <Placeholder title="Selector de Asientos" />,
    },
    {
        path: '/pagos',
        element: <Placeholder title="Billetera y Pagos" />,
    },
    {
        path: '/servicios',
        element: <Placeholder title="Servicios Extras" />,
    },
    {
        path: '/foros',
        element: <Placeholder title="Comunidad y Foros" />,
    },
    {
        path: '/streaming/:id',
        element: <Placeholder title="Transmisión en Vivo" />,
    },
    {
        path: '/notificaciones',
        element: <Placeholder title="Centro de Notificaciones" />,
    },
    {
        path: '/admin',
        element: <Placeholder title="Panel de Administración" />,
    },
    {
        path: '/encuestas/:id',
        element: <Placeholder title="Encuesta de Satisfacción" />,
    },
    {
        path: '*',
        element: <Navigate to="/" replace />,
    },
]);
