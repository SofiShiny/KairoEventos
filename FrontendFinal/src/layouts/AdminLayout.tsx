import { useState } from 'react';
import { useLocation, Link, Outlet } from 'react-router-dom';
import { useAuth } from 'react-oidc-context';
import {
    LayoutDashboard,
    Calendar,
    BarChart3,
    Users,
    LogOut,
    ShoppingBag,
    Menu,
    Bell,
    Search,
    User
} from 'lucide-react';
import { clsx, type ClassValue } from 'clsx';
import { twMerge } from 'tailwind-merge';

function cn(...inputs: ClassValue[]) {
    return twMerge(clsx(inputs));
}

export default function AdminLayout() {
    const location = useLocation();
    const auth = useAuth();
    const [isSidebarOpen, setIsSidebarOpen] = useState(true);

    const menuItems = [
        { icon: LayoutDashboard, label: 'Dashboard', path: '/admin' },
        { icon: Calendar, label: 'Eventos', path: '/admin/eventos' },
        { icon: BarChart3, label: 'Ventas', path: '/admin/ventas' },
        { icon: Users, label: 'Usuarios', path: '/admin/usuarios' },
    ];

    const handleLogout = () => {
        auth.signoutRedirect();
    };

    return (
        <div className="min-h-screen bg-[#0f1115] text-slate-300 font-sans selection:bg-blue-500/30">
            {/* Sidebar */}
            <aside className={cn(
                "fixed left-0 top-0 h-full bg-[#16191f] border-r border-slate-800 transition-all duration-300 z-50",
                isSidebarOpen ? "w-64" : "w-20"
            )}>
                <div className="flex flex-col h-full">
                    {/* Logo Section */}
                    <div className="p-6 flex items-center gap-3">
                        <div className="w-8 h-8 bg-blue-600 rounded-lg flex items-center justify-center flex-shrink-0">
                            <span className="text-white font-black italic">K</span>
                        </div>
                        {isSidebarOpen && (
                            <span className="text-xl font-bold bg-gradient-to-r from-white to-slate-400 bg-clip-text text-transparent">
                                Kairo<span className="text-blue-500 underline decoration-blue-500/50 decoration-2 underline-offset-4">Admin</span>
                            </span>
                        )}
                    </div>

                    {/* Navigation */}
                    <nav className="flex-1 px-3 space-y-1">
                        {menuItems.map((item) => {
                            const isActive = location.pathname === item.path;
                            return (
                                <Link
                                    key={item.path}
                                    to={item.path}
                                    className={cn(
                                        "flex items-center gap-3 px-3 py-3 rounded-xl transition-all group relative",
                                        isActive
                                            ? "bg-blue-600/10 text-blue-400"
                                            : "hover:bg-slate-800/50 text-slate-400 hover:text-slate-200"
                                    )}
                                >
                                    <item.icon className={cn("w-5 h-5", isActive ? "text-blue-500" : "group-hover:text-slate-200")} />
                                    {isSidebarOpen && <span className="font-medium">{item.label}</span>}
                                    {isActive && <div className="absolute left-0 top-1/2 -translate-y-1/2 w-1 h-6 bg-blue-500 rounded-r-full shadow-[0_0_10px_rgba(59,130,246,0.5)]" />}
                                </Link>
                            );
                        })}
                    </nav>

                    {/* User Section Bottom */}
                    <div className="p-4 border-t border-slate-800 bg-[#1a1e26]/50">
                        <button
                            onClick={handleLogout}
                            className="flex items-center gap-3 w-full px-3 py-3 rounded-xl text-slate-400 hover:text-red-400 hover:bg-red-400/10 transition-all"
                        >
                            <LogOut className="w-5 h-5" />
                            {isSidebarOpen && <span className="font-medium">Cerrar Sesión</span>}
                        </button>
                    </div>
                </div>
            </aside>

            {/* Main Content */}
            <main className={cn(
                "transition-all duration-300 min-h-screen flex flex-col",
                isSidebarOpen ? "ml-64" : "ml-20"
            )}>
                {/* Navbar superior */}
                <header className="h-16 bg-[#16191f]/80 backdrop-blur-md border-b border-slate-800 flex items-center justify-between px-8 sticky top-0 z-40">
                    <div className="flex items-center gap-4">
                        <button
                            onClick={() => setIsSidebarOpen(!isSidebarOpen)}
                            className="p-2 hover:bg-slate-800 rounded-lg text-slate-400 transition-colors"
                        >
                            <Menu className="w-5 h-5" />
                        </button>
                        <div className="hidden md:flex items-center gap-2 bg-slate-900 border border-slate-800 px-3 py-1.5 rounded-lg text-sm group focus-within:border-blue-500/50 transition-all">
                            <Search className="w-4 h-4 text-slate-500 group-focus-within:text-blue-400" />
                            <input
                                type="text"
                                placeholder="Buscar eventos o ventas..."
                                className="bg-transparent border-none outline-none text-slate-300 w-64"
                            />
                        </div>
                    </div>

                    <div className="flex items-center gap-6">
                        <Link
                            to="/"
                            className="flex items-center gap-2 text-sm font-semibold text-slate-400 hover:text-white transition-colors group"
                        >
                            <ShoppingBag className="w-4 h-4 group-hover:scale-110 transition-transform" />
                            <span className="hidden sm:inline">Volver a la Tienda</span>
                        </Link>

                        <div className="h-6 w-[1px] bg-slate-800" />

                        <div className="flex items-center gap-3">
                            <button className="relative p-2 text-slate-400 hover:text-white hover:bg-slate-800 rounded-full transition-all">
                                <Bell className="w-5 h-5" />
                                <span className="absolute top-2 right-2 w-2 h-2 bg-blue-500 rounded-full border-2 border-[#16191f]" />
                            </button>
                            <div className="flex items-center gap-3 pl-2">
                                <div className="text-right hidden sm:block">
                                    <p className="text-sm font-bold text-white">{auth.user?.profile.preferred_username || 'Admin User'}</p>
                                    <p className="text-[10px] text-blue-500 uppercase font-black tracking-widest">Administrator</p>
                                </div>
                                <div className="w-10 h-10 bg-gradient-to-br from-blue-600 to-indigo-600 rounded-xl flex items-center justify-center border border-white/10 shadow-lg">
                                    <User className="text-white w-5 h-5" />
                                </div>
                            </div>
                        </div>
                    </div>
                </header>

                {/* Content Area */}
                <div className="p-8 flex-1 overflow-x-hidden">
                    <Outlet />
                </div>

                {/* Footer Admin */}
                <footer className="p-8 border-t border-slate-800 bg-[#16191f]/30 flex flex-col md:flex-row justify-between items-center gap-4">
                    <p className="text-sm text-slate-500">© 2026 Kairo Events System. Todos los derechos reservados.</p>
                    <div className="flex gap-6 text-sm text-slate-600">
                        <Link to="#" className="hover:text-slate-400 transition-colors">Documentación</Link>
                        <Link to="#" className="hover:text-slate-400 transition-colors">Soporte Técnico</Link>
                        <Link to="#" className="hover:text-slate-400 transition-colors">Estado del Sistema</Link>
                    </div>
                </footer>
            </main>
        </div>
    );
}
